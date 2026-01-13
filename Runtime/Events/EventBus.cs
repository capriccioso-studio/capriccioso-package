using System;
using System.Collections.Generic;
using UnityEngine;
using Capriccioso.Runtime.Logging;

namespace Capriccioso.Runtime.Events
{
    /// <summary>
    /// Improved static event bus for decoupled communication between systems.
    /// Supports both enum-based event types and generic type-safe events.
    /// </summary>
    /// <example>
    /// <code>
    /// // Define your event types in an enum
    /// public enum GameEventType
    /// {
    ///     PlayerSpawned,
    ///     PlayerDied,
    ///     LevelComplete,
    ///     ScoreChanged
    /// }
    /// 
    /// // Subscribe to an event (manual unsubscribe)
    /// EventBus.Subscribe(GameEventType.PlayerDied, OnPlayerDied);
    /// 
    /// // Subscribe with auto-dispose (recommended)
    /// private EventSubscription _subscription;
    /// 
    /// void Start()
    /// {
    ///     _subscription = EventBus.Subscribe(GameEventType.PlayerDied, OnPlayerDied);
    /// }
    /// 
    /// void OnDestroy()
    /// {
    ///     _subscription?.Dispose(); // Automatically unsubscribes
    /// }
    /// 
    /// // Type-safe events with auto-dispose
    /// private EventSubscription _scoreSubscription;
    /// 
    /// void Start()
    /// {
    ///     _scoreSubscription = EventBus.Subscribe&lt;ScoreChangedEvent&gt;(OnScoreChanged);
    /// }
    /// </code>
    /// </example>
    public static class EventBus
    {
        private static Dictionary<object, List<Delegate>> s_eventTable = new();
        private static readonly object s_lock = new();
        
        /// <summary>
        /// Optional callback invoked when an event handler throws an exception.
        /// If null, errors are logged via CLogger.LogError.
        /// </summary>
        public static Action<Exception, object> OnEventError { get; set; }

        /// <summary>
        /// Unity calls this when domain reloads to reset static fields.
        /// Prevents stale event handlers from previous play sessions.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            lock (s_lock)
            {
                s_eventTable?.Clear();
                s_eventTable = new Dictionary<object, List<Delegate>>();
                OnEventError = null;
            }
        }

        #region Enum-based Events (Backwards Compatible)

        /// <summary>
        /// Subscribes a listener to an event type.
        /// Returns a disposable subscription for automatic cleanup.
        /// </summary>
        /// <typeparam name="TEnum">The enum type defining event types.</typeparam>
        /// <param name="eventType">The event type to subscribe to.</param>
        /// <param name="listener">The callback to invoke when the event is published.</param>
        /// <returns>An EventSubscription that can be disposed to unsubscribe.</returns>
        public static EventSubscription Subscribe<TEnum>(TEnum eventType, Action<object> listener) where TEnum : Enum
        {
            if (listener == null)
            {
                CLogger.LogWarning($"Attempted to subscribe null listener to event {eventType}");
                return null;
            }

            lock (s_lock)
            {
                if (!s_eventTable.TryGetValue(eventType, out List<Delegate> listeners))
                {
                    listeners = new List<Delegate>();
                    s_eventTable[eventType] = listeners;
                }
                
                if (!listeners.Contains(listener))
                {
                    listeners.Add(listener);
                }
            }
            
            return new EventSubscription(() => Unsubscribe(eventType, listener));
        }

        /// <summary>
        /// Unsubscribes a listener from an event type.
        /// </summary>
        public static void Unsubscribe<TEnum>(TEnum eventType, Action<object> listener) where TEnum : Enum
        {
            if (listener == null)
            {
                CLogger.LogWarning($"Attempted to unsubscribe null listener from event {eventType}");
                return;
            }

            lock (s_lock)
            {
                if (s_eventTable.TryGetValue(eventType, out List<Delegate> listeners))
                {
                    listeners.Remove(listener);
                    if (listeners.Count == 0)
                    {
                        s_eventTable.Remove(eventType);
                    }
                }
            }
        }

        /// <summary>
        /// Publishes an event to all subscribers.
        /// </summary>
        /// <param name="eventType">The event type to publish.</param>
        /// <param name="param">Optional data to pass to listeners.</param>
        public static void Publish<TEnum>(TEnum eventType, object param = null) where TEnum : Enum
        {
            CLogger.LogEvent($"🍁 Event: {eventType}");
            
            List<Delegate> listenersCopy;
            
            lock (s_lock)
            {
                if (!s_eventTable.TryGetValue(eventType, out List<Delegate> listeners) || listeners.Count == 0)
                {
                    return;
                }
                
                // Copy to avoid modification during iteration
                listenersCopy = new List<Delegate>(listeners);
            }

            foreach (Delegate handler in listenersCopy)
            {
                try
                {
                    ((Action<object>)handler).Invoke(param);
                }
                catch (Exception ex)
                {
                    HandleEventError(ex, eventType);
                }
            }
        }

        #endregion

        #region Type-Safe Generic Events

        /// <summary>
        /// Subscribes to a strongly-typed event.
        /// Returns a disposable subscription for automatic cleanup.
        /// </summary>
        /// <typeparam name="T">The event data type.</typeparam>
        /// <param name="listener">The callback to invoke with the event data.</param>
        /// <returns>An EventSubscription that can be disposed to unsubscribe.</returns>
        public static EventSubscription Subscribe<T>(Action<T> listener) where T : struct
        {
            if (listener == null)
            {
                CLogger.LogWarning($"Attempted to subscribe null listener to event {typeof(T).Name}");
                return null;
            }

            Type eventType = typeof(T);
            
            lock (s_lock)
            {
                if (!s_eventTable.TryGetValue(eventType, out List<Delegate> listeners))
                {
                    listeners = new List<Delegate>();
                    s_eventTable[eventType] = listeners;
                }
                
                if (!listeners.Contains(listener))
                {
                    listeners.Add(listener);
                }
            }
            
            return new EventSubscription(() => Unsubscribe(listener));
        }

        /// <summary>
        /// Unsubscribes from a strongly-typed event.
        /// </summary>
        public static void Unsubscribe<T>(Action<T> listener) where T : struct
        {
            if (listener == null) return;

            Type eventType = typeof(T);
            
            lock (s_lock)
            {
                if (s_eventTable.TryGetValue(eventType, out List<Delegate> listeners))
                {
                    listeners.Remove(listener);
                    if (listeners.Count == 0)
                    {
                        s_eventTable.Remove(eventType);
                    }
                }
            }
        }

        /// <summary>
        /// Publishes a strongly-typed event.
        /// </summary>
        /// <typeparam name="T">The event data type.</typeparam>
        /// <param name="eventData">The event data to send to listeners.</param>
        public static void Publish<T>(T eventData) where T : struct
        {
            Type eventType = typeof(T);
            CLogger.LogEvent($"🍁 Event: {eventType.Name}");
            
            List<Delegate> listenersCopy;
            
            lock (s_lock)
            {
                if (!s_eventTable.TryGetValue(eventType, out List<Delegate> listeners) || listeners.Count == 0)
                {
                    return;
                }
                
                listenersCopy = new List<Delegate>(listeners);
            }

            foreach (Delegate handler in listenersCopy)
            {
                try
                {
                    ((Action<T>)handler).Invoke(eventData);
                }
                catch (Exception ex)
                {
                    HandleEventError(ex, eventType);
                }
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Removes all subscriptions for a specific event type.
        /// </summary>
        public static void UnsubscribeAll<TEnum>(TEnum eventType) where TEnum : Enum
        {
            lock (s_lock)
            {
                s_eventTable.Remove(eventType);
            }
        }

        /// <summary>
        /// Clears all event subscriptions. Use with caution!
        /// </summary>
        public static void Clear()
        {
            lock (s_lock)
            {
                s_eventTable.Clear();
            }
            CLogger.LogWarning("EventBus cleared all subscriptions.");
        }

        /// <summary>
        /// Gets the number of subscribers for a specific event type.
        /// Useful for debugging.
        /// </summary>
        public static int GetSubscriberCount<TEnum>(TEnum eventType) where TEnum : Enum
        {
            lock (s_lock)
            {
                if (s_eventTable.TryGetValue(eventType, out List<Delegate> listeners))
                {
                    return listeners.Count;
                }
            }
            return 0;
        }

        /// <summary>
        /// Gets the number of subscribers for a type-safe event.
        /// </summary>
        public static int GetSubscriberCount<T>() where T : struct
        {
            Type eventType = typeof(T);
            
            lock (s_lock)
            {
                if (s_eventTable.TryGetValue(eventType, out List<Delegate> listeners))
                {
                    return listeners.Count;
                }
            }
            return 0;
        }
        
        private static void HandleEventError(Exception ex, object eventType)
        {
            if (OnEventError != null)
            {
                try
                {
                    OnEventError.Invoke(ex, eventType);
                }
                catch
                {
                    // Prevent error handler from throwing
                }
            }
            else
            {
                CLogger.LogError($"Error in event handler for {eventType}: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
        }

        #endregion
    }
    
    /// <summary>
    /// Represents a subscription to an event that can be disposed to unsubscribe.
    /// Use this for automatic cleanup in OnDestroy or when the subscriber is no longer needed.
    /// </summary>
    /// <example>
    /// <code>
    /// public class PlayerUI : MonoBehaviour
    /// {
    ///     private EventSubscription _healthSubscription;
    ///     private EventSubscription _scoreSubscription;
    ///     
    ///     private void Start()
    ///     {
    ///         _healthSubscription = EventBus.Subscribe&lt;HealthChangedEvent&gt;(OnHealthChanged);
    ///         _scoreSubscription = EventBus.Subscribe(GameEvent.ScoreChanged, OnScoreChanged);
    ///     }
    ///     
    ///     private void OnDestroy()
    ///     {
    ///         // Clean up subscriptions
    ///         _healthSubscription?.Dispose();
    ///         _scoreSubscription?.Dispose();
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class EventSubscription : IDisposable
    {
        private Action _unsubscribeAction;
        private bool _disposed;
        
        internal EventSubscription(Action unsubscribeAction)
        {
            _unsubscribeAction = unsubscribeAction;
        }
        
        /// <summary>
        /// Unsubscribes from the event. Safe to call multiple times.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            
            _unsubscribeAction?.Invoke();
            _unsubscribeAction = null;
        }
    }
}
