using ComicStoreASP.Models;
using Microsoft.AspNetCore.Identity;


namespace ComicStoreASP.Data
{
    public class SavedComic
    {
        public int Id { get; set; }

        public string UserId { get; set; }   
        public IdentityUser User { get; set; }

        public int ComicId { get; set; }

        public DatabaseComic Comic { get; set; }

        public DateTime SavedAt { get; set; }

    }
}
