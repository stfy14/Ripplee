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

        // 1. ������������� �� ������� SizeChanged
        this.SizeChanged += MenuPanel_SizeChanged;

        // �������� ������ ������ ����� ������, ����� ��� �� "���������" ��� ������ ��������
        this.TranslationY = -10000;
    }

    // 2. ������� ���������� ��� ������� SizeChanged
    private void MenuPanel_SizeChanged(object? sender, EventArgs e)
    {
        // ��� ������ ������ ���� ��������, �� ����� ����������, 
        // ����� ���� ��� �� ���������� ��� ������ ��������� ������� ����.
        this.SizeChanged -= MenuPanel_SizeChanged;

        // ������, ���� �������� ������, ������ ���� � ���������� ��������� ��������� (�� �������)
        // ���� ��� ���������� ������ ���� ���.
        this.TranslationY = -this.Height;
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
        // ���������, ��� ������ ��� ���� ����������, ����� �������� �������� � ������� -1 ��� 0
        if (this.Height <= 0)
            return;

        if (open)
        {
            this.TranslateTo(0, 0, AnimationDuration, Easing.CubicOut);
        }
        else
        {
            this.TranslateTo(0, -this.Height, AnimationDuration, Easing.CubicIn);
        }
    }
}