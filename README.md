# Capriccioso Starter Package

![image](https://github.com/user-attachments/assets/f2da4e37-3c2e-4944-b689-9cb6ea2d2e43)

A comprehensive Unity starter package providing core systems for game development.

## Features

- **MonoSingleton** — Thread-safe singleton base class for MonoBehaviours
- **ServiceLocator** — Interface-based dependency injection
- **EventBus** — Decoupled pub/sub messaging with auto-dispose
- **BrokerChain** — Chain of Responsibility pattern for pipelines
- **ObjectPool** — GameObject and object pooling with IPoolable
- **Timer/Cooldown** — Lightweight timing utilities
- **Extensions** — Vector, Transform, Collection helpers
- **CLogger** — Colored console logging

## Installation

1. Download or clone this repository
2. Copy to your Unity project's `Packages/` folder
3. Or add via Package Manager using the git URL

## Quick Start

### 1. Create a Bootstrap Scene

Create a new scene called `Bootstrap` and set it as the first scene in Build Settings. Add a `BootstrapLoader` component to an empty GameObject:

```csharp
using Capriccioso.Runtime.Bootstrap;
using Capriccioso.Runtime.Core;
using Capriccioso.Runtime.Events;
using Capriccioso.Runtime.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootstrapper : MonoBehaviour
{
    [SerializeField] private string _firstGameScene = "MainMenu";
    
    private void Awake()
    {
        CLogger.LogMajorAction("Game Starting...");
        
        // Register core services
        RegisterServices();
        
        // Subscribe to global events
        SubscribeToEvents();
        
        // Load the first game scene
        SceneManager.LoadScene(_firstGameScene);
    }
    
    private void RegisterServices()
    {
        // Register interface-based services with ServiceLocator
        ServiceLocator.Register<IAudioService>(new AudioService());
        ServiceLocator.Register<ISaveService>(new SaveService());
        
        CLogger.LogSuccess("Services registered");
    }
    
    private void SubscribeToEvents()
    {
        // Use auto-dispose pattern for safe event handling
        EventBus.Subscribe<PlayerDeathEvent>(OnPlayerDeath);
        EventBus.Subscribe<LevelCompleteEvent>(OnLevelComplete);
        
        CLogger.LogSuccess("Events subscribed");
    }
    
    private void OnPlayerDeath(PlayerDeathEvent evt)
    {
        CLogger.LogEvent($"Player died at position {evt.Position}");
    }
    
    private void OnLevelComplete(LevelCompleteEvent evt)
    {
        CLogger.LogSuccess($"Level {evt.LevelIndex} complete!");
    }
}
```

### 2. Create a Service (MonoSingleton Pattern)

```csharp
using Capriccioso.Runtime.Core;
using Capriccioso.Runtime.Logging;

public class GameManager : MonoSingleton<GameManager>
{
    public int Score { get; private set; }
    
    protected override void Awake()
    {
        base.Awake();
        CLogger.LogInfo("GameManager initialized");
    }
    
    public void AddScore(int points)
    {
        Score += points;
        EventBus.Publish(new ScoreChangedEvent(Score));
    }
}
```

### 3. Define Events

```csharp
using Capriccioso.Runtime.Events;
using UnityEngine;

public struct PlayerDeathEvent : IEvent
{
    public Vector3 Position { get; }
    
    public PlayerDeathEvent(Vector3 position)
    {
        Position = position;
    }
}

public struct ScoreChangedEvent : IEvent
{
    public int NewScore { get; }
    
    public ScoreChangedEvent(int score)
    {
        NewScore = score;
    }
}
```

### 4. Use Object Pooling

```csharp
using Capriccioso.Runtime.Pooling;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _bulletPrefab;
    private GameObjectPool _bulletPool;
    
    private void Start()
    {
        _bulletPool = new GameObjectPool(_bulletPrefab, initialSize: 20);
    }
    
    public void FireBullet(Vector3 position, Vector3 direction)
    {
        GameObject bullet = _bulletPool.Get();
        bullet.transform.position = position;
        bullet.GetComponent<Bullet>().Fire(direction);
    }
    
    public void ReturnBullet(GameObject bullet)
    {
        _bulletPool.Release(bullet);
    }
}
```

### 5. Use Timers and Cooldowns

```csharp
using Capriccioso.Runtime.Timing;
using UnityEngine;

public class AbilitySystem : MonoBehaviour
{
    private Cooldown _dashCooldown;
    private Timer _buffTimer;
    
    private void Start()
    {
        _dashCooldown = new Cooldown(2f);
        _buffTimer = new Timer(10f, onComplete: () => RemoveBuff());
    }
    
    private void Update()
    {
        _dashCooldown.Tick(Time.deltaTime);
        _buffTimer.Tick(Time.deltaTime);
    }
    
    public void TryDash()
    {
        if (_dashCooldown.IsReady)
        {
            PerformDash();
            _dashCooldown.Use();
        }
    }
    
    public void ApplyBuff()
    {
        _buffTimer.Start();
    }
}
```

---

## Package Systems

| System | Namespace | Description |
|--------|-----------|-------------|
| **MonoSingleton** | `Capriccioso.Runtime.Core` | Thread-safe singleton base class |
| **ServiceLocator** | `Capriccioso.Runtime.Core` | Interface-based dependency injection |
| **EventBus** | `Capriccioso.Runtime.Events` | Decoupled pub/sub messaging with auto-dispose |
| **GameEvent** | `Capriccioso.Runtime.Events` | ScriptableObject-based events |
| **ObjectPool** | `Capriccioso.Runtime.Pooling` | GameObject and object pooling with IPoolable |
| **Timer/Cooldown** | `Capriccioso.Runtime.Timing` | Lightweight timing utilities |
| **BrokerChain** | `Capriccioso.Runtime.Pipeline` | Chain of Responsibility pattern |
| **CLogger** | `Capriccioso.Runtime.Logging` | Colored console logging |
| **Extensions** | `Capriccioso.Runtime.Extensions` | Vector, Transform, Collection helpers |

---

## Documentation

| Document | Description |
|----------|-------------|
| [Architecture](Documentation~/ARCHITECTURE.md) | Layer hierarchy & access rules |
| [Systems Reference](Documentation~/SYSTEMS.md) | Detailed documentation for all systems |
| [Code Standards](Documentation~/CODE_STANDARDS.md) | Style guidelines & conventions |
| [Contributing](Documentation~/CONTRIBUTING.md) | Branch workflow & versioning |
| [Changelog](CHANGELOG.md) | Version history |

---

## Support

For issues, questions, or contributions, please refer to the [Contributing Guide](Documentation~/CONTRIBUTING.md).
