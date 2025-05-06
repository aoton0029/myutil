using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Core
{
    public enum ThemeType
    {
        Light,
        Dark,
        System
    }

    public class ThemeManager
    {
        private readonly Dictionary<string, Color> _lightColors = new();
        private readonly Dictionary<string, Color> _darkColors = new();

        private ThemeType _currentTheme = ThemeType.Light;

        public event EventHandler<ThemeType> ThemeChanged;

        public ThemeType CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    ThemeChanged?.Invoke(this, _currentTheme);
                }
            }
        }

        public ThemeManager()
        {
            InitializeDefaultColors();
        }

        private void InitializeDefaultColors()
        {
            // ライトテーマのカラー
            _lightColors["Background"] = Color.White;
            _lightColors["Surface"] = Color.FromArgb(242, 247, 251); // F2F7FB
            _lightColors["Primary"] = Color.SteelBlue;
            _lightColors["PrimaryVariant"] = Color.LightSteelBlue;
            _lightColors["Secondary"] = Color.FromArgb(91, 155, 213); // 5B9BD5
            _lightColors["TextPrimary"] = Color.FromArgb(30, 46, 62); // 1E2E3E
            _lightColors["TextSecondary"] = Color.FromArgb(100, 116, 139); // 64748B
            _lightColors["Divider"] = Color.FromArgb(140, 170, 197); // 8CAAC5
            _lightColors["Error"] = Color.FromArgb(179, 38, 30); // B3261E

            // ダークテーマのカラー
            _darkColors["Background"] = Color.FromArgb(18, 18, 18); // 121212
            _darkColors["Surface"] = Color.FromArgb(30, 30, 30); // 1E1E1E
            _darkColors["Primary"] = Color.FromArgb(100, 160, 210); // 64A0D2
            _darkColors["PrimaryVariant"] = Color.FromArgb(126, 186, 236); // 7EBAEC
            _darkColors["Secondary"] = Color.FromArgb(111, 175, 233); // 6FAFE9
            _darkColors["TextPrimary"] = Color.FromArgb(230, 230, 230); // E6E6E6
            _darkColors["TextSecondary"] = Color.FromArgb(170, 170, 170); // AAAAAA
            _darkColors["Divider"] = Color.FromArgb(70, 70, 70); // 464646
            _darkColors["Error"] = Color.FromArgb(255, 99, 71); // FF6347
        }

        public Color GetColor(string colorName)
        {
            var colorMap = _currentTheme == ThemeType.Dark ? _darkColors : _lightColors;

            if (colorMap.TryGetValue(colorName, out var color))
            {
                return color;
            }

            return Color.Gray; // フォールバック
        }

        public bool IsDarkTheme()
        {
            if (_currentTheme == ThemeType.System)
            {
                return IsSystemUsingDarkTheme();
            }

            return _currentTheme == ThemeType.Dark;
        }

        private bool IsSystemUsingDarkTheme()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");

                if (key != null)
                {
                    var value = key.GetValue("AppsUseLightTheme");
                    if (value != null && value is int lightThemeEnabled)
                    {
                        return lightThemeEnabled == 0;
                    }
                }
            }
            catch
            {
                // レジストリ読み取りに失敗した場合は何もしない
            }

            return false; // デフォルトはライトテーマ
        }
    }
}
