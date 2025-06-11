using Ripplee.Views;
using Ripplee.Services.Interfaces;

namespace Ripplee
{
    public partial class AppShell : Shell
    {
        private readonly IUserService _userService;
        public AppShell(IUserService userService)
        {
            InitializeComponent();
            _userService = userService;

            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(VoiceChatPage), typeof(VoiceChatPage));

            _ = PerformAutoLogin();
        }

        private async Task PerformAutoLogin()
        {
            // Даем приложению "вздохнуть" и инициализироваться
            await Task.Delay(100);

            if (await _userService.TryAutoLoginAsync())
            {
                // Если авто-логин успешен, переходим сразу на главный экран
                // UI-операции нужно выполнять в главном потоке
                await MainThread.InvokeOnMainThreadAsync(async () => {
                    await Shell.Current.GoToAsync("//MainApp");
                });
            }
        }
    }
}
