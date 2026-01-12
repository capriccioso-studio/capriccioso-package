# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-01-12

### Added

- **Core**
  - `MonoSingleton<T>` - Thread-safe singleton base class for MonoBehaviours
  - `Constants` - Centralized constants including log colors
  - `ServiceLocator` - Service registration and resolution pattern
  - `BuildInfo` - Runtime build version and timestamp information

- **Logging**
  - `CLogger` - Static colored logging utility with multiple log levels
  - `CLoggerSettings` - ScriptableObject for runtime log configuration

- **Events**
  - `EventBus` - Improved type-safe static event system
  - `GameEvent` - ScriptableObject-based events for designer-friendly workflows
  - `GameEventListener` - MonoBehaviour component to respond to GameEvents

- **Pooling**
  - `ObjectPool<T>` - Generic object pooling wrapper around Unity's ObjectPool

- **Scene Management**
  - `SceneLoader` - Async scene loading with progress callbacks and transition support

- **Timing**
  - `Timer` - Frame-independent countdown timer
  - `Cooldown` - Reusable cooldown tracker

- **Extensions**
  - `VectorExtensions` - Vector2/Vector3 helper methods (With, Flat, etc.)
  - `TransformExtensions` - Transform utilities (DestroyChildren, SetPositionX, etc.)
  - `CollectionExtensions` - List/Array utilities (Shuffle, Random, IsNullOrEmpty)

- **Data**
  - `RuntimeSet<T>` - ScriptableObject-based runtime collections

- **Bootstrap**
  - `BootstrapLoader` - Scene-agnostic initialization system

- **Debugging**
  - `DebugDisplay` - Runtime debug overlay (FPS counter, memory usage)

- **Editor**
  - `CLoggerSettingsWindow` - Editor window for configuring logger settings

- **Documentation**
  - Contribution guidelines
  - Code style guidelines
  - Commenting guidelines
  - Logging guidelines
  - Deployment guidelines
  - Game versioning system
