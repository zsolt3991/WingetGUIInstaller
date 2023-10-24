using WingetGUIInstaller.Enums;
using WingetHelper.Enums;

namespace WingetGUIInstaller.Utils
{
    internal static class LocalizationUtils
    {
        public static string GetResourceKey(this WingetProcessState processState)
        {
            return processState switch
            {
                WingetProcessState.Found => "PackageFoundText",
                WingetProcessState.Downloading => "PackageDownloadingText",
                WingetProcessState.Verifying => "PackageVerifyingText",
                WingetProcessState.Installing => "PackageInstallingText",
                WingetProcessState.Success => "PackageSuccessText",
                WingetProcessState.Error => "PackageErrorText",
                _ => throw new System.NotImplementedException(),
            };
        }

        public static string GetResourceKey(this InstallOperation installOperation)
        {
            return installOperation switch
            {
                InstallOperation.Install => "InstallOperationText",
                InstallOperation.Upgrade => "UpgradeOperationText",
                InstallOperation.Uninstall => "UninstallOperationText",
                _ => throw new System.NotImplementedException(),
            };
        }
    }
}
