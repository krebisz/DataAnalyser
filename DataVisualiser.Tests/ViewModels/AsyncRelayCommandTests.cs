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
        await Task.Delay(10);

        Assert.True(command.CanExecute(null));
    }
}