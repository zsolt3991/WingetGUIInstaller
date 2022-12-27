using System;
using System.Collections.Generic;
using System.Linq;
using WingetHelper.Models;

namespace WingetHelper.Commands
{
    public class WingetCommandMetadata<TResult>
    {
        internal Action<WingetProcessState> ProgressMonitor { get; private set; }
        internal Action<string> OutputListener { get; private set; }
        internal Func<IEnumerable<string>, TResult> ResultDecoder { get; private set; }
        internal List<string> Arguments { get; private set; }
        internal bool AsAdministrator { get; private set; } = false;

        public WingetCommandMetadata(params string[] arguments)
        {
            Arguments = arguments.ToList();
        }

        public WingetCommandMetadata<TResult> ConfigureProgressListener(Action<WingetProcessState> progressListener)
        {
            ProgressMonitor = progressListener;
            return this;
        }

        public WingetCommandMetadata<TResult> ConfigureOutputListener(Action<string> outputListener)
        {
            OutputListener = outputListener;
            return this;
        }

        public WingetCommandMetadata<TResult> ConfigureResultDecoder(Func<IEnumerable<string>, TResult> resultDecoder)
        {
            ResultDecoder = resultDecoder;
            return this;
        }

        public WingetCommandMetadata<TResult> AddExtraArguments(params string[] arguments)
        {
            if (arguments.Any())
            {
                Arguments.AddRange(arguments);
            }
            return this;
        }

        public WingetCommandMetadata<TResult> RunAsAdministrator()
        {
            AsAdministrator = true;
            return this;
        }
    }
}
