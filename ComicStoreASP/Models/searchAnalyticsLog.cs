using ComicStoreASP.Data;
using Humanizer;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ComicStoreASP.Models
{
    public class searchAnalyticsLog
    {
        public int Id { get; set; }

        public int SavedSearchId { get; set; }
        public SavedSearch SavedSearch { get; set; }

        public int ComicId { get; set; }
        public DatabaseComic Comic { get; set; }
    }
}
