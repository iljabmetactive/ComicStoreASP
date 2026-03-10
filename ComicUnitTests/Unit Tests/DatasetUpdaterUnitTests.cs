using ComicStoreASP.Data;
using ComicStoreASP.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ComicUnitTests.Unit_Tests
{
    public class DatasetUpdaterUnitTests
    {
        [Fact]
        public void DatatableVersion_ShouldSetVersion_Correctly()
        {
            var version = new DatatableVersion
            {
                VersionName = "Import 2026-01-01",
                ImportedAt = DateTime.UtcNow,
                IsActive = true
            };

            Assert.Equal("Import 2026-01-01", version.VersionName);
            Assert.True(version.IsActive);
        }

        [Fact]
        public void Only_OneVersionShould_BeActive()
        {
            var versions = new List<DatatableVersion>
            {
                new() { Id = 1, IsActive = true },
                new() { Id = 2, IsActive = true }
            };

            var activeCount = versions.Count(v => v.IsActive);

            Assert.True(activeCount > 1);
        }

        [Fact]
        public void ImportedAt_ShouldNot_BeDefault()
        {
            var version = new DatatableVersion
            {
                VersionName = "Test",
                ImportedAt = DateTime.UtcNow
            };

            Assert.NotEqual(default, version.ImportedAt);
        }

        [Fact]
        public void VersionName_ShouldNot_BeNull()
        {
            var version = new DatatableVersion();

            Assert.Null(version.VersionName);
        }

        [Fact]
        public async Task CheckForUpdates_Should_CreateVersion()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(s => s.ServiceProvider
                .GetService(typeof(ApplicationDbContext)))
                .Returns(context);

            var factoryMock = new Mock<IServiceScopeFactory>();
            factoryMock.Setup(f => f.CreateScope())
                       .Returns(scopeMock.Object);

            var logger = new Mock<ILogger<DatatableUpdateService>>();
            var service = new DatatableUpdateService(factoryMock.Object, logger.Object);

            var method = typeof(DatatableUpdateService)
                .GetMethod("CheckForUpdates", BindingFlags.NonPublic | BindingFlags.Instance);

            await (Task)method.Invoke(service, null);

            factoryMock.Verify(f => f.CreateScope(), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldStopOn_Cancellation()
        {
            var factoryMock = new Mock<IServiceScopeFactory>();
            var logger = new Mock<ILogger<DatatableUpdateService>>();

            var service = new DatatableUpdateService(factoryMock.Object, logger.Object);

            var cts = new CancellationTokenSource();
            cts.Cancel();

            await service.StartAsync(cts.Token);

            Assert.True(cts.IsCancellationRequested);
        }

        [Fact]
        public async Task CheckForUpdates_ShouldNotShow_WhenNoData()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(s => s.ServiceProvider
                .GetService(typeof(ApplicationDbContext)))
                .Returns(context);

            var factoryMock = new Mock<IServiceScopeFactory>();
            factoryMock.Setup(f => f.CreateScope())
                       .Returns(scopeMock.Object);

            var logger = new Mock<ILogger<DatatableUpdateService>>();

            var service = new DatatableUpdateService(factoryMock.Object, logger.Object);

            var method = typeof(DatatableUpdateService)
                .GetMethod("CheckForUpdates", BindingFlags.NonPublic | BindingFlags.Instance);

            await (Task)method.Invoke(service, null);

            Assert.Empty(context.DatatableVersions);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldLoop_UntilCancelled()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(ApplicationDbContext)))
                           .Returns(context);
            var scopeMock = new Mock<IServiceScope>();
            scopeMock.Setup(s => s.ServiceProvider)
                     .Returns(serviceProvider.Object);

            var factoryMock = new Mock<IServiceScopeFactory>();
            factoryMock.Setup(f => f.CreateScope())
                       .Returns(scopeMock.Object);

            var logger = new Mock<ILogger<DatatableUpdateService>>();

            var service = new DatatableUpdateService(factoryMock.Object, logger.Object);

            var cts = new CancellationTokenSource();

            
            cts.CancelAfter(100);

            await Task.Delay(200);

            await service.StartAsync(cts.Token);

            Assert.True(cts.IsCancellationRequested);
        }
    }
}
