using System;
using System.Collections.Generic;
using System.Globalization;
using HtmlAgilityPack;

namespace WebScraperApp
{
    public class ScraperSsDotGe
    {
        public List<Info> GetInfos(string url)
        {
            var uri = new Uri(url);
            List<Info> infos = new List<Info>();

            HtmlWeb web = new HtmlWeb();

            var doc = web.Load(url);

            for (int i = 0, j = 0; i < 20; j++)
            {
                var inputTitle = doc.DocumentNode.SelectSingleNode($"//*[@id=\"list\"]/div[{j}]/div[1]/div[1]/div[1]/div[2]/div[1]/a/div/span")?.InnerText;
                if (inputTitle != null)
                {
                    var inputCost = doc.DocumentNode.SelectSingleNode($"//*[@id=\"list\"]/div[{j}]/div[1]/div[1]/div[2]/div[2]/div[1]/text()")?.InnerText ?? "Zero";
                    var cost = int.TryParse(inputCost.Replace(" ", ""), out int number) == true ? number : 0;

                    var inputDate = doc.DocumentNode.SelectSingleNode($"//*[@id=\"list\"]/div[{j}]/div[1]/div[1]/div[1]/div[2]/div[2]/div/div[2]/div/div[1]/text()")?.InnerText.Replace(" ", "").Replace("\r\n","") ?? "Zero";

                    var date = DateTime.ParseExact(inputDate, "dd.MM.yyyy/HH:mm", CultureInfo.InvariantCulture);

                    infos.Add(new Info() { Title = inputTitle, Cost = cost, Data = date });
                    i++;
                    continue;
                }
            }

            return infos;
        }
    }
}
