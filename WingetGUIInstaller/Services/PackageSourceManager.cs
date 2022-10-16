using System.Threading.Tasks;
using WingetHelper.Commands;

namespace WingetGUIInstaller.Services
{
    public sealed class PackageSourceManager
    {
        private readonly ConsoleOutputCache _consoleBuffer;

        public PackageSourceManager(ConsoleOutputCache consoleBuffer)
        {
            _consoleBuffer = consoleBuffer;
        }

        public async Task AddPackageSource(string packageSourceName, string packageSourceArgument, string packageSourceType = default)
        {
            var command = await PackageSourceCommands.AddPackageSource(packageSourceName, packageSourceArgument, packageSourceType)
                .ConfigureOutputListener(_consoleBuffer.IngestMessage)
                .ExecuteAsync();
        }

        public async Task RemovePackageSource(string packageSourceName)
        {
            var command = await PackageSourceCommands.RemovePackageSource(packageSourceName)
                .ConfigureOutputListener(_consoleBuffer.IngestMessage)
                .ExecuteAsync();
        }
    }
}
