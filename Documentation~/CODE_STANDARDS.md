# Code Standards

This document defines the coding standards, style guidelines, commenting conventions, and logging practices for Capriccioso projects.

## Table of Contents

- [General Guidelines](#general-guidelines)
  - [Performance](#performance)
  - [Correctness](#correctness)
  - [Asset Folder Structure](#asset-folder-structure)
- [Code Style](#code-style)
  - [Consistency](#consistency)
  - [Readability](#readability)
  - [Formatting](#formatting)
  - [Character Casing](#character-casing)
  - [Access Modifiers](#access-modifiers)
  - [Parentheses](#parentheses)
  - [Initialization](#initialization)
  - [Service-Handler Relationship](#service-handler-relationship)
  - [Expression Body](#expression-body)
  - [nameof](#nameof)
  - [C# with Nullable](#c-with-nullable)
  - [var](#var)
  - [One File Per Type](#one-file-per-type)
  - [Explicit Parameter Names](#explicit-parameter-names)
  - [Code Width](#code-width)
  - [Hardcoded Constants](#hardcoded-constants)
  - [Locked Suffix](#locked-suffix)
  - [Dedicated Locks](#dedicated-locks)
  - [Identifiers with Units](#identifiers-with-units)
  - [Is for Null Checking](#is-for-null-checking)
  - [No Fire and Forget](#no-fire-and-forget)
- [Commenting Guidelines](#commenting-guidelines)
  - [Proper English](#proper-english)
  - [XML Documentation](#xml-documentation)
  - [Inline Comments](#inline-comments)
  - [Summaries and Remarks](#summaries-and-remarks)
- [Logging Guidelines](#logging-guidelines)
  - [CLogger](#clogger)
  - [Log Levels](#log-levels)
  - [Log Coverage](#log-coverage)
  - [Method Entry/Exit Logging](#method-entryexit-logging)
  - [ToString Override](#tostring-override)
  - [Exception Logging](#exception-logging)

---

# General Guidelines

## Performance

We only **aim for asymptotic optimality** with structures designed to contain a non-trivial number of elements. We do not optimize for performance unless a bottleneck is identified. Code readability takes priority over optimization.

## Correctness

We **aim for theoretical correctness**. This includes avoiding race conditions with proper locking. In Unity, avoid `WaitForSeconds` or `Invoke` when possible. Anything waiting for a coroutine should be a callback or `Task`.

## Asset Folder Structure

We use underscores (`_`) to indicate internally-developed content, keeping project folders at the top of the Assets folder.

### Structure

```
Assets/
├── _Animations/
├── _Fonts/
├── _Materials/
├── _Prefabs/
├── _Resources/
├── _Scenes/
├── _Scripts/
│   ├── Classes/
│   ├── Data/
│   ├── Events/
│   ├── Handlers/
│   ├── Managers/
│   └── Services/
├── _Sounds/
└── _Sprites/
```

> **Tip:** Use the **CreateFolders** menu in Unity (`Assets > Create Folders`) to generate this structure automatically.

---

# Code Style

## Consistency

We aim for **globally consistent source code**. When global consistency is impractical, achieve local consistency within that part of the project.

## Readability

Code should be **optimized for reading**. Avoid complex language features or library calls. Avoid LINQ in production code unless trivial. Avoid fluent-style call chains.

## Formatting

Use the **default Visual Studio formatting**: 4-space indentation, [Allman-style braces](https://en.wikipedia.org/wiki/Indentation_style#Allman_style), and standard spacing.

## Character Casing

- **PascalCase**: Public identifiers, constants, methods (including private/local)
- **camelCase**: Everything else
- **_underscore prefix**: Private fields

```csharp
public const string ErrorProcessIsAlreadyRunning = nameof(CoreManager) + "_" + nameof(ErrorProcessIsAlreadyRunning);

private const int CoreStopTimeoutMilliseconds = 15_000;

private readonly bool _passDataDir;

private bool _hasMoved;

[SerializeField] private string _capricciosoName;

public override async Task<Result<bool, string>> InitializeAsync()
```

## Access Modifiers

Use the **least privilege principle**. Use `readonly` as much as possible. Always explicitly specify access modifiers.

## Parentheses

Use **parentheses for clarity** with all binary operators including `is` and `is not`.

```csharp
bool result = (this.coreInstance is not null) && this.coreInstance.IsRunning;

if ((i > 0) && ((j == 3) || this.object.Property))

int a = b + (c * d);
```

## Initialization

- **Non-static members**: Initialize in constructor
- **Static members**: Initialize outside constructor

```csharp
// Static initialization outside constructor
private static readonly JsonSerializerOptions serializationOptions = new()
{
    AllowTrailingCommas = true,
    WriteIndented = true,
};

// Non-static initialization inside constructor
private readonly object mapLock;

public ClassConstructor()
{
    this.mapLock = new();
}
```

## Service-Handler Relationship

### Services

Plug-and-play modules that derive from `MonoSingleton`. Developed as reusable functions callable from anywhere. Names should have a `Service` suffix.

```csharp
public class RegenerationService : MonoSingleton<RegenerationService>
{
    public void RegenerateHealth(float amount) { /* ... */ }
}
```

### Handlers

Classes that call services and interact with the game or UI. Names should end with `Handler`.

```csharp
public class PlayerHealthHandler : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        RegenerationService.Instance.RegenerateHealth(10f);
    }
}
```

## Expression Body

Only use expression bodies when readability improves—typically for direct assignments or single short calls.

```csharp
public bool SupportsRecycling => false;

public bool Match(object data) => data is ViewModelBase;
```

## nameof

Always use `nameof` instead of string literals.

```csharp
CLogger.LogInfo($"* {nameof(appDataFolder)}='{appDataFolder}', {nameof(passDataDir)}={passDataDir}");
```

## C# with Nullable

Use nullable reference types and target-typed new expressions. Put type specifications on the left.

```csharp
string? logFile = null;

DirectoryInfo directoryInfo = new(logFolder);
```

## var

**Never use `var`**. Always explicitly declare types.

## One File Per Type

Use **one file per type**. Exceptions allowed for small helper test classes.

## Explicit Parameter Names

Use explicit parameter naming when it helps readability.

```csharp
AccountLogoutRequest request = new(this.sessionToken, id: 1);
```

## Code Width

Lines should not exceed **180 characters** for readability. Exceptions for log lines that extend beyond.

```csharp
public KucoinRestClientFactory(IDateTimeProvider dateTimeProvider, 
    KucoinSymbolConverter symbolConverter, KucoinHttpErrorHandler errorHandler)
```

## Hardcoded Constants

Avoid hardcoding constants. Declare each constant properly.

## Locked Suffix

Use `Locked` suffix for methods requiring the caller to hold a lock. For async, use `LockedAsync`.

```csharp
/// <remarks>Caller must hold <see cref="lockObject"/>.</remarks>
private void ModifyCountLocked(long difference, bool add)

/// <remarks>Caller must hold <see cref="rpcClientLock"/>.</remarks>
private async Task<RecoverStatusAction> CheckClientLoggedInLockedAsync(bool verifyStatus)
```

## Dedicated Locks

Use **dedicated objects for locks**. Do not lock on objects with other purposes.

```csharp
/// <summary>Lock object to protect <see cref="count"/>.</summary>
private readonly object lockObject;

/// <summary>The remaining count.</summary>
/// <remarks>Access protected by <see cref="lockObject"/>.</remarks>
private long count;
```

## Identifiers with Units

Add unit suffixes to identifiers containing measurement values (unless the type defines units, like `TimeSpan`).

```csharp
public const int MaxQueueTimeIntervalMs = 30_000;
public const int DefaultMaxScriptFolderSizeKilobytes = 10 * 1024;
public const int MaxSecretSizeBytes = 256;
public const int MaxKeyLength = 256; // Characters, no suffix needed
```

## Is for Null Checking

Use `is` and `is not` for null checks instead of `==` and `!=`.

```csharp
if (instance is null) { /* ... */ }
if (instance is not null) { /* ... */ }
```

## No Fire and Forget

**Never call async methods fire-and-forget**. Always await the returned Task. Components must ensure background tasks complete before disposal.

---

# Commenting Guidelines

## Proper English

All comments use proper English. Articles may be omitted at the start of parameter/property comments. End sentences with `.`, `?`, or `!`.

```csharp
/// <summary>Queue of actions to be executed in a UI context.</summary>
private readonly Queue<PendingAction> uiQueue;

/// <param name="factory">Factory for starting async tasks.</param>
```

## XML Documentation

Use XML documentation (`///`) for all public and private members. Comments start with triple slash followed by a space.

### Added Value

Describe elements with valuable comments. Avoid repeating the type or nature of the element.

### No Empty Lines

Avoid empty lines in XML docs. Use `<para>` for paragraphs. Do not use `<br>`.

## Inline Comments

Double-slash (`//`) comments should be on a **separate line above** the relevant code.

```csharp
// Calculate the final damage after armor reduction
float finalDamage = baseDamage - armorReduction;
```

### Added Value

Do not write what is obvious from the code. Comments must provide new information.

## Summaries and Remarks

- **Summary**: Short paragraph describing the element, followed by additional explanation
- **Remarks**: Implementation details belong here

```csharp
/// <summary>
/// Creates a new instance of the object.
/// </summary>
public ClassName() { }

/// <summary>
/// Creates an empty instance for serialization.
/// </summary>
/// <remarks>Used by database serializer.</remarks>
public ClassName(bool empty) { }
```

---

# Logging Guidelines

## CLogger

We use `CLogger` for colored, categorized logging. CLogger is a **static** utility—no instance required.

```csharp
// Basic logging
CLogger.Log("Normal log message");
CLogger.LogInfo("Player connected to server");
CLogger.LogSuccess("Level completed successfully");
CLogger.LogWarning("Low memory detected");
CLogger.LogError("Failed to load asset");

// Backend-specific
CLogger.LogBackendInfo("Fetching user data from API");
CLogger.LogBackendSuccess("User data retrieved");
CLogger.LogBackendError("API request failed: 404");

// Events and major actions
CLogger.LogEvent("OnPlayerDeath triggered");
CLogger.LogMajorAction("Game Started");
```

## Log Levels

| Level | Color | Usage |
|-------|-------|-------|
| LOGG | Gray (#6C757D) | Normal log messages |
| INFO | Blue (#007BFF) | Notable events, state changes |
| GOOD | Green (#28A745) | Successful operations |
| WARN | Yellow (#FFC107) | Non-ideal but non-problematic paths |
| ERRR | Red (#DC3545) | Errors and exceptions |
| EVENT | Magenta (#FF00EA) | Event triggers and pub/sub |
| DB INFO | Cyan (#99CCFF) | Backend/API informational |
| DB GOOD | Light Green (#98FF39) | Successful backend operations |
| DB ERRR | Pink (#FF0066) | Backend/API errors |
| WAH | Bright Yellow (#FFFF00) | Major actions and milestones |
| DATA | Purple (#563D7C) | Data dumps and class content |

## Log Coverage

Write logs to **trace execution flow**. Log at crucial decision points—branching, loop counts, etc. Keep the number of logs minimal while achieving this goal.

## Method Entry/Exit Logging

For logged methods, include **entry and exit logs**:

- **Entry log**: Starts with `*`, captures input arguments
- **Exit log**: Starts with `$`, captures return values

> **Note:** The `*` and `$` prefixes are team conventions, not enforced by CLogger.

```csharp
public async Task<Result> ConnectAsync(IPAddress address, int port)
{
    CLogger.LogInfo($"* {nameof(address)}='{address}', {nameof(port)}={port}");
    
    // Method body...
    
    CLogger.LogSuccess("$ Connection established");
    return result;
}
```

### Exit Log Variations

```csharp
// Exit with tag
if (this.disposedValue)
{
    CLogger.LogInfo("$<ALREADY_DISPOSED>");
    return;
}

// Exit with return value
CLogger.LogSuccess($"$='{result}'");
return result;

// Exit with tag and value
CLogger.LogWarning($"$<INVALID_STATE>='{result}'");
return result;
```

## ToString Override

All logged classes should override `ToString()` for debugging:

```csharp
public class Bullet
{
    public string name;
    public float damage;

    public override string ToString()
    {
        return $"Name: {name}, Damage: {damage}";
    }
}
```

## Exception Logging

Always log exceptions with full stack traces:

```csharp
try
{
    // Operation
}
catch (Exception ex)
{
    CLogger.LogError($"Operation failed: {ex.Message}\nStack: {ex.StackTrace}");
    throw;
}
```

### Performance

Avoid logging in tight loops or performance-critical paths. Use conditional logging for high-frequency events.
