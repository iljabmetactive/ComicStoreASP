using Bogus;
using ComicStoreASP.Data;
using ComicStoreASP.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace ComicUnitTests.Unit_Tests
{
    public class AdvancedSearchTests
    {
        private static string EnsureGenre(string? genre) =>
            string.IsNullOrWhiteSpace(genre) ? "Unknown" : genre!;

        private static string EnsurePublisher(string? publisher) =>
            string.IsNullOrWhiteSpace(publisher) ? "Unknown Publisher" : publisher!;

        [Fact]
        public async Task AdvancedSearch_ShouldFilter_ByTitle()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            var version = new DatatableVersion { VersionName = "v1", ImportedAt = DateTime.UtcNow, IsActive = true };
            context.DatatableVersions.Add(version);
            await context.SaveChangesAsync();

            var comicVm = new ComicGroupedViewModel
            {
                Title = "Thor",
                Genre = "Superhero",
                Names = new List<string> { "Bob Kane" }
            };

            context.DataComics.Add(new DatabaseComic
            {
                DatasetVersionId = version.Id,
                Title = "Thor",
                Genre = EnsureGenre(comicVm.Genre),
                Publisher = EnsurePublisher(null),
                DataJson = JsonSerializer.Serialize(comicVm)
            });

            await context.SaveChangesAsync();

            var controller = ControllerFactory.CreateController(context, "user1");

            var search = new AdvancedSearchVariables
            {
                Title = "batman"
            };

            var result = await controller.AdvancedSearchAsync(search);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var list = Assert.IsAssignableFrom<List<ComicGroupedViewModel>>(jsonResult.Value);

            Assert.Single(list);
        }

        [Fact]
        public async Task AdvancedSearch_ShouldFilter_ByMultipleParameters()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            var version = new DatatableVersion { VersionName = "v1", ImportedAt = DateTime.UtcNow, IsActive = true };
            context.DatatableVersions.Add(version);
            await context.SaveChangesAsync();

            var faker = new Faker();

            var comic = new ComicGroupedViewModel
            {
                Title = "Spider-Man",
                Genre = "Marvel",
                PublicationYears = new List<string> { "1999" },
                Languages = new List<string> { "English" },
                Names = new List<string> { "Stan Lee" }
            };

            context.DataComics.Add(new DatabaseComic
            {
                DatasetVersionId = version.Id,
                Title = comic.Title,
                Genre = EnsureGenre(comic.Genre),
                Publisher = EnsurePublisher("Marvel Publications"),
                DataJson = JsonSerializer.Serialize(comic)
            });

            await context.SaveChangesAsync();

            var controller = ControllerFactory.CreateController(context, "user1");

            var search = new AdvancedSearchVariables
            {
                Title = "Spider",
                Genre = "Marvel",
                YearOfPublication = "1999"
            };

            var result = await controller.AdvancedSearchAsync(search);
            var json = Assert.IsType<JsonResult>(result);

            var results = Assert.IsAssignableFrom<List<ComicGroupedViewModel>>(json.Value);

            Assert.Single(results);
        }

        [Fact]
        public async Task AdvancedSearch_ShouldLog_Search()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            var version = new DatatableVersion { VersionName = "v1", ImportedAt = DateTime.UtcNow, IsActive = true };
            context.DatatableVersions.Add(version);
            await context.SaveChangesAsync();

            var controller = ControllerFactory.CreateController(context, "user1");

            await controller.AdvancedSearchAsync(new AdvancedSearchVariables
            {
                Title = "Test"
            });

            Assert.Single(context.SavedSearches);
        }

        [Fact]
        public async Task AdvancedSearch_ShouldHandle_EmptyResults()
        {
            var context = TestDbHelper.GetInMemoryDbContext();
            var version = new DatatableVersion { VersionName = "v1", ImportedAt = DateTime.UtcNow, IsActive = true };
            context.DatatableVersions.Add(version);
            await context.SaveChangesAsync();
            var controller = ControllerFactory.CreateController(context, "user1");
            var search = new AdvancedSearchVariables
            {
                Title = "NonExistentComic"
            };
            var result = await controller.AdvancedSearchAsync(search);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var list = Assert.IsAssignableFrom<List<ComicGroupedViewModel>>(jsonResult.Value);
            Assert.Empty(list);
        }

        [Fact]
        public async Task AdvancedSearch_ShouldReturnError_WhenUserIsMissing()
        {
            var context = TestDbHelper.GetInMemoryDbContext();
            var controller = ControllerFactory.CreateController(context);
            var search = new AdvancedSearchVariables
            {
                Title = "Test"
            };
            var result = await controller.AdvancedSearchAsync(search);
            Assert.IsType<UnauthorizedResult>(result);
        }
    }
}
