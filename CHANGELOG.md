# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0] - 2026-03-10

### Added
- **Core Systems**
  - `MonoSingleton<T>` — Thread-safe singleton base class
  - `ServiceLocator` — Interface-based service registration
  - `Constants` — Centralized log colors and defaults
  - `BuildInfo` — Runtime version information

- **Event System**
  - `EventBus` — Static pub/sub with auto-dispose subscriptions
  - `GameEvent` — ScriptableObject-based events
  - `GameEventListener` — Inspector-configurable event listeners
  - `GameEvent<T>` — Generic typed events (Int, Float, String, Bool, Vector3)

- **Pipeline System**
  - `BrokerChain<T>` — Synchronous chain of responsibility
  - `AsyncBrokerChain<T>` — Async variant with Task support
  - `BrokerChainBuilder<T>` — Fluent builder pattern
  - `BrokerChainRegistry` — Global chain registration

- **Utilities**
  - `ObjectPool<T>` — Generic object pooling
  - `GameObjectPool` — GameObject-specific pooling with IPoolable
  - `Timer` — Countdown timer with events
  - `Cooldown` — Elapsed time tracking for abilities
  - `CLogger` — Colored console logging

- **Extensions**
  - `VectorExtensions` — With*, Flat, Random, Clamp helpers
  - `TransformExtensions` — Position/scale setters, child management
  - `CollectionExtensions` — Random, shuffle, safe access

- **Scene Management**
  - `BootstrapLoader` — Scene-agnostic initialization
  - `SceneLoader` — Async scene loading with progress

- **Debug Tools**
  - `DebugDisplay` — FPS/memory overlay
  - `RuntimeSet<T>` — ScriptableObject-based collections

- **Editor Tools**
  - `CreateFoldersMenu` — Quick folder structure setup
  - `CLoggerSettingsWindow` — Logger configuration
  - `BuildInfoUpdater` — Auto-update build info

### Documentation
- README with Quick Start guide
- ARCHITECTURE.md with layer hierarchy
- SYSTEMS.md with consolidated system docs
- CODE_STANDARDS.md with style guidelines
- CONTRIBUTING.md with workflow guide
