using Ripplee.ViewModels;

namespace Ripplee.Views;

public partial class SearchingPage : ContentPage
{
    public SearchingPage(SearchingViewModel viewModel) 
    {
        InitializeComponent();
        BindingContext = viewModel; 
    }
}