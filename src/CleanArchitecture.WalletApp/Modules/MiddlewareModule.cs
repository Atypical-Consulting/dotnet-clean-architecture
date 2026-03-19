namespace CleanArchitecture.WalletApp.Modules;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using TheAppManager.Modules;

/// <summary>
///     Module for HTTP middleware pipeline configuration.
/// </summary>
public sealed class MiddlewareModule : IAppModule
{
    /// <inheritdoc />
    public void ConfigureMiddleware(WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();
        app.UseAntiforgery();
    }
}
