namespace Ripplee.Views;

public partial class MenuPanel : ContentView
{
    private bool _isMenuOpen;
    private const uint AnimationDuration = 500;

    public bool IsMenuOpen => _isMenuOpen; 

    public MenuPanel()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object sender, EventArgs e)
    {
        menuPanel.TranslationY = -Height;
    }

    public void ToggleMenu()
    {
        if (_isMenuOpen)
        {
            menuPanel.TranslateTo(0, -Height, AnimationDuration, Easing.CubicOut);
        }
        else

        {
            menuPanel.TranslateTo(0, 0, AnimationDuration, Easing.CubicOut);
        }
        _isMenuOpen = !_isMenuOpen;
    }
    private async void OnSettingsButtonClicked(object sender, EventArgs e)
    {
        var settingsPage = new SettingsPage();

        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.Navigation.PushModalAsync(settingsPage, true);
        }
    }
}