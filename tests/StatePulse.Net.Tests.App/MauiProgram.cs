using Microsoft.Extensions.Logging;
using StatePulse.Net.Blazor;
namespace StatePulse.Net.Tests.App;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });
        builder.Services.AddStatePulseServices(o =>
        {
            o.ScanAssemblies = new Type[] { typeof(MauiProgram) };
        });
        builder.Services.AddStatePulseBlazor();
        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
