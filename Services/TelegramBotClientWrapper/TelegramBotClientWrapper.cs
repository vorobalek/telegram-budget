#define DEBUG_RESPONSE_TIME

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBudget.Extensions;
using TelegramBudget.Services.Trace;

namespace TelegramBudget.Services.TelegramBotClientWrapper;

public class TelegramBotClientWrapper(
    ITelegramBotClient botClient,
    IHttpContextAccessor httpContextAccessor,
    ITraceService trace) : ITelegramBotClientWrapper
{
    public ITelegramBotClient BotClient => botClient;

    public Task<Message> SendTextMessageAsync(ChatId chatId, string text, int? messageThreadId = default,
        ParseMode? parseMode = default,
        IEnumerable<MessageEntity>? entities = default, bool? disableWebPagePreview = default,
        bool? disableNotification = default,
        bool? protectContent = default, int? replyToMessageId = default, bool? allowSendingWithoutReply = default,
        IReplyMarkup? replyMarkup = default, CancellationToken cancellationToken = default)
    {
        return botClient
            .SendTextMessageAsync(
                chatId,
#if DEBUG_RESPONSE_TIME
                $"{text.Trim()}\n\n{trace.Milliseconds} ms",
#else
                text.Trim(),
#endif
                messageThreadId,
                parseMode,
                entities,
                disableWebPagePreview,
                disableNotification,
                protectContent,
                replyToMessageId,
                allowSendingWithoutReply,
                replyMarkup,
                cancellationToken);
    }

    public Task<Message> EditMessageTextAsync(ChatId chatId, int messageId, string text, ParseMode? parseMode = default,
        IEnumerable<MessageEntity>? entities = default, bool? disableWebPagePreview = default,
        InlineKeyboardMarkup? replyMarkup = default,
        CancellationToken cancellationToken = default)
    {
        return botClient
            .EditMessageTextAsync(
                chatId,
                messageId,
#if DEBUG_RESPONSE_TIME
                $"{text.Trim()}\n\n{trace.Milliseconds} ms",
#else
                text.Trim(),
#endif
                parseMode,
                entities,
                disableWebPagePreview,
                replyMarkup,
                cancellationToken);
    }
}