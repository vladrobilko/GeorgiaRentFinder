﻿using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace WebScraper.SsDotGe
{
    public class AdScraperSsDotGe
    {
        private const int FlatsOnPage = 20;

        private const int FlatLowestPrice = 70;

        private const int FlatHighestPrice = 360;

        public List<FlatInfoModel> ScrapPageWithAllFlats(string url)
        {
            var flats = new List<FlatInfoModel>();

            HtmlDocument mainPage = GetHtmlDocumentForPage(url);

            for (int i = 0, j = 0; i < FlatsOnPage; j++)
            {
                var flatTitle = GetFlatTitleFromMainPage(mainPage, j);

                var flatCost = GetFlatCostFromMainPage(mainPage, j);

                if (IsFlatSuit(flatTitle, flatCost))
                {
                    var flatLink = GetFLatLinkFromMainPage(mainPage, url, j);

                    var flatDescription = GetValidDescriptionFromMainPage(mainPage, j, 200);

                    flats.Add(GetFlatPage(flatLink, flatTitle, flatCost, flatDescription));
                    i++;
                }

                if (flatTitle != null && !IsFlatSuit(flatTitle, flatCost)) i++;
            }

            return flats;
        }

        private FlatInfoModel GetFlatPage(string flatLink, string flatTitle, int flatCost, string flatDescription)
        {
            HtmlDocument flatPage = GetHtmlDocumentForPage(flatLink);

            var flatCreationDate = GetFlatCreationDateFromFlatPage(flatPage);

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
                null
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
        
        public bool IsFlatSuit(string flatTitle, int flatCost)
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

        private DateTime GetFlatCreationDateFromFlatPage(HtmlDocument flatPage)
        {
            var formatInputDate = "dd.MM.yyyy/HH:mm";
            var maxDateInFormat = "31.12.9999/23:59";
            var inputDate = flatPage.DocumentNode.SelectSingleNode(
                    "//*[@id=\"main-body\"]/div[2]/div[2]/div[1]/div[1]/div[6]/div/div[1]/div[2]/div[2]/text()")?
                .InnerText.Replace(" ", "")
                .Replace("\r\n", "") ?? maxDateInFormat;

            return DateTime.ParseExact(inputDate, formatInputDate, CultureInfo.InvariantCulture);
        }
    }
}
