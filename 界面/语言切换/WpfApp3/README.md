# WpfApp3

一个基于 **.NET 8 + WPF** 的桌面应用示例项目，演示了 **MVVM 架构** 与 **国际化（中英文切换）** 的落地方式。代码结构清晰、依赖精简，可作为 WPF 新项目的启动模板或国际化方案的学习参考。

---

## 功能特性

- 🖥️ **WPF 桌面应用**：基于 `.NET 8`（`net8.0-windows`），Windows 平台原生运行。
- 🧩 **MVVM 架构**：使用 `CommunityToolkit.Mvvm` 实现 ViewModel 与视图解耦。
- 🌐 **国际化（i18n）**：基于 `Antelcat.I18N.WPF` 实现运行时语言切换（中文 / English）。
- ⏱️ **实时时钟**：通过 `DispatcherTimer` 每秒刷新，演示 MVVM 属性变更通知。
- ⚡ **现代化语言特性**：启用 `Nullable` 与 `ImplicitUsings`。

---

## 技术栈

| 依赖 / 技术 | 版本 | 用途 |
| --- | --- | --- |
| .NET SDK | 8.0 | 运行时与编译（见 `global.json`） |
| WPF | — | 桌面 UI 框架 |
| CommunityToolkit.Mvvm | 8.4.2 | MVVM 工具库（`ObservableObject`、`[ObservableProperty]`） |
| Antelcat.I18N.WPF | 2.0.0-preview-1 | WPF 国际化扩展（`{I18N ...}` 标记扩展） |

---

## 目录结构

```
WpfApp3/
├── global.json                          # 固定 .NET SDK 版本（8.0.0，rollForward: latestMinor）
├── WpfApp3.sln                          # 解决方案文件
└── WpfApp3/
    ├── App.xaml / App.xaml.cs           # 应用入口（启动 MainWindow）
    ├── AssemblyInfo.cs                  # 主题资源位置声明
    ├── Views/
    │   └── MainWindow.xaml(.cs)         # 主窗口视图（Code-Behind 仅负责绑定 DataContext）
    ├── ViewModels/
    │   └── MainWindowViewModels.cs      # 主窗口 ViewModel（含语言切换 + 计时器逻辑）
    ├── Languages/
    │   ├── LangKeys.cs                  # 资源键（由 Antelcat.I18N 属性自动生成）
    │   ├── Language.resx                # 默认语言资源（中文）
    │   └── Language.en-us.resx          # 英文语言资源
    └── WpfApp3.csproj                   # 项目文件
```

---

## 快速开始

### 环境要求

- Windows 10 / 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### 构建与运行

在项目根目录下执行：

```bash
# 还原依赖
dotnet restore

# 构建
dotnet build

# 运行
dotnet run --project WpfApp3/WpfApp3.csproj
```

也可以使用 Visual Studio 2022（17.8+）或 JetBrains Rider 直接打开 `WpfApp3.sln` 后按 `F5` 运行。

---

## 国际化（i18n）实现说明

本项目的国际化基于 `Antelcat.I18N.WPF`，核心机制如下：

1. **资源文件**：`Languages/Language.resx`（中文）与 `Languages/Language.en-us.resx`（英文）中定义同名资源键（如 `Name`、`DateLabel`、`Chinese`、`English`）。
2. **资源键封装**：`LangKeys` 通过 `[ResourceKeysOf(typeof(Language))]` 特性自动生成强类型键，避免硬编码字符串。
3. **标记扩展**：在 XAML 中使用 `{I18N ...}` 标记扩展绑定资源键：

   ```xml
   <TextBlock Text="{I18N {x:Static languages:LangKeys.Name}}" />
   ```
4. **运行时切换**：ViewModel 中监听语言选择变化，调用 `I18NExtension.Culture` 切换当前区域文化，并同步更新 `CultureInfo.DefaultThreadCurrentUICulture` 与窗口的 `XmlLanguage`，使界面即时刷新。

> 切换入口为主窗口的下拉框（中文 / English），切换后所有绑定文本会立即更新。

---

## MVVM 说明

- **View（视图）**：`MainWindow.xaml` 仅声明 UI 结构，通过 `{Binding}` 与 ViewModel 交互。
- **ViewModel**：`MainWindowViewModels` 继承 `ObservableObject`，使用 `[ObservableProperty]` 特性生成可通知属性（如 `SelectedItem`），并通过 `partial` 方法 `OnSelectedItemChanged` 响应属性变化，实现语言切换。
- **Code-Behind 极简**：`MainWindow.xaml.cs` 仅做 `DataContext` 赋值，业务逻辑全部位于 ViewModel。

---

## 许可证

_请根据实际需要补充许可证信息。_
