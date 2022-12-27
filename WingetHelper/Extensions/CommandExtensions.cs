using System.Threading.Tasks;
using WingetHelper.Commands;
using WingetHelper.Services;

namespace WingetHelper.Extensions
{
    public static class CommandExtensions
    {
        public static async Task<TResult> ExecuteCommandAsync<TResult>(this WingetCommandMetadata<TResult> commandMetadata)
        {
            var commandExecutor = new CommandExecutor(null);
            return await commandExecutor.ExecuteCommandAsync(commandMetadata).ConfigureAwait(false);
        }
    }
}
