using Ripplee.ViewModels;
using System.ComponentModel;

namespace Ripplee.Views;

public partial class MainPage : ContentPage
{
    private MainViewModel _viewModel;
    private const uint ButtonRotationDuration = 400;
    private const uint MainPageFadeDuration = 1000;

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