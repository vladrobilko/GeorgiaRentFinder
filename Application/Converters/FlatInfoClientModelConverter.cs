using Application.Models;
using WebScraper.Converters;
using WebScraper.Models;
using static System.Net.WebRequestMethods;

namespace Application.Converters
{
    public static class FlatInfoClientModelConverter
    {
        public static string ToTelegramCaption(this FlatInfoClientModel flat)
        {
            if (flat.FlatCoordinateClientModel.Latitude == 0 || flat.FlatCoordinateClientModel.Longitude == 0)
            {
                return $"{flat.Title}\n\n" +

                       $"<strong>Cost:</strong> {flat.Cost} $\n\n" +

                       $"<strong>Views on site:</strong> {flat.ViewsOnSite}\n" +
                       $"<strong>Published:</strong> {flat.SitePublication.ToCommonViewString()}\n" +
                       $"<strong>Description:</strong> {flat.Description}\n\n" +

                       $"<strong>Web page:</strong><a href=\"{flat.PageLink}\"> link</a>\n" +
                       $"<strong>Mobile phone:</strong> {flat.FlatPhoneClientModel.PhoneNumber}\n\n" +
                       $"<strong>Maybe it's a realtor:</strong> <ins>The number was mentioned {flat.FlatPhoneClientModel.MentionOnSite} times</ins>";
            }

            return $"{flat.Title}\n\n" +

                   $"<strong>Cost:</strong> {flat.Cost} $\n\n" +

                   $"<strong>Views on site:</strong> {flat.ViewsOnSite}\n" +
                   $"<strong>Published:</strong> {flat.SitePublication.ToCommonViewString()}\n" +
                   $"<strong>Description:</strong> {flat.Description}\n\n" +

                   $"<strong>Location:</strong><a href=\"{GetGoogleMapLocation(flat.FlatCoordinateClientModel.Latitude, flat.FlatCoordinateClientModel.Longitude)}\"> link</a>\n" +
                   $"<strong>Web page:</strong><a href=\"{flat.PageLink}\"> link</a>\n" +
                   $"<strong>Mobile phone:</strong> {flat.FlatPhoneClientModel.PhoneNumber}\n\n" +
                   $"<strong>Maybe it's a realtor:</strong> <ins>The number was mentioned {flat.FlatPhoneClientModel.MentionOnSite} times</ins>";
        }

        private static string GetGoogleMapLocation(double latitude, double longitude)
        {
            return $"https://www.google.com/maps/search/?api=1&query={latitude},{longitude}";
        }
    }
}
