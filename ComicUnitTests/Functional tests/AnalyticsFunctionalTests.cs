using ComicStoreASP.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicUnitTests.Functional_tests
{
    public class AnalyticsFunctionalTests
    {
        [Fact]
        public void GetCurrentSearchAnalytics_ShouldReturn_Json()
        {
            var context = TestDbHelper.GetInMemoryDbContext();
            var controller = ControllerFactory.CreateController(context);

            var result = controller.GetCurrentSearchAnalytics();

            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public void GetCurrentSearchAnalytics_ShouldUpdate_InRealTime()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            context.SavedSearches.Add(new SavedSearch
            {
                SearchJson = "Batman",
                UserId = "test-user",
                CreatedAt = DateTime.UtcNow
            });
            context.SaveChanges();

            var controller = ControllerFactory.CreateController(context);

            var result = controller.GetCurrentSearchAnalytics();
            var json = Assert.IsType<JsonResult>(result);

            var data = Assert.IsAssignableFrom<IEnumerable<dynamic>>(json.Value);

            Assert.Single(data);
        }

        [Fact]
        public void GetCurrentSearchAnalytics_ShouldLimit_To10Comics()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            for (int i = 0; i < 20; i++)
            {
                context.SavedSearches.Add(new SavedSearch
                {
                    SearchJson = "Search" + i,
                    UserId = "user-" + (i % 3),
                    CreatedAt = DateTime.UtcNow.AddMinutes(-i)
                });
            }

            context.SaveChanges();

            var controller = ControllerFactory.CreateController(context);

            var result = controller.GetCurrentSearchAnalytics();
            var json = Assert.IsType<JsonResult>(result);

            var data = Assert.IsAssignableFrom<IEnumerable<dynamic>>(json.Value);

            Assert.True(data.Count() <= 10);
        }

        [Fact]
        public void GetCurrentSearchAnalytics_ShouldOrder_ByCount()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            context.SavedSearches.AddRange(
                new SavedSearch { SearchJson = "A", UserId = "u1", CreatedAt = DateTime.UtcNow.AddMinutes(-3) },
                new SavedSearch { SearchJson = "A", UserId = "u2", CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
                new SavedSearch { SearchJson = "B", UserId = "u3", CreatedAt = DateTime.UtcNow.AddMinutes(-1) }
            );

            context.SaveChanges();

            var controller = ControllerFactory.CreateController(context);

            var result = controller.GetCurrentSearchAnalytics();
            var json = Assert.IsType<JsonResult>(result);

            var data = Assert.IsAssignableFrom<IEnumerable<object>>(json.Value);
            var first = data.First();

            var tempPorperty = first.GetType().GetProperty("Term");
            var tempValue = tempPorperty?.GetValue(first)?.ToString();

            Assert.Equal("A", tempValue);
        }
    }
}
