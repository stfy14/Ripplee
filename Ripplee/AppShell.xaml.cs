using Ripplee.Views;

namespace Ripplee
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(AuthPage), typeof(AuthPage));
        }
    }
}
