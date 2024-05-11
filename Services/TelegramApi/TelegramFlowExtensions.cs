using Common.Infrastructure.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Flow;
using Telegram.Flow.Extensions;
using Telegram.Flow.Updates;
using Telegram.Flow.Updates.CallbackQueries.Data;
using Telegram.Flow.Updates.Messages.Texts.BotCommands;
using TelegramBudget.Services.TelegramApi.Handlers;
using TelegramBudget.Services.TelegramApi.NewHandlers;
using TelegramBudget.Services.TelegramApi.PreHandler;
using TelegramBudget.Services.Trace;

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
            .AddScoped<IUpdateHandler>(serviceProvider => serviceProvider
                .WatchServiceProvider(sp => TelegramFlow.New
                    .ForMessage(message => message
                        .ForText(text => text
                            .WithInjection<TransactionPlainText>()
                            .WithAsyncProcessing((context, injected, token) =>
                                injected.ProcessAsync(context.Message, context.Text, token))))
                    .WithDisplayName(nameof(TransactionPlainText))
                    .Build<TransactionPlainText>(sp)))

            .AddScoped<TransactionEditedPlainText>()
            .AddScoped<IUpdateHandler>(serviceProvider => serviceProvider
                .WatchServiceProvider(sp => TelegramFlow.New
                    .ForEditedMessage(message => message
                        .ForText(text => text
                            .WithInjection<TransactionEditedPlainText>()
                            .WithAsyncProcessing((context, injected, token) =>
                                injected.ProcessAsync(context.EditedMessage, context.Text, token))))
                    .WithDisplayName(nameof(TransactionEditedPlainText))
                    .Build<TransactionEditedPlainText>(sp)))

            .AddCallbackData<CmdAllCallback>(
                (handler, context, token) => handler.ProcessAsync(context.CallbackQuery.Message, token),
                "cmd.all")

            .AddCallbackData<MainCallback>(
                (handler, context, token) => handler.ProcessAsync(context.CallbackQuery.Message, token),
                "main.old");
    }

    private static IServiceCollection AddBotCommand<T>(
        this IServiceCollection services,
        Func<T, IBotCommandTextMessageUpdateHandlerContext, CancellationToken, Task> processing,
        params string[] targetCommands) where T : class
    {
        return
            services
                .AddScoped<T>()
                .AddScoped<IUpdateHandler>(serviceProvider => serviceProvider
                    .WatchServiceProvider(sp =>
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
                            .Build<T>(sp)));
    }

    private static IServiceCollection AddBotCommandPrefix<T>(
        this IServiceCollection services,
        Func<T, IBotCommandTextMessageUpdateHandlerContext, CancellationToken, Task> processing,
        params string[] targetCommands) where T : class
    {
        return
            services
                .AddScoped<T>()
                .AddScoped<IUpdateHandler>(serviceProvider => serviceProvider
                    .WatchServiceProvider(sp =>
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
                            .Build<T>(sp)));
    }

    private static IServiceCollection AddCallbackData<T>(
        this IServiceCollection services,
        Func<T, IDataCallbackQueryUpdateHandlerContext, CancellationToken, Task> processing,
        params string[] targetCommands) where T : class
    {
        return
            services
                .AddScoped<T>()
                .AddScoped<IUpdateHandler>(serviceProvider => serviceProvider
                    .WatchServiceProvider(sp =>
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
                            .Build<T>(sp)));
    }

    private static IServiceCollection AddCallbackDataPrefix<T>(
        this IServiceCollection services,
        Func<T, IDataCallbackQueryUpdateHandlerContext, CancellationToken, Task> processing,
        params string[] targetPrefixes) where T : class
    {
        return
            services
                .AddScoped<T>()
                .AddScoped<IUpdateHandler>(serviceProvider => serviceProvider
                    .WatchServiceProvider(sp =>
                        TelegramFlow.New
                            .ForCallbackQuery(callbackQuery => callbackQuery
                                .ForData(data =>
                                {
                                    foreach (var targetPrefix in targetPrefixes) data.ForPrefix(targetPrefix);

                                    data = data
                                        .WithInjection<T>()
                                        .WithAsyncProcessing((context, injected, token) =>
                                            processing(injected, context, token));

                                    return data;
                                }))
                            .WithDisplayName(typeof(T).GetName())
                            .Build<T>(sp)));
    }

    private static IServiceCollection AddPreHandler(this IServiceCollection services)
    {
        return
            services
                .AddScoped<IPreHandlerService>(serviceProvider => serviceProvider
                    .WatchServiceProvider(sp => new PreHandlerService(
                        TelegramFlow.New
                            .ForMessage(message => message
                                .ForText(text => text
                                    .WithInjection<ITelegramBotClient>()
                                    .WithAsyncProcessing((context, injected, token) =>
                                        injected.SendChatActionAsync(context.Message.From!.Id, ChatAction.Typing,
                                            cancellationToken: token))))
                            .ForCallbackQuery(callbackQuery => callbackQuery
                                .WithInjection<ITelegramBotClient>()
                                .WithAsyncProcessing((context, injected, token) =>
                                    injected.AnswerCallbackQueryAsync(context.CallbackQuery.Id,
                                        cancellationToken: token)))
                            .ForEditedMessage(message => message
                                .ForText(text => text
                                    .WithInjection<ITelegramBotClient>()
                                    .WithAsyncProcessing((context, injected, token) =>
                                        injected.SendChatActionAsync(context.EditedMessage.From!.Id, ChatAction.Typing,
                                            cancellationToken: token))))
                            .Build<ITelegramBotClient>(sp))));
    }

    public static IServiceCollection AddTelegramFlowNewInterface(this IServiceCollection services)
    {
        return services
            .AddFullLogging()
            .AddNewBotCommand<NewMainHandler>(NewMainHandler.Command)
            .AddNewCallbackData<NewMainHandler>(NewMainHandler.Command)
            
            .AddNewCallbackData<NewHistoryHandler>(NewHistoryHandler.Command)
            .AddNewCallbackDataPrefix<NewHistoryHandler>(NewHistoryHandler.CommandPrefix)
            
            .AddNewCallbackData<NewSwitchHandler>(NewSwitchHandler.Command)
            .AddNewCallbackDataPrefix<NewSwitchHandler>(NewSwitchHandler.CommandPrefix);
    }

    private static IServiceCollection AddNewBotCommand<T>(
        this IServiceCollection services,
        params string[] targetCommands) where T : class, IBotCommandHandler
    {
        return services
            .AddBotCommand<T>((command, context, token) =>
                    command.ProcessAsync(context.Data, token),
                targetCommands);
    }

    private static IServiceCollection AddNewCallbackData<T>(
        this IServiceCollection services,
        params string[] targetCommands) where T : class, ICallbackQueryHandler
    {
        return services
            .AddCallbackData<T>((command, context, token) =>
                    context.CallbackQuery.Message?.MessageId is { } messageId
                        ? command.ProcessAsync(messageId, context.Data, token)
                        : Task.CompletedTask,
                targetCommands);
    }

    private static IServiceCollection AddNewCallbackDataPrefix<T>(
        this IServiceCollection services,
        params string[] targetCommands) where T : class, ICallbackQueryHandler
    {
        return services
            .AddCallbackDataPrefix<T>((command, context, token) =>
                    context.CallbackQuery.Message?.MessageId is { } messageId
                        ? command.ProcessAsync(messageId, context.Data, token)
                        : Task.CompletedTask,
                targetCommands);
    }

    private static IServiceCollection AddFullLogging(this IServiceCollection services)
    {
        return services
            .AddScoped<IUpdateHandler>(serviceProvider => serviceProvider
                .WatchServiceProvider(sp => TelegramFlow.New
                    .ForMessage(message => message
                        .ForText(text => text
                            .WithInjection<ITraceService>()
                            .WithAsyncProcessing((context, trace, _) =>
                            {
                                trace.Log(LogLevel.Debug, $"MSG {context.Message.From!.Id} {context.Text}");
                                return Task.CompletedTask;
                            }))
                    )
                    .ForEditedMessage(editedMessage => editedMessage
                        .ForText(text => text
                            .WithInjection<ITraceService>()
                            .WithAsyncProcessing((context, trace, _) =>
                            {
                                trace.Log(LogLevel.Debug, $"EDT {context.EditedMessage.From!.Id} {context.Text}");
                                return Task.CompletedTask;
                            })))
                    .ForCallbackQuery(callbackQuery => callbackQuery
                        .ForData(data => data
                            .ForPrefix("")
                            .WithInjection<ITraceService>()
                            .WithAsyncProcessing((context, trace, _) =>
                            {
                                trace.Log(LogLevel.Debug, $"CLB {context.CallbackQuery.From!.Id} {context.Data}");
                                return Task.CompletedTask;
                            })))
                    .WithDisplayName("FullLogging")
                    .Build<ITraceService>(sp)));
    }

    private static T WatchServiceProvider<T>(
        this IServiceProvider serviceProvider,
        Func<IServiceProvider, T> builder) where T : class
    {
        using (serviceProvider.GetRequiredService<ITraceService>().Fixed("telegram_flow_init"))
        {
            var service = builder(serviceProvider);
            return service;
        }
    }
}