using System;

namespace WingetHelper.Models
{
    internal class WingetPackageListExportData
    {
        public DateTime CreationDate { get; set; }
        public PackageGroupExportData[] Sources { get; set; }
        public string WinGetVersion { get; set; }
    }

    public class PackageGroupExportData
    {
        public PackageExportData[] Packages { get; set; }
        public PackageSourceExportData SourceDetails { get; set; }
    }

    public class PackageSourceExportData
    {
        public string Argument { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class PackageExportData
    {
        public string PackageIdentifier { get; set; }
        public string Version { get; set; }
        public string Channel { get; set; }
        public string Scope { get; set; }
    }
}

