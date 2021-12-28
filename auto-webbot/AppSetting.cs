using System;
using System.Collections.Generic;
using System.Text;

namespace auto_webbot.Model
{
    public class AppSetting
    {
        public Credential Credential { get; set; }
        public RandomSleep RandomSleep { get; set; }
        public string OutputFilePrefix { get; set; }
    }
    public class Credential
    {
        public string UserName { get; set; }
        public string Pass { get; set; }
    }
    public class RandomSleep
    {
        public int From { get; set; }
        public int To { get; set; }
    }
    
}
