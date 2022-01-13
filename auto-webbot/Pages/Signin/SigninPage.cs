using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace auto_webbot.Pages
{
    public class SigninPage
    {
        public IWebDriver webDriver { get; set; }
        public SigninPage(IWebDriver webDriver)
        {
            this.webDriver = webDriver;
        }
        private WebDriverWait WebWaiter => new WebDriverWait(webDriver, TimeSpan.FromSeconds(120));
        private By EmailLocaltor = By.Id("emailOrNickname");
        private By PassLocaltor = By.Id("password");
        private By SubmitLocaltor = By.CssSelector("button[class*='signInButton']");


        public void Login(string email, string pass)
        {
            var emailElement = WebWaiter
                .Until(SeleniumExtras
                    .WaitHelpers
                    .ExpectedConditions
                    .ElementIsVisible(EmailLocaltor));
            emailElement.SendKeys(email);

            var passElement = WebWaiter
                .Until(SeleniumExtras
                    .WaitHelpers
                    .ExpectedConditions
                    .ElementIsVisible(PassLocaltor));
            passElement.SendKeys(pass);

            var submitElement = WebWaiter
                .Until(SeleniumExtras
                    .WaitHelpers
                    .ExpectedConditions
                    .ElementIsVisible(SubmitLocaltor));
            submitElement.Click();
        }
    }
}
