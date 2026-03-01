using Microsoft.FluentUI.AspNetCore.Components;
using pz_server_administrator.Components;
using pz_server_administrator.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
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
builder.Services.AddHostedService<pz_server_administrator.BackgroundServices.PzLogObserver>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Localization initialization is handled per-session by AppLoader

app.Run();
