namespace WingetHelper.Commands
{
    public static class PackageListCommands
    {
        public static WingetCommandMetadata<object> ExportPackagesToFile(string filePath, bool includeVersions = false,
            bool acceptSourceAgreements = false, string packageSourceFilter = default)
        {
            var command = new WingetCommandMetadata<object>("export", "-o", filePath)
                .ConfigureResultDecoder(commandResult => null);

            if (includeVersions)
            {
                command.AddExtraArguments("--include-versions");
            }

            if (acceptSourceAgreements)
            {
                command.AddExtraArguments("--accept-source-agreements");
            }

            if (!string.IsNullOrEmpty(packageSourceFilter))
            {
                command.AddExtraArguments("-s", packageSourceFilter);
            }

            return command;
        }

        public static WingetCommandMetadata<object> ImportPackagesFromFile(string filePath, bool ignoreUnavailable = true,
            bool ignoreVersions = false, bool acceptPackageAgreements = false, bool acceptSourceAgreements = false)
        {
            var command = new WingetCommandMetadata<object>("import", "-i", filePath)
                .ConfigureResultDecoder(commandResult => null);

            if (ignoreUnavailable)
            {
                command.AddExtraArguments("--ignore-unavailable");
            }

            if (ignoreVersions)
            {
                command.AddExtraArguments("--ignore-versions");
            }

            if (acceptPackageAgreements)
            {
                command.AddExtraArguments("--accept-package-agreements");
            }

            if (acceptSourceAgreements)
            {
                command.AddExtraArguments("--accept-source-agreements");
            }

            return command;
        }
    }
}
