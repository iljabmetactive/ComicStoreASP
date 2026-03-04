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
    public class UserUITests
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
        public void LoginPage_ShouldLoad()
        {
            driver.Navigate().GoToUrl("https://localhost:7210/Identity/Account/Login");

            var emailInput = Wait().Until(
                ExpectedConditions.ElementIsVisible(By.Name("Input.Email")));

            Assert.True(emailInput.Displayed);
        }

        [Fact]
        public void Login_WithValidCredentials_ShouldLogin()
        {
            driver.Navigate().GoToUrl("https://localhost:7210/Identity/Account/Login");

            driver.FindElement(By.Name("Input.Email"))
                   .SendKeys("bmc192029862@student.bmet.ac.uk.new");

            driver.FindElement(By.Name("Input.Password"))
                   .SendKeys("654321");

            driver.FindElement(By.Id("login-submit")).Click();

            Wait().Until(d => !d.Url.Contains("Login"));

            Assert.DoesNotContain("Log in", driver.PageSource);
        }

        [Fact]
        public void Login_WithInvalidCredentials_ShouldShowError()
        {
            driver.Navigate().GoToUrl("https://localhost:7210/Identity/Account/Login");

            driver.FindElement(By.Name("Input.Email"))
                   .SendKeys("wrong@test.com");

            driver.FindElement(By.Name("Input.Password"))
                   .SendKeys("WrongPassword");

            driver.FindElement(By.Id("login-submit")).Click();

            Assert.Contains("Invalid login attempt", driver.PageSource);
        }

        
        //the function seems to work just fine, but the Assert result is different,
        //due to the logout button automatically relocating the user to the log-in page.
        [Fact]
        public void Logout_ShouldLogUserOut()
        {
            LoginAsStaff();

            var logoutButton = Wait().Until(
                ExpectedConditions.ElementIsVisible(
                    By.XPath("//button[contains(text(),'Logout')]")));

            logoutButton.Click();

            Wait().Until(d => d.PageSource.Contains("successfully logged out"));

            Assert.Contains("successfully logged out", driver.PageSource);
        }

        [Fact]
        public void Register_AsStaff_ShouldCreateStaffUser()
        {
            driver.Navigate().GoToUrl("https://localhost:7210/Identity/Account/Register");

            var email = $"staff{Guid.NewGuid()}@test.com";

            driver.FindElement(By.Name("Input.Email")).SendKeys(email);
            driver.FindElement(By.Name("Input.Password")).SendKeys("Password123!");
            driver  .FindElement(By.Name("Input.ConfirmPassword")).SendKeys("Password123!");

            driver.FindElement(By.Name("Input.IsStaff")).Click();

            driver.FindElement(By.Id("registerSubmit")).Click();

            Wait().Until(d => !d.Url.Contains("Register"));

            Assert.DoesNotContain("Register", driver.Url);
        }
    }
}
