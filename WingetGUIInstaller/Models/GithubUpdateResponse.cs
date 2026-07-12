using System;
using WingetGUIInstaller.Contracts;


#if !UNPACKAGED
namespace WingetGUIInstaller.Models
{
    internal sealed class GithubUpdateResponse : IUpdateResponse
    {
        public Version UpdateVersion { get; }
        public string ChangeLog { get; }
        public bool IsUpdateAvailable { get; }
        public Uri UpdateUri { get; }

        public GithubUpdateResponse(
            Version updateVersion = null,
            string changeLog = null,
            Uri updateUri = null)
        {
            UpdateVersion = updateVersion;
            ChangeLog = changeLog ?? string.Empty;
            UpdateUri = updateUri;
            IsUpdateAvailable = updateVersion != null && updateUri != null;
        }
    }
}
#endif
