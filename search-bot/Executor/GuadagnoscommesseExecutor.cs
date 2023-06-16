using AngleSharp.Dom;
using auto_webbot.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot;

namespace kijiji_searchbot.Executor
{
    public class GuadagnoscommesseExecutor : ExecutorBase
    {

        public GuadagnoscommesseExecutor(IWebDriver WebDriver, GuadagnoscommesseAppSetting GuadagnoscommesseAppSetting, string SavedTitlesFilePath) :
            base(WebDriver, GuadagnoscommesseAppSetting, SavedTitlesFilePath)
        {
            this.GuadagnoscommesseAppSetting = GuadagnoscommesseAppSetting;
            this.Bot = new TelegramBotClient("6271390630:AAHwEQlwr7shs9tNr3t5otHSXZ99wMC3vuk");
        }

        public GuadagnoscommesseAppSetting GuadagnoscommesseAppSetting { get; }
        public TelegramBotClient Bot { get; }

        public async Task Run()
        {
            WebDriver.Navigate().GoToUrl("https://guadagnoscommesse.com");
            await TryLogin();
            RandomSleep();
            var savedFilters = await GetAllSavedFilters(GuadagnoscommesseAppSetting.Prefix);
            Console.WriteLine(string.Join(";", savedFilters));

            foreach (var filter in savedFilters)
            {
                Console.WriteLine(filter);
                var results = await ScanSavedFilterWithTitle(filter);
                await Sendnotification(filter, results);
            }
        }

        private async Task Sendnotification(string filter, List<string> results)
        {
            foreach (var result in results.Select(r=> $"{filter}{Environment.NewLine}{r}"))
            {
                if (!CheckIfAdIsInFile(result) && !result.Contains("Nessun dato presente nella tabella"))
                {
                    await SendMessage($"Found a new result in:{Environment.NewLine}{result}", Bot);
                }
                await SendLogMessage(new StringBuilder($"Dummy result:{Environment.NewLine}{result}"), Bot);
                WriteAdToFile(result);
            }
        }

        private Task TryLogin()
        {
            WebDriver.Navigate().GoToUrl("https://guadagnoscommesse.com/membri/login");
            var userNames = WebDriver.FindElements(By.Id("amember-login"));
            if (!userNames.Any())
            {
                return Task.CompletedTask;
            }
            userNames.First().SendKeys(GuadagnoscommesseAppSetting.User);

            var passes = WebDriver.FindElements(By.Id("amember-pass"));
            if (!passes.Any())
            {
                return Task.CompletedTask;
            }
            passes.First().SendKeys(GuadagnoscommesseAppSetting.Password);

            var submits = WebDriver.FindElements(By.CssSelector("input[type='submit']"));
            if (!submits.Any())
            {
                return Task.CompletedTask;
            }
            submits.First().Click();
            return Task.CompletedTask;
        }

        private Task<List<string>> GetAllSavedFilters(string prefix)
        {
            WebDriver.Navigate().GoToUrl("https://guadagnoscommesse.com/membri/prodotti/sbancobet/ricerca-globale/");
            var savedSearchButtons = WebDriver.FindElements(By.Id("scegli_btn"));
            if (!savedSearchButtons.Any())
            {
                return Task.FromResult(new List<string>());
            }
            savedSearchButtons.First().Click();
            RandomSleep();
            var savedSearchs = WebDriver.FindElements(By.Id("nome_salv"));
            var result = new List<string>();
            foreach (var savedSearch in savedSearchs)
            {
                var title = savedSearch.GetAttribute("innerText");
                if (!title.ToUpper().Contains(prefix.ToUpper()))
                {
                    continue;
                }
                result.Add(title);
            }
            return Task.FromResult(result);
        }

        private Task<List<string>> ScanSavedFilterWithTitle(string title)
        {
            WebDriver.Navigate().GoToUrl("https://guadagnoscommesse.com/membri/prodotti/sbancobet/ricerca-globale/");
            var savedSearchButtons = WebDriver.FindElements(By.Id("scegli_btn"));
            if (!savedSearchButtons.Any())
            {
                return Task.FromResult(new List<string>());
            }
            savedSearchButtons.First().Click();
            RandomSleep();
            var filterXpath = $"//span[translate(.,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')='{title.ToLower()}']";
            var filters = WebDriver.FindElements(By.XPath(filterXpath));
            if (!filters.Any())
            {
                return Task.FromResult(new List<string>());
            }
            filters.First().Click();
            RandomSleep();
            var submitSearchs = WebDriver.FindElements(By.Id("submit_form"));
            if (!submitSearchs.Any())
            {
                return Task.FromResult(new List<string>());
            }
            new Actions(WebDriver).Click(submitSearchs.First()).Perform();
            submitSearchs.First().Click();
            RandomSleep();

            var rows = WebDriver.FindElements(By.XPath("//*[@id=\"example\"]/tbody/tr"));

            if (!rows.Any())
            {
                return Task.FromResult(new List<string>());
            }
            var texts = rows.Select(row =>
            {
                var formattedRow = row.GetAttribute("innerText")
                    .Replace(Environment.NewLine, "-")
                    .Replace("\t", Environment.NewLine);
                var urlElements = row.FindElements(By.TagName("a"));
                var url = urlElements.Any() ? urlElements.First().GetAttribute("href") : string.Empty;
                return $"{formattedRow}{Environment.NewLine}Link: {url}";
            }).ToList();
            Console.WriteLine(string.Join(";", texts));
            return Task.FromResult(texts);
        }
    }
}
