using System.Collections.Generic;
using UnityEngine;

namespace Capriccioso
{
    /// <summary>
    /// ScriptableObject-based runtime collection for tracking active objects.
    /// Useful for maintaining lists of entities without tight coupling.
    /// Objects register/unregister themselves; other systems query the set.
    /// </summary>
    /// <example>
    /// <code>
    /// // 1. Create the RuntimeSet asset:
    /// //    Right-click > Create > Capriccioso > Data > Runtime Set (GameObject)
    /// 
    /// // 2. Reference in your tracked objects:
    /// public class Enemy : MonoBehaviour
    /// {
    ///     [SerializeField] private GameObjectRuntimeSet _allEnemies;
    ///     
    ///     private void OnEnable()
    ///     {
    ///         _allEnemies.Add(gameObject);
    ///     }
    ///     
    ///     private void OnDisable()
    ///     {
    ///         _allEnemies.Remove(gameObject);
    ///     }
    /// }
    /// 
    /// // 3. Query from other systems:
    /// public class WaveManager : MonoBehaviour
    /// {
    ///     [SerializeField] private GameObjectRuntimeSet _allEnemies;
    ///     
    ///     public bool AllEnemiesDefeated => _allEnemies.Count == 0;
    ///     
    ///     public void DamageAllEnemies(float damage)
    ///     {
    ///         foreach (GameObject enemy in _allEnemies.Items)
    ///         {
    ///             enemy.GetComponent&lt;Health&gt;().TakeDamage(damage);
    ///         }
    ///     }
    ///     
    ///     public GameObject GetClosestEnemy(Vector3 position)
    ///     {
    ///         return _allEnemies.GetClosest(position);
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <typeparam name="T">The type of items to track.</typeparam>
    public abstract class RuntimeSet<T> : ScriptableObject
    {
        [SerializeField] private List<T> _items = new();

        /// <summary>Read-only access to all items in the set.</summary>
        public IReadOnlyList<T> Items => _items;

        /// <summary>Number of items currently in the set.</summary>
        public int Count => _items.Count;

        /// <summary>
        /// Adds an item to the set if not already present.
        /// </summary>
        public void Add(T item)
        {
            if (item != null && !_items.Contains(item))
            {
                _items.Add(item);
            }
        }

        /// <summary>
        /// Removes an item from the set.
        /// </summary>
        public void Remove(T item)
        {
            if (item != null)
            {
                _items.Remove(item);
            }
        }

        /// <summary>
        /// Clears all items from the set.
        /// </summary>
        public void Clear()
        {
            _items.Clear();
        }

        /// <summary>
        /// Checks if the set contains the item.
        /// </summary>
        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        /// <summary>
        /// Gets a random item from the set.
        /// </summary>
        public T GetRandom()
        {
            if (_items.Count == 0) return default;
            return _items[Random.Range(0, _items.Count)];
        }

        /// <summary>
        /// Called when the asset is loaded. Ensures clean state.
        /// </summary>
        private void OnEnable()
        {
            // Clear on play mode start to avoid stale references
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChanged;
            #endif
        }

        #if UNITY_EDITOR
        private void OnPlayModeChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                Clear();
            }
        }
        #endif
    }

    /// <summary>
    /// RuntimeSet for GameObjects with additional spatial queries.
    /// </summary>
    [CreateAssetMenu(fileName = "GameObjectSet", menuName = "Capriccioso/Data/Runtime Set (GameObject)")]
    public class GameObjectRuntimeSet : RuntimeSet<GameObject>
    {
        /// <summary>
        /// Gets the closest GameObject to a world position.
        /// </summary>
        public GameObject GetClosest(Vector3 position)
        {
            GameObject closest = null;
            float closestDistance = float.MaxValue;

            foreach (GameObject item in Items)
            {
                if (item == null) continue;
                
                float distance = Vector3.Distance(item.transform.position, position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = item;
                }
            }

            return closest;
        }

        /// <summary>
        /// Gets all GameObjects within a radius of a position.
        /// </summary>
        public List<GameObject> GetWithinRadius(Vector3 position, float radius)
        {
            List<GameObject> result = new List<GameObject>();
            float radiusSqr = radius * radius;

            foreach (GameObject item in Items)
            {
                if (item == null) continue;
                
                float distanceSqr = (item.transform.position - position).sqrMagnitude;
                if (distanceSqr <= radiusSqr)
                {
                    result.Add(item);
                }
            }

            return result;
        }
    }

    /// <summary>
    /// RuntimeSet for Transform references.
    /// </summary>
    [CreateAssetMenu(fileName = "TransformSet", menuName = "Capriccioso/Data/Runtime Set (Transform)")]
    public class TransformRuntimeSet : RuntimeSet<Transform>
    {
        /// <summary>
        /// Gets the closest Transform to a world position.
        /// </summary>
        public Transform GetClosest(Vector3 position)
        {
            Transform closest = null;
            float closestDistance = float.MaxValue;

            foreach (Transform item in Items)
            {
                if (item == null) continue;
                
                float distance = Vector3.Distance(item.position, position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = item;
                }
            }

            return closest;
        }
    }

    /// <summary>
    /// RuntimeSet for ScriptableObjects.
    /// </summary>
    [CreateAssetMenu(fileName = "ScriptableObjectSet", menuName = "Capriccioso/Data/Runtime Set (ScriptableObject)")]
    public class ScriptableObjectRuntimeSet : RuntimeSet<ScriptableObject> { }
}
