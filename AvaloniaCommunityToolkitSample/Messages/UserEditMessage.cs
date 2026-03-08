using AvaloniaCommunityToolkitSample.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AvaloniaCommunityToolkitSample.Messages;

/// <summary>
/// 请求打开用户编辑窗体（新增或编辑模式）
/// </summary>
public class UserEditMessage : RequestMessage<bool>
{
    public UserEditMessage(User? user)
    {
        User = user;
        IsEditMode = user is not null;
    }

    /// <summary>
    /// 编辑模式下的用户，新增时为 null
    /// </summary>
    public User? User { get; }

    /// <summary>
    /// 是否为编辑模式
    /// </summary>
    public bool IsEditMode { get; }
}
