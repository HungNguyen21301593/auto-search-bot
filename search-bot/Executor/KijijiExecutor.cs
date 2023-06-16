using AngleSharp.Common;
using auto_webbot.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace kijiji_searchbot.Executor
{
    public class KijijiExecutor : ListingPageSearchExecutorBase, IExecutor
    {

        public KijijiExecutor(IWebDriver WebDriver, ListingPageSearchExecutorSetting AppSetting, string SavedTitlesFilePath) :
            base(WebDriver, AppSetting, SavedTitlesFilePath)
        {
        }

        public async Task Run()
        {
            foreach (var criteriaUrl in ListingPageSearchExecutorSetting.BaseUrlSetting.CriteriaUrls)
            {
                for (var pageIndex = ListingPageSearchExecutorSetting.StartPage; pageIndex < ListingPageSearchExecutorSetting.EndPage; pageIndex++)
                {
                    var allParams = ListingPageSearchExecutorSetting.BaseUrlSetting.StaticParams;
                    allParams.TryAdd(ListingPageSearchExecutorSetting.BaseUrlSetting.DynamicParams.Page, pageIndex.ToString());
                    var homeUrl = allParams.Apply(criteriaUrl);
                    try
                    {
                        LogsMessageBuilder.Clear();
                        Console.WriteLine($"********************* Start page {pageIndex} *********************");
                        LogsMessageBuilder.AppendLine($"------------------------------------------------------------------");
                        LogsMessageBuilder.AppendLine($"{ListingPageSearchExecutorSetting.Type}: Proceed scan page {homeUrl} with keywords: {string.Join(", ", ListingPageSearchExecutorSetting.Keywords)}");
                        await SendLogMessage(LogsMessageBuilder);
                        await ScanPage(homeUrl);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Warning, there was an error {e.Message} {e.GetType()}");
                        await TryHandleErrorUnreachableException(e);
                    }
                    finally
                    {
                        LogsMessageBuilder.Clear();
                        Console.WriteLine($"********************* End page {pageIndex} *********************");
                        LogsMessageBuilder.AppendLine($"{ListingPageSearchExecutorSetting.Type}: Done scan page {homeUrl}");
                        LogsMessageBuilder.AppendLine($"------------------------------------------------------------------");
                        await SendLogMessage(LogsMessageBuilder);
                    }
                }
            }
        }

        public async Task ScanPage(string homeUrl)
        {
            
            var urls = GetAllUrlElements(homeUrl);
            var startPosition = GetStartPosition();
            var stopPosition = GetStopPosition(urls.Count);
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
                    LogsMessageBuilder.AppendLine($"--------------------------------------------------------------------");
                    LogsMessageBuilder.AppendLine($"Ad {urlIndex}");
                    var titleElement = urls.GetItemByIndex(urlIndex).FindElement(By.ClassName("title"));
                    var title = titleElement.GetAttribute("innerText").ToLower();
                    LogsMessageBuilder.AppendLine($"Found title: {title}");
                    if (CheckIfAdIsInFile(title))
                    {
                        Console.WriteLine($"title: {title} is already saved, savedTitles");
                        LogsMessageBuilder.AppendLine($"{title} is already saved, so skip");
                        continue;
                    }

                    titleElement.Click();
                    RandomSleep();
                    Console.WriteLine($"Title: {title}");
                    LogsMessageBuilder.AppendLine($"Ad URL: {WebDriver.Url}");

                    var description = GetDescription().ToLower();
                    Console.WriteLine($"Description: {description}");
                    //LogsMessageBuilder.AppendLine($"Found description: {description}");

                    await ProceedSendMessageAsync(description, title);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"There was an error reading ad {urlIndex} with home url {homeUrl}, skipped, e:{e.Message}");
                }
                finally
                {
                    RandomSleep();
                    urls = GetAllUrlElements(homeUrl);
                    Console.WriteLine("--------------------------------------------------------------------");
                    await SendLogMessage(LogsMessageBuilder);
                }
            }
        }

        private string GetDescription()
        {
            RandomSleep();
            var element = WebDriver.FindElements(By.CssSelector("div[class*='descriptionContainer']"));
            return element?.FirstOrDefault().GetAttribute("innerText");
        }

        private string GetTitle()
        {
            RandomSleep();
            var elements = WebDriver.FindElements(By.CssSelector("div[class*='mainColumn-']"));
            return elements?.FirstOrDefault().GetAttribute("innerText");
        }

        public IReadOnlyCollection<IWebElement> GetAllUrlElements(string homeUrl)
        {
            Switch(homeUrl);
            RandomSleep();
            var elements = WebDriver.FindElements(By.ClassName("info-container"));
            new Actions(WebDriver).MoveToElement(elements.Last()).Perform();
            return WebDriver.FindElements(By.ClassName("info-container"));
        }
    }
}
