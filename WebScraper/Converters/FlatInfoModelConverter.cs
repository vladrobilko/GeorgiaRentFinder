using WebScraper.Models;

namespace WebScraper.Converters
{
    public static class FlatInfoModelConverter
    {
        public static string ToTelegramCaption(this FlatInfoModel flatInfoModel)
        {
            // test flat model
            var images = new List<string>();

            var testFlat = new FlatInfoModel("1 room Flat for rent.  Kobulet",
                300,
                new DateTime(2023, 11, 12),
                " For rent in Kobuleti, 100 meters from the sea, on Davit Aghmashenebeli Street, in Pichvnar, a 36 sq.m. isolated studio apartment. with kitchen, furniture and appliances. With 40-inch LED TV, cable channels, wi-fi, air conditioner. With 24-hour security. This price includes utility bills. ",
               "557 73 72 21",
                images,
                "https://ss.ge/en/real-estate/1-room-flat-for-rent-kobuleti-3320498",
                4089,
                new FlatCoordinate(0, 0));

            // test flat model

            if (testFlat.FlatCoordinate.Latitude == 0 || testFlat.FlatCoordinate.Longitude == 0)
            {
                return $"{testFlat.Title}\n\n" +

                       $"<strong>Cost:</strong> {testFlat.Cost} $\n\n" +

                       $"<strong>Views on site:</strong> {testFlat.ViewsOnSite}\n" +
                       $"<strong>SitePublication of public:</strong> {testFlat.SitePublication}\n" +
                       $"<strong>Description:</strong> {testFlat.Description}\n\n" +
                       
                       $"<strong>Web page:</strong><a href=\"{testFlat.PageLink}\"> link</a>\n" +
                       $"<strong>Mobile phone:</strong> {testFlat.PhoneNumber}\n\n" +
                       $"<strong>Maybe it's a realtor:</strong> <ins>The number was mentioned {testFlat.SitePublication} times</ins>";
            }

            return $"{testFlat.Title}\n\n" +

                          $"<strong>Cost:</strong> {testFlat.Cost} $\n\n" +

                          $"<strong>Views on site:</strong> {testFlat.ViewsOnSite}\n" +
                          $"<strong>SitePublication of public:</strong> {testFlat.SitePublication}\n" +
                          $"<strong>Description:</strong> {testFlat.Description}\n\n" +

                          $"<strong>Location:</strong><a href=\"https://www.google.com/maps/search/?api=1&query={testFlat.FlatCoordinate.Latitude},{testFlat.FlatCoordinate.Longitude}\"> link</a>\n" +
                          $"<strong>Web page:</strong><a href=\"{testFlat.PageLink}\"> link</a>\n" +
                          $"<strong>Mobile phone:</strong> {testFlat.PhoneNumber}\n\n" +
                          $"<strong>Maybe it's a realtor:</strong> <ins>The number was mentioned {testFlat.SitePublication} times</ins>";
        }

        /* private string GetGoodleMapLocation()
         {
             return
         }*/
    }
}
