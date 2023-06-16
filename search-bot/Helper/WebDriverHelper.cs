using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager;
using OpenQA.Selenium.DevTools.V85.Runtime;

namespace kijiji_searchbot.Helper
{
    public class WebDriverHelper
    {
        public static IWebDriver SetupDriverInstance(IWebDriver webDriver, bool hardReload = false, bool runOnLocal = false)
        {
            if (!hardReload && IsWebDriverIsAvailable(webDriver))
            {
                Console.WriteLine($"No reload needed, proceed to use existing browser");
                return webDriver;
            }
            webDriver?.Quit();
            Thread.Sleep(10000);
            return runOnLocal ? SetupDriverInstanceForDesktop() : SetupDriverInstanceForDocker();
        }

        public static IWebDriver SetupDriverInstanceForDesktop()
        {
            try
            {
                new DriverManager().SetUpDriver(new ChromeConfig(), "MatchingBrowser");
            }
            catch (Exception)
            {
            }
            var remoteDriver = GeneralChrome();
            return remoteDriver ?? throw new ArgumentNullException($"Could not init web diver for local computer");
        }

        public static IWebDriver SetupDriverInstanceForDocker()
        {
            
            var remoteDriver = GenerateEdge();
            return remoteDriver ?? throw new ArgumentNullException($"Could not init web diver for docker");
        }

        private static IWebDriver GeneralChrome()
        {
            var chromeArguments = GetGeneralChromeSetting();
            var options = new ChromeOptions();
            options.AddAdditionalOption("useAutomationExtension", false);
            options.AddArguments(chromeArguments);
            options.AddUserProfilePreference("profile.managed_default_content_settings.images", 2);
            options.PageLoadStrategy = PageLoadStrategy.Normal;
            Console.WriteLine($"Connecting to webdriver server ");
            var driver = new ChromeDriver(options);
            Console.WriteLine($"Connected to webdriver server ...");
            return driver ?? throw new ArgumentNullException($"Could not init web diver");
        }

        private static IEnumerable<string> GetGeneralChromeSetting()
        {
            var chromeArguments = new List<string> {
                 "--disable-notifications",
                "--start-maximized",
                //"--incognito",
                "--ignore-ssl-errors",
                "--ignore-certificate-errors",
                //"--disable-extensions",
                //"--headless",
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
            options.PageLoadStrategy = PageLoadStrategy.Eager;
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

        public static bool IsWebDriverIsAvailable(IWebDriver webDriver)
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

        public static void KeepDriverAlive(IWebDriver webDriver)
        {
            try
            {
                if (WebDriverHelper.IsWebDriverIsAvailable(webDriver))
                {
                    var url = webDriver?.Url;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to keep Driver Alive");
            }
        }
    }
}
