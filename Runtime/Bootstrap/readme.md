# Bootstrap Loader

The BootstrapLoader provides scene-agnostic initialization, ensuring required services are loaded regardless of which scene you start from. This is essential for rapid iteration during development.

## Why Use Bootstrap?

Without bootstrap, you must always start from a specific "main" scene to initialize managers. This slows development:

```
❌ Without Bootstrap:
- Start from Level3 scene → AudioManager not found!
- Start from MainMenu scene → Works ✓
- Every play test requires going through main menu
```

```
✅ With Bootstrap:
- Start from any scene → Bootstrap loads automatically
- All managers initialized → Everything works ✓
- Instant iteration on any scene
```

---

## Quick Start

### 1. Create the Bootstrap Scene

1. **File > New Scene**
2. Save as `Bootstrap` (or your preferred name)
3. Add persistent managers (they will use `DontDestroyOnLoad`):

```
Bootstrap Scene Hierarchy:
├── [AudioManager]        ← MonoSingleton
├── [GameManager]         ← MonoSingleton
├── [NetworkManager]      ← MonoSingleton
├── EventSystem           ← Unity's EventSystem
├── AnalyticsService      ← Registered to ServiceLocator
└── ...
```

### 2. Add Bootstrap to Build Settings

1. **File > Build Settings**
2. Add `Bootstrap` scene (should be index 0 or at least in the list)

### 3. Create Settings Asset (Optional)

1. **Create > Capriccioso > Bootstrap Settings**
2. Save to `Assets/Resources/BootstrapSettings.asset`
3. Configure as needed

### 4. Done!

Now when you press Play from any scene, the Bootstrap scene loads automatically.

---

## How It Works

```
┌─────────────────────────────────────────────────────────┐
│                    Game Starts                          │
└─────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────┐
│  RuntimeInitializeOnLoadMethod(BeforeSceneLoad)         │
│  BootstrapLoader.OnBeforeSceneLoad()                    │
└─────────────────────────────────────────────────────────┘
                           │
              ┌────────────┴────────────┐
              ▼                         ▼
    ┌─────────────────┐       ┌─────────────────┐
    │ Bootstrap scene │       │ Bootstrap scene │
    │   NOT loaded    │       │ already loaded  │
    └─────────────────┘       └─────────────────┘
              │                         │
              ▼                         │
    ┌─────────────────┐                 │
    │ Load Bootstrap  │                 │
    │   additively    │                 │
    └─────────────────┘                 │
              │                         │
              └────────────┬────────────┘
                           ▼
┌─────────────────────────────────────────────────────────┐
│           Original scene continues loading              │
│         All services now available from Bootstrap       │
└─────────────────────────────────────────────────────────┘
```

---

## Configuration

### BootstrapSettings Asset

Create via **Assets > Create > Capriccioso > Bootstrap Settings**

| Field | Description | Default |
|-------|-------------|---------|
| Enabled | Enable/disable the bootstrap system | `true` |
| Bootstrap Scene Name | Name of the bootstrap scene | `"Bootstrap"` |
| Excluded Scenes | Scenes that skip bootstrap loading | Empty |

### Example Configuration

```
BootstrapSettings
├── Enabled: ✓
├── Bootstrap Scene Name: "Bootstrap"
└── Excluded Scenes:
    ├── "SplashScreen"
    └── "CompanyLogo"
```

---

## Bootstrap Scene Structure

### Recommended Hierarchy

```
Bootstrap Scene
│
├── [Managers]
│   ├── AudioManager         (MonoSingleton<AudioManager>)
│   ├── GameManager          (MonoSingleton<GameManager>)
│   ├── SaveManager          (MonoSingleton<SaveManager>)
│   └── SettingsManager      (MonoSingleton<SettingsManager>)
│
├── [Services]
│   ├── AnalyticsService     (→ ServiceLocator.Register<IAnalytics>)
│   ├── NetworkService       (→ ServiceLocator.Register<INetwork>)
│   └── AdService            (→ ServiceLocator.Register<IAds>)
│
├── [Systems]
│   ├── EventSystem          (Unity's EventSystem)
│   └── Input System         (InputActionAsset)
│
└── [Debug]
    └── DebugDisplay         (Shows FPS, memory, etc.)
```

### Example Bootstrap Script

```csharp
using Capriccioso;
using UnityEngine;

public class BootstrapInitializer : MonoBehaviour
{
    [SerializeField] private AnalyticsService _analyticsService;
    [SerializeField] private NetworkService _networkService;
    
    private void Awake()
    {
        // Make this object persistent
        DontDestroyOnLoad(gameObject);
        
        // Register services
        ServiceLocator.Register<IAnalyticsService>(_analyticsService);
        ServiceLocator.Register<INetworkService>(_networkService);
        
        // Initialize pipelines
        InitializeBrokerChains();
        
        CLogger.LogSuccess("Bootstrap complete!");
    }
    
    private void InitializeBrokerChains()
    {
        // Set up any BrokerChains
        var buffChain = new BrokerChainBuilder<BuffContext>()
            .AddHandler(new BuffValidationHandler())
            .AddHandler(new BuffApplicationHandler())
            .Build();
        
        BrokerChainRegistry.Register(buffChain);
    }
}
```

---

## Excluded Scenes

Some scenes shouldn't trigger bootstrap loading:

- **Splash screens** — Company logos, age gates
- **Loading scenes** — Minimal scenes that redirect elsewhere
- **Editor-only scenes** — Test scenes not in builds

Add these to the `Excluded Scenes` list in BootstrapSettings.

---

## API Reference

### Properties

```csharp
// Check if bootstrap has completed
if (BootstrapLoader.HasBootstrapped)
{
    // Services are ready
}

// Default scene name if no settings
string defaultName = BootstrapLoader.DefaultBootstrapSceneName; // "Bootstrap"
```

### Methods

```csharp
// Force bootstrap manually (useful for testing)
BootstrapLoader.ForceBootstrap();
```

---

## Common Patterns

### Wait for Bootstrap

If you need to ensure bootstrap is complete:

```csharp
using UnityEngine;
using Capriccioso;

public class MyComponent : MonoBehaviour
{
    private void Start()
    {
        // Bootstrap always completes before Start() is called
        // because it runs in RuntimeInitializeLoadType.BeforeSceneLoad
        
        var audio = ServiceLocator.Get<IAudioService>();
        audio.PlayMusic("ambient");
    }
}
```

### Conditional Initialization

```csharp
public class LevelManager : MonoBehaviour
{
    private void Start()
    {
        // Only initialize if bootstrap loaded managers
        if (BootstrapLoader.HasBootstrapped)
        {
            GameManager.Instance.StartLevel(levelId);
        }
        else
        {
            Debug.LogWarning("Bootstrap didn't run - managers not available");
        }
    }
}
```

---

## Troubleshooting

### Bootstrap scene not loading

1. **Check scene is in Build Settings**
   - File > Build Settings > Verify scene is listed

2. **Check scene name matches**
   - BootstrapSettings.BootstrapSceneName must match exactly

3. **Check settings are in Resources**
   - `BootstrapSettings.asset` must be in a `Resources` folder

4. **Check if disabled**
   - BootstrapSettings.Enabled must be `true`

### Services not available

1. **Timing issue** — Access services in `Start()`, not `Awake()`
2. **Missing registration** — Check Bootstrap scene has the service
3. **Excluded scene** — Check if current scene is in excluded list

### Multiple Bootstrap scenes loading

This shouldn't happen — BootstrapLoader checks if Bootstrap is already loaded. If it does:

1. Check for duplicate `[RuntimeInitializeOnLoadMethod]` calls
2. Ensure Bootstrap objects use `DontDestroyOnLoad` properly

---

## Best Practices

### ✅ Do

- Keep Bootstrap scene minimal (only persistent managers)
- Use `DontDestroyOnLoad` on all Bootstrap objects
- Register services in `Awake()`, use them in `Start()` or later
- Add splash/loading screens to excluded scenes list
- Test starting from various scenes during development

### ❌ Don't

- Put level-specific content in Bootstrap
- Rely on Bootstrap scene being the "first" scene
- Access services in `Awake()` of non-Bootstrap objects
- Forget to add Bootstrap to Build Settings
