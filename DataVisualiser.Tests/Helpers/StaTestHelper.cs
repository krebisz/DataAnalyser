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

        Exception? captured = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                captured = ex;
            }
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
