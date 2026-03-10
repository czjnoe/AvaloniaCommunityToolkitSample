using System;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaCommunityToolkitSample.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    public string[] ThemeOptions { get; } = ["跟随系统", "浅色", "深色"];

    [ObservableProperty]
    private string _selectedTheme = "跟随系统";

    public SettingsViewModel()
    {
        SelectedTheme = GetCurrentThemeOption();
    }

    private static string GetCurrentThemeOption()
    {
        var current = Application.Current?.RequestedThemeVariant;
        if (current == ThemeVariant.Light) return "浅色";
        if (current == ThemeVariant.Dark) return "深色";
        return "跟随系统";
    }

    [RelayCommand]
    private void Apply()
    {
        if (Application.Current is null) return;

        Application.Current.RequestedThemeVariant = SelectedTheme switch
        {
            "浅色" => ThemeVariant.Light,
            "深色" => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };

        CloseRequested?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke();
    }

    public event Action? CloseRequested;
}

