using ComicStoreASP.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicUnitTests.Unit_Tests
{
    public class MyComicsTests
    {
        [Fact]
        public void MyComics_ShouldReturnOnly_CurrentUserComics()
        {
            var context = TestDbHelper.GetInMemoryDbContext();

            context.SavedComics.Add(new SavedComic
            {
                UserId = "user1",
                Comic = new DatabaseComic { Title = "Batman", DataJson ="{}", Genre = "Superhero", Publisher ="DC"}
                
            });

            context.SavedComics.Add(new SavedComic
            {
                UserId = "user2",
                Comic = new DatabaseComic { Title = "Spider-Man", DataJson = "{}", Genre = "Superhero", Publisher = "Marvel" }
            });

            context.SaveChanges();

            var controller = ControllerFactory.CreateController(context, "user1");

            var result = controller.MyComics();

            var view = Assert.IsType<ViewResult>(result);
            var comics = Assert.IsAssignableFrom<List<DatabaseComic>>(view.Model);

            Assert.Single(comics);
        }

        [Fact]
        public void MyComics_ShouldReturnEmptyList_WhenNoComicsSaved()
        {
            var context = TestDbHelper.GetInMemoryDbContext();
            var controller = ControllerFactory.CreateController(context, "user1");
            var result = controller.MyComics();
            var view = Assert.IsType<ViewResult>(result);
            var comics = Assert.IsAssignableFrom<List<DatabaseComic>>(view.Model);
            Assert.Empty(comics);
        }

        [Fact]
        public void MyComics_ShouldReturnEmptyList_WhenUserNotLoggedIn()
        {
            var context = TestDbHelper.GetInMemoryDbContext();
            var controller = ControllerFactory.CreateController(context);
            var result = controller.MyComics();
            var view = Assert.IsType<ViewResult>(result);
            var comics = Assert.IsAssignableFrom<List<DatabaseComic>>(view.Model);
            Assert.Empty(comics);
        }

        [Fact]
        public void MyComics_ShouldHandle_ComicsWithMissingData()
        {
            var context = TestDbHelper.GetInMemoryDbContext();
            context.SavedComics.Add(new SavedComic
            {
                UserId = "user1",
                Comic = new DatabaseComic { Title = "", DataJson ="{}", Genre = "", Publisher ="" }
            });
            context.SaveChanges();
            var controller = ControllerFactory.CreateController(context, "user1");
            var result = controller.MyComics();
            var view = Assert.IsType<ViewResult>(result);
            var comics = Assert.IsAssignableFrom<List<DatabaseComic>>(view.Model);
            Assert.Single(comics);
        }
    }
}
