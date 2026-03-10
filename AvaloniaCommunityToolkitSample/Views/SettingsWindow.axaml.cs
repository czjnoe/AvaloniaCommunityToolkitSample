using Avalonia.Controls;

namespace AvaloniaCommunityToolkitSample.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    public SettingsWindow(ViewModels.SettingsViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += OnCloseRequested;
    }

    private void OnCloseRequested()
    {
        if (DataContext is ViewModels.SettingsViewModel vm)
            vm.CloseRequested -= OnCloseRequested;
        Close();
    }
}

