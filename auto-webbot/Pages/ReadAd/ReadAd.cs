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
        private By imageLocators = By.CssSelector("img[class*='image-']");
        private By pictureLocators = By.TagName("picture");
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
            var listAdDeatails = new List<AdDetails>();
            try
            {
                ReadOnlyCollection<IWebElement> ImageTitles;
                LoadReadPage(out ImageTitles);
                for (int i = 0; i < ImageTitles.Count(); i++)
                {
                    var position = i + 1;
                    if (position < adGlocalSeting.Position.From || position > adGlocalSeting.Position.To)
                    {
                        continue;
                    }
                    ImageTitles[i].Click();
                    listAdDeatails.Add(ReadSingleAd());
                    Thread.Sleep(adGlocalSeting.Sleep.DelayBetweenEachRead);
                    LoadReadPage(out ImageTitles);
                }
                return listAdDeatails;
            }
            catch (Exception e)
            {
                throw new Exception($"Error readding Ad, error {e.Message}, result: {JsonConvert.SerializeObject(listAdDeatails)}, stack trace {e.StackTrace}");
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

            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            DownloadPics(adDetails);
            Console.WriteLine($"DownloadPics {JsonConvert.SerializeObject(adDetails)}");

            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            var editAd = webDriver.FindElements(EditAdLocator);
            if (editAd.Any())
            {
                editAd.First().Click();
            }
           
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
            var company = webDriver.FindElements(companyLocator);
            if (company.Any())
            {
                adDetails.Company = company.First().GetAttribute("value");
            }
        }

        private void ReadPrice(AdDetails adDetails)
        {
            var prices = webDriver.FindElements(priceLocators);
            if (!prices.Any()) return;
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            var value = prices.First().GetAttribute("value");
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            decimal.TryParse(value, out var result);
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            adDetails.Price = result;
        }

        private void DownloadPics(AdDetails adDetails)
        {
            adDetails.InputPicturePaths = new List<string>();
            var matchedSettings = config.AdGlobalSetting.SpecicalAdtitleSetting
                .Where(s => adDetails.AdTitle.ToLower().Contains(s.ToLower()))
                .ToList();

            if (matchedSettings.Any())
            {
                adDetails.InputPicturePaths = UsedSpecialAdSetting(matchedSettings);
                return;
            }
            adDetails.InputPicturePaths = DownloadNormalAdPics();
        }

        private static List<string> UsedSpecialAdSetting(List<string> matchedSettings)
        {
            var inputPicturePaths = new List<string>();
            if (matchedSettings.Count() > 1)
            {
                Console.WriteLine($"Warning, multiple matched SpecialAdSetting, {string.Join(',',matchedSettings)}");
            }

            foreach (var matchedSetting in matchedSettings)
            {
                var directoryInfo = new DirectoryInfo($"{Directory.GetCurrentDirectory()}\\{matchedSetting}");
                var allFiles = directoryInfo.GetFiles();
                inputPicturePaths.AddRange(allFiles.Select(f => f.FullName));
            }
            return inputPicturePaths;
        }

        private List<string> DownloadNormalAdPics()
        {
            var inputPicturePaths = new List<string>();
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            var pictures = webDriver.FindElements(pictureLocators);
            if (!pictures.Any()) return inputPicturePaths;
            Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);

            var urls = new List<string>();
            foreach (var picture in pictures)
            {
                var images = picture.FindElements(imageLocators);
                if (!images.Any())
                {
                    continue;
                }
                var url = images.First().GetAttribute("src");
                if (url is null)
                {
                    continue;
                }
                urls.Add(url.Trim());
            }
            using var client = new WebClient();
            foreach (var url in urls.Distinct())
            {
                var fileName = $"{Guid.NewGuid()}.JPG ";
                client.DownloadFile(url, fileName);
                var savedPath = $"{Directory.GetCurrentDirectory()}\\AdPics\\{fileName}";
                if (new FileInfo(fileName).Length < 10000)//20000b
                {
                    File.Delete(fileName);
                    continue;
                }
                else
                {
                    File.Move(fileName, savedPath);
                }
                inputPicturePaths.Add(Path.GetFullPath(savedPath));
            }
            return inputPicturePaths;
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
            var items = webDriver.FindElements(desLocator);
            if (items.Any())
            {
                adDetails.Description = items.First().Text;
            }
        }

        private void ReadAdTitle(AdDetails adDetails)
        {
            var items = webDriver.FindElements(adtitleLocator);
            if (items.Any())
            {
                adDetails.AdTitle = items.First().GetAttribute("value");
            }
        }

        private string ProceedAdTitle(string adTitle)
        {
            var output = adTitle.Split(':')[1].Trim();
            return output;
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
