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
    public class UITests : IDisposable
    {
        private readonly IWebDriver _driver;

        public UITests()
        {
            _driver = new ChromeDriver();
        }
        public void Dispose()
        {
            _driver.Quit();
            _driver.Dispose();
        }
        [Fact]
        public void Create_WhenExecuted_ReturnCreateView()
        {
            _driver.Navigate().GoToUrl("https://localhost:7210/");

            Assert.Equal("- ComicStoreASP", _driver.Title);
            Assert.Contains("Comic Store", _driver.PageSource);
        }

    }
}
