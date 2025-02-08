using CommunityToolkit.Mvvm.Messaging;
using Ripplee.Misc.UI;

namespace Ripplee.Views;
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.MainViewModel();

        WeakReferenceMessenger.Default.Register<CloseMenuMessage>(this, (r, m) =>
        {
            if (menuControl.IsMenuOpen)
            {
                menuControl.ToggleMenu();
                menuButton.RotateTo(0, 300);
            }
        });
    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        WeakReferenceMessenger.Default.Unregister<CloseMenuMessage>(this);
    }

    private void OnMenuButtonClicked(object sender, EventArgs e)
    {
        if (menuControl.IsMenuOpen)
        {
            menuButton.RotateTo(0, 300);
            menuControl.ToggleMenu();
        }
        else
        {
            menuButton.RotateTo(90, 300);
            menuControl.ToggleMenu();
        }
    }
}