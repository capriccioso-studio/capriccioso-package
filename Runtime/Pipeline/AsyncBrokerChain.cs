using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Capriccioso.Runtime.Logging;

namespace Capriccioso.Runtime.Pipeline
{
    /// <summary>
    /// Async variant of BrokerChain for async/await pipelines.
    /// </summary>
    /// <typeparam name="TContext">The context type for the chain.</typeparam>
    public class AsyncBrokerChain<TContext> where TContext : IBrokerContext
    {
        private readonly List<IAsyncBrokerHandler<TContext>> _handlers = new();
        private readonly object _lock = new();

        public void Register(IAsyncBrokerHandler<TContext> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            lock (_lock)
            {
                _handlers.Add(handler);
                _handlers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                CLogger.LogSuccess($"Async broker handler registered: {handler.GetType().Name} (Priority {handler.Priority})");
            }
        }

        public void Unregister(IAsyncBrokerHandler<TContext> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            lock (_lock)
            {
                _handlers.Remove(handler);
                CLogger.LogInfo($"Async broker handler unregistered: {handler.GetType().Name}");
            }
        }

        public async Task ExecuteAsync(TContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            List<IAsyncBrokerHandler<TContext>> handlersCopy;
            lock (_lock)
            {
                handlersCopy = _handlers.ToList();
            }
            int index = 0;
            async Task Next(TContext ctx)
            {
                if (ctx.IsCancelled || ctx.IsHandled || index >= handlersCopy.Count) return;
                var handler = handlersCopy[index++];
                try
                {
                    await handler.HandleAsync(ctx, Next);
                }
                catch (Exception ex)
                {
                    CLogger.LogError($"Exception in async broker handler {handler.GetType().Name}: {ex}");
                }
            }
            await Next(context);
        }
    }

    /// <summary>
    /// Interface for async broker handlers.
    /// </summary>
    public interface IAsyncBrokerHandler<TContext> where TContext : IBrokerContext
    {
        int Priority { get; }
        Task HandleAsync(TContext context, Func<TContext, Task> next);
    }
}
