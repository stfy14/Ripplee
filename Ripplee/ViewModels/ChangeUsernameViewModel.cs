using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ripplee.Misc; // Для ValidationHelper
using Ripplee.Misc.UI; // Для KeyboardHelper
using Ripplee.Services.Interfaces;
using System.Threading.Tasks;

namespace Ripplee.ViewModels
{
    public partial class ChangeUsernameViewModel : ObservableObject
    {
        private readonly IUserService _userService;

        [ObservableProperty]
        private string? newUsername;

        [ObservableProperty]
        private string? currentPassword;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string? generalErrorMessage;

        public ChangeUsernameViewModel(IUserService userService)
        {
            _userService = userService;
        }

        [RelayCommand]
        private async Task SubmitChangeUsername()
        {
            KeyboardHelper.HideKeyboard();
            GeneralErrorMessage = null; // Сброс ошибки

            if (string.IsNullOrWhiteSpace(CurrentPassword))
            {
                GeneralErrorMessage = "Введите текущий пароль для подтверждения.";
                return;
            }

            // Валидация нового логина
            if (!ValidationHelper.ValidateUsername(NewUsername, out string userError))
            {
                GeneralErrorMessage = userError;
                return;
            }

            IsBusy = true;
            var (success, message) = await _userService.ChangeUsernameAsync(NewUsername!, CurrentPassword);
            IsBusy = false;

            if (success)
            {
                await Shell.Current.DisplayAlert("Успех", message ?? "Логин успешно изменен.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                GeneralErrorMessage = $"Ошибка: {message ?? "Не удалось изменить логин."}";
            }
        }
    }
}