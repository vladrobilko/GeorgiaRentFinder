using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using HtmlAgilityPack;

namespace WebScraperApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            HtmlWeb web = new HtmlWeb();

            var doc = web.Load("https://ss.ge/en/real-estate/3-room-flat-for-rent-kobuleti-5190457");



            var urls = doc.DocumentNode.Descendants("img")
                .Where(e => e.Attributes["class"]?.Value == "img-responsive")
                .Select(e => e.GetAttributeValue("src", null))
                .Where(s => !String.IsNullOrEmpty(s) && !s.Contains("Thumb") && s.Contains("static.ss.ge"))
                .ToList();
            /*
             * var urls = document.DocumentNode.Descendants("img")
                                .Where(e => e.Attributes["class"]?.Value == "my-class" 
                                            && !string.IsNullOrEmpty(e.Attributes["alt"]?.Value))
                                .Select(e => e.GetAttributeValue("src", null));
             */


            var htmlScraper = new ScraperSsDotGe();

            var urlPageOne = "https://ss.ge/en/real-estate/l/For-Rent?Page=1&RealEstateDealTypeId=1&MunicipalityId=3&CityIdList=14&PriceType=false&CurrencyId=1";

            var apartmentPageOne = htmlScraper.GetInfos(urlPageOne);

            apartmentPageOne.ForEach(item =>
            {
                if (item.Cost < 351)
                {
                    Console.WriteLine($"{item.Title} " +
                                      $"\n\t Cost: {item.Cost}$" +
                                      $" \n\t Date: {item.Data}");
                }
            });

            var urlPageTwo = "https://ss.ge/en/real-estate/l/For-Rent?Page=2&RealEstateDealTypeId=1&MunicipalityId=3&CityIdList=14&PriceType=false&CurrencyId=1";

            var apartmentPageOTwo = htmlScraper.GetInfos(urlPageTwo);

            apartmentPageOTwo.ForEach(item =>
            {
                if (item.Cost < 351)
                {
                    Console.WriteLine($"{item.Title} " +
                                      $"\n\t Cost: {item.Cost}$" +
                                      $" \n\t Date: {item.Data}");
                }
            });

            Console.ReadLine();
        }
    }
}
