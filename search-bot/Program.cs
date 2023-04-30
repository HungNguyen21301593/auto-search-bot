using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using auto_webbot.Model;
using kijiji_searchbot.Executor;
using kijiji_searchbot.Helper;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using Scheduling;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace auto_webbot
{
    internal class Program
    {
        private static IWebDriver _globalWebDriver;
        private static string savedTitlesFilePath = "SavedTitles.txt";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting...");
            Thread.Sleep(10000);
            Console.WriteLine("Do you like to clean up saved ads? y/n");
            var result = Console.ReadLine();
            var jsonText = await File.ReadAllTextAsync("AppSetting.json");
            var configs = JsonConvert.DeserializeObject<List<ExecutorSetting>>(jsonText,
                new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Error,
                });
            var myProcess = System.Diagnostics.Process.GetCurrentProcess();
            myProcess.PriorityClass = System.Diagnostics.ProcessPriorityClass.High;

            var executors = new List<IExecutor>();


            if (result == "y" || result == "Y")
            {
                executors.ForEach(e => e.CleanUpHistory());
            }
            var success = int.TryParse(Environment.GetEnvironmentVariable("SEARCH_INTERVAL"), out var searchInterval);
            var scheduleInMinutes = success ? searchInterval : 2;
            
            var action = async (ExecutorSetting config) => 
            {
                try
                {
                    var randomSleep = new Random().Next(4, 10);
                    var randomBetweenExecutor = new Random().Next(10, 30);
                    Console.WriteLine($"config {JsonConvert.SerializeObject(config)}");
                    if (config.Type == ExecutorType.ExecutorTypeKijiji)
                    {
                        _globalWebDriver = SetupDriverInstance(hardReload: true);
                        var kijijiExecutor = new KijijiExecutor(_globalWebDriver, config, savedTitlesFilePath);
                        await kijijiExecutor.Run();
                    }
                    if (config.Type == ExecutorType.ExecutorTypeMarketPlace)
                    {
                        _globalWebDriver = SetupDriverInstance(hardReload: true);
                        var marketplaceExecutor = new MarketPlaceExecutor(_globalWebDriver, config, savedTitlesFilePath);
                        await marketplaceExecutor.Run();

                        Thread.Sleep(TimeSpan.FromSeconds(randomBetweenExecutor));
                    }
                    if (config.Type == ExecutorType.ExecutorChotot)
                    {
                        _globalWebDriver = SetupDriverInstance(hardReload: true);
                        var chototExecutor = new ChototExecutor(_globalWebDriver, config, savedTitlesFilePath);
                        await chototExecutor.Run();
                    }
                    if (config.Type == ExecutorType.ExecutorAlonhadat)
                    {
                        _globalWebDriver = SetupDriverInstance(hardReload: true);
                        var alonhadatExecutor = new AlonhadatExecutor(_globalWebDriver, config, savedTitlesFilePath);
                        await alonhadatExecutor.Run();
                    }
                    if (config.Type == ExecutorType.ExecutorAlonhadatAgent)
                    {
                        _globalWebDriver = SetupDriverInstance(hardReload: true);
                        var alonhadatAgentExecutor = new AlonhadatAgentExecutor(_globalWebDriver, config, savedTitlesFilePath);
                        await alonhadatAgentExecutor.Run();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Global excaption, There is an error on executor {e}");
                    _globalWebDriver?.Quit();
                }
            };
            
            Schedule.Every(scheduleInMinutes).Minutes().Run(async () =>
            {
                Console.WriteLine($"Schedule {scheduleInMinutes} minutes");
                await configs.ForEachAsync(async (config) => 
                await action(config)
                );
            });
            Schedule.Every(5).Seconds().Run(() => 
            {
                KeepDriverAlive();
            });
        }

        private static void KeepDriverAlive()
        {
            try
            {
                if (IsWebDriverIsAvailable(_globalWebDriver))
                {
                    var url = _globalWebDriver?.Url;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to keep Driver Alive");
            }
        }

        private static IWebDriver SetupDriverInstance(bool hardReload = false)
        {
            if (!hardReload && IsWebDriverIsAvailable(_globalWebDriver))
            {
                Console.WriteLine($"No reload needed, proceed to use existing browser");
                return _globalWebDriver;
            }
            _globalWebDriver?.Quit();
            Thread.Sleep(10000);
            try
            {
                new DriverManager().SetUpDriver(new ChromeConfig(), "101.0.4951.41");
            }
            catch (Exception)
            {
            }
            var remoteDriver = GenerateEdge();
            return remoteDriver ?? throw new ArgumentNullException($"Could not init web diver");
        }

        private static IWebDriver GeneralChrome()
        {
            var chromeArguments = GetGeneralChromeSetting();
            var options = new ChromeOptions();
            options.AddAdditionalOption("useAutomationExtension", false);
            options.AddArguments(chromeArguments);
            options.AddUserProfilePreference("profile.managed_default_content_settings.images", 2);
            options.PageLoadStrategy = PageLoadStrategy.Normal;
            var settings = new RemoteSessionSettings(options);
            var remoteDriverUrl = Environment.GetEnvironmentVariable("REMOTE_DRIVER_URL") ?? "http://localhost:4446/wd/hub";
            Console.WriteLine($"Connecting to webdriver server {remoteDriverUrl}");
            var remoteDriver = new RemoteWebDriver(new Uri(remoteDriverUrl ?? string.Empty), settings);
            Console.WriteLine($"Connected to webdriver server {remoteDriverUrl}...");
            return remoteDriver ?? throw new ArgumentNullException($"Could not init web diver");
        }

        private static IEnumerable<string> GetGeneralChromeSetting()
        {
            var chromeArguments = new List<string> {
                //"--disable-extensions",
                //"--headless",
                //"--incognito",
                //"--disable-notifications",
                "--start-maximized",
                "--ignore-ssl-errors",
                "--ignore-certificate-errors",
                "--no-sandbox",
                "--disable-gpu",
                "--disable-logging",
                "--disable-popup-blocking",
                "disable-blink-features=AutomationControlled",
                "--disable-dev-shm-usage",
                "--log-level=3",
                //"enable-features=NetworkServiceInProcess",
                "--disable-features=NetworkService",
                //"user-agent=Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.125 Safari/537.36"
                //"--blink-settings=imagesEnabled=false",
            };
            return chromeArguments;
        }

        private static IWebDriver GeneralFireFox()
        {
            var chromeArguments = GetGeneralFireFoxSetting();
            var options = new FirefoxOptions();
            //options.AddExcludedArgument("enable-automation");
            //options.AddAdditionalOption("useAutomationExtension", false);
            options.AddArguments(chromeArguments);
            //options.AddUserProfilePreference("profile.managed_default_content_settings.images", 2);
            options.PageLoadStrategy = PageLoadStrategy.Normal;
            var settings = new RemoteSessionSettings(options);
            var remoteDriverUrl = Environment.GetEnvironmentVariable("REMOTE_DRIVER_URL") ?? "http://localhost:4446/wd/hub";
            Console.WriteLine($"Connecting to webdriver server {remoteDriverUrl}");
            var remoteDriver = new RemoteWebDriver(new Uri(remoteDriverUrl ?? string.Empty), settings);
            Console.WriteLine($"Connected to webdriver server {remoteDriverUrl}...");
            remoteDriver.Manage().Window.Maximize();
            return remoteDriver ?? throw new ArgumentNullException($"Could not init web diver");
        }

        private static IEnumerable<string> GetGeneralFireFoxSetting()
        {
            var chromeArguments = new List<string> {
                //"--disable-extensions",
                //"--headless",
                //"--incognito",
                //"--disable-notifications",
                "--start-maximized",
                "--ignore-ssl-errors",
                "--ignore-certificate-errors",
                "--no-sandbox",
                "--disable-gpu",
                "--disable-logging",
                "--disable-popup-blocking",
                "disable-blink-features=AutomationControlled",
                "--disable-dev-shm-usage",
                "--log-level=3",
                //"enable-features=NetworkServiceInProcess",
                "--disable-features=NetworkService",
                //"user-agent=Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.125 Safari/537.36"
                //"--blink-settings=imagesEnabled=false",
            };
            return chromeArguments;
        }

        private static IWebDriver GenerateEdge()
        {
            var chromeArguments = GetGeneralEdgeSetting();
            var options = new EdgeOptions();
            options.AddExcludedArgument("enable-automation");
            options.AddArguments(chromeArguments);
            options.AddUserProfilePreference("profile.managed_default_content_settings.images", 2);
            options.PageLoadStrategy = PageLoadStrategy.Normal;
            var settings = new RemoteSessionSettings(options);
            var remoteDriverUrl = Environment.GetEnvironmentVariable("REMOTE_DRIVER_URL") ?? "http://localhost:4447/wd/hub";
            Console.WriteLine($"Connecting to webdriver server {remoteDriverUrl}");
            var remoteDriver = new RemoteWebDriver(new Uri(remoteDriverUrl ?? string.Empty), settings);
            Console.WriteLine($"Connected to webdriver server {remoteDriverUrl}...");
            remoteDriver.Manage().Window.Maximize();
            return remoteDriver ?? throw new ArgumentNullException($"Could not init web diver");
        }

        private static IEnumerable<string> GetGeneralEdgeSetting()
        {
            var chromeArguments = new List<string> {
                //"--disable-extensions",
                //"--headless",
                //"--incognito",
                //"enable-features=NetworkServiceInProcess",
                //"user-agent=Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.125 Safari/537.36"
                //"--blink-settings=imagesEnabled=false",

                "--disable-notifications",
                "--start-maximized",
                "--ignore-ssl-errors",
                "--ignore-certificate-errors",
                "--no-sandbox",
                "--disable-gpu",
                "--disable-logging",
                "--disable-popup-blocking",
                "disable-blink-features=AutomationControlled",
                "--disable-dev-shm-usage",
                "--log-level=3",
                "--disable-features=NetworkService",
            };
            return chromeArguments;
        }

        private static bool IsWebDriverIsAvailable(IWebDriver webDriver)
        {
            try
            {
                var url = webDriver.Url;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
