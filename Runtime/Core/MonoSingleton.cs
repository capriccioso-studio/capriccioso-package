using UnityEngine;

namespace Capriccioso.Runtime.Core
{
    /// <summary>
    /// Thread-safe singleton base class for MonoBehaviours.
    /// Services should derive from this class for global access.
    /// </summary>
    /// <typeparam name="T">The type of the singleton class.</typeparam>
    /// <example>
    /// <code>
    /// // Creating a service that uses MonoSingleton
    /// public class AudioService : MonoSingleton&lt;AudioService&gt;
    /// {
    ///     protected override bool Awake()
    ///     {
    ///         if (!base.Awake()) return false;
    ///         
    ///         // Initialize audio system
    ///         return true;
    ///     }
    ///     
    ///     public void PlaySound(AudioClip clip)
    ///     {
    ///         // Play sound implementation
    ///     }
    /// }
    /// 
    /// // Usage from anywhere in the codebase
    /// AudioService.Instance.PlaySound(myClip);
    /// </code>
    /// </example>
    public class MonoSingleton<T> : MonoBehaviour
                            where T : MonoBehaviour
    {
        #region Fields
        
        private static T s_instance;
        private static readonly object LOCK = new object();
        private static bool s_applicationQuitting = false;
        
        private bool _isPurged = false;
        
        #endregion

        #region Properties

        /// <summary>
        /// Gets the singleton instance of this class.
        /// Creates a new instance if one does not exist.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (s_applicationQuitting)
                {
                    lock (LOCK)
                    {
                        return s_instance ? s_instance : null;
                    }
                }

                lock (LOCK)
                {
                    if (s_instance == null)
                    {
                        LazyInitialize();
                    }
                }

                lock (LOCK)
                {
                    return s_instance;
                }
            }
        }

        /// <summary>
        /// Returns true if an instance exists without creating one.
        /// </summary>
        public static bool HasInstance
        {
            get
            {
                lock (LOCK)
                {
                    return s_instance != null;
                }
            }
        }

        #endregion

        #region Unity Events

        /// <summary>
        /// Unity calls this when domain reloads to reset static fields.
        /// Prevents stale references from previous play sessions.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            s_applicationQuitting = false;
            s_instance = null;
        }
        
        protected virtual bool Awake()
        {
            return Init();
        }
        
        protected virtual void OnDestroy()
        {
            if (!_isPurged)
            {
                s_applicationQuitting = true;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Manually purges the singleton instance.
        /// Useful for scene transitions or cleanup.
        /// </summary>
        public void Purge()
        {
            _isPurged = true;
            lock (LOCK)
            {
                s_instance = null;
            }
            Destroy(gameObject);
        }

        #endregion

        #region Private Methods
        
        protected virtual bool Init()
        {
            if (s_instance == null)
            {
                LazyInitialize();
            }
            else
            {
                if (s_instance == this)
                {
                    return true;
                }

                Destroy(this.gameObject);
                return false; 
            }

            return true;
        }
        
        private static void LazyInitialize()
        {
            Type type = typeof(T);

            T[] instances = FindObjectsByType<T>(FindObjectsSortMode.None);
            switch (instances.Length)
            {
                case 0:
                    GameObject go = new GameObject($"[{type.Name}]");
                    s_instance = go.AddComponent<T>();
                    DontDestroyOnLoad(go);
                    break;
                case > 1:
                    Debug.LogError($"[MonoSingleton] Multiple instances of {type.Name} found. Using first instance.");
                    s_instance = instances[0];
                    DontDestroyOnLoad(s_instance.gameObject);
                    return;
                default:
                    s_instance = instances[0];
                    DontDestroyOnLoad(s_instance.gameObject);
                    break;
            }
        }
        
        #endregion
    }   
}
