using ComicStoreASP.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicUnitTests.Unit_Tests
{

    //the test bellow revealed the lack of proper validation in the FlagComic method,
    //which allowed non-staff users to flag comics and also allowed empty reasons.
    //The test also revealed that there was no check for duplicate flags,
    //which could lead to multiple flags for the same comic from the same user.
    //Additionally, the test showed that there was no handling for concurrent flags,
    //which could lead to visual errors or data corruption. Finally, the test revealed that there was no trimming of the reason string,
    //which could lead to inconsistent data in the database. These issues were addressed by adding proper validation for user roles,
    //reason content, duplicate flags, concurrent flags, and trimming of the reason string in the FlagComic method.
    public class FlagComicTests
    {
        [Fact]
        public async Task FlagComic_ShouldWork_ForStaffUsers()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            var controller = ControllerFactory.CreateStaffController(
                context,
                userId: "staff1"
            );

            var result = await controller.FlagComic(
                new HomeController.FlaggedComicRequest
                {
                    ComicId = 1,
                    Reason = "Incorrect data"
                });

            Assert.IsType<OkResult>(result);
            Assert.Single(context.ComicFlags);
        }

        [Fact]
        public async Task FlagComic_ShouldStop_NonStaffUser()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            var controller = ControllerFactory.CreateLoggedInController(
                context,
                "user1");

            var result = await controller.FlagComic(
                new HomeController.FlaggedComicRequest
                {
                    ComicId = 1,
                    Reason = "Invalid"
                });

            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task FlagComic_ShouldReturnError_WhenUserIsMissing()
        {
            var context = TestDbHelper.GetInMemoryDbContext();
            var controller = ControllerFactory.CreateStaffController(
                context,
                userId: "");
            var result = await controller.FlagComic(
                new HomeController.FlaggedComicRequest
                {
                    ComicId = 1,
                    Reason = "Invalid"
                });
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task FlagComic_ShouldReturnError_WhenReasonIsMissing()
        {
            var context = TestDbHelper.GetInMemoryDbContext();
            var controller = ControllerFactory.CreateStaffController(
                context,
                userId: "staff1");
            var result = await controller.FlagComic(
                new HomeController.FlaggedComicRequest
                {
                    ComicId = 1,
                    Reason = ""
                });
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task FlagComic_ShouldReturnError_WhenComicIdIsInvalid()
        {
            var context = TestDbHelper.GetInMemoryDbContext();
            var controller = ControllerFactory.CreateStaffController(
                context,
                userId: "staff1");
            var result = await controller.FlagComic(
                new HomeController.FlaggedComicRequest
                {
                    ComicId = -1,
                    Reason = "Invalid ID"
                });
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task FlagComic_ShouldNotDuplicateFlags()
        {
            var context = TestDbHelper.GetInMemoryDbContext();
            var controller = ControllerFactory.CreateStaffController(
                context,
                userId: "staff1");
            var request = new HomeController.FlaggedComicRequest
            {
                ComicId = 1,
                Reason = "Duplicate flag test"
            };
            await controller.FlagComic(request);
            await controller.FlagComic(request);
            Assert.Single(context.ComicFlags);
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

        [Fact]
        public async Task FlagComic_ShouldAllowComicFlags_OnSameList_FromDifferentUsers()
        {
            var context = TestDbHelper.GetInMemoryDbContext();
            var controller1 = ControllerFactory.CreateStaffController(
                context,
                userId: "staff1");
            var controller2 = ControllerFactory.CreateStaffController(
                context,
                userId: "staff2");
            await controller1.FlagComic(new HomeController.FlaggedComicRequest
            {
                ComicId = 1,
                Reason = "First flag"
            });
            await controller2.FlagComic(new HomeController.FlaggedComicRequest
            {
                ComicId = 2,
                Reason = "Second flag"
            });
            Assert.Equal(2, context.ComicFlags.Count());
        }

        [Fact]
        public async Task FlagComic_ShouldTrimReason()
        {
            var context = TestDbHelper.GetInMemoryDbContext();
            var controller = ControllerFactory.CreateStaffController(
                context,
                userId: "staff1");
            await controller.FlagComic(new HomeController.FlaggedComicRequest
            {
                ComicId = 1,
                Reason = "   Reason with spaces   "
            });
            var flag = context.ComicFlags.First();
            Assert.Equal("Reason with spaces", flag.Reason);
        }
    }
}
