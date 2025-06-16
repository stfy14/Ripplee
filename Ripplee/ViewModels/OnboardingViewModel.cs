using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ripplee.Misc; 
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
        private ImageSource? selectedAvatarSource; 

        [ObservableProperty]
        private FileResult? pickedAvatarFile; 

        [ObservableProperty]
        private string? validationErrorMessage;


        [ObservableProperty]
        private string greetingMessage = "С возвращением!"; 

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
            ValidationErrorMessage = null; 
            SelectedAvatarSource = null; 
            PickedAvatarFile = null;     
        }

        [RelayCommand]
        private void StartFlow()
        {
            ValidationErrorMessage = null; 
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
            ValidationErrorMessage = null; 

            if (CurrentStepIndex == 1) 
            {
                if (!ValidationHelper.ValidateUsername(Username, out string userError))
                {
                    ValidationErrorMessage = userError;
                    return;
                }

                IsLoading = true; 
                bool userExists = await _apiClient.CheckUserExistsAsync(Username!);
                IsLoading = false;

                Password = string.Empty; 
                if (userExists)
                {
                    GreetingMessage = $"С возвращением, {Username}!";
                    CurrentStepIndex = 4; 
                }
                else
                {
                    CurrentStepIndex = 2; 
                }
            }
            else if (CurrentStepIndex == 2) 
            {
                if (!ValidationHelper.ValidatePassword(Password, out string passError))
                {
                    ValidationErrorMessage = passError;
                    return;
                }
                CurrentStepIndex = 3; 
            }
            else if (CurrentStepIndex == 3) 
            {
                IsLoading = true;
                bool registrationSuccess = await _userService.RegisterAndLoginAsync(Username!, Password!);

                if (registrationSuccess)
                {
                    if (PickedAvatarFile != null)
                    {
                        using var stream = await PickedAvatarFile.OpenReadAsync();
                        bool avatarUploadSuccess = await _userService.UploadAvatarAsync(stream, PickedAvatarFile.FileName);
                        if (!avatarUploadSuccess)
                        {
                            await Shell.Current.DisplayAlert("Информация", "Регистрация прошла успешно, но не удалось загрузить аватар. Вы можете сделать это позже в настройках.", "OK");
                        }
                    }
                    await AnimateAndNavigateToMain();
                }
                else
                {
                    ValidationErrorMessage = "Не удалось завершить регистрацию. Попробуйте другой логин или проверьте соединение.";
                }
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task Login() 
        {
            KeyboardHelper.HideKeyboard();
            ValidationErrorMessage = null;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ValidationErrorMessage = "Введите пароль.";
                return;
            }

            IsLoading = true;
            bool success = await _userService.LoginAsync(Username, Password);
            IsLoading = false;

            if (success)
            {
                await AnimateAndNavigateToMain();
            }
            else
            {
                ValidationErrorMessage = "Неверный логин или пароль.";
            }
        }

        [RelayCommand]
        private async Task PickAvatar()
        {
            try
            {
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    ValidationErrorMessage = "Выбор фото не поддерживается.";
                    return;
                }

                var photoResult = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Выберите аватар"
                });

                if (photoResult != null)
                {
                    PickedAvatarFile = photoResult; 

                    SelectedAvatarSource = ImageSource.FromFile(photoResult.FullPath);
                    ValidationErrorMessage = null; 
                }
            }
            catch (Exception ex)
            {
                ValidationErrorMessage = "Ошибка при выборе фото.";
                System.Diagnostics.Debug.WriteLine($"PickAvatar error: {ex}");
            }
        }

        [RelayCommand]
        private void PreviousStep()
        {
            KeyboardHelper.HideKeyboard();
            ValidationErrorMessage = null;

            if (CurrentStepIndex == 4) 
            {
                CurrentStepIndex = 1;
            }
            else if (CurrentStepIndex == 1) 
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