using System.Text;
using Application.Models;
using System.Text.RegularExpressions;
using WebScraper;

namespace Application.Converters
{
    public static class FlatInfoClientModelConverter
    {
        private const long CountForRealtorDetection = 20;

        public static string ToTelegramCaptionWithRussianLanguage(this FlatInfoClientModel flat, bool isForAdmin, string language, string apiToken)
        {
            return $"{GetTitleDescribe(flat, language, apiToken)}" +
                   $"{GetCostDescribe(flat)}" +
                   $"{GetComfortStuffDescribe(flat.ComfortStuffClientModel)}" +
                   $"{GetTimeDescribe(flat)}" +
                   $"{GetViewsOnSiteDescribe(flat)}" +
                   $"{GetDescriptionOrEmptyString(flat.Description, language, apiToken)}" +
                   $"{GetPageLinkDescribe(flat)}" +
                   $"{GetNumberDescribe(flat.FlatPhoneClientModel.PhoneNumber)}" +
                   $"{GetCoordinateOrEmptyDescribe(flat)}" +
                   $"{GetRealtorDescribe(flat, flat.FlatPhoneClientModel.MentionOnSite)}" +
                   $"{GetIdDescriptionOrEmptyString(isForAdmin,flat.Id)}";
        }

        private static string GetTitleDescribe(FlatInfoClientModel flat, string language, string apiToken)
        {
            return $"🏠{flat.Title.Translate(language, apiToken)}";
        }

        private static string GetCostDescribe(FlatInfoClientModel flat)
        {
            return $"\n\n💵<strong>Цена:</strong> {flat.Cost} $ {GetCostInGelOrEmptyDescribe(flat.Cost)}";
        }

        private static string GetComfortStuffDescribe(ComfortStuffClientModel comfortStuff)
        {
            var describe = "";

            if (comfortStuff.BedRooms != "No bedrooms") describe += $"\n🛏<strong>Спален:</strong> {comfortStuff.BedRooms}";

            if (comfortStuff.Floor != "No floors") describe += $"\n🏢<strong>Этаж:</strong> {comfortStuff.Floor}";

            if (comfortStuff.TotalArea != "No total area") describe += $"\n📌<strong>Площадь:</strong> {comfortStuff.TotalArea}";

            if (comfortStuff.IsThereGas != null) describe += comfortStuff.IsThereGas == true ? $"\n✅Газ" : $"\n❌Газ";

            if (comfortStuff.IsThereHotWater != null) describe += comfortStuff.IsThereHotWater == true ? $"\n✅Горячая вода" : $"\n❌Горячая вода";

            if (comfortStuff.IsThereConditioner != null) describe += comfortStuff.IsThereConditioner == true ? $"\n✅Кондиционер" : $"\n❌Кондиционер";

            return describe;
        }

        private static string GetTimeDescribe(FlatInfoClientModel flat)
        {
            return $"\n\n🕑<strong>Опубликовано:</strong> {flat.SitePublication.ToCommonViewString()}";
        }

        private static string GetViewsOnSiteDescribe(FlatInfoClientModel flat)
        {
            return $"\n👀<strong>Просмотры на сайте:</strong> {flat.ViewsOnSite}";
        }

        private static string GetDescriptionOrEmptyString(string description, string language, string apiToken)
        {
            if (description == "No description") return "";
            return $"\n📝<strong>Описание:</strong> {Regex.Replace(description.Translate(language, apiToken), @"\s{2,}", " ")}";
        }

        private static string GetPageLinkDescribe(FlatInfoClientModel flat)
        {
            if (flat.PageLink.Contains("myhome"))
            {
                return $"\n\n🔗<strong>Сайт:</strong><a href=\"{flat.PageLink}\"> myhome.ge</a>";
            }
            return $"\n\n🔗<strong>Сайт:</strong><a href=\"{flat.PageLink}\"> ss.ge</a>";
        }

        private static string GetCoordinateOrEmptyDescribe(FlatInfoClientModel flat)
        {
            if (!IsCoordinateExist(flat.FlatCoordinateClientModel)) return "";

            return $"\n📍<strong>На карте:</strong><a href=\"{GetGoogleMapLocation(flat.FlatCoordinateClientModel.Latitude, flat.FlatCoordinateClientModel.Longitude)}\"> google.com/maps</a>";
        }

        private static string GetNumberDescribe(string number)
        {
            if (number == "No number") return "";
            return $"\n☎️<strong>Телефон:</strong> {ConvertMobilePhoneToViewGeorgiaFormat(number)}";
        }

        private static string GetRealtorDescribe(FlatInfoClientModel flat, long mentionOnSite)
        {
            if (!IsItRealtor(flat)) return "";

            return $"\n‼️<strong>Возможно это риелтор:</strong> <ins>Номер упоминался {mentionOnSite / 2} раз</ins>";
        }

        private static string ConvertMobilePhoneToViewGeorgiaFormat(string number)
        {
            var numberWithAddWhiteSpaces = new StringBuilder();

            var codeOfGeorgia = "+995";

            if (number.Length > 9) return number;

            for (int i = 0; i < number.Length; i++)
            {
                numberWithAddWhiteSpaces.Append(number[i]);

                if ((i + 1) % 3 == 0 && i != number.Length - 1) numberWithAddWhiteSpaces.Append(" ");
            }

            return $"{codeOfGeorgia} {numberWithAddWhiteSpaces}";
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
            return flat.FlatPhoneClientModel.MentionOnSite / 2 > CountForRealtorDetection &&
                   flat.FlatPhoneClientModel.PhoneNumber != "No number";
        }

        private static string GetIdDescriptionOrEmptyString(bool isForAdmin, long id)
        {
            if (isForAdmin)
            {
                return $"\nID in database - {id}";
            }

            return "";
        }
    }
}