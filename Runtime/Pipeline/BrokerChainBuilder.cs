using System;
using System.Collections.Generic;

namespace Capriccioso.Runtime.Pipeline
{
    /// <summary>
    /// Fluent builder for constructing broker chains.
    /// </summary>
    /// <typeparam name="TContext">The context type for the chain.</typeparam>
    public class BrokerChainBuilder<TContext> where TContext : IBrokerContext
    {
        private readonly List<IBrokerHandler<TContext>> _handlers = new();

        public BrokerChainBuilder<TContext> AddHandler(IBrokerHandler<TContext> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            _handlers.Add(handler);
            return this;
        }

        public BrokerChain<TContext> Build()
        {
            var chain = new BrokerChain<TContext>();
            foreach (var handler in _handlers)
            {
                chain.Register(handler);
            }
            return chain;
        }
    }
}
