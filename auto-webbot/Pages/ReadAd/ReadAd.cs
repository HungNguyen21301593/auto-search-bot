using auto_webbot.Model;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace auto_webbot.Pages.Delete
{
    public class ReadAdPage
    {
        public IWebDriver webDriver { get; set; }
        private AppSetting config { get; set; }
        private WebDriverWait WebWaiter => new WebDriverWait(webDriver, TimeSpan.FromSeconds(120));
        public ReadAdPage(IWebDriver webDriver, AppSetting config)
        {
            this.webDriver = webDriver;
            this.config = config;
        }

        private By ImageTitleLocator = By.CssSelector("div[class*='imageTitleContainer']");
        private By EditAdLocator = By.PartialLinkText("Edit Ad");
        private By categoriesLocators1 = By.CssSelector("span[class*='breadcrumb-']");
        private By adtitleLocator = By.Id("postad-title");
        private By desLocator = By.Id("pstad-descrptn");
        private By tagsLocators = By.CssSelector("li[class*='tagItem']");
        private By addressLocator = By.Id("servicesLocationInput");
        private By locationLocator = By.CssSelector("div[class*='locationText-']");
        private By imageLocators = By.CssSelector("div[class='image']");
        private By companyLocator = By.Id("company_s");
        private By typeLocator = By.Id("type_s");

        private By carYearLocator = By.Id("caryear_i");
        private By carKmLocator = By.Id("carmileageinkms_i");

        private List<By> dynamicLabelsLocators = new List<By> {
            By.CssSelector("div[data-qa-id='active-listings-stat-line']"),
            By.CssSelector("span[class*='noLabelValue']"),
            By.CssSelector("dd[class*='attributeValue']"),
            By.CssSelector("div[class*='line-'")
        };

        // optional
        private By priceLocators = By.Id("PriceAmount");


        public List<AdDetails> ReadAds(AdGlobalSetting adGlocalSeting)
        {
            var ListAdDeatails = new List<AdDetails>();
            try
            {
                LoadReadPage(out ReadOnlyCollection<IWebElement> ImageTitles);
                for (int i = 0; i < ImageTitles.Count(); i++)
                {
                    var position = i + 1;
                    if (position < adGlocalSeting.Position.From || position > adGlocalSeting.Position.To)
                    {
                        continue;
                    }
                    ImageTitles[i].Click();
                    ListAdDeatails.Add(ReadSingleAd());
                    Thread.Sleep(adGlocalSeting.Sleep.DelayBetweenEachRead);
                    LoadReadPage(out ImageTitles);
                }
                return ListAdDeatails;
            }
            catch (Exception e)
            {
                throw new Exception($"Error readding Ad, error {e.Message}, result: {JsonConvert.SerializeObject(ListAdDeatails)}");
            }
        }

        private void LoadReadPage( out ReadOnlyCollection<IWebElement> ImageTitles)
        {
            webDriver.Navigate().GoToUrl("https://www.kijiji.ca/m-my-ads/active");
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            ImageTitles = webDriver.FindElements(ImageTitleLocator);
        }

        private AdDetails ReadSingleAd()
        {
            var adDetails = new AdDetails();
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            ReadDynamicsTexts(adDetails);
            Console.WriteLine($"ReadDynamicsTexts {JsonConvert.SerializeObject(adDetails)}");

            var editAd = WebWaiter
            .Until(SeleniumExtras
                .WaitHelpers
                .ExpectedConditions
                .ElementToBeClickable(EditAdLocator));
            editAd.Click();
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            ReadCategories(adDetails);
            Console.WriteLine($"ReadCategories {JsonConvert.SerializeObject(adDetails)}");
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            ReadAdTitle(adDetails);
            Console.WriteLine($"ReadAdTitle {JsonConvert.SerializeObject(adDetails)}");
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            ReadAdDescription(adDetails);
            Console.WriteLine($"ReadAdDescription {JsonConvert.SerializeObject(adDetails)}");
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            ReadTags(adDetails);
            Console.WriteLine($"ReadTags {JsonConvert.SerializeObject(adDetails)}");
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            ReadAddress(adDetails);
            Console.WriteLine($"ReadAddress {JsonConvert.SerializeObject(adDetails)}");
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            ReadLocation(adDetails);
            Console.WriteLine($"ReadLocation {JsonConvert.SerializeObject(adDetails)}");
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            DownloadPics(adDetails);
            Console.WriteLine($"DownloadPics {JsonConvert.SerializeObject(adDetails)}");
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            ReadPrice(adDetails);
            Console.WriteLine($"ReadPrice {JsonConvert.SerializeObject(adDetails)}");
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            ReadCompany(adDetails);
            Console.WriteLine($"ReadCompany {JsonConvert.SerializeObject(adDetails)}");
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            ReadTypes(adDetails);
            Console.WriteLine($"ReadTypes {JsonConvert.SerializeObject(adDetails)}");
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            ReadCarYear(adDetails);
            Console.WriteLine($"ReadCarYear {JsonConvert.SerializeObject(adDetails)}");
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            ReadCarKm(adDetails);
            Console.WriteLine($"ReadCarKm {JsonConvert.SerializeObject(adDetails)}");
            return adDetails;
        }

        private void ReadDynamicsTexts(AdDetails adDetails)
        {
            foreach (var locator in dynamicLabelsLocators)
            {
                var elements = webDriver.FindElements(locator);
                if (elements.Any())
                {
                    foreach (var element in elements)
                    {
                        var textValue = element.Text;
                        adDetails.DynamicTextOptions.Add(textValue.Trim());
                    }
                }
            }
        }

        private void ReadTypes(AdDetails adDetails)
        {
            var types = webDriver.FindElements(typeLocator);
            if (types.Any())
            {
                adDetails.Type = types.First().GetAttribute("value");
            }
        }

        private void ReadCarYear(AdDetails adDetails)
        {
            var items = webDriver.FindElements(carYearLocator);
            if (items.Any())
            {
                adDetails.CarYear = items.First().GetAttribute("value");
            }
        }

        private void ReadCarKm(AdDetails adDetails)
        {
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            var items = webDriver.FindElements(carKmLocator);
            if (items.Any())
            {
                adDetails.CarKm = items.First().GetAttribute("value");
            }
        }

        private void ReadCompany(AdDetails adDetails)
        {
            var companys = webDriver.FindElements(companyLocator);
            if (companys.Any())
            {
                adDetails.Company = companys.First().GetAttribute("value");
            }
        }

        private void ReadPrice(AdDetails adDetails)
        {
            var prices = webDriver.FindElements(priceLocators);
            if (prices.Any())
            {
                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                var value = prices.First().GetAttribute("value");
                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                decimal.TryParse(value, out var result);
                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                adDetails.Price = result;
            }
        }

        private void DownloadPics(AdDetails adDetails)
        {
            adDetails.InputPicturePaths = new List<string>();
            var images = WebWaiter.Until(SeleniumExtras
                                .WaitHelpers
                                .ExpectedConditions
                                .PresenceOfAllElementsLocatedBy(imageLocators));
            if (images.Any())
            {
                var urls = new List<string>();
                foreach (var image in images)
                {
                    var url = image.GetAttribute("data-large-url");
                    if (url is null)
                    {
                        continue;
                    }
                    urls.Add(url.Trim());
                }
                using var client = new WebClient();
                foreach (var url in urls.Distinct())
                {
                    var fileName = $"{Guid.NewGuid()}.PNG ";
                    client.DownloadFile(url, fileName);
                    var savedPath = $"{Directory.GetCurrentDirectory()}\\AdPics\\{fileName}";
                    File.Move(fileName, savedPath);
                    adDetails.InputPicturePaths.Add(Path.GetFullPath(savedPath));
                }
            }
        }

        private void ReadAddress(AdDetails adDetails)
        {
            var items = webDriver.FindElements(addressLocator);
            if (items.Any())
            {
                adDetails.Address = items.First().Text;
            }
        }

        private void ReadLocation(AdDetails adDetails)
        {
            var items = webDriver.FindElements(locationLocator);
            if (items.Any())
            {
                adDetails.Location = items.First().Text;
            }
        }

        private void ReadTags(AdDetails adDetails)
        {
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            var tags = webDriver.FindElements(tagsLocators);
            if (tags.Any())
            {
                adDetails.Tags = new List<string>();
                foreach (var item in tags)
                {
                    adDetails.Tags.Add(item.Text);
                }
            }
        }

        private void ReadAdDescription(AdDetails adDetails)
        {
            var item = WebWaiter
                                        .Until(SeleniumExtras
                                            .WaitHelpers
                                            .ExpectedConditions
                                            .ElementIsVisible(desLocator));
            adDetails.Description = item.Text;
        }

        private void ReadAdTitle(AdDetails adDetails)
        {
            var adtitle = WebWaiter
                            .Until(SeleniumExtras
                                .WaitHelpers
                                .ExpectedConditions
                                .ElementIsVisible(adtitleLocator));
            adDetails.AdTitle = adtitle.GetAttribute("value");
        }

        private void ReadCategories(AdDetails adDetails)
        {
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            var categories = webDriver
                            .FindElements(categoriesLocators1);
            if (!categories.Any())
            {
                Console.WriteLine("Warning: could not locale categories, try another localtor");
                Console.WriteLine("Trying className: category -> TagName: strong");
                var forms = webDriver
                            .FindElements(By.ClassName("category"));
                if (forms.Any())
                {
                    categories = forms.First().FindElements(By.TagName("strong"));
                }
            }

            adDetails.Categories = new List<string>();
            foreach (var categorie in categories)
            {
                adDetails.Categories.Add(categorie.Text);
            }
        }
    }
}
