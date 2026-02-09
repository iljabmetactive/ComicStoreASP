using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComicStoreASP.Models;

namespace ComicStoreASP.Views.Models
{
    public class SearchResultAnalyticsModel
    {
        private readonly Dictionary<string, int> comicSearchAmount = new();
        private readonly Dictionary<string, int> comicResultAmount = new();

        // Log the search term
        public void LogSearches(string searches)
        {
            if (string.IsNullOrWhiteSpace(searches))
                return;

            if (comicSearchAmount.ContainsKey(searches))
                comicSearchAmount[searches]++;
            else
                comicSearchAmount[searches] = 1;
        }

        // Log each search result
        public void LogResults(IEnumerable<Comics> results)
        {
            foreach (var comic in results)
            {
                string key = comic.Title ?? "<Unknown>";

                if (comicResultAmount.ContainsKey(key))
                    comicResultAmount[key]++;
                else
                    comicResultAmount[key] = 1;
            }
        }


        public List<string> over100Comics()
        {
            return comicResultAmount
                .Where(c => c.Value > 100)
                .Select(c => $"{c.Key} — {c.Value} amount of times")
                .ToList();
        }


        public List<string> Top10Searches()
        {
            return comicSearchAmount
                .OrderByDescending(s => s.Value)
                .Take(10)
                .Select(s => $"{s.Key} — {s.Value} searches")
                .ToList();
        }

        public List<string> Top10Results()
        {
            return comicResultAmount
                .OrderByDescending(r => r.Value)
                .Take(10)
                .Select(r => $"{r.Key} — {r.Value} results of search")
                .ToList();
        }
    }
}
