
using Microsoft.Maui.Controls;

namespace SayApp.FichajesQR.LectorAndroid;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new NavigationPage(new MainPage());
    }
}
