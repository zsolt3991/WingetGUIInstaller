using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace WingetGUIInstaller.Utils
{
    internal static class WindowInteropUtils
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void ShowWindow(Window window)
        {
            // Bring the window to the foreground... first get the window handle...
            var hwnd = WindowNative.GetWindowHandle(window);

            // Restore window if minimized... requires DLL import above
            ShowWindow(hwnd, 0x00000009);

            // And call SetForegroundWindow... requires DLL import above
            SetForegroundWindow(hwnd);
        }

        internal static TControl InitializeWithAppWindow<TControl>(this TControl targetControl)
        {
            var hwnd = WindowNative.GetWindowHandle(App.Window);
            InitializeWithWindow.Initialize(targetControl, hwnd);
            return targetControl;
        }
    }
}
