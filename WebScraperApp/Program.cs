using System;
using WebScraperApp.SsDotGe;

namespace WebScraperApp
{
    internal class Program
    {
        static void Main()
        {
            var htmlScraper = new AdScraperSsDotGe();

            for (var i = 1; i < 10; i++)
            {
                var apartmentPageOne = htmlScraper.ScrapPageWithAllFlats(AdjaraMunicipallyLinksSsDotGe.GetBatumiLink(i));

                apartmentPageOne.ForEach(item =>
                {
                    Console.WriteLine($"{item.Title} " +
                                      $"\n\t Cost: {item.Cost}$" +
                                      $"\n\t Date: {item.Date}" +
                                      $"\n\t Description: {item.Description}" +
                                      $"\n\t AdLink: {item.AdLink}" +
                                      $"\n\t Phone: {item.PhoneNumber}" +
                                      "\n\t");
                });
            }

            for (var i = 1; i < 5; i++)
            {
                var apartmentPageOne = htmlScraper.ScrapPageWithAllFlats(AdjaraMunicipallyLinksSsDotGe.GetKhelvachauriLink(i));

                apartmentPageOne.ForEach(item =>
                {
                    Console.WriteLine($"{item.Title} " +
                                                    $"\n\t Cost: {item.Cost}$" +
                                                    $"\n\t Date: {item.Date}" +
                                                    $"\n\t Description: {item.Description}" +
                                                    $"\n\t AdLink: {item.AdLink}" +
                                                    $"\n\t Phone: {item.PhoneNumber}" +
                                                    "\n\t");
                });
            }

            for (var i = 1; i < 5; i++)
            {
                var apartmentPageOne = htmlScraper.ScrapPageWithAllFlats(AdjaraMunicipallyLinksSsDotGe.GetKobuletiLink(i));

                apartmentPageOne.ForEach(item =>
                {
                    Console.WriteLine($"{item.Title} " +
                                      $"\n\t Cost: {item.Cost}$" +
                                      $"\n\t Date: {item.Date}" +
                                      $"\n\t Description: {item.Description}" +
                                      $"\n\t AdLink: {item.AdLink}" +
                                      $"\n\t Phone: {item.PhoneNumber}" +
                                      "\n\t");
                });
            }

            Console.ReadKey();
        }
    }
}
