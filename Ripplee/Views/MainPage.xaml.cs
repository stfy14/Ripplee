using CommunityToolkit.Maui.Views;

namespace Ripplee.Views;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.MainViewModel();
    }
    private async void OnMenuButtonClicked(object sender, EventArgs e)
    {
        if (menuControl.IsMenuOpen) 
        {
            await menuControl.ToggleMenuAsync();
            await menuButton.RotateTo(0, 300); 
        }
        else
        {
            await menuControl.ToggleMenuAsync();
            await menuButton.RotateTo(90, 300); 
        }
    }
}