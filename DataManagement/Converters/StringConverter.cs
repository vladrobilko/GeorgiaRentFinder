using Newtonsoft.Json;
using WebScraper.Models;

namespace DataManagement.Converters
{
    public static class StringConverter
    {
        public static ComfortStuff ToComfortStuff(this string comfortStuffJson)
        {
            return JsonConvert.DeserializeObject<ComfortStuff>(comfortStuffJson) ?? new ComfortStuff();
        }
    }
}
