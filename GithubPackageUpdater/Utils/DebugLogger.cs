using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace GithubPackageUpdater.Utils
{
    internal class DebugLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
#if DEBUG
            Debug.WriteLine(string.Format("[{0}] {1}", logLevel, formatter.Invoke(state, exception)));
#endif
        }
    }
}
