using Ripplee.ViewModels;

namespace Ripplee.Views;

public partial class VoiceChatPage : ContentPage
{
    public VoiceChatPage(VoiceChatViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}