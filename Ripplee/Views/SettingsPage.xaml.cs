using Ripplee.ViewModels; // Добавили

namespace Ripplee.Views;
public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel) // Запрашиваем ViewModel
    {
        InitializeComponent();
        BindingContext = viewModel; // Присваиваем
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }

    private async void CloseSettingsButton_Clicked(object sender, EventArgs e)
    {
        // Вместо прямого вызова Navigation.PopModalAsync, мы будем использовать Shell-навигацию
        // ".." означает "вернуться назад"
        await Shell.Current.GoToAsync("..", true);

        // Логику анимации можно оставить, если она нужна перед закрытием,
        // но Shell-анимация (true в GoToAsync) часто выглядит достаточно хорошо.
    }
}