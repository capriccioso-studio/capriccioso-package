using System;

namespace Capriccioso.Runtime.Pipeline
{
    /// <summary>
    /// Represents the context passed through a broker chain.
    /// Contains the request data and any accumulated state.
    /// </summary>
    public interface IBrokerContext
    {
        /// <summary>Gets or sets whether processing should stop.</summary>
        bool IsHandled { get; set; }
        /// <summary>Gets or sets whether the chain should be aborted.</summary>
        bool IsCancelled { get; set; }
    }
}
