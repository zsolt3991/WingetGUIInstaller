using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WingetGUIInstaller.Messages;

namespace WingetGUIInstaller.Services
{
    public class ConsoleOutputCache
    {
        private const int MaxCapacity = 255;
        private readonly ConcurrentQueue<string> _buffer;

        public ConsoleOutputCache()
        {
            _buffer = new ConcurrentQueue<string>();
        }

        public void IngestMessage(string message)
        {
            // Remove oldest entry from the buffer
            if (_buffer.Count == MaxCapacity)
            {
                _ = _buffer.TryDequeue(out var _);
            }
            _buffer.Enqueue(message);
            WeakReferenceMessenger.Default.Send(new CommandlineOutputMessage(message));
        }

        public IEnumerable<string> GetCachedMessages()
        {
            return _buffer.AsEnumerable().ToList();
        }
    }
}
