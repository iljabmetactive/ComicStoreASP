using ComicStoreASP.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ComicUnitTests.Integration_tests
{
    public class DatasetUpdaterIntegTests
    {
        [Fact]
        public async Task DataComics_ShouldLink_ToDatatableVersion()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            var version = new DatatableVersion
            {
                VersionName = "V1",
                ImportedAt = DateTime.UtcNow,
                IsActive = true
            };

            context.DatatableVersions.Add(version);
            await context.SaveChangesAsync();

            var comic = new DatabaseComic
            {
                Title = "Batman",
                DatasetVersionId = version.Id,
                DataJson = "{}",
                Genre = "Action",
                Publisher = "DC"
            };

            context.DataComics.Add(comic);
            await context.SaveChangesAsync();

            Assert.Equal(version.Id, comic.DatasetVersionId);
        }

        [Fact]
        public async Task DeletingVersion_ShouldDelete_AssociatedComics()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            var version = new DatatableVersion
            {
                VersionName = "V1",
                ImportedAt = DateTime.UtcNow
            };

            context.DatatableVersions.Add(version);
            await context.SaveChangesAsync();

            context.DataComics.Add(new DatabaseComic
            {
                Title = "Test",
                DatasetVersionId = version.Id,
                DataJson = "{}",
                Genre = "TestGenre",
                Publisher = "TestPublisher"
            });

            await context.SaveChangesAsync();

            context.DatatableVersions.Remove(version);
            await context.SaveChangesAsync();

            Assert.Empty(context.DataComics);
        }

        [Fact]
        public void DataComics_ShouldHave_DatasetVersionId()
        {
            var property = typeof(DatabaseComic).GetProperty("DatasetVersionId");

            Assert.NotNull(property);
        }

        [Fact]
        public async Task MultipleVersions_CanBe_Stored()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            context.DatatableVersions.AddRange(
                new DatatableVersion { VersionName = "V1", ImportedAt = DateTime.UtcNow },
                new DatatableVersion { VersionName = "V2", ImportedAt = DateTime.UtcNow }
            );

            await context.SaveChangesAsync();

            Assert.Equal(2, context.DatatableVersions.Count());
        }
    }
}
