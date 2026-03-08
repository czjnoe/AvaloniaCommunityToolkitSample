using System.Collections.Generic;
using AvaloniaCommunityToolkitSample.Models;

namespace AvaloniaCommunityToolkitSample.Services;

/// <summary>
/// 用户服务实现 - 内存存储
/// </summary>
public class UserService : IUserService
{
    private readonly List<User> _users = new();
    private int _nextId = 1;

    public UserService()
    {
        // 初始示例数据
        _users.AddRange(new[]
        {
            new User { Id = _nextId++, Name = "张三", Email = "zhangsan@example.com", Phone = "13800138001" },
            new User { Id = _nextId++, Name = "李四", Email = "lisi@example.com", Phone = "13800138002" },
            new User { Id = _nextId++, Name = "王五", Email = "wangwu@example.com", Phone = "13800138003" }
        });
    }

    public IReadOnlyList<User> GetAllUsers() => _users.AsReadOnly();

    public void AddUser(User user)
    {
        user.Id = _nextId++;
        _users.Add(user);
    }

    public void UpdateUser(User user)
    {
        var index = _users.FindIndex(u => u.Id == user.Id);
        if (index >= 0)
            _users[index] = user;
    }
}
