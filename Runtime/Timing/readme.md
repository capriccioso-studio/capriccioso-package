# Timing System

The Timing system provides two complementary classes for managing time-based game mechanics:

| Class | Purpose | Use Case |
|-------|---------|----------|
| `Timer` | Countdown from duration to zero | Bomb timers, spawn delays, timed events |
| `Cooldown` | Track elapsed time since last use | Ability cooldowns, attack rates, spawn intervals |

---

## Timer

A countdown timer with events, pause/resume support, and progress tracking.

### Basic Usage

```csharp
using Capriccioso;
using UnityEngine;

public class BombController : MonoBehaviour
{
    private Timer _fuseTimer;
    
    private void Start()
    {
        // Create a 5-second timer
        _fuseTimer = new Timer(5f);
        
        // Subscribe to events
        _fuseTimer.OnTimerComplete += Explode;
        _fuseTimer.OnTimerTick += UpdateFuseUI;
        
        // Start the countdown
        _fuseTimer.Start();
    }
    
    private void Update()
    {
        // Must call Tick every frame
        _fuseTimer.Tick(Time.deltaTime);
    }
    
    private void UpdateFuseUI(float remaining)
    {
        fuseText.text = $"{remaining:F1}s";
        fuseBar.fillAmount = 1f - _fuseTimer.Progress;
    }
    
    private void Explode()
    {
        Debug.Log("BOOM!");
        // Explosion logic...
    }
}
```

### One-Shot Timer (Static Helper)

For simple delayed actions without managing the timer yourself:

```csharp
// Execute after 3 seconds - no Update() needed!
Timer.Create(3f, () => {
    Debug.Log("3 seconds passed!");
    SpawnEnemy();
});

// With tick callback for progress
Timer.Create(5f, 
    onComplete: () => ShowReward(),
    onTick: remaining => progressBar.fillAmount = 1f - (remaining / 5f)
);
```

### Pause and Resume

```csharp
public class PauseableTimer : MonoBehaviour
{
    private Timer _matchTimer;
    
    private void Start()
    {
        _matchTimer = new Timer(300f); // 5-minute match
        _matchTimer.OnTimerComplete += EndMatch;
        _matchTimer.Start();
    }
    
    private void Update()
    {
        _matchTimer.Tick(Time.deltaTime);
        
        matchTimeText.text = FormatTime(_matchTimer.RemainingTime);
    }
    
    public void OnPausePressed()
    {
        if (_matchTimer.IsPaused)
            _matchTimer.Resume();
        else
            _matchTimer.Pause();
    }
    
    public void RestartMatch()
    {
        _matchTimer.Reset(); // Reset to original 300s
        _matchTimer.Start();
    }
    
    public void StartOvertime()
    {
        _matchTimer.Reset(60f); // Reset with new 60s duration
        _matchTimer.Start();
    }
    
    private string FormatTime(float seconds)
    {
        int mins = (int)(seconds / 60);
        int secs = (int)(seconds % 60);
        return $"{mins:D2}:{secs:D2}";
    }
}
```

### Timer Properties

| Property | Type | Description |
|----------|------|-------------|
| `Duration` | float | Original/current duration |
| `RemainingTime` | float | Seconds remaining |
| `Progress` | float | 0 (started) to 1 (complete) |
| `IsRunning` | bool | Timer is active |
| `IsPaused` | bool | Timer is paused |
| `IsComplete` | bool | Timer reached zero |

---

## Cooldown

Tracks elapsed time for ability cooldowns and rate limiting.

### Basic Usage

```csharp
using Capriccioso;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    // Define cooldowns
    private Cooldown _attackCooldown = new Cooldown(0.5f);   // Attack every 0.5s
    private Cooldown _dodgeCooldown = new Cooldown(2f);      // Dodge every 2s
    private Cooldown _specialCooldown = new Cooldown(10f);   // Special every 10s
    
    private void Update()
    {
        // Update all cooldowns
        float dt = Time.deltaTime;
        _attackCooldown.Tick(dt);
        _dodgeCooldown.Tick(dt);
        _specialCooldown.Tick(dt);
        
        HandleInput();
        UpdateUI();
    }
    
    private void HandleInput()
    {
        // Method 1: Check then use
        if (Input.GetButton("Fire1") && _attackCooldown.IsReady)
        {
            Attack();
            _attackCooldown.Use();
        }
        
        // Method 2: TryUse (check and use in one call)
        if (Input.GetButtonDown("Dodge") && _dodgeCooldown.TryUse())
        {
            Dodge();
        }
        
        // Special ability
        if (Input.GetButtonDown("Special") && _specialCooldown.TryUse())
        {
            Special();
        }
    }
    
    private void UpdateUI()
    {
        // Progress for radial fill (0 = just used, 1 = ready)
        dodgeIcon.fillAmount = _dodgeCooldown.Progress;
        specialIcon.fillAmount = _specialCooldown.Progress;
        
        // Text display
        specialText.text = _specialCooldown.GetDisplayText("READY!");
    }
}
```

### Starting On Cooldown

```csharp
// Ability starts ready to use (default)
var dash = new Cooldown(3f, startReady: true);

// Ability starts on cooldown (e.g., powerful starting abilities)
var ultimate = new Cooldown(30f, startReady: false);
```

### Cooldown Modification

```csharp
public class AbilityUpgradeSystem
{
    private Cooldown _fireball = new Cooldown(5f);
    
    // Apply cooldown reduction (e.g., from items/talents)
    public void ApplyCooldownReduction(float percentReduction)
    {
        // 20% reduction: 5s * 0.8 = 4s
        float multiplier = 1f - percentReduction;
        float newDuration = _fireball.Duration * multiplier;
        
        // Preserve progress so current cooldown scales proportionally
        _fireball.SetDuration(newDuration, preserveProgress: true);
    }
    
    // Reduce current cooldown (e.g., from kills)
    public void OnEnemyKilled()
    {
        _fireball.ReduceCooldown(1f); // Reduce by 1 second
    }
    
    // Reset cooldown (e.g., from buff)
    public void OnCooldownReset()
    {
        _fireball.Reset(); // Immediately ready
    }
    
    // Force start cooldown (e.g., from debuff)
    public void OnSilenced()
    {
        _fireball.ForceStart(); // Start cooldown even if was ready
    }
}
```

### Cooldown Properties

| Property | Type | Description |
|----------|------|-------------|
| `Duration` | float | Cooldown duration in seconds |
| `ElapsedTime` | float | Time since last use |
| `RemainingTime` | float | Time until ready (0 if ready) |
| `Progress` | float | 0 (just used) to 1 (ready) |
| `IsReady` | bool | Can be used |
| `IsOnCooldown` | bool | Currently cooling down |

---

## Timer vs Cooldown

| Feature | Timer | Cooldown |
|---------|-------|----------|
| Counts | Down (duration → 0) | Up (0 → duration) |
| Events | OnComplete, OnTick | None (poll-based) |
| Pause/Resume | Yes | No (use deltaTime = 0) |
| Primary Use | One-time countdowns | Repeatable actions |
| Progress | 0 → 1 as time passes | 0 → 1 as cooldown recovers |

### When to Use Timer

- Bomb/explosion countdowns
- Match timers
- Spawn delays
- Timed challenges
- Any one-shot delayed action

### When to Use Cooldown

- Ability cooldowns
- Attack rates
- Spawn intervals
- Rate limiting
- Any repeatable action with delay

---

## Complete Example: Ability System

```csharp
using Capriccioso;
using UnityEngine;
using System;

public class Ability
{
    public string Name { get; }
    public Cooldown Cooldown { get; }
    public Action Execute { get; }
    
    public Ability(string name, float cooldownDuration, Action execute)
    {
        Name = name;
        Cooldown = new Cooldown(cooldownDuration);
        Execute = execute;
    }
    
    public bool TryExecute()
    {
        if (Cooldown.TryUse())
        {
            Execute?.Invoke();
            return true;
        }
        return false;
    }
    
    public void Tick(float deltaTime) => Cooldown.Tick(deltaTime);
}

public class AbilityManager : MonoBehaviour
{
    private Ability[] _abilities;
    
    private void Awake()
    {
        _abilities = new[]
        {
            new Ability("Fireball", 2f, CastFireball),
            new Ability("Ice Shield", 8f, CastIceShield),
            new Ability("Teleport", 5f, CastTeleport),
            new Ability("Ultimate", 30f, CastUltimate)
        };
    }
    
    private void Update()
    {
        // Tick all abilities
        float dt = Time.deltaTime;
        foreach (var ability in _abilities)
        {
            ability.Tick(dt);
        }
        
        // Input handling
        if (Input.GetKeyDown(KeyCode.Alpha1)) _abilities[0].TryExecute();
        if (Input.GetKeyDown(KeyCode.Alpha2)) _abilities[1].TryExecute();
        if (Input.GetKeyDown(KeyCode.Alpha3)) _abilities[2].TryExecute();
        if (Input.GetKeyDown(KeyCode.Alpha4)) _abilities[3].TryExecute();
    }
    
    private void CastFireball() => Debug.Log("Fireball!");
    private void CastIceShield() => Debug.Log("Ice Shield!");
    private void CastTeleport() => Debug.Log("Teleport!");
    private void CastUltimate() => Debug.Log("ULTIMATE!");
}
```

---

## Unscaled Time (Pause-Safe)

For UI or systems that should work during pause:

```csharp
private void Update()
{
    // Use unscaledDeltaTime for pause-immune timing
    _menuCooldown.Tick(Time.unscaledDeltaTime);
    _menuTimer.Tick(Time.unscaledDeltaTime);
}
```
