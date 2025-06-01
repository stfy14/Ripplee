namespace Ripplee.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        this.Opacity = 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
#if WINDOWS
        double screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
        this.TranslationY = screenHeight; 

        await Task.WhenAll(
            this.TranslateTo(0, 0, 300, Easing.CubicOut), // ������������ 300��
            this.FadeTo(1, 250) // ������������ 250��
        );
#else
        this.Opacity = 1;
#endif
    }

    private async void CloseSettingsButton_Clicked(object sender, EventArgs e)
    {
#if WINDOWS
        // ������ ��� Windows ��������� ��������� �������� "������������"
        double screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;

        // ��������� ������������: ����� ���� � ���������
        await Task.WhenAll(
            this.TranslateTo(0, screenHeight, 300, Easing.CubicIn),
            this.FadeTo(0, 250)
        );

        // ��������� ��������� ���� ��� ����������� �������� Windows
        await Navigation.PopModalAsync(false);
#else
        // ��� ������ �������� ���������� ����������� �������� � �� ���������
        await Navigation.PopModalAsync(true);
#endif
    }
}