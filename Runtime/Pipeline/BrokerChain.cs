using System;
using System.Collections.Generic;
using System.Linq;
using Capriccioso.Runtime.Logging;

namespace Capriccioso.Runtime.Pipeline
{
    /// <summary>
    /// Manages a chain of handlers that process requests sequentially.
    /// Combines Chain of Responsibility with a central broker for registration.
    /// </summary>
    /// <typeparam name="TContext">The context type for the chain.</typeparam>
    public class BrokerChain<TContext> where TContext : IBrokerContext
    {
        private readonly List<IBrokerHandler<TContext>> _handlers = new();
        private readonly object _lock = new();

        /// <summary>
        /// Registers a handler in the chain.
        /// </summary>
        public void Register(IBrokerHandler<TContext> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            lock (_lock)
            {
                _handlers.Add(handler);
                _handlers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                CLogger.LogSuccess($"Broker handler registered: {handler.GetType().Name} (Priority {handler.Priority})");
            }
        }

        /// <summary>
        /// Unregisters a handler from the chain.
        /// </summary>
        public void Unregister(IBrokerHandler<TContext> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            lock (_lock)
            {
                _handlers.Remove(handler);
                CLogger.LogInfo($"Broker handler unregistered: {handler.GetType().Name}");
            }
        }

        /// <summary>
        /// Executes the chain for the given context.
        /// </summary>
        public void Execute(TContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            List<IBrokerHandler<TContext>> handlersCopy;
            lock (_lock)
            {
                handlersCopy = _handlers.ToList();
            }
            int index = 0;
            void Next(TContext ctx)
            {
                if (ctx.IsCancelled || ctx.IsHandled || index >= handlersCopy.Count) return;
                var handler = handlersCopy[index++];
                try
                {
                    handler.Handle(ctx, Next);
                }
                catch (Exception ex)
                {
                    CLogger.LogError($"Exception in broker handler {handler.GetType().Name}: {ex}");
                }
            }
            Next(context);
        }
    }
}
