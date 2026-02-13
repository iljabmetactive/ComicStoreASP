using ComicStoreASP.Models;
using static ComicStoreASP.Models.InterfaceLibrary;

namespace ComicStoreASP.Services
{
//following the O in SOLID principles by creating sorting functions that allow for easy implementation without changing existing code.

public class SortByName : IComicSortBy
{
public IEnumerable<ComicGroupedViewModel> SortAssendingOrDescending(IEnumerable<ComicGroupedViewModel> comics, bool descending) =>
    descending
        ? comics.OrderByDescending(c => c.Names)
        : comics.OrderBy(c => c.Names);
}

public class SortByYearOfPublication : IComicSortBy
{
    public IEnumerable<ComicGroupedViewModel> SortAssendingOrDescending(IEnumerable<ComicGroupedViewModel> comics, bool descending) =>
        descending
            ? comics.OrderByDescending(c => c.PublicationYears)
            : comics.OrderBy(c => c.PublicationYears);
}

public class SortBy
{
    private readonly Dictionary<string, IComicSortBy> sortingStrat = new()
{
    { "Name", new SortByName() },
    { "Year of Publication", new SortByYearOfPublication() },
};

    public IComicSortBy SortingParameters(string sortBy) =>
        sortingStrat.ContainsKey(sortBy) ? sortingStrat[sortBy]
            : new SortByName();
}
}



