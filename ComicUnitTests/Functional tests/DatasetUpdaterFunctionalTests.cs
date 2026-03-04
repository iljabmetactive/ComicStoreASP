using ComicStoreASP.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ComicUnitTests.Functional_tests
{
    public class DatasetUpdaterFunctionalTests
    {
        [Fact]
        public async Task VersionUpdate_ShouldNotDelete_SavedSearches()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            context.SavedSearches.Add(new SavedSearch { UserId = "test-user", SearchJson = "Test", CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();

            context.DatatableVersions.Add(new DatatableVersion
            {
                VersionName = "V2",
                ImportedAt = DateTime.UtcNow,
                IsActive = true
            });

            await context.SaveChangesAsync();

            Assert.Single(context.SavedSearches);
        }

        [Fact]
        public async Task OnlyOneVersion_ShouldBeActive_AfterUpdate()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            context.DatatableVersions.AddRange(
                new DatatableVersion { VersionName = "V1", IsActive = true },
                new DatatableVersion { VersionName = "V2", IsActive = true }
            );

            await context.SaveChangesAsync();

            var active = context.DatatableVersions.Count(v => v.IsActive);

            Assert.True(active > 1);
        }

        [Fact]
        public async Task SwitchingActiveVersion_Should_StopOld()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            var oldVersion = new DatatableVersion
            {
                VersionName = "old",
                IsActive = true,
                ImportedAt = DateTime.UtcNow
            };

            context.DatatableVersions.Add(oldVersion);
            await context.SaveChangesAsync();

            var newVersion = new DatatableVersion
            {
                VersionName = "new",
                IsActive = true,
                ImportedAt = DateTime.UtcNow
            };

            oldVersion.IsActive = false;
            context.DatatableVersions.Add(newVersion);

            await context.SaveChangesAsync();

            Assert.Single(context.DatatableVersions.Where(v => v.IsActive));
        }
    }
}
