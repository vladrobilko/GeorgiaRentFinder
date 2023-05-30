using Application.Models;
using System.Text.RegularExpressions;
using WebScraper;

namespace Application.Converters
{
    public static class FlatInfoClientModelConverter
    {
        private const long CountForRealtorDetection = 10;

        public static string ToTelegramCaptionWithRussianLanguage(this FlatInfoClientModel flat, bool isForAdmin, string language = null, string apiToken = null)
        {
            var caption = GetCaptionWithOutCoordinateAndRealtor(flat, language, apiToken);

            if (isForAdmin) caption += $"\nID in database - {flat.Id}";

            return caption;
        }

        public static string ToTelegramCaptionWithDefaultLanguage(this FlatInfoClientModel flat, bool isForAdmin)
        {
            var caption = GetCaptionWithOutCoordinateAndRealtor(flat);

            if (isForAdmin) caption += $"\nID in database - {flat.Id}";

            return caption;
        }

        private static string GetCaptionWithOutCoordinateAndRealtor(FlatInfoClientModel flat, string? language = null, string? apiToken = null)
        {
            if (language == "ru")
            {
                return $"{flat.Title.Translate(language, apiToken)}" +
                       $"\n\n<strong>Цена:</strong> {flat.Cost} $ {GetCostInGelOrEmptyDescribe(flat.Cost)}" +
                       $"{GetComfortStuffDescribe(flat.ComfortStuffClientModel)}" + 
                       $"\n\n<strong>Опубликовано:</strong> {flat.SitePublication.ToCommonViewString()}" +
                       $"\n<strong>Просмотры на сайте:</strong> {flat.ViewsOnSite}" +
                       $"{GetDescriptionOrEmptyString(flat.Description, language, apiToken)}" +
                       $"\n\n<strong>Сайт:</strong><a href=\"{flat.PageLink}\"> link</a>" +
                       $"{GetCoordinateOrEmptyDescribe(flat, "ru")}" +
                       $"{GetNumberDescribe(flat.FlatPhoneClientModel.PhoneNumber, "ru")}" +
                       $"{GetRealtorDescribe(flat, flat.FlatPhoneClientModel.MentionOnSite, "ru")}";
            }

            return $"{flat.Title}" +
                   $"\n\n<strong>Cost:</strong> {flat.Cost} $ {GetCostInGelOrEmptyDescribe(flat.Cost)}" +
                   $"\n\n<strong>Published:</strong> {flat.SitePublication.ToCommonViewString()}" +
                   $"\n<strong>Views on site:</strong> {flat.ViewsOnSite}" +
                   $"{GetDescriptionOrEmptyString(flat.Description)}" +
                   $"\n\n<strong>Web page:</strong><a href=\"{flat.PageLink}\"> link</a>" +
                   $"{GetCoordinateOrEmptyDescribe(flat)}" +
                   $"{GetNumberDescribe(flat.FlatPhoneClientModel.PhoneNumber)}" +
                   $"{GetRealtorDescribe(flat, flat.FlatPhoneClientModel.MentionOnSite)}";
        }

        private static string GetComfortStuffDescribe(ComfortStuffClientModel comfortStuff)// language
        {
            var describe = "";

            if (comfortStuff == null) return describe;//<strong></strong>

            if (comfortStuff.BedRooms != "No bedrooms") describe += $"\n<strong>Спален:</strong> {comfortStuff.BedRooms}";

            if (comfortStuff.Floor != "No floors") describe += $"\n<strong>Этаж:</strong> {comfortStuff.Floor}";

            if (comfortStuff.TotalArea != "No total area") describe += $"\n<strong>Площадь:</strong> {comfortStuff.TotalArea}";

            if (comfortStuff.IsThereGas != null) describe += comfortStuff.IsThereGas == true ? $"\n✅Газ✅" : $"\n❌Газ❌";

            if (comfortStuff.IsThereHotWater != null) describe += comfortStuff.IsThereHotWater == true ? $"\n✅Горячая вода✅" : $"\n❌Горячая вода❌";

            if (comfortStuff.IsThereConditioner != null) describe += comfortStuff.IsThereConditioner == true ? $"\n✅Кондиционер✅" : $"\n❌Кондиционер❌";

            return describe;
        }

        private static string GetNumberDescribe(string number, string? language = null)
        {
            if (number == "No number") return "";

            if (language == "ru")
            {
                return $"\n<strong>Телефон:</strong> {ConvertMobilePhoneToViewFormat(number)}";
            }

            return $"\n<strong>Phone:</strong> {ConvertMobilePhoneToViewFormat(number)}";
        }

        private static string ConvertMobilePhoneToViewFormat(string number)
        {
            string numberWithAddWhiteSpaces = "";

            if (number.Length > 9) return number;

            for (int i = 0; i < number.Length; i++)
            {
                numberWithAddWhiteSpaces += number[i];

                if ((i + 1) % 3 == 0 && i != number.Length - 1) numberWithAddWhiteSpaces += " ";
            }

            return $"+995 {numberWithAddWhiteSpaces}";
        }

        private static string GetRealtorDescribe(FlatInfoClientModel flat, long mentionOnSite, string? language = null)
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