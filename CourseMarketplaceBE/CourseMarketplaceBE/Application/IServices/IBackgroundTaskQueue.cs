using System;
using System.Threading;
using System.Threading.Tasks;

namespace CourseMarketplaceBE.Application.IServices
{
    public interface IBackgroundTaskQueue
    {
        ValueTask QueueBackgroundWorkItemAsync<TService>(Func<TService, CancellationToken, ValueTask> workItem) where TService : notnull;
        ValueTask<Func<IServiceProvider, CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken);
    }
}
