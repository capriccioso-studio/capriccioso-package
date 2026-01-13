# Object Pooling System

The Object Pooling system provides efficient reuse of frequently instantiated objects like bullets, particles, enemies, and UI elements. It wraps Unity's built-in `UnityEngine.Pool.ObjectPool` with a simplified API and automatic `IPoolable` lifecycle callbacks.

## When to Use

- **Projectiles** — Bullets, arrows, spells that spawn and despawn frequently
- **Particles** — Custom particle effects not using ParticleSystem pooling
- **Enemies** — Spawning waves of enemies
- **UI Elements** — Dynamic list items, damage numbers, notifications
- **Audio Sources** — One-shot sound effects

---

## Core Components

| Component | Description |
|-----------|-------------|
| `IPoolable` | Interface for objects that need spawn/return callbacks |
| `ObjectPool<T>` | Generic pool for any class type |
| `GameObjectPool` | Specialized pool for GameObjects with automatic `IPoolable` support |

---

## IPoolable Interface

Implement `IPoolable` on your pooled components to receive lifecycle callbacks:

```csharp
using Capriccioso;
using UnityEngine;

public class Bullet : MonoBehaviour, IPoolable
{
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _lifetime = 3f;
    
    private Rigidbody _rb;
    private TrailRenderer _trail;
    private float _spawnTime;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _trail = GetComponent<TrailRenderer>();
    }
    
    /// <summary>
    /// Called when spawned from the pool.
    /// Reset all state here.
    /// </summary>
    public void OnSpawnFromPool()
    {
        // Reset physics
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        
        // Clear visual trails
        _trail.Clear();
        
        // Reset spawn time for lifetime tracking
        _spawnTime = Time.time;
    }
    
    /// <summary>
    /// Called when returned to the pool.
    /// Clean up and prepare for reuse.
    /// </summary>
    public void OnReturnToPool()
    {
        // Stop any movement
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        
        // Clear trails so they don't show on next spawn
        _trail.Clear();
    }
    
    /// <summary>
    /// Initialize the bullet with direction. Call this after spawning.
    /// </summary>
    public void Initialize(Vector3 direction, float speedMultiplier = 1f)
    {
        _rb.linearVelocity = direction.normalized * _speed * speedMultiplier;
    }
    
    private void Update()
    {
        // Auto-return to pool after lifetime expires
        if (Time.time - _spawnTime > _lifetime)
        {
            // The pool manager will call OnReturnToPool automatically
            BulletPoolManager.Instance.ReturnBullet(gameObject);
        }
    }
}
```

---

## GameObjectPool Usage

### Basic Setup

```csharp
using Capriccioso;
using UnityEngine;

public class BulletPoolManager : MonoSingleton<BulletPoolManager>
{
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private int _initialPoolSize = 20;
    [SerializeField] private int _maxPoolSize = 100;
    
    private GameObjectPool _pool;
    
    protected override bool Awake()
    {
        if (!base.Awake()) return false;
        
        _pool = new GameObjectPool(
            createFunc: CreateBullet,
            defaultCapacity: _initialPoolSize,
            maxSize: _maxPoolSize
        );
        
        return true;
    }
    
    private GameObject CreateBullet()
    {
        var bullet = Instantiate(_bulletPrefab, transform);
        bullet.SetActive(false); // Start inactive
        return bullet;
    }
    
    public GameObject GetBullet()
    {
        return _pool.Get();
    }
    
    public void ReturnBullet(GameObject bullet)
    {
        _pool.Release(bullet);
    }
    
    private void OnDestroy()
    {
        _pool?.Dispose();
    }
}
```

### Spawning with Initialization

```csharp
public class PlayerWeapon : MonoBehaviour
{
    [SerializeField] private Transform _muzzle;
    
    public void Fire()
    {
        // Get bullet from pool (OnSpawnFromPool is called automatically)
        var bulletGO = BulletPoolManager.Instance.GetBullet();
        
        // Position it
        bulletGO.transform.position = _muzzle.position;
        bulletGO.transform.rotation = _muzzle.rotation;
        
        // Initialize with additional data
        var bullet = bulletGO.GetComponent<Bullet>();
        bullet.Initialize(_muzzle.forward, speedMultiplier: 1.5f);
    }
}
```

---

## Generic ObjectPool Usage

For non-GameObject objects:

```csharp
using Capriccioso;

// Pooling data objects
public class DamageNumberPool
{
    private readonly ObjectPool<DamageNumber> _pool;
    
    public DamageNumberPool()
    {
        _pool = new ObjectPool<DamageNumber>(
            createFunc: () => new DamageNumber(),
            actionOnGet: dn => dn.Reset(),
            actionOnRelease: dn => dn.Clear()
        );
    }
    
    public DamageNumber Get() => _pool.Get();
    public void Release(DamageNumber dn) => _pool.Release(dn);
}

public class DamageNumber
{
    public int Value { get; set; }
    public Vector3 Position { get; set; }
    public Color Color { get; set; }
    public float Lifetime { get; set; }
    
    public void Reset()
    {
        Value = 0;
        Position = Vector3.zero;
        Color = Color.white;
        Lifetime = 1f;
    }
    
    public void Clear()
    {
        // Any cleanup before going back to pool
    }
}
```

---

## Using Statement Pattern

For temporary object usage with automatic return:

```csharp
using Capriccioso;
using UnityEngine.Pool;

public class RaycastHelper
{
    private readonly ObjectPool<List<RaycastHit>> _hitListPool;
    
    public RaycastHelper()
    {
        _hitListPool = new ObjectPool<List<RaycastHit>>(
            createFunc: () => new List<RaycastHit>(16),
            actionOnGet: list => list.Clear(),
            actionOnRelease: list => list.Clear()
        );
    }
    
    public void DoSphereCheck(Vector3 origin, float radius)
    {
        // Object is automatically returned when scope ends
        using (PooledObject<List<RaycastHit>> pooled = _hitListPool.Get(out var hits))
        {
            // Use hits list...
            Physics.SphereCastNonAlloc(origin, radius, Vector3.forward, hits.ToArray());
            
            foreach (var hit in hits)
            {
                ProcessHit(hit);
            }
        } // List is returned to pool here
    }
}
```

---

## Complete Enemy Spawner Example

```csharp
using Capriccioso;
using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private int _poolSize = 30;
    [SerializeField] private int _maxEnemies = 50;
    
    private GameObjectPool _enemyPool;
    private List<GameObject> _activeEnemies = new();
    
    private void Awake()
    {
        _enemyPool = new GameObjectPool(
            createFunc: () => {
                var enemy = Instantiate(_enemyPrefab, transform);
                enemy.GetComponent<Enemy>().OnDeath += HandleEnemyDeath;
                return enemy;
            },
            defaultCapacity: _poolSize,
            maxSize: _maxEnemies
        );
    }
    
    public void SpawnWave(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnEnemy();
        }
    }
    
    private void SpawnEnemy()
    {
        if (_activeEnemies.Count >= _maxEnemies) return;
        
        var spawnPoint = _spawnPoints.GetRandom();
        var enemyGO = _enemyPool.Get();
        
        enemyGO.transform.position = spawnPoint.position;
        enemyGO.transform.rotation = spawnPoint.rotation;
        
        _activeEnemies.Add(enemyGO);
    }
    
    private void HandleEnemyDeath(GameObject enemy)
    {
        _activeEnemies.Remove(enemy);
        _enemyPool.Release(enemy);
    }
    
    public void DespawnAll()
    {
        foreach (var enemy in _activeEnemies.ToArray())
        {
            _enemyPool.Release(enemy);
        }
        _activeEnemies.Clear();
    }
    
    private void OnDestroy()
    {
        _enemyPool?.Dispose();
    }
}

// Enemy component implementing IPoolable
public class Enemy : MonoBehaviour, IPoolable
{
    [SerializeField] private float _maxHealth = 100f;
    
    public event System.Action<GameObject> OnDeath;
    
    private float _currentHealth;
    private Animator _animator;
    private NavMeshAgent _agent;
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
    }
    
    public void OnSpawnFromPool()
    {
        // Reset health
        _currentHealth = _maxHealth;
        
        // Reset animator
        _animator.Rebind();
        _animator.Update(0f);
        
        // Enable AI
        _agent.enabled = true;
        _agent.isStopped = false;
        
        // Reset any other state
        GetComponent<Collider>().enabled = true;
    }
    
    public void OnReturnToPool()
    {
        // Disable AI
        _agent.isStopped = true;
        _agent.enabled = false;
        
        // Clear event listeners (except spawner's)
        // Be careful not to clear the spawner's OnDeath subscription
    }
    
    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        
        if (_currentHealth <= 0f)
        {
            Die();
        }
    }
    
    private void Die()
    {
        GetComponent<Collider>().enabled = false;
        OnDeath?.Invoke(gameObject);
    }
}
```

---

## Pool Statistics

```csharp
// Check pool state
int inactiveCount = _pool.CountInactive;
Debug.Log($"Available in pool: {inactiveCount}");

// Clear pool (useful for scene transitions)
_pool.Clear();
```

---

## Best Practices

### Do

- ✅ Implement `IPoolable` for automatic lifecycle management
- ✅ Reset ALL state in `OnSpawnFromPool` (transform, physics, visuals)
- ✅ Clear references in `OnReturnToPool` to prevent memory leaks
- ✅ Use `GetComponentsInChildren<IPoolable>` for nested poolable objects
- ✅ Pre-warm pools during loading screens
- ✅ Dispose pools when no longer needed

### Don't

- ❌ Destroy pooled objects directly — always return to pool
- ❌ Keep references to pooled objects after returning them
- ❌ Forget to reset state (common bug: bullets keeping old velocity)
- ❌ Create pools with `maxSize` smaller than typical usage
- ❌ Use `DontDestroyOnLoad` on pooled objects unless pool is also persistent

---

## Performance Tips

1. **Pre-warm during loading**: Call `Get()` and `Release()` in a loop during load screens
2. **Right-size your pool**: Monitor `CountInactive` to tune initial capacity
3. **Avoid GetComponent in hot paths**: Cache component references in Awake
4. **Consider multiple pools**: Separate pools for different enemy types, projectile types, etc.
