using Android.App;
using Android.Content.PM;
using Android.OS;
using Android;
using Android.Content;
using System;
using System.Linq;

namespace SayApp.FichajesQR.LectorAndroid
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                if (CheckSelfPermission(Manifest.Permission.Camera) != Permission.Granted)
                {
                    RequestPermissions(new string[] { Manifest.Permission.Camera }, 0);
                }
            }
        }
    }
}
