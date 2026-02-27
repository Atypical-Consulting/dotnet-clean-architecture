using WalletApp.Components;
using WalletApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults (OpenTelemetry, health checks, service discovery, resilience)
builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<AccountApiClient>(client =>
{
    // When running under Aspire, "https+http://webapi" resolves via service discovery.
    // Falls back to configuration or localhost for standalone usage.
    var apiBaseUrl = builder.Configuration["services:webapi:https:0"]
                    ?? builder.Configuration["services:webapi:http:0"]
                    ?? builder.Configuration["ApiBaseUrl"]
                    ?? "http://localhost:5000";
    client.BaseAddress = new Uri(apiBaseUrl);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapDefaultEndpoints();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
