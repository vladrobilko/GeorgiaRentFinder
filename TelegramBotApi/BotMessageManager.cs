﻿using WebScraper.Converters;

namespace TelegramBotApi
{
    public static class BotMessageManager
    {
        public const string Usage = "Usage:"
                              + "\n/FindSuitAdjaraFlats" +
                              "\n/GetLastAvailableFlat";

        private const string GetLastAvailableFlatLink = "/GetLastAvailableFlat";
        public static string GetUsageWithTimeNow()
        {
            return "______________________________________" +
                   $"\nThe bot started at {DateTime.Now.ToCommonViewString()}" +
                   $"\n{Usage}" ;
        }

        public static string GetUsageWithNoFreeFlats()
        {
            return $"<ins><strong>No free flats</strong></ins>" +
                   $"\n\n{Usage}";
        }

        public static string GetMessageAfterPost()
        {
            return $"<ins><strong>The post has been sent!</strong></ins>" +
                   $"\n{Usage}";
        }

        public static string GetMessageAfterRefusePost()
        {
            return $"<ins><strong>The post has NOT been sent!</strong></ins>" +
                   $"\n{Usage}";
        }

        public static string GetMessageWithCountNotDistributedFlats(long countNotViewedFlats)
        {
            return $"There are <ins><strong>{countNotViewedFlats} NOT distributed flats.</strong></ins>" +
                   $"\nYou need to do this: {GetLastAvailableFlatLink}";
        }
        public static string GetMessageWithCountFoundedFlats(long countFoundedFlats)
        {
            return $"There are <ins><strong>{countFoundedFlats} NOT distributed flats.</strong></ins>" +
                   $"\nYou need to do this: {GetLastAvailableFlatLink}";
        }
    }
}