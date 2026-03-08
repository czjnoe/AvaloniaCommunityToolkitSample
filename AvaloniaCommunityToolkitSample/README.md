# AvaloniaCommunityToolkitSample

基于 Avalonia + CommunityToolkit.Mvvm 的用户列表示例，演示新增/编辑用户功能，并复用一个编辑窗体。

---

## CommunityToolkit.Mvvm 特性说明

### 1. RelayCommand

**简介**：用 `[RelayCommand]` 标记私有方法，源生成器会生成对应的 `IRelayCommand` 属性（如 `SaveCommand`），无需手写命令类和 `ICommand` 实现。

**使用场景**：按钮、菜单等需要绑定“点击执行某逻辑”时，用命令代替直接事件，便于单元测试和 MVVM 解耦。

**本项目中**：
- `MainWindowViewModel`：`AddUserCommand`、`EditUserCommand`
- `UserEditViewModel`：`SaveCommand`、`CancelCommand`

```csharp
[RelayCommand]
private async Task AddUserAsync() { ... }
// 生成 AddUserCommand，支持 async Task
```

---

### 2. ObservableProperty

**简介**：在私有字段上使用 `[ObservableProperty]`，源生成器会生成对应的公共属性，并在 set 时自动触发 `INotifyPropertyChanged`，无需手写属性与 `OnPropertyChanged`。

**使用场景**：所有需要与 UI 双向绑定、且变化时需要刷新界面的数据，都适合用该特性声明。

**本项目中**：
- `MainWindowViewModel`：`Users`、`SelectedUser`
- `UserEditViewModel`：`Name`、`Email`、`Phone`、`IsEditMode`

```csharp
[ObservableProperty]
private string _name = string.Empty;
// 生成 public string Name { get; set; }，set 时自动通知
```

---

### 3. RelayCommand(CanExecute)

**简介**：`[RelayCommand(CanExecute = nameof(方法名))]` 指定一个返回 `bool` 的方法作为“是否可执行”的判断。只有当该方法返回 `true` 时，生成的命令才可执行，按钮等会随之启用/禁用。

**使用场景**：根据当前数据状态控制操作是否可用（如：未选中行时禁用“编辑”、必填项未填时禁用“保存”）。

**本项目中**：
- `UserEditViewModel.SaveCommand`：`CanExecute = nameof(CanSave)`，姓名和邮箱非空才可保存
- `MainWindowViewModel.EditUserCommand`：`CanExecute = nameof(CanEditUser)`，选中用户后才可编辑

```csharp
[RelayCommand(CanExecute = nameof(CanSave))]
private void Save() { ... }
private bool CanSave() => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Email);
```

---

### 4. NotifyCanExecuteChanged

**简介**：在生成的命令上调用 `XXXCommand.NotifyCanExecuteChanged()`，会重新执行一次 `CanExecute`，并更新所有绑定该命令的 UI 的启用状态。

**使用场景**：当“影响 CanExecute 的数据”在代码里被修改后，需要主动通知界面刷新按钮状态时（例如选中项变化、表单字段变化）。

**本项目中**：
- `MainWindowViewModel`：在 `OnSelectedUserChanged` 中调用 `EditUserCommand.NotifyCanExecuteChanged()`，使“编辑用户”在选中/取消选中时即时启用或禁用。

```csharp
partial void OnSelectedUserChanged(User? value)
{
    EditUserCommand.NotifyCanExecuteChanged();
}
```

---

### 5. NotifyPropertyChangedFor（AlsoNotifyChangeFor）

**简介**：在 `[ObservableProperty]` 上附加 `[NotifyPropertyChangedFor(nameof(其他属性))]`，当本属性变化时，会同时触发“其他属性”的 `PropertyChanged`，从而让依赖该属性的计算属性或显示文本也能刷新。

**使用场景**：有“派生属性”（如标题、汇总信息）依赖某几个字段时，避免手写多处 `OnPropertyChanged`。

**本项目中**：
- `UserEditViewModel`：`IsEditMode` 上标记 `[NotifyPropertyChangedFor(nameof(DisplayTitle))]`，切换新增/编辑时窗口标题（DisplayTitle）自动更新。

```csharp
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(DisplayTitle))]
private bool _isEditMode;
public string DisplayTitle => IsEditMode ? "编辑用户" : "新增用户";
```

---

### 6. NotifyCanExecuteChangedFor（AlsoNotifyCanExecuteFor）

**简介**：在 `[ObservableProperty]` 上附加 `[NotifyCanExecuteChangedFor(nameof(某Command))]`，当该属性变化时，会自动对指定命令调用 `NotifyCanExecuteChanged()`，无需在属性 set 里手写。

**使用场景**：表单字段（如姓名、邮箱）直接影响“保存”按钮是否可用时，用该特性可让保存按钮状态随输入自动更新。

**本项目中**：
- `UserEditViewModel`：`Name`、`Email`、`Phone`、`IsEditMode` 均标记了 `[NotifyCanExecuteChangedFor(nameof(SaveCommand))]`，输入变化时“保存”按钮的启用状态自动更新。

```csharp
[ObservableProperty]
[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
private string _name = string.Empty;
```

---

### 7. WeakReferenceMessenger.Register / Send

**简介**：
- **Register**：让某个对象（通常实现 `IRecipient<TMessage>`）订阅一类消息，收到该消息时执行 `Receive(TMessage)`。
- **Send**：向全局 Messenger 发送一条消息，所有已注册的接收者会收到，发送方与接收方无直接引用，解耦模块。

**使用场景**：跨窗体、跨 ViewModel 的轻量通知（如“用户已保存，请刷新列表”），避免层层传递委托或引用。

**本项目中**：
- 主窗口：`_messenger.Register<Messages.UserSavedMessage>(this)`，实现 `IRecipient<UserSavedMessage>` 的 `Receive`，在收到保存消息后刷新用户列表。
- 编辑窗体：保存时 `_messenger.Send(new UserSavedMessage(user, !IsEditMode))`，通知主窗口“有用户被保存”。

```csharp
// 主窗口注册并接收
_messenger.Register<Messages.UserSavedMessage>(this);
public void Receive(Messages.UserSavedMessage message) { ... }

// 编辑窗体发送
_messenger.Send(new Messages.UserSavedMessage(user, !IsEditMode));
```

---

### 8. IOC（依赖注入）

**简介**：使用 `Microsoft.Extensions.DependencyInjection` 在启动时注册服务与 ViewModel，通过构造函数注入依赖，避免在业务代码里 `new` 具体实现，便于测试和替换实现。

**使用场景**：服务（如 `IUserService`）、Messenger、以及需要参数的 ViewModel（如带 `User?` 的 `UserEditViewModel`）通过工厂或直接注册，由容器创建主窗口 ViewModel 并注入。

**本项目中**：
- `App.axaml.cs` 中 `ConfigureServices` 注册：`IUserService`、`IMessenger`、`MainWindowViewModel`、以及 `User? → UserEditViewModel`、`UserEditViewModel → UserEditWindow` 的工厂。
- `MainWindowViewModel` 通过构造函数注入 `IUserService`、`IMessenger` 和两个工厂，主窗口的 `DataContext` 由 `Services.GetRequiredService<MainWindowViewModel>()` 解析。

---

## 项目结构简述

| 目录/文件 | 说明 |
|-----------|------|
| `Models/User.cs` | 用户实体 |
| `Messages/` | 消息类型（如 `UserSavedMessage`） |
| `Services/` | `IUserService` 与内存实现，供 IOC 注入 |
| `ViewModels/` | MainWindowViewModel（列表）、UserEditViewModel（新增/编辑共用） |
| `Views/` | MainWindow（用户列表 + 按钮）、UserEditWindow（新增/编辑窗体） |

运行项目：在解决方案目录执行 `dotnet run`，即可使用“新增用户”和“编辑用户”功能，并观察上述特性的效果。
