using UnityEngine;
using UnityEngine.Events;

namespace Capriccioso
{
    /// <summary>
    /// Component that listens for a GameEvent and invokes a UnityEvent response.
    /// Attach to any GameObject and configure the response in the inspector.
    /// </summary>
    /// <example>
    /// <code>
    /// // In the Inspector:
    /// // 1. Add GameEventListener component to a GameObject
    /// // 2. Drag a GameEvent asset to the "Event" field
    /// // 3. Configure the "Response" UnityEvent with methods to call
    /// 
    /// // You can also configure it via code:
    /// public class UIManager : MonoBehaviour
    /// {
    ///     [SerializeField] private GameEvent _onPlayerDied;
    ///     
    ///     private GameEventListener _listener;
    ///     
    ///     private void Awake()
    ///     {
    ///         _listener = gameObject.AddComponent&lt;GameEventListener&gt;();
    ///         _listener.Event = _onPlayerDied;
    ///         _listener.Response.AddListener(ShowGameOverScreen);
    ///     }
    ///     
    ///     private void ShowGameOverScreen()
    ///     {
    ///         Debug.Log("Showing game over screen...");
    ///     }
    /// }
    /// </code>
    /// </example>
    public class GameEventListener : MonoBehaviour
    {
        [Tooltip("The GameEvent asset to listen for.")]
        [SerializeField] private GameEvent _event;
        
        [Tooltip("The response to invoke when the event is raised.")]
        [SerializeField] private UnityEvent _response;

        /// <summary>
        /// The GameEvent this listener is subscribed to.
        /// </summary>
        public GameEvent Event
        {
            get => _event;
            set
            {
                // Unregister from old event
                if (_event != null && enabled)
                {
                    _event.UnregisterListener(this);
                }
                
                _event = value;
                
                // Register to new event
                if (_event != null && enabled)
                {
                    _event.RegisterListener(this);
                }
            }
        }

        /// <summary>
        /// The UnityEvent invoked when the GameEvent is raised.
        /// </summary>
        public UnityEvent Response
        {
            get
            {
                _response ??= new UnityEvent();
                return _response;
            }
        }

        private void OnEnable()
        {
            if (_event != null)
            {
                _event.RegisterListener(this);
            }
        }

        private void OnDisable()
        {
            if (_event != null)
            {
                _event.UnregisterListener(this);
            }
        }

        /// <summary>
        /// Called by the GameEvent when it is raised.
        /// Invokes the configured response.
        /// </summary>
        public void OnEventRaised()
        {
            _response?.Invoke();
        }
    }
}
