using Telegram.Bot.Types;
using TelegramBudget.Services.TelegramApi;

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
        return services
            .AddTelegramFlow()
            .AddScoped<ITelegramApiService, TelegramApiService>();
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