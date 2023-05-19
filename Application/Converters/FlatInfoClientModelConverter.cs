using Application.Models;
using System.Text.RegularExpressions;
using WebScraper;
using WebScraper.Converters;

namespace Application.Converters
{
    public static class FlatInfoClientModelConverter
    {
        private const long CountForRealtorDetection = 10;

        public static string ToTelegramCaptionWithDefaultLanguage(this FlatInfoClientModel flat, bool isForAdmin)
        {
            var caption = GetCaptionWithOutCoordinateAndRealtor(flat);

            if (isForAdmin) caption += $"\nID in database - {flat.Id}";

            return caption;
        }

        public static string ToTelegramCaptionWithRussianLanguage(this FlatInfoClientModel flat, bool isForAdmin, string language = null, string apiToken = null)
        {
            var caption = GetCaptionWithOutCoordinateAndRealtor(flat, language, apiToken);

            if (isForAdmin) caption += $"\nID in database - {flat.Id}";

            return caption;
        }

        private static string GetCaptionWithOutCoordinateAndRealtor(FlatInfoClientModel flat, string language = null, string apiToken = null)
        {
            if (language == "ru")
            {
                return $"{flat.Title.Translate(language, apiToken)}" +
                       $"\n\n<strong>Цена:</strong> {flat.Cost} $ {GetCostInGelOrEmptyDescribe(flat.Cost)}" +
                       $"\n<strong>Опубликовано:</strong> {flat.SitePublication.ToCommonViewString()}" +
                       $"\n<strong>Просмотры на сайте:</strong> {flat.ViewsOnSite}" +
                       $"{GetDescriptionOrEmptyString(flat.Description, language, apiToken)}" +
                       $"\n\n<strong>Сайт:</strong><a href=\"{flat.PageLink}\"> link</a>" +
                       $"{GetCoordinateOrEmptyDescribe(flat, "ru")}" +
                       $"\n<strong>Телефон:</strong> {flat.FlatPhoneClientModel.PhoneNumber.Translate(language, apiToken)}" +
                       $"{GetRealtorDescribe(flat, flat.FlatPhoneClientModel.MentionOnSite, "ru")}";
            }

            return $"{flat.Title}" +
                   $"\n\n<strong>Cost:</strong> {flat.Cost} $ {GetCostInGelOrEmptyDescribe(flat.Cost)}" +
                   $"\n<strong>Published:</strong> {flat.SitePublication.ToCommonViewString()}" +
                   $"\n<strong>Views on site:</strong> {flat.ViewsOnSite}" +
                   $"{GetDescriptionOrEmptyString(flat.Description)}" +
                   $"\n\n<strong>Web page:</strong><a href=\"{flat.PageLink}\"> link</a>" +
                   $"{GetCoordinateOrEmptyDescribe(flat)}" +
                   $"\n<strong>Mobile phone:</strong> {flat.FlatPhoneClientModel.PhoneNumber}" +
                   $"{GetRealtorDescribe(flat, flat.FlatPhoneClientModel.MentionOnSite, "ru")}";
        }

        private static string GetRealtorDescribe(FlatInfoClientModel flat, long mentionOnSite, string language = null)
        {
            if (!IsItRealtor(flat)) return "";

            if (language == "ru")
            {
                return $"\n<strong>Возможно это риелтор:</strong> <ins>Номер упоминался {mentionOnSite} раз</ins>";
            }

            return $"\n<strong>Maybe it's a realtor:</strong> <ins>The number was mentioned {mentionOnSite} times</ins>";
        }

        private static string GetCoordinateOrEmptyDescribe(FlatInfoClientModel flat, string language = null)
        {
            if (!IsCoordinateExist(flat.FlatCoordinateClientModel)) return "";

            if (language == "ru")
            {
                return
                    $"\n<strong>Локация:</strong><a href=\"{GetGoogleMapLocation(flat.FlatCoordinateClientModel.Latitude, flat.FlatCoordinateClientModel.Longitude)}\"> link</a>";
            }
            return
                $"\n<strong>Location:</strong><a href=\"{GetGoogleMapLocation(flat.FlatCoordinateClientModel.Latitude, flat.FlatCoordinateClientModel.Longitude)}\"> link</a>";
        }

        private static string GetDescriptionOrEmptyString(string description, string language = null, string apiToken = null)
        {
            if (description == "No description") return "";

            if (language == null) return $"\n<strong>Description:</strong> {description}";

            return
                $"\n<strong>Описание:</strong> {Regex.Replace(description.Translate(language, apiToken), @"\s{2,}", " ")}";
        }

        private static string GetCostInGelOrEmptyDescribe(long cost)
        {
            var usdToGel = UsdToGelCourseScraper.GetGelInOneDollarFromGeorgiaNationalBank();

            if (usdToGel == double.MaxValue) return "";

            return $"({Convert.ToInt32(usdToGel * cost)} ლ)";
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
    }
}
