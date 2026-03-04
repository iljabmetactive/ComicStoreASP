using ComicStoreASP.Controllers;
using ComicStoreASP.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicUnitTests.Unit_Tests
{
    public class SaveComicTests
    {
        [Fact]
        public async Task SaveComic_ShouldSave_WhenLoggedIn()
        {
            var context = TestDbHelper.GetInMemoryDbContext();
            var controller = ControllerFactory.CreateController(context, "user1");

            var result = await controller.SaveComic(
                new HomeController.SaveComicRequest { ComicId = 1 });

            Assert.IsType<OkResult>(result);
            Assert.Single(context.SavedComics);
        }

        [Fact]
        public async Task SaveComic_Should_NotDuplicate()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            context.SavedComics.Add(new SavedComic
            {
                ComicId = 1,
                UserId = "user1"
            });
            await context.SaveChangesAsync();

            var controller = ControllerFactory.CreateController(context, "user1");

            await controller.SaveComic(
                new HomeController.SaveComicRequest { ComicId = 1 });

            Assert.Single(context.SavedComics);
        }

        [Fact]
        public async Task SaveComic_StopUser_WhenNotLoggedIn()
        {
            var context = TestDbHelper.GetInMemoryDbContext();
            var controller = ControllerFactory.CreateController(context);

            var result = await controller.SaveComic(
                new HomeController.SaveComicRequest { ComicId = 1 });

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task SaveComic_ShouldReturnError_WhenUserIsMissing()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            var controller = ControllerFactory.CreateController(context, userId: "");

            var result = await controller.SaveComic(
                new HomeController.SaveComicRequest { ComicId = 1 });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task SaveComic_ShouldReturnError_WhenComicIdIsInvalid()
        {
            var context = TestDbHelper.GetInMemoryDbContext();
            var controller = ControllerFactory.CreateController(context, "user1");
            var result = await controller.SaveComic(
                new HomeController.SaveComicRequest { ComicId = -1 });
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task SaveComic_ShouldReturnError_WhenComicDoesNotExist()
        {
            var context = TestDbHelper.GetInMemoryDbContext();
            var controller = ControllerFactory.CreateController(context, "user1");
            var result = await controller.SaveComic(
                new HomeController.SaveComicRequest { ComicId = 999 });
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task SaveComic_ShouldHandle_SavesAtTheSameTime()
        {
            var context = TestDbHelper.GetInMemoryDbContext();
            var controller = ControllerFactory.CreateController(context, "user1");
            var request = new HomeController.SaveComicRequest { ComicId = 1 };
            await Task.WhenAll(
                controller.SaveComic(request),
                controller.SaveComic(request)
            );
            Assert.Single(context.SavedComics);
        }
    }
}