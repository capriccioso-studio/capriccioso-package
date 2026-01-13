using System;

namespace Capriccioso.Runtime.Pipeline
{
    /// <summary>
    /// Represents a handler in the broker chain.
    /// Handlers process the context and optionally pass to the next handler.
    /// </summary>
    /// <typeparam name="TContext">The context type this handler processes.</typeparam>
    public interface IBrokerHandler<TContext> where TContext : IBrokerContext
    {
        /// <summary>The priority of this handler (lower executes first).</summary>
        int Priority { get; }
        /// <summary>Processes the context and invokes the next handler if appropriate.</summary>
        void Handle(TContext context, Action<TContext> next);
    }
}
