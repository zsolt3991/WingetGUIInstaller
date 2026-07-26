using Microsoft.Windows.Storage;
using WingetGUIInstaller.Constants;

namespace WingetGUIInstaller.Services
{
    internal interface IApplicationDataProvider
    {
        ApplicationData GetApplicationData();
    }

    internal sealed class ApplicationDataProvider : IApplicationDataProvider
    {
        public ApplicationData GetApplicationData()
        {
#if UNPACKAGED
            return ApplicationData.GetForUnpackaged(
                UnpackagedApplicationDataConstants.Publisher,
                UnpackagedApplicationDataConstants.Product);
#else
            return ApplicationData.GetDefault();
#endif
        }
    }
}
