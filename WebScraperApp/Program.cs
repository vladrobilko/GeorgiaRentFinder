using System;

namespace WebScraperApp
{
    internal class Program
    {
        static void Main()
        {
            var htmlScraper = new ScraperSsDotGe();

            for (var i = 1; i < 10; i++)
            {
                var apartmentPageOne = htmlScraper.GetInfos($"https://ss.ge/en/real-estate/l/For-Rent?Page={i}&RealEstateDealTypeId=1&BaseUrl=/en/real-estate/l&CurrentUserId=&Query=&MunicipalityId=5&CityIdList=248,257,234,60,267,256,252,225,230,227,229,247,266,232,259,246,260,265,264,268,261,14,251,245,231,233,238,270,236,239,242,269,249,224,262,3734,243,226,254,240,253,263,65,255,241,258,244,228,237,250,235,362,336,3676,343,382,393,392,391,370,361,3717,385,3770,344,372,109,337,356,375,366,348,46,342,338,355,358,369,379,116,368,367,378,347,384,16,346,377,390,381,374,353,352,341,383,376,359,389,126,339,340,351,373,360,354,364,365,349,380,371,3761,386,345,394,363,387,357,350,388&IsMap=false&subdistr=&stId=&PrcSource=1&RealEstateStatus=&CommercialRealEstateType=&HouseWillHaveToLive=&QuantityFrom=&QuantityTo=&PriceType=false&CurrencyId=1&PriceFrom=&PriceTo=&Context.Request.Query[Query]=&FloorType=&Balcony_Loggia=&Toilet=&Project=&Other=&State=&AdditionalInformation=&ConstructionAgencyId=&AgencyId=&VipStatus=&PageSize=20&Sort.SortExpression=%221%22&WIth360Image=false&IsConstruction=false&WithImageOnly=false&IndividualEntityOnly=false&IsPetFriendly=false&IsForUkraine=false");

                apartmentPageOne.ForEach(item =>
                {
                    Console.WriteLine($"{item.Title} " +
                                      $"\n\t Cost: {item.Cost}$" +
                                      $"\t Date: {item.Data}" +
                                      $"\n Description: {item.Description}" +
                                      "\n");
                });
            }

            Console.ReadKey();
        }
    }
}
