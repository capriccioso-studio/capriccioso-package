# Capriccioso Core Package

A comprehensive Unity starter package providing core utilities, logging, event systems, pooling, scene management, and debugging tools for rapid game development.

[![Unity 6000.0+](https://img.shields.io/badge/Unity-6000.0%2B-blue.svg)](https://unity.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.md)

## Installation

### Via Unity Package Manager (Git URL)

1. Open Unity Package Manager (`Window > Package Manager`)
2. Click `+` → `Add package from git URL...`
3. Enter: `https://github.com/capriccioso-studio/capriccioso-package.git`

### Via Local Package

1. Clone or download this repository
2. In Unity: `Window > Package Manager` → `+` → `Add package from disk...`
3. Select the `package.json` file

---

## Package Contents

### 📁 Runtime/Core

| Script | Description |
|--------|-------------|
| `MonoSingleton<T>` | Thread-safe singleton base for MonoBehaviours |
| `Constants` | Centralized constants including log colors |
| `ServiceLocator` | Service registration/resolution pattern |
| `BuildInfo` | Runtime build version and timestamp info |

### 📁 Runtime/Logging

| Script | Description |
|--------|-------------|
| `CLogger` | Static colored logging with multiple levels |
| `CLoggerSettings` | ScriptableObject for runtime configuration |

### 📁 Runtime/Events

| Script | Description |
|--------|-------------|
| `EventBus` | Type-safe static pub/sub event system |
| `GameEvent` | ScriptableObject-based events (designer-friendly) |
| `GameEventListener` | Component to respond to GameEvents |
| `GameEvent<T>` | Generic typed events (Int, Float, String, Bool, Vector3) |

### 📁 Runtime/Pooling

| Script | Description |
|--------|-------------|
| `ObjectPool<T>` | Generic wrapper around Unity's ObjectPool |
| `GameObjectPool` | Specialized pool for GameObjects |

### 📁 Runtime/SceneManagement

| Script | Description |
|--------|-------------|
| `SceneLoader` | Async scene loading with progress & transitions |

### 📁 Runtime/Timing

| Script | Description |
|--------|-------------|
| `Timer` | Frame-independent countdown timer |
| `Cooldown` | Reusable ability/action cooldown tracker |

### 📁 Runtime/Extensions

| Script | Description |
|--------|-------------|
| `VectorExtensions` | Vector2/Vector3 helpers (With, Flat, etc.) |
| `TransformExtensions` | Transform utilities (DestroyChildren, SetPositionX) |
| `CollectionExtensions` | List/Array utilities (Shuffle, Random, IsNullOrEmpty) |

### 📁 Runtime/Data

| Script | Description |
|--------|-------------|
| `RuntimeSet<T>` | ScriptableObject-based runtime collections |

### 📁 Runtime/Bootstrap

| Script | Description |
|--------|-------------|
| `BootstrapLoader` | Scene-agnostic initialization system |
| `BootstrapSettings` | Configuration for bootstrap behavior |

### 📁 Runtime/Debug

| Script | Description |
|--------|-------------|
| `DebugDisplay` | Runtime debug overlay (FPS, memory, build info) |
| `DebugDisplayService` | Singleton access to DebugDisplay |

### 📁 Editor

| Script | Description |
|--------|-------------|
| `CLoggerSettingsWindow` | Editor window for logger configuration |
| `CreateFoldersMenu` | Quick-create project folder structure |
| `BuildInfoUpdater` | Auto-updates build info before builds |

---

## Quick Start Examples

### MonoSingleton (Services)

```csharp
// Create a service
public class AudioService : MonoSingleton<AudioService>
{
    public void PlaySound(AudioClip clip) { /* ... */ }
}

// Use from anywhere
AudioService.Instance.PlaySound(clip);
```

### ServiceLocator

```csharp
// Register (in bootstrap)
ServiceLocator.Register<IAudioService>(AudioService.Instance);

// Resolve (anywhere)
IAudioService audio = ServiceLocator.Get<IAudioService>();
```

### CLogger

```csharp
CLogger.LogInfo("Player connected");
CLogger.LogSuccess("Level complete!");
CLogger.LogWarning("Low memory");
CLogger.LogError("Failed to load asset");
CLogger.LogEvent("OnPlayerDeath triggered");
```

### EventBus

```csharp
// Subscribe
EventBus.Subscribe(GameEventType.PlayerDied, OnPlayerDied);

// Publish
EventBus.Publish(GameEventType.PlayerDied, deathData);

// Unsubscribe (important!)
EventBus.Unsubscribe(GameEventType.PlayerDied, OnPlayerDied);
```

### Timer & Cooldown

```csharp
// Timer
Timer timer = new Timer(5f);
timer.OnTimerComplete += () => Debug.Log("Done!");
timer.Start();
// In Update: timer.Tick(Time.deltaTime);

// Cooldown
Cooldown dashCooldown = new Cooldown(3f);
// In Update: dashCooldown.Tick(Time.deltaTime);
if (dashCooldown.TryUse()) { Dash(); }
```

### SceneLoader

```csharp
await SceneLoader.LoadSceneAsync("GameLevel", progress => 
{
    loadingBar.fillAmount = progress;
});
```

### RuntimeSet

```csharp
// In Enemy.cs
[SerializeField] private GameObjectRuntimeSet _allEnemies;

void OnEnable() => _allEnemies.Add(gameObject);
void OnDisable() => _allEnemies.Remove(gameObject);

// In WaveManager.cs
bool AllDefeated => _allEnemies.Count == 0;
GameObject closest = _allEnemies.GetClosest(playerPos);
```

### DebugDisplay

```csharp
// Toggle with F3 key (default)
// Or via code:
DebugDisplayService.Instance.Toggle();
DebugDisplayService.Instance.ShowFPS = true;
```

---

## Editor Tools

### Logger Settings
`Window > Capriccioso > Logger Settings`

Configure log levels, test logging, view color preview.

### Project Folders
`Assets > Create > Capriccioso > Project Folders`

Creates the standard folder structure with underscore prefixes.

### Build Info
`Window > Capriccioso > Update Build Info`

Manually updates build info (auto-runs on build).

---

## Documentation

See the [Documentation~](Documentation~/) folder for detailed guidelines:

- [Contribution Guidelines](Documentation~/0-contribution-guidelines.md)
- [General Guidelines](Documentation~/1-general-guidelines.md)
- [Code Style Guidelines](Documentation~/2-code-style-guidelines.md)
- [Commenting Guidelines](Documentation~/3-commenting-guidelines.md)
- [Logging Guidelines](Documentation~/4-logging-guidelines.md)
- [Testing Guidelines](Documentation~/5-testing-guidelines.md)
- [Deployment Guidelines](Documentation~/6-deployment-guidelines.md)
- [Versioning Guidelines](Documentation~/7-versioning-guidelines.md)

---

## Requirements

- Unity 6000.0 or later
- C# 10 with nullable enabled

---

## License

MIT License - see [LICENSE.md](LICENSE.md)
