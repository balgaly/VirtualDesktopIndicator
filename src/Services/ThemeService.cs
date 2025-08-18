using System;
using Microsoft.Win32;

namespace VirtualDesktopIndicator.Services
{
    public enum ThemeMode
    {
        FollowSystem,
        Light,
        Dark
    }

    public class ThemeService
    {
        public static bool IsSystemDarkTheme()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var value = key?.GetValue("AppsUseLightTheme");
                
                // If AppsUseLightTheme is 0, it means dark theme is enabled
                // If AppsUseLightTheme is 1, it means light theme is enabled
                return value != null && (int)value == 0;
            }
            catch
            {
                // Default to light theme if detection fails
                return false;
            }
        }

        public static bool ShouldUseDarkTheme(ThemeMode themeMode)
        {
            return themeMode switch
            {
                ThemeMode.FollowSystem => IsSystemDarkTheme(),
                ThemeMode.Dark => true,
                ThemeMode.Light => false,
                _ => false
            };
        }

        public static string GetThemeModeDisplayName(ThemeMode themeMode)
        {
            return themeMode switch
            {
                ThemeMode.FollowSystem => "Follow System",
                ThemeMode.Light => "Light",
                ThemeMode.Dark => "Dark",
                _ => "Follow System"
            };
        }
    }
}