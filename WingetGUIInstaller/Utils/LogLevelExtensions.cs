using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace WingetGUIInstaller.Utils
{
    internal static class LogLevelExtensions
    {
        public static LogEventLevel ToSerilogLevel(this LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return LogEventLevel.Verbose;
                case LogLevel.Debug:
                    return LogEventLevel.Debug;
                case LogLevel.Information:
                    return LogEventLevel.Information;
                case LogLevel.Warning:
                    return LogEventLevel.Warning;
                case LogLevel.Error:
                    return LogEventLevel.Error;
                case LogLevel.Critical:
                    return LogEventLevel.Fatal;
                default:
                    return LogEventLevel.Information;
            }
        }
    }
}
