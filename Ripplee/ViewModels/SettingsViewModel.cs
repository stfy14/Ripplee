using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ripplee.Services.Interfaces;

namespace Ripplee.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly IUserService _userService;

        [ObservableProperty]
        private string? username;

        // ИЗМЕНЕНО: Конструктор теперь не зависит от MainViewModel
        public SettingsViewModel(IUserService userService)
        {
            _userService = userService;
            Username = userService.CurrentUser.Username;
        }

        [RelayCommand]
        private void ChangePhoto()
        {
            // Реализация изменения фото
        }

        [RelayCommand]
        private void ChangePassword()
        {
            // Реализация изменения пароля
        }

        [RelayCommand]
        private void ChangeEmail()
        {
            // Реализация изменения email
        }

        [RelayCommand]
        private async Task Logout()
        {
            await _userService.LogoutAsync();
            await Shell.Current.GoToAsync("//OnboardingPage");
        }

        [RelayCommand]
        private async Task DeleteAccount()
        {
            bool confirm = await Shell.Current.DisplayAlert(
                "Подтверждение", 
                "Вы уверены, что хотите удалить аккаунт?", 
                "Да", "Отмена");
                
            if (confirm)
            {
                // Реализация удаления аккаунта
            }
        }
    }
}