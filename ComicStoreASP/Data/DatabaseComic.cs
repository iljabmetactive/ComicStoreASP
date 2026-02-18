using System.ComponentModel.DataAnnotations;

namespace ComicStoreASP.Data
{
    public class DatabaseComic
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Publisher { get; set; }
        public string Genre { get; set; }
        public string DataJson { get; set; }
    }


}
