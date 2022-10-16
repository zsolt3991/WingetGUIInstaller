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

namespace WingetGUIInstaller.ViewModels
{
    public sealed partial class ConsolePageViewModel : ObservableObject
    {
        private const string RegexPattern = @"[ ](?=(?:[^""]*""[^""]*"")*[^""]*$)";
        private readonly DispatcherQueue _dispatcherQueue;
        private readonly ConsoleOutputCache _cache;

        [ObservableProperty]
        private string _commandLine;

        public ConsolePageViewModel(DispatcherQueue dispatcherQueue, ConsoleOutputCache cache)
        {
            _dispatcherQueue = dispatcherQueue;
            _cache = cache;
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
