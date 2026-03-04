using ComicStoreASP.Data;
using ComicStoreASP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicUnitTests.Load_Tests
{
    public class AnalyticsLoadTests
    {
        [Fact]
        public async Task ConcurrentSearchLogging_ShouldNot_CorruptData()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            var comic = new DatabaseComic
            {
                Title = "ConcurrentComic",
                Publisher = "TestPublisher",
                Genre = "TestGenre",
                DataJson = "{}"
            };
            context.DataComics.Add(comic);
            await context.SaveChangesAsync();

            var tasks = Enumerable.Range(0, 20).Select(async i =>
            {
                var search = new SavedSearch { UserId = $"test-user-{i}", SearchJson = "Test" };
                context.SavedSearches.Add(search);
                await context.SaveChangesAsync();

                context.SearchAnalyticsLogs.Add(new searchAnalyticsLog
                {
                    SavedSearchId = search.Id,
                    ComicId = comic.Id
                });

                await context.SaveChangesAsync();
            });

            await Task.WhenAll(tasks);

            Assert.Equal(20, context.SearchAnalyticsLogs.Count());
        }

        [Fact]
        public async Task HighVolumeSearches_ShouldNot_Cause_PerformanceIssues()
        {
            var context = TestDbHelper.GetInMemoryDbContext();
            var comic = new DatabaseComic
            {
                Title = "PerformanceComic",
                Publisher = "PerfPublisher",
                Genre = "PerfGenre",
                DataJson = "{}"
            };
            context.DataComics.Add(comic);
            await context.SaveChangesAsync();
            var tasks = Enumerable.Range(0, 100).Select(async i =>
            {
                var search = new SavedSearch { UserId = $"perf-user-{i}", SearchJson = "Test" };
                context.SavedSearches.Add(search);
                await context.SaveChangesAsync();
                context.SearchAnalyticsLogs.Add(new searchAnalyticsLog
                {
                    SavedSearchId = search.Id,
                    ComicId = comic.Id
                });
                await context.SaveChangesAsync();
            });
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await Task.WhenAll(tasks);
            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds < 5000, $"Logging took too long: {stopwatch.ElapsedMilliseconds} ms");
        }

        [Fact]
        public async Task DeletingComic_ShouldHandle_ConcurrentLogDeletions()
        {
            var context = TestDbHelper.GetInMemoryDbContext();
            var comic = new DatabaseComic
            {
                Title = "DeleteTestComic",
                Publisher = "DeletePublisher",
                Genre = "DeleteGenre",
                DataJson = "{}"
            };
            context.DataComics.Add(comic);
            await context.SaveChangesAsync();
            var search = new SavedSearch { UserId = "test-user", SearchJson = "Test" };
            context.SavedSearches.Add(search);
            await context.SaveChangesAsync();
            context.SearchAnalyticsLogs.Add(new searchAnalyticsLog
            {
                SavedSearchId = search.Id,
                ComicId = comic.Id
            });
            await context.SaveChangesAsync();
            var deleteTask = Task.Run(async () =>
            {
                context.DataComics.Remove(comic);
                await context.SaveChangesAsync();
            });
            var logTask = Task.Run(async () =>
            {
                context.SearchAnalyticsLogs.RemoveRange(context.SearchAnalyticsLogs.Where(log => log.ComicId == comic.Id));
                await context.SaveChangesAsync();
            });
            await Task.WhenAll(deleteTask, logTask);
            Assert.Empty(context.SearchAnalyticsLogs.Where(log => log.ComicId == comic.Id));
        }

        [Fact]
        public async Task BulkSearchLogging_ShouldMaintain_DataIntegrity()
        {
            var context = TestDbHelper.GetInMemoryDbContext();
            var comic = new DatabaseComic
            {
                Title = "BulkTestComic",
                Publisher = "BulkPublisher",
                Genre = "BulkGenre",
                DataJson = "{}"
            };
            context.DataComics.Add(comic);
            await context.SaveChangesAsync();
            var tasks = Enumerable.Range(0, 50).Select(async i =>
            {
                var search = new SavedSearch { UserId = $"bulk-user-{i}", SearchJson = "BulkTest" };
                context.SavedSearches.Add(search);
                await context.SaveChangesAsync();
                context.SearchAnalyticsLogs.Add(new searchAnalyticsLog
                {
                    SavedSearchId = search.Id,
                    ComicId = comic.Id
                });
                await context.SaveChangesAsync();
            });
            await Task.WhenAll(tasks);
            Assert.Equal(50, context.SearchAnalyticsLogs.Count(log => log.ComicId == comic.Id));
        }
    }
}