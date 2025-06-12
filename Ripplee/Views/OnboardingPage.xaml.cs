using Ripplee.Services.Interfaces;
using Ripplee.ViewModels;
using System.ComponentModel;

namespace Ripplee.Views
{
    public partial class OnboardingPage : ContentPage
    {
        private static bool _autoLoginCheckPerformed = false;

        private readonly IUserService _userService;
        private OnboardingViewModel? _viewModel;

        private int _currentVisibleStepIndex = 0;
        private bool _isAnimating = false;

        private readonly List<View> _stepLayouts;
        private readonly List<View> _headerLayouts;

        public OnboardingPage(OnboardingViewModel viewModel, IUserService userService)
        {
            InitializeComponent();
            _userService = userService;
            _viewModel = viewModel;
            BindingContext = _viewModel;

            _stepLayouts = [Step0Layout, Step1Layout, Step2Layout, Step3Layout, StepLoginPasswordLayout];
            _headerLayouts = [HeaderStep0, HeaderStep1, HeaderStep2, HeaderStep3, HeaderStep4];
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!_autoLoginCheckPerformed)
            {
                _autoLoginCheckPerformed = true;

                if (await _userService.TryAutoLoginAsync())
                {
                    await Shell.Current.GoToAsync("//MainPage");
                }

            }
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (_viewModel != null)
            {
                _viewModel.ResetState();
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_viewModel == null) return;

            if (e.PropertyName == nameof(OnboardingViewModel.CurrentStepIndex))
            {
                SetStepVisibility(_viewModel.CurrentStepIndex, true); 
            }
        }

        private async void SetStepVisibility(int newStepIndex, bool animate)
        {
            if (_currentVisibleStepIndex == newStepIndex) return;
            if (_isAnimating) return;

            _isAnimating = true;

            View fromView = _stepLayouts[_currentVisibleStepIndex];
            View toView = _stepLayouts[newStepIndex];

            _headerLayouts[_currentVisibleStepIndex].IsVisible = false;
            _headerLayouts[newStepIndex].IsVisible = true;

            if (animate && Width > 0)
            {
                bool isGoingForward = newStepIndex > _currentVisibleStepIndex;
                double translationX = isGoingForward ? Width : -Width;

                toView.TranslationX = translationX;

                toView.IsVisible = true;

                var animationOut = fromView.TranslateTo(-translationX, 0, 300, Easing.CubicOut); 
                var animationIn = toView.TranslateTo(0, 0, 300, Easing.CubicOut);

                await Task.WhenAll(animationIn, animationOut);

                fromView.IsVisible = false;
                fromView.TranslationX = 0; 
            }
            else
            {
                fromView.IsVisible = false;
                toView.IsVisible = true;
            }

            _currentVisibleStepIndex = newStepIndex;
            _isAnimating = false;
        }
    }
}