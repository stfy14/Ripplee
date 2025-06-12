using Ripplee.Services.Interfaces;
using Ripplee.Views;

namespace Ripplee
{
    public partial class AppShell : Shell
    {
        public AppShell(IUserService userService)
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(VoiceChatPage), typeof(VoiceChatPage));
        }
    }
}