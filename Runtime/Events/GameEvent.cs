using System.Collections.Generic;
using UnityEngine;

namespace Capriccioso
{
    /// <summary>
    /// ScriptableObject-based event for designer-friendly event workflows.
    /// Create events as assets and reference them in the inspector.
    /// Works with GameEventListener components.
    /// </summary>
    /// <example>
    /// <code>
    /// // 1. Create a GameEvent asset: Right-click > Create > Capriccioso > Events > Game Event
    /// 
    /// // 2. Reference it in your script
    /// public class PlayerHealth : MonoBehaviour
    /// {
    ///     [SerializeField] private GameEvent _onPlayerDied;
    ///     [SerializeField] private GameEvent _onPlayerDamaged;
    ///     
    ///     public void TakeDamage(float damage)
    ///     {
    ///         _currentHealth -= damage;
    ///         
    ///         // Raise the event - all listeners will be notified
    ///         _onPlayerDamaged.Raise();
    ///         
    ///         if (_currentHealth &lt;= 0)
    ///         {
    ///             _onPlayerDied.Raise();
    ///         }
    ///     }
    /// }
    /// 
    /// // 3. Add GameEventListener to any GameObject that needs to respond
    /// //    - Drag the same GameEvent asset to the listener
    /// //    - Configure UnityEvent responses in the inspector
    /// </code>
    /// </example>
    [CreateAssetMenu(fileName = "NewGameEvent", menuName = "Capriccioso/Events/Game Event")]
    public class GameEvent : ScriptableObject
    {
        [TextArea(2, 5)]
        [Tooltip("Description of what this event represents.")]
        [SerializeField] private string _description;

        [Tooltip("Log when this event is raised.")]
        [SerializeField] private bool _logEvent = true;

        private readonly List<GameEventListener> _listeners = new();

        /// <summary>
        /// Raises the event, notifying all registered listeners.
        /// </summary>
        public void Raise()
        {
            if (_logEvent)
            {
                CLogger.LogEvent($"🎯 GameEvent: {name}");
            }

            // Iterate backwards in case listeners unregister during the loop
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                if (_listeners[i] != null)
                {
                    _listeners[i].OnEventRaised();
                }
            }
        }

        /// <summary>
        /// Registers a listener to receive notifications when this event is raised.
        /// Called automatically by GameEventListener.
        /// </summary>
        public void RegisterListener(GameEventListener listener)
        {
            if (listener != null && !_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        /// <summary>
        /// Unregisters a listener from receiving notifications.
        /// Called automatically by GameEventListener.
        /// </summary>
        public void UnregisterListener(GameEventListener listener)
        {
            if (listener != null)
            {
                _listeners.Remove(listener);
            }
        }

        /// <summary>
        /// Gets the current number of registered listeners.
        /// </summary>
        public int ListenerCount => _listeners.Count;
    }
}
