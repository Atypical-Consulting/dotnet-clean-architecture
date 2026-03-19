namespace CleanArchitecture.WebApi;

using CleanArchitecture.WebApi.Modules.AppModules;
using TheAppManager.Startup;

/// <summary>
///     Program entry point.
/// </summary>
public static class Program
{
    /// <summary>
    ///     Application entry point.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static void Main(string[] args)
    {
        AppManager.Start(args, modules =>
        {
            modules
                .Add<SerilogModule>()
                .Add<AspireDefaultsModule>()
                .Add<ObservabilityModule>()
                .Add<ApplicationServicesModule>();
        });
    }
}
