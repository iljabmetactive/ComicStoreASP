using ComicStoreASP.Models;

namespace ComicStoreASP.Services
{
    public class ComicGenreFilter
    {
        public IEnumerable<ComicGroupedViewModel> FilterByGenre(IEnumerable<ComicGroupedViewModel> comics, string genre)
        {
            if (string.IsNullOrEmpty(genre) || genre == "show all")
                return comics;

            return comics.Where(c => !string.IsNullOrEmpty(c.Genre) &&
                             c.Genre.Contains(genre, StringComparison.OrdinalIgnoreCase));

        }


    }
}
