namespace ComicStoreASP.Models.View
{
    public class SearchAnalyticsDashboardViewModel
    {
        public IEnumerable<object> TopSearches { get; set; }
        public IEnumerable<object> TopResults { get; set; }
        public IEnumerable<object> Over100Results { get; set; }
    }
}
