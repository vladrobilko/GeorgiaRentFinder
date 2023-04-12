using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace WebScraperApp
{
    public class AdScraperSsDotGe
    {
        public List<RealtyInfo> ScrapPageWithAllApartments(string url)
        {
            var infos = new List<RealtyInfo>();

            var web = new HtmlWeb();

            var doc = web.Load(url);

            var adsOnPage = 20;

            for (int i = 0, j = 0; i < adsOnPage; j++)
            {
                var inputTitle = GetTitle(doc, j);

                if (inputTitle != null)
                {
                    var cost = GetCost(doc, j);

                    if (cost == 0 || cost > 351)
                    {
                        i++;
                        continue;
                    }

                    var creationDate = GetAdCreationDate(doc, j);

                    var nextPageUrl = GetOwnLinkToAd(doc, j, url);

                    var number = GetNumber(nextPageUrl);

                    infos.Add(new RealtyInfo(inputTitle, cost, creationDate, GetValidDescriptionForShow(doc, j, 150),
                        nextPageUrl, GetFirstTenImages(nextPageUrl), number));
                    i++;
                }
            }

            return infos;
        }

        private string GetNumber(string linkMainAdPage)
        {
            var webForPhone = new HtmlWeb();

            var document = webForPhone.Load(linkMainAdPage);

            return document.DocumentNode.SelectSingleNode(
                "//*[@id=\"main-body\"]/div[2]/div[2]/div[1]/div[1]/div[7]/div/div/div/div[5]/div/div/div[3]/div/div[1]/div[3]/a/span")
                ?.InnerText ?? "No number";
        }

        private string GetOwnLinkToAd(HtmlDocument document, int number, string url)
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

        private string GetTitle(HtmlDocument document, int number)
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
                return "No description";
            }

            var removeWhitespace = Regex.Replace(input, @"\s{2,}", " ");

            if (removeWhitespace.Length <= descriptionLength) return removeWhitespace;

            return string.Concat(removeWhitespace.Substring(0, descriptionLength - 3) + "...");
        }

        private int GetCost(HtmlDocument document, int number)
        {
            var inputCost = document.DocumentNode.SelectSingleNode(
                $"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[2]/div[2]/div[1]/text()")?.InnerText;
            return int.TryParse(inputCost?.Replace(" ", ""), out var result) ? result : 0;
        }

        private DateTime GetAdCreationDate(HtmlDocument document, int number)
        {
            var inputDate = document.DocumentNode.SelectSingleNode(
                    $"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[1]/div[2]/div[2]/div/div[2]/div/div[1]/text()")?
                .InnerText.Replace(" ", "")
                .Replace("\r\n", "") ?? "Zero";

            return DateTime.ParseExact(inputDate, "dd.MM.yyyy/HH:mm", CultureInfo.InvariantCulture);
        }
    }
}
