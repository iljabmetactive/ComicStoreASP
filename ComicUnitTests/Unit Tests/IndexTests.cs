using ComicStoreASP.Data;
using ComicStoreASP.Models;
using Microsoft.AspNetCore.Mvc;
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
        public void Index_ShouldReturn_OnlyActiveVersion()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            var active = new DatatableVersion { IsActive = true, VersionName ="V2" };
            var old = new DatatableVersion { IsActive = false, VersionName = "V1" };

            context.DatatableVersions.AddRange(active, old);
            context.SaveChanges();

            context.DataComics.Add(new DatabaseComic
            {
                DatasetVersionId = active.Id,
                Title = "ActiveComic",
                DataJson = JsonSerializer.Serialize(new ComicGroupedViewModel()),
                Genre = "Superhero",
                Publisher = "Marvel"
            });

            context.DataComics.Add(new DatabaseComic
            {
                DatasetVersionId = old.Id,
                Title = "OldComic",
                DataJson = JsonSerializer.Serialize(new ComicGroupedViewModel()),
                Genre = "Superhero",
                Publisher = "DC"
            });

            context.SaveChanges();

            var controller = ControllerFactory.CreateController(context);

            var result = controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var list = Assert.IsAssignableFrom<IEnumerable<ComicGroupedViewModel>>(view.Model);

            Assert.Single(list);
        }
    }
}
