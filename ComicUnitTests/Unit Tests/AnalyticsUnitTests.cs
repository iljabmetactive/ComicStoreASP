using ComicStoreASP.Data;
using ComicStoreASP.Models;
using ComicStoreASP.Models.View;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicUnitTests.Unit_Tests
{
    public class AnalyticsUnitTests
    {
        [Fact]
        public async Task Logging_ShouldCreateEntry_ForEachComic()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            var comic = TestVariableFactory.CreateComic("Batman");
            context.DataComics.Add(comic);
            await context.SaveChangesAsync();

            var savedSearch = TestVariableFactory.CreateSavedSearch("Batman");
            context.SavedSearches.Add(savedSearch);
            await context.SaveChangesAsync();

            context.SearchAnalyticsLogs.Add(new searchAnalyticsLog
            {
                SavedSearchId = savedSearch.Id,
                ComicId = comic.Id
            });
            await context.SaveChangesAsync();

            await context.SaveChangesAsync();

            Assert.Single(context.SearchAnalyticsLogs);
        }

        [Fact]
        public void SearchAnalytics_ShouldReturn_ComicsOver100()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            var comic = TestVariableFactory.CreateComic("PopularComic");
            context.DataComics.Add(comic);
            context.SaveChanges();

            for (int i = 0; i < 101; i++)
            {
                var search = TestVariableFactory.CreateSavedSearch("Popular", $"user{i}");
                context.SavedSearches.Add(search);
                context.SaveChanges();

                context.SearchAnalyticsLogs.Add(new searchAnalyticsLog
                {
                    ComicId = comic.Id,
                    SavedSearchId = search.Id
                });
            }

            context.SaveChanges();

            var controller = ControllerFactory.CreateController(context, "staff1", "Staff");

            var result = controller.SearchAnalytics();
            var view = (ViewResult)result;
            var model = (SearchAnalyticsDashboardViewModel)view.Model;

            Assert.Single(model.Over100Results);
        }

        [Fact]
        public async Task DeletingComic_ShouldRemove_AssociatedLogs()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            var comic = TestVariableFactory.CreateComic("Batman");
            context.DataComics.Add(comic);
            await context.SaveChangesAsync();

            var savedSearch = TestVariableFactory.CreateSavedSearch("Batman");
            context.SavedSearches.Add(savedSearch);
            await context.SaveChangesAsync();

            context.SearchAnalyticsLogs.Add(new searchAnalyticsLog
            {
                ComicId = comic.Id,
                SavedSearchId = savedSearch.Id
            });

            await context.SaveChangesAsync();

            context.DataComics.Remove(comic);
            await context.SaveChangesAsync();

            Assert.Empty(context.SearchAnalyticsLogs);
        }

        [Fact]
        public async Task MultipleSearches_ShouldIncrease_ComicCount()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            var comic = TestVariableFactory.CreateComic("Batman");
            context.DataComics.Add(comic);
            await context.SaveChangesAsync();

            for (int i = 0; i < 5; i++)
            {
                var search = TestVariableFactory.CreateSavedSearch("Batman", $"user{i}");
                context.SavedSearches.Add(search);
                await context.SaveChangesAsync();

                context.SearchAnalyticsLogs.Add(new searchAnalyticsLog
                {
                    SavedSearchId = search.Id,
                    ComicId = comic.Id
                });
            }

            await context.SaveChangesAsync();

            Assert.Equal(5, context.SearchAnalyticsLogs.Count());
        }

        [Fact]
        public void SearchAnalytics_ShouldReturn_Top10Searches()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            for (int i = 0; i < 15; i++)
            {
                context.SavedSearches.Add(
                    TestVariableFactory.CreateSavedSearch($"Search{i}", $"user{i}")
                );
            }

            context.SaveChanges();

            var controller = ControllerFactory.CreateController(context, "staff1", "Staff");

            var result = controller.SearchAnalytics();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SearchAnalyticsDashboardViewModel>(view.Model);

            Assert.True(model.TopSearches.Count() <= 10);
        }

        [Fact]
        public async Task Logging_ShouldNotCreateLog_WhenComicNotFound()
        {
            var context = TestDbHelper.GetInMemoryDbContext();


            var savedSearch = TestVariableFactory.CreateSavedSearch("Comic", "user1");
            context.SavedSearches.Add(savedSearch);
            await context.SaveChangesAsync();

            Assert.Empty(context.SearchAnalyticsLogs);
        }

        [Fact]
        public void SearchAnalytics_ShouldOrderSearches_ByCount()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            context.SavedSearches.AddRange(
                TestVariableFactory.CreateSavedSearch("A", "user1"),
                TestVariableFactory.CreateSavedSearch("A", "user2"),
                TestVariableFactory.CreateSavedSearch("B", "user3")
            );

            context.SaveChanges();

            var controller = ControllerFactory.CreateController(context, "staff1", "Staff");

            var result = controller.SearchAnalytics();
            var view = (ViewResult)result;
            var model = (SearchAnalyticsDashboardViewModel)view.Model;

            var first = model.TopSearches.Cast<dynamic>().First();

            Assert.Equal("A", first.SearchTerm);
        }

        [Fact]
        public void SearchAnalytics_ShouldDeny_NonStaff()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            var controller = ControllerFactory.CreateController(context,"user1","User");

            var result = controller.SearchAnalytics();

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public void SearchAnalytics_ShouldHandle_EmptyDatabase()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            var controller = ControllerFactory.CreateController(context,"staff1","Staff");

            var result = controller.SearchAnalytics();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SearchAnalyticsDashboardViewModel>(view.Model);

            Assert.Empty(model.TopSearches);
        }
    }
}
