using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBudget.Configuration;
using Tracee;

namespace TelegramBudget.Services.TelegramBotClientWrapper;

internal sealed class TelegramBotWrapper(
    ITelegramBotClient botClient,
    ITracee tracee) : ITelegramBotWrapper
{
    public ITelegramBotClient BotClient => botClient;

    public async Task<Message> SendTextMessageAsync(ChatId chatId, string text, int? messageThreadId = default,
        ParseMode? parseMode = default,
        IEnumerable<MessageEntity>? entities = default, bool? disableWebPagePreview = default,
        bool? disableNotification = default,
        bool? protectContent = default, int? replyToMessageId = default, bool? allowSendingWithoutReply = default,
        IReplyMarkup? replyMarkup = default, CancellationToken cancellationToken = default)
    {
        using var trace = tracee.Fixed("submit_total");
        var message = await botClient
            .SendTextMessageAsync(
                chatId,
                text.Trim(),
                messageThreadId,
                parseMode,
                // ReSharper disable once PossibleMultipleEnumeration
                entities,
                disableWebPagePreview,
                disableNotification,
                protectContent,
                replyToMessageId,
                allowSendingWithoutReply,
                replyMarkup,
                cancellationToken);

        if (AppConfiguration.DebugResponseTime)
        {
            await botClient
                .EditMessageTextAsync(
                    chatId,
                    message.MessageId,
                    $"{text.Trim()}\n\n{tracee.Milliseconds} ms",
                    parseMode,
                    // ReSharper disable once PossibleMultipleEnumeration
                    entities,
                    disableWebPagePreview,
                    replyMarkup as InlineKeyboardMarkup,
                    cancellationToken);
        }
        
        return message;
    }

    public async Task<Message> EditMessageTextAsync(ChatId chatId, int messageId, string text,
        ParseMode? parseMode = default,
        IEnumerable<MessageEntity>? entities = default, bool? disableWebPagePreview = default,
        InlineKeyboardMarkup? replyMarkup = default,
        CancellationToken cancellationToken = default)
    {
        using var trace = tracee.Fixed("submit_total");
        var message = await botClient
            .EditMessageTextAsync(
                chatId,
                messageId,
                text.Trim(),
                parseMode,
                // ReSharper disable once PossibleMultipleEnumeration
                entities,
                disableWebPagePreview,
                replyMarkup,
                cancellationToken);

        if (AppConfiguration.DebugResponseTime)
        {
            await botClient
                .EditMessageTextAsync(
                    chatId,
                    message.MessageId,
                    $"{text.Trim()}\n\n{tracee.Milliseconds} ms",
                    parseMode,
                    // ReSharper disable once PossibleMultipleEnumeration
                    entities,
                    disableWebPagePreview,
                    replyMarkup,
                    cancellationToken);
        }
        
        return message;
    }
}