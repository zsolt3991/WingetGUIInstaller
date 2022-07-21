using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WingetHelper.Models;

namespace WingetHelper.Commands
{
    public class WingetCommand<TResult>
    {
        public const string ExecutableName = "winget";
        public const string ShellName = "cmd.exe";
        public const string CommandPrefixArgument = "/C";
        public const string PipeToFile = ">";

        private readonly ProcessStartInfo _processStartInfo;
        private readonly Guid _requestId;
        private readonly Dictionary<string, WingetProcessState> _processStateKeywordMap = new Dictionary<string, WingetProcessState>
        {
            {"found", WingetProcessState.Found },
            {"downloading", WingetProcessState.Downloading },
            {"verifying" , WingetProcessState.Verifying},
            {"verified" , WingetProcessState.Verifying},
            {"starting package install" , WingetProcessState.Installing},
            {"successfully installed", WingetProcessState.Success },
            {"error", WingetProcessState.Error },
        };

        private Action<WingetProcessState> _progressMonitor;
        private Action<string> _outputListener;
        private ILogger _logger;
        private Func<IEnumerable<string>, TResult> _resultDecoder;

        internal WingetCommand(params string[] arguments)
        {
            _requestId = Guid.NewGuid();
            _processStartInfo = new ProcessStartInfo(ShellName)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.Default
            };

            _processStartInfo.ArgumentList.Add(CommandPrefixArgument);
            _processStartInfo.ArgumentList.Add(ExecutableName);
            if (arguments.Any())
            {
                Array.ForEach(arguments, arg => _processStartInfo.ArgumentList.Add(arg));
            }
        }

        public WingetCommand<TResult> AddExtraArguments(params string[] arguments)
        {
            if (arguments.Any())
            {
                Array.ForEach(arguments, arg => _processStartInfo.ArgumentList.Add(arg));
            }
            return this;
        }

        public WingetCommand<TResult> ConfigureProgressListener(Action<WingetProcessState> progressListener)
        {
            _progressMonitor = progressListener;
            return this;
        }

        public WingetCommand<TResult> ConfigureOutputListener(Action<string> outputListener)
        {
            _outputListener = outputListener;
            return this;
        }

        public WingetCommand<TResult> ConfigureLogger(ILogger logger)
        {
            _logger = logger;
            return this;
        }

        internal WingetCommand<TResult> ConfigureResultDecoder(Func<IEnumerable<string>, TResult> resultDecoder)
        {
            _resultDecoder = resultDecoder;
            return this;
        }

        internal WingetCommand<TResult> UseShellExecute()
        {
            _logger?.LogDebug("Setting ShellExecute parameters");
            _processStartInfo.UseShellExecute = true;
            _processStartInfo.RedirectStandardOutput = false;
            _processStartInfo.StandardOutputEncoding = default;
            return this;
        }

        internal WingetCommand<TResult> AsAdministrator()
        {
            if (_processStartInfo.UseShellExecute == false)
            {
                _logger?.LogWarning("Cannot run as admin when ShellExecute is not set.");
                UseShellExecute();
            }
            _processStartInfo.Verb = "runas";
            return this;
        }

        public async Task<TResult> ExecuteAsync()
        {
            var result = default(TResult);

            if (_outputListener != default)
            {
                _outputListener.Invoke(">>" + string.Join(" ", _processStartInfo.ArgumentList.Skip(1)));
            }

            if (_processStartInfo.UseShellExecute)
            {
                _logger?.LogDebug("[{commandId}] Creating output file", _requestId);
                var outputFile = Path.Combine(AppContext.BaseDirectory, _requestId.ToString());
                Array.ForEach(new string[] { PipeToFile, outputFile }, arg => _processStartInfo.ArgumentList.Add(arg));
            }

            _logger?.LogInformation("[{commandId}] Executing command with arguments: {args}", _requestId,
                string.Join(',', _processStartInfo.ArgumentList));

            try
            {
                using (var p = Process.Start(_processStartInfo))
                {
                    if (p != default)
                    {
                        if (_processStartInfo.RedirectStandardOutput)
                        {
                            result = await HandleOutputAsync(p.StandardOutput).ConfigureAwait(false);
                        }
                        else
                        {
                            if (!p.HasExited)
                            {
                                p.WaitForExit(5_000);
                            }
                        }
                    }
                }

                if (_processStartInfo.UseShellExecute)
                {
                    _logger?.LogDebug("[{commandId}] Parsing output file", _requestId);
                    var outputFile = Path.Combine(AppContext.BaseDirectory, _requestId.ToString());
                    if (File.Exists(outputFile))
                    {
                        using (var fileReader = File.OpenText(outputFile))
                        {
                            result = await HandleOutputAsync(fileReader).ConfigureAwait(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[{commandId}] Exception occured while executing command", _requestId);
                throw;
            }

            return result;
        }

        private async Task<TResult> HandleOutputAsync(StreamReader streamReader)
        {
            var response = new List<string>();
            while (!streamReader.EndOfStream)
            {
                var line = await streamReader.ReadLineAsync().ConfigureAwait(false);
                HandleReceivedLine(line);
                response.Add(line);
            }
            if (_resultDecoder != default)
            {
                try
                {
                    return _resultDecoder.Invoke(response);
                }
                catch (Exception decodeException)
                {
                    return default;
                }
            }
            else
            {
                return default;
            }
        }

        private void HandleReceivedLine(string line)
        {
            if (_progressMonitor != default)
            {
                foreach (var key in _processStateKeywordMap.Keys)
                {
                    if (line.Contains(key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        _progressMonitor.Invoke(_processStateKeywordMap[key]);
                    }
                }
            }
            if (_outputListener != default)
            {
                _outputListener.Invoke(line);
            }
        }
    }
}
