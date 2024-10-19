using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using WingetHelper.Commands;
using WingetHelper.Services;

namespace WingetGUIInstaller.Services
{
    public sealed class PackageSourceManager
    {
        private readonly ConsoleOutputCache _consoleBuffer;
        private readonly ICommandExecutor _commandExecutor;
        private readonly ILogger<PackageSourceManager> _logger;

        public PackageSourceManager(ConsoleOutputCache consoleBuffer, ICommandExecutor commandExecutor, ILogger<PackageSourceManager> logger)
        {
            _consoleBuffer = consoleBuffer;
            _commandExecutor = commandExecutor;
            _logger = logger;
        }

        public async Task AddPackageSource(string packageSourceName, string packageSourceArgument, string packageSourceType = default)
        {
            _logger.LogInformation("Adding Package source: {Name} with url: {Url}", packageSourceName, packageSourceArgument);
            var command = PackageSourceCommands.AddPackageSource(packageSourceName, packageSourceArgument, packageSourceType)
                .ConfigureOutputListener(_consoleBuffer.IngestMessage);
            await _commandExecutor.ExecuteCommandAsync(command);
        }

        public async Task RemovePackageSource(string packageSourceName)
        {
            _logger.LogInformation("Removing Package source: {Name}", packageSourceName);
            var command = PackageSourceCommands.RemovePackageSource(packageSourceName)
                .ConfigureOutputListener(_consoleBuffer.IngestMessage);
            await _commandExecutor.ExecuteCommandAsync(command);
        }
    }
}
