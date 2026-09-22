using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace Pdf_Test.Services
{
    /// <summary>
    /// Provides theme and accent management for the application.
    /// Persists user choices to %AppData% and updates application resources.
    /// </summary>
    public class ThemeService
    {
        /// <summary>
        /// 三种主题模式
        /// </summary>
        public enum ThemeMode
        {
            Light,
            Dark,
            System
        }

        private const string AccentConfigFile = "theme.accent";
        private readonly string _themeConfigPath;
        private readonly string _accentConfigPath;
        // 默认主题
        private ThemeMode _currentMode = ThemeMode.System;
        // 默认强调色
        private string _accentName = "Blue";

        // 主题变化时触发
        public event EventHandler? ThemeChanged;
        // 强调色变化时触发
        public event EventHandler? AccentChanged;

        /// <summary>Creates the service and loads persisted theme and accent settings.</summary>
        public ThemeService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, "Pdf-Test");
            Directory.CreateDirectory(dir);
            _themeConfigPath = Path.Combine(dir, "theme.config");
            _accentConfigPath = Path.Combine(dir, AccentConfigFile);
            _currentMode = LoadModeFromConfig();
            _accentName = LoadAccentFromConfig();
        }

        public ThemeMode CurrentMode => _currentMode;
        public string SelectedAccentName => _accentName;

        /// <summary>
        /// 应用默认/保存的主题模式和强调色
        /// </summary>
        public void Initialize()
        {
            ApplyTheme(_currentMode);
            ApplyAccent(_accentName);
        }

        /// <summary>
        /// 让界面应用指定的主题模式（Light、Dark、System），并保存选择到配置文件。
        /// </summary>
        /// <param name="mode"></param>
        public void ApplyTheme(ThemeMode mode)
        {
            _currentMode = mode;
            SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;

            var applied = mode;
            if (mode == ThemeMode.System)
            {
                SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
                applied = IsSystemInLightMode() ? ThemeMode.Light : ThemeMode.Dark;
            }

            // 从 WPF 资源字典中移除旧的 Light.xaml 或 Dark.xaml
            var dicts = Application.Current.Resources.MergedDictionaries;
            var existing = dicts.FirstOrDefault(d => d.Source != null && (d.Source.OriginalString.EndsWith("Light.xaml") || d.Source.OriginalString.EndsWith("Dark.xaml")));
            if (existing != null) dicts.Remove(existing);

            var file = applied == ThemeMode.Light ? "Themes/Light.xaml" : "Themes/Dark.xaml";
            try
            {
                var newDict = new ResourceDictionary { Source = new Uri(file, UriKind.Relative) };
                dicts.Add(newDict);
            }
            catch (Exception)
            {
            }

            SaveModeToConfig(_currentMode);
            ThemeChanged?.Invoke(this, EventArgs.Empty);
            ApplyAccent(_accentName, suppressThemeChanged:true);
        }

        /// <summary>
        /// 让界面应用指定的强调色，并保存选择到配置文件。
        /// </summary>
        /// <param name="accentName">强调色名称 (e.g., "Blue", "Emerald").</param>
        public void SetAccent(string accentName)
        {
            _accentName = string.IsNullOrWhiteSpace(accentName) ? "Blue" : accentName;
            ApplyAccent(_accentName);
        }

        /// <summary>
        /// 让界面应用指定的强调色，并保存选择到配置文件。
        /// </summary>
        /// <param name="accentName">强调色名称 (e.g., "Blue", "Emerald").</param>
        /// <param name="suppressThemeChanged">是否抑制主题更改事件 不设置为 false 将会触发 ThemeChanged 事件 view界面将会更新</param>
        public void ApplyAccent(string accentName, bool suppressThemeChanged = false)
        {
            var color = GetAccentColor(accentName);
            var appResources = Application.Current.Resources;

            appResources["AccentBrush"] = new SolidColorBrush(color);
            appResources["AccentDarkBrush"] = new SolidColorBrush(AdjustColor(color, -20));
            appResources["AccentLightBrush"] = new SolidColorBrush(AdjustColor(color, 35));
            appResources["AccentSoftBrush"] = new SolidColorBrush(Color.FromArgb(40, color.R, color.G, color.B));
            appResources["OnAccentBrush"] = new SolidColorBrush(Colors.White);
            appResources["LinkBrush"] = new SolidColorBrush(color);

            SaveAccentToConfig(_accentName);
            if (!suppressThemeChanged)
            {
                AccentChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 获取保存的主题模式名称
        /// </summary>
        /// <returns></returns>
        public string GetSavedModeName() => _currentMode.ToString();

        /// <summary>
        /// 加载主题模式配置文件
        /// </summary>
        /// <returns></returns>
        private ThemeMode LoadModeFromConfig()
        {
            try
            {
                if (!File.Exists(_themeConfigPath)) return ThemeMode.System;
                var txt = File.ReadAllText(_themeConfigPath).Trim();
                return txt.Equals("Light", StringComparison.OrdinalIgnoreCase) ? ThemeMode.Light :
                       txt.Equals("Dark", StringComparison.OrdinalIgnoreCase) ? ThemeMode.Dark : ThemeMode.System;
            }
            catch
            {
                return ThemeMode.System;
            }
        }

        /// <summary>
        /// 加载强调色配置文件
        /// </summary>
        /// <returns></returns>
        private string LoadAccentFromConfig()
        {
            try
            {
                if (!File.Exists(_accentConfigPath)) return "Blue";
                var txt = File.ReadAllText(_accentConfigPath).Trim();
                return string.IsNullOrWhiteSpace(txt) ? "Blue" : txt;
            }
            catch
            {
                return "Blue";
            }
        }

        /// <summary>
        /// 保存主题模式配置文件
        /// </summary>
        /// <param name="mode"></param>
        private void SaveModeToConfig(ThemeMode mode)
        {
            try { File.WriteAllText(_themeConfigPath, mode.ToString()); }
            catch { }
        }

        /// <summary>
        /// 保存强调色配置文件
        /// </summary>
        /// <param name="accentName"></param>
        private void SaveAccentToConfig(string accentName)
        {
            try { File.WriteAllText(_accentConfigPath, accentName); }
            catch { }
        }

        /// <summary>
        /// 检查系统当前是否为浅色模式
        /// </summary>
        /// <returns></returns>
        private bool IsSystemInLightMode()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize");
                if (key != null)
                {
                    var val = key.GetValue("AppsUseLightTheme");
                    if (val is int i) return i != 0;
                    if (val is byte[] bytes && bytes.Length >= 4) return BitConverter.ToInt32(bytes, 0) != 0;
                }
            }
            catch { }
            return true;
        }

        /// <summary>
        /// 根据强调色名称获取对应的 RGB 颜色值
        /// </summary>
        /// <param name="accentName"></param>
        /// <returns></returns>
        private static Color GetAccentColor(string accentName)
        {
            return accentName switch
            {
                "Emerald" => Color.FromRgb(16, 185, 129),
                "Rose" => Color.FromRgb(244, 63, 94),
                "Purple" => Color.FromRgb(168, 85, 247),
                "Amber" => Color.FromRgb(245, 158, 11),
                _ => Color.FromRgb(47, 107, 255),
            };
        }

        /// <summary>
        /// 根据基色和亮度调整值计算新的颜色
        /// </summary>
        /// <param name="baseColor"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        private static Color AdjustColor(Color baseColor, int delta)
        {
            var r = Clamp(baseColor.R + delta);
            var g = Clamp(baseColor.G + delta);
            var b = Clamp(baseColor.B + delta);
            return Color.FromRgb(r, g, b);
        }

        /// <summary>
        /// 限制颜色值在 0-255 范围内
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static byte Clamp(int value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return (byte)value;
        }

        /// <summary>
        /// 当系统主题发生变化时，如果当前模式为 System，则重新应用主题
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SystemEvents_UserPreferenceChanged(object? sender, UserPreferenceChangedEventArgs e)
        {
            if (_currentMode == ThemeMode.System)
            {
                ApplyTheme(ThemeMode.System);
            }
        }
    }
}
