using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;

//"Name","Dates associated with name","Type of name","Role","Other names","BL record ID","Type of resource","Content type",
//"BNB number","ISBN","Title","Variant titles","Series title","Number within series","Country of publication","Place of publication",
//"Publisher","Date of publication","Edition","Physical description","Dewey classification","BL shelfmark","Topics","Genre","Languages","Notes"
namespace ComicStoreASP.Models
{
    public class Comics
    {
        public string? Title { get; set; }
        public string? Name { get; set; }

        public string? Role { get; set; }
        public string? OtherNames { get; set; }
        public int? BLRecordID { get; set; }
        public string? ContentType { get; set; }
        
        public string? CountryOfPublication { get; set; }

        public string? Publisher { get; set; }

        public string? DateOfPublication { get; set; }

        public string? Edition { get; set; }

        public string? Topics { get; set; }

        public string? Genre { get; set; }

        public string? Languages { get; set; }

        public Dictionary<string, string> NameKeyValue =>
            ParseKeyValue(Name, "Name");

        public Dictionary<string, string> RoleKeyValue =>
            ParseKeyValue(Role, "Role");
        public Dictionary<string, string> OtherNamesKeyValue => 
            ParseKeyValue(OtherNames, "Other Name");

        public Dictionary<string, string> GenreKeyValue =>
            ParseKeyValue(Genre, "Genre");

        public Dictionary<string, string> PublisherKeyValue =>
            ParseKeyValue(Publisher, "Publisher");

        public Dictionary<string, string> CountryOfPublicationKeyValue =>
            ParseKeyValue(CountryOfPublication, "Country of Publication");

        public Dictionary<string, string> DateOfPublicationKeyValue =>
            ParseKeyValue(DateOfPublication, "Date of Publication");

        public Dictionary<string, string> EditionKeyValue =>
            ParseKeyValue(Edition, "Edition");
        public Dictionary<string, string> ContentTypeKeyValue =>
            ParseKeyValue(ContentType, "Content Type");
        
        public Dictionary<string, string> BLRecordIDKeyValue =>
            BLRecordID.HasValue
                ? new Dictionary<string, string> { { "BL Record ID", BLRecordID.Value.ToString() } }
                : new Dictionary<string, string> { { "BL Record ID", "Unknown" } };

        public Dictionary<string, string> TopicsKeyValue =>
            ParseKeyValue(Topics, "Topic");

        public Dictionary<string, string> LanguagesKeyValue =>
            ParseKeyValue(Languages, "Language");

        private Dictionary<string, string> ParseKeyValue(string? value, string label)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new Dictionary<string, string>
                {
                    { label, "Unknown" }
                };

            return value.Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select((v, i) => new { v = v.Trim(), i })
                        .ToDictionary(
                            x => $"{label} {x.i + 1}",
                            x => x.v
                        );
        }
    }
}
