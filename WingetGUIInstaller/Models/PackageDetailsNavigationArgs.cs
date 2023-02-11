using WingetGUIInstaller.Enums;

namespace WingetGUIInstaller.Models
{
    internal sealed class PackageDetailsNavigationArgs
    {
        public string PackageId { get; set; }
        public AvailableOperation AvailableOperation { get; set; }
    }
}
