using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AvaloniaCommunityToolkitSample.Models;
using AvaloniaCommunityToolkitSample.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ObservableCollections;

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
    private readonly Func<SettingsViewModel> _settingsViewModelFactory;
    private readonly Func<SettingsViewModel, Views.SettingsWindow> _settingsWindowFactory;

    private const int MaxLogs = 100;
    private readonly ObservableFixedSizeRingBuffer<string> _logBuffer = new(MaxLogs);

    /// <summary>
    /// 日志列表，用于 UI 绑定（ObservableCollections 的 NotifyCollectionChangedSynchronizedViewList 视图）
    /// </summary>
    public NotifyCollectionChangedSynchronizedViewList<string> LogInfos { get; }

    [ObservableProperty]
    private ObservableCollection<User> _users = new();

    [ObservableProperty]
    private User? _selectedUser;

    public MainWindowViewModel(
        IUserService userService,
        IMessenger messenger,
        Func<User?, UserEditViewModel> userEditViewModelFactory,
        Func<UserEditViewModel, Views.UserEditWindow> userEditWindowFactory,
        Func<SettingsViewModel> settingsViewModelFactory,
        Func<SettingsViewModel, Views.SettingsWindow> settingsWindowFactory)
    {
        _userService = userService;
        _messenger = messenger;
        _userEditViewModelFactory = userEditViewModelFactory;
        _userEditWindowFactory = userEditWindowFactory;
        _settingsViewModelFactory = settingsViewModelFactory;
        _settingsWindowFactory = settingsWindowFactory;

        //for (int i = 0; i < MaxLogs; i++)
        //{
        //    _logBuffer.AddFirst(i.ToString());
        //}

        LogInfos = _logBuffer.ToNotifyCollectionChanged();

        // WeakReferenceMessenger.Default.Register - 注册接收 UserSavedMessage
        _messenger.Register<Messages.UserSavedMessage>(this);
        AddLog("应用启动，加载用户列表");
        LoadUsers();
    }

    private void AddLog(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        _logBuffer.AddLast($"[{timestamp}] {message}");
    }

    private void LoadUsers()
    {
        Users.Clear();
        foreach (var user in _userService.GetAllUsers())
            Users.Add(user);
        AddLog($"已加载 {Users.Count} 个用户");
    }

    /// <summary>
    /// 新增用户命令
    /// </summary>
    [RelayCommand]
    private async Task AddUserAsync()
    {
        AddLog("打开新增用户窗口");
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
        AddLog($"打开编辑用户窗口: {SelectedUser.Name} (ID: {SelectedUser.Id})");
        var vm = _userEditViewModelFactory(SelectedUser);
        var window = _userEditWindowFactory(vm);
        var owner = GetOwnerWindow();
        if (owner is not null)
            await window.ShowDialog(owner);
        else
            window.Show();
    }

    [RelayCommand]
    private async Task OpenSettingsAsync()
    {
        AddLog("打开设置页面");
        var vm = _settingsViewModelFactory();
        var window = _settingsWindowFactory(vm);
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
        {
            _userService.AddUser(message.Value);
            AddLog($"新增用户: {message.Value.Name} (ID: {message.Value.Id})");
        }
        else
        {
            _userService.UpdateUser(message.Value);
            AddLog($"更新用户: {message.Value.Name} (ID: {message.Value.Id})");
        }
        LoadUsers();
    }

    private static Avalonia.Controls.Window? GetOwnerWindow() =>
        (Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
}
