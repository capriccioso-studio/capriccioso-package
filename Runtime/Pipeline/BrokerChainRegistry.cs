using System;
using System.Collections.Generic;
using Capriccioso.Runtime.Logging;

namespace Capriccioso.Runtime.Pipeline
{
    /// <summary>
    /// Global registry for broker chains, following the ServiceLocator pattern.
    /// Allows registering and retrieving chains by context type.
    /// </summary>
    public static class BrokerChainRegistry
    {
        private static readonly Dictionary<Type, object> s_chains = new();
        private static readonly object s_lock = new();

        /// <summary>
        /// Registers a broker chain for a given context type.
        /// </summary>
        public static void Register<TContext>(BrokerChain<TContext> chain, bool overwrite = false) where TContext : IBrokerContext
        {
            if (chain == null) throw new ArgumentNullException(nameof(chain));
            Type type = typeof(TContext);
            lock (s_lock)
            {
                if (s_chains.ContainsKey(type) && !overwrite)
                {
                    throw new InvalidOperationException($"BrokerChain for {type.Name} is already registered. Use overwrite=true to replace.");
                }
                s_chains[type] = chain;
                CLogger.LogSuccess($"BrokerChain registered: {type.Name}");
            }
        }

        /// <summary>
        /// Retrieves a registered broker chain for the specified context type.
        /// </summary>
        public static BrokerChain<TContext> Get<TContext>() where TContext : IBrokerContext
        {
            Type type = typeof(TContext);
            lock (s_lock)
            {
                if (!s_chains.TryGetValue(type, out var chain))
                {
                    throw new InvalidOperationException($"BrokerChain for {type.Name} is not registered.");
                }
                return (BrokerChain<TContext>)chain;
            }
        }

        /// <summary>
        /// Attempts to retrieve a registered broker chain for the specified context type.
        /// </summary>
        public static bool TryGet<TContext>(out BrokerChain<TContext> chain) where TContext : IBrokerContext
        {
            Type type = typeof(TContext);
            lock (s_lock)
            {
                if (s_chains.TryGetValue(type, out var obj))
                {
                    chain = (BrokerChain<TContext>)obj;
                    return true;
                }
                chain = null;
                return false;
            }
        }

        /// <summary>
        /// Removes a broker chain registration for the specified context type.
        /// </summary>
        public static void Unregister<TContext>() where TContext : IBrokerContext
        {
            Type type = typeof(TContext);
            lock (s_lock)
            {
                s_chains.Remove(type);
                CLogger.LogInfo($"BrokerChain unregistered: {type.Name}");
            }
        }
    }
}
