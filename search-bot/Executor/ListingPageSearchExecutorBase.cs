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
    public abstract class ListingPageSearchExecutorBase: ExecutorBase
    {
        public ListingPageSearchExecutorSetting ListingPageSearchExecutorSetting { get; }

        public ListingPageSearchExecutorBase(IWebDriver WebDriver, ListingPageSearchExecutorSetting AppSetting, string SavedTitlesFilePath)
            : base(WebDriver, AppSetting, SavedTitlesFilePath)
        {
            LogsMessageBuilder = new StringBuilder();
            RandomSleepEngine = new Random();
            this.ListingPageSearchExecutorSetting = AppSetting;
        }

        public async Task ProceedSendMessageAsync(string description, string title, string phone = "", string name = "", string urlToscan = "")
        {
            var webUrl = WebDriver.Url;
            var message = GenerateMessage(description, title, phone, name, webUrl);
            var lowerDescription = description.ToLower().Trim();
            var lowerTitle = title.ToLower().Trim();

            var containAllMustHaveKeywords = ListingPageSearchExecutorSetting.MustHaveKeywords
                .Select(k => k.ToLower())
                .All(musthaveKeyword => lowerDescription.Contains(musthaveKeyword) || lowerTitle.Contains(musthaveKeyword));
            if (!containAllMustHaveKeywords)
            {
                WriteAdToFile(title);
                LogsMessageBuilder.AppendLine($"Does not contain all must have keywords: {string.Join(",", ListingPageSearchExecutorSetting.MustHaveKeywords)}, skip");
                return;
            }
            var shouldIgnored = ListingPageSearchExecutorSetting.ExcludeKeywords
                .Select(k => k.ToLower())
                .Any(lowkey => lowerDescription.Contains(lowkey) || lowerTitle.Contains(lowkey));
            if (shouldIgnored)
            {
                WriteAdToFile(title);
                LogsMessageBuilder.AppendLine("Found an exculude keywords in title or description, skip");
                return;
            }
            if (!ListingPageSearchExecutorSetting.Keywords.Any())
            {
                WriteAdToFile(title);
                var phoneText = string.IsNullOrWhiteSpace(phone) ? $" phone:{phone}":"";
                await SendMessage(message);
                LogsMessageBuilder.AppendLine("There is no keyword define, proceed send notification");
            }
            
            var listMatcheKeywords = new List<string>();
            foreach (var lowkeyword in ListingPageSearchExecutorSetting.Keywords.Select(k => k.ToLower()))
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
            return ListingPageSearchExecutorSetting.MinAdsPositionOnEachPage;
        }

        public int GetStopPosition(int urlsCount)
        {
            return Math.Min(urlsCount, ListingPageSearchExecutorSetting.MinAdsPositionOnEachPage + ListingPageSearchExecutorSetting.MaximumAdsOnEachPage);
        }
    }
}
