// OnboardingPage.xaml.cs (ИСПРАВЛЕННАЯ ВЕРСИЯ)

using Ripplee.ViewModels;
using System.ComponentModel;

namespace Ripplee.Views;

public partial class OnboardingPage : ContentPage
{
    private OnboardingViewModel? _viewModel;

    public OnboardingPage(OnboardingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    // ✅ ИСПРАВЛЕНО: Используем OnAppearing
    // Этот метод вызывается каждый раз, когда страница появляется на экране.
    protected override void OnAppearing()
    {
        base.OnAppearing();

        // ✅ ВАЖНО: Сбрасываем прозрачность при каждом появлении страницы.
        // Это нужно, чтобы после возврата на эту страницу она не осталась невидимой.
        RootGrid.Opacity = 1;

        if (_viewModel != null)
        {
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }
    }

    // ✅ ИСПРАВЛЕНО: Используем OnDisappearing
    // Этот метод вызывается, когда страница уходит с экрана.
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Реагируем на смену шага
        if (e.PropertyName == nameof(OnboardingViewModel.StepChangeDirection))
        {
            HandleStepAnimation();
        }
        // ✅ НОВЫЙ БЛОК: Реагируем на сигнал к финальной навигации
        else if (e.PropertyName == nameof(OnboardingViewModel.IsNavigatingToMainApp))
        {
            if (_viewModel is not null && _viewModel.IsNavigatingToMainApp)
            {
                AnimateAndNavigateToMain();
            }
        }
    }

    private async void AnimateAndNavigateToMain()
    {
        // Плавно делаем всю страницу невидимой
        await RootGrid.FadeTo(0, 400, Easing.CubicIn);

        // После завершения анимации — выполняем переход
        await Shell.Current.GoToAsync("//MainApp");
    }

    private async void HandleStepAnimation()
    {
        if (_viewModel is null || _viewModel.StepChangeDirection == AnimationDirection.None)
            return;

        View? currentView = GetViewForStep(_viewModel.CurrentStepIndex);
        if (currentView == null) return;

        bool isGoingForward = _viewModel.StepChangeDirection == AnimationDirection.Forward;

        // Анимация "ухода" текущего вида
        double translationX = isGoingForward ? -this.Width : this.Width;
        await currentView.TranslateTo(translationX, 0, 300, Easing.CubicIn);

        // Говорим ViewModel, что можно поменять CurrentStepIndex
        if (_viewModel.CompleteStepChangeCommand.CanExecute(null))
        {
            _viewModel.CompleteStepChangeCommand.Execute(null);
        }

        // Находим новый вид, который стал видимым
        View? nextView = GetViewForStep(_viewModel.CurrentStepIndex);
        if (nextView == null) return;

        // Анимация "прихода" нового вида
        nextView.TranslationX = -translationX;
        await nextView.TranslateTo(0, 0, 300, Easing.CubicOut);
    }

    private View? GetViewForStep(int stepIndex)
    {
        return stepIndex switch
        {
            0 => Step0Layout,
            1 => Step1Layout,
            2 => Step2Layout,
            3 => Step3Layout,
            _ => null
        };
    }
}