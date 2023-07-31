using HtmlAgilityPack;

namespace WebScraper
{
    public static class UsdToGelCourseScraper
    {
        public static double GetGelInOneDollarFromGeorgiaNationalBank()
        {
            var urlGeorgiaNationalBank = "https://nbg.gov.ge/en/monetary-policy/currency";

            var web = new HtmlWeb();

            try
            {
                var page = web.Load(urlGeorgiaNationalBank);

                var gelInOneDollar = page.DocumentNode.SelectSingleNode(
                    "//*[@id=\"nbg\"]/div[1]/div[4]/div/div[2]/div[2]/div[4]/div[41]/div/div/div[3]/span")?.InnerText;

                return double.TryParse(gelInOneDollar, out var result) ? result : int.MaxValue;
            }
            catch
            {
                return double.MaxValue;
            }
        }
    }
}