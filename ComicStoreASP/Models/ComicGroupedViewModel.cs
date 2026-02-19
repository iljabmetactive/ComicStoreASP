namespace ComicStoreASP.Models
{
    public class ComicGroupedViewModel
    {
        public int ComicId { get; set; }
        public string Title { get; set; }
        public string Publisher { get; set; }
        public string ContentType { get; set; }
        public string CountryOfPublication { get; set; }
        public string Genre { get; set; }

        public int? BLRecordID { get; set; }
        public List<string> Names { get; set; } = new();
        public List<string> Roles { get; set; } = new();
        public List<string> OtherNames { get; set; } = new();

        public List<string> PublicationYears { get; set; } = new();
        public List<string> Editions { get; set; } = new();
        public List<int> BLRecordIDs { get; set; } = new();
        public List<string> Topics { get; set; } = new();
        public List<string> Languages { get; set; } = new();
        public string DataJson { get; internal set; }

        public string NamesString => Names != null ? string.Join(",", Names) : "";
        public string RolesString => Roles != null ? string.Join(",", Roles) : "";
        public string EditionsString => Editions != null ? string.Join(",", Editions) : "";
        public string TopicsString => Topics != null ? string.Join(",", Topics) : "";
        public string LanguagesString => Languages != null ? string.Join(",", Languages) : "";
        public string YearsString => PublicationYears != null ? string.Join(",", PublicationYears) : "";
    }
}
