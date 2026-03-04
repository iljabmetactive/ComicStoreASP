using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicUnitTests.UITests
{
    public class GenreFilterUITests
    {
        static IWebDriver driver = new ChromeDriver();

        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10)); //use this when adding to a variable, otherwise it will throw an error about the driver not being initialized

        private WebDriverWait Wait()
        {
            return new WebDriverWait(driver, TimeSpan.FromSeconds(10)); //Use this when Needing to create a new wait instance within a test method,
                                                                        //otherwise it will throw an error about the driver not being initialized
        }

        [Fact]
        public void GenreFilter_ShouldFilterResults()
        {
            driver.Navigate().GoToUrl("https://localhost:7210/");

            var dropdown = Wait().Until(
                ExpectedConditions.ElementExists(By.Id("genreFilter")));

            var select = new SelectElement(dropdown);

            select.SelectByIndex(1);

            System.Threading.Thread.Sleep(1000);

            Assert.NotEmpty(driver.FindElements(By.CssSelector("#comicsTable tbody tr")));
        }

        [Fact]
        public void GenreFilter_ShowAll_ShouldResetTable()
        {
            driver.Navigate().GoToUrl("https://localhost:7210/");

            var select = new SelectElement(driver.FindElement(By.Id("genreFilter")));

            select.SelectByValue("show all");

            Assert.True(driver.FindElements(By.CssSelector("#comicsTable tbody tr")).Count > 0);
        }
    }
}
