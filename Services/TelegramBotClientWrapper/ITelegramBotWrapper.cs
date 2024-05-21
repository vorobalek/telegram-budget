using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBudget.Services.TelegramBotClientWrapper;

internal interface ITelegramBotWrapper
{
    ITelegramBotClient BotClient { get; }

    /// <inheritdoc cref="Telegram.Bot.TelegramBotClientExtensions.SendTextMessageAsync"/>
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

    /// <inheritdoc cref="Telegram.Bot.TelegramBotClientExtensions.EditMessageTextAsync(ITelegramBotClient, ChatId, int, string, ParseMode?, IEnumerable{MessageEntity}?, bool?, InlineKeyboardMarkup?, CancellationToken)"/>
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