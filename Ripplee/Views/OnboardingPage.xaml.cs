// Файл: Ripplee/Views/OnboardingPage.xaml.cs

using CommunityToolkit.Mvvm.Messaging; // <-- Добавлен using
using Ripplee.ViewModels;
using System.ComponentModel;

namespace Ripplee.Views
{
    public partial class OnboardingPage : ContentPage
    {
        private OnboardingViewModel? _viewModel;

        public OnboardingPage(OnboardingViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RootGrid.Opacity = 1;

            WeakReferenceMessenger.Default.Send(new ResetStateMessage());

            Step0Layout.IsVisible = true;
            Step1Layout.IsVisible = false;
            Step2Layout.IsVisible = false;
            Step3Layout.IsVisible = false;
            StepLoginPasswordLayout.IsVisible = false;

            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

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
            if (e.PropertyName == nameof(OnboardingViewModel.StepChangeDirection))
            {
                HandleStepAnimation();
            }
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
            await RootGrid.FadeTo(0, 400, Easing.CubicIn);
            await Shell.Current.GoToAsync("//MainApp", true, new Dictionary<string, object>
            {
                { "FromOnboarding", true }
            });
        }

        private async void HandleStepAnimation()
        {
            Step0Layout.ClearValue(IsVisibleProperty);
            Step1Layout.ClearValue(IsVisibleProperty);
            Step2Layout.ClearValue(IsVisibleProperty);
            Step3Layout.ClearValue(IsVisibleProperty);
            StepLoginPasswordLayout.ClearValue(IsVisibleProperty);

            if (_viewModel is null || _viewModel.StepChangeDirection == AnimationDirection.None)
                return;

            View? currentView = GetViewForStep(_viewModel.CurrentStepIndex);
            if (currentView == null) return;

            bool isGoingForward = _viewModel.StepChangeDirection == AnimationDirection.Forward;

            double translationX = isGoingForward ? -this.Width : this.Width;
            await currentView.TranslateTo(translationX, 0, 300, Easing.CubicIn);

            if (_viewModel.CompleteStepChangeCommand.CanExecute(null))
            {
                _viewModel.CompleteStepChangeCommand.Execute(null);
            }

            View? nextView = GetViewForStep(_viewModel.CurrentStepIndex);
            if (nextView == null) return;

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
                4 => StepLoginPasswordLayout,
                _ => null
            };
        }
    }
}