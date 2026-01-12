using System;
using System.Collections.Generic;
using UnityEngine;

namespace Capriccioso
{
    /// <summary>
    /// A simple service locator pattern implementation.
    /// Provides a centralized registry for services without tight coupling.
    /// Can be used alongside MonoSingleton for more flexible dependency management.
    /// </summary>
    /// <example>
    /// <code>
    /// // Registering a service (typically in a bootstrap/initialization script)
    /// ServiceLocator.Register&lt;IAudioService&gt;(new AudioService());
    /// 
    /// // Or register an existing MonoSingleton service
    /// ServiceLocator.Register&lt;IAudioService&gt;(AudioService.Instance);
    /// 
    /// // Retrieving a service from anywhere
    /// IAudioService audio = ServiceLocator.Get&lt;IAudioService&gt;();
    /// audio.PlaySound("explosion");
    /// 
    /// // Safe retrieval with TryGet
    /// if (ServiceLocator.TryGet&lt;IAnalyticsService&gt;(out IAnalyticsService analytics))
    /// {
    ///     analytics.LogEvent("player_died");
    /// }
    /// 
    /// // Using with MonoSingleton pattern
    /// public class GameManager : MonoSingleton&lt;GameManager&gt;, IGameManager
    /// {
    ///     protected override bool Awake()
    ///     {
    ///         if (!base.Awake()) return false;
    ///         
    ///         // Register self as the IGameManager implementation
    ///         ServiceLocator.Register&lt;IGameManager&gt;(this);
    ///         return true;
    ///     }
    /// }
    /// </code>
    /// </example>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> s_services = new();
        private static readonly object s_lock = new();

        /// <summary>
        /// Unity calls this when domain reloads to reset static fields.
        /// Prevents stale service references from previous play sessions.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            lock (s_lock)
            {
                s_services.Clear();
            }
        }

        /// <summary>
        /// Registers a service implementation for a given interface type.
        /// </summary>
        /// <typeparam name="T">The interface or base type to register.</typeparam>
        /// <param name="service">The service implementation instance.</param>
        /// <param name="overwrite">If true, overwrites existing registration. Default is false.</param>
        /// <exception cref="ArgumentNullException">Thrown if service is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if service already registered and overwrite is false.</exception>
        public static void Register<T>(T service, bool overwrite = false) where T : class
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service), "Cannot register null service.");
            }

            Type type = typeof(T);
            
            lock (s_lock)
            {
                if (s_services.ContainsKey(type) && !overwrite)
                {
                    throw new InvalidOperationException(
                        $"Service of type {type.Name} is already registered. Use overwrite=true to replace.");
                }
                
                s_services[type] = service;
                CLogger.LogSuccess($"Service registered: {type.Name}");
            }
        }

        /// <summary>
        /// Retrieves a registered service of the specified type.
        /// </summary>
        /// <typeparam name="T">The interface or base type to retrieve.</typeparam>
        /// <returns>The registered service instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown if service is not registered.</exception>
        public static T Get<T>() where T : class
        {
            Type type = typeof(T);
            
            lock (s_lock)
            {
                if (s_services.TryGetValue(type, out object service))
                {
                    return (T)service;
                }
            }

            throw new InvalidOperationException(
                $"Service of type {type.Name} is not registered. Register it before use.");
        }

        /// <summary>
        /// Attempts to retrieve a registered service of the specified type.
        /// </summary>
        /// <typeparam name="T">The interface or base type to retrieve.</typeparam>
        /// <param name="service">The service instance if found; otherwise, null.</param>
        /// <returns>True if the service was found; otherwise, false.</returns>
        public static bool TryGet<T>(out T service) where T : class
        {
            Type type = typeof(T);
            
            lock (s_lock)
            {
                if (s_services.TryGetValue(type, out object obj))
                {
                    service = (T)obj;
                    return true;
                }
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Checks if a service of the specified type is registered.
        /// </summary>
        /// <typeparam name="T">The interface or base type to check.</typeparam>
        /// <returns>True if the service is registered; otherwise, false.</returns>
        public static bool IsRegistered<T>() where T : class
        {
            Type type = typeof(T);
            
            lock (s_lock)
            {
                return s_services.ContainsKey(type);
            }
        }

        /// <summary>
        /// Unregisters a service of the specified type.
        /// </summary>
        /// <typeparam name="T">The interface or base type to unregister.</typeparam>
        /// <returns>True if the service was unregistered; otherwise, false.</returns>
        public static bool Unregister<T>() where T : class
        {
            Type type = typeof(T);
            
            lock (s_lock)
            {
                bool removed = s_services.Remove(type);
                if (removed)
                {
                    CLogger.LogInfo($"Service unregistered: {type.Name}");
                }
                return removed;
            }
        }

        /// <summary>
        /// Clears all registered services.
        /// Use with caution, typically only during scene transitions or shutdown.
        /// </summary>
        public static void Clear()
        {
            lock (s_lock)
            {
                s_services.Clear();
                CLogger.LogWarning("ServiceLocator cleared all services.");
            }
        }

        /// <summary>
        /// Gets the count of registered services.
        /// Useful for debugging and testing.
        /// </summary>
        public static int ServiceCount
        {
            get
            {
                lock (s_lock)
                {
                    return s_services.Count;
                }
            }
        }
    }
}
