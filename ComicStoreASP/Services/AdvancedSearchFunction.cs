using ComicStoreASP.Models;
using System.Linq;

namespace ComicStoreASP.Services
{
    public class AdvancedSearchFunction
    {
        public IEnumerable<ComicGroupedViewModel> Search(
        IEnumerable<ComicGroupedViewModel> comics,
        AdvancedSearchVariables searchVariables)
        {
            var query = comics.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchVariables.Title))
                query = query.Where(c =>
                    c.Title != null &&
                    c.Title.Contains(searchVariables.Title, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(searchVariables.AuthorName))
                query = query.Where(c =>
                    c.Names != null &&
                    c.Names.Any(n =>
                        n != null &&
                        n.Contains(searchVariables.AuthorName, StringComparison.OrdinalIgnoreCase)));

            if (!string.IsNullOrWhiteSpace(searchVariables.YearOfPublication))
                query = query.Where(c =>
                    c.PublicationYears != null &&
                    c.PublicationYears.Any(y =>
                        y != null &&
                        y.Contains(searchVariables.YearOfPublication, StringComparison.OrdinalIgnoreCase)));

            if (!string.IsNullOrWhiteSpace(searchVariables.Genre))
                query = query.Where(c =>
                    c.Genre != null &&
                    c.Genre.Contains(searchVariables.Genre, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(searchVariables.Edition))
                query = query.Where(c =>
                    c.Editions != null &&
                    c.Editions.Any(e =>
                        e != null &&
                        e.Contains(searchVariables.Edition, StringComparison.OrdinalIgnoreCase)));

            if (!string.IsNullOrWhiteSpace(searchVariables.Language))
                query = query.Where(c =>
                    c.Languages != null &&
                    c.Languages.Any(lang =>
                        lang != null &&
                        lang.Contains(searchVariables.Language, StringComparison.OrdinalIgnoreCase)));

            return query.ToList();
        }
    }
}
