using System;
using System.Threading.Tasks;

namespace WingetGUIInstaller.Utils
{
    internal static class BackroundTaskUtils
    {
        public static void RunInBackground(Func<Task> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            Task.Run(action);
        }

        public static void RunInBackground(Action action)
        {
            ArgumentNullException.ThrowIfNull(action);
            Task.Run(action);
        }
    }
}
