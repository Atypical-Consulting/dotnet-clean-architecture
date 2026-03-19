namespace CleanArchitecture.WalletApp.Modules;

using System;
using CleanArchitecture.WalletApp.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using TheAppManager.Modules;

/// <summary>
///     Module for Blazor Razor components and HTTP client registration.
/// </summary>
public sealed class BlazorServicesModule : IAppModule
{
    /// <inheritdoc />
    public void ConfigureServices(WebApplicationBuilder builder)
    {
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
    }
}
