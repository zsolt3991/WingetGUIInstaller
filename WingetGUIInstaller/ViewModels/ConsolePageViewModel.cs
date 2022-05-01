using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using WingetGUIInstaller.Messages;
using WingetGUIInstaller.Services;
using WingetHelper.Commands;

namespace WingetGUIInstaller.ViewModels
{
    public class ConsolePageViewModel : ObservableObject
    {
        private const string RegexPattern = @"[ ](?=(?:[^""]*""[^""]*"")*[^""]*$)";
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly ConsoleOutputCache _cache;
        private string _commandLine;

        public ConsolePageViewModel(DispatcherQueue dispatcherQueue, ConsoleOutputCache cache)
        {
            WeakReferenceMessenger.Default.Register<CommandlineOutputMessage>(this, ProcessMessage);
            _dispatcherQueue = dispatcherQueue;
            _cache = cache;
        }

        public string ComposedMessage => string.Join(Environment.NewLine, _cache.GetCachedMessages());

        public string CommandLine
        {
            get => _commandLine;
            set => SetProperty(ref _commandLine, value);
        }

        public ICommand ExecuteCustomCommand => new AsyncRelayCommand(InvokeCustomCommand);

        private async Task InvokeCustomCommand()
        {
            if (string.IsNullOrWhiteSpace(CommandLine))
            {
                return;
            }

            var arguments = new Regex(RegexPattern).Split(CommandLine);
            var commandResult = await WingetInfo.CustomWingetCommand(arguments)
                .ConfigureOutputListener(_cache.IngestMessage)
                .ExecuteAsync();
        }

        private void ProcessMessage(object recipient, CommandlineOutputMessage message)
        {
            if (_dispatcherQueue.HasThreadAccess)
            {
                OnPropertyChanged(nameof(ComposedMessage));
            }
            else
            {
                _dispatcherQueue.TryEnqueue(() => OnPropertyChanged(nameof(ComposedMessage)));
            }
        }
    }
}
