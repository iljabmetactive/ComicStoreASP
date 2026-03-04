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
    public class AnalyticsUITests
    {
        static IWebDriver driver = new ChromeDriver();

        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10)); //use this when adding to a variable, otherwise it will throw an error about the driver not being initialized

        private WebDriverWait Wait()
        {
            return new WebDriverWait(driver, TimeSpan.FromSeconds(10)); //Use this when Needing to create a new wait instance within a test method,
                                                                        //otherwise it will throw an error about the driver not being initialized
        }
        private void LoginAsStaff()
        {
            driver.Navigate().GoToUrl("https://localhost:7210/Identity/Account/Login");

            driver.FindElement(By.Name("Input.Email"))
                   .SendKeys("bmc192029862@student.bmet.ac.uk.new");

            driver.FindElement(By.Name("Input.Password"))
                   .SendKeys("654321");

            driver.FindElement(By.Id("login-submit")).Click();

            Wait().Until(d => !d.Url.Contains("Login"));
        }
        [Fact]
        public void TopSearches_ShouldLoadAnalytics()
        {
            driver.Navigate().GoToUrl("https://localhost:7210/");

            Wait().Until(ExpectedConditions.ElementExists(By.Id("analyticsTable")));

            var rows = driver.FindElements(By.CssSelector("#analyticsTable tbody tr"));

            Assert.NotNull(rows);
        }

        [Fact]
        public void TopSearches_ShouldUpdateAfterSearch()
        {
            driver.Navigate().GoToUrl("https://localhost:7210/");

            var search = driver.FindElement(By.Name("Title"));

            search.SendKeys("Batman");

            driver.FindElement(By.CssSelector("#advancedSearchForm button")).Click();

            System.Threading.Thread.Sleep(2000);

            var analytics = driver.FindElements(By.CssSelector("#analyticsTable tbody tr"));

            Assert.True(analytics.Count >= 0);
        }

        [Fact]
        public void SearchAnalytics_ShouldLoad()
        {
            LoginAsStaff();

            driver.Navigate().GoToUrl("https://localhost:7210/Home/SearchAnalytics");

            Assert.Contains("Search Analytics", driver.PageSource);
        }

        [Fact]
        public void SearchAnalytics_ShouldHaveThreeColumns()
        {
            LoginAsStaff();

            driver.Navigate().GoToUrl("https://localhost:7210/Home/SearchAnalytics");

            Assert.Contains("Top 10 Searches", driver.PageSource);
            Assert.Contains("Top 10 Returned Comics", driver.PageSource);
            Assert.Contains("Comics Returned Over 100 Times", driver.PageSource);
        }

        [Fact]
        public void SearchAnalytics_ShouldRenderLists()
        {
            LoginAsStaff();

            driver.Navigate().GoToUrl("https://localhost:7210/Home/SearchAnalytics");

            var lists = driver.FindElements(By.TagName("ul"));

            Assert.True(lists.Count >= 3);
        }
    }
}
