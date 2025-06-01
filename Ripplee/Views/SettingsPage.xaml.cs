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
            this.TranslateTo(0, 0, 300, Easing.CubicOut), // Длительность 300мс
            this.FadeTo(1, 250) // Длительность 250мс
        );
#else
        this.Opacity = 1;
#endif
    }

    private async void CloseSettingsButton_Clicked(object sender, EventArgs e)
    {
#if WINDOWS
        // Только для Windows применяем кастомную анимацию "исчезновения"
        double screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;

        // Анимируем исчезновение: сдвиг вниз и затухание
        await Task.WhenAll(
            this.TranslateTo(0, screenHeight, 300, Easing.CubicIn),
            this.FadeTo(0, 250)
        );

        // Закрываем модальное окно БЕЗ стандартной анимации Windows
        await Navigation.PopModalAsync(false);
#else
        // Для других платформ используем стандартное закрытие с их анимацией
        await Navigation.PopModalAsync(true);
#endif
    }
}