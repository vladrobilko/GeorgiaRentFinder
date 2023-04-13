using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace WebScraperApp.SsDotGe
{
    public class AdScraperSsDotGe
    {
        private const int FlatsOnPage = 20;
        private const int FlatLowestPrice = 70;
        private const int FlatHighestPrice = 360;

        public List<FlatInfo> ScrapPageWithAllFlats(string url)
        {
            var flats = new List<FlatInfo>();

            for (int i = 0, j = 0; i < FlatsOnPage; j++, i++)
            {
                var flatTitle = GetFlatTitle(url, j);

                var flatCost = GetFlatCost(url, j);

                if (IsFlatValid(flatTitle, flatCost))
                {
                    var flatLink = GetFLatLink(url, j);

                    var flatCreationDate = GetFlatCreationDate(flatLink);

                    var flatOwnerPhoneNumber = GetFlatOwnerPhoneNumber(flatLink);

                    flats.Add(new FlatInfo(
                        flatTitle,
                        flatCost,
                        flatCreationDate,
                        GetValidDescriptionForShow(url, j, 150),
                        flatOwnerPhoneNumber,
                        GetFirstTenImages(flatLink),
                        flatLink,
                        10000));// ad views
                }
            }

            return flats;
        }

        public bool IsFlatValid(string flatTitle, int flatCost)
        {
            return flatTitle != null && flatCost > FlatLowestPrice && flatCost < FlatHighestPrice;
        }

        private string GetFlatOwnerPhoneNumber(string linkMainAdPage)
        {
            var webForPhone = new HtmlWeb();

            var document = webForPhone.Load(linkMainAdPage);

            return document.DocumentNode.SelectSingleNode(
                "//*[@id=\"main-body\"]/div[2]/div[2]/div[1]/div[1]/div[7]/div/div/div/div[5]/div/div/div[3]/div/div[1]/div[3]/a/span")
                ?.InnerText ?? "No number";
        }

        private string GetFLatLink(string url, int number)
        {
            var htmlWeb = new HtmlWeb();
            var htmlDocument = htmlWeb.Load(url);

            HtmlNode nextPage = htmlDocument.DocumentNode.SelectSingleNode($"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[1]/div[2]/div[1]/a");
            var uri = new Uri(url);
            return uri.Scheme + "://" + uri.Authority + nextPage.GetAttributeValue<string>("href", null);
        }

        private List<string> GetFirstTenImages(string linkAd)
        {
            HtmlWeb web = new HtmlWeb();

            var doc = web.Load(linkAd);

            var imagesUrl = doc.DocumentNode.Descendants("img")
                .Where(e => e.Attributes["class"]?.Value == "img-responsive")
                .Select(e => e.GetAttributeValue("src", null))
                .Where(s => !String.IsNullOrEmpty(s) && !s.Contains("Thumb") && s.Contains("static.ss.ge"))
                .Take(10).ToList();

            return imagesUrl;
        }

        private string GetFlatTitle(string url, int number)
        {
            var htmlWeb = new HtmlWeb();
            var htmlDocument = htmlWeb.Load(url);

            return htmlDocument.DocumentNode
                .SelectSingleNode($"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[1]/div[2]/div[1]/a/div/span")
                ?.InnerText;
        }

        private string GetValidDescriptionForShow(string url, int number, int descriptionLength)
        {
            var htmlWeb = new HtmlWeb();
            var htmlDocument = htmlWeb.Load(url);

            var input = htmlDocument.DocumentNode
                .SelectSingleNode(
                    $"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[1]/div[2]/div[2]/div/div[1]/text()")?.InnerText
                .Replace("\r\n", "");

            if (string.IsNullOrWhiteSpace(input))
            {
                input = htmlDocument.DocumentNode
                    .SelectSingleNode(
                        $"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[1]/div[2]/div[1]/div[5]/text()")?.InnerText
                    .Replace("\r\n", "");
            }

            if (string.IsNullOrWhiteSpace(input) || !(new Regex("^[\x20-\x7E]+$").IsMatch(input))) return "No description";

            var removeWhitespace = Regex.Replace(input, @"\s{2,}", " ");

            if (removeWhitespace.Length <= descriptionLength) return removeWhitespace;

            return string.Concat(removeWhitespace.Substring(0, descriptionLength - 3) + "...");
        }

        private int GetFlatCost(string url, int number)
        {
            var htmlWeb = new HtmlWeb();
            var htmlDocument = htmlWeb.Load(url);

            var inputCost = htmlDocument.DocumentNode.SelectSingleNode(
                $"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[2]/div[2]/div[1]/text()")?.InnerText;
            return int.TryParse(inputCost?.Replace(" ", ""), out var result) ? result : 0;
        }

        private DateTime GetFlatCreationDate(string flatLink)// test it
        {
            var htmlWeb = new HtmlWeb();
            var htmlDocument = htmlWeb.Load(flatLink);
            var formatInputDate = "dd.MM.yyyy/HH:mm";
            var maxDateInFormat = "31.12.9999/23:59";
            var inputDate = htmlDocument.DocumentNode.SelectSingleNode(
                    "//*[@id=\"main-body\"]/div[2]/div[2]/div[1]/div[1]/div[6]/div/div[1]/div[2]/div[2]/text()")?
                .InnerText.Replace(" ", "")
                .Replace("\r\n", "") ?? maxDateInFormat;

            return DateTime.ParseExact(inputDate, formatInputDate, CultureInfo.InvariantCulture);
        }
    }
}
