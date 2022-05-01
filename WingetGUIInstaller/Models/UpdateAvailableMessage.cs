using CommunityToolkit.Mvvm.Messaging.Messages;
using GithubPackageUpdater.Models;

namespace WingetGUIInstaller.Models
{
    internal class UpdateAvailableMessage : ValueChangedMessage<PackageUpdateResponse>
    {
        public UpdateAvailableMessage(PackageUpdateResponse value) : base(value)
        {
        }
    }
}
