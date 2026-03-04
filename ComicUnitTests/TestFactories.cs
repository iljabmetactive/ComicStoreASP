using ComicStoreASP.Controllers;
using ComicStoreASP.Data;
using ComicStoreASP.Models;
using ComicStoreASP.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory; // Add this using directive
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ComicUnitTests
{
    internal class TestFactories
    {
    }

    public static class TestDbHelper
    {
        public static ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }
    }

    public static class ControllerFactory
    {
        public static HomeController CreateController(
            ApplicationDbContext context,
            string userId = null,
            string role = null)
        {
            var logger = new Mock<ILogger<HomeController>>();
            var csvReader = new Mock<CSVDataReader>();
            var genreFilter = new Mock<ComicGenreFilter>();
            var comicStore = new Mock<ComicStore>();

            var controller = new HomeController(
                csvReader.Object,
                logger.Object,
                genreFilter.Object,
                comicStore.Object,
                context
            );

            if (userId != null)
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

                if (role != null)
                    claims.Add(new Claim(ClaimTypes.Role, role));

                var identity = new ClaimsIdentity(claims, "TestUser");
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(identity)
                    }
                };
            }

            return controller;
        }
    }

    public static class TestVariableFactory
    {
        public static DatabaseComic CreateComic(string title = "TestComic")
        {
            return new DatabaseComic
            {
                Title = title,
                Genre = "Superhero",
                Publisher = "DC",
                DataJson = "{}"
            };
        }

        public static SavedSearch CreateSavedSearch(string search = "Test", string userId = "user1")
        {
            return new SavedSearch
            {
                SearchJson = search,
                UserId = userId
            };
        }
    }
}
