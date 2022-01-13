using System;
using System.Linq;
using System.Threading;
using auto_webbot.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace auto_webbot.Pages.Post
{
    public class SelectCategoryPage
    {
        public IWebDriver webDriver { get; set; }
        public AppSetting config { get; set; }
        public SelectCategoryPage(IWebDriver webDriver, AppSetting config)
        {
            this.webDriver = webDriver;
            this.config = config;
        }
        private WebDriverWait WebWaiter => new WebDriverWait(webDriver, TimeSpan.FromSeconds(120));
        private By AdTitleLocaltor = By.Id("AdTitleForm");
        private By NextButtonLocaltor = By.XPath("/html/body/div[3]/div[2]/div/div/div/div[2]/div/div/div[2]/div[1]/div/button");


        public void SubmitAdTitle(AdDetails adDetails)
        {
            var inputTitle = WebWaiter
                .Until(SeleniumExtras
                    .WaitHelpers
                    .ExpectedConditions
                    .ElementIsVisible(AdTitleLocaltor));
            inputTitle.SendKeys(adDetails.AdTitle);

            var nextButton = WebWaiter
                .Until(SeleniumExtras
                    .WaitHelpers
                    .ExpectedConditions
                    .ElementIsVisible(NextButtonLocaltor));
            nextButton.Click();
            if (adDetails.Categories.Any())
            {
                SelectCategories(adDetails);
            }
        }

        private void SelectCategories(AdDetails adDetails)
        {
            foreach (var category in adDetails.Categories)
            {
                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                var categoryButtons = WebWaiter
                    .Until(SeleniumExtras
                        .WaitHelpers
                        .ExpectedConditions
                        .VisibilityOfAllElementsLocatedBy(By.CssSelector("span[class*='categoryName']")));
                categoryButtons.Where(b => b.Text == category).FirstOrDefault().Click();
            }
        }
    }
}
