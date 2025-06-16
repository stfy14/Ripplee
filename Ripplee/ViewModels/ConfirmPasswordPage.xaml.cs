namespace Ripplee.Views;

public partial class ConfirmPasswordPage : ContentPage
{
    private TaskCompletionSource<string?> _tcs;

    public ConfirmPasswordPage()
    {
        InitializeComponent();
        _tcs = new TaskCompletionSource<string?>();
        Shell.SetNavBarIsVisible(this, false);
    }

    public static async Task<string?> GetPasswordAsync(INavigation navigation)
    {
        var page = new ConfirmPasswordPage();
        await navigation.PushModalAsync(page, false); 
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
        _tcs.SetResult(null); 
        await Navigation.PopModalAsync(false);
    }

    protected override bool OnBackButtonPressed()
    {
        _tcs.SetResult(null);
        MainThread.BeginInvokeOnMainThread(async () => await Navigation.PopModalAsync(false));
        return true;
    }
}