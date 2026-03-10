using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ComicUnitTests.UITests
{

    public class ComicTableUITests
    {
        static IWebDriver driver = new ChromeDriver();

        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10)); //use this when adding to a variable, otherwise it will throw an error about the driver not being initialized

        private WebDriverWait Wait()
        {
            return new WebDriverWait(driver, TimeSpan.FromSeconds(10)); //Use this when Needing to create a new wait instance within a test method,
                                                                        //otherwise it will throw an error about the driver not being initialized
        }


        [Fact]
        public void ComicsTable_ShouldLoad()
        {
            driver.Navigate().GoToUrl("https://localhost:7210/");

            var table = wait.Until(
                ExpectedConditions.ElementIsVisible(By.Id("comicsTable")));

            Assert.True(table.Displayed);
        }

        [Fact]
        public void ComicsTable_ShouldContainRows()
        {
            driver.Navigate().GoToUrl("https://localhost:7210/");

            Wait().Until(ExpectedConditions.ElementExists(By.Id("comicsTable")));

            var rows = driver.FindElements(By.CssSelector("#comicsTable tbody tr"));

            Assert.True(rows.Count > 0);
        }

        //elements that return are different then expected results, storing data through EF1924891 style then expected output
        [Fact]
        public void ComicsTable_ShouldHaveHeaders()
        {
            driver.Navigate().GoToUrl("https://localhost:7210/");

            Wait().Until(d => d.PageSource.Contains("Title"));

            Assert.Contains("Title", driver.PageSource);
            Assert.Contains("Publisher", driver.PageSource);
        }

        [Fact]
        public void Pagination_ShouldChangeEntriesCount()
        {
            driver.Navigate().GoToUrl("https://localhost:7210/");

            var select = new SelectElement(
                Wait().Until(ExpectedConditions.ElementExists(
                    By.Name("comicsTable_length"))));

            select.SelectByText("50");

            Assert.True(driver.FindElements(By.CssSelector("#comicsTable tbody tr")).Count <= 50);
        }
    }
}
