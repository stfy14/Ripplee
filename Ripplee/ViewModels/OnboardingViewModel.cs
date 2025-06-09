using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ripplee.Services.Interfaces;
using System.Diagnostics;
using Ripplee.Misc.UI;


namespace Ripplee.ViewModels
{
    public partial class OnboardingViewModel : ObservableObject
    {
        private readonly IUserService _userService;

        [ObservableProperty]
        private int currentStepIndex = 0;

        private bool isGuestFlow = false;

        [ObservableProperty]
        private string? username;

        [ObservableProperty]
        private string? password;

        public OnboardingViewModel(IUserService userService)
        {
            _userService = userService;
        }

        [RelayCommand]
        private void StartGuestFlow()
        {
            isGuestFlow = true;
            CurrentStepIndex = 1; // Переходим на шаг ввода имени
            Debug.WriteLine("Started GUEST flow.");
        }

        [RelayCommand]
        private void StartRegistrationFlow()
        {
            isGuestFlow = false;
            CurrentStepIndex = 1; // Переходим на шаг ввода имени
            Debug.WriteLine("Started REGISTRATION flow.");
        }

        [RelayCommand]
        private async Task NextStep()
        {
            KeyboardHelper.HideKeyboard();
            // --- Блок проверки полей ввода ---
            if (isGuestFlow)
            {
                // Проверка для гостя
                if (CurrentStepIndex == 1 && string.IsNullOrWhiteSpace(Username))
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Пожалуйста, введите ваше имя.", "OK");
                    return; // Прерываем выполнение команды
                }
            }
            else // Если это регистрация
            {
                if (CurrentStepIndex == 1 && string.IsNullOrWhiteSpace(Username))
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Пожалуйста, введите ваше имя.", "OK");
                    return;
                }
                if (CurrentStepIndex == 2 && string.IsNullOrWhiteSpace(Password))
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Пожалуйста, придумайте пароль.", "OK");
                    return;
                }
            }
            // --- Конец блока проверки ---


            Debug.WriteLine($"NextStep called. IsGuest: {isGuestFlow}, CurrentStep: {CurrentStepIndex}");

            if (isGuestFlow)
            {
                await ProcessGuestLogin();
            }
            else
            {
                await ProcessRegistrationSteps();
            }
        }

        private async Task ProcessGuestLogin()
        {
            Debug.WriteLine("Processing GUEST login...");
            bool success = await _userService.LoginAsGuestAsync(Username!);
            if (success)
            {
                await NavigateToMainApp();
            }
        }

        private async Task ProcessRegistrationSteps()
        {
            Debug.WriteLine($"Processing REGISTRATION step: {CurrentStepIndex}");
            switch (CurrentStepIndex)
            {
                case 1: // После ввода имени
                    CurrentStepIndex = 2; // Переходим к паролю
                    break;
                case 2: // После ввода пароля
                    CurrentStepIndex = 3; // Переходим к аватару
                    break;
                case 3: // После шага с аватаром
                    Debug.WriteLine("Final registration step. Calling service...");
                    bool success = await _userService.RegisterAndLoginAsync(Username!, Password!);
                    if (success)
                    {
                        await NavigateToMainApp();
                    }
                    break;
            }
        }

        private async Task NavigateToMainApp()
        {
            Debug.WriteLine("Navigating to //MainApp...");
            if (MainThread.IsMainThread)
            {
                await Shell.Current.GoToAsync("//MainApp");
            }
            else
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.GoToAsync("//MainApp");
                });
            }
            Debug.WriteLine("Navigation to //MainApp command sent.");
        }

        // Метод CanExecuteNextStep больше не используется, его можно удалить.
    }
}