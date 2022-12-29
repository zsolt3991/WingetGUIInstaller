using System.Runtime.Serialization;

namespace WingetHelper.Models
{
    public sealed class WingetPackageDetails
    {
        public string Id { get; set; } 

        public string Name { get; set; }

        public string Author { get; set; }

        public string Version { get; set; }

        public string Publisher { get; set; }

        [DataMember(Name = "Publisher Url")]
        public string Publisher_Url { get; set; }

        [DataMember(Name = "Publisher Support Url")]
        public string Publisher_Support_Url { get; set; }

        public string Moniker { get; set; }

        public string Description { get; set; }

        public string Homepage { get; set; }

        public string License { get; set; }

        [DataMember(Name = "License Url")]
        public string License_Url { get; set; }

        [DataMember(Name = "Privacy Url")]
        public string Privacy_Url { get; set; }

        public string Copyright { get; set; }

        [DataMember(Name = "Copyright Url")]
        public string Copyright_Url { get; set; }

        [DataMember(Name = "Release Notes")]
        public string Release_Notes { get; set; }

        [DataMember(Name = "Release Notes Url")]
        public string Release_Notes_Url { get; set; }

        public WingetPackageInstaller Installer { get; set; }
    }

    public class WingetPackageInstaller
    {
        public string Type { get; set; }

        [DataMember(Name = "Download Url")]
        public string Download_Url { get; set; }

        public string SHA256 { get; set; }
    }
}
