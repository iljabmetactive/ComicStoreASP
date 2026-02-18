namespace ComicStoreASP.Data
{
    public class FlaggedComic
    {
        public int Id { get; set; }
        public int ComicId { get; set; }
        public DatabaseComic Comic { get; set; }
        public string StaffUserId { get; set; }
        public string Reason { get; set; }
        public DateTime FlaggedAt { get; set; }
    }
}
