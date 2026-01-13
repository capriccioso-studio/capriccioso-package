using UnityEngine;
using Capriccioso.Runtime.Logging;

namespace Capriccioso.Runtime.Events
{
    /// <summary>
    /// Generic GameEvent that carries data of type T.
    /// Use for events that need to pass information to listeners.
    /// </summary>
    /// <typeparam name="T">The type of data this event carries.</typeparam>
    /// <example>
    /// <code>
    /// // Create a typed event by inheriting
    /// [CreateAssetMenu(fileName = "IntEvent", menuName = "Capriccioso/Events/Int Event")]
    /// public class IntGameEvent : GameEvent&lt;int&gt; { }
    /// 
    /// // Use in your scripts
    /// public class ScoreManager : MonoBehaviour
    /// {
    ///     [SerializeField] private IntGameEvent _onScoreChanged;
    ///     
    ///     private int _score;
    ///     
    ///     public void AddScore(int points)
    ///     {
    ///         _score += points;
    ///         _onScoreChanged.Raise(_score);
    ///     }
    /// }
    /// </code>
    /// </example>
    public abstract class GameEvent<T> : ScriptableObject
    {
        [TextArea(2, 5)]
        [Tooltip("Description of what this event represents.")]
        [SerializeField] private string _description;

        [Tooltip("Log when this event is raised.")]
        [SerializeField] private bool _logEvent = true;

        private readonly System.Collections.Generic.List<GameEventListener<T>> _listeners = new();

        /// <summary>
        /// Raises the event with the specified data.
        /// </summary>
        /// <param name="value">The data to pass to listeners.</param>
        public void Raise(T value)
        {
            if (_logEvent)
            {
                CLogger.LogEvent($"🎯 GameEvent<{typeof(T).Name}>: {name} = {value}");
            }

            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                if (_listeners[i] != null)
                {
                    _listeners[i].OnEventRaised(value);
                }
            }
        }

        /// <summary>
        /// Registers a listener.
        /// </summary>
        public void RegisterListener(GameEventListener<T> listener)
        {
            if (listener != null && !_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        /// <summary>
        /// Unregisters a listener.
        /// </summary>
        public void UnregisterListener(GameEventListener<T> listener)
        {
            if (listener != null)
            {
                _listeners.Remove(listener);
            }
        }

        public int ListenerCount => _listeners.Count;
    }

    /// <summary>
    /// Generic listener for typed GameEvents.
    /// </summary>
    public abstract class GameEventListener<T> : MonoBehaviour
    {
        [SerializeField] protected GameEvent<T> _event;

        protected virtual void OnEnable()
        {
            if (_event != null)
            {
                _event.RegisterListener(this);
            }
        }

        protected virtual void OnDisable()
        {
            if (_event != null)
            {
                _event.UnregisterListener(this);
            }
        }

        /// <summary>
        /// Called when the event is raised. Override to handle the data.
        /// </summary>
        public abstract void OnEventRaised(T value);
    }

    #region Common Typed Events

    /// <summary>Integer-based GameEvent.</summary>
    [CreateAssetMenu(fileName = "IntEvent", menuName = "Capriccioso/Events/Int Event")]
    public class IntGameEvent : GameEvent<int> { }

    /// <summary>Float-based GameEvent.</summary>
    [CreateAssetMenu(fileName = "FloatEvent", menuName = "Capriccioso/Events/Float Event")]
    public class FloatGameEvent : GameEvent<float> { }

    /// <summary>String-based GameEvent.</summary>
    [CreateAssetMenu(fileName = "StringEvent", menuName = "Capriccioso/Events/String Event")]
    public class StringGameEvent : GameEvent<string> { }

    /// <summary>Bool-based GameEvent.</summary>
    [CreateAssetMenu(fileName = "BoolEvent", menuName = "Capriccioso/Events/Bool Event")]
    public class BoolGameEvent : GameEvent<bool> { }

    /// <summary>Vector3-based GameEvent.</summary>
    [CreateAssetMenu(fileName = "Vector3Event", menuName = "Capriccioso/Events/Vector3 Event")]
    public class Vector3GameEvent : GameEvent<Vector3> { }

    #endregion
}
