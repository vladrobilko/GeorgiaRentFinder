using HtmlAgilityPack;
using System.Globalization;
using System.Text.RegularExpressions;
using WebScraper.Models;

namespace WebScraper
{
    public class MyHomeDotGeFlatScraper : IFlatScraper
    {
        public DateTime GetFlatCreationOrMinDate(HtmlDocument mainPage, int htmlDivNumber)
        {
            var formatInputDate = "dd MMM HH:mm yyyy";
            var minDateInFormat = DateTime.MinValue.ToString(formatInputDate);
            var inputDate = mainPage.DocumentNode.SelectNodes(
                    "//div[contains(@class,'statement-date')]").ToList().ElementAtOrDefault(htmlDivNumber)?.InnerText ?? minDateInFormat;

            if (inputDate != minDateInFormat)
            {
                var currentYear = DateTime.Now.Year.ToString();
                inputDate += " " + currentYear;
            }

            return DateTime.ParseExact(inputDate, formatInputDate, CultureInfo.InvariantCulture);
        }

        public string GetFlatTitle(HtmlDocument mainPage, int htmlDivNumber)
        {
            return mainPage.DocumentNode.SelectNodes(
                "//h5[contains(@class,'card-title')]").ToList().ElementAtOrDefault(htmlDivNumber)?.InnerText ?? "No title";
        }

        public int GetFlatCost(HtmlDocument mainPage, int htmlDivNumber)
        {
            var inputCost =  mainPage.DocumentNode.SelectNodes(
                "//b[contains(@class,'item-price-usd  mr-2')]").ToList().ElementAtOrDefault(htmlDivNumber)?.InnerText;

            return int.TryParse(inputCost, out var result) ? result : int.MaxValue;
        }

        public string GetFLatLink(HtmlDocument mainPage, string url, int htmlDivNumber)
        {
            return  mainPage.DocumentNode.SelectNodes(
                "//a[contains(@class,'card-container')]").ToList().ElementAtOrDefault(htmlDivNumber)?.GetAttributeValue<string>("href", null);
        }

        public string GetFlatDescription(HtmlDocument flatPage, int descriptionLength)
        {
            var input = flatPage.DocumentNode
                .SelectSingleNode(
                    "//*[@id=\"main_block\"]/div[5]/div[5]/div[2]/div/p[1]/text()")?.InnerText
                .Replace("\r\n", "");

            if (string.IsNullOrWhiteSpace(input)) return "No description";

            if (!new Regex("^[\x20-\x7E]+$").IsMatch(input)) return input;

            var removeWhitespace = Regex.Replace(input, @"\s{2,}", " ");

            if (removeWhitespace.Length <= descriptionLength) return removeWhitespace;

            return string.Concat(removeWhitespace.Substring(0, descriptionLength - 3) + "..."); // check it
        }

        public string GetFlatOwnerPhoneNumber(HtmlDocument flatPage)
        {
            return flatPage.DocumentNode.SelectNodes(
                "//div[contains(@class,'container full-height d-flex align-items-center justify-content-between')]")?.ToList().FirstOrDefault()?.InnerText ?? "No number";
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
