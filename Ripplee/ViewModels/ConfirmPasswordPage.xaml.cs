namespace Ripplee.Views;

public partial class ConfirmPasswordPage : ContentPage
{
    // TaskCompletionSource для возврата результата (введенного пароля или null при отмене)
    private TaskCompletionSource<string?> _tcs;

    public ConfirmPasswordPage()
    {
        InitializeComponent();
        _tcs = new TaskCompletionSource<string?>();
        // Убираем системную навигацию для этой страницы
        Shell.SetNavBarIsVisible(this, false);
    }

    // Публичный метод для отображения страницы и получения результата
    public static async Task<string?> GetPasswordAsync(INavigation navigation)
    {
        var page = new ConfirmPasswordPage();
        await navigation.PushModalAsync(page, false); // false чтобы не было анимации по умолчанию Shell
        return await page._tcs.Task;
    }

    private async void ConfirmButton_Clicked(object sender, EventArgs e)
    {
        string password = PasswordEntry.Text;
        if (string.IsNullOrWhiteSpace(password))
        {
            ErrorMessageLabel.Text = "Пароль не может быть пустым.";
            ErrorMessageLabel.IsVisible = true;
            return;
        }
        ErrorMessageLabel.IsVisible = false;
        _tcs.SetResult(password);
        await Navigation.PopModalAsync(false);
    }

    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        _tcs.SetResult(null); // null означает отмену
        await Navigation.PopModalAsync(false);
    }

    protected override bool OnBackButtonPressed()
    {
        // При нажатии системной кнопки "Назад" также считаем это отменой
        _tcs.SetResult(null);
        // PopModalAsync будет вызван системой, поэтому просто возвращаем true, чтобы перехватить событие
        // и не дать странице закрыться стандартным образом без установки результата.
        // Однако, лучше, чтобы PopModalAsync был вызван явно, как в CancelButton_Clicked.
        // Но для безопасности оставим.
        MainThread.BeginInvokeOnMainThread(async () => await Navigation.PopModalAsync(false));
        return true;
    }
}