using HtmlAgilityPack;
using System.Globalization;
using WebScraper.Models;

namespace WebScraper
{
    public class MyHomeDotGeFlatScraper : IFlatScraper
    {
        public DateTime GetFlatCreationOrMinDate(HtmlDocument mainPage, int htmlDivNumber)
        {
            var formatInputDate = "dd.MM.yyyy/HH:mm";
            var minDateInFormat = DateTime.MinValue.ToString(formatInputDate);
            var inputDate = mainPage.DocumentNode.SelectSingleNode(
                    $"//*[@id=\"main_block\"]/div[3]/div[1]/div[4]/div/div[{htmlDivNumber}]/a/div/div/div/div[4]/text");
            ///html/body/div[5]/div[1]/div[3]/div[1]/div[4]/div/div[1]/a/div[1]/div/div/div[4]
            return new DateTime();
        }

        public string GetFlatTitle(HtmlDocument mainPage, int htmlDivNumber)
        {
            throw new NotImplementedException();
        }

        public int GetFlatCost(HtmlDocument mainPage, int htmlDivNumber)
        {
            throw new NotImplementedException();
        }

        public string GetFLatLink(HtmlDocument mainPage, string url, int htmlDivNumber)
        {
            throw new NotImplementedException();
        }

        public string GetFlatDescription(HtmlDocument flatPage, int htmlDivNumber, int descriptionLength)
        {
            throw new NotImplementedException();
        }

        public string GetFlatOwnerPhoneNumber(HtmlDocument flatPage)
        {
            throw new NotImplementedException();
        }

        public List<string> GetFirstTenImages(HtmlDocument flatPage)
        {
            throw new NotImplementedException();
        }

        public int GetPageViews(HtmlDocument flatPage)
        {
            throw new NotImplementedException();
        }

        public FlatCoordinate GetFlatCoordinate(HtmlDocument flatPage)
        {
            throw new NotImplementedException();
        }
    }
}
