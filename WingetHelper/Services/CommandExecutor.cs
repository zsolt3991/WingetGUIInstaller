using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WingetHelper.Commands;
using WingetHelper.Constants;

namespace WingetHelper.Services
{
    internal sealed class CommandExecutor : ICommandExecutor
    {
        private ILogger _logger;

        public CommandExecutor(ILogger<CommandExecutor> logger)
        {
            _logger = logger ?? NullLogger<CommandExecutor>.Instance;
        }

        public async Task<TResult> ExecuteCommandAsync<TResult>(WingetCommandMetadata<TResult> commandMetadata)
        {
            var commandId = Guid.NewGuid();

            var processStartInfo = BuildStartInfoFromMetadata(commandMetadata);
            if (processStartInfo.UseShellExecute)
            {
                var outputFile = Path.Combine(AppContext.BaseDirectory, commandId.ToString());
                var redirectArgs = new List<string> { ">", outputFile };
                redirectArgs.ForEach(arg => processStartInfo.ArgumentList.Add(arg));
            }

            if (commandMetadata.OutputListener != default)
            {
                var argumentsString = string.Join(" ", processStartInfo.ArgumentList.Skip(1));
                commandMetadata.OutputListener.Invoke(">>" + argumentsString); ;
                _logger.LogInformation("[{commandId}] Executing command with arguments: {arguments}", commandId, argumentsString);
            }

            try
            {
                using (var p = Process.Start(processStartInfo))
                {
                    if (processStartInfo.RedirectStandardOutput)
                    {
                        return await HandleOutputAsync(p.StandardOutput, commandMetadata).ConfigureAwait(false);
                    }
                    else
                    {
                        if (!p.HasExited)
                        {
                            p.WaitForExit(5_000);
                        }
                    }
                }

                if (processStartInfo.UseShellExecute)
                {
                    var outputFile = Path.Combine(AppContext.BaseDirectory, commandId.ToString());
                    if (File.Exists(outputFile))
                    {
                        using (var fileReader = File.OpenText(outputFile))
                        {
                            return await HandleOutputAsync(fileReader, commandMetadata).ConfigureAwait(false);
                        }
                    }
                }

                return default;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "[{commandId}] Error executing command.", commandId);
                return default;
            }
        }

        private static async Task<TResult> HandleOutputAsync<TResult>(StreamReader streamReader, WingetCommandMetadata<TResult> commandMetadata)
        {
            var response = new List<string>();
            while (!streamReader.EndOfStream)
            {
                var line = await streamReader.ReadLineAsync().ConfigureAwait(false);
                HandleReceivedLine(line, commandMetadata);
                response.Add(line);
            }
            if (commandMetadata.ResultDecoder != default)
            {
                return commandMetadata.ResultDecoder.Invoke(response);
            }
            else
            {
                return default;
            }
        }

        private static void HandleReceivedLine<TResult>(string line, WingetCommandMetadata<TResult> commandMetadata)
        {
            if (commandMetadata.ProgressMonitor != default)
            {
                var existingKey = DecodingConstants.ProcessStatesMap.Keys
                    .FirstOrDefault(key => line.Contains(key, StringComparison.InvariantCultureIgnoreCase));
                if (existingKey != default)
                {
                    commandMetadata.ProgressMonitor.Invoke(DecodingConstants.ProcessStatesMap[existingKey]);
                }
            }
            if (commandMetadata.OutputListener != default)
            {
                commandMetadata.OutputListener.Invoke(line);
            }
        }

        private static ProcessStartInfo BuildStartInfoFromMetadata<TResult>(WingetCommandMetadata<TResult> commandMetadata)
        {
            var processStartInfo = new ProcessStartInfo(ProcessConstants.ShellName)
            {
                CreateNoWindow = true,
                UseShellExecute = commandMetadata.AsAdministrator ? true : false,
                RedirectStandardOutput = commandMetadata.AsAdministrator ? false : true,
                StandardOutputEncoding = commandMetadata.AsAdministrator ? default : Encoding.Default,
                Verb = commandMetadata.AsAdministrator ? "runas" : string.Empty
            };

            var processArgs = new List<string> { "/C", ProcessConstants.ExecutableName }.Concat(commandMetadata.Arguments);
            foreach (var arg in processArgs)
            {
                processStartInfo.ArgumentList.Add(arg);
            }
            return processStartInfo;
        }
    }
}
