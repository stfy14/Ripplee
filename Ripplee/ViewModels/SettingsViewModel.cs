using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Media; // Для MediaPicker
using Ripplee.Models;
using Ripplee.Services.Interfaces;
using Ripplee.Services.Services; // Для UserChangedMessage
using System.Diagnostics;
using Ripplee.Views;
using System.Threading.Tasks; // Для Task

namespace Ripplee.ViewModels
{
    public partial class SettingsViewModel : ObservableObject, IRecipient<UserChangedMessage>
    {
        private readonly IUserService _userService;

        [ObservableProperty]
        private UserModel currentUser;

        [ObservableProperty]
        private bool isBusy;

        public SettingsViewModel(IUserService userService)
        {
            _userService = userService;
            CurrentUser = _userService.CurrentUser; 
            WeakReferenceMessenger.Default.Register<UserChangedMessage>(this);
        }

        public void Receive(UserChangedMessage message)
        {
            if (message.NewUser != null)
            {
                // Важно обновлять на UI потоке
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    CurrentUser = message.NewUser;
                });
            }
            else // Если пришел null, значит пользователь разлогинился
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    CurrentUser = new UserModel(); // Сбрасываем пользователя
                });
            }
        }

        [RelayCommand]
        private async Task ChangePhoto()
        {
            try
            {
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    await Shell.Current.DisplayAlert("Нет поддержки", "Выбор фото не поддерживается на этом устройстве.", "OK");
                    return;
                }

                var photoResult = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Выберите аватар"
                });

                if (photoResult == null)
                    return;

                IsBusy = true;

                using var stream = await photoResult.OpenReadAsync();
                bool success = await _userService.UploadAvatarAsync(stream, photoResult.FileName);

                IsBusy = false;

                if (success)
                {
                    await Shell.Current.DisplayAlert("Успех", "Аватар обновлен.", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Не удалось загрузить аватар.", "OK");
                }
            }
            catch (PermissionException pEx)
            {
                Debug.WriteLine($"PermissionException in ChangePhoto: {pEx.Message}");
                await Shell.Current.DisplayAlert("Нет разрешения", "Необходимо разрешение на доступ к галерее.", "OK");
            }
            catch (Exception ex)
            {
                IsBusy = false;
                Debug.WriteLine($"ChangePhoto error: {ex}");
                await Shell.Current.DisplayAlert("Ошибка", "Произошла ошибка при выборе фото.", "OK");
            }
        }

        [RelayCommand]
        private async Task ChangePassword()
        {
            await Shell.Current.GoToAsync(nameof(Views.ChangePasswordPage));
        }

        [RelayCommand]
        private async Task ChangeUsername() // Переименовано из ChangeEmail
        {
            await Shell.Current.GoToAsync(nameof(Views.ChangeUsernamePage));
        }

        [RelayCommand]
        private async Task Logout()
        {
            IsBusy = true;
            await _userService.LogoutAsync();
            IsBusy = false;
            await Shell.Current.GoToAsync("//OnboardingPage");
        }

        [RelayCommand]
        private async Task DeleteAccount()
        {
            bool confirmDelete = await Shell.Current.DisplayAlert(
                "Подтверждение удаления",
                "Вы уверены, что хотите удалить свой аккаунт? Это действие необратимо.",
                "Да, продолжить", "Отмена");

            if (!confirmDelete) return;

            string? password = await ConfirmPasswordPage.GetPasswordAsync(Shell.Current.Navigation);

            if (password == null) 
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            IsBusy = true;
            var (success, message) = await _userService.DeleteAccountAsync(password);
            IsBusy = false;

            if (success)
            {
                await Shell.Current.GoToAsync("//OnboardingPage");
            }
            else
            {
                await Shell.Current.DisplayAlert("Ошибка удаления", message ?? "Не удалось удалить аккаунт.", "OK");
            }
        }
    }
}