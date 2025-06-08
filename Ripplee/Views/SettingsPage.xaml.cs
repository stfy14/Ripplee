using Ripplee.ViewModels; // ��������

namespace Ripplee.Views;
public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel) // ����������� ViewModel
    {
        InitializeComponent();
        BindingContext = viewModel; // �����������
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }

    private async void CloseSettingsButton_Clicked(object sender, EventArgs e)
    {
        // ������ ������� ������ Navigation.PopModalAsync, �� ����� ������������ Shell-���������
        // ".." �������� "��������� �����"
        await Shell.Current.GoToAsync("..", true);

        // ������ �������� ����� ��������, ���� ��� ����� ����� ���������,
        // �� Shell-�������� (true � GoToAsync) ����� �������� ���������� ������.
    }
}