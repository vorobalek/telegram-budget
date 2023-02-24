using Common.Database.Extensions;
using Microsoft.EntityFrameworkCore;
using Sentry;
using Telegram.Bot;
using TelegramBudget.Configuration;
using TelegramBudget.Data;
using TelegramBudget.Extensions;
using TelegramBudget.Middleware;
using TelegramBudget.Services;

var host = Host
    .CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(builder =>
    {
        builder.ConfigureServices((context, services) =>
        {
            services
                .AddHostedService<ConfigureWebhook>()
                .AddHttpClient("Telegram.Webhook")
                .AddTypedClient<ITelegramBotClient>(httpClient => new TelegramBotClient(
                    TelegramBotConfiguration.BotToken,
                    httpClient));
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(context.Configuration.GetConnectionString("Default") ??
                                  AppConfiguration.ConnectionString);
            });
            services.AddCommonDatabaseFeatures<ApplicationDbContext>();
            services.AddControllers().AddNewtonsoftJson();
            services.AddHealthChecks();

            services.AddTelegramHandlers();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddHttpContextAccessor();
        });

        builder.ConfigureLogging(logging =>
        {
            logging.AddSentry(configuration =>
            {
                configuration.Dsn = SentryConfiguration.Dsn;
                configuration.MinimumEventLevel = SentryConfiguration.MinimumEventLevel;
                configuration.DisableDuplicateEventDetection();
            });
        });

        builder.Configure(app =>
        {
            app.UseHealthChecks("/health");
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