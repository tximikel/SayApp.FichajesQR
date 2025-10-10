using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using CommunityToolkit.Maui;
using BarcodeScanning.Native.Maui;   // ✅ correcto para la versión 1.2.5

namespace SayApp.FichajesQR.LectorAndroid;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseBarcodeScanningMaui()         // ✅ correcta en v1.2.5
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        return builder.Build();
    }
}
