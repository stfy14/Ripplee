using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ripplee.Misc; // Для ValidationHelper
using Ripplee.Misc.UI; // Для KeyboardHelper
using Ripplee.Services.Interfaces;
using System.Threading.Tasks;

namespace Ripplee.ViewModels
{
    public partial class ChangePasswordViewModel : ObservableObject
    {
        private readonly IUserService _userService;

        [ObservableProperty]
        private string? oldPassword;

        [ObservableProperty]
        private string? newPassword;

        [ObservableProperty]
        private string? confirmNewPassword;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string? generalErrorMessage;

        public ChangePasswordViewModel(IUserService userService)
        {
            _userService = userService;
        }

        [RelayCommand]
        private async Task SubmitChangePassword()
        {
            KeyboardHelper.HideKeyboard();
            GeneralErrorMessage = null; // Сброс ошибки

            if (string.IsNullOrWhiteSpace(OldPassword))
            {
                GeneralErrorMessage = "Введите текущий пароль.";
                return;
            }

            // Валидация нового пароля
            if (!ValidationHelper.ValidatePassword(NewPassword, out string passError))
            {
                GeneralErrorMessage = passError;
                return;
            }

            if (NewPassword != ConfirmNewPassword)
            {
                GeneralErrorMessage = "Новый пароль и подтверждение не совпадают.";
                return;
            }

            IsBusy = true;
            var (success, errorMessage) = await _userService.ChangePasswordAsync(OldPassword, NewPassword!);
            IsBusy = false;

            if (success)
            {
                await Shell.Current.DisplayAlert("Успех", "Пароль успешно изменен.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                GeneralErrorMessage = $"Ошибка: {errorMessage ?? "Не удалось изменить пароль."}";
            }
        }
    }
}