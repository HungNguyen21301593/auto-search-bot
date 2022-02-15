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
    class Program
    {
        private static IWebDriver _globalWebDriver;
        private static AppSetting _globalSetting;
        private static WebDriverWait _globalDriverWait;
        private static readonly Random Random = new Random();
        private static readonly TimeSpan waitTime = TimeSpan.FromMinutes(1);
        

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting...");
            var jsonText = await File.ReadAllTextAsync("AppSetting.json");
            var config = JsonConvert.DeserializeObject<AppSetting>(jsonText);
            Console.WriteLine($"Config: {JsonConvert.SerializeObject(config)}");
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
            _globalDriverWait = new WebDriverWait(_globalWebDriver, TimeSpan.FromMinutes(1));

            while (true)
            {
                for (var pageIndex = config.StartPage; pageIndex < config.EndPage; pageIndex++)
                {
                    await ScanPage(pageIndex);
                }
            }
        }

        private static async Task ScanPage(int pageIndex)
        {
            var urls = GetAllUrlElements(pageIndex);
            var count = urls.Count;
            for (var urlIndex = 0; urlIndex < count; urlIndex++)
            {
                try
                {
                    urls.GetItemByIndex(urlIndex).FindElement(By.TagName("a")).Click();
                    var title = GetTitle().ToLower();
                    var description = GetDescription().ToLower();
                    foreach (var keyword in _globalSetting.Keywords.Select(k => k.ToLower()))
                    {
                        if (title.Contains(keyword))
                        {
                            await SendMessage($"Found keyword {keyword} in title | url {_globalWebDriver.Url}");
                        }

                        if (description.Contains(keyword))
                        {
                            await SendMessage($"Found keyword {keyword} in description | url {_globalWebDriver.Url}");
                        }
                    }
                }
                catch (Exception e)
                {
                    continue;
                }
                finally
                {
                    urls = GetAllUrlElements(pageIndex);
                }
            }
        }

        private static string GetDescription()
        {
            var element =  new WebDriverWait(_globalWebDriver, waitTime)
                .Until(ExpectedConditions.ElementIsVisible((By.CssSelector("div[class*='descriptionContainer']"))));
            return element.GetAttribute("innerText");
        }

        private static string GetTitle()
        {
            var element = new WebDriverWait(_globalWebDriver, waitTime)
                .Until(ExpectedConditions.ElementIsVisible((By.CssSelector("div[class*='realEstateTitle']"))));
            return element.GetAttribute("innerText");
        }

        private static IReadOnlyCollection<IWebElement> GetAllUrlElements(int pageIndex)
        {
            var homeUrl =
                $"https://www.kijiji.ca/b-immobilier/ville-de-montreal/page-{pageIndex}/c34l1700281?ad=offering";
            _globalWebDriver.Navigate().GoToUrl(homeUrl);
            return new WebDriverWait(_globalWebDriver, waitTime)
                .Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy((By.ClassName("info-container"))));
            
        }

        public static async Task SendMessage(string text)
        {
            try
            {
                var bot = new TelegramBotClient("5150406902:AAF73-gIDknNLkrYqpfTlODO-Wz9oh8mxG8");
                foreach (var telegramId in _globalSetting.TelegramIds)
                {
                    await bot.SendTextMessageAsync(telegramId, text);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"err:{e.Message} | {e.StackTrace}");
            }
        }

        private static IWebDriver SetupDriverInstance()
        {
            new DriverManager().SetUpDriver(new ChromeConfig());
            var service = ChromeDriverService.CreateDefaultService();

            service.LogPath = "chromedriver.log";

            service.EnableVerboseLogging = true;

            var chromeArguments = GetGeneralSetting();

            var options = new ChromeOptions();
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);
            options.AddArguments(chromeArguments);
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
                //"--headless",
                "no-sandbox",
                "--disable-gpu",
                //"--disable-logging",
                "--disable-popup-blocking",
                "disable-blink-features=AutomationControlled",
                "--disable-dev-shm-usage",
                "--log-level=0",
                "--disable-application-cache",
                "enable-features=NetworkServiceInProcess",
                "--disable-features=NetworkService"
            };
            return chromeArguments;
        }


        private static void SetConsoleOutput(string prefix)
        {
            var path = $"output\\{prefix}{DateTime.Now:yyyy-MM-dd HH-mm-ss-fff}.txt";
            Console.WriteLine($"Setup OutputPath: {path}");
            Directory.CreateDirectory("output");
            Directory.CreateDirectory("AdPics");
            FileStream outfilestream = new FileStream(path, FileMode.CreateNew);

            Console.OutputEncoding = System.Text.Encoding.Unicode;
            var outstreamwriter = new StreamWriter(outfilestream)
            {
                AutoFlush = true
            };
        }

        private static void NonBlockedSleepInMinutes(int sleep)
        {
            //var minutesToSleep = _globalSetting.AdGlobalSetting.Sleep.SleepInterval;
            //var numberOfSleeps = sleep / minutesToSleep;
            //for (var i = 0; i < numberOfSleeps; i++)
            //{
            //    Console.WriteLine($"Wait {minutesToSleep} minutes then reload the page to stay signed in | {i + 1}/{numberOfSleeps}");
            //    Thread.Sleep(TimeSpan.FromMinutes(minutesToSleep));
            //    //try
            //    //{
            //    //    GlobalWebDriver.Navigate().GoToUrl("https://www.kijiji.ca/?siteLocale=en_CA");
            //    //}
            //    //catch (Exception e)
            //    //{
            //    //    Console.WriteLine($"Warning, webdriver has been disconnected...|{e.Message}");
            //    //}
            //}
        }
    }
}
