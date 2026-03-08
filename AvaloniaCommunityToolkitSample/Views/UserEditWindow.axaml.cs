using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Messaging;

namespace AvaloniaCommunityToolkitSample.Views;

public partial class UserEditWindow : Window
{
    public UserEditWindow()
    {
        InitializeComponent();
    }

    public UserEditWindow(ViewModels.UserEditViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += OnCloseRequested;
    }

    private void OnCloseRequested()
    {
        if (DataContext is ViewModels.UserEditViewModel vm)
            vm.CloseRequested -= OnCloseRequested;
        Close();
    }
}
