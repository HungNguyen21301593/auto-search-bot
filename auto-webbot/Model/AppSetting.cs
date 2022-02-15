using System;
using System.Collections.Generic;

namespace auto_webbot.Model
{
    public class AppSetting
    {
        public int StartPage { get; set; }
        public int EndPage { get; set; }
        public List<string> Keywords { get; set; }
        public List<string> TelegramIds { get; set; }
    }
}
