namespace ComicStoreASP.Data
{
    public class SavedSearch
    {
            public int Id { get; set; }
            public string UserId { get; set; }

            public string SearchJson { get; set; }
            public DateTime CreatedAt { get; set; }

    }
}
