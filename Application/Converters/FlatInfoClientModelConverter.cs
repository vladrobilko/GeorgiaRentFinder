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
            var caption = GetCaptionWithOutCoordinateAndRealtor(flat);

            if (IsCoordinateExist(flat.FlatCoordinateClientModel)) caption += GetCoordinateOrEmptyDescription(flat);

            if (IsItRealtor(flat)) caption += GetRealtorDescription(flat.FlatPhoneClientModel.MentionOnSite);

            return caption;
        }

        private static string GetCaptionWithOutCoordinateAndRealtor(FlatInfoClientModel flat)
        {
            return $"{flat.Title}\n\n" +

                 $"<strong>Cost:</strong> {flat.Cost} $ {GetCostInGelOrEmptyDescription(flat.Cost)}\n\n" +

                 $"<strong>Views on site:</strong> {flat.ViewsOnSite}\n" +
                 $"<strong>Published:</strong> {flat.SitePublication.ToCommonViewString()}\n" +
                 $"<strong>Description:</strong> {flat.Description}\n\n" +

                 $"<strong>Web page:</strong><a href=\"{flat.PageLink}\"> link</a>\n" +
                 $"<strong>Mobile phone:</strong> {flat.FlatPhoneClientModel.PhoneNumber}\n";
        }

        private static string GetCostInGelOrEmptyDescription(long cost)
        {
            var usdToGel = UsdToGelCourseScraper.GetGelInOneDollarFromGeorgiaNationalBank();

            if (usdToGel == double.MaxValue) return "";

            return $"({Convert.ToInt32(usdToGel * cost)} ლ)";
        }

        private static string GetCoordinateOrEmptyDescription(FlatInfoClientModel flat)
        {
            return
                $"<strong>Location:</strong><a href=\"{GetGoogleMapLocation(flat.FlatCoordinateClientModel.Latitude, flat.FlatCoordinateClientModel.Longitude)}\"> link</a>\n";

        }

        private static bool IsCoordinateExist(FlatCoordinateClientModel flatCoordinate)
        {
            return flatCoordinate.Latitude != 0 && flatCoordinate.Longitude != 0;
        }

        private static string GetGoogleMapLocation(double latitude, double longitude)
        {
            return $"https://www.google.com/maps/search/?api=1&query={latitude},{longitude}";
        }

        private static bool IsItRealtor(FlatInfoClientModel flat)
        {
            return flat.FlatPhoneClientModel.MentionOnSite > CountForRealtorDetection &&
                   flat.FlatPhoneClientModel.PhoneNumber != "No number";
        }

        private static string GetRealtorDescription(long mentionOnSite)
        {
            return $"<strong>Maybe it's a realtor:</strong> <ins>The number was mentioned {mentionOnSite} times</ins>";
        }
    }
}
