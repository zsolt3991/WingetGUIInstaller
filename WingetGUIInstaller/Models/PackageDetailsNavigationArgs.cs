using WingetGUIInstaller.Enums;
using WingetGUIInstaller.ViewModels;

namespace WingetGUIInstaller.Models
{
    internal sealed class PackageDetailsNavigationArgs
    {
        public string PackageId { get; set; }
        public PackageDetailsViewModel PackageDetails { get; set; }
        public AvailableOperation AvailableOperation { get; set; }
    }
}
