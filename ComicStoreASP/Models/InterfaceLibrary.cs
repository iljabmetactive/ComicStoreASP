namespace ComicStoreASP.Models
{
    public class InterfaceLibrary
    {
        public interface IComicSortBy
        {
            IEnumerable<ComicGroupedViewModel> SortAssendingOrDescending(IEnumerable<ComicGroupedViewModel> comics, bool descending);
        }
    }
}
