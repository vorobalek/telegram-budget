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
            .AddTelegramFlowNewInterface()
            .AddScoped<ITelegramApiService, TelegramApiService>();
    }

    public static string GetFullNameLink(this User user)
    {
        return TelegramHelper.GetFullNameLink(user.Id, user.FirstName, user.LastName);
    }
}