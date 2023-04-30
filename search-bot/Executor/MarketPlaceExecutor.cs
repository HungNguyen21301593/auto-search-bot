using AngleSharp.Common;
using AngleSharp.Dom;
using auto_webbot.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace kijiji_searchbot.Executor
{
    public class MarketPlaceExecutor : ExecutorBase, IExecutor
    {

        public MarketPlaceExecutor(IWebDriver WebDriver, ExecutorSetting AppSetting, string SavedTitlesFilePath) :
            base(WebDriver, AppSetting, SavedTitlesFilePath)
        {
        }

        public async Task Run()
        {
            TryLogin(AppSetting.User, AppSetting.Password);
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
                        LogsMessageBuilder.Clear();
                        LogsMessageBuilder.AppendLine($"------------------------------------------------------------------");
                        LogsMessageBuilder.AppendLine($"{AppSetting.Type}: Proceed scan page {homeUrl} with keywords: {string.Join(", ", AppSetting.Keywords)}");
                        await SendLogMessage(LogsMessageBuilder);
                        await ScanPage(homeUrl);
                        Console.WriteLine($"********************* End page {pageIndex} *********************");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Warning, there was an error {e.Message} {e.GetType()}");
                        await TryHandleErrorUnreachableException(e);
                    }
                    finally
                    {
                        LogsMessageBuilder.Clear();
                        LogsMessageBuilder.AppendLine($"{AppSetting.Type}: Done scan page {homeUrl}");
                        LogsMessageBuilder.AppendLine($"------------------------------------------------------------------");
                        await SendLogMessage(LogsMessageBuilder);
                    }
                }
            }
        }

        private void TryLogin(string user, string password)
        {
            try
            {
                Console.WriteLine($"Proceed loging in with {user}, {password}");
                RandomSleep();
                WebDriver.Navigate().GoToUrl("https://www.facebook.com/");
                RandomSleep();
                WebDriver.FindElements(By.Id("email")).First().SendKeys(user);
                WebDriver.FindElements(By.Id("pass")).First().SendKeys(password);
                WebDriver.FindElements(By.CssSelector("button[type='submit']")).First().Click();
                RandomSleep();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Login already");
            }
        }

        public async Task ScanPage(string homeUrl)
        {
            var urls = GetAllUrlElements(homeUrl);
            var startPosition = GetStartPosition();
            var stopPosition = GetStopPosition(urls.Count);
            LogsMessageBuilder.Clear();
            LogsMessageBuilder.AppendLine($"Found {urls.Count}, scanning from position {startPosition} to position {stopPosition}");
            if (stopPosition == 0 || startPosition >= stopPosition)
            {
                LogsMessageBuilder.AppendLine($"Found no result");
                Console.WriteLine($"Found no result");
                await SendLogMessage(LogsMessageBuilder);
                return;
            }
            await SendLogMessage(LogsMessageBuilder);
            for (var urlIndex = startPosition; urlIndex < stopPosition; urlIndex++)
            {
                try
                {
                    LogsMessageBuilder.Clear();
                    Console.WriteLine($"Page {homeUrl} - Ad {urlIndex}");
                    Console.WriteLine("--------------------------------------------------------------------");
                    LogsMessageBuilder.AppendLine($"Ad {urlIndex}");
                    LogsMessageBuilder.AppendLine("--------------------------------------------------------------------");
                    var item = urls.GetItemByIndex(urlIndex);

                    var resultsFromOutSideIndicators = WebDriver.FindElements(By.XPath("//*[contains(text(), 'Results from outside your search')]"));
                    if (resultsFromOutSideIndicators.Any())
                    {
                        Console.WriteLine($"There are 'Results from outside your search', proceed to check for location, " +
                            $"position results {resultsFromOutSideIndicators.First().Location.Y} - position item {item.Location.Y}");
                        if (resultsFromOutSideIndicators.First().Location.Y < item.Location.Y)
                        {
                            Console.WriteLine("The current ad is on the 'Results from outside your search', so skip");
                            LogsMessageBuilder.AppendLine($"The ad is out side of your search so skip");
                            continue;
                        }
                    }

                    var title = item.GetAttribute("innerText").ToLower();
                    LogsMessageBuilder.AppendLine($"Found title: {title}");
                    Console.WriteLine($"Title: {title}");

                    if (CheckIfAdIsInFile(title))
                    {
                        Console.WriteLine($"title: {title} is already saved, savedTitles");
                        LogsMessageBuilder.AppendLine($"{title} is already saved, so skip");
                        continue;
                    }
                    item.Click();
                    var description = GetDescription().ToLower();
                    Console.WriteLine($"Description: {description}");
                    LogsMessageBuilder.AppendLine($"Found description: {description}");
                    await ProceedSendMessageAsync(description, title);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"There was an error reading ad {urlIndex} with home url {homeUrl}, skipped");
                }
                finally
                {
                    await SendLogMessage(LogsMessageBuilder);
                    RandomSleep();
                    urls = GetAllUrlElements(homeUrl);
                    Console.WriteLine("--------------------------------------------------------------------");
                   
                }
            }
        }

        private string GetDescription()
        {
            RandomSleep();
            var element = WebDriver.FindElements(By.TagName("body"));
            return element?.FirstOrDefault()?.GetAttribute("innerText");
        }

        public IReadOnlyCollection<IWebElement> GetAllUrlElements(string homeUrl)
        {
            try
            {
                Switch(homeUrl);
                RandomSleep();
                new Actions(WebDriver).SendKeys(Keys.Escape).Perform();
                Console.WriteLine("Press Escape to exit temporary block");
                RandomSleep();
                var noResult = WebDriver.FindElements(By.XPath("//*[contains(text(), 'No results found')]"));
                if (noResult.Any())
                {
                    return new List<IWebElement>();
                }
                RandomSleep();
                Switch(homeUrl);
                RandomSleep();
                var urls1 = WebDriver.FindElements(By.CssSelector("a[href*='/marketplace/item']"));
                if (urls1.Any())
                {
                    return urls1;
                }
                Switch(homeUrl);
                RandomSleep();
                var urls2 = WebDriver.FindElements(By.CssSelector("a[href*='/marketplace/item']"));
                if (urls2.Any())
                {
                    return urls2;
                }
            }
            catch (Exception)
            {
                Switch(homeUrl);
                RandomSleep();
                var urls2 = WebDriver.FindElements(By.CssSelector("a[href*='/marketplace/item']"));
                if (urls2.Any())
                {
                    return urls2;
                }
            }
            return new List<IWebElement>();
        }
    }
}
