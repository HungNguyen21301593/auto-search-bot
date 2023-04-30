using System.Collections.Generic;

namespace auto_webbot.Model
{
    public class AppSetting
    {
        public int StartPage { get; set; }
        public int EndPage { get; set; }
        public List<string> Keywords { get; set; }
        public List<string> TelegramIds { get; set; }
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
}
