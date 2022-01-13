using auto_webbot.Model;
using auto_webbot.Pages.Delete;
using auto_webbot.Pages.Post;
using auto_webbot.Pages.Post.auto_webbot.Pages.Post;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace auto_webbot.Pages
{
    public class HomePage
    {
        public IWebDriver webDriver { get; set; }
        public AppSetting config { get; set; }
        public HomePage(IWebDriver webDriver, AppSetting config)
        {
            this.webDriver = webDriver;
            this.config = config;
        }
        public WebDriverWait WebWaiter => new WebDriverWait(webDriver, TimeSpan.FromSeconds(120));
        public By SigninLocator = By.LinkText("Sign In");
        public By PostAdButtonLocator = By.CssSelector("a[class*='headerButtonPostAd']");
        public By avatarLocator = By.CssSelector("a[class*='avatar']");


        public void Login(string email, string pass)
        {
            webDriver.Navigate().GoToUrl("https://www.kijiji.ca/?siteLocale=en_CA");

            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            var signinExist = webDriver.FindElements(SigninLocator);
            if (!signinExist.Any())
            {
                Console.WriteLine("Logged in already so skip");
                return;
            }
            var Signin = WebWaiter
            .Until(SeleniumExtras
                .WaitHelpers
                .ExpectedConditions
                .ElementIsVisible(SigninLocator));
            Signin.Click();
            var signinPage = new SigninPage(webDriver);
            signinPage.Login(email, pass);
        }

        public void PostAds(List<AdDetails> adDetails)
        {
            Random random = new Random();
            var exceptionMessages = new List<string>();
            foreach (var adDetail in adDetails)
            {
                try
                {
                    webDriver.Navigate().GoToUrl("https://www.kijiji.ca/?siteLocale=en_CA");
                    ClickPost();
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    SubmitAdTitle(adDetail);
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    InputAdDetails(adDetail);
                    var randomTime = random.Next(config.AdGlobalSetting.Sleep.DelayAfterEachPost.From,
                        config.AdGlobalSetting.Sleep.DelayAfterEachPost.To);
                    Console.WriteLine($"Wait DelayAfterEachPost {randomTime} minutes");
                    Thread.Sleep(TimeSpan.FromMinutes(randomTime));
                }
                catch (Exception e)
                {
                    exceptionMessages.Add($"Error postting Ad, error {e.Message}, object: {JsonConvert.SerializeObject(adDetail)}");
                }
            }
            if (exceptionMessages.Any())
            {
                throw new Exception(JsonConvert.SerializeObject(exceptionMessages));
            }
        }

        public void DeleteAds(List<AdDetails> adDetails)
        {
            var exceptionMessages = new List<string>();
            foreach (var adDetail in adDetails)
            {
                try
                {
                    webDriver.Navigate().GoToUrl("https://www.kijiji.ca/m-my-ads/active");
                    var deletePage = new DeletePage(webDriver, config);
                    deletePage.DeleteAd(adDetail);
                    Thread.Sleep(config.AdGlobalSetting.Sleep.DelayBetweenEachDelete);
                }
                catch (Exception e)
                {
                    exceptionMessages.Add($"Error deleting Ad, error {e.Message}, object: {JsonConvert.SerializeObject(adDetail)}");
                }
            }
            if (exceptionMessages.Any())
            {
                throw new Exception(JsonConvert.SerializeObject(exceptionMessages));
            }
        }

        private void ClickPost()
        {
            var postAd = WebWaiter
                            .Until(SeleniumExtras
                                .WaitHelpers
                                .ExpectedConditions
                                .ElementToBeClickable(PostAdButtonLocator));
            postAd.Click();
        }

        private void InputAdDetails(AdDetails adDetails)
        {
            var adDetailInputPage = new AdDetailInputPage(webDriver,config);
            adDetailInputPage.InputAdDetails(adDetails);
        }

        private void SubmitAdTitle(AdDetails adDetails)
        {
            var adTitlePage = new SelectCategoryPage(webDriver, config);
            adTitlePage.SubmitAdTitle(adDetails);
        }
    }
}
