using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Generic;
using System.Linq;
using WingetGUIInstaller.Models;

namespace WingetGUIInstaller.Services
{
    public class ConsoleOutputCache
    {
        private const int MaxCapacity = 100;
        private readonly Queue<string> _buffer;

        public ConsoleOutputCache()
        {
            _buffer = new Queue<string>(MaxCapacity);
        }

        public void IngestMessage(string message)
        {
            // Remove oldest entry from the buffer
            if (_buffer.Count == MaxCapacity)
            {
                _ = _buffer.Dequeue();
            }
            _buffer.Enqueue(message);
            WeakReferenceMessenger.Default.Send(new CommandlineOutputMessage(message));
        }

        public IEnumerable<string> GetCachedMessages()
        {
            return _buffer.AsEnumerable();
        }
    }
}
