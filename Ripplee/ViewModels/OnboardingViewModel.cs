// Файл: Ripplee/ViewModels/OnboardingViewModel.cs

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Ripplee.Misc.UI;
using Ripplee.Services.Data;
using Ripplee.Services.Interfaces;
using System.Diagnostics;

namespace Ripplee.ViewModels
{
    // Сообщение для сброса состояния
    public sealed class ResetStateMessage { }

    public enum AnimationDirection { None, Forward, Backward }

    // 1. Указываем, что ViewModel теперь является получателем сообщений типа ResetStateMessage
    public partial class OnboardingViewModel : ObservableObject, IRecipient<ResetStateMessage>
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

            // 2. Подписываем ViewModel на получение сообщений.
            // Теперь он будет "слушать" сообщения типа ResetStateMessage.
            WeakReferenceMessenger.Default.Register<ResetStateMessage>(this);
        }

        // 3. Реализуем метод Receive, который будет вызван, когда придет сообщение.
        public void Receive(ResetStateMessage message)
        {
            Debug.WriteLine("OnboardingViewModel received a reset message. Resetting state.");
            // Сбрасываем все свойства ViewModel к их начальным значениям
            CurrentStepIndex = 0;
            StepChangeDirection = AnimationDirection.None;
            IsNavigatingToMainApp = false;
            Username = string.Empty;
            Password = string.Empty;
            GreetingMessage = "С возвращением!";
        }

        [RelayCommand]
        private void StartFlow()
        {
            _nextStepTarget = 1;
            StepChangeDirection = AnimationDirection.Forward;
        }

        [RelayCommand]
        private async Task StartGuestFlow()
        {
            await Shell.Current.DisplayAlert("Функция в разработке", "Вход как гость будет добавлен позже.", "OK");
        }

        [RelayCommand]
        private async Task NextStep()
        {
            KeyboardHelper.HideKeyboard();

            if (CurrentStepIndex == 1)
            {
                if (string.IsNullOrWhiteSpace(Username))
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Введите имя.", "OK");
                    return;
                }
                bool userExists = await _apiClient.CheckUserExistsAsync(Username);
                Password = string.Empty;
                _nextStepTarget = userExists ? 4 : 2;
                if (userExists) GreetingMessage = $"С возвращением, {Username}!";
                StepChangeDirection = AnimationDirection.Forward;
            }
            else if (CurrentStepIndex == 2)
            {
                if (string.IsNullOrWhiteSpace(Password))
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Придумайте пароль.", "OK");
                    return;
                }
                _nextStepTarget = 3;
                StepChangeDirection = AnimationDirection.Forward;
            }
            else if (CurrentStepIndex == 3)
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

        [RelayCommand]
        private void CompleteStepChange()
        {
            if (StepChangeDirection == AnimationDirection.Forward)
            {
                CurrentStepIndex = _nextStepTarget;
            }
            else if (StepChangeDirection == AnimationDirection.Backward)
            {
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