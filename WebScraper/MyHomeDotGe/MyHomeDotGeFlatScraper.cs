using HtmlAgilityPack;
using System.Globalization;
using System.Text.RegularExpressions;
using WebScraper.Interfaces;
using WebScraper.Models;

namespace WebScraper.MyHomeDotGe
{
    public class MyHomeDotGeFlatScraper : IFlatScraper
    {
        public DateTime GetFlatCreationOrMinDate(HtmlDocument mainPage, int htmlDivNumber)
        {
            var formatInputDate = "d MMM HH:mm yyyy";
            var minDateInFormat = DateTime.MinValue.ToString(formatInputDate);

            var inputDate = mainPage.DocumentNode
                .SelectNodes(
                "//div[contains(@class,'statement-date')]")?
                .ToList()
                .ElementAtOrDefault(htmlDivNumber)?.InnerText ?? minDateInFormat;

            if (inputDate != minDateInFormat)
            {
                var currentYear = DateTime.Now.Year.ToString();
                inputDate += " " + currentYear;
            }

            return DateTime.ParseExact(inputDate, formatInputDate, CultureInfo.InvariantCulture);
        }

        public string GetFlatTitle(HtmlDocument mainPage, int htmlDivNumber)
        {
            return mainPage.DocumentNode
                .SelectNodes(
                "//h5[contains(@class,'card-title')]")?
                .ToList()
                .ElementAtOrDefault(htmlDivNumber)?.InnerText ?? "No title";
        }

        public int GetFlatCost(HtmlDocument mainPage, int htmlDivNumber)
        {
            var inputCost = mainPage.DocumentNode
                .SelectNodes(
                "//b[contains(@class,'item-price-usd  mr-2')]")?
                .ToList()
                .ElementAtOrDefault(htmlDivNumber)?.InnerText;

            if (inputCost != null) Regex.Replace(inputCost, @"[^\d\s]+", string.Empty);

            return int.TryParse(inputCost, out var result) ? result : int.MaxValue;
        }

        public string GetFLatLink(HtmlDocument mainPage, string url, int htmlDivNumber)
        {
            return mainPage.DocumentNode
                .SelectNodes(
                "//a[contains(@class,'card-container')]")?
                .ToList()
                .ElementAtOrDefault(htmlDivNumber)?
                .GetAttributeValue<string>("href", null);
        }

        public string GetFlatDescription(HtmlDocument flatPage, int descriptionLength)
        {
            var input = flatPage.DocumentNode
                .SelectSingleNode(
                    "//*[@id=\"main_block\"]/div[5]/div[5]/div[2]/div/p[1]/text()")?.InnerText
                .Replace("\r\n", "");

            if (input == null)
            {
                input = flatPage.DocumentNode
                    .SelectNodes(
                    "//p[contains(@class,'pr-comment translated')]")?
                    .ToList()
                    .FirstOrDefault()?.InnerText;
            }

            if (string.IsNullOrWhiteSpace(input)) return "No description";

            if (!new Regex("^[\x20-\x7E]+$").IsMatch(input)) return input;

            var removeWhitespace = Regex.Replace(input, @"\s{2,}", " ");

            if (removeWhitespace.Length <= descriptionLength) return removeWhitespace;

            return string.Concat(removeWhitespace.Substring(0, descriptionLength - 3) + "..."); // check it
        }

        public string GetFlatOwnerPhoneNumber(HtmlDocument flatPage)
        {
            return flatPage.DocumentNode.SelectNodes(
                    "//div[contains(@class,'container full-height d-flex align-items-center justify-content-between')]//div")?
                .Where(p => p.InnerText.Contains("Phone") && p.InnerText.Length < 20)
                .Select(p => Regex.Replace(p.InnerText, @"[^\d\s]+", string.Empty))
                .ToList()
                .FirstOrDefault() ?? "No number";
        }

        public List<string> GetFlatImages(HtmlDocument flatPage)
        {
            var imagesUrl = flatPage.DocumentNode.SelectNodes(
                "//div[contains(@class,'new-popup-gallery-thumbs')]//p")?
                .Select(e => e.GetAttributeValue("data-image", null))
                .Select(s => Regex.Replace(s, @"(?<!/)/(?!/)", "//"))
                .ToList() ?? new List<string>();

            if (imagesUrl == null || imagesUrl.Count == 0)
            {
                var linkBlurredImageIfFlatNotHaveImages =
                    "https://media.istockphoto.com/id/955951212/photo/blurred-background-modern-kitchen-and-dinning-room-in-house-with-bokeh-light-lifestyle-backdrop.jpg?s=612x612&w=0&k=20&c=THHBhrRhOCnD0DdfLj42JNsDuzZpC0oqp7K0EIO4B8U=";
                imagesUrl.Add(linkBlurredImageIfFlatNotHaveImages);
            }

            return imagesUrl;
        }

        public int GetPageViews(HtmlDocument flatPage)
        {
            var viewsFromPage = flatPage.DocumentNode
                .SelectNodes(
                "//div[contains(@class,'d-flex align-items-center views')]")?
                .FirstOrDefault()?.InnerText;

            if (viewsFromPage != null) Regex.Replace(viewsFromPage, @"[^\d\s]+", string.Empty);

            return int.TryParse(viewsFromPage?.Replace(" ", ""), out var result) ? result : int.MaxValue;
        }

        public FlatCoordinate GetFlatCoordinate(HtmlDocument flatPage)
        {
            var viewsFromPage = flatPage.DocumentNode
                .SelectSingleNode(
                "//div[contains(@id,'map')]");

            var latitude = Convert.ToDouble(viewsFromPage.GetAttributeValue("data-lat", null));
            var longitude = Convert.ToDouble(viewsFromPage.GetAttributeValue("data-lng", null));

            if (latitude == 0 || longitude == 0) return new FlatCoordinate().GetDefaultFlatCoordinate();

            return new FlatCoordinate(latitude, longitude);
        }
    }
}
