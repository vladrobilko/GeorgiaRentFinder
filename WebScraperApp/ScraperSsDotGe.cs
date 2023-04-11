using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace WebScraperApp
{
    public class ScraperSsDotGe
    {
        public List<RealtyInfo> GetInfos(string url)
        {
            var infos = new List<RealtyInfo>();

            var web = new HtmlWeb();

            var doc = web.Load(url);

            for (int i = 0, j = 0; i < 20; j++)
            {
                var inputTitle = GetTitle(doc, j);

                if (inputTitle != null)
                {
                    var inputDescription = GetValidDescriptionForShow(doc, j, 150);

                    var cost = GetCost(doc, j);

                    if (cost == 0 || cost > 351)
                    {
                        i++;
                        continue;
                    }

                    var date = GetDate(doc, j);

                    infos.Add(new RealtyInfo()
                    {
                        Title = inputTitle,
                        Cost = cost,
                        Data = date,
                        Description = inputDescription
                    });
                    i++;
                }
            }

            return infos;
        }

        public List<string> GetImages(string linkAd)
        {
            HtmlWeb web = new HtmlWeb();

            var doc = web.Load("https://ss.ge/en/real-estate/3-room-flat-for-sale-saburtalo-6644529"); // link of ad

            var imagesUrl = doc.DocumentNode.Descendants("img")
                .Where(e => e.Attributes["class"]?.Value == "img-responsive")
                .Select(e => e.GetAttributeValue("src", null))
                .Where(s => !String.IsNullOrEmpty(s) && !s.Contains("Thumb") && s.Contains("static.ss.ge"))
                .ToList();
            return imagesUrl;
        }

        private string GetTitle(HtmlDocument doc, int number)
        {
            return doc.DocumentNode
                .SelectSingleNode($"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[1]/div[2]/div[1]/a/div/span")
                ?.InnerText;
        }

        private string GetValidDescriptionForShow(HtmlDocument doc, int number, int descriptionLength)
        {
            var input = doc.DocumentNode
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

        private int GetCost(HtmlDocument doc, int number)
        {
            var inputCost = doc.DocumentNode.SelectSingleNode(
                $"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[2]/div[2]/div[1]/text()")?.InnerText;
            return int.TryParse(inputCost?.Replace(" ", ""), out var result) ? result : 0;
        }

        private DateTime GetDate(HtmlDocument doc, int number)
        {
            var inputDate = doc.DocumentNode.SelectSingleNode(
                    $"//*[@id=\"list\"]/div[{number}]/div[1]/div[1]/div[1]/div[2]/div[2]/div/div[2]/div/div[1]/text()")?
                .InnerText.Replace(" ", "")
                .Replace("\r\n", "") ?? "Zero";

            return DateTime.ParseExact(inputDate, "dd.MM.yyyy/HH:mm", CultureInfo.InvariantCulture);
        }
    }
}
