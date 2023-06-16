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
using Scheduling;

namespace auto_webbot
{
    internal class Program
    {
        private static IWebDriver _globalWebDriver;
        private static string savedTitlesFilePath = "SavedTitles.txt";
        private static object lockObject= new object();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting...");
            int.TryParse(Environment.GetEnvironmentVariable("WARM_UP_SLEEP"), out var warmupSleep);
            Console.WriteLine($"Wait {warmupSleep} seconds");
            Thread.Sleep(TimeSpan.FromSeconds(warmupSleep));
            Console.WriteLine("Do you like to clean up saved ads? y/n");
            var result = Console.ReadLine();
            var jsonText = await File.ReadAllTextAsync("AppSetting.json");
            var configs = JsonConvert.DeserializeObject<List<dynamic>>(jsonText,
                new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                });
            var myProcess = System.Diagnostics.Process.GetCurrentProcess();
            myProcess.PriorityClass = System.Diagnostics.ProcessPriorityClass.High;

            var executors = new List<IExecutor>();
            if (result == "y" || result == "Y")
            {
                executors.ForEach(e => e.CleanUpHistory());
            }
            var success = int.TryParse(Environment.GetEnvironmentVariable("SEARCH_INTERVAL"), out var searchInterval);
            var scheduleInMinutes = success ? searchInterval : 5;
            
            var action = async (dynamic config) => 
            {
                try
                {
                    var randomSleep = new Random().Next(4, 10);
                    var randomBetweenExecutor = new Random().Next(10, 30);
                    Console.WriteLine($"config {JsonConvert.SerializeObject(config)}");
                    if (config.Type == ExecutorType.ExecutorTypeKijiji)
                    {
                        var setting = JsonConvert.DeserializeObject<ListingPageSearchExecutorSetting>(JsonConvert.SerializeObject(config));
                        _globalWebDriver = WebDriverHelper.SetupDriverInstance(_globalWebDriver, hardReload: true, runOnLocal: false);
                        var kijijiExecutor = new KijijiExecutor(_globalWebDriver, setting, savedTitlesFilePath);
                        await kijijiExecutor.Run();
                    }
                    if (config.Type == ExecutorType.ExecutorTypeGuadagnoscommesse)
                    {
                        var guadagnoscommesseAppSetting = JsonConvert.DeserializeObject<GuadagnoscommesseAppSetting>(JsonConvert.SerializeObject(config));
                        _globalWebDriver = WebDriverHelper.SetupDriverInstance(_globalWebDriver, hardReload: true);
                        var executor = new GuadagnoscommesseExecutor(_globalWebDriver, guadagnoscommesseAppSetting, savedTitlesFilePath);
                        await executor.Run();
                    }
                    if (config.Type == ExecutorType.ExecutorTypeMarketPlace)
                    {
                        _globalWebDriver = WebDriverHelper.SetupDriverInstance(_globalWebDriver, hardReload: true);
                        var marketplaceExecutor = new MarketPlaceExecutor(_globalWebDriver, config, savedTitlesFilePath);
                        await marketplaceExecutor.Run();
                        Thread.Sleep(TimeSpan.FromSeconds(randomBetweenExecutor));
                    }
                    if (config.Type == ExecutorType.ExecutorChotot)
                    {
                        _globalWebDriver = WebDriverHelper.SetupDriverInstance(_globalWebDriver, hardReload: true);
                        var chototExecutor = new ChototExecutor(_globalWebDriver, config, savedTitlesFilePath);
                        await chototExecutor.Run();
                    }
                    if (config.Type == ExecutorType.ExecutorAlonhadat)
                    {
                        _globalWebDriver = WebDriverHelper.SetupDriverInstance(_globalWebDriver, hardReload: true);
                        var alonhadatExecutor = new AlonhadatExecutor(_globalWebDriver, config, savedTitlesFilePath);
                        await alonhadatExecutor.Run();
                    }
                    if (config.Type == ExecutorType.ExecutorAlonhadatAgent)
                    {
                        _globalWebDriver = WebDriverHelper.SetupDriverInstance(_globalWebDriver, hardReload: true);
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
                lock (lockObject)
                {
                    Console.WriteLine($"Schedule {scheduleInMinutes} minutes");
                    configs.ForEachAsync(async (config) =>
                    await action(config)
                    ).Wait();
                }
            });
            Schedule.Every(5).Seconds().Run(() => 
            {
                WebDriverHelper.KeepDriverAlive(_globalWebDriver);
            });

            Schedule.Every(2).Days().Run(() =>
            {
                File.WriteAllText(savedTitlesFilePath, string.Empty);
            });
        }
    }
}
