using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace TelegramBotApi.Services.Managers
{
    public class OnUserMassageManager
    {
        private readonly ITelegramBotClient _bot;

        public OnUserMassageManager(ITelegramBotClient bot)
        {
            this._bot = bot;
        }

        public  async Task<Message> BotStart(Message message, CancellationToken cancel)
        {
            await _bot.DeleteMessageAsync(message.Chat.Id, message.MessageId, cancel);

            //make logic if user exist

            // save user to db

            return await SendTextMessageAsync(message, cancel, MessageToUserManager.GetStartMessage(), GetKeyboardWithLanguageChoice(message));
        }

        public  async Task<Message> OnTextResponse( Message message, CancellationToken cancellationToken)
        {
            // get language from db and give answer with this language
            var language = "ru";

            return await SendTextMessageAsync(message, cancellationToken, MessageToUserManager.GetMessageForAfterOnlyTextSending(language), new ReplyKeyboardRemove());
        }

        public  async Task<Message> Rent( Message mes, CancellationToken cancel)
        {
            // find city and language of user
            var language = "ru";
            var city = "Batumi";
            throw new NotImplementedException();
        }

        public  async Task<Message> RentOut( Message mes, CancellationToken cancel)
        {
            // find city and language of user
            throw new NotImplementedException();
        }

        public  async Task<Message> Admin( Message mes, CancellationToken cancel)
        {
            await _bot.DeleteMessageAsync(mes.Chat.Id, mes.MessageId - 1, cancel);
            await _bot.DeleteMessageAsync(mes.Chat.Id, mes.MessageId, cancel);
            
            //get user language  

            return await SendTextMessageAsync(mes, cancel, MessageToUserManager.GetMessageAdminInfo("ru"));
        }

        private  InlineKeyboardMarkup GetKeyboardWithLanguageChoice(Message message)
        {
            return new(
                new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("🪆Русский🐻",$"language_ru_{message.From?.Id}"),
                        InlineKeyboardButton.WithCallbackData("⛰ქართული🌊",$"language_ka_{message.From?.Id}"),
                    }
                });
        }

        private  async Task<Message> SendTextMessageAsync( Message message, CancellationToken cancellationToken, string text)
        {
            return await _bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: text,
                cancellationToken: cancellationToken);
        }

        private  async Task<Message> SendTextMessageAsync( Message message, CancellationToken cancellationToken, string text, IReplyMarkup replyMarkup)
        {
            return await _bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: text,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken,
                replyMarkup: replyMarkup);
        }
    }
}