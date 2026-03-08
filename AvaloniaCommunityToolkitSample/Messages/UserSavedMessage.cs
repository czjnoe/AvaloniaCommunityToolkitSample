using AvaloniaCommunityToolkitSample.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AvaloniaCommunityToolkitSample.Messages;

/// <summary>
/// 用户保存完成消息，用于通知列表刷新
/// </summary>
public class UserSavedMessage : ValueChangedMessage<User>
{
    public UserSavedMessage(User user, bool isNew) : base(user)
    {
        IsNew = isNew;
    }

    public bool IsNew { get; }
}
