using System;
using System.Runtime.InteropServices;

namespace WebScraperApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
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
