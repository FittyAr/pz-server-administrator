using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.FluentUI.AspNetCore.Components;
using pz_server_administrator.Components;
using pz_server_administrator.Services;
using Serilog;
using Serilog.Events;

// Configuración de bootstrap logger para capturar cualquier fallo durante el arranque del host
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando aplicación Project Zomboid Server Administrator...");
    var builder = WebApplication.CreateBuilder(args);

    // Determinar la ruta de almacenamiento de logs de aplicación (persistente en Docker o local)
    var logsPath = Directory.Exists("/app/config")
        ? "/app/config/logs"
        : Path.Combine(AppContext.BaseDirectory, "logs");

    if (!Directory.Exists(logsPath))
    {
        Directory.CreateDirectory(logsPath);
    }

    // Configuración completa de Serilog como el logger de la aplicación
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: Path.Combine(logsPath, "pz-admin-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                fileSizeLimitBytes: 50 * 1024 * 1024,
                rollOnFileSizeLimit: true,
                shared: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}");
    });

    // Configure forwarded headers for reverse proxy (1Panel / OpenResty / Nginx)
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    });

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents(options =>
        {
            options.DetailedErrors = true;
        });
    builder.Services.AddFluentUIComponents();

    // Register application services
    builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
    builder.Services.AddSingleton<IPasswordHashingService, PasswordHashingService>();
    builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ILocalizationService, LocalizationService>();
    builder.Services.AddSingleton<IPzServerService, PzServerService>();
    builder.Services.AddSingleton<ISqliteService, SqliteService>();
    builder.Services.AddScoped<IRconService, RconService>();
    builder.Services.AddScoped<IDatabaseContextFactory, DatabaseContextFactory>();
    builder.Services.AddScoped<IServerLoggerService, ServerLoggerService>();
    builder.Services.AddHttpClient();
    builder.Services.AddScoped<IModDiscoveryService, ModDiscoveryService>();
    builder.Services.AddScoped<IAiService, AiService>();
    builder.Services.AddScoped<pz_server_administrator.Services.Ai.GeminiProvider>();
    builder.Services.AddScoped<pz_server_administrator.Services.Ai.OpenAiProvider>();
    builder.Services.AddScoped<pz_server_administrator.Services.Ai.AnthropicProvider>();
    builder.Services.AddScoped<pz_server_administrator.Services.Ai.OllamaProvider>();
    builder.Services.AddScoped<pz_server_administrator.Services.Ai.AiProviderFactory>();
    builder.Services.AddScoped<ICommunityService, CommunityService>();
    builder.Services.AddScoped<IModPresetService, ModPresetService>();
    builder.Services.AddScoped<CircuitHandler, BlazorCircuitHandler>();
    builder.Services.AddHostedService<pz_server_administrator.BackgroundServices.PzLogObserver>();

    var app = builder.Build();

    app.UseForwardedHeaders();

    // Registro de peticiones HTTP en Serilog
    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseAntiforgery();

    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó inesperadamente debido a una excepción fatal");
}
finally
{
    Log.CloseAndFlush();
}
