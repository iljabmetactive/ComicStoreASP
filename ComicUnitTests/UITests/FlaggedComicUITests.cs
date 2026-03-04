using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicUnitTests.UITests
{
    public class FlaggedComicUITests
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
        public void FlaggedComicsPage_ShouldLoad()
        {
            LoginAsStaff();

            driver.Navigate().GoToUrl("https://localhost:7210/Home/FlaggedComics");

            Wait().Until(d => d.PageSource.Contains("Flagged Comics"));

            Assert.Contains("Flagged Comics", driver.PageSource);
        }

        [Fact]
        public void FlaggedComics_ShouldShowTableHeaders()
        {
            LoginAsStaff();

            driver.Navigate().GoToUrl("https://localhost:7210/Home/FlaggedComics");

            if (driver.PageSource.Contains("<table"))
            {
                Assert.Contains("Title", driver.PageSource);
                Assert.Contains("Publisher", driver .PageSource);
                Assert.Contains("Reason", driver.PageSource);
                Assert.Contains("Flagged At", driver.PageSource);
            }
        }

        [Fact]
        public void FlagButton_ShouldBeHidden_ForNonStaff()
        {
            driver.Navigate().GoToUrl("https://localhost:7210/");

            Assert.DoesNotContain("Flag", driver.PageSource);
        }

        [Fact]
        public void FlagButton_ShouldBeVisible_ForStaff()
        {
            LoginAsStaff();

            driver.Navigate().GoToUrl("https://localhost:7210/");

            Assert.Contains("Flag", driver.PageSource);
        }
    }
}
