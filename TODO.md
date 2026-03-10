# Capriccioso Starter Package — Development Roadmap

This document outlines the development priorities for creating a comprehensive, production-ready Unity package.

---

## ✅ PHASE 0: Documentation & Structure (Current)

Foundation work to establish clear documentation and remove game-specific content.

| Task | Priority | Status | Notes |
|------|----------|--------|-------|
| Create TODO.md roadmap | P0 | ✅ Done | This file |
| Trim README.md to Quick Start | P0 | 🔲 Pending | Remove architecture section |
| Create ARCHITECTURE.md | P0 | 🔲 Pending | Layer hierarchy & access rules |
| Create SYSTEMS.md | P0 | 🔲 Pending | Consolidate all system readmes |
| Update CODE_STANDARDS.md | P1 | 🔲 Pending | Mark folder structure as "Recommended" |
| Create CHANGELOG.md | P1 | 🔲 Pending | Minimal 1.0.0 entry |

---

## � PHASE 1: Testing & Validation (P0 — Critical)

Unit test coverage for production readiness.

| Task | Priority | Status | Notes |
|------|----------|--------|-------|
| Create `Tests~` folder structure | P0 | ✅ Done | Assembly definitions for edit/play mode |
| MonoSingleton tests | P0 | ✅ Done | Thread safety, domain reload, duplicates |
| ServiceLocator tests | P0 | ✅ Done | Registration, retrieval, overwrite |
| EventBus tests | P0 | ✅ Done | Subscribe, publish, dispose, error handling |
| BrokerChain tests | P0 | ✅ Done | Priority ordering, cancellation, async |
| Timer/Cooldown tests | P1 | ✅ Done | Edge cases, pause/resume |
| ObjectPool tests | P1 | ✅ Done | IPoolable callbacks, capacity limits |

**Target: 20+ unit tests covering edge cases** ✅ **80+ tests implemented**

---

## 🟠 PHASE 2: Core System Improvements (P1 — High)

Polish existing systems for production readiness.

| Task | Priority | Status | Notes |
|------|----------|--------|-------|
| Add null checks & validation | P1 | 🔲 Pending | Audit all public methods |
| Add `Clear()` to GameObjectPool | P1 | 🔲 Pending | Memory management |
| Add `GetAll<T>()` to ServiceLocator | P1 | 🔲 Pending | Multiple registrations support |
| Add `UnsubscribeAll<T>()` to EventBus | P1 | 🔲 Pending | Bulk cleanup |
| Document thread safety matrix | P1 | 🔲 Pending | Which systems are thread-safe |
| Add log level filtering to CLogger | P2 | 🔲 Pending | Runtime toggle Info/Warning/Error |

---

## 🟡 PHASE 3: Documentation Expansion (P2 — Medium)

Additional guides for team onboarding.

| Task | Priority | Status | Notes |
|------|----------|--------|-------|
| Create TROUBLESHOOTING.md | P2 | 🔲 Pending | Common issues & solutions |
| Create PERFORMANCE.md | P2 | 🔲 Pending | Best practices, pooling, threading |
| Create EXAMPLES.md | P2 | 🔲 Pending | Recipes for common scenarios |
| Document BrokerChain priority conventions | P2 | 🔲 Pending | 0 = highest, standard ranges |
| Document exception handling patterns | P2 | 🔲 Pending | What throws vs logs |

---

## 🟢 PHASE 4: Optional Enhancements (P3 — Nice to Have)

Low-priority improvements for future iterations.

| Task | Priority | Status | Notes |
|------|----------|--------|-------|
| WeakEventBus pattern | P3 | 🔲 Pending | Prevent memory leaks from listeners |
| BuildInfo config file fallback | P3 | 🔲 Pending | Beyond Resources.Load |
| BootstrapSettings direct path support | P3 | 🔲 Pending | Not just Resources folder |
| Performance benchmarks | P3 | 🔲 Pending | Baseline metrics |
| Migration guide template | P3 | 🔲 Pending | For version upgrades |

---

## 📋 Core Pattern Checklist

The package should provide these **four core patterns** with comprehensive documentation:

### 1. ServiceLocator ✅
- [x] Interface-based registration
- [x] Thread-safe access
- [x] TryGet pattern
- [x] Logging on registration
- [ ] GetAll<T>() support

### 2. EventBus ✅
- [x] Type-safe struct events
- [x] Enum-based events
- [x] Auto-dispose subscriptions
- [x] Error handling
- [ ] UnsubscribeAll<T>()

### 3. BrokerChain ✅
- [x] Sync chain
- [x] Async chain
- [x] Priority-based ordering
- [x] Cancellation support
- [x] Builder pattern
- [x] Registry pattern

### 4. MonoSingleton ✅
- [x] Thread-safe
- [x] DontDestroyOnLoad
- [x] Domain reload safe
- [x] Lazy initialization
- [x] Duplicate prevention

---

## 📦 Package Contents Summary

```
capriccioso-starterpackage/
├── Documentation~/
│   ├── ARCHITECTURE.md      ← Layer hierarchy & access rules
│   ├── CODE_STANDARDS.md    ← Style guidelines
│   ├── CONTRIBUTING.md      ← Branch workflow
│   └── SYSTEMS.md           ← All system documentation
├── Editor/
│   └── ...                  ← Editor tools
├── Runtime/
│   ├── Bootstrap/           ← Scene-agnostic initialization
│   ├── Core/                ← MonoSingleton, ServiceLocator, Constants
│   ├── Data/                ← RuntimeSet
│   ├── Debug/               ← DebugDisplay
│   ├── Events/              ← EventBus, GameEvent, GameEventListener
│   ├── Extensions/          ← Vector, Transform, Collection helpers
│   ├── Logging/             ← CLogger
│   ├── Pipeline/            ← BrokerChain, AsyncBrokerChain
│   ├── Pooling/             ← ObjectPool, GameObjectPool
│   ├── SceneManagement/     ← SceneLoader
│   └── Timing/              ← Timer, Cooldown
├── Tests~/                  ← Unit tests (80+ tests)
├── CHANGELOG.md             ← Version history
├── LICENSE.md
├── package.json
├── README.md                ← Quick Start guide
└── TODO.md                  ← This file
```

---

## 🎯 Definition of Done

The package is **production-ready** when:

1. ✅ All four core patterns implemented with thread safety
2. ✅ 20+ unit tests passing (80+ implemented)
3. 🔲 All public APIs have XML documentation
4. 🔲 No null reference exceptions on normal use
5. 🔲 README provides working Quick Start example
6. 🔲 ARCHITECTURE.md explains layer hierarchy
7. 🔲 SYSTEMS.md consolidates all pattern documentation

---

## 🚀 Quick Win Checklist

For immediate use before full completion:

- [x] Import package into Unity project
- [x] Create Bootstrap scene with managers
- [x] Register services in ServiceLocator
- [x] Define event structs
- [x] Use EventBus for decoupled communication
- [x] Use MonoSingleton for global managers
- [x] Use ObjectPool for frequent spawns
- [x] Use Timer/Cooldown for time-based mechanics

---

*Last updated: March 2026*
