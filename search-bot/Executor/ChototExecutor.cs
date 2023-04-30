using AngleSharp.Common;
using auto_webbot.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace kijiji_searchbot.Executor
{
    public class ChototExecutor : ExecutorBase, IExecutor
    {

        public ChototExecutor(IWebDriver WebDriver, ExecutorSetting AppSetting, string SavedTitlesFilePath) :
            base(WebDriver, AppSetting, SavedTitlesFilePath)
        {
        }

        public async Task Run()
        {
            foreach (var criteriaUrl in AppSetting.BaseUrlSetting.CriteriaUrls)
            {
                for (var pageIndex = AppSetting.StartPage; pageIndex < AppSetting.EndPage; pageIndex++)
                {
                    var allParams = AppSetting.BaseUrlSetting.StaticParams;
                    allParams.TryAdd(AppSetting.BaseUrlSetting.DynamicParams.Page, pageIndex.ToString());
                    var homeUrl = allParams.Apply(criteriaUrl);
                    try
                    {
                        Console.WriteLine($"********************* Start page {pageIndex} *********************");
                        await ScanPage(homeUrl);
                        Console.WriteLine($"********************* End page {pageIndex} *********************");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(
                            $"Warning, there was an error {e.Message}");
                    }
                }
            }
        }

        public async Task ScanPage(string homeUrl)
        {
            var urls = GetAllUrlElements(homeUrl);
            var startPosition = GetStartPosition();
            var stopPosition = GetStopPosition(urls.Count);
            if (stopPosition == 0 || startPosition >= stopPosition)
            {
                return;
            }

            for (var urlIndex = startPosition; urlIndex < stopPosition; urlIndex++)
            {
                try
                {
                    Console.WriteLine($"Page {homeUrl} - Ad {urlIndex}");
                    Console.WriteLine("--------------------------------------------------------------------");
                    var item = urls.GetItemByIndex(urlIndex);
                    RandomSleep();
                    new Actions(WebDriver).MoveToElement(item).DoubleClick().Perform();
                    item.Click();
                    RandomSleep();
                    var title = GetTitle().ToLower();
                    Console.WriteLine($"Title: {title}");

                    if (CheckIfAdIsInFile(title))
                    {
                        Console.WriteLine($"title: {title} is already saved, savedTitles");
                        continue;
                    }

                    var description = GetDescription().ToLower();
                    Console.WriteLine($"Description: {description}");

                    var phone = GetPhone()?.ToLower()??"";
                    Console.WriteLine($"Phone: {phone}");

                    var name = GetName().ToLower();
                    Console.WriteLine($"Name: {name}");

                    await ProceedSendMessageAsync(description, title, phone, name);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"There was an error reading ad {urlIndex} with home url {homeUrl}, skipped, {e.Message}");
                }
                finally
                {
                    RandomSleep();
                    urls = GetAllUrlElements(homeUrl);
                    Console.WriteLine("--------------------------------------------------------------------");
                }
            }
        }

        private string GetPhone()
        {
            RandomSleep();
            var element = WebDriver.FindElements(By.CssSelector("div[class*='ShowPhoneButton_phoneButton']"));
            element?.FirstOrDefault()?.Click();
            RandomSleep();
            return element?.FirstOrDefault()?.GetAttribute("innerText");
        }

        private string GetDescription()
        {
            RandomSleep();
            var element = WebDriver.FindElements(By.CssSelector("div[class*='AdDecriptionVeh_adPrice']"));
            return element?.FirstOrDefault()?.GetAttribute("innerText");
        }

        private string GetTitle()
        {
            RandomSleep();
            var elements = WebDriver.FindElements(By.CssSelector("h1[class*='AdDecriptionVeh_adTitle']"));
            return elements?.FirstOrDefault()?.GetAttribute("innerText");
        }

        private string GetName()
        {
            RandomSleep();
            var elements = WebDriver.FindElements(By.CssSelector("div[class*='SellerProfile_name']"));
            return elements?.FirstOrDefault()?.GetAttribute("innerText");
        }

        public IReadOnlyCollection<IWebElement> GetAllUrlElements(string homeUrl)
        {
            WebDriver.Navigate().Back();
            RandomSleep();
            if (!WebDriver.Url.Equals(homeUrl))
            {
                Switch(homeUrl);
            }
            return WebDriver.FindElements(By.CssSelector("a[class*='AdItem_adItem']"));
        }
    }
}
