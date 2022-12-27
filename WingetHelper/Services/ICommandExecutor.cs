using System.Threading.Tasks;
using WingetHelper.Commands;

namespace WingetHelper.Services
{
    public interface ICommandExecutor
    {
        Task<TResult> ExecuteCommandAsync<TResult>(WingetCommandMetadata<TResult> commandMetadata);
    }
}