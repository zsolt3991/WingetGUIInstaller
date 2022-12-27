using System.Threading.Tasks;
using WingetHelper.Commands;
using WingetHelper.Services;

namespace WingetGUIInstaller.Services
{
    public sealed class PackageSourceManager
    {
        private readonly ConsoleOutputCache _consoleBuffer;
        private readonly ICommandExecutor _commandExecutor;

        public PackageSourceManager(ConsoleOutputCache consoleBuffer, ICommandExecutor commandExecutor)
        {
            _consoleBuffer = consoleBuffer;
            _commandExecutor = commandExecutor;
        }

        public async Task AddPackageSource(string packageSourceName, string packageSourceArgument, string packageSourceType = default)
        {
            var command = PackageSourceCommands.AddPackageSource(packageSourceName, packageSourceArgument, packageSourceType)
                .ConfigureOutputListener(_consoleBuffer.IngestMessage);
            await _commandExecutor.ExecuteCommandAsync(command);
        }

        public async Task RemovePackageSource(string packageSourceName)
        {
            var command = PackageSourceCommands.RemovePackageSource(packageSourceName)
                .ConfigureOutputListener(_consoleBuffer.IngestMessage);
            await _commandExecutor.ExecuteCommandAsync(command);
        }
    }
}
