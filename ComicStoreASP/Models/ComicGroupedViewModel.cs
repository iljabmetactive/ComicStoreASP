namespace ComicStoreASP.Models
{
    public class ComicGroupedViewModel : Comics
    {
        public string Title { get; set; }
        public string Publisher { get; set; }
        public string ContentType { get; set; }
        public string CountryOfPublication { get; set; }
        public string Genre { get; set; }
        public List<string> Names { get; set; } = new();
        public List<string> Roles { get; set; } = new();
        public List<string> OtherNames { get; set; } = new();

        public List<string> PublicationYears { get; set; } = new();
        public List<string> Editions { get; set; } = new();
        public List<int> BLRecordIDs { get; set; } = new();
        public List<string> Topics { get; set; } = new();
        public List<string> Languages { get; set; } = new();
    }
}
