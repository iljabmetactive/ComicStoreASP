using ComicStoreASP.Controllers;
using ComicStoreASP.Data;
using ComicStoreASP.Models;
using ComicStoreASP.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ComicUnitTests.Unit_Tests
{
    public class IndexTests
    {
        [Fact]
        public async Task Index_ShouldReturn_OnlyActiveVersion()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            var csvReader = new Mock<CSVDataReader>();
            var logger = new Mock<ILogger<HomeController>>();
            var genreFilter = new Mock<ComicGenreFilter>();
            var comicStore = new Mock<ComicStore>();

            var csvData = new List<Comics>
            {
                new Comics
                {
                    Title = "Test Comic",
                    Publisher = "Marvel",
                    Genre = "Superhero",
                    ContentType = "Comic"

                }
            };

            csvReader.Setup(r => r.ReadCsvFile(It.IsAny<Stream>(),7000))
                     .Returns(csvData);

            var controller = new HomeController(
                csvReader.Object,
                logger.Object,
                genreFilter.Object,
                comicStore.Object,
                context
            );

            var bytes = Encoding.UTF8.GetBytes("Fake CSV content");
            var stream = new MemoryStream(bytes);

            var file = new FormFile(stream, 0, bytes.Length, "Data", "test.csv");

            var results = await controller.Index(file);

            var view = Assert.IsType<ViewResult>(results);

            Assert.Single(context.DatatableVersions);
            Assert.Single(context.DataComics);
        }
    }
}
