using ComicStoreASP.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace ComicStoreASP.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SavedComic> SavedComics { get; set; }
        public DbSet<SavedSearch> SavedSearches { get; set; }
        public DbSet<FlaggedComic> ComicFlags { get; set; }
        public DbSet<DatabaseComic> DataComics { get; set; }

        public DbSet<DatatableVersion> DatatableVersions { get; set; }
        public DbSet<searchAnalyticsLog> SearchAnalyticsLogs { get; set; }
    }
}