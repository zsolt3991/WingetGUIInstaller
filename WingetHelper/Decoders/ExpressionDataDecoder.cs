using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WingetHelper.Models;

namespace WingetHelper.Decoders
{
    internal static partial class ExpressionDataDecoder
    {
        private const string FoundResultRegexFormat = @"^Found\s*?(?<packageName>.*)\s*?\[(?<packageId>.*)\]$";

#if NET7_0_OR_GREATER
        [GeneratedRegex(FoundResultRegexFormat)]
        private static partial Regex FoundResultRegex();
#else
        private static readonly Regex FoundResultRegex = new Regex(FoundResultRegexFormat, RegexOptions.Compiled);
#endif

        internal static bool ParseInstallSuccessResult(IEnumerable<string> commandResult)
        {
            return commandResult.Any(line => line.Contains("successfully installed", StringComparison.InvariantCultureIgnoreCase));
        }

        internal static bool ParseUninstallSuccessResult(IEnumerable<string> commandResult)
        {
            return commandResult.Any(line => line.Contains("successfully uninstalled", StringComparison.InvariantCultureIgnoreCase));
        }

        internal static Version ParseResultsVersion(IEnumerable<string> commandResult)
        {
            var version = default(Version);
            if (commandResult.Count() == 1)
            {
                string versionString = commandResult.FirstOrDefault();
                if (!string.IsNullOrEmpty(versionString))
                {
                    version = new Version(versionString.Replace("v", ""));
                }
            }
            return version;
        }

        internal static WingetPackageDetails ParseDetailsResponse(IEnumerable<string> output)
        {
            output = output.Where(line => !string.IsNullOrEmpty(line));
            var currentIndex = 0;

            foreach (var line in output)
            {
                // Reached results not found line and should return early
                if (line.StartsWith("no", StringComparison.InvariantCultureIgnoreCase))
                {
                    return default;
                }
# if NET7_0_OR_GREATER
                var match = FoundResultRegex().Match(line);
#else
                var match = FoundResultRegex.Match(line);
#endif
                // Reached results found line
                if (match.Success)
                {
                    var result = ObjectDataDecoder.DeserializeObject<WingetPackageDetails>(output.Skip(currentIndex + 1));
                    result.Name = match.Groups["packageName"].Value.Trim();
                    result.Id = match.Groups["packageId"].Value.Trim();
                    return result;
                }

                currentIndex++;
            }

            return default;
        }
    }
}
