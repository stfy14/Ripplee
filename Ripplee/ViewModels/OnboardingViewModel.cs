// Файл: Ripplee/ViewModels/OnboardingViewModel.cs (ФИНАЛЬНАЯ ИСПРАВЛЕННАЯ ВЕРСИЯ)

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Ripplee.Services.Data;
using Ripplee.Services.Interfaces;
using Ripplee.Misc.UI;
using System.Diagnostics;

namespace Ripplee.ViewModels
{
    public sealed class ResetStateMessage { }

    public enum AnimationDirection { None, Forward, Backward }

    public partial class OnboardingViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly ChatApiClient _apiClient;

        [ObservableProperty]
        private int currentStepIndex = 0;

        [ObservableProperty]
        private AnimationDirection stepChangeDirection = AnimationDirection.None;

        [ObservableProperty]
        private bool isNavigatingToMainApp = false;

        private int _nextStepTarget = 0;

        [ObservableProperty]
        private string? username;

        [ObservableProperty]
        private string? password;

        [ObservableProperty]
        private string greetingMessage = "С возвращением!";

        public OnboardingViewModel(IUserService userService, ChatApiClient apiClient)
        {
            _userService = userService;
            _apiClient = apiClient;
        }

        [RelayCommand]
        private void StartFlow()
        {
            _nextStepTarget = 1;
            StepChangeDirection = AnimationDirection.Forward;
        }

        // Метод для гостя пока не трогаем, он требует отдельной логики
        [RelayCommand]
        private async Task StartGuestFlow()
        {
            // TODO: Implement guest flow with animations
            await Shell.Current.DisplayAlert("Функция в разработке", "Вход как гость будет добавлен позже.", "OK");
        }

        [RelayCommand]
        private async Task NextStep()
        {
            KeyboardHelper.HideKeyboard();

            if (CurrentStepIndex == 1) // --- ШАГ 1: ВВОД ИМЕНИ ---
            {
                if (string.IsNullOrWhiteSpace(Username))
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Введите имя.", "OK");
                    return;
                }
                bool userExists = await _apiClient.CheckUserExistsAsync(Username);
                Password = string.Empty;
                _nextStepTarget = userExists ? 4 : 2; // Цель: 4 (логин) или 2 (регистрация)
                if (userExists) GreetingMessage = $"С возвращением, {Username}!";

                StepChangeDirection = AnimationDirection.Forward; // Запускаем анимацию
            }
            else if (CurrentStepIndex == 2) // --- ШАГ 2: ПАРОЛЬ РЕГИСТРАЦИИ ---
            {
                if (string.IsNullOrWhiteSpace(Password))
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Придумайте пароль.", "OK");
                    return;
                }
                _nextStepTarget = 3; // Цель: шаг аватара
                StepChangeDirection = AnimationDirection.Forward;
            }
            else if (CurrentStepIndex == 3) // --- ШАГ 3: АВАТАР И ФИНАЛ РЕГИСТРАЦИИ ---
            {
                bool success = await _userService.RegisterAndLoginAsync(Username!, Password!);
                if (success) IsNavigatingToMainApp = true;
                else await Shell.Current.DisplayAlert("Ошибка", "Не удалось завершить регистрацию.", "OK");
            }
        }

        [RelayCommand]
        private async Task Login()
        {
            KeyboardHelper.HideKeyboard();
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                await Shell.Current.DisplayAlert("Ошибка", "Введите пароль.", "OK");
                return;
            }
            bool success = await _userService.LoginAsync(Username, Password);
            if (success) IsNavigatingToMainApp = true;
            else await Shell.Current.DisplayAlert("Ошибка входа", "Неверный пароль.", "OK");
        }

        [RelayCommand]
        private void PreviousStep()
        {
            KeyboardHelper.HideKeyboard();
            StepChangeDirection = AnimationDirection.Backward;
        }

        // ✅ ИСПРАВЛЕННЫЙ МЕТОД, КОТОРЫЙ ТЕПЕРЬ РАБОТАЕТ ПРАВИЛЬНО
        [RelayCommand]
        private void CompleteStepChange()
        {
            if (StepChangeDirection == AnimationDirection.Forward)
            {
                // Устанавливаем тот шаг, который был определен в NextStep
                CurrentStepIndex = _nextStepTarget;
            }
            else if (StepChangeDirection == AnimationDirection.Backward)
            {
                // Логика кнопки "назад"
                if (CurrentStepIndex == 4 || CurrentStepIndex == 1)
                {
                    CurrentStepIndex = 0;
                }
                else
                {
                    CurrentStepIndex--;
                }
            }
            StepChangeDirection = AnimationDirection.None;
        }
    }
}