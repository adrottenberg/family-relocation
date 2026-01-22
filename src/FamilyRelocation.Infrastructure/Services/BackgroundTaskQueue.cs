using System.Threading.Channels;
using FamilyRelocation.Application.Common.Interfaces;

namespace FamilyRelocation.Infrastructure.Services;

/// <summary>
/// Channel-based implementation of background task queue.
/// </summary>
public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<IServiceProvider, CancellationToken, Task>> _queue;

    public BackgroundTaskQueue()
    {
        // Use unbounded channel for simplicity
        _queue = Channel.CreateUnbounded<Func<IServiceProvider, CancellationToken, Task>>();
    }

    public void QueueBackgroundWorkItem(Func<IServiceProvider, CancellationToken, Task> workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        _queue.Writer.TryWrite(workItem);
    }

    public async Task<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);
        return workItem;
    }
}
