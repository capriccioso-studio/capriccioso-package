# Systems Reference

Consolidated documentation for all Capriccioso package systems.

---

## Table of Contents

- [MonoSingleton](#monosingleton)
- [ServiceLocator](#servicelocator)
- [EventBus](#eventbus)
- [BrokerChain Pipeline](#brokerchain-pipeline)
- [Object Pooling](#object-pooling)
- [Timing (Timer & Cooldown)](#timing)
- [Bootstrap Loader](#bootstrap-loader)
- [Extensions](#extensions)
- [CLogger](#clogger)

---

## MonoSingleton

Thread-safe singleton base class for MonoBehaviours.

### When to Use
- Global managers (AudioManager, GameManager)
- Persistent services that survive scene loads
- Unity-dependent singletons needing Update(), coroutines, or inspector config

### Basic Usage

```csharp
using Capriccioso.Runtime.Core;

public class AudioManager : MonoSingleton<AudioManager>
{
    [SerializeField] private AudioSource _musicSource;
    
    protected override bool Awake()
    {
        if (!base.Awake()) return false;
        // Your initialization
        return true;
    }
    
    public void PlayMusic(AudioClip clip)
    {
        _musicSource.clip = clip;
        _musicSource.Play();
    }
}

// Access from anywhere
AudioManager.Instance.PlayMusic(menuMusic);

// Check existence without creating
if (AudioManager.HasInstance)
{
    AudioManager.Instance.PlayMusic(menuMusic);
}
```

### Key Features
- **Thread-safe** — Protected by locks
- **DontDestroyOnLoad** — Persists across scenes
- **Domain reload safe** — Static fields properly reset
- **Duplicate prevention** — Only first instance survives
- **Lazy initialization** — Created on first access if none exists

---

## ServiceLocator

Interface-based service registration without frameworks.

### When to Use
- Code against interfaces, not implementations
- Swap real/mock implementations for testing
- Decouple systems from direct references

### Basic Usage

```csharp
using Capriccioso.Runtime.Core;

// 1. Define interface
public interface IAudioService
{
    void PlaySound(string soundId);
    void SetVolume(float volume);
}

// 2. Implement
public class AudioService : MonoBehaviour, IAudioService
{
    public void PlaySound(string soundId) { /* ... */ }
    public void SetVolume(float volume) { /* ... */ }
}

// 3. Register (in Bootstrap)
ServiceLocator.Register<IAudioService>(audioService);

// 4. Use anywhere
var audio = ServiceLocator.Get<IAudioService>();
audio.PlaySound("explosion");

// Safe retrieval
if (ServiceLocator.TryGet<IAnalytics>(out var analytics))
{
    analytics.LogEvent("player_died");
}
```

### API

| Method | Description |
|--------|-------------|
| `Register<T>(service)` | Register service by interface |
| `Register<T>(service, overwrite: true)` | Overwrite existing |
| `Get<T>()` | Get service (throws if not found) |
| `TryGet<T>(out service)` | Safe retrieval |
| `IsRegistered<T>()` | Check if registered |
| `Unregister<T>()` | Remove service |
| `Clear()` | Remove all services |

---

## EventBus

Decoupled publish/subscribe messaging.

### When to Use
- Cross-system communication without coupling
- Global state changes (player death, level complete)
- UI updates, audio triggers, analytics

### Type-Safe Events (Recommended)

```csharp
using Capriccioso.Runtime.Events;

// 1. Define event struct
public struct PlayerDiedEvent
{
    public Vector3 Position;
    public string Cause;
}

// 2. Subscribe with auto-dispose
public class GameOverUI : MonoBehaviour
{
    private EventSubscription _subscription;
    
    private void Start()
    {
        _subscription = EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
    }
    
    private void OnDestroy()
    {
        _subscription?.Dispose();
    }
    
    private void OnPlayerDied(PlayerDiedEvent e)
    {
        ShowGameOver(e.Cause);
    }
}

// 3. Publish
EventBus.Publish(new PlayerDiedEvent 
{ 
    Position = transform.position, 
    Cause = "Fell off map" 
});
```

### Enum-Based Events

```csharp
public enum GameEventType { PlayerSpawned, PlayerDied, LevelComplete }

// Subscribe
_sub = EventBus.Subscribe(GameEventType.PlayerDied, OnPlayerDied);

// Publish
EventBus.Publish(GameEventType.PlayerDied, deathData);
```

### Key Features
- **Auto-dispose** — `EventSubscription` for automatic cleanup
- **Exception-safe** — Failed handlers don't crash others
- **Thread-safe** — Safe for multi-threaded use

---

## BrokerChain Pipeline

Chain of Responsibility pattern for middleware-style pipelines.

### When to Use
- Buff/debuff systems
- Damage calculation (armor, resistances, crits)
- Input processing & validation
- Request/response pipelines

### Basic Usage

```csharp
using Capriccioso.Runtime.Pipeline;

// 1. Define context
public class DamageContext : IBrokerContext
{
    public bool IsHandled { get; set; }
    public bool IsCancelled { get; set; }
    
    public float BaseDamage { get; set; }
    public float FinalDamage { get; set; }
    public bool IsCritical { get; set; }
}

// 2. Create handlers
public class ArmorHandler : IBrokerHandler<DamageContext>
{
    public int Priority => 10; // Lower = executes first
    
    public void Handle(DamageContext ctx, Action<DamageContext> next)
    {
        ctx.FinalDamage = ctx.BaseDamage * 0.8f; // 20% reduction
        next(ctx); // Continue chain
    }
}

public class CriticalHandler : IBrokerHandler<DamageContext>
{
    public int Priority => 20;
    
    public void Handle(DamageContext ctx, Action<DamageContext> next)
    {
        if (ctx.IsCritical)
            ctx.FinalDamage *= 2f;
        next(ctx);
    }
}

// 3. Build and use chain
var chain = new BrokerChainBuilder<DamageContext>()
    .AddHandler(new ArmorHandler())
    .AddHandler(new CriticalHandler())
    .Build();

var ctx = new DamageContext { BaseDamage = 100, IsCritical = true };
chain.Process(ctx);
// Result: FinalDamage = 160 (100 * 0.8 * 2)
```

### Async Variant

```csharp
var asyncChain = new BrokerChainBuilder<DamageContext>()
    .AddAsyncHandler(new AsyncArmorHandler())
    .BuildAsync();

await asyncChain.ProcessAsync(ctx);
```

### Components

| Component | Description |
|-----------|-------------|
| `IBrokerContext` | Interface for chain data |
| `IBrokerHandler<T>` | Handler with Priority and Handle() |
| `BrokerChain<T>` | Synchronous chain execution |
| `AsyncBrokerChain<T>` | Async/await variant |
| `BrokerChainBuilder<T>` | Fluent builder |
| `BrokerChainRegistry` | Global chain storage |

---

## Object Pooling

Efficient reuse of frequently instantiated objects.

### When to Use
- Projectiles (bullets, arrows, spells)
- Particles and VFX
- Enemies and spawned entities
- UI elements (damage numbers, list items)

### Basic Usage

```csharp
using Capriccioso.Runtime.Pooling;

// 1. Implement IPoolable (optional but recommended)
public class Bullet : MonoBehaviour, IPoolable
{
    public void OnSpawnFromPool()
    {
        // Reset state
        _rb.linearVelocity = Vector3.zero;
        _trail.Clear();
    }
    
    public void OnReturnToPool()
    {
        // Cleanup
        _rb.linearVelocity = Vector3.zero;
    }
}

// 2. Create pool
public class BulletManager : MonoSingleton<BulletManager>
{
    [SerializeField] private GameObject _bulletPrefab;
    private GameObjectPool _pool;
    
    protected override bool Awake()
    {
        if (!base.Awake()) return false;
        _pool = new GameObjectPool(
            createFunc: () => Instantiate(_bulletPrefab),
            defaultCapacity: 20,
            maxSize: 100
        );
        return true;
    }
    
    public GameObject GetBullet() => _pool.Get();
    public void ReturnBullet(GameObject b) => _pool.Release(b);
}
```

---

## Timing

Lightweight utilities for time-based mechanics.

### Timer (Countdown)

```csharp
using Capriccioso.Runtime.Timing;

// Basic timer
var timer = new Timer(5f);
timer.OnTimerComplete += () => Explode();
timer.Start();

// In Update
timer.Tick(Time.deltaTime);

// Properties
float remaining = timer.RemainingTime;
float progress = timer.Progress; // 0 to 1
bool running = timer.IsRunning;

// Control
timer.Pause();
timer.Resume();
timer.Reset(10f); // New duration
```

### One-Shot Timer (Static)

```csharp
// Fire and forget - no Update needed
Timer.Create(3f, () => SpawnEnemy());

// With progress callback
Timer.Create(5f, 
    onComplete: () => ShowReward(),
    onTick: remaining => progressBar.fillAmount = 1f - (remaining / 5f)
);
```

### Cooldown (Ability Rate Limiting)

```csharp
using Capriccioso.Runtime.Timing;

var dashCooldown = new Cooldown(2f);

// In Update
dashCooldown.Tick(Time.deltaTime);

// Check and use
if (dashCooldown.IsReady)
{
    PerformDash();
    dashCooldown.Use();
}

// Properties
float remaining = dashCooldown.RemainingTime;
float progress = dashCooldown.Progress; // 0 (ready) to 1 (just used)
```

---

## Bootstrap Loader

Scene-agnostic initialization for rapid iteration.

### Why Use
Without bootstrap, you must always start from a specific scene. With bootstrap, press Play from any scene and all managers initialize automatically.

### Setup

1. Create `Bootstrap` scene with persistent managers
2. Add scene to Build Settings
3. Create `BootstrapSettings` asset in Resources (optional)

### Configuration

| Field | Description | Default |
|-------|-------------|---------|
| Enabled | Enable/disable system | `true` |
| Bootstrap Scene Name | Scene to load | `"Bootstrap"` |
| Excluded Scenes | Scenes that skip bootstrap | Empty |

### How It Works

```
Game Start → RuntimeInitializeOnLoadMethod
    ↓
Bootstrap not loaded? → Load additively
    ↓
Original scene continues
    ↓
All services now available
```

---

## Extensions

Convenient extension methods for common Unity types.

### VectorExtensions

```csharp
using Capriccioso.Runtime.Extensions;

// Component modification
transform.position = pos.WithX(0f);
transform.position = pos.With(x: 0f, z: 10f);

// Ground calculations
Vector3 flatDir = toTarget.Flat(); // Y = 0
float groundDist = toTarget.FlatMagnitude();

// Random
Vector3 spawn = basePos.WithRandomOffset(2f);
Vector3 point = center.RandomPointInRadius(10f);

// Conversion
Vector3 movement = input2D.ToVector3XZ();
Vector2 screenPos = worldPos.ToVector2XY();

// Utility
Vector3 clamped = velocity.Clamp(-max, max);
Vector3 grid = pos.Round();
Vector2 rotated = dir.Rotate(45f);
```

### TransformExtensions

```csharp
// Child management
container.DestroyChildren();
container.DestroyChildrenImmediate();

// Position setters
transform.SetPositionX(0f);
transform.SetLocalPositionY(5f);

// Scale
transform.SetLocalScaleUniform(2f);

// Reset
transform.ResetLocal();
```

### CollectionExtensions

```csharp
// Random access
var item = list.GetRandom();

// Shuffle (Fisher-Yates)
list.Shuffle();

// Safe access
var safeItem = list.GetOrDefault(index, fallback);
```

---

## CLogger

Colored console logging with server support.

### Log Methods

```csharp
using Capriccioso.Runtime.Logging;

CLogger.LogInfo("General information");
CLogger.LogSuccess("Operation completed");
CLogger.LogWarning("Something unusual");
CLogger.LogError("Something broke");
CLogger.LogEvent("Game event occurred");
CLogger.LogMajorAction("Important milestone");
```

### Features
- **Colored output** — Different colors per log level
- **Server support** — ANSI colors for headless builds
- **Caller info** — Automatic method/line tracking
- **File logging** — Optional log file in batch mode

---

## See Also

- [ARCHITECTURE.md](ARCHITECTURE.md) — Layer hierarchy & access rules
- [CODE_STANDARDS.md](CODE_STANDARDS.md) — Style guidelines
- [README.md](../README.md) — Quick Start guide
