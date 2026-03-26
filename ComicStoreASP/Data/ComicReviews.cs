using System.ComponentModel.DataAnnotations;

namespace ComicStoreASP.Data
{
    public class ComicReviews
    {
        [Key]
        public int Id { get; set; }
        public int ComicId { get; set; }
        public string? UserId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
