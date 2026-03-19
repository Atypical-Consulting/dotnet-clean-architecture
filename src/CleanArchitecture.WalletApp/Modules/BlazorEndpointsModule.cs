namespace CleanArchitecture.WalletApp.Modules;

using CleanArchitecture.WalletApp.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using TheAppManager.Modules;

/// <summary>
///     Module for mapping static assets and Blazor Razor component endpoints.
/// </summary>
public sealed class BlazorEndpointsModule : IAppModule
{
    /// <inheritdoc />
    public void ConfigureEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapStaticAssets();
        endpoints.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
    }
}
