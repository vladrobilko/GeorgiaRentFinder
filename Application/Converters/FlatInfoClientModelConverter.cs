﻿using Application.Models;
using System.Text.RegularExpressions;
using WebScraper;
using WebScraper.Converters;

namespace Application.Converters
{
    public static class FlatInfoClientModelConverter
    {
        private const long CountForRealtorDetection = 10;

        public static string ToTelegramCaptionWithDefaultLanguage(this FlatInfoClientModel flat)
        {
            var caption = GetCaptionWithOutCoordinateAndRealtor(flat);

            if (IsCoordinateExist(flat.FlatCoordinateClientModel)) caption += GetCoordinateOrEmptyDescribe(flat);

            if (IsItRealtor(flat)) caption += GetRealtorDescribe(flat.FlatPhoneClientModel.MentionOnSite);

            return caption;
        }

        public static string ToTelegramCaptionWithRussianLanguage(this FlatInfoClientModel flat, bool isForAdmin, string language = null, string apiToken = null)
        {
            var caption = GetCaptionWithOutCoordinateAndRealtor(flat, language, apiToken);

            if (IsCoordinateExist(flat.FlatCoordinateClientModel)) caption += GetCoordinateOrEmptyDescribe(flat, "ru");

            if (IsItRealtor(flat)) caption += GetRealtorDescribe(flat.FlatPhoneClientModel.MentionOnSite, "ru");

            if (isForAdmin) caption += $"\nID in database - {flat.Id}";

            return caption;
        }

        private static string GetCaptionWithOutCoordinateAndRealtor(FlatInfoClientModel flat, string language = null, string apiToken = null)
        {
            if (language == "ru")
            {
                return $"{flat.Title.Translate(language, apiToken)}\n\n" +

                       $"<strong>Цена:</strong> {flat.Cost} $ {GetCostInGelOrEmptyDescribe(flat.Cost)}\n\n" +

                       $"<strong>Просмотры на сайте:</strong> {flat.ViewsOnSite}\n" +
                       $"<strong>Опубликовано:</strong> {flat.SitePublication.ToCommonViewString()}\n" +
                       $"{GetDescriptionOrEmptyString(flat.Description, language, apiToken)}" +

                       $"<strong>Сайт:</strong><a href=\"{flat.PageLink}\"> link</a>\n" +
                       $"<strong>Телефон:</strong> {flat.FlatPhoneClientModel.PhoneNumber.Translate(language, apiToken)}\n";
            }

            return $"{flat.Title}\n\n" +

                 $"<strong>Cost:</strong> {flat.Cost} $ {GetCostInGelOrEmptyDescribe(flat.Cost)}\n\n" +

                 $"<strong>Views on site:</strong> {flat.ViewsOnSite}\n" +
                 $"<strong>Published:</strong> {flat.SitePublication.ToCommonViewString()}\n" +
                 $"{GetDescriptionOrEmptyString(flat.Description)}" +

                 $"<strong>Web page:</strong><a href=\"{flat.PageLink}\"> link</a>\n" +
                 $"<strong>Mobile phone:</strong> {flat.FlatPhoneClientModel.PhoneNumber}\n";
        }

        private static string GetDescriptionOrEmptyString(string description, string language = null, string apiToken = null)
        {
            if (description == "No description") return "";

            if (language == null) return $"<strong>Description:</strong> {description}\n\n";

            return
                $"<strong>Описание:</strong> {Regex.Replace(description.Translate(language, apiToken), @"\s{2,}", " ")}\n\n";
        }

        private static string GetCostInGelOrEmptyDescribe(long cost)
        {
            var usdToGel = UsdToGelCourseScraper.GetGelInOneDollarFromGeorgiaNationalBank();

            if (usdToGel == double.MaxValue) return "";

            return $"({Convert.ToInt32(usdToGel * cost)} ლ)";
        }

        private static string GetCoordinateOrEmptyDescribe(FlatInfoClientModel flat, string language = null)
        {
            if (language == "ru")
            {
                return
                    $"<strong>Локация:</strong><a href=\"{GetGoogleMapLocation(flat.FlatCoordinateClientModel.Latitude, flat.FlatCoordinateClientModel.Longitude)}\"> link</a>\n";
            }
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

        private static string GetRealtorDescribe(long mentionOnSite, string language = null)
        {
            if (language == "ru")
            {
                return $"<strong>Возможно это риелтор:</strong> <ins>Номер упоминался {mentionOnSite} раз</ins>";
            }
            return $"<strong>Maybe it's a realtor:</strong> <ins>The number was mentioned {mentionOnSite} times</ins>";
        }
    }
}
