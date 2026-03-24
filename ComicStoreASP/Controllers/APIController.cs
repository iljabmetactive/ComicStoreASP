using ComicStoreASP.Data;
using ComicStoreASP.Models;
using ComicStoreASP.Services;
using Microsoft.AspNetCore.Mvc;
using OpenQA.Selenium.Internal;
using System;
using System.Data.Entity;
using System.Linq;
using System.Text.Json;

namespace ComicStoreASP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class APIController : Controller
    {
        private readonly CSVDataReader _csvDataReader;
        private readonly ILogger<HomeController> _logger;
        private readonly ComicGenreFilter genreFilter;
        private readonly ComicStore comicStore;
        private readonly AdvancedSearchFunction _advancedSearch;
        private readonly ApplicationDbContext _context;
        public APIController(CSVDataReader csvDataReader, ILogger<HomeController> logger, ComicGenreFilter _genreFilter, 
            ComicStore comicStore, ApplicationDbContext context)
        {
            _csvDataReader = csvDataReader;
            _logger = logger;
            this.genreFilter = _genreFilter;
            this.comicStore = comicStore;
            _advancedSearch = new AdvancedSearchFunction();
            _context = context;
        }

        [HttpGet("suggestions")]
        public IActionResult SearchSuggestions(string field, string search)
        {
            if (string.IsNullOrWhiteSpace(search) || string.IsNullOrWhiteSpace(field))
                return Ok(new List<string>());

            var comics = _context.DataComics
                .AsNoTracking()
                .Take(5000)
                .ToList()
                .Select(c => JsonSerializer.Deserialize<ComicGroupedViewModel>(c.DataJson)!);

            List<string> results = new();

            switch (field.ToLower())
            {
                case "title":
                    results = comics
                        .Where(c => c.Title != null &&
                                    c.Title.Contains(search, StringComparison.OrdinalIgnoreCase))
                        .Select(c => c.Title)
                        .Distinct()
                        .Take(10)
                        .ToList();
                    break;

                case "author":
                    results = comics
                        .Where(c => c.Names.Any(n =>
                            n.Contains(search, StringComparison.OrdinalIgnoreCase)))
                        .SelectMany(c => c.Names)
                        .Where(n => n.Contains(search, StringComparison.OrdinalIgnoreCase))
                        .Distinct()
                        .Take(10)
                        .ToList();
                    break;

                case "genre":
                    results = comics
                        .Where(c => c.Genre != null &&
                                    c.Genre.Contains(search, StringComparison.OrdinalIgnoreCase))
                        .Select(c => c.Genre)
                        .Distinct()
                        .Take(10)
                        .ToList();
                    break;
            }

            return Ok(results);
        }
        //var suggestions = comics
        //    .Where(searchLog => searchLog.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) 
        //        || searchLog.Names.Any(name => name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
        //        || searchLog.Genre.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
        //    .SelectMany(searchLog => new List<string>
        //    {
        //        searchLog.Title,
        //        searchLog.Genre
        //    }.Concat(searchLog.Names))
        //    .Where(suggestions => !string.IsNullOrWhiteSpace(suggestions))
        //    .Distinct()
        //    .Take(10)
        //    .ToList();
    }
}
