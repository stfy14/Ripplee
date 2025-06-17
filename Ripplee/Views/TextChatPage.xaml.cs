using Ripplee.Models; // ��� ChatMessageModel
using Ripplee.ViewModels; // ��� TextChatViewModel
using System.Collections.Specialized; // ��� INotifyCollectionChanged

namespace Ripplee.Views;

public partial class TextChatPage : ContentPage
{
    private TextChatViewModel? _viewModel;

    public TextChatPage(TextChatViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel != null)
        {
            _viewModel.Messages.CollectionChanged += Messages_CollectionChanged;
        }
#if ANDROID
    Platform.CurrentActivity?.Window?.SetStatusBarColor(Android.Graphics.Color.ParseColor("#1c1c1c"));
    Platform.CurrentActivity?.Window?.SetNavigationBarColor(Android.Graphics.Color.ParseColor("#1c1c1c"));
#endif
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (_viewModel != null)
        {
            _viewModel.Messages.CollectionChanged -= Messages_CollectionChanged;
        }
#if ANDROID
    Platform.CurrentActivity?.Window?.SetStatusBarColor(Android.Graphics.Color.ParseColor("#202020"));
    Platform.CurrentActivity?.Window?.SetNavigationBarColor(Android.Graphics.Color.ParseColor("#202020"));
#endif
    }

    private void Messages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // ��������� � ���������� ��������� ��� ���������� ������
        if (e.Action == NotifyCollectionChangedAction.Add && _viewModel != null && _viewModel.Messages.Count > 0)
        {
            var lastMessage = _viewModel.Messages[_viewModel.Messages.Count - 1];
            // �������� �����, ����� ���� UI ����� ���������� ����� ������� ����� ����������
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
            {
                MessagesCollectionView.ScrollTo(lastMessage, position: ScrollToPosition.End, animate: true);
            });
        }
    }
}