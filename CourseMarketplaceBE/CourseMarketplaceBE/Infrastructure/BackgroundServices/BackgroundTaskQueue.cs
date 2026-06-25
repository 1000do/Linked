using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.IServices;
using Microsoft.Extensions.DependencyInjection;

namespace CourseMarketplaceBE.Infrastructure.BackgroundServices
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly Channel<Func<IServiceProvider, CancellationToken, ValueTask>> _queue;

        public BackgroundTaskQueue()
        {
            // Capacity should be set based on the expected application load.
            // BoundedChannelFullMode.Wait will cause calls to WriteAsync() to return a task,
            // which completes only when space becomes available. This leads to backpressure,
            // preventing out of memory issues.
            var options = new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<Func<IServiceProvider, CancellationToken, ValueTask>>(options);
        }

        public async ValueTask QueueBackgroundWorkItemAsync<TService>(Func<TService, CancellationToken, ValueTask> workItem) where TService : notnull
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            // Wrap the strongly-typed Func into the generic IServiceProvider Func
            // This keeps the DI resolution strictly inside the Infrastructure layer
            Func<IServiceProvider, CancellationToken, ValueTask> wrapper = (serviceProvider, token) =>
            {
                var service = serviceProvider.GetRequiredService<TService>();
                return workItem(service, token);
            };

            await _queue.Writer.WriteAsync(wrapper);
        }

        public async ValueTask<Func<IServiceProvider, CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken)
        {
            var workItem = await _queue.Reader.ReadAsync(cancellationToken);
            return workItem;
        }
    }
}
