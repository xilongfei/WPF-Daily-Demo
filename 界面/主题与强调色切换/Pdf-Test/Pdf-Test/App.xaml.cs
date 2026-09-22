using System.Windows;
using Pdf_Test.Services;

namespace Pdf_Test;

public partial class App : Application
{
    private ThemeService _themeService = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _themeService = new ThemeService();
        _themeService.Initialize();

        var main = new MainWindow(_themeService);
        main.Show();
    }
}
