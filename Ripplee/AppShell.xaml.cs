using Ripplee.Services.Interfaces;
using Ripplee.Views;

namespace Ripplee
{
    public partial class AppShell : Shell
    {
        public AppShell(IUserService userService) // Убедись, что userService здесь нужен или удали
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(VoiceChatPage), typeof(VoiceChatPage));
            Routing.RegisterRoute(nameof(SearchingPage), typeof(SearchingPage));
            Routing.RegisterRoute(nameof(ChangePasswordPage), typeof(ChangePasswordPage)); // Добавлено
            Routing.RegisterRoute(nameof(ChangeUsernamePage), typeof(ChangeUsernamePage)); // Добавлено
        }
    }
}