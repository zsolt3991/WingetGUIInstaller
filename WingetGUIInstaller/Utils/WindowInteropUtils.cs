using Microsoft.UI.Xaml;
using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace WingetGUIInstaller.Utils
{
    internal static class WindowInteropUtils
    {
        private const string UserThemeSettingRegistryKey = "HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize";
        private const string UserThemeSettingRegistryValue = "AppsUseLightTheme";

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;


        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void ShowWindow(Window window)
        {
            // Bring the window to the foreground... first get the window handle...
            var hwnd = (IntPtr)window.AppWindow.Id.Value;

            // Restore window if minimized... requires DLL import above
            ShowWindow(hwnd, 0x00000009);

            // And call SetForegroundWindow... requires DLL import above
            SetForegroundWindow(hwnd);
        }

        public static void SetWin32ApplicationTheme(ApplicationTheme applicationTheme, Window window)
        {
            var hwnd = (IntPtr)window.AppWindow.Id.Value;
            if (IsWindowsBuildGreater(17763))
            {
                int attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                if (IsWindowsBuildGreater(18985))
                {
                    attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                }

                int useImmersiveDarkMode = applicationTheme == ApplicationTheme.Dark ? 1 : 0;
                _ = DwmSetWindowAttribute(hwnd, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
            }
        }

        public static ApplicationTheme GetUserThemePreference()
        {
            try
            {
                int? res = (int?)Registry.GetValue(UserThemeSettingRegistryKey, UserThemeSettingRegistryValue, -1);
                if (res.HasValue)
                {
                    return res.Value switch
                    {
                        0 => ApplicationTheme.Dark,
                        1 => ApplicationTheme.Light,
                        _ => ApplicationTheme.Light
                    };
                }
                else
                {
                    return ApplicationTheme.Light;
                }
            }
            catch
            {
                return ApplicationTheme.Light;
            }
        }

        public static TControl InitializeWithAppWindow<TControl>(this TControl targetControl)
        {
            InitializeWithWindow.Initialize(targetControl, (IntPtr)App.Window.AppWindow.Id.Value);
            return targetControl;
        }

        public static bool IsWindowsBuildGreater(int build = -1)
        {
            return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
        }
    }
}
