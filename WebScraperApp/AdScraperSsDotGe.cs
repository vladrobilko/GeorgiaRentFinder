using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace WebScraperApp
{
    public class AdScraperSsDotGe
    {
        private const int FlatsOnPage = 20;

        public AdScraperSsDotGe()
        {
            
        }

        public List<FlatInfo> ScrapPageWithAllFlats(string url)
        {
            var flats = new List<FlatInfo>();

            var htmlWeb = new HtmlWeb();

            var htmlDocument = htmlWeb.Load(url);

            for (int i = 0, j = 0; i < FlatsOnPage; j++)
            {
                var flatTitle = GetFlatTitle(htmlDocument, j);

                if (flatTitle != null)
                {
                    var flatCost = GetFlatCost(htmlDocument, j);

                    if (flatCost < 70 || flatCost > 351)
                    {
                        i++;
                        continue;
                    }

                    var flatCreationDate = GetFlatCreationDate(htmlDocument, j);

                    var flatLink = GetOwnLinkToFLat(htmlDocument, j, url);

                    var flatOwnerPhoneNumber = GetFlatOwnerPhoneNumber(flatLink);

                    flats.Add(new FlatInfo(
                        flatTitle,
                        flatCost,
                        flatCreationDate,
                        GetValidDescriptionForShow(htmlDocument, j, 150),
                        flatOwnerPhoneNumber, GetFirstTenImages(flatLink), flatLink));
                    i++;
                }
            }

            return flats;
        }

        private string GetFlatOwnerPhoneNumber(string linkMainAdPage)
        {
            var webForPhone = new HtmlWeb();

            var document = webForPhone.Load(linkMainAdPage);

            return document.DocumentNode.SelectSingleNode(
                "//*[@id=\"main-body\"]/div[2]/div[2]/div[1]/div[1]/div[7]/div/div/div/div[5]/div/div/div[3]/div/div[1]/div[3]/a/span")
                ?.InnerText ?? "No number";
        }

        private string GetOwnLinkToFLat(HtmlDocument document, int number, string url)
        {
            HtmlNode nextPage = document.DocumentNode.SelectSingleNode($"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[1]/div[2]/div[1]/a");
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

        private string GetFlatTitle(HtmlDocument document, int number)
        {
            return document.DocumentNode
                .SelectSingleNode($"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[1]/div[2]/div[1]/a/div/span")
                ?.InnerText;
        }

        private string GetValidDescriptionForShow(HtmlDocument document, int number, int descriptionLength)
        {
            var input = document.DocumentNode
                .SelectSingleNode(
                    $"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[1]/div[2]/div[2]/div/div[1]/text()")?.InnerText
                .Replace("\r\n", "");

            if (string.IsNullOrWhiteSpace(input))
            {
                input = document.DocumentNode
                    .SelectSingleNode(
                        $"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[1]/div[2]/div[1]/div[5]/text()")?.InnerText
                    .Replace("\r\n", "");
            }

            if (string.IsNullOrWhiteSpace(input) || !(new Regex("^[\x20-\x7E]+$").IsMatch(input))) return "No description";

            var removeWhitespace = Regex.Replace(input, @"\s{2,}", " ");

            if (removeWhitespace.Length <= descriptionLength) return removeWhitespace;

            return string.Concat(removeWhitespace.Substring(0, descriptionLength - 3) + "...");
        }

        private int GetFlatCost(HtmlDocument document, int number)
        {
            var inputCost = document.DocumentNode.SelectSingleNode(
                $"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[2]/div[2]/div[1]/text()")?.InnerText;
            return int.TryParse(inputCost?.Replace(" ", ""), out var result) ? result : 0;
        }

        private DateTime GetFlatCreationDate(HtmlDocument document, int number)
        {
            var inputDate = document.DocumentNode.SelectSingleNode(
                    $"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[1]/div[2]/div[2]/div/div[2]/div/div[1]/text()")?
                .InnerText.Replace(" ", "")
                .Replace("\r\n", "");
            if (inputDate == null)
            {
                inputDate = document.DocumentNode.SelectSingleNode(
                        $"//*[@id=\"list\"]/div[{number + 1}]/div[1]/div[1]/div[1]/div[2]/div[2]/div/div/div[1]/text()")?
                    .InnerText.Replace(" ", "")
                    .Replace("\r\n", "") ?? "01.01.0001/00:00";
            }

            return DateTime.ParseExact(inputDate, "dd.MM.yyyy/HH:mm", CultureInfo.InvariantCulture);
        }
    }
}
