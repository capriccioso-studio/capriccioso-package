# Pipeline / Broker Chain System

The Pipeline system implements a **Chain of Responsibility** pattern combined with a **Broker** for centralized handler registration. This allows you to build middleware-style pipelines where multiple handlers process a request sequentially.

## When to Use

- **Buff/Debuff Systems** — Apply, stack, and remove timed effects
- **Damage Calculation** — Chain modifiers (armor, resistances, critical hits)
- **Input Processing** — Validate, transform, and route input
- **Command Validation** — Check permissions, cooldowns, resources before executing
- **Request/Response Pipelines** — API calls, save/load operations

---

## Core Concepts

| Component | Description |
|-----------|-------------|
| `IBrokerContext` | Interface for data passed through the chain |
| `BrokerContext` | Default implementation with `IsHandled` and `IsCancelled` flags |
| `IBrokerHandler<T>` | Interface for handlers with `Priority` and `Handle()` |
| `BrokerChain<T>` | Manages and executes a chain of handlers |
| `AsyncBrokerChain<T>` | Async variant using `Task` and `async/await` |
| `BrokerChainBuilder<T>` | Fluent builder for constructing chains |
| `BrokerChainRegistry` | Global static registry for chains (ServiceLocator pattern) |

---

## Basic Usage

### 1. Define Your Context

The context carries all data through the pipeline:

```csharp
using Capriccioso.Pipeline;

public class BuffContext : IBrokerContext
{
    // Required by IBrokerContext
    public bool IsHandled { get; set; }
    public bool IsCancelled { get; set; }
    
    // Your custom data
    public GameObject Target { get; set; }
    public BuffType BuffType { get; set; }
    public float Duration { get; set; }
    public float Magnitude { get; set; }
    public bool WasApplied { get; set; }
}

public enum BuffType
{
    SpeedBoost,
    DamageReduction,
    Regeneration,
    Poison
}
```

### 2. Create Handlers

Each handler processes the context and decides whether to continue the chain:

```csharp
using Capriccioso.Pipeline;

/// <summary>
/// Validates that the target can receive buffs.
/// </summary>
public class BuffValidationHandler : IBrokerHandler<BuffContext>
{
    // Lower priority = executes first
    public int Priority => 0;
    
    public void Handle(BuffContext context, Action<BuffContext> next)
    {
        // Check if target is valid
        if (context.Target == null)
        {
            context.IsCancelled = true;
            CLogger.LogWarning("Buff target is null, cancelling.");
            return; // Don't call next - stops the chain
        }
        
        // Check if target is immune
        var immunity = context.Target.GetComponent<BuffImmunity>();
        if (immunity != null && immunity.IsImmuneToType(context.BuffType))
        {
            context.IsCancelled = true;
            CLogger.LogInfo($"Target is immune to {context.BuffType}");
            return;
        }
        
        // Continue to next handler
        next(context);
    }
}

/// <summary>
/// Checks for existing buffs and handles stacking.
/// </summary>
public class BuffStackingHandler : IBrokerHandler<BuffContext>
{
    public int Priority => 10;
    
    public void Handle(BuffContext context, Action<BuffContext> next)
    {
        var buffManager = context.Target.GetComponent<BuffManager>();
        if (buffManager == null)
        {
            next(context);
            return;
        }
        
        // Check if buff already exists
        var existingBuff = buffManager.GetBuff(context.BuffType);
        if (existingBuff != null)
        {
            // Stack: refresh duration and increase magnitude
            existingBuff.RefreshDuration(context.Duration);
            existingBuff.StackMagnitude(context.Magnitude);
            
            context.IsHandled = true; // Mark as handled
            context.WasApplied = true;
            CLogger.LogSuccess($"Stacked {context.BuffType} on {context.Target.name}");
            return; // Don't apply new buff
        }
        
        next(context);
    }
}

/// <summary>
/// Actually applies the buff to the target.
/// </summary>
public class BuffApplicationHandler : IBrokerHandler<BuffContext>
{
    public int Priority => 100;
    
    public void Handle(BuffContext context, Action<BuffContext> next)
    {
        var buffManager = context.Target.GetComponent<BuffManager>();
        if (buffManager == null)
        {
            buffManager = context.Target.AddComponent<BuffManager>();
        }
        
        // Apply the buff
        buffManager.ApplyBuff(new Buff
        {
            Type = context.BuffType,
            Duration = context.Duration,
            Magnitude = context.Magnitude
        });
        
        context.WasApplied = true;
        context.IsHandled = true;
        
        CLogger.LogSuccess($"Applied {context.BuffType} to {context.Target.name} for {context.Duration}s");
        
        next(context); // Allow post-processing handlers
    }
}

/// <summary>
/// Triggers visual/audio feedback after buff is applied.
/// </summary>
public class BuffFeedbackHandler : IBrokerHandler<BuffContext>
{
    public int Priority => 200;
    
    public void Handle(BuffContext context, Action<BuffContext> next)
    {
        if (!context.WasApplied) return;
        
        // Play VFX
        var vfx = context.Target.GetComponent<BuffVFX>();
        vfx?.PlayBuffEffect(context.BuffType);
        
        // Trigger event for UI
        EventBus.Publish(new BuffAppliedEvent 
        { 
            Target = context.Target, 
            BuffType = context.BuffType 
        });
        
        next(context);
    }
}
```

### 3. Build and Register the Chain

```csharp
using Capriccioso.Pipeline;

public class BuffSystemInitializer : MonoBehaviour
{
    private void Awake()
    {
        // Option 1: Using the builder
        var chain = new BrokerChainBuilder<BuffContext>()
            .AddHandler(new BuffValidationHandler())
            .AddHandler(new BuffStackingHandler())
            .AddHandler(new BuffApplicationHandler())
            .AddHandler(new BuffFeedbackHandler())
            .Build();
        
        // Register globally for access anywhere
        BrokerChainRegistry.Register(chain);
    }
    
    // Alternative: Manual registration
    private void SetupManually()
    {
        var chain = new BrokerChain<BuffContext>();
        chain.Register(new BuffValidationHandler());
        chain.Register(new BuffStackingHandler());
        chain.Register(new BuffApplicationHandler());
        chain.Register(new BuffFeedbackHandler());
        
        BrokerChainRegistry.Register(chain);
    }
}
```

### 4. Execute the Chain

```csharp
public class BuffCaster : MonoBehaviour
{
    public void ApplySpeedBoost(GameObject target)
    {
        // Get the chain from registry
        var chain = BrokerChainRegistry.Get<BuffContext>();
        
        // Create context with request data
        var context = new BuffContext
        {
            Target = target,
            BuffType = BuffType.SpeedBoost,
            Duration = 5f,
            Magnitude = 1.5f // 50% speed increase
        };
        
        // Execute the chain
        chain.Execute(context);
        
        // Check result
        if (context.WasApplied)
        {
            Debug.Log("Buff successfully applied!");
        }
        else if (context.IsCancelled)
        {
            Debug.Log("Buff was blocked.");
        }
    }
}
```

---

## Async Pipelines

For operations that require async/await (network calls, file I/O):

```csharp
public class AsyncBuffValidationHandler : IAsyncBrokerHandler<BuffContext>
{
    public int Priority => 0;
    
    public async Task HandleAsync(BuffContext context, Func<BuffContext, Task> next)
    {
        // Async validation (e.g., server-side check)
        bool isAllowed = await NetworkManager.ValidateBuffAsync(
            context.Target, 
            context.BuffType
        );
        
        if (!isAllowed)
        {
            context.IsCancelled = true;
            return;
        }
        
        await next(context);
    }
}

// Usage
var asyncChain = new AsyncBrokerChain<BuffContext>();
asyncChain.Register(new AsyncBuffValidationHandler());

await asyncChain.ExecuteAsync(context);
```

---

## Complete Timed Buff System Example

Here's a complete implementation of a timed buff system:

```csharp
// ============================================
// Buff.cs - The buff data structure
// ============================================
public class Buff
{
    public BuffType Type { get; set; }
    public float Duration { get; set; }
    public float RemainingTime { get; set; }
    public float Magnitude { get; set; }
    public int StackCount { get; set; } = 1;
    
    public bool IsExpired => RemainingTime <= 0f;
    public float Progress => 1f - (RemainingTime / Duration);
    
    public void RefreshDuration(float newDuration)
    {
        Duration = Mathf.Max(Duration, newDuration);
        RemainingTime = Duration;
    }
    
    public void StackMagnitude(float additionalMagnitude, int maxStacks = 5)
    {
        if (StackCount < maxStacks)
        {
            Magnitude += additionalMagnitude;
            StackCount++;
        }
    }
}

// ============================================
// BuffManager.cs - Per-entity buff tracking
// ============================================
public class BuffManager : MonoBehaviour
{
    private readonly Dictionary<BuffType, Buff> _activeBuffs = new();
    
    public event Action<Buff> OnBuffAdded;
    public event Action<Buff> OnBuffRemoved;
    public event Action<Buff> OnBuffExpired;
    
    private void Update()
    {
        TickBuffs(Time.deltaTime);
    }
    
    private void TickBuffs(float deltaTime)
    {
        var expiredBuffs = new List<BuffType>();
        
        foreach (var kvp in _activeBuffs)
        {
            kvp.Value.RemainingTime -= deltaTime;
            
            if (kvp.Value.IsExpired)
            {
                expiredBuffs.Add(kvp.Key);
            }
        }
        
        foreach (var buffType in expiredBuffs)
        {
            RemoveBuff(buffType, expired: true);
        }
    }
    
    public void ApplyBuff(Buff buff)
    {
        buff.RemainingTime = buff.Duration;
        _activeBuffs[buff.Type] = buff;
        OnBuffAdded?.Invoke(buff);
        
        // Apply effect
        ApplyBuffEffect(buff);
    }
    
    public Buff GetBuff(BuffType type)
    {
        return _activeBuffs.GetValueOrDefault(type);
    }
    
    public bool HasBuff(BuffType type) => _activeBuffs.ContainsKey(type);
    
    public void RemoveBuff(BuffType type, bool expired = false)
    {
        if (_activeBuffs.TryGetValue(type, out var buff))
        {
            _activeBuffs.Remove(type);
            RemoveBuffEffect(buff);
            
            if (expired)
                OnBuffExpired?.Invoke(buff);
            else
                OnBuffRemoved?.Invoke(buff);
        }
    }
    
    private void ApplyBuffEffect(Buff buff)
    {
        switch (buff.Type)
        {
            case BuffType.SpeedBoost:
                GetComponent<CharacterMovement>().SpeedMultiplier *= buff.Magnitude;
                break;
            case BuffType.DamageReduction:
                GetComponent<Health>().DamageReduction += buff.Magnitude;
                break;
            case BuffType.Regeneration:
                StartCoroutine(RegenerationTick(buff));
                break;
        }
    }
    
    private void RemoveBuffEffect(Buff buff)
    {
        switch (buff.Type)
        {
            case BuffType.SpeedBoost:
                GetComponent<CharacterMovement>().SpeedMultiplier /= buff.Magnitude;
                break;
            case BuffType.DamageReduction:
                GetComponent<Health>().DamageReduction -= buff.Magnitude;
                break;
        }
    }
    
    private IEnumerator RegenerationTick(Buff buff)
    {
        var health = GetComponent<Health>();
        while (HasBuff(BuffType.Regeneration))
        {
            health.Heal(buff.Magnitude);
            yield return new WaitForSeconds(1f);
        }
    }
}

// ============================================
// BuffRemovalContext.cs - For removing buffs
// ============================================
public class BuffRemovalContext : IBrokerContext
{
    public bool IsHandled { get; set; }
    public bool IsCancelled { get; set; }
    
    public GameObject Target { get; set; }
    public BuffType BuffType { get; set; }
    public bool ForceRemove { get; set; }
    public bool WasRemoved { get; set; }
}

// ============================================
// Buff removal handlers
// ============================================
public class BuffRemovalValidationHandler : IBrokerHandler<BuffRemovalContext>
{
    public int Priority => 0;
    
    public void Handle(BuffRemovalContext context, Action<BuffRemovalContext> next)
    {
        var buffManager = context.Target?.GetComponent<BuffManager>();
        if (buffManager == null || !buffManager.HasBuff(context.BuffType))
        {
            context.IsCancelled = true;
            return;
        }
        
        next(context);
    }
}

public class BuffRemovalHandler : IBrokerHandler<BuffRemovalContext>
{
    public int Priority => 100;
    
    public void Handle(BuffRemovalContext context, Action<BuffRemovalContext> next)
    {
        var buffManager = context.Target.GetComponent<BuffManager>();
        buffManager.RemoveBuff(context.BuffType);
        context.WasRemoved = true;
        context.IsHandled = true;
        
        CLogger.LogSuccess($"Removed {context.BuffType} from {context.Target.name}");
        next(context);
    }
}
```

---

## Flow Control

### Stopping the Chain

```csharp
public void Handle(BuffContext context, Action<BuffContext> next)
{
    // Option 1: Cancel (something went wrong)
    context.IsCancelled = true;
    return; // Don't call next
    
    // Option 2: Handled (processing complete, no error)
    context.IsHandled = true;
    return; // Don't call next
    
    // Option 3: Continue to next handler
    next(context);
}
```

### Handler Priority

Handlers execute in priority order (lowest first):

| Priority | Phase | Example |
|----------|-------|---------|
| 0-9 | Validation | Check permissions, validate input |
| 10-49 | Pre-processing | Modify context, check stacking |
| 50-99 | Core processing | Apply buff, deal damage |
| 100-199 | Post-processing | Apply effects, update stats |
| 200+ | Feedback | VFX, audio, UI updates |

---

## Error Handling

Exceptions in handlers are caught and logged, allowing the chain to continue:

```csharp
// In BrokerChain.Execute():
try
{
    handler.Handle(ctx, Next);
}
catch (Exception ex)
{
    CLogger.LogError($"Exception in broker handler {handler.GetType().Name}: {ex}");
    // Chain continues to next handler
}
```

---

## Thread Safety

Both `BrokerChain` and `BrokerChainRegistry` are thread-safe:

- Handler lists are protected by locks
- A copy of the handler list is made before execution
- Safe to register/unregister handlers from any thread
