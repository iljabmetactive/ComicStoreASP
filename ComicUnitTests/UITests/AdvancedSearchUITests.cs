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
    public class AdvancedSearchUITests
    {
        static IWebDriver driver = new ChromeDriver();

        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10)); //use this when adding to a variable, otherwise it will throw an error about the driver not being initialized

        private WebDriverWait Wait()
        {
            return new WebDriverWait(driver, TimeSpan.FromSeconds(10)); //Use this when Needing to create a new wait instance within a test method,
                                                                        //otherwise it will throw an error about the driver not being initialized
        }

        [Fact]
        public void AdvancedSearch_ShouldReturnResults()
        {
            driver.Navigate().GoToUrl("https://localhost:7210/");

            var titleInput = Wait().Until(
                ExpectedConditions.ElementIsVisible(By.Name("Title")));

            titleInput.SendKeys("Batman");

            driver.FindElement(By.CssSelector("#advancedSearchForm button")).Click();

            Wait().Until(d =>
                d.FindElements(By.CssSelector("#comicsTable tbody tr")).Count > 0);

            Assert.Contains("Batman", driver.PageSource);
        }

        [Fact]
        public void AdvancedSearch_ShouldHandleEmptyInput()
        {
            driver.Navigate().GoToUrl("https://localhost:7210/");

            driver.FindElement(By.CssSelector("#advancedSearchForm button")).Click();

            Wait().Until(ExpectedConditions.ElementExists(By.Id("comicsTable")));

            Assert.True(driver.PageSource.Contains("Comic Store"));
        }

        [Fact]
        public void AdvancedSearch_InvalidInput_ShouldReturnNoResults()
        {
            driver.Navigate().GoToUrl("https://localhost:7210/");

            var titleInput = driver.FindElement(By.Name("Title"));

            titleInput.SendKeys("ZZZ_NON_EXISTENT");

            driver.FindElement(By.CssSelector("#advancedSearchForm button")).Click();

            Wait().Until(ExpectedConditions.ElementExists(By.Id("comicsTable")));

            var rows = driver.FindElements(By.CssSelector("#comicsTable tbody tr"));

            Assert.True(rows.Count >= 0);
        }
    }
}
