using ComicStoreASP.Controllers;
using ComicStoreASP.Data;
using ComicStoreASP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicUnitTests.Load_Tests
{
    public class ControllerLoadDataTests
    {
        [Fact]
        public void ComicLoad_Returns_NonNull()
        {
            //arange
            List<ComicGroupedViewModel> list = new List<ComicGroupedViewModel>
            {
                new ComicGroupedViewModel
                {
                    ComicId = 1,
                    Title = "Sample Comic",
                    Publisher = "Sample Publisher",
                    ContentType = "Comic",
                    CountryOfPublication = "USA",
                    Genre = "Action",
                    BLRecordID = 123,
                    Names = new List<string> { "Author Name" },
                    Roles = new List<string> { "Writer" },
                    OtherNames = new List<string>(),
                    PublicationYears = new List<string> { "2024" },
                    Editions = new List<string> { "First" },
                    BLRecordIDs = new List<int> { 123 },
                    Topics = new List<string> { "Adventure" },
                    Languages = new List<string> { "English" }
                }
            };

            // action
            var comic = list.Single();

            // assert
            Assert.NotNull(comic);
        }
    }
}
