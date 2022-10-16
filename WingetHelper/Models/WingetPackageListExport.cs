using System;

namespace WingetHelper.Models
{
    internal sealed class WingetPackageListExportData
    {
        public DateTime CreationDate { get; set; }
        public PackageGroupExportData[] Sources { get; set; }
        public string WinGetVersion { get; set; }
    }

    internal sealed class PackageGroupExportData
    {
        public PackageExportData[] Packages { get; set; }
        public PackageSourceExportData SourceDetails { get; set; }
    }

    internal sealed class PackageSourceExportData
    {
        public string Argument { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }

    internal sealed class PackageExportData
    {
        public string PackageIdentifier { get; set; }
        public string Version { get; set; }
        public string Channel { get; set; }
        public string Scope { get; set; }
    }
}

