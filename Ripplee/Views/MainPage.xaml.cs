using Ripplee.ViewModels;
using System.ComponentModel;

namespace Ripplee.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly MainViewModel _viewModel;
        private bool _isInitialPositionSet = false;
        private bool _isAnimatingPanels = false;

        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (!_isInitialPositionSet && width > 0)
            {
                // При первой загрузке ставим панель критериев за экран
                UserCriteriaBorder.TranslationX = this.Width;
                _isInitialPositionSet = true;
            }
        }

        private async void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.IsShowingFilterControls))
            {
                if (_isAnimatingPanels) return;
                await AnimatePanels();
            }
            else if (e.PropertyName == nameof(MainViewModel.IsMenuOpen))
            {
                await AnimateMenuButton();
            }
        }

        private async Task AnimateMenuButton()
        {
            uint duration = 250;
            var easing = _viewModel.IsMenuOpen ? Easing.CubicOut : Easing.CubicIn;
            double rotation = _viewModel.IsMenuOpen ? 90 : 0;
            await menuButton.RotateTo(rotation, duration, easing);
        }

        private async Task AnimatePanels()
        {
            _isAnimatingPanels = true;
            try
            {
                uint animationDuration = 250;
                Easing easingIn = Easing.CubicIn;
                Easing easingOut = Easing.CubicOut;

                if (_viewModel.IsShowingFilterControls)
                {
                    // --- СЦЕНАРИЙ: ПОКАЗЫВАЕМ ПАНЕЛЬ ФИЛЬТРОВ ---

                    // ШАГ 1: Панель критериев уезжает вправо
                    await UserCriteriaBorder.TranslateTo(this.Width, 0, animationDuration, easingIn);
                    UserCriteriaBorder.IsVisible = false; // Прячем ее ПОСЛЕ анимации

                    // ШАГ 2: Панель фильтров выезжает слева
                    FilterBorder.TranslationX = -this.Width; // Ставим за левый край
                    FilterBorder.IsVisible = true;           // Делаем видимой
                    await FilterBorder.TranslateTo(0, 0, animationDuration, easingOut);
                }
                else
                {
                    // --- СЦЕНАРИЙ: ПОКАЗЫВАЕМ ПАНЕЛЬ КРИТЕРИЕВ ---

                    // ШАГ 1: Панель фильтров уезжает влево
                    await FilterBorder.TranslateTo(-this.Width, 0, animationDuration, easingIn);
                    FilterBorder.IsVisible = false; // Прячем ее ПОСЛЕ анимации

                    // ШАГ 2: Панель критериев выезжает справа
                    UserCriteriaBorder.TranslationX = this.Width; // Ставим за правый край
                    UserCriteriaBorder.IsVisible = true;          // Делаем видимой
                    await UserCriteriaBorder.TranslateTo(0, 0, animationDuration, easingOut);
                }
            }
            finally
            {
                _isAnimatingPanels = false;
            }
        }
    }
}