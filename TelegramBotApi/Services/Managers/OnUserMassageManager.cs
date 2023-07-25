using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using Application.Interfaces;
using static System.Net.Mime.MediaTypeNames;
using System.Threading;

namespace TelegramBotApi.Services.Managers
{
    public class OnUserMassageManager
    {
        protected OnUserMassageManager() { }

        public static async Task<Message> BotStart(ITelegramBotClient bot, Message message, CancellationToken cancel)
        {
            await bot.DeleteMessageAsync(message.Chat.Id, message.MessageId, cancel);

            //make logic if user exist

            // save user to db

            return await SendTextMessageAsync(bot, message, cancel, MessageToUserManager.GetStartMessage(), GetKeyboardWithLanguageChoice(message));
        }

        public static async Task<Message> OnTextResponse(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            // get language from db and give answer with this language
            var language = "ru";

            return await SendTextMessageAsync(botClient, message, cancellationToken, MessageToUserManager.GetMessageForAfterOnlyTextSending(language), new ReplyKeyboardRemove());
        }

        public static async Task<Message> Rent(ITelegramBotClient bot, Message mes, CancellationToken cancel)
        {
            // find city and language of user
            var language = "ru";
            var city = "Batumi";
            throw new NotImplementedException();
        }

        public static async Task<Message> RentOut(ITelegramBotClient bot, Message mes, CancellationToken cancel)
        {
            // find city and language of user
            throw new NotImplementedException();
        }

        public static async Task<Message> Admin(ITelegramBotClient bot, Message mes, CancellationToken cancel)
        {
            await bot.DeleteMessageAsync(mes.Chat.Id, mes.MessageId - 1, cancel);
            await bot.DeleteMessageAsync(mes.Chat.Id, mes.MessageId, cancel);
            
            //get user language  

            return await SendTextMessageAsync(bot, mes, cancel, MessageToUserManager.GetMessageAdminInfo("ru"));
        }

        private static InlineKeyboardMarkup GetKeyboardWithLanguageChoice(Message message)
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
