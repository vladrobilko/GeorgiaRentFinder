using Application.Models;
using WebScraper;
using WebScraper.Converters;

namespace Application.Converters
{
    public static class FlatInfoClientModelConverter
    {
        private const long CountForRealtorDetection = 10;

        public static string ToTelegramCaption(this FlatInfoClientModel flat)
        {
            var caption = IsCoordinateExist(flat.FlatCoordinateClientModel) ? GetCaptionWithCoordinate(flat) : GetCaptionWithOutCoordinate(flat);

            if (flat.FlatPhoneClientModel.MentionOnSite > CountForRealtorDetection && flat.FlatPhoneClientModel.PhoneNumber != "No number")
            {
                caption += GetRealtorDescription(flat.FlatPhoneClientModel.MentionOnSite);
            }

            return caption;
        }

        private static bool IsCoordinateExist(FlatCoordinateClientModel flatCoordinate)
        {
            return flatCoordinate.Latitude != 0 && flatCoordinate.Longitude != 0;
        }

        private static string GetGoogleMapLocation(double latitude, double longitude)
        {
            return $"https://www.google.com/maps/search/?api=1&query={latitude},{longitude}";
        }

        private static string GetCaptionWithCoordinate(FlatInfoClientModel flat)
        {
            return $"{flat.Title}\n\n" +

                 $"<strong>Cost:</strong> {flat.Cost} $ {GetCostInGelOrEmptyDescription(flat.Cost)}\n\n" +

                 $"<strong>Views on site:</strong> {flat.ViewsOnSite}\n" +
                 $"<strong>Published:</strong> {flat.SitePublication.ToCommonViewString()}\n" +
                 $"<strong>Description:</strong> {flat.Description}\n\n" +

                 $"<strong>Location:</strong><a href=\"{GetGoogleMapLocation(flat.FlatCoordinateClientModel.Latitude, flat.FlatCoordinateClientModel.Longitude)}\"> link</a>\n" +
                 $"<strong>Web page:</strong><a href=\"{flat.PageLink}\"> link</a>\n" +
                 $"<strong>Mobile phone:</strong> {flat.FlatPhoneClientModel.PhoneNumber}\n\n";
        }

        private static string GetCaptionWithOutCoordinate(FlatInfoClientModel flat)
        {
            return $"{flat.Title}\n\n" +

                   $"<strong>Cost:</strong> {flat.Cost} $ {GetCostInGelOrEmptyDescription(flat.Cost)}\n\n" +

                   $"<strong>Views on site:</strong> {flat.ViewsOnSite}\n" +
                   $"<strong>Published:</strong> {flat.SitePublication.ToCommonViewString()}\n" +
                   $"<strong>Description:</strong> {flat.Description}\n\n" +

                   $"<strong>Web page:</strong><a href=\"{flat.PageLink}\"> link</a>\n" +
                   $"<strong>Mobile phone:</strong> {flat.FlatPhoneClientModel.PhoneNumber}\n\n";
        }

        private static string GetCostInGelOrEmptyDescription(long cost)
        {
            var usdToGel = UsdToGelCourseScraper.GetGelInOneDollarFromGeorgiaNationalBank();

            if (usdToGel == double.MaxValue) return "";

            return $"({Convert.ToInt32(usdToGel * cost)} ლ)";
        }

        private static string GetRealtorDescription(long mentionOnSite)
        {
            return $"<strong>Maybe it's a realtor:</strong> <ins>The number was mentioned {mentionOnSite} times</ins>";
        }
    }
}
