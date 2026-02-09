using ComicStoreASP.Models;
using ComicStoreASP.Services;
using ComicStoreASP.Views.Models;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ComicStoreASP.Controllers
{
    //[Route("[controller]")]
    public class HomeController : Controller
    {
        private readonly CSVDataReader _csvDataReader;
        private readonly ILogger<HomeController> _logger;
        private readonly SearchResultAnalyticsModel analytics;

        // Fix CS0246 and CS1520: Correct constructor name and parameter type
        public HomeController(CSVDataReader csvDataReader, ILogger<HomeController> logger, SearchResultAnalyticsModel _analytics)
        {
            _csvDataReader = csvDataReader;
            _logger = logger;
            analytics = _analytics;

        }
        //[HttpGet("")]
        [HttpGet]

        public IActionResult Index()
        {
            return View(new List<ComicGroupedViewModel>());
        }

        [HttpPost]
        public IActionResult LogSearch([FromBody] string searchTerm)
        {
            analytics.LogSearches(searchTerm);
            return Ok();
        }

        [HttpGet]
        public IActionResult GetSearchAnalytics()
        {
            return Json(analytics.Top10Searches());
        }

        [HttpPost]
        public IActionResult Index([FromForm] IFormFile csvFile)
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
            //not all of the comic items load, work here to add all items
            var grouped = comics
                .GroupBy(com => new { Title = com.Title?.Trim(), Publisher = com.Publisher?.Trim(), com.Genre })
                .Select(g => new ComicGroupedViewModel
                {
                    Title = g.Key.Title,
                    Publisher = g.Key.Publisher,
                    Genre = g.Select(x => x.Genre).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? "Unknown",
                    ContentType = g.Select(x => x.ContentType).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? "Unknown",
                    CountryOfPublication = g.Select(x => x.CountryOfPublication).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? "Unknown",

                    PublicationYears = g
                    .Select(x => x.DateOfPublication)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList(),

                    Editions = g
                    .Select(x => x.Edition)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList(),

                    BLRecordIDs = g
                    .Select(x => x.BLRecordID)
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .Distinct()
                    .ToList(),

                    Names = g
                    .Select(x => x.Name)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList(),

                    Roles = g
                    .Select(x => x.Role)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList(),

                    OtherNames = g
                    .Select(x => x.OtherNames)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList(),

                    Topics = g
                    .SelectMany(x => (x.Topics ?? "")
                    .Split(';', StringSplitOptions.RemoveEmptyEntries))
                    .Select(x => x.Trim())
                    .Distinct()
                    .ToList(),

                    Languages = g
                    .SelectMany(x => (x.Languages ?? "")
                    .Split(';', StringSplitOptions.RemoveEmptyEntries))
                    .Select(x => x.Trim())
                    .Distinct()
                    .ToList()

                })
                .ToList();
            return View(grouped); // SAME VIEW
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
