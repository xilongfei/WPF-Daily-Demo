using CommunityToolkit.Mvvm.ComponentModel;
using Pdf_Test.Services;
using System;

namespace Pdf_Test.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ThemeService _themeService;

        public double[] Xs { get; }
        public double[] Ys { get; }

        [ObservableProperty]
        private string selectedTheme = string.Empty;

        [ObservableProperty]
        private string selectedAccent = "Blue";

        public MainViewModel(ThemeService themeService)
        {
            _themeService = themeService;

            var rnd = new Random(0);
            var n = 120;
            Xs = new double[n];
            Ys = new double[n];
            for (int i = 0; i < n; i++)
            {
                Xs[i] = i;
                Ys[i] = Math.Sin(i * 0.18) + rnd.NextDouble() * 0.25;
            }

            SelectedTheme = _themeService.GetSavedModeName();
            SelectedAccent = _themeService.SelectedAccentName;
        }

        /// <summary>
        /// 当 SelectedTheme 属性变化时，应用对应的主题模式
        /// </summary>
        /// <param name="value"></param>
        partial void OnSelectedThemeChanged(string value)
        {
            if (string.Equals(value, "Light", StringComparison.OrdinalIgnoreCase))
                _themeService.ApplyTheme(ThemeService.ThemeMode.Light);
            else if (string.Equals(value, "Dark", StringComparison.OrdinalIgnoreCase))
                _themeService.ApplyTheme(ThemeService.ThemeMode.Dark);
            else
                _themeService.ApplyTheme(ThemeService.ThemeMode.System);
        }

        /// <summary>
        /// 当 SelectedAccent 属性变化时，应用对应的强调色
        /// </summary>
        /// <param name="value"></param>
        partial void OnSelectedAccentChanged(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                _themeService.SetAccent(value);
        }
    }
}
