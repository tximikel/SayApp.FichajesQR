
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using BarcodeScanning; // .UseBarcodeScanning()

namespace SayApp.FichajesQR.LectorAndroid;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseBarcodeScanning(); // Habilita el escaneo nativo (Google ML Kit / Apple Vision)

        return builder.Build();
    }
}
