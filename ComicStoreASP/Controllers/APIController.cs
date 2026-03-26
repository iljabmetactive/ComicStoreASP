using ComicStoreASP.Data;
using ComicStoreASP.Models;
using ComicStoreASP.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenQA.Selenium.Internal;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace ComicStoreASP.Controllers
{
    [Route("api")]
    [ApiController]
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

        [HttpPost("reviews/{comicId}")]
        public async Task<IActionResult> AddReview(int comicId, [FromBody] ComicReviews review)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            review.ComicId = comicId;
            review.UserId = userId ?? "anonymous user";
            review.CreatedAt = DateTime.UtcNow;

            _context.ComicReviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("reviews/{comicId}")]
        public async Task<IActionResult> GetReviews(int comicId)
        {
            try {
                var reviews = await _context.ComicReviews
                    .Where(r => r.ComicId == comicId)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                return Ok(reviews);
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
                
        }
    }
}
