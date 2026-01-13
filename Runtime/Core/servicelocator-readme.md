# ServiceLocator

A simple service locator pattern implementation for decoupled dependency management. Register services by interface and retrieve them from anywhere without direct references.

## When to Use

- **Interface-based design** — Code against interfaces, not implementations
- **Swappable services** — Easy to swap real/mock implementations
- **Testing** — Register mock services for unit tests
- **Decoupling** — Systems don't need direct references to each other

---

## Quick Start

### 1. Define an Interface

```csharp
public interface IAudioService
{
    void PlaySound(string soundId);
    void PlayMusic(string musicId);
    void StopMusic();
    void SetVolume(float volume);
}
```

### 2. Implement the Interface

```csharp
using UnityEngine;

public class AudioService : MonoBehaviour, IAudioService
{
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;
    
    public void PlaySound(string soundId)
    {
        var clip = Resources.Load<AudioClip>($"Sounds/{soundId}");
        _sfxSource.PlayOneShot(clip);
    }
    
    public void PlayMusic(string musicId)
    {
        var clip = Resources.Load<AudioClip>($"Music/{musicId}");
        _musicSource.clip = clip;
        _musicSource.Play();
    }
    
    public void StopMusic() => _musicSource.Stop();
    
    public void SetVolume(float volume)
    {
        _musicSource.volume = volume;
        _sfxSource.volume = volume;
    }
}
```

### 3. Register the Service

```csharp
using Capriccioso;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private AudioService _audioService;
    
    private void Awake()
    {
        ServiceLocator.Register<IAudioService>(_audioService);
    }
}
```

### 4. Use the Service

```csharp
using Capriccioso;

public class Enemy : MonoBehaviour
{
    public void Die()
    {
        // Get service by interface
        var audio = ServiceLocator.Get<IAudioService>();
        audio.PlaySound("enemy_death");
        
        Destroy(gameObject);
    }
}
```

---

## API Reference

### Register

```csharp
// Basic registration
ServiceLocator.Register<IAudioService>(audioService);

// Overwrite existing registration
ServiceLocator.Register<IAudioService>(newAudioService, overwrite: true);
```

### Get

```csharp
// Get service (throws if not registered)
IAudioService audio = ServiceLocator.Get<IAudioService>();

// Safe get with TryGet
if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
{
    analytics.LogEvent("player_died");
}

// Check if registered
if (ServiceLocator.IsRegistered<IAudioService>())
{
    // Service is available
}
```

### Unregister

```csharp
// Remove a specific service
ServiceLocator.Unregister<IAudioService>();

// Clear all services
ServiceLocator.Clear();

// Check service count
int count = ServiceLocator.ServiceCount;
```

---

## With MonoSingleton

Combine ServiceLocator with MonoSingleton for the best of both worlds:

```csharp
using Capriccioso;

public class GameManager : MonoSingleton<GameManager>, IGameManager
{
    public GameState CurrentState { get; private set; }
    
    protected override bool Awake()
    {
        if (!base.Awake()) return false;
        
        // Register as interface for consumers
        ServiceLocator.Register<IGameManager>(this);
        
        return true;
    }
    
    protected override void OnDestroy()
    {
        ServiceLocator.Unregister<IGameManager>();
        base.OnDestroy();
    }
}

// Consumers use interface
public class PauseMenu : MonoBehaviour
{
    public void Resume()
    {
        var gameManager = ServiceLocator.Get<IGameManager>();
        gameManager.ResumeGame();
    }
}
```

---

## Testing with Mock Services

```csharp
// Mock implementation for tests
public class MockAudioService : IAudioService
{
    public List<string> PlayedSounds { get; } = new();
    public string CurrentMusic { get; private set; }
    
    public void PlaySound(string soundId) => PlayedSounds.Add(soundId);
    public void PlayMusic(string musicId) => CurrentMusic = musicId;
    public void StopMusic() => CurrentMusic = null;
    public void SetVolume(float volume) { }
}

// In your test
[Test]
public void Enemy_OnDie_PlaysDeathSound()
{
    // Arrange
    var mockAudio = new MockAudioService();
    ServiceLocator.Register<IAudioService>(mockAudio, overwrite: true);
    var enemy = new GameObject().AddComponent<Enemy>();
    
    // Act
    enemy.Die();
    
    // Assert
    Assert.Contains("enemy_death", mockAudio.PlayedSounds);
}
```

---

## Complete Example: Multi-Service Architecture

```csharp
// ============================================
// Service Interfaces
// ============================================
public interface IAudioService
{
    void PlaySound(string id);
    void PlayMusic(string id);
}

public interface ISaveService
{
    void Save(string key, object data);
    T Load<T>(string key);
}

public interface IAnalyticsService
{
    void LogEvent(string eventName, Dictionary<string, object> parameters = null);
}

// ============================================
// Bootstrap - Register all services
// ============================================
public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private AudioService _audioService;
    [SerializeField] private SaveService _saveService;
    [SerializeField] private AnalyticsService _analyticsService;
    
    private void Awake()
    {
        // Register all services
        ServiceLocator.Register<IAudioService>(_audioService);
        ServiceLocator.Register<ISaveService>(_saveService);
        ServiceLocator.Register<IAnalyticsService>(_analyticsService);
        
        CLogger.LogSuccess("All services registered");
    }
    
    private void OnDestroy()
    {
        ServiceLocator.Clear();
    }
}

// ============================================
// Consumer - Uses services without references
// ============================================
public class Player : MonoBehaviour
{
    private int _score;
    
    public void CollectCoin()
    {
        _score += 10;
        
        // Play sound
        ServiceLocator.Get<IAudioService>().PlaySound("coin_collect");
        
        // Track analytics
        ServiceLocator.Get<IAnalyticsService>().LogEvent("coin_collected", new()
        {
            { "total_score", _score }
        });
    }
    
    public void SaveProgress()
    {
        ServiceLocator.Get<ISaveService>().Save("player_score", _score);
    }
    
    public void LoadProgress()
    {
        _score = ServiceLocator.Get<ISaveService>().Load<int>("player_score");
    }
}
```

---

## Best Practices

### ✅ Do

```csharp
// Use interfaces for abstraction
ServiceLocator.Register<IAudioService>(audioService);

// Use TryGet when service is optional
if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
{
    analytics.LogEvent("optional_event");
}

// Register in Bootstrap/Awake, use in Start or later
private void Start()
{
    var audio = ServiceLocator.Get<IAudioService>();
}
```

### ❌ Don't

```csharp
// DON'T: Register concrete types (loses abstraction benefit)
ServiceLocator.Register<AudioService>(audioService);  // Bad

// DON'T: Get services in Awake (may not be registered yet)
private void Awake()
{
    _audio = ServiceLocator.Get<IAudioService>();  // May fail!
}

// DON'T: Forget to unregister MonoBehaviour services
// They become stale references after destruction
```

---

## ServiceLocator vs MonoSingleton

| Feature | ServiceLocator | MonoSingleton |
|---------|---------------|---------------|
| Interface-based | ✅ Yes | ❌ Concrete only |
| Testability | ✅ Easy mocking | ⚠️ Harder |
| Unity Lifecycle | ⚠️ Must wrap | ✅ Full access |
| Lazy Creation | ❌ Must register | ✅ Automatic |
| Inspector Config | ❌ No | ✅ Yes |
| Registration | Manual | Automatic |

### Recommendation

- **Use ServiceLocator** for services that need interface abstraction and testability
- **Use MonoSingleton** for Unity-centric managers with inspector configuration
- **Combine both** by registering MonoSingletons as interfaces

---

## Thread Safety

ServiceLocator is thread-safe. All operations are protected by locks:

```csharp
// Safe to call from any thread
ServiceLocator.Register<IService>(service);
ServiceLocator.Get<IService>();
ServiceLocator.TryGet<IService>(out var s);
```
