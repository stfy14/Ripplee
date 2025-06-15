using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ripplee.Misc; // Для ValidationHelper
using Ripplee.Misc.UI; // Для KeyboardHelper
using Ripplee.Services.Data;
using Ripplee.Services.Interfaces;
using System.Threading.Tasks;

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
        private string greetingMessage = "С возвращением!"; // Инициализируем

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
            ValidationErrorMessage = null; // Сброс ошибки при переходе
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
            ValidationErrorMessage = null; // Сброс предыдущей ошибки

            if (CurrentStepIndex == 1) // Ввод логина
            {
                if (!ValidationHelper.ValidateUsername(Username, out string userError))
                {
                    ValidationErrorMessage = userError;
                    // await Shell.Current.DisplayAlert("Ошибка логина", userError, "OK"); // Можно заменить на Label
                    return;
                }

                IsLoading = true; // Показываем индикатор на время проверки
                bool userExists = await _apiClient.CheckUserExistsAsync(Username!);
                IsLoading = false;

                Password = string.Empty; // Сбрасываем поле пароля на всякий случай
                if (userExists)
                {
                    GreetingMessage = $"С возвращением, {Username}!";
                    CurrentStepIndex = 4; // Переход к вводу пароля для существующего пользователя
                }
                else
                {
                    CurrentStepIndex = 2; // Переход к созданию пароля для нового пользователя
                }
            }
            else if (CurrentStepIndex == 2) // Ввод пароля (регистрация)
            {
                if (!ValidationHelper.ValidatePassword(Password, out string passError))
                {
                    ValidationErrorMessage = passError;
                    // await Shell.Current.DisplayAlert("Ошибка пароля", passError, "OK");
                    return;
                }
                CurrentStepIndex = 3; // Переход к выбору аватара
            }
            else if (CurrentStepIndex == 3) // Шаг выбора аватара и завершения регистрации
            {
                IsLoading = true;
                // Сначала регистрируем пользователя и логинимся
                bool registrationSuccess = await _userService.RegisterAndLoginAsync(Username!, Password!);

                if (registrationSuccess)
                {
                    // Если пользователь зарегистрирован и залогинен, и выбран аватар, загружаем его
                    if (PickedAvatarFile != null)
                    {
                        using var stream = await PickedAvatarFile.OpenReadAsync();
                        bool avatarUploadSuccess = await _userService.UploadAvatarAsync(stream, PickedAvatarFile.FileName);
                        if (!avatarUploadSuccess)
                        {
                            // Можно показать некритичную ошибку, что аватар не загрузился, но регистрация прошла
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
        private async Task Login() // Для шага 4 (вход существующего пользователя)
        {
            KeyboardHelper.HideKeyboard();
            ValidationErrorMessage = null;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ValidationErrorMessage = "Введите пароль.";
                // await Shell.Current.DisplayAlert("Ошибка", "Введите пароль.", "OK");
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
                // await Shell.Current.DisplayAlert("Ошибка входа", "Неверный пароль.", "OK");
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
                    PickedAvatarFile = photoResult; // Сохраняем результат для последующей загрузки
                                                    // Отображаем выбранное изображение
                    SelectedAvatarSource = ImageSource.FromFile(photoResult.FullPath);
                    ValidationErrorMessage = null; // Сбрасываем ошибку, если была
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

            if (CurrentStepIndex == 4) // Если были на вводе пароля для существующего, возвращаемся к вводу логина
            {
                CurrentStepIndex = 1;
            }
            else if (CurrentStepIndex == 1) // С логина на главный
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