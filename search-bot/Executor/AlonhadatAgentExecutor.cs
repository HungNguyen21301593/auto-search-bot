using auto_webbot.Model;
using OpenQA.Selenium;
using System;
using System.Linq;
using System.Threading.Tasks;
using kijiji_searchbot.Exceptions;

namespace kijiji_searchbot.Executor
{
    public class AlonhadatAgentExecutor : ListingPageSearchExecutorBase, ISimpleExecutor
    {

        public AlonhadatAgentExecutor(IWebDriver WebDriver, ListingPageSearchExecutorSetting AppSetting, string SavedTitlesFilePath) :
            base(WebDriver, AppSetting, SavedTitlesFilePath)
        {
            this.AppSetting = AppSetting;
        }

        public ListingPageSearchExecutorSetting AppSetting { get; }

        public async Task Run()
        {
            try
            {
                var random = new Random();
                var randomAgentId = random.Next(AppSetting.StartPage, AppSetting.EndPage).ToString();
                var allParams = AppSetting.BaseUrlSetting.StaticParams;
                allParams.Remove(AppSetting.BaseUrlSetting.DynamicParams.Page);
                allParams.TryAdd(AppSetting.BaseUrlSetting.DynamicParams.Page, randomAgentId.ToString());
                var agentUrl = allParams.Apply(AppSetting.BaseUrlSetting.CriteriaUrls.FirstOrDefault() ?? "");
                Console.WriteLine($"********************* random from agent {AppSetting.StartPage} *********************");
                if (CheckIfAdIsInFile(randomAgentId))
                {
                    Console.WriteLine($"Agent Id is exist, so ignore");
                    return;
                }
                
                var result = await ScanAgent(agentUrl);
                if (string.IsNullOrWhiteSpace(result))
                {
                    Console.WriteLine($"Agent Id invalid so ignore");
                    return;
                }
                
                await ProceedSendMessageAsync(description: "", title: result);
                WriteAdToFile(randomAgentId);
                Console.WriteLine($"Agent Id {randomAgentId} is valid, so sent and saved");
            }
            catch (CaptchaException e)
            {
                Console.WriteLine(
                    $"There is captcha error, will try to resolve");
                await TrySolveCaptchaAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    $"Warning, there was an error {e.Message}");
                await TrySolveCaptchaAsync();
            }
            finally
            {
                Console.WriteLine($"********************* random to agent {AppSetting.EndPage} *********************");
            }
        }

        public async Task<string> ScanAgent(string agentUrl)
        {
            await TrySolveCaptchaAsync();
            WebDriver.Navigate().GoToUrl(agentUrl);
            RandomSleep();
            if (WebDriver.Url.Equals("https://alonhadat.com.vn/"))
            {
                return "";
            }
            var elements = WebDriver.FindElements(By.CssSelector("div[class*='agent']"));
            var result = elements.FirstOrDefault()?.GetAttribute("innerText") ?? "";
            return result;
        }
    }
}
