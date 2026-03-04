using ComicStoreASP.Models;
using ComicStoreASP.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicUnitTests.Unit_Tests
{
    public class GenreFilterTests
    {
        private readonly ComicGenreFilter _filter = new ComicGenreFilter();

        private List<ComicGroupedViewModel> TestComics()
        {
            return new List<ComicGroupedViewModel>
            {
                new ComicGroupedViewModel { Title = "Batman", Genre = "Superhero" },
                new ComicGroupedViewModel { Title = "Spider-Man", Genre = "Marvel Superhero" },
                new ComicGroupedViewModel { Title = "Calvin", Genre = "Comedy" },
                new ComicGroupedViewModel { Title = "Unknown", Genre = null }
            };
        }

        [Fact]
        public void FilterByGenre_ShouldReturnAll_WhenGenreIsNull()
        {
            var comics = TestComics();

            var result = _filter.FilterByGenre(comics, null);

            Assert.Equal(comics.Count, result.Count());
        }

        [Fact]
        public void FilterByGenre_ShouldReturnAll_WhenShowAll()
        {
            var comics = TestComics();

            var result = _filter.FilterByGenre(comics, "show all");

            Assert.Equal(comics.Count, result.Count());
        }

        [Fact]
        public void FilterByGenre_ShouldBe_CaseInsensitive()
        {
            var comics = TestComics();

            var result = _filter.FilterByGenre(comics, "superhero");

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void FilterByGenre_ShouldIgnoreComics_WithNullGenre()
        {
            var comics = TestComics();

            var result = _filter.FilterByGenre(comics, "Comedy");

            Assert.Single(result);
            Assert.DoesNotContain(result, c => c.Genre == null);
        }

        [Fact]
        public void FilterByGenre_ShouldReturnEmpty_WhenNoMatch()
        {
            var comics = TestComics();

            var result = _filter.FilterByGenre(comics, "Horror");

            Assert.Empty(result);
        }
    }
}
