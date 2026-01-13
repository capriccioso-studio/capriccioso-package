namespace Capriccioso.Runtime.Pipeline
{
    /// <summary>
    /// Default implementation of IBrokerContext.
    /// </summary>
    public class BrokerContext : IBrokerContext
    {
        public bool IsHandled { get; set; }
        public bool IsCancelled { get; set; }
    }
}
