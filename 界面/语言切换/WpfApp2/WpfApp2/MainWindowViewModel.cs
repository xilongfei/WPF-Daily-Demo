using System.ComponentModel;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Windows;

namespace WpfApp2;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public MainWindowViewModel()
    {
        
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// 手动切换语言
    /// </summary>
    /// <param name="culture"></param>
    public void SwitchLanguage(string culture)
    {
        if (string.IsNullOrWhiteSpace(culture)) return;
        var app = Application.Current;
        if (app == null) return;

        // 找到已有的语言字典（项目中的 zh.xaml 或 us.xaml）并替换
        ResourceDictionary? existing = null;
        foreach (var d in app.Resources.MergedDictionaries)
        {
            if (d?.Source == null) continue;
            var s = d.Source.OriginalString;
            if (s.Contains("zh.xaml") || s.Contains("us.xaml"))
            {
                existing = d;
                break;
            }
        }

        var fileName = culture == "en-US" ? "us.xaml" : "zh.xaml";
        try
        {
            var uri = new Uri($"pack://application:,,,/WpfApp2;component/{fileName}", UriKind.Absolute);
            var newDict = new ResourceDictionary { Source = uri };

            if (existing != null)
                app.Resources.MergedDictionaries.Remove(existing);

            app.Resources.MergedDictionaries.Add(newDict);

            // 更新线程文化（可用于日期/数字格式等）
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(culture);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"切换语言失败: {ex.Message}");
        }
    }
}