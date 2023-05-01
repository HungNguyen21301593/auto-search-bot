﻿using System.Collections.Generic;

namespace auto_webbot.Model
{
    public class ExecutorSetting
    {
        public string Type { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public int StartPage { get; set; }
        public int EndPage { get; set; }
        public int MinAdsPositionOnEachPage { get; set; }
        public int MaximumAdsOnEachPage { get; set; }
        public List<string> Keywords { get; set; }
        public List<string> ExcludeKeywords { get; set; }
        public List<string> TelegramIds { get; set; }
        public List<string> DumbTelegramIds { get; set; }
        public BaseUrlSetting BaseUrlSetting { get; set; }
    }

    public class BaseUrlSetting
    {
        public List<string> CriteriaUrls { get; set; }
        public DynamicParam DynamicParams { get; set; }
        public Dictionary<string, string> StaticParams { get; set; }
    }

    public class DynamicParam
    {
        public string Page { get; set; }
    }

    public class DynamicParamValue : DynamicParam
    {
    }

    public static class ParamExtension
    {
        public static string Apply(this Dictionary<string, string> staticParams, string criteriaUrl)
        {
            var result = criteriaUrl;
            foreach (var staticParam in staticParams)
            {
                result = result.Replace(staticParam.Key, staticParam.Value);
            }
            return result;
        }
    }

    public static class ExecutorType
    {
        public const string ExecutorTypeKijiji = "Kijiji";
        public const string ExecutorTypeMarketPlace = "MarketPlace";
        public const string ExecutorChotot = "Chotot";
        public const string ExecutorAlonhadat = "Alonhadat";
        public const string ExecutorAlonhadatAgent = "AlonhadatAgent";
    }
}