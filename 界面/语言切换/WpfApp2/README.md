# WpfApp2 — WPF 运行时多语言切换示例

## 项目简介

这是一个基于 **WPF（.NET 8）** 的最小可运行示例，演示如何在**程序运行时动态切换界面语言**（中文 ↔ 英文），无需重启应用。项目名仍沿用 Visual Studio 默认模板名 `WpfApp2`。

核心思路：把文案抽到独立的 `ResourceDictionary` 语言字典中，界面用 `DynamicResource` 绑定；切换语言时在运行时替换 `Application.Resources` 里合并的语言字典，并同步更新线程文化。

## 技术栈与环境

| 项目 | 说明 |
| --- | --- |
| 目标框架 | `net8.0-windows` |
| UI 框架 | WPF（`UseWPF=true`） |
| SDK 版本 | 8.0.0（`global.json` 固定，`rollForward=latestMinor`） |
| 架构 | MVVM 雏形（`INotifyPropertyChanged`） |

## 已实现功能

- 默认中文界面，显示「欢迎使用WpfApp2」
- 点击按钮切换为英文，显示「Welcome to WpfApp2---new」
- 运行时替换语言资源字典
- 同步更新 `CurrentCulture` / `CurrentUICulture`（供日期、数字等格式化使用）

## 目录结构

```
WpfApp2/
├── App.xaml / App.xaml.cs      # 应用入口，启动时合并默认中文字典 zh.xaml
├── MainWindow.xaml / .xaml.cs  # 主窗口，用 DynamicResource 绑定文案，含切换按钮
├── MainWindowViewModel.cs      # 语言切换核心逻辑 SwitchLanguage()
├── zh.xaml                     # 中文资源字典
├── us.xaml                     # 英文资源字典
├── WpfApp2.csproj              # 项目文件（含资源打包配置）
└── global.json                 # SDK 版本锁定
WpfApp2.sln                     # 解决方案
```

## 实现原理

1. **文案抽离**：`zh.xaml`、`us.xaml` 中用 `<sys:String x:Key="WelcomeMessage">` 定义同一 Key 的不同语言文案。
2. **默认语言**：`App.xaml` 在 `Application.Resources.MergedDictionaries` 中合并 `zh.xaml`。
3. **界面绑定**：`MainWindow.xaml` 用 `{DynamicResource WelcomeMessage}` 绑定文本。
   > 关键点：必须用 `DynamicResource` 而不是 `StaticResource`。`StaticResource` 只在首次加载时取值，运行期替换字典不会刷新界面；`DynamicResource` 会在资源变化时自动更新。
4. **切换流程**（`SwitchLanguage("en-US")`）：
   - 遍历 `MergedDictionaries`，找到旧的 `zh.xaml` / `us.xaml` 字典并移除；
   - 通过 pack URI 加载新语言字典并 `Add` 回 `MergedDictionaries`；
   - 更新 `Thread.CurrentThread.CurrentCulture` 与 `CurrentUICulture`。

## 运行方式

- 用 Visual Studio / Rider 打开 `WpfApp2.sln` 直接 F5 运行；
- 或在项目根目录执行 `dotnet run --project WpfApp2`。

## 已知问题 ⚠️

`us.xaml` 的打包方式与加载 URI **不匹配**，导致「切换为英文」实际会失败：

- `WpfApp2.csproj` 中把 `us.xaml` 做了 `<Page Remove="us.xaml" />`，再以 `Content` + `CopyToOutputDirectory=PreserveNewest` 输出，它只会作为**松散文件**放在 `bin` 目录（编译期以 `AssemblyAssociatedContentFileAttribute` 注册），**不会编译进程序集资源**。编译产物 `WpfApp2.g.resources` 中仅包含 `zh.xaml`。
- 但 `MainWindowViewModel.SwitchLanguage()` 里用的是 `pack://application:,,,/WpfApp2;component/us.xaml`，该 URI 只查找**程序集内嵌资源**，找不到松散文件，因此抛 `IOException: 找不到资源 "us.xaml"`，被 `catch` 后弹出「切换语言失败」。

**修复方案（二选一）：**

1. 让 `us.xaml` 也作为 `Page` 编译进程序集（删除 `<Page Remove="us.xaml" />` 与对应的 `Content` 项），保持现有 `;component/` URI 不变；
2. 或改用松散文件 URI：`pack://siteoforigin:,,,/us.xaml`。

> 已实际验证：`;component/us.xaml` 与 `application:,,,/us.xaml` 两种写法均报「找不到资源」，而 `pack://siteoforigin:,,,/us.xaml` 可正常加载。

## 遗留与可扩展方向

- 项目名仍为默认 `WpfApp2`，旧的「中文.xaml / 英文.xaml」命名已在 csproj 中清理为 `zh.xaml` / `us.xaml`。
- 可扩展：新增更多语言（法、日等）；用同套 `ResourceDictionary` 替换机制做**主题/皮肤动态切换**；结合 `CultureInfo` 做日期、数字、货币的本地化格式化。
