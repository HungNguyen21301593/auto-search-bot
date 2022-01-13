using auto_webbot.Extensions;
using auto_webbot.Model;
using auto_webbot.Pages;
using auto_webbot.Pages.Delete;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chrome.ChromeDriverExtensions;
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
        private static WebDriverWait WebWaiter => new WebDriverWait(GlobalWebDriver, TimeSpan.FromSeconds(120));
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
<<<<<<< HEAD

=======
                    try
                    {
                        GlobalWebDriver = SetupDriver(userSetting, Config);
                        GlobalWebDriver.Manage().Timeouts().PageLoad = TimeSpan.FromMinutes(Config.AdGlobalSetting.Timeout);
                        var homePage = new HomePage(GlobalWebDriver, Config);
                        LoginAndWait(userSetting, homePage, Config.AdGlobalSetting.LoginRetry);
                        var readAdPage = new ReadAdPage(GlobalWebDriver, Config);
                        adDetails = readAdPage.ReadAds(Config.AdGlobalSetting);
                        if (!adDetails.Any())
                        {
                            Console.WriteLine($"Could not find any Ads from {Config.AdGlobalSetting.Position.From}" +
                                 $" to {Config.AdGlobalSetting.Position.To}");
                            var randomValue = GetRandomScanEvery(Config.AdGlobalSetting.Sleep.ScanEvery);
                            Console.WriteLine($"Wait ScanEvery {randomValue} minutes");
                            NonBlockedSleepInMinutes(randomValue);
                            GlobalWebDriver.Close();
                            continue;
                        }
                        Console.WriteLine("******************************************************");
                        Console.WriteLine($"ReadAd Done, found {adDetails.Count()} Ads: {JsonConvert.SerializeObject(adDetails)}");
                        Console.WriteLine("******************************************************");
                        if (Config.AdGlobalSetting.PauseDuringRun)
                        {
                            Console.WriteLine("PauseDuringRun is activated, press enter to continue");
                            Console.ReadLine();
                        }
                        Console.WriteLine($"Wait DelayAfterAllRead {Config.AdGlobalSetting.Sleep.DelayAfterAllRead} minutes");
                        NonBlockedSleepInMinutes(Config.AdGlobalSetting.Sleep.DelayAfterAllRead);

                        LoginAndWait(userSetting, homePage, Config.AdGlobalSetting.LoginRetry);
                        homePage.DeleteAds(adDetails);
                        Console.WriteLine("******************************************************");
                        Console.WriteLine("DeleteAd Done");
                        Console.WriteLine("******************************************************");
                        if (Config.AdGlobalSetting.PauseDuringRun)
                        {
                            Console.WriteLine("PauseDuringRun is activated, press enter to continue");
                            Console.ReadLine();
                        }
                        Console.WriteLine($"Wait DelayAfterAllDeleted {Config.AdGlobalSetting.Sleep.DelayAfterAllDeleted} minutes");
                        NonBlockedSleepInMinutes(Config.AdGlobalSetting.Sleep.DelayAfterAllDeleted);

                        LoginAndWait(userSetting, homePage, Config.AdGlobalSetting.LoginRetry);
                        homePage.PostAds(adDetails);
                        Console.WriteLine("******************************************************");
                        Console.WriteLine("PostAd Done");
                        Console.WriteLine("******************************************************");
                        if (Config.AdGlobalSetting.PauseDuringRun)
                        {
                            Console.WriteLine("PauseDuringRun is activated, press enter to continue");
                            Console.ReadLine();
                        }
                        Console.WriteLine($"Wait DelayAfterAllPost {Config.AdGlobalSetting.Sleep.DelayAfterAllPost} minutes");
                        NonBlockedSleepInMinutes(Config.AdGlobalSetting.Sleep.DelayAfterAllPost);
                        var scanValue = GetRandomScanEvery(Config.AdGlobalSetting.Sleep.ScanEvery);
                        Console.WriteLine($"Wait ScanEvery {scanValue} minutes");
                        NonBlockedSleepInMinutes(scanValue);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Something went wrong | {e.Message} | {e.StackTrace}");
                        SendErrorEmails(Config, adDetails, e, userSetting);
                        Console.WriteLine($"Sent emails to {string.Join(", ", Config.ErrorEmail.Receivers)}");
                    }
                    finally
                    {
                        GlobalWebDriver.Close();
                    }
>>>>>>> 6ba429a345a00d4dd73c72177e55ad715aacd0fd
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

<<<<<<< HEAD
        private static IWebDriver SetupDriver()
=======
        private static int GetRandomScanEvery(List<ScanEvery> scanEverys)
        {
            var timeUtc = DateTime.UtcNow;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);
            var scanSetting = scanEverys.Where(s => s.DayOfWeek == easternTime.DayOfWeek).FirstOrDefault();
            if (scanSetting is null)
            {
                throw new Exception($"Could not found scan setting for {easternTime.DayOfWeek}");
            }
            return random.Next(scanSetting.RandomFrom, scanSetting.RandomTo);
        }

        private static void SendErrorEmails(AppSetting Config, List<AdDetails> adDetails, Exception e, UserSetting userSetting)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(Config.ErrorEmail.Sender, Config.ErrorEmail.PassForSender),
                EnableSsl = true,
            };
            foreach (var Receiver in Config.ErrorEmail.Receivers)
            {
                smtpClient.Send(Config.ErrorEmail.Sender, Receiver,
                    $"There was some thing wrong with Auto-Kijiji - Account {userSetting.Email}",
                    $"*******ADS:{JsonConvert.SerializeObject(adDetails)}," +
                    Environment.NewLine +
                    Environment.NewLine +
                    $"*******ERROR : {e.Message}," +
                    Environment.NewLine +
                    Environment.NewLine +
                    $"*******STACK: {e.StackTrace}");
            }
        }

        private static void LoginAndWait(UserSetting userSetting, HomePage homePage, int Retry)
        {
            for (int i = 0; i < Retry; i++)
            {
                try
                {
                    Console.WriteLine($"Logging in as {userSetting.Email} - {userSetting.Pass} - try {i}");
                    homePage.Login(userSetting.Email, userSetting.Pass);
                    Console.WriteLine("Wait a while. If there is a capcha, please resolve it manually.");
                    Thread.Sleep(15000);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"There was an error during loging {e.Message} - proceed retry");
                }
            }
        }

        private static IWebDriver SetupDriver(UserSetting userSetting, AppSetting config)
>>>>>>> 6ba429a345a00d4dd73c72177e55ad715aacd0fd
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
                "--disable-logging",
                "--disable-popup-blocking",
                "disable-blink-features=AutomationControlled",
                "--disable-dev-shm-usage",
                "--log-level=3",
                "--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246"
            };
            var options = new ChromeOptions();
            options.AddArguments(chromeArguments);
<<<<<<< HEAD
=======
            if (userSetting.Proxy != null
                && userSetting.Proxy.Host != null
                && userSetting.Proxy.Port != 0
                && userSetting.Proxy.UserName != null
                && userSetting.Proxy.Password != null)
            {
                Console.WriteLine($"Proxy activated: {userSetting.Proxy.Host}:{userSetting.Proxy.Port}:{userSetting.Proxy.UserName}:{userSetting.Proxy.Password}");
                options.AddHttpProxy(userSetting.Proxy.Host, userSetting.Proxy.Port, userSetting.Proxy.UserName, userSetting.Proxy.Password);
            }
>>>>>>> 6ba429a345a00d4dd73c72177e55ad715aacd0fd
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);
            return new ChromeDriver(ChromeDriverService.CreateDefaultService(), options, TimeSpan.FromMinutes(config.AdGlobalSetting.Timeout));
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

        private static void NonBlockedSleepInMinutes(int sleep)
        {
            for (int i = 0; i < sleep; i++)
            {
                Console.WriteLine($"Wait 1 minutes then reload the page to stay signed in | {i + 1}/{sleep}");
                Thread.Sleep(TimeSpan.FromMinutes(1));
                GlobalWebDriver.Navigate().Refresh();
            }
        }
    }
}
