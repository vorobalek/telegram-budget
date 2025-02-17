using Common.Infrastructure.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Flow;
using Telegram.Flow.Extensions;
using Telegram.Flow.Updates;
using Telegram.Flow.Updates.CallbackQueries.Data;
using Telegram.Flow.Updates.Messages.Texts.BotCommands;
using TelegramBudget.Services.TelegramApi.Handle;
using TelegramBudget.Services.TelegramApi.NewFlow;
using TelegramBudget.Services.TelegramApi.NewFlow.Infrastructure;
using TelegramBudget.Services.TelegramApi.PreHandle;
using TelegramBudget.Services.TelegramApi.UserPrompt;
using Tracee;

namespace TelegramBudget.Services.TelegramApi;

public static class TelegramFlowExtensions
{
    public static IServiceCollection AddTelegramFlow(this IServiceCollection services)
    {
        return services
            .AddPreHandler()

            .AddBotCommand<StartBotCommand>(
                (handler, _, token) => handler.ProcessAsync(token),
                "help")

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
            .AddScoped<IUpdateFlow>(serviceProvider => serviceProvider
                .WatchServiceProvider(
                    $"init_msg_txt_{nameof(TransactionPlainText)}",
                    sp => TelegramFlow.New
                    .ForMessage(message => message
                        .ForText(text => text
                            .WithInjection(sp.GetRequiredService<TransactionPlainText>())
                            .WithAsyncProcessing((context, injected, token) =>
                                injected.ProcessAsync(context.Message, context.Text, token))))
                    .WithDisplayName(nameof(TransactionPlainText))
                    .Build()))

            .AddScoped<TransactionEditedPlainText>()
            .AddScoped<IUpdateFlow>(serviceProvider => serviceProvider
                .WatchServiceProvider(
                    $"init_edt_txt_{nameof(TransactionEditedPlainText)}",
                    sp => TelegramFlow.New
                        .ForEditedMessage(message => message
                            .ForText(text => text
                                .WithInjection(sp.GetRequiredService<TransactionEditedPlainText>())
                                .WithAsyncProcessing((context, injected, token) =>
                                    injected.ProcessAsync(context.EditedMessage, context.Text, token))))
                        .WithDisplayName(nameof(TransactionEditedPlainText))
                        .Build()))

            .AddCallbackData<CmdAllCallback>(
                (handler, context, token) => handler.ProcessAsync(context.CallbackQuery.Message, token),
                "cmd.all")

            .AddCallbackData<MainCallback>(
                (handler, context, token) => handler.ProcessAsync(context.CallbackQuery.Message, token),
                "main.old");
    }

    private static IServiceCollection AddBotCommand<T>(
        this IServiceCollection services,
        Func<T, IBotCommandContext, CancellationToken, Task> processing,
        params string[] targetCommands) where T : class
    {
        return
            services
                .AddScoped<T>()
                .AddScoped<IUpdateFlow>(serviceProvider => serviceProvider
                    .WatchServiceProvider(
                        $"init_msg_cmd_exact_{typeof(T).GetName()}",
                        sp =>
                            TelegramFlow.New
                                .ForMessage(message => message
                                    .ForText(text => text
                                        .ForBotCommand(command =>
                                        {
                                            foreach (var targetCommand in targetCommands)
                                                command.ForExact(targetCommand);

                                            command = command
                                                .WithInjection(sp.GetRequiredService<T>())
                                                .WithAsyncProcessing((context, injected, token) =>
                                                    processing(injected, context, token));

                                            return command;
                                        })))
                                .WithDisplayName(typeof(T).GetName())
                                .Build()));
    }

    private static IServiceCollection AddBotCommandPrefix<T>(
        this IServiceCollection services,
        Func<T, IBotCommandContext, CancellationToken, Task> processing,
        params string[] targetCommands) where T : class
    {
        return
            services
                .AddScoped<T>()
                .AddScoped<IUpdateFlow>(serviceProvider => serviceProvider
                    .WatchServiceProvider(
                        $"init_msg_cmd_prefix_{typeof(T).GetName()}",
                        sp =>
                            TelegramFlow.New
                                .ForMessage(message => message
                                    .ForText(text => text
                                        .ForBotCommand(command =>
                                        {
                                            foreach (var targetCommand in targetCommands)
                                                command.ForPrefix(targetCommand);

                                            command = command
                                                .WithInjection(sp.GetRequiredService<T>())
                                                .WithAsyncProcessing((context, injected, token) =>
                                                    processing(injected, context, token));

                                            return command;
                                        })))
                                .WithDisplayName(typeof(T).GetName())
                                .Build()));
    }

    private static IServiceCollection AddCallbackData<T>(
        this IServiceCollection services,
        Func<T, IDataContext, CancellationToken, Task> processing,
        params string[] targetCommands) where T : class
    {
        return
            services
                .AddScoped<T>()
                .AddScoped<IUpdateFlow>(serviceProvider => serviceProvider
                    .WatchServiceProvider(
                        $"init_clb_data_exact_{typeof(T).GetName()}",
                        sp =>
                        TelegramFlow.New
                            .ForCallbackQuery(callbackQuery => callbackQuery
                                .ForData(data =>
                                {
                                    foreach (var targetCommand in targetCommands) data.ForExact(targetCommand);

                                    data = data
                                        .WithInjection(sp.GetRequiredService<T>())
                                        .WithAsyncProcessing((context, injected, token) =>
                                            processing(injected, context, token));

                                    return data;
                                }))
                            .WithDisplayName(typeof(T).GetName())
                            .Build()));
    }

    private static IServiceCollection AddPreHandler(this IServiceCollection services)
    {
        return
            services
                .AddScoped<IPreHandlerService>(serviceProvider => serviceProvider
                    .WatchServiceProvider(
                        $"init_{nameof(PreHandlerService)}",
                        sp => new PreHandlerService(
                            sp.GetRequiredService<ITracee>(),
                            TelegramFlow.New
                                .ForMessage(message => message
                                    .ForText(text => text
                                        .WithInjection(sp.GetRequiredService<ITelegramBotClient>())
                                        .WithAsyncProcessing((context, injected, token) =>
                                            injected.SendChatAction(
                                                context.Message.From!.Id,
                                                ChatAction.Typing,
                                                cancellationToken: token))))
                                .ForCallbackQuery(callbackQuery => callbackQuery
                                    .WithInjection(sp.GetRequiredService<ITelegramBotClient>())
                                    .WithAsyncProcessing(async (context, injected, token) =>
                                    {
                                        string[] notImplementedCommands = [ DeleteFlow.Command, GrantFlow.Command ];
                                        if (notImplementedCommands.Any(e =>
                                                context.CallbackQuery.Data?.StartsWith(e) ?? false))
                                            return;
                                        await injected.AnswerCallbackQuery(
                                            context.CallbackQuery.Id,
                                            cancellationToken: token);
                                    }))
                                .ForEditedMessage(message => message
                                    .ForText(text => text
                                        .WithInjection(sp.GetRequiredService<ITelegramBotClient>())
                                        .WithAsyncProcessing((context, injected, token) =>
                                            injected.SendChatAction(
                                                context.EditedMessage.From!.Id,
                                                ChatAction.Typing,
                                                cancellationToken: token))))
                                .Build())));
    }

    private static IServiceCollection AddCallbackDataPrefix<T>(
        this IServiceCollection services,
        Func<T, IDataContext, CancellationToken, Task> processing,
        params string[] targetPrefixes) where T : class
    {
        return
            services
                .AddScoped<T>()
                .AddScoped<IUpdateFlow>(serviceProvider => serviceProvider
                    .WatchServiceProvider(
                        $"init_clb_data_prefix_{typeof(T).GetName()}",
                        sp =>
                            TelegramFlow.New
                                .ForCallbackQuery(callbackQuery => callbackQuery
                                    .ForData(data =>
                                    {
                                        foreach (var targetPrefix in targetPrefixes) data.ForPrefix(targetPrefix);

                                        data = data
                                            .WithInjection(sp.GetRequiredService<T>())
                                            .WithAsyncProcessing((context, injected, token) =>
                                                processing(injected, context, token));

                                        return data;
                                    }))
                                .WithDisplayName(typeof(T).GetName())
                                .Build()));
    }

    public static IServiceCollection AddTelegramFlowNewInterface(this IServiceCollection services)
    {
        return services
            .EnableFullLogging()
            .AddUserPrompt()
            .AddBotCommandFlow<MainFlow>(MainFlow.Command)
            .AddCallbackDataFlow<MainFlow>(MainFlow.Command, MainFlow.CommandShow)
            .AddCallbackDataFlow<HistoryFlow>(HistoryFlow.Command)
            .AddCallbackDataPrefixFlow<HistoryFlow>(HistoryFlow.CommandPrefix)
            .AddCallbackDataFlow<SwitchFlow>(SwitchFlow.Command)
            .AddCallbackDataPrefixFlow<SwitchFlow>(SwitchFlow.CommandPrefix)
            .AddCallbackDataFlow<CreateFlow>(CreateFlow.Command)
            .AddBotCommandFlow<CancelFlow>(CancelFlow.Command)
            .AddCallbackDataFlow<GrantFlow>(GrantFlow.Command)
            .AddCallbackDataFlow<DeleteFlow>(DeleteFlow.Command)
            .AddCallbackDataPrefixFlow<DeleteFlow>(DeleteFlow.CommandPrefix);
    }

    private static IServiceCollection AddBotCommandFlow<T>(
        this IServiceCollection services,
        params string[] targetCommands) where T : class, IBotCommandFlow
    {
        return services
            .AddBotCommand<T>((flow, context, token) =>
                    flow.ProcessAsync(context, token),
                targetCommands);
    }

    private static IServiceCollection AddCallbackDataFlow<T>(
        this IServiceCollection services,
        params string[] targetCommands) where T : class, ICallbackQueryFlow
    {
        return services
            .AddCallbackData<T>((flow, context, token) =>
                    context.CallbackQuery.Message?.Id is not null
                        ? flow.ProcessAsync(context, token)
                        : Task.CompletedTask,
                targetCommands);
    }

    private static IServiceCollection AddCallbackDataPrefixFlow<T>(
        this IServiceCollection services,
        params string[] targetCommands) where T : class, ICallbackQueryFlow
    {
        return services
            .AddCallbackDataPrefix<T>((flow, context, token) =>
                    context.CallbackQuery.Message?.Id is not null
                        ? flow.ProcessAsync(context, token)
                        : Task.CompletedTask,
                targetCommands);
    }

    private static IServiceCollection AddTextFlow<T>(this IServiceCollection services) 
        where T : class, ITextFlow
    {
        return services
            .AddScoped<T>()
            .AddScoped<IUpdateFlow>(serviceProvider => serviceProvider
                .WatchServiceProvider(
                    $"init_msg_txt_{typeof(T).GetName()}",
                    sp => TelegramFlow.New
                        .ForMessage(message => message
                            .ForText(text => text
                                .WithInjection(sp.GetRequiredService<T>())
                                .WithAsyncProcessing((context, injected, token) =>
                                    injected.ProcessAsync(context, token))))
                        .WithDisplayName(typeof(T).GetName())
                        .Build()));
    }

    private static IServiceCollection AddUserPrompt(this IServiceCollection services)
    {
        return services
            .AddScoped<IUserPromptService, UserPromptService>()
            .AddScoped<IUserPromptFlow, CreateFlow>();
    }

    private static IServiceCollection EnableFullLogging(this IServiceCollection services)
    {
        return services
            .AddScoped<IUpdateFlow>(serviceProvider => serviceProvider
                .WatchServiceProvider(
                    "init_FullLogging",
                    sp => TelegramFlow.New
                        .ForMessage(message => message
                            .ForText(text => text
                                .WithInjection(sp.GetRequiredService<ITracee>())
                                .WithAsyncProcessing((context, tracee, _) =>
                                {
                                    tracee.Logger.Log(
                                        LogLevel.Debug,
                                        "MSG {FromId} {Text}",
                                        context.Message.From!.Id,
                                        context.Text);
                                    return Task.CompletedTask;
                                })))
                        .ForEditedMessage(editedMessage => editedMessage
                            .ForText(text => text
                                .WithInjection(sp.GetRequiredService<ITracee>())
                                .WithAsyncProcessing((context, tracee, _) =>
                                {
                                    tracee.Logger.Log(
                                        LogLevel.Debug,
                                        "EDT {FromId} {Text}",
                                        context.EditedMessage.From!.Id,
                                        context.Text);
                                    return Task.CompletedTask;
                                })))
                        .ForCallbackQuery(callbackQuery => callbackQuery
                            .ForData(data => data
                                .ForPrefix("")
                                .WithInjection(sp.GetRequiredService<ITracee>())
                                .WithAsyncProcessing((context, tracee, _) =>
                                {
                                    tracee.Logger.Log(
                                        LogLevel.Debug,
                                        "CLB {FromId} {Data}",
                                        context.CallbackQuery.From.Id,
                                        context.Data);
                                    return Task.CompletedTask;
                                })))
                        .WithDisplayName("FullLogging")
                        .Build()));
    }

    private static T WatchServiceProvider<T>(
        this IServiceProvider serviceProvider,
        string name,
        Func<IServiceProvider, T> builder) where T : class
    {
        using (serviceProvider.GetRequiredService<ITracee>().Fixed("init_total"))
        {
            var service = builder(serviceProvider);
            return service;
        }
    }
}