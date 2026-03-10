using ComicStoreASP.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicUnitTests.Load_Tests
{
    public class SaveFlagComicLoadTests
    {
        [Fact]
        public async Task SaveComic_ShouldHandle_SavesAtTheSameTime()
        {
            var context = TestDbHelper.GetInMemoryDbContext();
            var controller = ControllerFactory.CreateStaffController(context, "user1");

            var request = new HomeController.SaveComicRequest { ComicId = 1 };

            await Task.WhenAll(
                controller.SaveComic(request),
                controller.SaveComic(request)
            );

            Assert.Single(context.SavedComics);
        }

        [Fact]
        public async Task FlagComic_ShouldHandle_ConcurrentFlags()
        {
            var context = TestDbHelper.GetInMemoryDbContext();
            var controller1 = ControllerFactory.CreateStaffController(
                context,
                userId: "staff1");
            var controller2 = ControllerFactory.CreateStaffController(
                context,
                userId: "staff2");
            var request = new HomeController.FlaggedComicRequest
            {
                ComicId = 1,
                Reason = "Concurrent flag test"
            };
            await Task.WhenAll(
                controller1.FlagComic(request),
                controller2.FlagComic(request)
            );
            Assert.Single(context.ComicFlags);
        }
    }
}
