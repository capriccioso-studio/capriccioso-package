# Event System

The Event system provides decoupled communication between game systems without direct references. It includes a static `EventBus` for code-centric workflows and ScriptableObject-based `GameEvent` for designer-friendly inspector workflows.

## When to Use

- **Decoupled Communication** — Systems that shouldn't know about each other
- **Global State Changes** — Player death, level complete, score changes
- **UI Updates** — Health bars, score displays, notifications
- **Audio Triggers** — Sound effects based on game events
- **Analytics** — Track player actions without coupling

---

## EventBus

A static event bus supporting both enum-based and type-safe generic events.

### Key Features

- **Auto-dispose subscriptions** — `EventSubscription` class for automatic cleanup
- **Exception-safe** — Failed handlers don't crash other handlers
- **Custom error handling** — Optional `OnEventError` callback
- **Thread-safe** — Safe for multi-threaded use

---

## Type-Safe Events (Recommended)

Define events as structs and get compile-time type safety:

### 1. Define Event Structs

```csharp
// Events/GameEvents.cs
namespace MyGame.Events
{
    public struct PlayerSpawnedEvent
    {
        public GameObject Player;
        public Vector3 SpawnPosition;
    }
    
    public struct PlayerDiedEvent
    {
        public GameObject Player;
        public string DeathCause;
        public Vector3 DeathPosition;
    }
    
    public struct ScoreChangedEvent
    {
        public int OldScore;
        public int NewScore;
        public int Delta;
    }
    
    public struct HealthChangedEvent
    {
        public float OldHealth;
        public float NewHealth;
        public float MaxHealth;
        public float Percent => NewHealth / MaxHealth;
    }
}
```

### 2. Subscribe with Auto-Dispose (Recommended)

```csharp
using Capriccioso;
using MyGame.Events;
using UnityEngine;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image _healthFill;
    
    private EventSubscription _subscription;
    
    private void Start()
    {
        // Subscribe and store the subscription
        _subscription = EventBus.Subscribe<HealthChangedEvent>(OnHealthChanged);
    }
    
    private void OnDestroy()
    {
        // Automatically unsubscribes - safe to call even if null
        _subscription?.Dispose();
    }
    
    private void OnHealthChanged(HealthChangedEvent e)
    {
        _healthFill.fillAmount = e.Percent;
    }
}
```

### 3. Publish Events

```csharp
using Capriccioso;
using MyGame.Events;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 100f;
    private float _currentHealth;
    
    private void Start()
    {
        _currentHealth = _maxHealth;
    }
    
    public void TakeDamage(float damage)
    {
        float oldHealth = _currentHealth;
        _currentHealth = Mathf.Max(0f, _currentHealth - damage);
        
        // Publish health changed event
        EventBus.Publish(new HealthChangedEvent
        {
            OldHealth = oldHealth,
            NewHealth = _currentHealth,
            MaxHealth = _maxHealth
        });
        
        if (_currentHealth <= 0f)
        {
            Die();
        }
    }
    
    private void Die()
    {
        EventBus.Publish(new PlayerDiedEvent
        {
            Player = gameObject,
            DeathCause = "Damage",
            DeathPosition = transform.position
        });
    }
}
```

---

## Enum-Based Events

For simpler scenarios or backwards compatibility:

### 1. Define Event Enum

```csharp
public enum GameEventType
{
    PlayerSpawned,
    PlayerDied,
    LevelComplete,
    ScoreChanged,
    EnemyKilled
}
```

### 2. Subscribe and Publish

```csharp
using Capriccioso;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int _score;
    private EventSubscription _subscription;
    
    private void Start()
    {
        _subscription = EventBus.Subscribe(GameEventType.EnemyKilled, OnEnemyKilled);
    }
    
    private void OnDestroy()
    {
        _subscription?.Dispose();
    }
    
    private void OnEnemyKilled(object data)
    {
        // Cast the data to expected type
        if (data is EnemyKillData killData)
        {
            _score += killData.Points;
            EventBus.Publish(GameEventType.ScoreChanged, _score);
        }
    }
}

// Data class for the event
public class EnemyKillData
{
    public int Points;
    public Vector3 Position;
}

// Publishing
public class Enemy : MonoBehaviour
{
    [SerializeField] private int _points = 10;
    
    public void Die()
    {
        EventBus.Publish(GameEventType.EnemyKilled, new EnemyKillData
        {
            Points = _points,
            Position = transform.position
        });
        
        Destroy(gameObject);
    }
}
```

---

## Multiple Subscriptions

Manage multiple subscriptions cleanly:

```csharp
using Capriccioso;
using System.Collections.Generic;

public class GameUI : MonoBehaviour
{
    private readonly List<EventSubscription> _subscriptions = new();
    
    private void Start()
    {
        _subscriptions.Add(EventBus.Subscribe<HealthChangedEvent>(OnHealthChanged));
        _subscriptions.Add(EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged));
        _subscriptions.Add(EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied));
        _subscriptions.Add(EventBus.Subscribe(GameEventType.LevelComplete, OnLevelComplete));
    }
    
    private void OnDestroy()
    {
        // Dispose all subscriptions
        foreach (var sub in _subscriptions)
        {
            sub?.Dispose();
        }
        _subscriptions.Clear();
    }
    
    private void OnHealthChanged(HealthChangedEvent e) { /* ... */ }
    private void OnScoreChanged(ScoreChangedEvent e) { /* ... */ }
    private void OnPlayerDied(PlayerDiedEvent e) { /* ... */ }
    private void OnLevelComplete(object data) { /* ... */ }
}
```

---

## Error Handling

Events are exception-safe by default. If one handler throws, others still execute:

```csharp
// Handler 1 throws an exception
EventBus.Subscribe<PlayerDiedEvent>(e => throw new Exception("Oops!"));

// Handler 2 still executes
EventBus.Subscribe<PlayerDiedEvent>(e => Debug.Log("Player died!")); // ✅ Runs

// Publish - both handlers are called, first error is logged
EventBus.Publish(new PlayerDiedEvent { });
```

### Custom Error Callback

Set a custom error handler for analytics or custom logging:

```csharp
// Set up custom error handling (e.g., in a bootstrap script)
EventBus.OnEventError = (exception, eventType) =>
{
    // Log to analytics
    Analytics.LogError("EventBus", exception.Message, eventType.ToString());
    
    // Custom logging
    Debug.LogError($"[EventBus] Handler failed for {eventType}: {exception}");
};
```

---

## Manual Unsubscribe

If you need to unsubscribe without the subscription object:

```csharp
// Store the handler reference
private Action<HealthChangedEvent> _healthHandler;

private void Start()
{
    _healthHandler = OnHealthChanged;
    EventBus.Subscribe(_healthHandler);
}

private void OnDestroy()
{
    EventBus.Unsubscribe(_healthHandler);
}

private void OnHealthChanged(HealthChangedEvent e) { /* ... */ }
```

---

## Utility Methods

```csharp
// Get subscriber count (useful for debugging)
int healthSubscribers = EventBus.GetSubscriberCount<HealthChangedEvent>();
int deathSubscribers = EventBus.GetSubscriberCount(GameEventType.PlayerDied);

// Remove all subscribers for an event type
EventBus.UnsubscribeAll(GameEventType.EnemyKilled);

// Clear ALL subscriptions (use with caution!)
EventBus.Clear();
```

---

## Complete Example: Achievement System

```csharp
using Capriccioso;
using System.Collections.Generic;
using UnityEngine;

public struct AchievementUnlockedEvent
{
    public string AchievementId;
    public string Title;
    public string Description;
}

public class AchievementManager : MonoBehaviour
{
    private readonly List<EventSubscription> _subscriptions = new();
    private readonly HashSet<string> _unlockedAchievements = new();
    
    private int _killCount;
    private int _totalScore;
    
    private void Start()
    {
        _subscriptions.Add(EventBus.Subscribe(GameEventType.EnemyKilled, OnEnemyKilled));
        _subscriptions.Add(EventBus.Subscribe<ScoreChangedEvent>(OnScoreChanged));
        _subscriptions.Add(EventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied));
    }
    
    private void OnDestroy()
    {
        foreach (var sub in _subscriptions)
        {
            sub?.Dispose();
        }
    }
    
    private void OnEnemyKilled(object data)
    {
        _killCount++;
        
        if (_killCount >= 10)
            TryUnlock("killer_10", "Serial Killer", "Kill 10 enemies");
        if (_killCount >= 100)
            TryUnlock("killer_100", "Mass Murderer", "Kill 100 enemies");
    }
    
    private void OnScoreChanged(ScoreChangedEvent e)
    {
        _totalScore = e.NewScore;
        
        if (_totalScore >= 10000)
            TryUnlock("score_10k", "High Scorer", "Reach 10,000 points");
    }
    
    private void OnPlayerDied(PlayerDiedEvent e)
    {
        TryUnlock("first_death", "Welcome to Dark Souls", "Die for the first time");
    }
    
    private void TryUnlock(string id, string title, string description)
    {
        if (_unlockedAchievements.Contains(id)) return;
        
        _unlockedAchievements.Add(id);
        
        // Publish achievement event for UI/sound
        EventBus.Publish(new AchievementUnlockedEvent
        {
            AchievementId = id,
            Title = title,
            Description = description
        });
    }
}

// Achievement popup UI
public class AchievementPopup : MonoBehaviour
{
    private EventSubscription _subscription;
    
    private void Start()
    {
        _subscription = EventBus.Subscribe<AchievementUnlockedEvent>(ShowPopup);
    }
    
    private void OnDestroy()
    {
        _subscription?.Dispose();
    }
    
    private void ShowPopup(AchievementUnlockedEvent e)
    {
        Debug.Log($"Achievement Unlocked: {e.Title} - {e.Description}");
        // Show UI popup...
    }
}
```

---

## Best Practices

### Do

- ✅ Use `EventSubscription` for automatic cleanup
- ✅ Define events as structs for type safety
- ✅ Keep event data immutable (readonly fields/properties)
- ✅ Dispose subscriptions in `OnDestroy`
- ✅ Use meaningful event names

### Don't

- ❌ Forget to unsubscribe (causes memory leaks)
- ❌ Publish events in tight loops (performance)
- ❌ Store mutable references in events
- ❌ Use events for same-frame communication (use direct calls)
- ❌ Catch exceptions in handlers without rethrowing

---

## Type-Safe vs Enum Events

| Feature | Type-Safe (struct) | Enum-Based |
|---------|-------------------|------------|
| Type Safety | ✅ Compile-time | ❌ Runtime casting |
| Refactoring | ✅ IDE support | ⚠️ Manual |
| Data | ✅ Strongly typed | ⚠️ `object` casting |
| Performance | ✅ No boxing (struct) | ⚠️ Boxing |
| Simplicity | ⚠️ More boilerplate | ✅ Quick setup |

**Recommendation:** Use type-safe events for new code.
