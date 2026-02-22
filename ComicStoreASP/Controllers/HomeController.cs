using ComicStoreASP.Data;
using ComicStoreASP.Models;
using ComicStoreASP.Models.View;
using ComicStoreASP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using ComicStoreASP.Models.Request;

namespace ComicStoreASP.Controllers
{
    public class HomeController : Controller
    {
        private readonly CSVDataReader _csvDataReader;
        private readonly ILogger<HomeController> _logger;
        private readonly ComicGenreFilter genreFilter;
        private readonly ComicStore comicStore;
        private readonly AdvancedSearchFunction _advancedSearch;
        private readonly ApplicationDbContext _context;

        public HomeController(CSVDataReader csvDataReader, ILogger<HomeController> logger, 
            ComicGenreFilter _genreFilter, ComicStore comicStore, ApplicationDbContext context)
        {
            _csvDataReader = csvDataReader;
            _logger = logger;
            this.genreFilter = _genreFilter;
            this.comicStore = comicStore;
            _advancedSearch = new AdvancedSearchFunction();
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<ComicGroupedViewModel> comics;

            if (TempData["AdvancedSearchResults"] != null)
            {
                comics = JsonSerializer.Deserialize<List<ComicGroupedViewModel>>(
                    TempData["AdvancedSearchResults"]!.ToString()!
                )!;
            }
            else
            {
                var activeVersionId = _context.DatatableVersions
                    .Where(v => v.IsActive)
                    .Select(v => v.Id)
                    .FirstOrDefault();

                if (activeVersionId == 0)
                    return View(new List<ComicGroupedViewModel>());

                comics = _context.DataComics
                    .Where(c => c.DatasetVersionId == activeVersionId)
                    .AsNoTracking()
                    .ToList()
                    .Select(c =>
                    {
                        var vm = JsonSerializer.Deserialize<ComicGroupedViewModel>(c.DataJson)!;
                        vm.ComicId = c.Id;
                        return vm;
                    })
                    .ToList();
            }

            return View(comics.Take(7000));
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
        //used to save comic ID and display it in the My Comics page for the user.
        public class SaveComicRequest { public int ComicId { get; set; } }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SaveComic([FromBody] SaveComicRequest request)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return BadRequest("UserId missing");

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
            var topSearches = _context.SavedSearches
                .GroupBy(s => s.SearchJson)
                .Select(g => new
                {
                    SearchTerm = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            var topResults = _context.SearchAnalyticsLogs
                .GroupBy(r => r.Comic.Title)
                .Select(g => new
                {
                    ComicTitle = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            var over100 = _context.SearchAnalyticsLogs
                .GroupBy(r => r.Comic.Title)
                .Where(g => g.Count() > 100)
                .Select(g => new
                {
                    ComicTitle = g.Key,
                    Count = g.Count()
                })
                .ToList();

            var model = new SearchAnalyticsDashboardViewModel
            {
                TopSearches = topSearches,
                TopResults = topResults,
                Over100Results = over100
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult GetCurrentSearchAnalytics()
        {
            var data = _context.SavedSearches
                .GroupBy(s => s.SearchJson)
                .Select(g => new {
                    Term = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            return Json(data);
        }

        //used to store the comic ID for the flagged comic and display it in the Flagged Comics page for staff members to see.
        public class FlaggedComicRequest { public int ComicId { get; set; } public string Reason { get; set; } }
        [Authorize(Roles = "Staff")]
        [HttpPost]
        public async Task<IActionResult> FlagComic([FromBody] FlaggedComicRequest request)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return BadRequest("UserId missing");

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
            return View();
        }
        [HttpPost]
        public IActionResult ComicGenreFilter([FromBody] string genre)
        {
            var filtered = genreFilter.FilterByGenre(comicStore.Comics, genre);
            return Json(filtered);
        }

        [HttpPost]
        [Route("Home/AdvancedSearch")]
        public async Task<IActionResult> AdvancedSearchAsync([FromBody]AdvancedSearchVariables searchVariables)
        {
            //Checks for the active version of the dataset
            var activeVersionId = _context.DatatableVersions
                .Where(v => v.IsActive)
                .Select(v => v.Id)
                .FirstOrDefault();

            if (activeVersionId == 0)
                return Json(new List<ComicGroupedViewModel>());

            var comics = _context.DataComics
                .Where(c => c.DatasetVersionId == activeVersionId)
                .AsNoTracking()
                .ToList()
                .Select(c =>
                {
                    var vm = JsonSerializer.Deserialize<ComicGroupedViewModel>(c.DataJson)!;
                    vm.ComicId = c.Id;
                    return vm;
                })
                .ToList();

            var filtered = comics.Where(c =>
                (string.IsNullOrWhiteSpace(searchVariables.Title) ||
                 c.Title.Contains(searchVariables.Title.Trim(), StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(searchVariables.AuthorName) ||
                 c.Names.Any(n => n.Contains(searchVariables.AuthorName.Trim(), StringComparison.OrdinalIgnoreCase))) &&
                (string.IsNullOrWhiteSpace(searchVariables.YearOfPublication) ||
                 c.PublicationYears.Any(y => y.Contains(searchVariables.YearOfPublication.Trim(), StringComparison.OrdinalIgnoreCase))) &&
                (string.IsNullOrWhiteSpace(searchVariables.Genre) ||
                 c.Genre.Contains(searchVariables.Genre.Trim(), StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(searchVariables.Edition) ||
                 c.Editions.Any(e => e.Contains(searchVariables.Edition.Trim(), StringComparison.OrdinalIgnoreCase))) &&
                (string.IsNullOrWhiteSpace(searchVariables.Language) ||
                 c.Languages.Any(l => l.Contains(searchVariables.Language.Trim(), StringComparison.OrdinalIgnoreCase)))
            ).ToList();

            // Save search info
            var savedSearch = new SavedSearch
            {
                SearchJson = JsonSerializer.Serialize(searchVariables),
                CreatedAt = DateTime.UtcNow,
                UserId = User.Identity.IsAuthenticated
                    ? User.FindFirstValue(ClaimTypes.NameIdentifier)
                    : null
            };

            _context.SavedSearches.Add(savedSearch);
            await _context.SaveChangesAsync();

            foreach (var comic in filtered)
            {
                var comicEntity = _context.DataComics.FirstOrDefault(d => d.Title == comic.Title);
                if (comicEntity != null)
                {
                    _context.SearchAnalyticsLogs.Add(new searchAnalyticsLog
                    {
                        SavedSearchId = savedSearch.Id,
                        ComicId = comicEntity.Id
                    });
                }
            }
            await _context.SaveChangesAsync();

            return Json(filtered);
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

            return View("Index");

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
