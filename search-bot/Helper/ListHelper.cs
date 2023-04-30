using AngleSharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kijiji_searchbot.Helper
{
    public static class ListHelper
    {
        public static T GetRandomItem<T>(this List<T> list)
        {
            var random = new Random();
            var randomPosition = random.Next(0, list.Count);
            return list.GetItemByIndex(randomPosition);
        }
    }
}
