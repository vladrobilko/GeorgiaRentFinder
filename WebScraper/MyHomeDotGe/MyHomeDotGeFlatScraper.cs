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
            var title = mainPage.DocumentNode
                .SelectNodes(
                    "//h5[contains(@class,'card-title')]")?
                .ElementAtOrDefault(htmlDivNumber)?.InnerText ?? "No title";

            if (title == "No title") return title;

            var location = mainPage.DocumentNode
                .SelectNodes(
                    "//div[contains(@class,'address')]")?
                .ElementAtOrDefault(htmlDivNumber)?.InnerText ?? "";

            if (location == "") return title;

            var locationWords = location.Split(new string[] { ", " }, StringSplitOptions.None);

            if (locationWords.Length < 2) return title;

            return $"{title}. {locationWords[^2]}";
        }

        public int GetFlatCost(HtmlDocument mainPage, int htmlDivNumber)
        {
            var inputCost = mainPage.DocumentNode
                .SelectNodes(
                "//b[contains(@class,'item-price-usd  mr-2')]")?
                .ElementAtOrDefault(htmlDivNumber)?.InnerText;

            if (inputCost != null) Regex.Replace(inputCost, @"[^\d\s]+", string.Empty);

            return int.TryParse(inputCost, out var result) ? result : int.MaxValue;
        }

        public string GetFLatLink(HtmlDocument mainPage, string url, int htmlDivNumber)
        {
            return mainPage.DocumentNode
                .SelectNodes(
                "//a[contains(@class,'card-container')]")?
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
                    .FirstOrDefault()?.InnerText;
            }

            if (string.IsNullOrWhiteSpace(input)) return "No description";

            var removeWhitespace = Regex.Replace(input, @"\s{2,}", " ");

            if (removeWhitespace.Length <= descriptionLength) return removeWhitespace;

            var description = removeWhitespace.Substring(0, descriptionLength - 3) + "...";

            return description;
        }

        public string GetFlatOwnerPhoneNumber(HtmlDocument flatPage)
        {
            return flatPage.DocumentNode.SelectNodes(
                    "//div[contains(@class,'container full-height d-flex align-items-center justify-content-between')]//div")?
                .Where(p => p.InnerText.Contains("Phone") && p.InnerText.Length < 20)
                .Select(p => Regex.Replace(p.InnerText, @"[^\d\s]+", string.Empty))?
                .FirstOrDefault()?
                .Replace(" ","") ?? "No number";
        }

        public List<string> GetFlatImages(HtmlDocument flatPage)
        {
            var imagesUrl = flatPage.DocumentNode.SelectNodes(
                "//div[contains(@class,'new-popup-gallery-thumbs')]//p")?
                .Select(e => e.GetAttributeValue("data-image", null))
                .Select(s => Regex.Replace(s, @"(?<!/)/(?!/)", "//"))
                .ToList() ?? new List<string>();

            if (imagesUrl.Count == 0)
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

        public FlatCoordinateModel GetFlatCoordinate(HtmlDocument flatPage)
        {
            var viewsFromPage = flatPage.DocumentNode
                .SelectSingleNode(
                "//div[contains(@id,'map')]");

            var latitude = Convert.ToDouble(viewsFromPage?.GetAttributeValue("data-lat", null));
            var longitude = Convert.ToDouble(viewsFromPage?.GetAttributeValue("data-lng", null));

            if (latitude == 0 || longitude == 0) return new FlatCoordinateModel().GetDefaultFlatCoordinate();

            return new FlatCoordinateModel(latitude, longitude);
        }

        public ComfortStuffModel GetComfortStuff(HtmlDocument flatPage)
        {
            var myHome = flatPage.DocumentNode
                .SelectNodes(
                    "//div[contains(@class,'col-6 col-lg-4 mb-0 mb-md-4 mb-lg-0 d-flex align-items-center mb-lg-0 mb-4 pr-2 pr-lg-0')]//span");

            var bedrooms = myHome?.ElementAtOrDefault(2)?.InnerText ?? "No bedrooms";

            var floor = myHome?.ElementAtOrDefault(4)?.InnerText.Replace(" ", "").Replace("\n", "").Replace("\t", "").Replace("\r", "") ?? "No floors";

            var totalArea = myHome?.ElementAtOrDefault(0)?.InnerText ?? "No total area";

            var myHomeAdditionalInfoAbsent = flatPage.DocumentNode
                .SelectNodes(
                    "//span[contains(@class,'d-block no')]");

            var isGas = myHomeAdditionalInfoAbsent?.FirstOrDefault(e => e.InnerText.Contains("Gas")) == null;

            var isHotWater = myHomeAdditionalInfoAbsent?.FirstOrDefault(e => e.InnerText.Contains("Hot water")) == null;

            var isConditioner = myHomeAdditionalInfoAbsent?.FirstOrDefault(e => e.InnerText.Contains("Air conditioner")) == null;

            return new ComfortStuffModel(bedrooms, floor, totalArea, isGas, isHotWater, isConditioner);
        }
    }
}
