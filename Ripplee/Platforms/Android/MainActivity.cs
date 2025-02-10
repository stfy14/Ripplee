using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace Ripplee
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Изменение цвета строки состояния
            Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#202020")); // Ваш цвет

            // Изменение цвета строки навигации 
            Window.SetNavigationBarColor(Android.Graphics.Color.ParseColor("#202020"));
        }
    }

}
