using System.Collections.Generic;
using AvaloniaCommunityToolkitSample.Models;

namespace AvaloniaCommunityToolkitSample.Services;

/// <summary>
/// 用户服务接口 - IOC 注入
/// </summary>
public interface IUserService
{
    IReadOnlyList<User> GetAllUsers();
    void AddUser(User user);
    void UpdateUser(User user);
}
