namespace Ripplee.Views;

public partial class MenuPanel : ContentView
{
    private bool _isMenuOpen;
    private const uint AnimationDuration = 300;

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

    public async Task ToggleMenuAsync()
    {
        if (_isMenuOpen)
        {
            await menuPanel.TranslateTo(0, -Height, AnimationDuration, Easing.CubicOut);
        }
        else
        {
            await menuPanel.TranslateTo(0, 0, AnimationDuration, Easing.CubicOut);
        }
        _isMenuOpen = !_isMenuOpen;
    }
}