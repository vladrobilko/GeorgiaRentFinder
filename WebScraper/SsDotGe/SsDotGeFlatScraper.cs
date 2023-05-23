using HtmlAgilityPack;
using System.Globalization;
using System.Text.RegularExpressions;
using WebScraper.Interfaces;
using WebScraper.Models;

namespace WebScraper.SsDotGe
{
    public class SsDotGeFlatScraper : IFlatScraper
    {
        public DateTime GetFlatCreationOrMinDate(HtmlDocument mainPage, int htmlDivNumber)
        {
            var formatInputDate = "dd.MM.yyyy/HH:mm";
            var minDateInFormat = DateTime.MinValue.ToString(formatInputDate);
            var inputDate = mainPage.DocumentNode.SelectSingleNode(
                    $"//*[@id=\"list\"]/div[{htmlDivNumber}]/div[1]/div[1]/div[1]/div[2]/div[2]/div/div[2]/div/div[1]/text()")?
                .InnerText.Replace(" ", "")
                .Replace("\r\n", "");
            if (inputDate == null)
            {
                inputDate = mainPage.DocumentNode.SelectSingleNode(
                        $"//*[@id=\"list\"]/div[{htmlDivNumber}]/div[1]/div[1]/div[1]/div[2]/div[2]/div/div/div[1]/text()")?
                    .InnerText.Replace(" ", "")
                    .Replace("\r\n", "") ?? minDateInFormat;
            }

            return DateTime.ParseExact(inputDate, formatInputDate, CultureInfo.InvariantCulture);
        }

        public string GetFlatTitle(HtmlDocument mainPage, int htmlDivNumber)
        {
            return mainPage.DocumentNode
                .SelectSingleNode($"//*[@id=\"list\"]/div[{htmlDivNumber}]/div[1]/div[1]/div[1]/div[2]/div[1]/a/div/span")
                ?.InnerText ?? "No title";
        }

        public int GetFlatCost(HtmlDocument mainPage, int htmlDivNumber)
        {
            var inputCost = mainPage.DocumentNode.SelectSingleNode(
                $"//*[@id=\"list\"]/div[{htmlDivNumber}]/div[1]/div[1]/div[2]/div[2]/div[1]/text()")?.InnerText;

            if (inputCost != null) inputCost = Regex.Replace(inputCost, @"[^\d\s]+", string.Empty);

            return int.TryParse(inputCost, out var result) ? result : int.MaxValue;
        }

        public string GetFLatLink(HtmlDocument mainPage, string url, int htmlDivNumber)
        {
            HtmlNode nextPage = mainPage.DocumentNode.SelectSingleNode($"//*[@id=\"list\"]/div[{htmlDivNumber}]/div[1]/div[1]/div[1]/div[2]/div[1]/a");
            var uri = new Uri(url);
            return uri.Scheme + "://" + uri.Authority + nextPage.GetAttributeValue<string>("href", null);
        }

        public string GetFlatDescription(HtmlDocument flatPage, int descriptionLength)
        {
            var input = flatPage.DocumentNode.SelectNodes(
                    "//span[contains(@class,'details_text')]")?
                .ToList()
                .FirstOrDefault()?
                .InnerText.Replace("\r\n", "");

            if (string.IsNullOrWhiteSpace(input)) return "No description";

            if (!new Regex("^[\x20-\x7E]+$").IsMatch(input)) return input;

            var removeWhitespace = Regex.Replace(input, @"\s{2,}", " ");

            if (removeWhitespace.Length <= descriptionLength) return removeWhitespace;

            return string.Concat(removeWhitespace.Substring(0, descriptionLength - 3) + "...");
        }

        public string GetFlatOwnerPhoneNumber(HtmlDocument flatPage)
        {
            var number = flatPage.DocumentNode.SelectSingleNode(
                    "//*[@id=\"main-body\"]/div[2]/div[2]/div[1]/div[1]/div[7]/div/div/div/div[5]/div/div/div[3]/div/div[1]/div[3]/a/span")
                ?.InnerText;

            if (number == null)
            {
                number = flatPage.DocumentNode
                    .SelectNodes(
                        "//div[contains(@class,'UserMObileNumbersBlock')]//a")?
                    .FirstOrDefault()?.InnerText.Replace("\r\n", "") ?? "No number";
            }

            return Regex.Replace(number, @"\s{2,}", " ");
        }

        public List<string> GetFlatImages(HtmlDocument flatPage)
        {
            var imagesUrl = flatPage.DocumentNode.Descendants("img")
                .Where(e => e.Attributes["class"]?.Value == "img-responsive")
                .Select(e => e.GetAttributeValue("src", null))
                .Where(s => !String.IsNullOrEmpty(s) && !s.Contains("Thumb") && s.Contains("static.ss.ge"))
                .Select(s => Regex.Replace(s, @"(?<!/)/(?!/)", "//"))
                .ToList();

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
                .SelectSingleNode("//*[@id=\"main-body\"]/div[2]/div[2]/div[1]/div[1]/div[6]/div/div[1]/div[2]/div[1]/span")
                ?.InnerText;

            if (viewsFromPage != null) Regex.Replace(viewsFromPage, @"[^\d\s]+", string.Empty);

            return int.TryParse(viewsFromPage, out var result) ? result : int.MaxValue;
        }

        public FlatCoordinate GetFlatCoordinate(HtmlDocument flatPage)
        {
            return new FlatCoordinate().GetDefaultFlatCoordinate();
        }
    }
}