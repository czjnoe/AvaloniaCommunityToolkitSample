using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AvaloniaCommunityToolkitSample.Models;
using AvaloniaCommunityToolkitSample.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AvaloniaCommunityToolkitSample.ViewModels;

/// <summary>
/// 主窗口 ViewModel
/// </summary>
public partial class MainWindowViewModel : ViewModelBase, IRecipient<Messages.UserSavedMessage>
{
    private readonly IUserService _userService;
    private readonly IMessenger _messenger;
    private readonly Func<User?, UserEditViewModel> _userEditViewModelFactory;
    private readonly Func<UserEditViewModel, Views.UserEditWindow> _userEditWindowFactory;

    [ObservableProperty]
    private ObservableCollection<User> _users = new();

    [ObservableProperty]
    private User? _selectedUser;

    public MainWindowViewModel(
        IUserService userService,
        IMessenger messenger,
        Func<User?, UserEditViewModel> userEditViewModelFactory,
        Func<UserEditViewModel, Views.UserEditWindow> userEditWindowFactory)
    {
        _userService = userService;
        _messenger = messenger;
        _userEditViewModelFactory = userEditViewModelFactory;
        _userEditWindowFactory = userEditWindowFactory;

        // WeakReferenceMessenger.Default.Register - 注册接收 UserSavedMessage
        _messenger.Register<Messages.UserSavedMessage>(this);
        LoadUsers();
    }

    private void LoadUsers()
    {
        Users.Clear();
        foreach (var user in _userService.GetAllUsers())
            Users.Add(user);
    }

    /// <summary>
    /// 新增用户命令
    /// </summary>
    [RelayCommand]
    private async Task AddUserAsync()
    {
        var vm = _userEditViewModelFactory(null);
        var window = _userEditWindowFactory(vm);
        var owner = GetOwnerWindow();
        if (owner is not null)
            await window.ShowDialog(owner);
        else
            window.Show();
    }

    /// <summary>
    /// 编辑用户命令 - 需要选中用户才能执行
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanEditUser))]
    private async Task EditUserAsync()
    {
        if (SelectedUser is null) return;
        var vm = _userEditViewModelFactory(SelectedUser);
        var window = _userEditWindowFactory(vm);
        var owner = GetOwnerWindow();
        if (owner is not null)
            await window.ShowDialog(owner);
        else
            window.Show();
    }

    private bool CanEditUser() => SelectedUser is not null;

    partial void OnSelectedUserChanged(User? value)
    {
        // 选中项变化时通知 EditUserCommand 的 CanExecute 重新计算
        EditUserCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// IRecipient 实现 - 接收 UserSavedMessage
    /// </summary>
    public void Receive(Messages.UserSavedMessage message)
    {
        if (message.IsNew)
            _userService.AddUser(message.Value);
        else
            _userService.UpdateUser(message.Value);
        LoadUsers();
    }

    private static Avalonia.Controls.Window? GetOwnerWindow() =>
        (Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
}
