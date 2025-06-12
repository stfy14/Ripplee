using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ripplee.Misc.UI;
using Ripplee.Services.Data;
using Ripplee.Services.Interfaces;

namespace Ripplee.ViewModels
{
    public partial class OnboardingViewModel : ObservableObject
    {
        private readonly IUserService _userService;
        private readonly ChatApiClient _apiClient;

        [ObservableProperty]
        private bool isLoading = true;

        [ObservableProperty]
        private int currentStepIndex = 0;

        [ObservableProperty]
        private string? username;

        [ObservableProperty]
        private string? password;

        [ObservableProperty]
        private string greetingMessage;

        public OnboardingViewModel(IUserService userService, ChatApiClient apiClient)
        {
            _userService = userService;
            _apiClient = apiClient;
        }

        [RelayCommand]
        private async Task InitializeAsync()
        {
            ResetState();

            if (await _userService.TryAutoLoginAsync())
            {
                await AnimateAndNavigateToMain();
            }
            else
            {
                IsLoading = false;
            }
        }

        public void ResetState()
        {
            IsLoading = true; 
            CurrentStepIndex = 0;
            Password = string.Empty;
            Username = string.Empty;
            GreetingMessage = "С возвращением!";
        }

        [RelayCommand]
        private void StartFlow()
        {
            CurrentStepIndex = 1;
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
                if (userExists) GreetingMessage = $"С возвращением, {Username}!";
                CurrentStepIndex = userExists ? 4 : 2;
            }
            else if (CurrentStepIndex == 2)
            {
                if (string.IsNullOrWhiteSpace(Password))
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Придумайте пароль.", "OK");
                    return;
                }
                CurrentStepIndex = 3;
            }
            else if (CurrentStepIndex == 3) 
            {
                bool success = await _userService.RegisterAndLoginAsync(Username!, Password!);
                if (success)
                {
                    await AnimateAndNavigateToMain();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Не удалось завершить регистрацию.", "OK");
                }
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
            if (success)
            {
                await AnimateAndNavigateToMain();
            }
            else
            {
                await Shell.Current.DisplayAlert("Ошибка входа", "Неверный пароль.", "OK");
            }
        }

        [RelayCommand]
        private void PreviousStep()
        {
            KeyboardHelper.HideKeyboard();

            if (CurrentStepIndex == 4 || CurrentStepIndex == 1)
            {
                CurrentStepIndex = 0;
            }
            else if (CurrentStepIndex > 0)
            {
                CurrentStepIndex--;
            }
        }

        private async Task AnimateAndNavigateToMain()
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}