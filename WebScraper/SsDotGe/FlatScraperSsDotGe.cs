using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using WebScraper.Models;

namespace WebScraper.SsDotGe
{
    public class FlatScraperSsDotGe
    {
        private readonly int _flatsOnPage;

        private readonly int _flatLowestPrice;

        private readonly int _flatHighestPrice;

        public FlatScraperSsDotGe(int flatOnPage, int flatLowestPrice, int flatHighestPrice)
        {
            _flatsOnPage = flatOnPage;
            _flatLowestPrice = flatLowestPrice;
            _flatHighestPrice = flatHighestPrice;
        }

        public List<FlatInfoModel> ScrapPageWithAllFlats(string url, DateTime lastCheckDate)
        {
            var flats = new List<FlatInfoModel>();

            HtmlDocument mainPage = GetHtmlDocumentForPage(url);

            for (int i = 0, j = 0; i < _flatsOnPage; j++)
            {
                if (j > _flatsOnPage * 2) break;

                var flatCreationDate = GetFlatCreationDateOrMinDate(mainPage, j);

                if (flatCreationDate < lastCheckDate) continue;

                var flatTitle = GetFlatTitle(mainPage, j);

                var flatCost = GetFlatCost(mainPage, j);

                if (IsFlatSuit(flatTitle, flatCost))
                {
                    var flatLink = GetFLatLink(mainPage, url, j);

                    var flatDescription = GetValidDescription(mainPage, j, 200);

                    flats.Add(GetFlatInfoModel(flatLink, flatTitle, flatCost, flatDescription, flatCreationDate));
                    i++;
                }

                if (flatTitle != "No title" && !IsFlatSuit(flatTitle, flatCost)) i++;
            }

            return flats;
        }

        private FlatInfoModel GetFlatInfoModel(string flatLink, string flatTitle, int flatCost, string flatDescription, DateTime flatCreationDate)
        {
            HtmlDocument flatPage = GetHtmlDocumentForPage(flatLink);

            var flatOwnerPhoneNumber = GetFlatOwnerPhoneNumberFromFlatPage(flatPage);

            var firstTenImagesFromFlatPage = GetFirstTenImagesFromFlatPage(flatPage);

            var pageViews = GetPageViewsFromFlatPage(flatPage);

            return new FlatInfoModel(
                flatTitle,
                flatCost,
                flatCreationDate,
                flatDescription,
                flatOwnerPhoneNumber,
                firstTenImagesFromFlatPage,
                flatLink,
                pageViews,
                new FlatCoordinate().GetDefaultFlatCoordinate()
            );
        }

        private int GetPageViewsFromFlatPage(HtmlDocument flatPage)
        {
            var viewsFromPage = flatPage.DocumentNode
                .SelectSingleNode("//*[@id=\"main-body\"]/div[2]/div[2]/div[1]/div[1]/div[6]/div/div[1]/div[2]/div[1]/span")
                ?.InnerText;

            return int.TryParse(viewsFromPage?.Replace(" ", ""), out var result) ? result : int.MaxValue;
        }

        private HtmlDocument GetHtmlDocumentForPage(string pageUrl)
        {
            var webForMainPage = new HtmlWeb();
            HtmlDocument mainPage = webForMainPage.Load(pageUrl);
            return mainPage;
        }

        private string GetFlatTitle(HtmlDocument mainPage, int number)
        {
            return mainPage.DocumentNode
                .SelectSingleNode($"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[1]/div[2]/div[1]/a/div/span")
                ?.InnerText ?? "No title";
        }

        private bool IsFlatSuit(string flatTitle, int flatCost)
        {
            return flatTitle != "No title" && flatCost > _flatLowestPrice && flatCost < _flatHighestPrice;
        }

        private string GetFlatOwnerPhoneNumberFromFlatPage(HtmlDocument flatPage)
        {
            return flatPage.DocumentNode.SelectSingleNode(
                "//*[@id=\"main-body\"]/div[2]/div[2]/div[1]/div[1]/div[7]/div/div/div/div[5]/div/div/div[3]/div/div[1]/div[3]/a/span")
                ?.InnerText ?? "No number";
        }

        private string GetFLatLink(HtmlDocument mainPage, string url, int number)
        {
            HtmlNode nextPage = mainPage.DocumentNode.SelectSingleNode($"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[1]/div[2]/div[1]/a");
            var uri = new Uri(url);
            return uri.Scheme + "://" + uri.Authority + nextPage.GetAttributeValue<string>("href", null);
        }

        private List<string> GetFirstTenImagesFromFlatPage(HtmlDocument flatPage)
        {
            var imagesUrl = flatPage.DocumentNode.Descendants("img")
                .Where(e => e.Attributes["class"]?.Value == "img-responsive")
                .Select(e => e.GetAttributeValue("src", null))
                .Where(s => !String.IsNullOrEmpty(s) && !s.Contains("Thumb") && s.Contains("static.ss.ge"))
                .Select(s => Regex.Replace(s, @"(?<!/)/(?!/)", "//"))
                .Take(10)
                .ToList();

            if (imagesUrl.Count == 0)
            {
                var linkBlurredImageIfFlatNotHaveImages =
                    "https://media.istockphoto.com/id/955951212/photo/blurred-background-modern-kitchen-and-dinning-room-in-house-with-bokeh-light-lifestyle-backdrop.jpg?s=612x612&w=0&k=20&c=THHBhrRhOCnD0DdfLj42JNsDuzZpC0oqp7K0EIO4B8U=";
                imagesUrl.Add(linkBlurredImageIfFlatNotHaveImages);
            }

            return imagesUrl;
        }

        private string GetValidDescription(HtmlDocument flatPage, int number, int descriptionLength)
        {
            var input = flatPage.DocumentNode
                .SelectSingleNode(
                    $"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[1]/div[2]/div[2]/div/div[1]/text()")?.InnerText
                .Replace("\r\n", "");

            if (string.IsNullOrWhiteSpace(input))
            {
                input = flatPage.DocumentNode
                    .SelectSingleNode(
                        $"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[1]/div[2]/div[1]/div[5]/text()")?.InnerText
                    .Replace("\r\n", "");
            }

            if (string.IsNullOrWhiteSpace(input)) return "No description";

            if (!new Regex("^[\x20-\x7E]+$").IsMatch(input)) return input;

            var removeWhitespace = Regex.Replace(input, @"\s{2,}", " ");

            if (removeWhitespace.Length <= descriptionLength) return removeWhitespace;

            return string.Concat(removeWhitespace.Substring(0, descriptionLength - 3) + "...");
        }

        private int GetFlatCost(HtmlDocument mainPage, int number)
        {
            var inputCost = mainPage.DocumentNode.SelectSingleNode(
                $"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[2]/div[2]/div[1]/text()")?.InnerText;
            return int.TryParse(inputCost?.Replace(" ", ""), out var result) ? result : int.MaxValue;
        }

        private DateTime GetFlatCreationDateOrMinDate(HtmlDocument page, int number)
        {
            var formatInputDate = "dd.MM.yyyy/HH:mm";
            var minDateInFormat = DateTime.MinValue.ToString(formatInputDate);
            var inputDate = page.DocumentNode.SelectSingleNode(
                    $"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[1]/div[2]/div[2]/div/div[2]/div/div[1]/text()")?
                .InnerText.Replace(" ", "")
                .Replace("\r\n", "");
            if (inputDate == null)
            {
                inputDate = page.DocumentNode.SelectSingleNode(
                        $"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[1]/div[2]/div[2]/div/div/div[1]/text()")?
                    .InnerText.Replace(" ", "")
                    .Replace("\r\n", "") ?? minDateInFormat;
            }

            return DateTime.ParseExact(inputDate, formatInputDate, CultureInfo.InvariantCulture);
        }
    }
}