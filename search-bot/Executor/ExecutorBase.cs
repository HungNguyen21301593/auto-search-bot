using AngleSharp.Common;
using AngleSharp.Dom;
using auto_webbot.Model;
using CaptchaSharp.Models;
using CaptchaSharp.Services;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace kijiji_searchbot.Executor
{
    public abstract class ExecutorBase
    {
        public IWebDriver WebDriver { get; }
        public ExecutorSetting AppSetting { get; }
        public string SavedTitlesFilePath { get; }
        public StringBuilder LogsMessageBuilder { get; set; }
        public Random RandomSleepEngine { get; set; }

        public ExecutorBase(IWebDriver WebDriver, ExecutorSetting AppSetting, string SavedTitlesFilePath)
        {
            this.WebDriver = WebDriver ?? throw new ArgumentNullException(nameof(WebDriver));
            this.AppSetting = AppSetting ?? throw new ArgumentNullException(nameof(AppSetting));
            this.SavedTitlesFilePath = SavedTitlesFilePath ?? throw new ArgumentNullException(nameof(SavedTitlesFilePath));
            LogsMessageBuilder = new StringBuilder();
            RandomSleepEngine = new Random();
        }

        public void RandomSleep()
        {
            var from = int.Parse(Environment.GetEnvironmentVariable("SLEEP_INTERVAL_FROM") ?? "5");
            var to = int.Parse(Environment.GetEnvironmentVariable("SLEEP_INTERVAL_TO") ?? "7");
            var sleep = RandomSleepEngine.Next(from, to);
            Console.WriteLine($"Sleep {sleep}");
            Thread.Sleep(TimeSpan.FromSeconds(sleep));
        }


        public async Task ProceedSendMessageAsync(string description, string title, string phone = "", string name = "", string urlToscan = "")
        {
            var webUrl = WebDriver.Url;
            var message = GenerateMessage(description, title, phone, name, webUrl);
            var lowerDescription = description.ToLower().Trim();
            var lowerTitle = title.ToLower().Trim();
            var shouldIgnored = AppSetting.ExcludeKeywords.Select(k => k.ToLower()).Any(lowkey => lowerDescription.Contains(lowkey) || lowerTitle.Contains(lowkey));
            if (shouldIgnored)
            {
                WriteAdToFile(title);
                LogsMessageBuilder.AppendLine("Found an exculude keywords in title or description, skip");
                return;
            }
            if (!AppSetting.Keywords.Any())
            {
                WriteAdToFile(title);
                var phoneText = string.IsNullOrWhiteSpace(phone) ? $" phone:{phone}":"";
                await SendMessage(message);
                LogsMessageBuilder.AppendLine("There is no keyword define, proceed send notification");
            }
            
            var listMatcheKeywords = new List<string>();
            foreach (var lowkeyword in AppSetting.Keywords.Select(k => k.ToLower()))
            {
                if (!lowerDescription.Contains(lowkeyword) && !lowerTitle.Contains(lowkeyword))
                {
                    Console.WriteLine($"There is no keyword \"{lowkeyword}\" on title or description");
                    LogsMessageBuilder.AppendLine($"There is no keyword \"{lowkeyword}\" on title or description");
                    continue;
                }

                if (lowerTitle.Contains(lowkeyword) || lowerDescription.Contains(lowkeyword))
                {
                    WriteAdToFile(title);
                   
                    
                    LogsMessageBuilder.AppendLine("Found ad matched, add to send list");
                    listMatcheKeywords.Add(lowkeyword);
                    continue;
                }
            }

            if (listMatcheKeywords.Any())
            {
                var foundMessage = $"{string.Join(", ", listMatcheKeywords.Select(s=>s.ToUpper()))} in:" +
                       $"{Environment.NewLine}>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>" +
                       $"{Environment.NewLine}{message}";
                await SendMessage(foundMessage);
            }
            WriteAdToFile(title);
        }

        public async Task SendMessage(string text)
        {
            try
            {
                var bot = new TelegramBotClient("5150406902:AAF73-gIDknNLkrYqpfTlODO-Wz9oh8mxG8");
                foreach (var telegramId in AppSetting.TelegramIds)
                {
                    await bot.SendTextMessageAsync(telegramId, text);
                    Console.WriteLine($"Sent success");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"err:{e.Message} | {e.StackTrace}");
            }
        }

        public async Task SendLogMessage(StringBuilder logsMessageBuilder)
        {
            try
            {
                var text = logsMessageBuilder.ToString();
                var truncateText = text.Substring(0, Math.Min(4000, text.Length - 1));
                var bot = new TelegramBotClient("5150406902:AAF73-gIDknNLkrYqpfTlODO-Wz9oh8mxG8");
                foreach (var telegramId in AppSetting.DumbTelegramIds)
                {
                    await bot.SendTextMessageAsync(telegramId, truncateText);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"err:{e.Message} | {e.StackTrace}");
            }
        }

        public void WriteAdToFile(string title)
        {
            using StreamWriter writer = File.AppendText(SavedTitlesFilePath);
            var textToSave = title.Replace(Environment.NewLine, " ");
            writer.WriteLine(textToSave.ToLower());
        }

        public void CleanUpHistory()
        {
            File.WriteAllText(SavedTitlesFilePath, string.Empty);
        }

        public bool CheckIfAdIsInFile(string title)
        {
            var textTocheck = title.Replace(Environment.NewLine, " ");
            var lines = File.ReadAllLines(SavedTitlesFilePath);
            return lines.Select(l => l.ToLower()).Any(l => l.Equals(textTocheck.ToLower()));
        }

        public void Switch(string url)
        {
            if(WebDriver.Url.Equals(url))
            {
                WebDriver.Navigate().Refresh();
                return;
            }
            WebDriver.Navigate().GoToUrl(url);
        }

        private string GenerateMessage(string description = "", string title ="", string phone = "", string name = "", string webUrl = "")
        {
            var stringBuilder = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(name))
            {
                stringBuilder.AppendLine($"Name: {name}");
                stringBuilder.AppendLine("------------------------------------------------------------------------");
            }
            if (!string.IsNullOrWhiteSpace(phone))
            {
                stringBuilder.AppendLine($"Phone: {phone}");
                stringBuilder.AppendLine("------------------------------------------------------------------------");
            }
            if (!string.IsNullOrWhiteSpace(title))
            {
                stringBuilder.AppendLine($"Title: {title}");
                stringBuilder.AppendLine("------------------------------------------------------------------------");
            }
            if (!string.IsNullOrWhiteSpace(description))
            {
                stringBuilder.AppendLine($"Description: {description}");
                stringBuilder.AppendLine("------------------------------------------------------------------------");
            }

            stringBuilder.AppendLine($"URL: {webUrl} ");
            return stringBuilder.ToString();
        }

        public int GetStartPosition()
        {
            return AppSetting.MinAdsPositionOnEachPage;
        }

        public int GetStopPosition(int urlsCount)
        {
            return Math.Min(urlsCount, AppSetting.MinAdsPositionOnEachPage + AppSetting.MaximumAdsOnEachPage);
        }

        public async Task TrySolveCaptchaAsync()
        {
            try
            {
                if (!WebDriver.Url.Contains("https://alonhadat.com.vn/xac-thuc-nguoi-dung.html"))
                {
                    Console.WriteLine("The current web site is not asking for captcha");
                    return;
                }
                var service = new TwoCaptchaService("afb3fd8b2549f4062e1b8d2971a9e84d");
                var mysiteKey = "6LdZc40jAAAAAD5UwNdx-ZxMNQgYLpM3V1sotiUL";
                var mysiteUrl = "https://alonhadat.com.vn/xac-thuc-nguoi-dung.html?url=/default.aspx";
                WebDriver.Navigate().GoToUrl(mysiteUrl);
                StringResponse solution = await service.SolveRecaptchaV2Async(siteKey: mysiteKey,
                    siteUrl:
                    mysiteUrl, false);
                (WebDriver as IJavaScriptExecutor).ExecuteScript($"document.getElementById('g-recaptcha-response').innerHTML = '{solution.Response}';");
                Console.WriteLine($"Got captcha token: {JsonConvert.SerializeObject(solution)}");
                RandomSleep();
                WebDriver.FindElements(By.CssSelector("button[type='submit']")).FirstOrDefault()?.Click();
                RandomSleep();
            }
            catch (Exception)
            {
                Console.WriteLine("There was an error while trying to resolve the captcha, ignored and will retry later");
            }
        }

        public Task TryHandleErrorUnreachableException(Exception inputException)
        {
            try
            {
                Console.WriteLine($"Try to handle error unreachable Exception, {inputException}");
                WebDriver.Navigate().GoToUrl("chrome://net-internals/#dns");
                WebDriver.FindElements(By.CssSelector("button[value='Clear host cache']"))
                    .FirstOrDefault()?.Click();
                Console.WriteLine("Cleaned host cache, proceed to quit and reload");
                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                Console.WriteLine($"There was an error to handle the UnreachableException, {e.Message}");
                throw;
            }
        }
    }
}
