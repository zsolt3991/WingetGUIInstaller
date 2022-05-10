using System;

namespace WingetGUIInstaller.Enums
{
    [Flags]
    public enum AvailableOperation
    {
        None = 0,
        Install = 1,
        Update = 2,
        Uninstall = 4
    }
}
