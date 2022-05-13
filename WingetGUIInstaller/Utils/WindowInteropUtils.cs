using WinRT.Interop;

namespace WingetGUIInstaller.Utils
{
    internal static class WindowInteropUtils
    {
        internal static TControl InitializeWithAppWindow<TControl>(this TControl targetControl)
        {
            var hwnd = WindowNative.GetWindowHandle(App.Window);
            InitializeWithWindow.Initialize(targetControl, hwnd);
            return targetControl;
        }
    }
}
