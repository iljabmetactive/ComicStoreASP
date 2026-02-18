namespace ComicStoreASP.Models
{
    internal class SearchAnalyticsViewModel
    {
        public string SearchTerm { get; set; }
        public int Count { get; set; }
        public DateTime LastSearched { get; set; }
    }
}