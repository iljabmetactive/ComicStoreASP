using ComicStoreASP.Models;
using ComicStoreASP.Services;
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

        // Fix CS0246 and CS1520: Correct constructor name and parameter type
        public HomeController(CSVDataReader csvDataReader, ILogger<HomeController> logger)
        {
            _csvDataReader = csvDataReader;
            _logger = logger;
        }
        //[HttpGet("")]
        [HttpGet]

        public IActionResult Index()
        {
            return View(new List<Comics>());
        }
        [HttpPost]
        public IActionResult Index([FromForm] IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                ModelState.AddModelError("", "Please select a valid CSV file.");
                return View(new List<Comics>());
            }


            using var stream = csvFile.OpenReadStream();
            var comics = _csvDataReader.ReadCsvFile(stream, 7000).ToList();

            if (comics.Count == 7000)
            {
                ViewBag.Warning = "Only the first 7000 records were loaded.";
            }

            return View(comics); // SAME VIEW
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
