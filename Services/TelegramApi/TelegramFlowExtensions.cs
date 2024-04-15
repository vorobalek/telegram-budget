using Common.Infrastructure.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Flow;
using Telegram.Flow.Extensions;
using Telegram.Flow.Updates;
using Telegram.Flow.Updates.CallbackQueries.Data;
using Telegram.Flow.Updates.Messages.Texts.BotCommands;
using TelegramBudget.Extensions;
using TelegramBudget.Services.TelegramApi.Handlers;

namespace TelegramBudget.Services.TelegramApi;

public static class TelegramFlowExtensions
{
    public static IServiceCollection AddTelegramFlow(this IServiceCollection services)
    {
        return services
            .AddPreHandler()

            .AddBotCommand<StartBotCommand>(
                (handler, _, token) => handler.ProcessAsync(token),
                "start", "help")
            
            .AddBotCommand<ListBotCommand>(
                (handler, _, token) => handler.ProcessAsync(token),
                "list")
            
            .AddBotCommand<MeBotCommand>(
                (handler, _, token) => handler.ProcessAsync(token),
                "me")
            
            .AddBotCommand<HistoryBotCommand>(
                (handler, context, token) => handler.ProcessAsync(context.Data, token),
                "history")
            .AddBotCommandPrefix<HistoryPrefixBotCommand>(
                (handler, context, token) => handler.ProcessAsync(context.Data, token),
                "history_")
            
            .AddBotCommand<CreateBotCommand>(
                (handler, context, token) => handler.ProcessAsync(context.Data, token),
                "create")
            
            .AddBotCommand<SwitchBotCommand>(
                (handler, context, token) => handler.ProcessAsync(context.Data, token),
                "switch")
            .AddBotCommandPrefix<SwitchPrefixBotCommand>(
                (handler, context, token) => handler.ProcessAsync(context.Data, token),
                "switch_")
            
            .AddBotCommand<TimezoneBotCommand>(
                (handler, context, token) => handler.ProcessAsync(context.Data, token),
                "timezone")
            
            .AddBotCommand<GrantBotCommand>(
                (handler, context, token) => handler.ProcessAsync(context.Data, token),
                "grant")
            .AddBotCommandPrefix<GrantPrefixBotCommand>(
                (handler, context, token) => handler.ProcessAsync(context.Data, token),
                "grant_")
            
            .AddBotCommand<RevokeBotCommand>(
                (handler, context, token) => handler.ProcessAsync(context.Data, token),
                "revoke")
            .AddBotCommandPrefix<RevokePrefixBotCommand>(
                (handler, context, token) => handler.ProcessAsync(context.Data, token),
                "revoke_")
            
            .AddBotCommand<DeleteBotCommand>(
                (handler, context, token) => handler.ProcessAsync(context.Data, token),
                "delete")
            .AddBotCommandPrefix<DeletePrefixBotCommand>(
                (handler, context, token) => handler.ProcessAsync(context.Data, token),
                "delete_")
            
            .AddScoped<TransactionPlainText>()
            .AddScoped<IUpdateHandler>(sp => TelegramFlow.New
                .ForMessage(message => message
                    .ForText(text => text
                        .WithInjection<TransactionPlainText>()
                        .WithAsyncProcessing((context, injected, token) =>
                            injected.ProcessAsync(context.Message, context.Text, token))))
                .WithDisplayName(nameof(TransactionPlainText))
                .Build<TransactionPlainText>(sp))
            
            .AddScoped<TransactionEditedPlainText>()
            .AddScoped<IUpdateHandler>(sp => TelegramFlow.New
                .ForEditedMessage(message => message
                    .ForText(text => text
                        .WithInjection<TransactionEditedPlainText>()
                        .WithAsyncProcessing((context, injected, token) =>
                            injected.ProcessAsync(context.EditedMessage, context.Text, token))))
                .WithDisplayName(nameof(TransactionEditedPlainText))
                .Build<TransactionEditedPlainText>(sp))
            
            .AddCallbackData<CmdAllCallback>(
                (handler, context, token) => handler.ProcessAsync(context.CallbackQuery.Message, token),
                "cmd.all")
            
            .AddCallbackData<MainCallback>(
                (handler, context, token) => handler.ProcessAsync(context.CallbackQuery.Message, token),
                "main");
    }

    private static IServiceCollection AddBotCommand<T>(
        this IServiceCollection services,
        Func<T, IBotCommandTextMessageUpdateHandlerContext, CancellationToken, Task> processing,
        params string[] targetCommands) where T : class
    {
        return
            services
                .AddScoped<T>()
                .AddScoped<IUpdateHandler>(sp =>
                    TelegramFlow.New
                        .ForMessage(message => message
                            .ForText(text => text
                                .ForBotCommand(command =>
                                {
                                    foreach (var targetCommand in targetCommands) command.ForExact(targetCommand);

                                    command = command
                                        .WithInjection<T>()
                                        .WithAsyncProcessing((context, injected, token) =>
                                            processing(injected, context, token));

                                    return command;
                                })))
                        .WithDisplayName(typeof(T).GetName())
                        .Build<T>(sp));
    }

    private static IServiceCollection AddBotCommandPrefix<T>(
        this IServiceCollection services,
        Func<T, IBotCommandTextMessageUpdateHandlerContext, CancellationToken, Task> processing,
        params string[] targetCommands) where T : class
    {
        return
            services
                .AddScoped<T>()
                .AddScoped<IUpdateHandler>(sp =>
                    TelegramFlow.New
                        .ForMessage(message => message
                            .ForText(text => text
                                .ForBotCommand(command =>
                                {
                                    foreach (var targetCommand in targetCommands) command.ForPrefix(targetCommand);

                                    command = command
                                        .WithInjection<T>()
                                        .WithAsyncProcessing((context, injected, token) =>
                                            processing(injected, context, token));

                                    return command;
                                })))
                        .WithDisplayName(typeof(T).GetName())
                        .Build<T>(sp));
    }

    private static IServiceCollection AddCallbackData<T>(
        this IServiceCollection services,
        Func<T, IDataCallbackQueryUpdateHandlerContext, CancellationToken, Task> processing,
        params string[] targetCommands) where T : class
    {
        return
            services
                .AddScoped<T>()
                .AddScoped<IUpdateHandler>(sp =>
                    TelegramFlow.New
                        .ForCallbackQuery(callbackQuery => callbackQuery
                            .ForData(data =>
                            {
                                foreach (var targetCommand in targetCommands) data.ForExact(targetCommand);

                                data = data
                                    .WithInjection<T>()
                                    .WithAsyncProcessing((context, injected, token) =>
                                        processing(injected, context, token));

                                return data;
                            }))
                        .WithDisplayName(typeof(T).GetName())
                        .Build<T>(sp));
    }

    private static IServiceCollection AddPreHandler(this IServiceCollection services)
    {
        return
            services
                .AddScoped<IPreHandler>(sp => new PreHandler(
                    TelegramFlow.New
                        .ForMessage(message => message
                            .ForText(text => text
                                .WithInjection<ITelegramBotClient>()
                                .WithAsyncProcessing((context, injected, token) =>
                                    injected.SendChatActionAsync(context.Update.GetUser().Id, ChatAction.Typing,
                                        cancellationToken: token))))
                        .ForCallbackQuery(callbackQuery => callbackQuery
                            .WithInjection<ITelegramBotClient>()
                            .WithAsyncProcessing((context, injected, token) =>
                                injected.AnswerCallbackQueryAsync(context.CallbackQuery.Id, cancellationToken: token)))
                        .ForEditedMessage(message => message
                            .ForText(text => text
                                .WithInjection<ITelegramBotClient>()
                                .WithAsyncProcessing((context, injected, token) =>
                                    injected.SendChatActionAsync(context.Update.GetUser().Id, ChatAction.Typing,
                                        cancellationToken: token))))
                        .Build<ITelegramBotClient>(sp)));
    }
}