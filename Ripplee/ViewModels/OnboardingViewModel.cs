// Файл: Ripplee/ViewModels/OnboardingViewModel.cs

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ripplee.Services.Interfaces;
using Ripplee.Misc.UI; // Для KeyboardHelper
using System.Diagnostics;

namespace Ripplee.ViewModels
{
    /// <summary>
    /// Определяет направление анимации для переключения шагов.
    /// </summary>
    public enum AnimationDirection { None, Forward, Backward }

    public partial class OnboardingViewModel : ObservableObject
    {
        private readonly IUserService _userService;

        /// <summary>
        /// Индекс текущего шага онбординга (0, 1, 2, 3).
        /// </summary>
        [ObservableProperty]
        private int currentStepIndex = 0;

        /// <summary>
        /// Сигнализирует View о необходимости начать анимацию смены шага.
        /// </summary>
        [ObservableProperty]
        private AnimationDirection stepChangeDirection = AnimationDirection.None;

        /// <summary>
        /// Сигнализирует View о необходимости начать анимацию выхода и перехода на главный экран.
        /// </summary>
        [ObservableProperty]
        private bool isNavigatingToMainApp = false;

        private bool isGuestFlow = false;

        [ObservableProperty]
        private string? username;

        [ObservableProperty]
        private string? password;

        public OnboardingViewModel(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Вызывается из View ПОСЛЕ завершения анимации "ухода" старого шага.
        /// Финализирует смену шага.
        /// </summary>
        [RelayCommand]
        private void CompleteStepChange()
        {
            if (StepChangeDirection == AnimationDirection.Forward)
            {
                CurrentStepIndex++;
            }
            else if (StepChangeDirection == AnimationDirection.Backward)
            {
                CurrentStepIndex--;
            }

            // Сбрасываем флаг, чтобы остановить цикл анимаций
            StepChangeDirection = AnimationDirection.None;
        }

        [RelayCommand]
        private void StartGuestFlow()
        {
            isGuestFlow = true;
            StepChangeDirection = AnimationDirection.Forward; // Запускаем анимацию на следующий шаг
            Debug.WriteLine("Started GUEST flow.");
        }

        [RelayCommand]
        private void StartRegistrationFlow()
        {
            isGuestFlow = false;
            StepChangeDirection = AnimationDirection.Forward; // Запускаем анимацию на следующий шаг
            Debug.WriteLine("Started REGISTRATION flow.");
        }

        [RelayCommand]
        private void PreviousStep()
        {
            KeyboardHelper.HideKeyboard();
            if (CurrentStepIndex > 0)
            {
                // Запускаем анимацию "назад"
                StepChangeDirection = AnimationDirection.Backward;
            }
        }

        /// <summary>
        /// Основная команда для кнопки "Дальше".
        /// </summary>
        [RelayCommand]
        private async Task NextStep()
        {
            KeyboardHelper.HideKeyboard();

            // Сначала проверяем, валидны ли введенные данные
            if (await IsInputValid() == false)
            {
                return; // Если нет, прерываем выполнение
            }

            Debug.WriteLine($"NextStep called. IsGuest: {isGuestFlow}, CurrentStep: {CurrentStepIndex}");

            if (isGuestFlow)
            {
                // Если это гостевой флоу, он состоит из одного шага, после которого сразу логин
                await ProcessGuestLogin();
            }
            else
            {
                // Если это регистрация, обрабатываем шаги последовательно
                await ProcessRegistrationSteps();
            }
        }

        /// <summary>
        /// Вспомогательный метод для проверки полей ввода.
        /// </summary>
        private async Task<bool> IsInputValid()
        {
            if (isGuestFlow)
            {
                if (CurrentStepIndex == 1 && string.IsNullOrWhiteSpace(Username))
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Пожалуйста, введите ваше имя.", "OK");
                    return false;
                }
            }
            else // Если это регистрация
            {
                if (CurrentStepIndex == 1 && string.IsNullOrWhiteSpace(Username))
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Пожалуйста, введите логин.", "OK");
                    return false;
                }
                if (CurrentStepIndex == 2 && string.IsNullOrWhiteSpace(Password))
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Пожалуйста, придумайте пароль.", "OK");
                    return false;
                }
            }
            return true;
        }

        private async Task ProcessGuestLogin()
        {
            Debug.WriteLine("Processing GUEST login...");
            bool success = await _userService.LoginAsGuestAsync(Username!);
            if (success)
            {
                // Подаем сигнал View для начала анимации и перехода
                NavigateToMainApp();
            }
        }

        private async Task ProcessRegistrationSteps()
        {
            // Если это не последний шаг регистрации, просто переходим на следующий
            if (CurrentStepIndex < 3)
            {
                StepChangeDirection = AnimationDirection.Forward;
            }
            else // Если это последний шаг (выбор аватара)
            {
                Debug.WriteLine("Final registration step. Calling service...");
                bool success = await _userService.RegisterAndLoginAsync(Username!, Password!);
                if (success)
                {
                    // Подаем сигнал View для начала анимации и перехода
                    NavigateToMainApp();
                }
            }
        }

        /// <summary>
        /// Подает сигнал View, что пора запустить анимацию и перейти на главный экран.
        /// </summary>
        private void NavigateToMainApp()
        {
            Debug.WriteLine("Signaling to View to start navigation animation...");
            IsNavigatingToMainApp = true;
        }
    }
}