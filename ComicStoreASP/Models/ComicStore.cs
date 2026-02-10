using ComicStoreASP.Views.Models;

namespace ComicStoreASP.Models
{
    public class ComicStore
    {
        private List<ComicGroupedViewModel> _comics = new();

        public IReadOnlyList<ComicGroupedViewModel> Comics => _comics;

        public void SetComics(List<ComicGroupedViewModel> comics)
        {
            _comics = comics;
        }
        
    }
}
