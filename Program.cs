global using LPlus;
using Common.Database.Extensions;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using TelegramBudget.Configuration;
using TelegramBudget.Data;
using TelegramBudget.Extensions;
using TelegramBudget.Middleware;
using TelegramBudget.Services;
using TelegramBudget.Services.CurrentUser;
using TelegramBudget.Services.DateTimeProvider;
using TelegramBudget.Services.TelegramBotClientWrapper;
using Tracee.AspNetCore.Extensions;

var host = Host
    .CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(builder =>
    {
        builder.ConfigureAppConfiguration((context, configurationBuilder) =>
        {
            var localizationFiles = Directory
                .EnumerateFiles(
                    Path.Combine(context.HostingEnvironment.ContentRootPath, "Localization"),
                    "*.json",
                    SearchOption.TopDirectoryOnly);

            foreach (var localizationFile in localizationFiles)
                configurationBuilder
                    .AddJsonFile(
                        localizationFile,
                        true,
                        true);
        });

        builder.ConfigureServices((context, services) =>
        {
            services.AddScoped<ExceptionHandlerMiddleware>();
            services.AddScoped<SecretTokenValidatorMiddleware>();
            TR.Configure(options =>
            {
                options.DetermineLanguageCodeDelegate = () => AppConfiguration.Locale;
                options.BuildTranslationKeyDelegate = (languageCode, text) => $"Localization:{languageCode}:{text}";
                options.TryGetTranslationDelegate =
                    translationKey => context.Configuration.GetValue<string>(translationKey);
#if DEBUG
                var path = Path.Combine(
                    context.HostingEnvironment.ContentRootPath,
                    "missing-translation-keys.txt");
                options.MissingTranslationKeyOutputDelegate = translationKey =>
                {
                    File.AppendAllLines(path, [translationKey]);
                };
#endif
            });
            services
                .AddHostedService<ConfigureWebhookHostedService>()
                .AddHttpClient("Telegram.Webhook")
                .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(
                    TelegramBotConfiguration.BotToken,
                    httpClient));
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(AppConfiguration.DbConnectionString ??
                                  context.Configuration.GetConnectionString("Default"));
            });
            services.AddCommonDatabaseFeatures<ApplicationDbContext>();
            services.ConfigureTelegramBotMvc().AddControllers().AddNewtonsoftJson();
            services.AddHealthChecks();

            services.AddSingleton<GlobalCancellationTokenSource>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ITelegramBotWrapper, TelegramBotWrapper>();
            services.AddTelegramHandlers();

            services.AddHttpContextAccessor();
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

            services.AddTracee("api");
        });

        builder.ConfigureLogging(logging =>
        {
            if (!string.IsNullOrWhiteSpace(SentryConfiguration.Dsn))
                logging.AddSentry(configuration =>
                {
                    configuration.Dsn = SentryConfiguration.Dsn;
                    configuration.MinimumEventLevel = SentryConfiguration.MinimumEventLevel;
                    configuration.DisableDuplicateEventDetection();
                });
        });

        builder.Configure(app =>
        {
            if (!string.IsNullOrWhiteSpace(AppConfiguration.PathBase))
                app.UsePathBase(AppConfiguration.PathBase);
            app.UseHealthChecks("/health");
            app.UseTracee(
                "request",
                postRequestAsync: tracee =>
                {
                    tracee.CollectAll(LogLevel.Debug);
                    return Task.CompletedTask;
                });
            app.UseMiddleware<ExceptionHandlerMiddleware>();
            app.UseWhen(
                context => context.Request.Path.StartsWithSegments("/bot"),
                a => a.UseMiddleware<SecretTokenValidatorMiddleware>());
            app.UseRouting();
            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    "Telegram.Webhook",
                    "/bot",
                    new { controller = "Webhook", action = "Post" });
                endpoints.MapControllers();
            });
        });

        builder.UseUrls($"http://+:{AppConfiguration.Port}");
    })
    .Build();

await using var asyncScope = host
    .Services
    .CreateAsyncScope();

var database = asyncScope
    .ServiceProvider
    .GetRequiredService<ApplicationDbContext>()
    .Database;

if (database.IsRelational())
    await database.MigrateAsync();

await host.RunAsync();