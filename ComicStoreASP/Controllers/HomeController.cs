using ComicStoreASP.Data;
using ComicStoreASP.Models;
using ComicStoreASP.Services;
using ComicStoreASP.Views.Models;
using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;


namespace ComicStoreASP.Controllers
{
    public class HomeController : Controller
    {
        private readonly CSVDataReader _csvDataReader;
        private readonly ILogger<HomeController> _logger;
        private readonly SearchResultAnalyticsModel analytics;
        private readonly ComicGenreFilter genreFilter;
        private readonly ComicStore comicStore;
        private readonly AdvancedSearchFunction _advancedSearch;
        private readonly ApplicationDbContext _context;

        public HomeController(CSVDataReader csvDataReader, ILogger<HomeController> logger, SearchResultAnalyticsModel _analytics, 
            ComicGenreFilter _genreFilter, ComicStore comicStore, ApplicationDbContext context)
        {
            _csvDataReader = csvDataReader;
            _logger = logger;
            analytics = _analytics;
            this.genreFilter = _genreFilter;
            this.comicStore = comicStore;
            _advancedSearch = new AdvancedSearchFunction();
            _context = context;
        }
        [HttpGet]

        public IActionResult Index()
        {
            return View(new List<ComicGroupedViewModel>());
        }

        [Authorize]
        public IActionResult MyComics()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var comics = _context.SavedComics
                .Where(sc => sc.UserId == userId)
                .Include(sc => sc.Comic)
                .Select(sc => sc.Comic)
                .ToList();

            return View(comics);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SaveComic([FromBody] SaveComicRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var exists = await _context.SavedComics
                .AnyAsync(sc => sc.UserId == userId && sc.ComicId == request.ComicId);

            if (!exists)
            {
                _context.SavedComics.Add(new SavedComic
                {
                    UserId = userId,
                    ComicId = request.ComicId,
                    SavedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }

            return Ok();
        }


        [Authorize(Roles = "Staff")]
        public IActionResult SearchAnalytics()
        {
            var analytics = _context.SavedSearches
                .GroupBy(s => s.SearchJson)
                .Select(g => new SearchAnalyticsViewModel
                {
                    SearchTerm = g.Key,
                    Count = g.Count(),
                    LastSearched = g.Max(x => x.CreatedAt)
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            return View(analytics);
        }

        [Authorize(Roles = "Staff")]
        [HttpPost]
        public async Task<IActionResult> FlagComic([FromBody] FlagComicRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _context.ComicFlags.Add(new FlaggedComic
            {
                ComicId = request.ComicId,
                StaffUserId = userId,
                Reason = request.Reason,
                FlaggedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok();
        }


        [Authorize(Roles = "Staff")]
        public IActionResult FlaggedComics()
        {
            var flags = _context.ComicFlags
            .Include(f => f.Comic)
            .OrderByDescending(f => f.FlaggedAt)
            .ToList();

            return View(flags);
        }


        [HttpPost]
        public async Task<IActionResult> LogSearch([FromBody] string searchTerm)
        {
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                _context.SavedSearches.Add(new SavedSearch
                {
                    SearchJson = searchTerm,
                    CreatedAt = DateTime.UtcNow,
                    UserId = User.Identity.IsAuthenticated
                        ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                        : null
                });

                await _context.SaveChangesAsync();
            }

            return Ok();
        }


        [HttpGet]
        public IActionResult GetSearchAnalytics()
        {
            return Json(analytics.Top10Searches());
        }
        [HttpPost]
        public IActionResult ComicGenreFilter([FromBody] string genre)
        {
            var filtered = genreFilter.FilterByGenre(comicStore.Comics, genre);
            return Json(filtered);
        }

        [HttpPost]
        public IActionResult AdvancedSearch(AdvancedSearchVariables searchVariables)
        {
            var comics = comicStore.Comics;

            var filterComics = _advancedSearch.Search(comicStore.Comics, searchVariables);
            return View("Index", filterComics);
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromForm] IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                ModelState.AddModelError("", "Please select a valid CSV file.");
                return View(new List<ComicGroupedViewModel>());
            }

            using var stream = csvFile.OpenReadStream();
            var comics = _csvDataReader.ReadCsvFile(stream, 7000).ToList();

            foreach (var comic in comics)
            {
                comic.Title = uknownTableValue(comic.Title);
                comic.Name = uknownTableValue(comic.Name);
                comic.Role = uknownTableValue(comic.Role);
                comic.OtherNames = uknownTableValue(comic.OtherNames);
                comic.ContentType = uknownTableValue(comic.ContentType);
                comic.CountryOfPublication = uknownTableValue(comic.CountryOfPublication);
                comic.Publisher = uknownTableValue(comic.Publisher);
                comic.DateOfPublication = uknownTableValue(comic.DateOfPublication);
                comic.Edition = uknownTableValue(comic.Edition);
                comic.Topics = uknownTableValue(comic.Topics);
                comic.Genre = uknownTableValue(comic.Genre);
                comic.Languages = uknownTableValue(comic.Languages);
            }

            var grouped = comics
                .GroupBy(com => new { Title = com.Title?.Trim(), Publisher = com.Publisher?.Trim(), com.Genre })
                .Select(g => new ComicGroupedViewModel
                {
                    Title = g.Key.Title,
                    Publisher = g.Key.Publisher,
                    Genre = g.Select(x => x.Genre).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? "Unknown",
                    ContentType = g.Select(x => x.ContentType).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? "Unknown",
                    CountryOfPublication = g.Select(x => x.CountryOfPublication).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? "Unknown",

                    PublicationYears = g.Select(x => x.DateOfPublication)
                                        .Where(x => !string.IsNullOrWhiteSpace(x))
                                        .Distinct()
                                        .ToList(),

                    Editions = g.Select(x => x.Edition)
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .Distinct()
                                .ToList(),

                    BLRecordIDs = g.Select(x => x.BLRecordID)
                                   .Where(x => x.HasValue)
                                   .Select(x => x.Value)
                                   .Distinct()
                                   .ToList(),

                    Names = g.Select(x => x.Name)
                             .Where(x => !string.IsNullOrWhiteSpace(x))
                             .Distinct()
                             .ToList(),

                    Roles = g.Select(x => x.Role)
                             .Where(x => !string.IsNullOrWhiteSpace(x))
                             .Distinct()
                             .ToList(),

                    OtherNames = g.Select(x => x.OtherNames)
                                  .Where(x => !string.IsNullOrWhiteSpace(x))
                                  .Distinct()
                                  .ToList(),

                    Topics = g.SelectMany(x => (x.Topics ?? "")
                                .Split(';', StringSplitOptions.RemoveEmptyEntries))
                                .Select(x => x.Trim())
                                .Distinct()
                                .ToList(),

                    Languages = g.SelectMany(x => (x.Languages ?? "")
                                  .Split(';', StringSplitOptions.RemoveEmptyEntries))
                                  .Select(x => x.Trim())
                                  .Distinct()
                                  .ToList()
                })
                .ToList();

            foreach (var groupedComic in grouped)
            {
                var existingComic = await _context.DataComics
                    .FirstOrDefaultAsync(c =>
                        c.Title == groupedComic.Title &&
                        c.Publisher == groupedComic.Publisher &&
                        c.Genre == groupedComic.Genre);

                if (existingComic != null)
                {
                    groupedComic.ComicId = existingComic.Id;
                }
                else
                {
                    var comicEntity = new DatabaseComic
                    {
                        Title = groupedComic.Title,
                        Publisher = groupedComic.Publisher,
                        Genre = groupedComic.Genre,
                        DataJson = JsonSerializer.Serialize(groupedComic)
                    };

                    _context.DataComics.Add(comicEntity);
                }
            }

            await _context.SaveChangesAsync();

            comicStore.SetComics(grouped);

            return View(grouped.Take(1000).ToList());
        }
        private string uknownTableValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? "Missing" : value;
            
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
