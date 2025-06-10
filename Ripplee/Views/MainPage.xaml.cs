// MainPage.xaml.cs (ФИНАЛЬНАЯ ИСПРАВЛЕННАЯ ВЕРСИЯ)

using Ripplee.ViewModels;
using System.ComponentModel;
using System.Diagnostics;

namespace Ripplee.Views;

[QueryProperty(nameof(CameFromOnboarding), "FromOnboarding")]
public partial class MainPage : ContentPage
{
    private MainViewModel _viewModel;
    private const uint ButtonRotationDuration = 400;

    public bool CameFromOnboarding { get; set; }

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Просто подписываемся на события ViewModel
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // И отписываемся от них
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
    }

    // ✅ ВСЯ ЛОГИКА АНИМАЦИИ ТЕПЕРЬ ЗДЕСЬ
    private void MainPage_Loaded(object? sender, EventArgs e)
    {
        // Этот код выполнится ПОСЛЕ того, как страница будет готова к отрисовке.
        if (CameFromOnboarding)
        {
            // Запускаем анимацию плавного появления
            RootLayout.FadeTo(1, 600, Easing.CubicOut);

            // Сбрасываем флаг, чтобы анимация не повторялась при возврате на страницу
            CameFromOnboarding = false;
        }
        else
        {
            // Если мы зашли на страницу обычным путем, просто делаем ее видимой
            RootLayout.Opacity = 1;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.IsMenuOpen))
        {
            if (_viewModel.IsMenuOpen)
            {
                menuButton.RotateTo(90, ButtonRotationDuration, Easing.CubicOut);
            }
            else
            {
                menuButton.RotateTo(0, ButtonRotationDuration, Easing.CubicIn);
            }
        }
    }
}