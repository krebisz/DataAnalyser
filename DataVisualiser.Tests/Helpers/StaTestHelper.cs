using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DataVisualiser.Tests.Helpers;

public static class StaTestHelper
{
    public static void Run(Action action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        // Some WPF components (XAML loading, pack URIs, etc.) assume a Dispatcher-based
        // synchronization context. Use a minimal Dispatcher loop for synchronous tests too.
        Exception? captured = null;
        var thread = new Thread(() =>
        {
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));

            void InvokeAction()
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    captured = ex;
                }
                finally
                {
                    Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                }
            }

            Dispatcher.CurrentDispatcher.BeginInvoke((Action)InvokeAction);
            Dispatcher.Run();
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (captured != null)
            throw new InvalidOperationException("STA test failed.", captured);
    }

    public static Task RunAsync(Func<Task> action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));

            async void InvokeAction()
            {
                try
                {
                    await action();
                    tcs.TrySetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(new InvalidOperationException("STA test failed.", ex));
                }
                finally
                {
                    Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                }
            }

            Dispatcher.CurrentDispatcher.BeginInvoke((Action)InvokeAction);
            Dispatcher.Run();
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return tcs.Task;
    }
}
