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

    public async Task<Message> SendMessage(
        ChatId chatId,
        string text,
        ParseMode parseMode = default,
        ReplyParameters? replyParameters = null,
        IReplyMarkup? replyMarkup = null,
        LinkPreviewOptions? linkPreviewOptions = null,
        int? messageThreadId = null,
        IEnumerable<MessageEntity>? entities = null,
        bool disableNotification = false,
        bool protectContent = false,
        string? messageEffectId = null,
        string? businessConnectionId = null,
        bool allowPaidBroadcast = false,
        CancellationToken cancellationToken = default)
    {
        using var trace = tracee.Fixed("submit_total");
        
        var entitiesArray = entities?.ToArray();
        var message = await botClient
            .SendMessage(
                chatId,
                text.Trim(),
                parseMode,
                replyParameters,
                replyMarkup,
                linkPreviewOptions,
                messageThreadId,
                entitiesArray,
                disableNotification,
                protectContent,
                messageEffectId,
                businessConnectionId,
                allowPaidBroadcast,
                cancellationToken);

        if (AppConfiguration.DebugResponseTime)
        {
            await botClient
                .EditMessageText(
                    chatId,
                    message.Id,
                    $"{text.Trim()}\n\n{tracee.Milliseconds} ms",
                    parseMode,
                    entitiesArray,
                    linkPreviewOptions,
                    replyMarkup as InlineKeyboardMarkup,
                    businessConnectionId,
                    cancellationToken);
        }
        
        return message;
    }

    public async Task<Message> EditMessageText(
        ChatId chatId,
        int messageId,
        string text,
        ParseMode parseMode = default,
        IEnumerable<MessageEntity>? entities = null,
        LinkPreviewOptions? linkPreviewOptions = null,
        InlineKeyboardMarkup? replyMarkup = null,
        string? businessConnectionId = null,
        CancellationToken cancellationToken = default)
    {
        using var trace = tracee.Fixed("submit_total");
        
        var entitiesArray = entities?.ToArray();
        var message = await botClient
            .EditMessageText(
                chatId,
                messageId,
                text.Trim(),
                parseMode,
                entitiesArray,
                linkPreviewOptions,
                replyMarkup,
                businessConnectionId,
                cancellationToken);

        if (AppConfiguration.DebugResponseTime)
        {
            await botClient
                .EditMessageText(
                    chatId,
                    message.Id,
                    $"{text.Trim()}\n\n{tracee.Milliseconds} ms",
                    parseMode,
                    // ReSharper disable once PossibleMultipleEnumeration
                    entitiesArray,
                    linkPreviewOptions,
                    replyMarkup,
                    businessConnectionId,
                    cancellationToken);
        }
        
        return message;
    }
}