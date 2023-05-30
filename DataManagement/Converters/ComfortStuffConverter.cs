﻿using Newtonsoft.Json;
using WebScraper.Models;

namespace DataManagement.Converters
{
    public static class ComfortStuffConverter
    {
        public static string ToJson(this ComfortStuffModel comfortStuffModel)
        {
            return JsonConvert.SerializeObject(comfortStuffModel);
        }
    }
}
