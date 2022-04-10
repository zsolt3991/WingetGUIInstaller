using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using WingetGUIInstaller.Models;
using WingetGUIInstaller.Services;

namespace WingetGUIInstaller.ViewModels
{
    public class ConsolePageViewModel:ObservableObject
    {
        private readonly ConsoleOutputCache _cache;

        public ConsolePageViewModel(ConsoleOutputCache cache)
        {
            WeakReferenceMessenger.Default.Register<CommandlineOutputMessage>(this, ProcessMessage);
            _cache = cache;
        }

        public string ComposedMessage => string.Join(Environment.NewLine, _cache.GetCachedMessages());

        private void ProcessMessage(object recipient, CommandlineOutputMessage message)
        {            
            OnPropertyChanged(ComposedMessage);
        }
    }
}
