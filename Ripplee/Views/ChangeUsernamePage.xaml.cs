using Ripplee.ViewModels;

namespace Ripplee.Views;

public partial class ChangeUsernamePage : ContentPage
{
    public ChangeUsernamePage(ChangeUsernameViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}