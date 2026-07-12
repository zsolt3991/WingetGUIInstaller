using Microsoft.UI.Xaml;
using System;

#if UNPACKAGED
using Velopack;
#endif

namespace WingetGUIInstaller
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
#if UNPACKAGED
            // VeloPack must be initialized before anything else.
            // This handles update apply/restart scenarios transparently
            // so the app exits early when an update is being applied.
            VelopackApp.Build()
                .WithFirstRun(v =>
                {
                    // Optional: show a welcome notification on first install
                })
                .Run();
#endif
            WinRT.ComWrappersSupport.InitializeComWrappers();
            Application.Start(p =>
            {
                var context = new global::Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(
                    global::Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
                global::System.Threading.SynchronizationContext.SetSynchronizationContext(context);
                _ = new App();
            });
        }
    }
}
