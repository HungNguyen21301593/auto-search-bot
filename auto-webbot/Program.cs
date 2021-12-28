using auto_webbot.Extensions;
using auto_webbot.Model;
using auto_webbot.Pages;
using auto_webbot.Pages.Delete;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chrome.ChromeDriverExtensions;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;

namespace AutoBot
{
    class Program
    {
        private static IWebDriver GlobalWebDriver;
        private static WebDriverWait WebWaiter => new WebDriverWait(GlobalWebDriver, TimeSpan.FromSeconds(60));
        private static Random random = new Random();

        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");
            var JsonText = File.ReadAllText("AppSetting.json");
            var Config = JsonConvert.DeserializeObject<AppSetting>(JsonText);
            SetConsoleOutput(Config.OutputFilePrefix);
            while (true)
            {
                var driver = SetupDriver();
                try
                {

                }
                catch (Exception e)
                {
                }
                finally
                {
                    driver.Quit();
                }
                Thread.Sleep(Config.RandomSleep.GetNextValuesInMinutes());
            }
        }

        private static IWebDriver SetupDriver()
        {
            var chromeArguments = new List<string> {
                "--disable-notifications",
                "--start-maximized",
                "--incognito",
                "--ignore-ssl-errors",
                "--ignore-certificate-errors",
                "--disable-extensions",
                //"--headless",
                "no-sandbox",
                "--disable-logging",
                "--disable-popup-blocking",
                "disable-blink-features=AutomationControlled",
                "--log-level=3",
                "--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246"
            };
            var options = new ChromeOptions();
            options.AddArguments(chromeArguments);
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);
            return new ChromeDriver(options);
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
            //Console.SetOut(outstreamwriter);
        }
    }
}
