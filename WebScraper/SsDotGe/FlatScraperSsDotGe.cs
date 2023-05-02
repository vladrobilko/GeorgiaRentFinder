using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using WebScraper.Models;

namespace WebScraper.SsDotGe
{
    public class FlatScraperSsDotGe
    {
        private const int FlatsOnPage = 20;

        private const int FlatLowestPrice = 70;

        private const int FlatHighestPrice = 360;

        public List<FlatInfoModel> ScrapPageWithAllFlats(string url, DateTime lastCheckDate)
        {
            var flats = new List<FlatInfoModel>();

            HtmlDocument mainPage = GetHtmlDocumentForPage(url);

            for (int i = 0, j = 0; i < FlatsOnPage; j++)
            {
                if (j > FlatsOnPage * 2) break;

                var flatCreationDate = GetFlatCreationDateFromFlatPage(mainPage, j);

                if (flatCreationDate < lastCheckDate) break;

                var flatTitle = GetFlatTitleFromMainPage(mainPage, j);

                var flatCost = GetFlatCostFromMainPage(mainPage, j);

                if (IsFlatSuit(flatTitle, flatCost))
                {
                    var flatLink = GetFLatLinkFromMainPage(mainPage, url, j);

                    var flatDescription = GetValidDescriptionFromMainPage(mainPage, j, 200);

                    flats.Add(GetFlatPage(flatLink, flatTitle, flatCost, flatDescription, flatCreationDate));
                    i++;
                }

                if (flatTitle != null && !IsFlatSuit(flatTitle, flatCost)) i++;
            }

            return flats;
        }

        private FlatInfoModel GetFlatPage(string flatLink, string flatTitle, int flatCost, string flatDescription, DateTime flatCreationDate)
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

        private string GetFlatTitleFromMainPage(HtmlDocument mainPage, int number)
        {
            return mainPage.DocumentNode
                .SelectSingleNode($"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[1]/div[2]/div[1]/a/div/span")
                ?.InnerText;
        }

        private bool IsFlatSuit(string flatTitle, int flatCost)
        {
            return flatTitle != null && flatCost > FlatLowestPrice && flatCost < FlatHighestPrice;
        }

        private string GetFlatOwnerPhoneNumberFromFlatPage(HtmlDocument flatPage)
        {
            return flatPage.DocumentNode.SelectSingleNode(
                "//*[@id=\"main-body\"]/div[2]/div[2]/div[1]/div[1]/div[7]/div/div/div/div[5]/div/div/div[3]/div/div[1]/div[3]/a/span")
                ?.InnerText ?? "No number";
        }

        private string GetFLatLinkFromMainPage(HtmlDocument mainPage, string url, int number)
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
                .Take(10).ToList();

            return imagesUrl;
        }

        private string GetValidDescriptionFromMainPage(HtmlDocument flatPage, int number, int descriptionLength)
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

            if (string.IsNullOrWhiteSpace(input) || !(new Regex("^[\x20-\x7E]+$").IsMatch(input))) return "No description";

            var removeWhitespace = Regex.Replace(input, @"\s{2,}", " ");

            if (removeWhitespace.Length <= descriptionLength) return removeWhitespace;

            return string.Concat(removeWhitespace.Substring(0, descriptionLength - 3) + "...");
        }

        private int GetFlatCostFromMainPage(HtmlDocument mainPage, int number)
        {
            var inputCost = mainPage.DocumentNode.SelectSingleNode(
                $"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[2]/div[2]/div[1]/text()")?.InnerText;
            return int.TryParse(inputCost?.Replace(" ", ""), out var result) ? result : int.MaxValue;
        }

        private DateTime GetFlatCreationDateFromFlatPage(HtmlDocument page, int number)
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