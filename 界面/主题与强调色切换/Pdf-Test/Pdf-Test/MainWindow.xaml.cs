using System;
using System.Windows;
using System.Windows.Media;
using Pdf_Test.Services;
using Pdf_Test.ViewModels;
using ScottPlot;
using Color = System.Drawing.Color;

namespace Pdf_Test;

public partial class MainWindow : Window
{
    private readonly ThemeService _themeService;

    public MainWindow(ThemeService themeService)
    {
        InitializeComponent();
        _themeService = themeService;
        DataContext = new MainViewModel(_themeService);
        _themeService.ThemeChanged += (_, _) => Dispatcher.Invoke(RefreshPlotTheme);
        _themeService.AccentChanged += (_, _) => Dispatcher.Invoke(RefreshPlotTheme);
        Loaded += MainWindow_Loaded;
    }
    
    /// <summary>
    /// ScottPlot 在 WPF 中的样式需要在窗口加载完成后才能正确应用
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshPlotTheme();
    }

    /// <summary>
    /// 强调色或主题变化时刷新 ScottPlot 的样式
    /// </summary>
    private void RefreshPlotTheme()
    {
        if (WpfPlot == null) return;
        if (DataContext is not MainViewModel vm) return;

        var figure = GetBrush("BgPrimaryBrush");
        var panel = GetBrush("BgElevatedBrush");
        var text = GetBrush("TextPrimaryBrush");
        var grid = GetBrush("BorderBrush");
        var accent = GetBrush("AccentBrush");

        var plot = WpfPlot.Plot;
        plot.Clear();
        plot.Style(
            ToDrawingColor(figure),
            ToDrawingColor(panel),
            ToDrawingColor(grid),
            ToDrawingColor(text),
            ToDrawingColor(text),
            ToDrawingColor(text));

        plot.AddScatter(vm.Xs, vm.Ys, ToDrawingColor(accent), 2.5f);
        WpfPlot.Refresh();
    }

    /// <summary>
    /// 从 WPF 资源字典中获取指定键的 SolidColorBrush
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private static SolidColorBrush? GetBrush(string key)
    {
        return Application.Current.TryFindResource(key) as SolidColorBrush;
    }

    /// <summary>
    /// 将 WPF 的 SolidColorBrush 转换为 System.Drawing.Color
    /// </summary>
    /// <param name="brush"></param>
    /// <returns></returns>
    private static Color ToDrawingColor(SolidColorBrush? brush)
    {
        return brush == null ? Color.Black : Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
    }
}
