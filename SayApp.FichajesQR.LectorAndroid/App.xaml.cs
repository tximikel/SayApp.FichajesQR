
using Microsoft.Maui.Controls;

namespace SayApp.FichajesQR.LectorAndroid;

public partial class App : Application
{
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage());
    }
}
