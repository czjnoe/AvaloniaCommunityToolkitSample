using System;
using AvaloniaCommunityToolkitSample.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AvaloniaCommunityToolkitSample.ViewModels;

/// <summary>
/// 用户编辑 ViewModel
/// </summary>
public partial class UserEditViewModel : ViewModelBase, IDisposable
{
    private readonly IMessenger _messenger;
    private readonly int _originalId;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyPropertyChangedFor(nameof(DisplayTitle))]
    private bool _isEditMode;

    partial void OnIsEditModeChanged(bool value)
    {

    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _email = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _phone = string.Empty;

    /// <summary>
    /// 显示标题：新增/编辑
    /// AlsoNotifyChangeFor 演示：IsEditMode 变化时自动通知 DisplayTitle
    /// </summary>
    public string DisplayTitle => IsEditMode ? "编辑用户" : "新增用户";

    public UserEditViewModel(User? user, IMessenger messenger)
    {
        _messenger = messenger;
        _originalId = user?.Id ?? 0;
        IsEditMode = user is not null;

        if (user is not null)
        {
            Name = user.Name;
            Email = user.Email;
            Phone = user.Phone;
        }
    }

    /// <summary>
    /// 保存命令 - RelayCommand(CanExecute) 演示
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        var user = new User
        {
            Id = _originalId,
            Name = Name.Trim(),
            Email = Email.Trim(),
            Phone = Phone.Trim()
        };

        _messenger.Send(new Messages.UserSavedMessage(user, !IsEditMode));
        CloseRequested?.Invoke();
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Email);

    /// <summary>
    /// 取消命令
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke();
    }

    /// <summary>
    /// 请求关闭窗体（由 View 订阅并关闭 Window）
    /// </summary>
    public event Action? CloseRequested;

    public void Dispose() => GC.SuppressFinalize(this);
}
