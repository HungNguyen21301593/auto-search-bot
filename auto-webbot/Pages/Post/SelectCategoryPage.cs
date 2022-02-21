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
        private By NextButtonLocaltor = By.XPath("//*[text()='Next']");


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
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            var normalCategoryContainer = webDriver.FindElements(By.CssSelector("div[class*='allCategoriesContainer']"));
            if (!normalCategoryContainer.Any())
            {
                throw new Exception("Could not find normalCategoryContainer");
            }
            foreach (var category in adDetails.Categories)
            {
                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                var categoryButtons =
                    normalCategoryContainer.First().FindElements(By.CssSelector("span[class*='categoryName']"));
                if (!categoryButtons.Any())
                {
                    throw new Exception("Could not find categoryButtons");
                }

                var categoryText = category;
                if (category.StartsWith("Houses for Sale"))
                {
                    categoryText = "Houses for Sale";
                }
                categoryButtons.Where(b => b.Text == categoryText).FirstOrDefault().Click();
            }

            if (config.AdGlobalSetting.Locations == null) return;
            foreach (var location in config.AdGlobalSetting.Locations)
            {
                ClickLocation(location);
            }
            ClickGo();
        }

        private void ClickLocation(string location)
        {
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            var element = webDriver.FindElements(By.XPath($"//*[text()='{location}']"));
            if (!element.Any())
            {
                Console.WriteLine($"Warning, could not find AdGlobalSetting-location {location}");
                return;
            }
            element.First().Click();
        }

        private void ClickGo()
        {
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            var element = webDriver.FindElements(By.Id("LocUpdate"));
            if (!element.Any())
            {
                Console.WriteLine("Warning, could not found Go button");
                return;
            }
            element.First().Click();
        }
    }
}
