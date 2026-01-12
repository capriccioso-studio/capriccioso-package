using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Capriccioso
{
    /// <summary>
    /// Generic object pool wrapper around Unity's ObjectPool.
    /// Provides a simple interface for pooling GameObjects or regular objects.
    /// </summary>
    /// <remarks>
    /// Unity 2021+ includes UnityEngine.Pool.ObjectPool. This class provides
    /// a simplified wrapper with common defaults and logging support.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Pooling GameObjects (e.g., bullets, particles)
    /// public class BulletPool : MonoBehaviour
    /// {
    ///     [SerializeField] private GameObject _bulletPrefab;
    ///     
    ///     private GameObjectPool _pool;
    ///     
    ///     private void Awake()
    ///     {
    ///         _pool = new GameObjectPool(
    ///             createFunc: () => Instantiate(_bulletPrefab),
    ///             actionOnGet: bullet => bullet.SetActive(true),
    ///             actionOnRelease: bullet => bullet.SetActive(false),
    ///             actionOnDestroy: bullet => Destroy(bullet),
    ///             defaultCapacity: 20,
    ///             maxSize: 100
    ///         );
    ///     }
    ///     
    ///     public GameObject GetBullet()
    ///     {
    ///         return _pool.Get();
    ///     }
    ///     
    ///     public void ReturnBullet(GameObject bullet)
    ///     {
    ///         _pool.Release(bullet);
    ///     }
    /// }
    /// 
    /// // Pooling regular objects
    /// public class ConnectionPool
    /// {
    ///     private ObjectPool&lt;NetworkConnection&gt; _pool;
    ///     
    ///     public ConnectionPool()
    ///     {
    ///         _pool = new ObjectPool&lt;NetworkConnection&gt;(
    ///             createFunc: () => new NetworkConnection(),
    ///             actionOnGet: conn => conn.Open(),
    ///             actionOnRelease: conn => conn.Reset()
    ///         );
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <typeparam name="T">The type of object to pool.</typeparam>
    public class ObjectPool<T> : IDisposable where T : class
    {
        private readonly IObjectPool<T> _pool;
        private bool _disposed = false;

        /// <summary>
        /// Creates a new object pool.
        /// </summary>
        /// <param name="createFunc">Function to create new instances.</param>
        /// <param name="actionOnGet">Called when an object is retrieved from the pool.</param>
        /// <param name="actionOnRelease">Called when an object is returned to the pool.</param>
        /// <param name="actionOnDestroy">Called when an object is destroyed (pool overflow).</param>
        /// <param name="collectionCheck">If true, checks that objects are not already in pool when released.</param>
        /// <param name="defaultCapacity">Initial pool capacity.</param>
        /// <param name="maxSize">Maximum pool size.</param>
        public ObjectPool(
            Func<T> createFunc,
            Action<T> actionOnGet = null,
            Action<T> actionOnRelease = null,
            Action<T> actionOnDestroy = null,
            bool collectionCheck = true,
            int defaultCapacity = Constants.DefaultPoolSize,
            int maxSize = Constants.DefaultMaxPoolSize)
        {
            _pool = new UnityEngine.Pool.ObjectPool<T>(
                createFunc: createFunc,
                actionOnGet: actionOnGet,
                actionOnRelease: actionOnRelease,
                actionOnDestroy: actionOnDestroy,
                collectionCheck: collectionCheck,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
        }

        /// <summary>
        /// Gets an object from the pool. Creates a new one if pool is empty.
        /// </summary>
        public T Get() => _pool.Get();

        /// <summary>
        /// Returns an object to the pool.
        /// </summary>
        public void Release(T element) => _pool.Release(element);

        /// <summary>
        /// Gets a pooled object wrapped in a disposable handle.
        /// Object is automatically returned when the handle is disposed.
        /// </summary>
        /// <example>
        /// <code>
        /// using (PooledObject&lt;MyClass&gt; pooled = pool.Get(out MyClass obj))
        /// {
        ///     // Use obj...
        /// } // Automatically returned to pool
        /// </code>
        /// </example>
        public PooledObject<T> Get(out T element)
        {
            return _pool.Get(out element);
        }

        /// <summary>Current number of inactive objects in the pool.</summary>
        public int CountInactive => _pool.CountInactive;

        /// <summary>Clears all objects from the pool.</summary>
        public void Clear() => _pool.Clear();

        public void Dispose()
        {
            if (!_disposed)
            {
                _pool.Clear();
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Specialized pool for GameObjects with common defaults.
    /// </summary>
    public class GameObjectPool : ObjectPool<GameObject>
    {
        /// <summary>
        /// Creates a new GameObject pool.
        /// </summary>
        /// <param name="createFunc">Function to create/instantiate GameObjects.</param>
        /// <param name="actionOnGet">Called when a GameObject is retrieved (default: SetActive(true)).</param>
        /// <param name="actionOnRelease">Called when a GameObject is returned (default: SetActive(false)).</param>
        /// <param name="actionOnDestroy">Called when a GameObject is destroyed.</param>
        /// <param name="defaultCapacity">Initial pool capacity.</param>
        /// <param name="maxSize">Maximum pool size.</param>
        public GameObjectPool(
            Func<GameObject> createFunc,
            Action<GameObject> actionOnGet = null,
            Action<GameObject> actionOnRelease = null,
            Action<GameObject> actionOnDestroy = null,
            int defaultCapacity = Constants.DefaultPoolSize,
            int maxSize = Constants.DefaultMaxPoolSize)
            : base(
                createFunc: createFunc,
                actionOnGet: actionOnGet ?? (go => go.SetActive(true)),
                actionOnRelease: actionOnRelease ?? (go => go.SetActive(false)),
                actionOnDestroy: actionOnDestroy ?? (go => UnityEngine.Object.Destroy(go)),
                collectionCheck: true,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize)
        {
        }
    }
}
