using System.Diagnostics;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.Tests.ViewModels;

public class AsyncRelayCommandTests
{
    [Fact]
    public async Task Execute_RunsAsyncAndBlocksReentry()
    {
        var tcs = new TaskCompletionSource<bool>();
        var started = new TaskCompletionSource<bool>();
        var command = new AsyncRelayCommand(async () =>
        {
            started.SetResult(true);
            await tcs.Task;
        });

        command.Execute(null);
        await started.Task;

        Assert.False(command.CanExecute(null));

        tcs.SetResult(true);
        await WaitUntilAsync(() => command.CanExecute(null));
    }

    private static async Task WaitUntilAsync(Func<bool> condition, int timeoutMs = 1000, int pollMs = 10)
    {
        if (condition())
            return;

        var sw = Stopwatch.StartNew();
        while (sw.ElapsedMilliseconds < timeoutMs)
        {
            await Task.Delay(pollMs);
            if (condition())
                return;
        }

        Assert.True(condition());
    }
}
