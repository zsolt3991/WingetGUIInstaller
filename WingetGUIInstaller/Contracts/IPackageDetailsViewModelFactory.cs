using WingetGUIInstaller.ViewModels;
using WingetHelper.Models;

namespace WingetGUIInstaller.Contracts
{
    public interface IPackageDetailsViewModelFactory
    {
        PackageDetailsViewModel GetPackageDetailsViewModel(WingetPackageDetails packageDetails);
    }
}