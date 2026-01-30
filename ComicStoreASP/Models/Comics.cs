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
    }
}
