using System;
using System.Collections.Generic;
using UnityEngine;

namespace Capriccioso
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
    /// // Subscribe to an event
    /// EventBus.Subscribe(GameEventType.PlayerDied, OnPlayerDied);
    /// 
    /// private void OnPlayerDied(object data)
    /// {
    ///     // data could be player info, death cause, etc.
    ///     PlayerDeathData deathData = data as PlayerDeathData;
    ///     Debug.Log($"Player died at {deathData.Position}");
    /// }
    /// 
    /// // Publish an event
    /// EventBus.Publish(GameEventType.PlayerDied, new PlayerDeathData 
    /// { 
    ///     Position = transform.position,
    ///     Cause = "Enemy Attack"
    /// });
    /// 
    /// // Unsubscribe when done (important to prevent memory leaks!)
    /// private void OnDestroy()
    /// {
    ///     EventBus.Unsubscribe(GameEventType.PlayerDied, OnPlayerDied);
    /// }
    /// 
    /// // Type-safe event usage
    /// EventBus.Subscribe&lt;ScoreChangedEvent&gt;(OnScoreChanged);
    /// EventBus.Publish(new ScoreChangedEvent { NewScore = 100 });
    /// </code>
    /// </example>
    public static class EventBus
    {
        private static Dictionary<object, List<Delegate>> s_eventTable = new();
        private static readonly object s_lock = new();

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
            }
        }

        #region Enum-based Events (Backwards Compatible)

        /// <summary>
        /// Subscribes a listener to an event type.
        /// </summary>
        /// <typeparam name="TEnum">The enum type defining event types.</typeparam>
        /// <param name="eventType">The event type to subscribe to.</param>
        /// <param name="listener">The callback to invoke when the event is published.</param>
        public static void Subscribe<TEnum>(TEnum eventType, Action<object> listener) where TEnum : Enum
        {
            if (listener == null)
            {
                CLogger.LogWarning($"Attempted to subscribe null listener to event {eventType}");
                return;
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
                    CLogger.LogError($"Error in event handler for {eventType}: {ex.Message}\nStack Trace: {ex.StackTrace}");
                }
            }
        }

        #endregion

        #region Type-Safe Generic Events

        /// <summary>
        /// Subscribes to a strongly-typed event.
        /// </summary>
        /// <typeparam name="T">The event data type.</typeparam>
        /// <param name="listener">The callback to invoke with the event data.</param>
        public static void Subscribe<T>(Action<T> listener) where T : struct
        {
            if (listener == null)
            {
                CLogger.LogWarning($"Attempted to subscribe null listener to event {typeof(T).Name}");
                return;
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
                    CLogger.LogError($"Error in event handler for {eventType.Name}: {ex.Message}\nStack Trace: {ex.StackTrace}");
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

        #endregion
    }
}
