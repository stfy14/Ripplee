// Файл: Views/MainPage.xaml.cs
using Ripplee.ViewModels;
using System.ComponentModel;

namespace Ripplee.Views;
public partial class MainPage : ContentPage
{
    private MainViewModel _viewModel;
    // Создаем константу для длительности анимации, такую же, как в MenuPanel.xaml.cs
    private const uint ButtonRotationDuration = 400; // <--- ИЗМЕНИТЕ ЭТО ЗНАЧЕНИЕ ПРИ НЕОБХОДИМОСТИ

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        this.Unloaded += MainPage_Unloaded;
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

    private void MainPage_Unloaded(object? sender, EventArgs e)
    {
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        this.Unloaded -= MainPage_Unloaded;
    }
}