using Microsoft.Extensions.DependencyInjection;
using WingetHelper.Services;

namespace WingetHelper.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWingetHelper(
            this IServiceCollection services)
        {
            services.AddSingleton<ICommandExecutor, CommandExecutor>();
            return services;
        }
    }
}
