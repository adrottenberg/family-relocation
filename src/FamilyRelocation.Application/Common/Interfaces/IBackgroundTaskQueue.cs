namespace FamilyRelocation.Application.Common.Interfaces;

/// <summary>
/// Interface for queuing background work items that run asynchronously.
/// </summary>
public interface IBackgroundTaskQueue
{
    /// <summary>
    /// Queues a background work item to be processed asynchronously.
    /// The work item receives a cancellation token and a service provider for DI.
    /// </summary>
    void QueueBackgroundWorkItem(Func<IServiceProvider, CancellationToken, Task> workItem);

    /// <summary>
    /// Dequeues a work item. Used by the background service.
    /// </summary>
    Task<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
}
