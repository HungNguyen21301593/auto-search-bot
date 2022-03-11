namespace auto_webbot.Pages.Post
{
    using System;
    using System.Linq;
    using System.Threading;
    using global::auto_webbot.Model;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    namespace auto_webbot.Pages.Post
    {
        public class AdDetailInputPage
        {
            public IWebDriver webDriver { get; set; }
            public AppSetting config { get; set; }
            public AdDetailInputPage(IWebDriver webDriver, AppSetting config)
            {
                this.webDriver = webDriver;
                this.config = config;
            }
            private WebDriverWait WebWaiter => new WebDriverWait(webDriver, TimeSpan.FromSeconds(120));
            private By AdTitleLocator = By.Id("postad-title");
            private By DescriptionLocaltor = By.Id("pstad-descrptn");
            private By FileInputWrapper = By.ClassName("imageUploadButtonWrapper");
            private By ChangeLocationButtonLocator = By.XPath("//*[text()='Change']");
            private By LocationLocator = By.Id("location");
            private By LocationFirstLocator = By.Id("LocationSelector-item-0");
            private By LocationTextLocator = By.CssSelector("div[class*='locationText']");
            private By addressLocator = By.Id("servicesLocationInput");
            private By addressLocatorFirst = By.Id("LocationSelector-item-0");
            private By PriceLocator = By.Id("PriceAmount");
            private By PricePleaseContactLocator = By.XPath("//*[text()='Please Contact']");
            private By PostButtonLocator = By.CssSelector("button[type='submit']");
            private By companyLocator = By.Id("company_s");
            private By carYearLocator = By.Id("caryear_i");
            private By carKmLocator = By.Id("carmileageinkms_i");
            private By selectBasicPackage = By.CssSelector("button[data-qa-id='package-0-bottom-select']");


            public bool InputAdDetails(AdDetails adDetails)
            {
                if (adDetails.AdTitle != null)
                {
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    InputAdTitle(adDetails);
                    Console.WriteLine("InputAdTitle");
                }
                if (adDetails.Description != null)
                {
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    InputDesciption(adDetails);
                    Console.WriteLine("InputDesciption");
                }
                if (adDetails.AdType != null)
                {
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    SelectAdtype(adDetails);
                    Console.WriteLine("SelectAdtype");
                }
                if (adDetails.ForSaleBy != null)
                {
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    SelectForSaleBy(adDetails);
                    Console.WriteLine("SelectForSaleBy");
                }
                if (adDetails.MoreInfo != null)
                {
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    SelectMoreInfo(adDetails);
                    Console.WriteLine("SelectMoreInfo");
                }
                if (adDetails.Fulfillments.Any())
                {
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    SelectFulfillment(adDetails);
                    Console.WriteLine("SelectFulfillment");
                }
                if (adDetails.Payments.Any())
                {
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    SelectPayment(adDetails);
                    Console.WriteLine("SelectFulfillment");
                }
                if (adDetails.Tags.Any())
                {
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    SelectTags(adDetails);
                    Console.WriteLine("SelectTags");
                }
                if (adDetails.InputPicturePaths.Any())
                {
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    InputPicture(adDetails);
                    Console.WriteLine("InputPicture");
                }
                if (adDetails.Location != null)
                {
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    InputLocation(adDetails.Location);
                    Console.WriteLine("InputLocation");
                }
                if (adDetails.Address != null)
                {
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    InputAddress(adDetails);
                    Console.WriteLine("InputAddress");
                }
                if (adDetails.Price != 0)
                {
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    InputPrice(adDetails);
                    Console.WriteLine("InputPrice");
                }

                if (adDetails.Price == 0)
                {
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    InputPricePleaseContact();
                    Console.WriteLine("InputPrice");
                }

                if (adDetails.Company != null)
                {
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    InputCompany(adDetails);
                    Console.WriteLine("InputCompany");
                }
                if (adDetails.DynamicTextOptions.Any())
                {
                    InputDynamicInputs(adDetails);
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    Console.WriteLine("InputDynamicInputs");
                    InputDynamicSelectOptions(adDetails);
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    Console.WriteLine("InputDynamicSelectOptions");
                }

                if (adDetails.CarYear != null)
                {
                    InputCarYear(adDetails);
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    Console.WriteLine("InputCarYear");
                }

                if (adDetails.CarKm != null)
                {
                    InputCarKm(adDetails);
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    Console.WriteLine("InputCarKm");
                }

                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                SelectBasicPakage();
                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                if (config.Mode == Mode.test)
                {
                    Console.WriteLine("App is in testing node, so no ad will be actual posted");
                    return true;
                }
                else
                {
                    Post();
                    return CheckIfPostedSuccess();
                }
            }

            private void InputAdTitle(AdDetails adDetails)
            {
                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                var element = webDriver.FindElements(AdTitleLocator);
                if (!element.Any())
                {
                    Console.WriteLine("Could not dound AdTitleLocator");
                }
                element.First().Clear();
                element.First().SendKeys(adDetails.AdTitle);
            }

            private bool CheckIfPostedSuccess()
            {
                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                var postedText = webDriver.FindElements(By.XPath("//*[text()='You have successfully posted your ad!']"));
                return postedText.Any();
            }

            private void SelectBasicPakage()
            {
                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                var elements = webDriver.FindElements(selectBasicPackage);
                if (elements.Any())
                {
                    elements.First().Click();
                }
            }

            private void InputDynamicInputs(AdDetails adDetails)
            {
                foreach (var option in adDetails.DynamicTextOptions)
                {
                    var elements = webDriver.FindElements(By.XPath($"//*[text()='{option}']"));
                    if (elements.Any())
                    {
                        elements.First().Click();
                    }
                }
            }

            private void InputDynamicSelectOptions(AdDetails adDetails)
            {
                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                var selects = webDriver.FindElements(By.TagName("select"));
                if (selects.Any())
                {
                    foreach (var select in selects)
                    {
                        try
                        {
                            var selectElement = new SelectElement(select);
                            foreach (var element in selectElement.Options)
                            {
                                if (!adDetails.DynamicTextOptions.Any(dynamicText => element.Text.Equals(dynamicText)))
                                    continue;
                                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                                element.Click();
                                //Console.WriteLine($"Selected |{dynamicText}| on {@select.GetAttribute("name")}");
                                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Skip {select.GetAttribute("name")}, {e.Message}, {e.StackTrace}");
                        }
                    }
                }
                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            }

            private void InputCompany(AdDetails adDetails)
            {
                var element = WebWaiter
                                .Until(SeleniumExtras
                                    .WaitHelpers
                                    .ExpectedConditions
                                    .ElementIsVisible(companyLocator));
                element.SendKeys(adDetails.Company);
            }

            private void InputCarYear(AdDetails adDetails)
            {
                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                var elements = webDriver.FindElements(carYearLocator);
                if (elements.Any())
                {
                    elements.First().SendKeys(adDetails.CarYear);
                }
            }

            private void InputCarKm(AdDetails adDetails)
            {
                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                var elements = webDriver.FindElements(carKmLocator);
                if (!elements.Any())
                {
                    Console.WriteLine("Could not found any InputCarKm so skip");
                    return;
                }
                elements.First().SendKeys(adDetails.CarKm);
            }

            private void InputAddress(AdDetails adDetails)
            {
                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                var addressElements = webDriver.FindElements(addressLocator);
                if (!addressElements.Any())
                {
                    Console.WriteLine("Could not found any InputAddress so skip");
                    return;
                }
                addressElements.First().Clear();
                addressElements.First().SendKeys(adDetails.Address);
                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                var addressElementFirst = WebWaiter
                               .Until(SeleniumExtras
                                   .WaitHelpers
                                   .ExpectedConditions
                                   .ElementToBeClickable(addressLocatorFirst));
                addressElementFirst.Click();
            }

            //private void RetryInputAddressMultipleTimes(string locationText)
            //{
            //    var location = WebWaiter
            //        .Until(SeleniumExtras
            //            .WaitHelpers
            //            .ExpectedConditions
            //            .ElementIsVisible(LocationLocator));

            //    for (var i = 0; i < 20; i++)
            //    {
            //        location.Clear();
            //        Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            //        location.SendKeys(locationText);
            //        Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            //        var locationFirst = webDriver.FindElements(LocationFirstLocator);
            //        if (!locationFirst.Any())
            //        {
            //            Console.WriteLine("Input location failed, proceed retry");
            //            continue;
            //        }
            //        locationFirst.First().Click();
            //        Console.WriteLine("Input location success");
            //        break;
            //    }
            //}

            private void SelectTags(AdDetails adDetails)
            {
                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                var inputTag = WebWaiter
                .Until(SeleniumExtras
                    .WaitHelpers
                    .ExpectedConditions
                    .ElementIsVisible(By.Id("pstad-tagsInput")));
                foreach (var tag in adDetails.Tags)
                {
                    inputTag.SendKeys(tag);
                    inputTag.SendKeys(Keys.Enter);
                }
            }

            private void SelectPayment(AdDetails adDetails)
            {
                foreach (var payment in adDetails.Payments)
                {
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    var Payment = WebWaiter
                    .Until(SeleniumExtras
                        .WaitHelpers
                        .ExpectedConditions
                        .ElementIsVisible(By.XPath($"//*[text()='{payment}']")));
                    Payment.Click();
                }
            }

            private void SelectFulfillment(AdDetails adDetails)
            {
                foreach (var Fulfillment in adDetails.Fulfillments)
                {
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    var input = WebWaiter
                    .Until(SeleniumExtras
                        .WaitHelpers
                        .ExpectedConditions
                        .ElementIsVisible(By.XPath($"//*[text()='{Fulfillment}']")));
                    input.Click();
                }
            }

            private void SelectAdtype(AdDetails adDetails)
            {
                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                var adtype = WebWaiter
                .Until(SeleniumExtras
                    .WaitHelpers
                    .ExpectedConditions
                    .ElementIsVisible(By.XPath($"//*[text()='{adDetails.AdType}']")));
                adtype.Click();
            }

            private void SelectForSaleBy(AdDetails adDetails)
            {
                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                var forSaleBy = WebWaiter
                .Until(SeleniumExtras
                    .WaitHelpers
                    .ExpectedConditions
                    .ElementIsVisible(By.XPath($"//*[text()='{adDetails.ForSaleBy}']")));
                forSaleBy.Click();
            }

            private void SelectMoreInfo(AdDetails adDetails)
            {
                var moreInforExist = webDriver.FindElements(By.Id("moreinfo_s"));
                if (moreInforExist.Any())
                {
                    var select = WebWaiter
                .Until(SeleniumExtras
                    .WaitHelpers
                    .ExpectedConditions
                    .ElementIsVisible(By.Id("moreinfo_s")));
                    select.Click();

                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    var MoreInfo = WebWaiter
                    .Until(SeleniumExtras
                        .WaitHelpers
                        .ExpectedConditions
                        .ElementIsVisible(By.XPath($"//*[text()='{adDetails.MoreInfo}']")));
                    MoreInfo.Click();
                }
            }

            private void Post()
            {
                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                var post = webDriver.FindElements(PostButtonLocator);
                if (post.Any())
                {
                    post.First().Click();
                }
            }

            private void InputPrice(AdDetails adDetails)
            {
                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                var price = WebWaiter
                .Until(SeleniumExtras
                    .WaitHelpers
                    .ExpectedConditions
                    .ElementIsVisible(PriceLocator));
                price.SendKeys(adDetails.Price.ToString());
            }

            private void InputPricePleaseContact()
            {
                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                var pleaseContact = WebWaiter
                    .Until(SeleniumExtras
                        .WaitHelpers
                        .ExpectedConditions
                        .ElementIsVisible(PricePleaseContactLocator));
                pleaseContact.Click();
            }

            private void InputPicture(AdDetails adDetails)
            {
                var fileInputWrapper = webDriver.FindElement(FileInputWrapper);
                var updatefile = fileInputWrapper.FindElement(By.TagName("input"));

                foreach (var path in adDetails.InputPicturePaths)
                {
                    updatefile.SendKeys(path);
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                }
                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
            }

            private void InputDesciption(AdDetails adDetails)
            {
                var description = WebWaiter
                                .Until(SeleniumExtras
                                    .WaitHelpers
                                    .ExpectedConditions
                                    .ElementIsVisible(DescriptionLocaltor));
                description.SendKeys(adDetails.Description);
            }

            private void InputLocation(string locationPrefix)
            {
                try
                {
                    InputLocatonOnce(locationPrefix);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed input location:{e.Message}, retry one more time");
                    InputLocatonOnce(locationPrefix);
                }
            }

            private void InputLocatonOnce(string locationText)
            {
                var changeButton = webDriver.FindElements(ChangeLocationButtonLocator);
                if (changeButton.Any())
                {
                    changeButton.First().Click();
                    CleanLocation();
                }

                RetryInputLocationMultipleTimes(locationText);

                Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                var locationTextElement = webDriver.FindElements(LocationTextLocator);
                if (!locationTextElement.Any()) throw new Exception("There was error input Location");
                var currentLocationText = locationTextElement.First().GetAttribute("innerText");
                if (!currentLocationText.StartsWith(locationText))
                {
                    throw new Exception("Input location failed");
                }
            }

            private void RetryInputLocationMultipleTimes(string locationText)
            {
                var location = WebWaiter
                    .Until(SeleniumExtras
                        .WaitHelpers
                        .ExpectedConditions
                        .ElementIsVisible(LocationLocator));

                for (var i = 0; i < 20; i++)
                {
                    location.Clear();
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    location.SendKeys(locationText);
                    Thread.Sleep(config.AdGlobalSetting.Sleep.SleepBetweenEachAction);
                    var locationFirst = webDriver.FindElements(LocationFirstLocator);
                    if (!locationFirst.Any())
                    {
                        Console.WriteLine("Input location failed, proceed retry");
                        continue;
                    }
                    locationFirst.First().Click();
                    Console.WriteLine("Input location success");
                    break;
                }
            }

            private void CleanLocation()
            {
                var location = WebWaiter
                    .Until(SeleniumExtras
                    .WaitHelpers
                    .ExpectedConditions
                    .ElementIsVisible(LocationLocator));
                location.Clear();
            }
        }
    }

}
