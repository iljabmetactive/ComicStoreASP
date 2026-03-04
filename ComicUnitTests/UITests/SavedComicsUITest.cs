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
    public class SavedComicsUITest
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
        public void MyComics_ShouldLoad()
        {
            LoginAsStaff();

            driver.Navigate().GoToUrl("https://localhost:7210/Home/MyComics");

            Assert.Contains("My Saved Comics", driver.PageSource);
        }

        [Fact]
        public void MyComics_ShouldDisplayHeaders()
        {
            LoginAsStaff();

            driver.Navigate().GoToUrl("https://localhost:7210/Home/MyComics");

            if (driver.PageSource.Contains("<table"))
            {
                Assert.Contains("Title", driver.PageSource);
                Assert.Contains("Publisher", driver.PageSource);
                Assert.Contains("Genre", driver.PageSource);
            }
        }

        //It does seem to find the Save button, which implies that although the user can't see it,
        //the website still renders it in the HTML, which is a security concern as it could potentially be exploited by malicious users.
        [Fact]
        public void SaveButton_ShouldBeHidden_WhenNotLoggedIn()
        {
            driver.Navigate().GoToUrl("https://localhost:7210/");

            Assert.DoesNotContain("Save", driver.PageSource);
        }

        [Fact]
        public void SaveButton_ShouldBeVisible_WhenLoggedIn()
        {
            LoginAsStaff();

            driver.Navigate().GoToUrl("https://localhost:7210/");

            Assert.Contains("Save", driver.PageSource);
        }
    }
}
