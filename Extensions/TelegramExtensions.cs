using Telegram.Bot.Types;
using TelegramBudget.Services.TelegramUpdates;
using TelegramBudget.Services.TelegramUpdates.EditedMessages;
using TelegramBudget.Services.TelegramUpdates.EditedMessages.Text;
using TelegramBudget.Services.TelegramUpdates.Messages;
using TelegramBudget.Services.TelegramUpdates.Messages.Text;

namespace TelegramBudget.Extensions;

public static class TelegramExtensions
{
    private static User? TryGetUser(this Update update)
    {
        return
            update.Message?.From
            ?? update.EditedMessage?.From
            ?? update.CallbackQuery?.From
            ?? update.ChannelPost?.From
            ?? update.ChatMember?.From
            ?? update.InlineQuery?.From
            ?? update.PollAnswer?.User
            ?? update.ShippingQuery?.From
            ?? update.ChatJoinRequest?.From
            ?? update.ChosenInlineResult?.From
            ?? update.EditedChannelPost?.From
            ?? update.MyChatMember?.From
            ?? update.PreCheckoutQuery?.From;
    }

    public static User GetUser(this Update update)
    {
        return TryGetUser(update) ?? throw new InvalidOperationException("Unable to determine update initiator");
    }

    public static IServiceCollection AddTelegramHandlers(this IServiceCollection services)
    {
        services.AddScoped<IHandleUpdateService, HandleUpdateService>();
        services.AddScoped<IUpdateHandler, MessageUpdateHandler>();

        services.AddScoped<IMessageHandler, TextMessageHandler>();
        services.AddScoped<ITextHandler, CreateBudgetTextHandler>();
        services.AddScoped<ITextHandler, GrantBudgetTextHandler>();
        services.AddScoped<ITextHandler, SwitchBudgetTextHandler>();
        services.AddScoped<ITextHandler, MeTextHandler>();
        services.AddScoped<ITextHandler, TransactionTextHandler>();
        services.AddScoped<ITextHandler, ListBudgetTextHandler>();
        services.AddScoped<ITextHandler, DeleteBudgetTextHandler>();
        services.AddScoped<ITextHandler, HelpTextHandler>();
        services.AddScoped<ITextHandler, HistoryTextHandler>();
        services.AddScoped<ITextHandler, TimeZoneTextHandler>();
        services.AddScoped<ITextHandler, RevokeBudgetTextHandler>();

        services.AddScoped<ITextHandler, SwitchBudgetInternalTextHandler>();
        services.AddScoped<ITextHandler, HistoryInternalTextHandler>();
        services.AddScoped<ITextHandler, GrantBudgetInternalTextHandler>();
        services.AddScoped<ITextHandler, RevokeBudgetInternalTextHandler>();

        services.AddScoped<IUpdateHandler, EditedMessageUpdateHandler>();
        services.AddScoped<IEditedMessageHandler, TextEditedMessageHandler>();
        services.AddScoped<ITextEditedHandler, TransactionTextEditedHandler>();

        return services;
    }

    private static string GetFullName(this User user)
    {
        return $"{user.FirstName.EscapeHtml()}" +
               $"{(user.LastName is not null
                   ? " " + user.LastName.EscapeHtml()
                   : string.Empty)}";
    }

    public static string GetFullNameLink(this User user)
    {
        return "<a href=\"tg://user?id=" +
               $"{user.Id}\">" +
               user.GetFullName() +
               "</a>";
    }
}