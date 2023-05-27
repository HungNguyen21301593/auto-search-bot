using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Common;
using auto_webbot.Model;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Telegram.Bot;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace auto_webbot
{
    internal class Program
    {
        private static IWebDriver _globalWebDriver;
        private static AppSetting _globalSetting;
        private static string savedTitlesFilePath = "SavedTitles.txt";
        private static int sleepTime = 10000;

        static async Task Main(string[] args)
        {
            var expriredDate = new DateTime(2024, 02, 07);
            if (DateTime.Now > expriredDate)
            {
                Console.WriteLine("App is exprired. Please contact developers, hungnguyen21301593@gmail.com");
                Console.ReadLine();
                return;
            }
            Console.WriteLine("Starting...");
            Console.WriteLine("Do you like to clean up saved ads? y/n");
            var result = Console.ReadLine();
            if (result == "y" || result == "Y")
            {
                CleanAllAdOnFile();
            }

            var jsonText = await File.ReadAllTextAsync("AppSetting.json");
            var config = JsonConvert.DeserializeObject<AppSetting>(jsonText);
            _globalSetting = config;
            Console.CancelKeyPress += delegate
            {
                if (_globalWebDriver == null) return;
                _globalWebDriver.Quit();
                Environment.Exit(0);
            };
            var myProcess = System.Diagnostics.Process.GetCurrentProcess();
            myProcess.PriorityClass = System.Diagnostics.ProcessPriorityClass.High;
            _globalWebDriver = SetupDriverInstance();

            while (true)
            {
                if (DateTime.Now > expriredDate)
                {
                    Console.WriteLine("App is exprired. Please contact developers, hungnguyen21301593@gmail.com");
                    Console.ReadLine();
                    return;
                }
                foreach (var criteriaUrl in _globalSetting.BaseUrlSetting.CriteriaUrls)
                {
                    for (var pageIndex = config.StartPage; pageIndex < config.EndPage; pageIndex++)
                    {
                        var allParams = _globalSetting.BaseUrlSetting.StaticParams;
                        allParams.TryAdd(_globalSetting.BaseUrlSetting.DynamicParams.Page, pageIndex.ToString());
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
                                $"Warning, there was an error {e.Message}, proceed to close current driver and reconnect");
                            _globalWebDriver.Quit();
                            _globalWebDriver = SetupDriverInstance();
                        }
                    }
                }
            }
        }

        private static async Task ScanPage(string homeUrl)
        {
            var urls = GetAllUrlElements(homeUrl);
            var count = Math.Min(urls.Count, 15);
            for (var urlIndex = 0; urlIndex < count; urlIndex++)
            {
                try
                {
                    Console.WriteLine($"Page {homeUrl} - Ad {urlIndex + 1}");
                    Console.WriteLine("--------------------------------------------------------------------");
                    urls.GetItemByIndex(urlIndex).FindElement(By.ClassName("title")).Click();
                    var title = GetTitle().ToLower();
                    Console.WriteLine($"Title: {title}");

                    if (CheckIfAdIsInFile(title))
                    {
                        Console.WriteLine($"title: {title} is already saved, savedTitles");
                        continue;
                    }

                    var description = GetDescription().ToLower();
                    Console.WriteLine($"Description: {description}");
                    await ProceedSendMessageAsync(description, title);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"There was an error reading ad {urlIndex} with home url {homeUrl}, skipped");
                }
                finally
                {
                    Thread.Sleep(sleepTime);
                    urls = GetAllUrlElements(homeUrl);
                    Console.WriteLine("--------------------------------------------------------------------");
                }
            }
        }

        private static async Task ProceedSendMessageAsync(string description, string title)
        {
            if (!_globalSetting.Keywords.Any())
            {
                WriteAdToFile(title);
                await SendMessage($"Found an ad with title: {title} | url {_globalWebDriver.Url}");
            }
            foreach (var keyword in _globalSetting.Keywords.Select(k => k.ToLower()))
            {
                if (!description.Contains(keyword) && !title.Contains(keyword))
                {
                    continue;
                }

                if (title.Contains(keyword))
                {
                    WriteAdToFile(title);
                    await SendMessage($"Found keyword {keyword} in title | url {_globalWebDriver.Url}");
                    continue;
                }

                if (description.Contains(keyword))
                {
                    WriteAdToFile(title);
                    await SendMessage($"Found keyword {keyword} in description | url {_globalWebDriver.Url}");
                    continue;
                }
            }
        }

        private static string GetDescription()
        {
            Thread.Sleep(sleepTime);
            var element = _globalWebDriver.FindElements(By.CssSelector("div[class*='descriptionContainer']"));
            return element?.FirstOrDefault().GetAttribute("innerText");
        }

        private static string GetTitle()
        {
            Thread.Sleep(sleepTime);
            var elements = _globalWebDriver.FindElements(By.CssSelector("div[class*='mainColumn-']"));
            return elements?.FirstOrDefault().GetAttribute("innerText");
        }

        private static IReadOnlyCollection<IWebElement> GetAllUrlElements(string homeUrl)
        {
            Thread.Sleep(sleepTime);
            _globalWebDriver.Navigate().GoToUrl(homeUrl);
            return _globalWebDriver.FindElements(By.ClassName("info-container"));
        }

        public static async Task SendMessage(string text)
        {
            try
            {
                var bot = new TelegramBotClient("6152200916:AAGfCn5mBnDhQ6qDEy-X8sKZF6rbnGjDPwk");
                foreach (var telegramId in _globalSetting.TelegramIds)
                {
                    await bot.SendTextMessageAsync(telegramId, text);
                    Console.WriteLine($"Sent: {text}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"err:{e.Message} | {e.StackTrace}");
            }
        }

        private static IWebDriver SetupDriverInstance()
        {
            try
            {
                new DriverManager().SetUpDriver(new ChromeConfig(), "MatchingBrowser");
            }
            catch (Exception)
            {
            }
            var service = ChromeDriverService.CreateDefaultService();
            service.LogPath = "chromedriver.log";

            service.EnableVerboseLogging = true;

            var chromeArguments = GetGeneralSetting();

            var options = new ChromeOptions();
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);
            options.AddArguments(chromeArguments);
            options.AddUserProfilePreference("profile.managed_default_content_settings.images", 2 );
            options.PageLoadStrategy = PageLoadStrategy.Eager;
            return new ChromeDriver(service, options);
        }


        private static IEnumerable<string> GetGeneralSetting()
        {
            var chromeArguments = new List<string> {
                "--disable-notifications",
                "--start-maximized",
                //"--incognito",
                "--ignore-ssl-errors",
                "--ignore-certificate-errors",
                //"--disable-extensions",
                "--headless",
                "--no-sandbox",
                "--disable-gpu",
                "--disable-logging",
                "--disable-popup-blocking",
                "disable-blink-features=AutomationControlled",
                "--disable-dev-shm-usage",
                "--log-level=3",
                "--disable-application-cache",
                "enable-features=NetworkServiceInProcess",
                "--disable-features=NetworkService",
                "--blink-settings=imagesEnabled=false"
            };
            return chromeArguments;
        }

        private static void WriteAdToFile(string title)
        {
            using StreamWriter writer = File.AppendText(savedTitlesFilePath);
            writer.WriteLine(title);
        }

        private static void CleanAllAdOnFile()
        {
            File.WriteAllText(savedTitlesFilePath, string.Empty);
        }


        private static bool CheckIfAdIsInFile(string title)
        {
            string readText = File.ReadAllText(savedTitlesFilePath);
            return readText.Contains(title);
        }
    }
}
