using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBudget.Services.TelegramBotClientWrapper;

public interface ITelegramBotClientWrapper
{
    ITelegramBotClient BotClient { get; }

    Task<Message> SendTextMessageAsync(
        ChatId chatId,
        string text,
        int? messageThreadId = default,
        ParseMode? parseMode = default,
        IEnumerable<MessageEntity>? entities = default,
        bool? disableWebPagePreview = default,
        bool? disableNotification = default,
        bool? protectContent = default,
        int? replyToMessageId = default,
        bool? allowSendingWithoutReply = default,
        IReplyMarkup? replyMarkup = default,
        CancellationToken cancellationToken = default
    );

    Task<Message> EditMessageTextAsync(
        ChatId chatId,
        int messageId,
        string text,
        ParseMode? parseMode = default,
        IEnumerable<MessageEntity>? entities = default,
        bool? disableWebPagePreview = default,
        InlineKeyboardMarkup? replyMarkup = default,
        CancellationToken cancellationToken = default
    );
}