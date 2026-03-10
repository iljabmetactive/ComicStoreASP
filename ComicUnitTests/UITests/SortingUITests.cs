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
    public class SortingUITests
    {
        static IWebDriver driver = new ChromeDriver();

        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10)); //use this when adding to a variable, otherwise it will throw an error about the driver not being initialized

        private WebDriverWait Wait()
        {
            return new WebDriverWait(driver, TimeSpan.FromSeconds(10)); //Use this when Needing to create a new wait instance within a test method,
                                                                        //otherwise it will throw an error about the driver not being initialized
        }

        //need to fix later
        [Fact]
        public void SortButton_ShouldToggleOrder()
        {
            driver.Navigate().GoToUrl("https://localhost:7210/");

            Wait().Until(ExpectedConditions.ElementExists(By.Id("toggleSortOrder")));

            var button = driver.FindElement(By.Id("toggleSortOrder"));

            var before = button.Text;

            button.Click();

            var after = button.Text;

            Assert.NotEqual(before, after);
        }

        [Fact]
        public void SortByTitle_ShouldSortTable()
        {
            driver.Navigate().GoToUrl("https://localhost:7210/");

            Wait().Until(ExpectedConditions.ElementExists(By.Id("comicsTable")));
            var select = new SelectElement(driver.FindElement(By.Id("sortColumn")));
            select.SelectByValue("1");

            var sortButton = driver.FindElement(By.Id("toggleSortOrder"));

            // Scroll button into view
            ((IJavaScriptExecutor)driver)
                .ExecuteScript("arguments[0].scrollIntoView(true);", sortButton);

            sortButton.Click();

            Assert.True(driver.FindElements(By.CssSelector("#comicsTable tbody tr")).Count > 0);
        }
    }
}
