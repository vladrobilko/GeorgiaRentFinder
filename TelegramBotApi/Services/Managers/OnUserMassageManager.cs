using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using Application.Interfaces;
using Application.Models;

namespace TelegramBotApi.Services.Managers
{
    public class OnUserMassageManager
    {
        protected OnUserMassageManager() { }

        public static async Task<Message> BotStart(ITelegramBotClient bot, IFlatInfoService informer, Message message, CancellationToken cancel)
        {
            return await SendTextMessageAsync(bot, message, cancel, MessageToUserManager.GetStartMessage(), GetKeyboardWithChoice(message));
        }

        private static InlineKeyboardMarkup GetKeyboardWithChoice(Message message)
        {
            return new(
                new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("🪆Русский🐻",$"language_ru_{message.From.Id}"),
                        InlineKeyboardButton.WithCallbackData("⛰ქართული🌊",$"language_ka_{message.From.Id}"),
                    }
                });
        }

        private static async Task<Message> SendTextMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string text)
        {
            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: text,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }

        private static async Task<Message> SendTextMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string text, IReplyMarkup replyMarkup)
        {
            return await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: text,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken,
                replyMarkup: replyMarkup);
        }
    }
}
