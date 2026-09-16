using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using Antelcat.I18N.WPF;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WpfApp3.ViewModels;

public partial class MainWindowViewModels : ObservableObject
{
    [ObservableProperty]
    private string _title = "Hello World";
    
    [ObservableProperty]
    private string _name = "Antelcat";
    
    [ObservableProperty]
    private List<string> _items = new List<string> { "Chinese", "English" };
    
    [ObservableProperty]
    private string _selectedItem = "Chinese";
    
    [ObservableProperty]
    private DateTime _currentTime = DateTime.Now;
    
    private readonly DispatcherTimer _timer;

    public MainWindowViewModels()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (_, _) => CurrentTime = DateTime.Now;
        _timer.Start();

        var initCulture = CultureInfo.DefaultThreadCurrentCulture ?? CultureInfo.CurrentCulture;
        var initXmlLang = XmlLanguage.GetLanguage(initCulture.IetfLanguageTag);
        Application.Current?.Dispatcher.Invoke(() =>
        {
            foreach (Window w in Application.Current.Windows)
            {
                w.Language = initXmlLang;
            }
        });
    }

    partial void OnSelectedItemChanged(string? value)
    {
        CultureInfo? culture = null;
        if (value == "Chinese")
        {
            culture = CultureInfo.GetCultureInfo("zh-CN");
        }
        else if (value == "English")
        {
            culture = CultureInfo.GetCultureInfo("en-US");
        }

        if (culture != null)
        {
            I18NExtension.Culture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            // Update WPF language so bindings use correct culture for formatting
            Application.Current?.Dispatcher.Invoke(() =>
            {
                var xmlLang = XmlLanguage.GetLanguage(culture.IetfLanguageTag);
                foreach (Window w in Application.Current.Windows)
                {
                    w.Language = xmlLang;
                }
            });

            // Force reformat of CurrentTime by reassigning its value and raising PropertyChanged
            // CurrentTime = DateTime.Now;
            // OnPropertyChanged(nameof(CurrentTime));
        }
    }
}