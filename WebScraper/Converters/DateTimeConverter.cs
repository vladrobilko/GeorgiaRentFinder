using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScraper.Converters
{
    public static class DateTimeConverter
    {
        public static string ToCommonViewString(this DateTime dateTime)
        {
            return $"{dateTime:dd/MM/yyyy HH:mm}";
        }
    }
}