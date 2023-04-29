using System;
using WingetHelper.Enums;

namespace WingetHelper.Utils
{
    internal static class EnumExtensions
    {
        public static string ToArgument(this PackageFilterCriteria packageSearchCriteria)
        {
            return packageSearchCriteria switch
            {
                PackageFilterCriteria.Default => "--query",
                PackageFilterCriteria.ByCommands => "--command",
                PackageFilterCriteria.ById => "--id",
                PackageFilterCriteria.ByMoniker => "--moniker",
                PackageFilterCriteria.ByName => "--name",
                PackageFilterCriteria.ByTag => "--tag",
                _ => throw new NotSupportedException(),
            };
        }
    }
}
