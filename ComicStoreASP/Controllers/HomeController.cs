using ComicStoreASP.Data;
using ComicStoreASP.Models;
using ComicStoreASP.Services;
using ComicStoreASP.Views.Models;
using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating.CodeCompilation;
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
            var activeVersionId = _context.DatatableVersions
                .Where(v => v.IsActive)
                .Select(v => v.Id)
                .FirstOrDefault();

            if (activeVersionId == 0)
                return View(new List<ComicGroupedViewModel>());

            var comicsFromDb = _context.DataComics
                .Where(c => c.DatasetVersionId == activeVersionId)
                .AsNoTracking()
                .ToList();

            var comics = comicsFromDb
                .Select(c =>
                {
                    var comic = JsonSerializer.Deserialize<ComicGroupedViewModel>(c.DataJson)!;

                    comic.Names ??= new List<string>();
                    comic.Roles ??= new List<string>();
                    comic.OtherNames ??= new List<string>();
                    comic.PublicationYears ??= new List<string>();
                    comic.Editions ??= new List<string>();
                    comic.BLRecordIDs ??= new List<int>();
                    comic.Topics ??= new List<string>();
                    comic.Languages ??= new List<string>();

                    return comic;
                })
                .ToList();

            return View(comics.Take(1000));
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

        public class SaveComicRequest { public int ComicId { get; set; } }
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

        public class FlagComicRequest { public int ComicId { get; set; } public string Reason { get; set; } }
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

            var filtered = comics.Where(c =>
            string.IsNullOrWhiteSpace(searchVariables.Title) || c.Title.Contains(searchVariables.Title.Trim(), StringComparison.OrdinalIgnoreCase) &&
            string.IsNullOrWhiteSpace(searchVariables.AuthorName) || c.Names.Any(n => n.Contains(searchVariables.AuthorName.Trim(), StringComparison.OrdinalIgnoreCase)) &&
            string.IsNullOrWhiteSpace(searchVariables.YearOfPublication) || c.PublicationYears.Any(y => y.Contains(searchVariables.YearOfPublication.Trim(), StringComparison.OrdinalIgnoreCase)) &&
            string.IsNullOrWhiteSpace(searchVariables.Genre) || c.Genre.Contains(searchVariables.Genre.Trim(), StringComparison.OrdinalIgnoreCase) &&
            string.IsNullOrWhiteSpace(searchVariables.Edition) || c.Editions.Any(e => e.Contains(searchVariables.Edition.Trim(), StringComparison.OrdinalIgnoreCase)) &&
            string.IsNullOrWhiteSpace(searchVariables.Language) || c.Languages.Any(l => l.Contains(searchVariables.Language.Trim(), StringComparison.OrdinalIgnoreCase))
            ).ToList();

            if (!filtered.Any())
            {
                _logger.LogInformation("Advanced search returned 0 results for: {@SearchVariables}", searchVariables);
                foreach (var comic in comics.Take(5))
                {
                    _logger.LogInformation("Comic: {@Comic}", comic);
                }
            }

            return View("Index", filtered);
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

            var newVersion = new DatatableVersion
            {
                VersionName = $"Import {DateTime.UtcNow}",
                ImportedAt = DateTime.UtcNow,
                IsActive = false
            };

            _context.DatatableVersions.Add(newVersion);
            await _context.SaveChangesAsync();

            foreach (var groupedComic in grouped)
            {
                var comicEntity = new DatabaseComic
                {
                    Title = groupedComic.Title,
                    Publisher = groupedComic.Publisher,
                    Genre = groupedComic.Genre,
                    DatasetVersionId = newVersion.Id,
                    DataJson = JsonSerializer.Serialize(groupedComic)
                };

                _context.DataComics.Add(comicEntity);
            }
            var oldVersions = _context.DatatableVersions
            .Where(v => v.IsActive)
            .ToList();

            foreach (var version in oldVersions)
            {
                version.IsActive = false;
            }

            newVersion.IsActive = true;

            await _context.SaveChangesAsync();

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");

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

        private IQueryable<DatabaseComic> GetActiveComics()
        {
            var activeVersionId = _context.DatatableVersions
                .Where(v => v.IsActive)
                .Select(v => v.Id)
                .FirstOrDefault();


            if (activeVersionId == 0)
            {
                return Enumerable.Empty<DatabaseComic>().AsQueryable();
            }

            

            return _context.DataComics
                .Where(c => c.DatasetVersionId == activeVersionId);
        }
    }
}
