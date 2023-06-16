using AngleSharp.Common;
using auto_webbot.Model;
using CaptchaSharp.Services;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using kijiji_searchbot.Exceptions;
using CaptchaSharp.Models;
using Newtonsoft.Json;

namespace kijiji_searchbot.Executor
{
    public class AlonhadatExecutor : ListingPageSearchExecutorBase, IExecutor
    {

        public AlonhadatExecutor(IWebDriver WebDriver, ListingPageSearchExecutorSetting AppSetting, string SavedTitlesFilePath) :
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
                    allParams.Remove(ListingPageSearchExecutorSetting.BaseUrlSetting.DynamicParams.Page);
                    allParams.TryAdd(ListingPageSearchExecutorSetting.BaseUrlSetting.DynamicParams.Page, pageIndex.ToString());
                    var homeUrl = allParams.Apply(criteriaUrl);
                    try
                    {
                        Console.WriteLine($"********************* Start page {pageIndex} *********************");
                        await ScanPage(homeUrl);
                        Console.WriteLine($"********************* End page {pageIndex} *********************");
                    }
                    catch (CaptchaException e)
                    {
                        Console.WriteLine(
                            $"There is captcha error, will try to resolve");
                        await TrySolveCaptchaAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(
                            $"Warning, there was an error {e.Message}");
                        await TrySolveCaptchaAsync();
                    }
                }
            }
        }

        public async Task ScanPage(string homeUrl)
        {
            await TrySolveCaptchaAsync();
            var urls = GetAllUrlElements(homeUrl);
            if (urls.Count == 0) {
                throw new CaptchaException();
            }
            var startPosition = GetStartPosition();
            var stopPosition = GetStopPosition(urls.Count);
            Console.WriteLine($"stop position: {stopPosition}, start position: {startPosition}");
            if (stopPosition == 0 || startPosition >= stopPosition)
            {
                Console.WriteLine($"stop and start position are wrong so skip");
                return;
            }
            for (var urlIndex = startPosition; urlIndex < stopPosition; urlIndex++)
            {
                try
                {
                    Console.WriteLine($"Page {homeUrl} - Ad {urlIndex}");
                    Console.WriteLine("--------------------------------------------------------------------");
                    var item = urls.GetItemByIndex(urlIndex);
                    var title = GetTitle(item).ToLower();
                    RandomSleep();
                    if (CheckIfAdIsInFile(title))
                    {
                        Console.WriteLine($"title: {title} is already saved");
                        continue;
                    }
                    Console.WriteLine($"Title: {title}");
                    item.FindElements(By.TagName("a"))?.FirstOrDefault()?.Click();

                    var description = GetDescription().ToLower();
                    Console.WriteLine($"Description: {description}");
                    RandomSleep();
                    var phone = GetPhone().ToLower();
                    Console.WriteLine($"Phone: {phone}");
                    RandomSleep();
                    var name = GetName().ToLower();
                    Console.WriteLine($"Name: {name}");
                    await ProceedSendMessageAsync(description, title, phone, name);
                    WriteAdToFile(title);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"There was an error reading ad {urlIndex} with home url {homeUrl}, skipped, {e.Message}");
                    await TrySolveCaptchaAsync();
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
            var element = WebDriver.FindElements(By.CssSelector("div[class='fone']"));
            return element?.FirstOrDefault()?.GetAttribute("innerText") ?? "";
        }

        private string GetDescription()
        {
            var descriptionElement = WebDriver.FindElements(By.CssSelector("div[class*='detail']"));
            var description = descriptionElement?.FirstOrDefault()?.GetAttribute("innerText") ?? "";
            var addressElement = WebDriver.FindElements(By.CssSelector("div[class='address']"));
            var address = addressElement?.FirstOrDefault()?.GetAttribute("innerText") ?? "";
            return $"{description}{Environment.NewLine} {address}";
        }

        private string GetTitle(IWebElement item)
        {
            var elements = item.FindElements(By.CssSelector("div[class='ct_title']"));
            return elements?.FirstOrDefault()?.GetAttribute("innerText") ?? "";
        }

        private string GetName()
        {
            var elements = WebDriver.FindElements(By.CssSelector("div[class='name']"));
            return elements?.FirstOrDefault()?.GetAttribute("innerText") ?? "";
        }

        public IReadOnlyCollection<IWebElement> GetAllUrlElements(string homeUrl)
        {
            if (!WebDriver.Url.Equals(homeUrl))
            {
                WebDriver.Navigate().GoToUrl("https://alonhadat.com.vn/");
                RandomSleep();
                Switch(homeUrl);
                RandomSleep();
            }
            return WebDriver.FindElements(By.CssSelector("div[class='content-item']"));
        }

    }
}
