# Code Style Guidelines

This file discusses which style do we use to write code.


## Table of Contents

  * [Consistency](#consistency)
  * [Readability](#readability)
  * [Formatting](#formatting)
  * [Analyzers](#analyzers)
  * [No compilation warnings](#no-compilation-warnings)
  * [Character casing](#character-casing)
  * [Access modifiers](#access-modifiers)
  * [Parentheses](#parentheses)
  * [Third party code](#third-party-code)
  * [Initialization](#initialization)
  * [Property getters and setters](#property-getters-and-setters)
  * [Expression body](#expression-body)
  * [nameof](#nameof)
  * [Class order](#class-order)
  * [C#10 with nullable](#c10-with-nullable)
  * [var](#var)
  * [One file per type](#one-file-per-type)
  * [Internal for testing](#internal-for-testing)
  * [Explicit names of parameters](#explicit-names-of-parameters)
  * [Code width](#code-width)
  * [Hardcoded constants](#hardcoded-constants)
  * [Locked suffix](#locked-suffix)
  * [Dedicated locks](#dedicated-locks)
  * [Identifiers with units](#identifiers-with-units)
  * [Update non-compliant code](#update-non-compliant-code)
  * [Is for null checking](#is-for-null-checking)
  * [No fire and forget](#no-fire-and-forget)

## Consistency

The optimal state is globally consistent source code. This means the style of the code in one part of the project is the same as in all other parts. When possible, **we aim for global consistency**. In some cases, that must be properly justified, the global consistency may be impractical and there we aim to achieve at least a local consistency. This means that within the specific part of the project where global consistency is not used, the code style should be consistent. If you are not sure what code style is used, because, for example, it is not explicitly defined below, try to find a similar code that exists already and be consistent with it.


## Readability

The code should be primarily **optimized for reading**. Avoid using complex language features or complex library calls. For example, avoid using **LINQ** in the production code unless it is trivial. Avoid Fluent style of calls in the production code.


## Formatting

We **use the default formatting that MSVS has set**. This allows us to use the automatic formatting of code provided by the IDE. This includes 4 spaces indentation for C# sources, [Allman style braces](https://en.wikipedia.org/wiki/Indentation_style#Allman_style), spacing around brackets, and many other rules.


## Analyzers

**We use `.editorconfig`** to prescribe a lot of our code style, such as using `this.`, empty lines, placement of namespaces, `Async` suffix, etc.


## No compilation warnings

Building a project must **never produce any warnings**. Messages are OK to have and they are considered on case-by-case basis. In cases of malfunctioning analyzers we allow using `SuppressMessageAttribute`. We also use suppressions in case of generated code. 

Exception to this rule can be a bootstrapping of a new project or a part of a code that uses a large chunk of third party code that is still under heavy development. In this case, it is **temporarily** allowed for the compiler to produce warnings. Eventually the warnings must be removed, however.


## Character casing

We **use `PascalCase` for all public identifiers and for all constants and methods**, including private and local methods and private constants. We use **`camelCase` for everything else**. We try to avoid using local constants as much as possible.

**Examples:**
```csharp
private readonly CapLog log = LogManager.GetCurrentClassLogger();

public const string ErrorProcessIsAlreadyRunning = nameof(CoreManager) + "_" + nameof(ErrorProcessIsAlreadyRunning);

private const int CoreStopTimeoutMilliseconds = 15_000;

private readonly bool _passDataDir; //use an underscore for private fields

private bool _hasMoved; 

[SerializeField] private string _capricciosoName; //If you want these to be shown on the inspector, use serializefield instead of public

public override async Task<Result<bool, string>> InitializeAsync()

public async Task ReconnectingScenarioAsync()
{
    //- Local consts are written using camel case.
    const string listenKey = "user-data-stream-key";
```


## Access modifiers

We **use the least privilege principle** at all times. This includes using `readonly` as much as possible. However, this does not necessarily contradicts using a higher than theoretically least necessary access to an element if the access helps simplification of the code elsewhere. We also always specify the access modifiers and never rely on defaults.


## Parentheses

We **use parentheses for clarity for all binary operators** including `is` and `is not` operator.

**Examples:**
```csharp
bool result = (this.coreInstance is not null) && this.coreInstance.IsRunning;

if ((i > 0) && ((j == 3) || this.object.Property))

int a = b + (c * d);
```


## Third party code

We **only use third party code when its license is known and it allows free commercial use**. We never simply copy and paste third party code into our code base but **always reformat it and adjust it** so that it complies with our code style guidelines, logging guidelines, and commenting guidelines.

Third-party Unity libraries/plugins should not be edited, only extended


## Initialization

All **non-static members should be initialized in the constructor**, except for the instance logger. All **static members should be initialized outside of the constructor**.

**Examples:**
```csharp

//- Static initialization outside the constructor.
private static readonly JsonSerializerOptions serializationOptions = new()
{
    AllowTrailingCommas = true,
    IgnoreReadOnlyFields = true,
    IgnoreReadOnlyProperties = true,
    IncludeFields = false,
    NumberHandling = JsonNumberHandling.Strict,
    PropertyNameCaseInsensitive = false,
    ReadCommentHandling = JsonCommentHandling.Skip,
    WriteIndented = true,
};

//- Non-static initializatiossn inside the constaructor.
private readonly object mapLock;

public ClassConstructor()
{
    //- ...
    this.mapLock = new();
    //- ...
}
```

## Service-Handler relationship

### Services 
    are plug-and-play collection of functions that derive from `MonoSingleton`. Services should be developed as modules that can be called from anywhere and usable anywhere. Accessing services should always be logged. Aside from the Logger, the names should have a `Service` suffix. Example: `RegenerationService.cs`

### Handlers
    are what calls the services and interacts with the actual game or UI. The names should end with `Handler`

## Expression body

We only, but optionally, use expression bodies in case of direct assignment or a single operator or a single short call. In general we prefer not using expression bodies and **only if the readability is improved**, we may decide to use it. 

**Examples:**
```csharp
public static bool operator !=(ScriptIdentifier left, ScriptIdentifier right)
{
    return !(left == right);
}

public bool SupportsRecycling => false;

public bool Match(object data) => data is ViewModelBase;

public string? Host
{
    get => this.host ?? string.Empty;
    private set => this.SetValue(nameof(this.Host), ref this.host, value ?? string.Empty);
}

//- Using expression bodies is optional.
public override int GetHashCode()
{
    return HashCode.Combine(this.AccountId.GetHashCode(), this.PackageName, this.ScriptName, this.DebugMode);
}
```


## nameof

We always use nameof instread of a string literal.

**Example:**
```csharp
CLogger.Instance.Debug("* {0}='{1}',{2}='{3}'", nameof(appDataFolder), appDataFolder, nameof(passDataDirToCore), passDataDirToCore);
```






## C#10 with nullable

We **use C# 10 with enabled nullable** reference types and we **use target-typed new expressions style**. We **put type specifications to the left** side.

**Examples:**
```csharp
//- Nullable.
string? logFile = null;

//- Type is on the left, constructors are called in C# target-typed new expressions style.
DirectoryInfo directoryInfo = new(logFolder);
```

We also use [File Scoped Namespaces](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-10.0/file-scoped-namespaces).

## var

We **never use `var`**.


## One file per type

Almost always we **use one file per type**. Some exceptions are allowed, however. For example, we sometimes put small helper test classes into the same file as the main test class.


## Internal for testing

In some cases it is beneficial to be able **to unit test a non-public part** of a class. For this purpose, we **use `internal` access modifier** in combination with `System.Runtime.CompilerServices.InternalsVisibleTo` assembly attribute.

**Example:**
```xml
//- In project file:
<AssemblyAttribute Include="System.Runtime.CompilerServices.InternalsVisibleTo">
<_Parameter1>Xarcade.CoreUnitTests</_Parameter1>
</AssemblyAttribute>
```

```csharp
//- In code:
internal const string DatabaseFileName = "main.db";

internal virtual Result<ScriptBoxInstance, string> Create(ScriptIdentifier scriptIdentifier, SessionData sessionData)
```


## Explicit names of parameters

We **use explicit parameter naming in calls in case it helps readability**. Usually this happens when the passed parameter identifier or literal does not suggest by itself what the parameter is.

**Examples:**
```csharp
//- "1" could mean anything, so we use "id:" to make it clear what the parameter is.
AccountLogoutRequest accountLogoutRequest = new(this.sessionToken, id: 1);

//- Again, "string.Empty" could mean anything, but when we explicitly say "username:", it is obvious the first argument is a user name.
AccountLoginRequest accountLoginRequest = new(username: string.Empty, encryptedPasswordAlice, id: 1);
```


## Code width

Again, readability over optimization.

We **aim to have visibility of all important code parts** on the screen in IDE. This is why lines of code should not exceed 180 characters. Exceptions are code lines that do not hold any interesting information in the part of the line beyond 180th character. This is common for example for long log lines.

In case of methods with many parameters, we put some, but usually more than one, parameters on the first line and then each following line contains as many parameters as they fit. It is, however, perfectly fine to end the line earlier.

Sometimes, but only when calling a method, it may be more readable to put each method argument separately on the line, in which case we also but the opening parenthesis on a separated line too.

**Examples:**
```csharp
//- It is fine to end the line earlier.
public KucoinRestClientFactory(IDateTimeProvider dateTimeProvider, KucoinSymbolConverter symbolConverter,
    KucoinHttpErrorHandler errorHandler, KucoinNewOrderRequestConverter newOrderRequestConverter,
    JsonDeserializerHelper jsonDeserializerHelper, IHttpClient httpClient)

//- Alternative way of writing method calls with many arguments for improved readability.
return string.Format
(
    CultureInfo.InvariantCulture,
    "[{0}={1},{2}=`{3}`,{4}=`{5}`]",
    nameof(this.Id), this.Id,
    nameof(this.SessionToken), this.SessionToken.BoundedString(),
    nameof(this.PackageScript), this.PackageScript.ToBoundedString()
);

//- It is OK to have a long log on a single line.
CLogger.Instance.Debug("* {0}.{1}='{2}',{3}={4},{5}={6}", nameof(userAccount), nameof(userAccount.Username), userAccount.Username, nameof(expirationTimeSeconds), expirationTimeSeconds, nameof(defaultExtensionTimeSeconds), defaultExtensionTimeSeconds);
```


## Hardcoded constants

Avoid hardcoding constants in the code. Each constant should be properly declared.


## Locked suffix

We use `Locked` suffix for method names which require the caller to acquire at least one lock object before calling the method. If the method is async at the same time, the suffix becomes `LockedAsync`.

**Examples:**
```csharp
//- Normal method that requires a lock.
/// <remarks>The caller is responsible for holding <see cref="lockObject"/>.</remarks>
private void ModifyCountLocked(long difference, bool add)

//- Async method that requires a lock.
/// <remarks>The caller is responsible for holding <see cref="rpcClientLock"/>.</remarks>
private async Task<RecoverStatusAction> CheckClientLoggedInLockedAsync(bool verifyStatus)
```


## Dedicated locks

We **use dedicated objects for locks**. This means we do not use `lock` keyword on objects that also have other, non-locking, purpose.

**Example:**
```csharp
//- A dedicated object is used for locking instead of just locking over "count".
/// <summary>Lock object to protect access to  <see cref="count"/>.</summary>
private readonly object lockObject;

/// <summary>The remaining count on this event.</summary>
/// <remarks>All access has to be protected by <see cref="lockObject"/>.</remarks>
private long count;
```


## Identifiers with units

If a constant, a field, a property, or a variable is supposed to contain a value of measurement we **add a unit suffix to its identifier** unless its type clearly defines the units on itself (e.g. `TimeSpan` variables do not need a unit suffix). We do not use any suffix if we refer to length in characters.

**Examples:**
```csharp
//- Note the "Ms" suffix as the value is in milliseconds. "Milliseconds" suffix would also be OK.
public const int MaxQueueTimeIntervalMs = 30_000;

public const int DefaultMaxScriptFolderSizeKilobytes = 10 * 1024;

public const int MaxSecretSizeBytes = 256;

//- This refers to a maximum length of a string (in characters), so we do not use any units.
public const int MaxKeyLength = 256;
```


## Update non-compliant code

Our code style rules sometimes evolve in time, for example with an introduction of a new C# language feature, so some of our code may not comply with all the rules. When such code is spotted, it is welcome to fix up the code even in a completely unrelated PR. In case there is a pattern of non-compliant code that would need fixing in many places, it is better to use a separate PR.


## Is for null checking

When we want **to check if something is, or is not, null, we use `is` and `is not` operators** instead of `==` and `!=`.


## No fire and forget

We **never call async method in fire and forget fashion.** When we are calling an `async` method we always want to await the `Task` it returns. Not always we can await it at the point of the method call, but we should never discard the returning task and forget about it. The component that creates such background running tasks should always make sure that all the background tasks are finished before the component is fully disposed.

Await the task at different point of the code in which it was not created often requires use of joinable task factory, for which we introduce our own interface `Xarcade.SharedLib.Utils.Sync.IJoinableTaskFactory`.