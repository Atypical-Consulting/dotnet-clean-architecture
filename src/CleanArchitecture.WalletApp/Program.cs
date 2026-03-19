using CleanArchitecture.WalletApp.Modules;
using TheAppManager.Startup;

AppManager.Start(args, modules =>
{
    modules
        .Add<AspireDefaultsModule>()
        .Add<BlazorServicesModule>()
        .Add<MiddlewareModule>()
        .Add<BlazorEndpointsModule>();
});
