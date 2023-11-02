using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Dispatching;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WingetGUIInstaller.Messages;
using WingetGUIInstaller.Services;
using WingetHelper.Commands;
using WingetHelper.Services;

namespace WingetGUIInstaller.ViewModels
{
    public sealed partial class ConsolePageViewModel : ObservableObject
    {
        private const string RegexPattern = @"[ ](?=(?:[^""]*""[^""]*"")*[^""]*$)";
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly ConsoleOutputCache _cache;
        private readonly ICommandExecutor _commandExecutor;
        [ObservableProperty]
        private string _commandLine;

        public ConsolePageViewModel(DispatcherQueue dispatcherQueue, ConsoleOutputCache cache, ICommandExecutor commandExecutor)
        {
            _dispatcherQueue = dispatcherQueue;
            _cache = cache;
            _commandExecutor = commandExecutor;
            WeakReferenceMessenger.Default.Register<CommandlineOutputMessage>(this, ProcessMessage);
        }

        public string ComposedMessage => string.Join(Environment.NewLine, _cache.GetCachedMessages());

        [RelayCommand]
        private async Task InvokeCustomCommand()
        {
            if (string.IsNullOrWhiteSpace(CommandLine))
            {
                return;
            }

            var arguments = Regex.Split(CommandLine, RegexPattern);
            var command = GeneralCommands.CustomWingetCommand(arguments)
                .ConfigureOutputListener(_cache.IngestMessage);
            await _commandExecutor.ExecuteCommandAsync(command);
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
