using auto_webbot.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace auto_webbot.Extensions
{
    public static class RandomSleepExtensions
    {
        public static TimeSpan GetNextValuesInMinutes(this RandomSleep randomSleep)
        { 
            var random = new Random();
           return TimeSpan.FromMinutes(random.Next(randomSleep.From, randomSleep.To));
        }
    }
}
