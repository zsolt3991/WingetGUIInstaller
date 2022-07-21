using System;
using System.Collections;
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

        private readonly ProcessStartInfo _processStartInfo;
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
        private Func<IEnumerable<string>, TResult> _resultDecoder;

        internal WingetCommand(params string[] arguments)
        {
            var extendedArgs = new List<string> { "/C", /*"chcp", "65001", "&",*/ ExecutableName }.Concat(arguments).ToList();
            _processStartInfo = new ProcessStartInfo(ShellName)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.Default
            };

            extendedArgs.ForEach(arg => _processStartInfo.ArgumentList.Add(arg));
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

        internal WingetCommand<TResult> ConfigureResultDecoder(Func<IEnumerable<string>, TResult> resultDecoder)
        {
            _resultDecoder = resultDecoder;
            return this;
        }

        internal WingetCommand<TResult> UseShellExecute()
        {
            _processStartInfo.UseShellExecute = true;
            _processStartInfo.RedirectStandardOutput = false;
            _processStartInfo.StandardOutputEncoding = default;
            return this;
        }

        internal WingetCommand<TResult> AsAdministrator()
        {
            if (_processStartInfo.UseShellExecute == false)
            {
                throw new Exception("Cannot run as admin when Shell Execute is not set");
            }
            _processStartInfo.Verb = "runas";
            return this;
        }

        public async Task<TResult> ExecuteAsync()
        {
            var requestId = Guid.NewGuid();
            var result = default(TResult);

            if (_outputListener != default)
            {
                _outputListener.Invoke(">>" + string.Join(" ", _processStartInfo.ArgumentList.Skip(1)));
            }

            if (_processStartInfo.UseShellExecute)
            {
                var outputFile = Path.Combine(AppContext.BaseDirectory, requestId.ToString());
                var redirectArgs = new List<string> { ">", outputFile };
                redirectArgs.ForEach(arg => _processStartInfo.ArgumentList.Add(arg));
            }

            using (var p = Process.Start(_processStartInfo))
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

            if (_processStartInfo.UseShellExecute)
            {
                var outputFile = Path.Combine(AppContext.BaseDirectory, requestId.ToString());
                if (File.Exists(outputFile))
                {
                    using (var fileReader = File.OpenText(outputFile))
                    {
                        result = await HandleOutputAsync(fileReader).ConfigureAwait(false);
                    }
                }
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
