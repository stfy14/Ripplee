namespace Ripplee.Views;

public partial class MenuPanel : ContentView
{
    private const uint AnimationDuration = 400;

    public static readonly BindableProperty IsOpenProperty =
        BindableProperty.Create(nameof(IsOpen), typeof(bool), typeof(MenuPanel), false,
            propertyChanged: OnIsOpenChanged);

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public MenuPanel()
    {
        InitializeComponent();

        SizeChanged += MenuPanel_SizeChanged;

        TranslationY = -10000;
    }

    // 2. —оздаем обработчик дл€ событи€ SizeChanged
    private void MenuPanel_SizeChanged(object? sender, EventArgs e)
    {
        //  ак только размер стал известен, мы можем отписатьс€, 
        // чтобы этот код не выполн€лс€ при каждом изменении размера окна.
        SizeChanged -= MenuPanel_SizeChanged;

        // “еперь, зна€ реальную высоту, ставим меню в правильное начальное положение (за экраном)
        // Ётот код выполнитс€ только один раз.
        TranslationY = -Height;
    }

    private static void OnIsOpenChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MenuPanel menu && newValue is bool isNowOpen)
        {
            menu.AnimateMenu(isNowOpen);
        }
    }

    private void AnimateMenu(bool open)
    {
        // ѕровер€ем, что высота уже была рассчитана, чтобы избежать анимации с высотой -1 или 0
        if (Height <= 0)
            return;

        if (open)
        {
            this.TranslateTo(0, 0, AnimationDuration, Easing.CubicOut);
        }
        else
        {
            this.TranslateTo(0, -Height, AnimationDuration, Easing.CubicIn);
        }
    }
}