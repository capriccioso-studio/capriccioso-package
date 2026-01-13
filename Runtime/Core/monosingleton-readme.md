# MonoSingleton

A thread-safe singleton base class for MonoBehaviours. Use this for services that need a single global instance with Unity lifecycle support.

## When to Use

- **Global Managers** — AudioManager, GameManager, InputManager
- **Persistent Services** — Services that survive scene loads
- **Unity-dependent singletons** — When you need Update(), coroutines, or inspector configuration

---

## Basic Usage

### 1. Create a Singleton Service

```csharp
using Capriccioso;
using UnityEngine;

public class AudioManager : MonoSingleton<AudioManager>
{
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;
    
    protected override bool Awake()
    {
        // Always call base.Awake() first!
        if (!base.Awake()) return false;
        
        // Your initialization here
        InitializeAudioSources();
        
        return true;
    }
    
    public void PlaySound(AudioClip clip)
    {
        _sfxSource.PlayOneShot(clip);
    }
    
    public void PlayMusic(AudioClip music)
    {
        _musicSource.clip = music;
        _musicSource.Play();
    }
}
```

### 2. Access From Anywhere

```csharp
// Direct access
AudioManager.Instance.PlaySound(explosionClip);

// Check if instance exists (doesn't create one)
if (AudioManager.HasInstance)
{
    AudioManager.Instance.PlayMusic(menuMusic);
}
```

---

## Key Features

### Thread-Safe

The singleton instance is protected by locks, making it safe to access from multiple threads.

### DontDestroyOnLoad

Singleton instances are automatically marked as `DontDestroyOnLoad`, persisting across scene loads.

### Domain Reload Safe

Static fields are properly reset when exiting play mode or when domain reloads occur:

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
private static void ResetStatics()
{
    s_applicationQuitting = false;
    s_instance = null;
}
```

### Duplicate Prevention

If multiple instances exist in a scene, only the first is kept:

```csharp
// In your derived class Awake:
protected override bool Awake()
{
    if (!base.Awake()) return false;  // Returns false if duplicate
    
    // Your init code here - only runs on the surviving instance
    return true;
}
```

### Lazy Initialization

If no instance exists when `Instance` is accessed, one is created automatically:

```csharp
// Even if no AudioManager exists in the scene, this works:
AudioManager.Instance.PlaySound(clip);  // Creates [AudioManager] GameObject
```

---

## Best Practices

### ✅ Do

```csharp
public class GameManager : MonoSingleton<GameManager>
{
    // 1. Always check base.Awake() return value
    protected override bool Awake()
    {
        if (!base.Awake()) return false;
        
        // Initialize
        return true;
    }
    
    // 2. Use HasInstance for optional access
    public void TryPlayWinSound()
    {
        if (AudioManager.HasInstance)
        {
            AudioManager.Instance.PlayWinSound();
        }
    }
}
```

### ❌ Don't

```csharp
public class BadSingleton : MonoSingleton<BadSingleton>
{
    // DON'T: Forget to call base.Awake()
    protected override bool Awake()
    {
        Initialize();  // Wrong! Call base.Awake() first!
        return true;
    }
    
    // DON'T: Access Instance during OnDestroy
    protected override void OnDestroy()
    {
        // May cause issues during application quit
        OtherSingleton.Instance.DoSomething();  // Risky!
        base.OnDestroy();
    }
}
```

---

## Purging

Manually destroy and reset a singleton:

```csharp
// Removes the instance completely
GameManager.Instance.Purge();

// After purge, accessing Instance creates a new one
var newInstance = GameManager.Instance;
```

---

## Complete Example

```csharp
using Capriccioso;
using UnityEngine;
using System.Collections.Generic;

public class ScoreManager : MonoSingleton<ScoreManager>
{
    public int CurrentScore { get; private set; }
    public int HighScore { get; private set; }
    
    private List<EventSubscription> _subscriptions = new();
    
    protected override bool Awake()
    {
        if (!base.Awake()) return false;
        
        // Load saved high score
        HighScore = PlayerPrefs.GetInt("HighScore", 0);
        
        // Subscribe to events
        _subscriptions.Add(EventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled));
        _subscriptions.Add(EventBus.Subscribe<CoinCollectedEvent>(OnCoinCollected));
        
        return true;
    }
    
    protected override void OnDestroy()
    {
        // Clean up subscriptions
        foreach (var sub in _subscriptions)
        {
            sub?.Dispose();
        }
        
        // Save high score
        if (CurrentScore > HighScore)
        {
            PlayerPrefs.SetInt("HighScore", CurrentScore);
        }
        
        base.OnDestroy();
    }
    
    private void OnEnemyKilled(EnemyKilledEvent e)
    {
        AddScore(e.Points);
    }
    
    private void OnCoinCollected(CoinCollectedEvent e)
    {
        AddScore(e.Value);
    }
    
    public void AddScore(int points)
    {
        CurrentScore += points;
        
        if (CurrentScore > HighScore)
        {
            HighScore = CurrentScore;
        }
        
        EventBus.Publish(new ScoreChangedEvent
        {
            NewScore = CurrentScore,
            HighScore = HighScore
        });
    }
    
    public void ResetScore()
    {
        CurrentScore = 0;
    }
}

// Usage from anywhere
public class Enemy : MonoBehaviour
{
    [SerializeField] private int _pointValue = 100;
    
    public void Die()
    {
        ScoreManager.Instance.AddScore(_pointValue);
        Destroy(gameObject);
    }
}
```

---

## MonoSingleton vs ServiceLocator

| Feature | MonoSingleton | ServiceLocator |
|---------|---------------|----------------|
| Unity Integration | ✅ Full (Update, Coroutines) | ⚠️ Indirect |
| Interface-based | ❌ Concrete types | ✅ Interface types |
| Testability | ⚠️ Harder to mock | ✅ Easy to swap |
| Lazy Creation | ✅ Automatic | ❌ Must register first |
| Inspector Config | ✅ Yes | ❌ No |

**Use MonoSingleton when:**
- You need Unity lifecycle (Update, Coroutines)
- You want inspector configuration
- You need a simple, concrete singleton

**Use ServiceLocator when:**
- You want interface-based design
- You need to swap implementations
- You want better testability
