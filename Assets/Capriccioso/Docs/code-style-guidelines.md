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
  * [Avoid exceptions, use Result](#avoid-exceptions-use-result)
  * [Code width](#code-width)
  * [Hardcoded constants](#hardcoded-constants)
  * [Locked suffix](#locked-suffix)
  * [Dedicated locks](#dedicated-locks)
  * [Identifiers with units](#identifiers-with-units)
  * [Update non-compliant code](#update-non-compliant-code)
  * [Is for null checking](#is-for-null-checking)
  * [Separated assignments](#separated-assignments)
  * [Multiple method exits](#multiple-method-exits)
  * [No fire and forget](#no-fire-and-forget)

## Consistency

The optimal state is globally consistent source code. This means the style of the code in one part of the project is the same as in all other parts. When possible, **we aim for global consistency**. In some cases, that must be properly justified, the global consistency may be impractical and there we aim to achieve at least a local consistency. This means that within the specific part of the project where global consistency is not used, the code style should be consistent. If you are not sure what code style is used, because, for example, it is not explicitly defined below, try to find a similar code that exists already and be consistent with it.


## Readability

The code should be primarily **optimized for reading**. Avoid using complex language features or complex library calls. For example, avoid using LINQ in the production code unless it is trivial. Avoid Fluent style of calls in the production code.


## Formatting

We **use the default formatting that MSVS has set**. This allows us to use the automatic formatting of code provided by the IDE. This includes 4 spaces indentation for C# sources, [Allman style braces](https://en.wikipedia.org/wiki/Indentation_style#Allman_style), spacing around brackets, and many other rules.


## Analyzers

**We use `.editorconfig`** to prescribe a lot of our code style, such as using `this.`, empty lines, placement of namespaces, `Async` suffix, etc.


## No compilation warnings

Building a project must **never produce any warnings**. Messages are OK to have and they are considered on case-by-case basis. In cases of malfunctioning analyzers we allow using `SuppressMessageAttribute`. We also use suppressions in case of generated code. 

Exception to this rule can be a bootstrapping of a new project or a part of a code that uses a large chunk of third party code that is still under heavy development. In this case, it is **temporarily** allowed for the compiler to produce warnings. Eventually the warnings must be removed, however.


## Character casing

We **use `PascalCase` for all public identifiers and for all constants and methods**, including private and local methods and private constants. We use **`camelCase` for everything else**, including local constants. We try to avoid using local constants as much as possible.

**Examples:**
```csharp
private readonly Logger log = LogManager.GetCurrentClassLogger();

public const string ErrorProcessIsAlreadyRunning = nameof(CoreManager) + "_" + nameof(ErrorProcessIsAlreadyRunning);

private const int CoreStopTimeoutMilliseconds = 15_000;

private readonly bool passDataDir;

public override async Task<Result<bool, string>> InitializeAsync()

public async Task ReconnectingScenarioAsync()
{
    //- Local consts are written using camel case.
    const string listenKey = "user-data-stream-key";
```


## Access modifiers

We **use the least privilege principle** at all times. This includes using `readonly` as much as possible. However, this does not necessarily contradicts using a higher than theoretically least necessary access to an element if the access helps simplification of the code elsewhere. We also always specify the access modifiers and never rely on defaults.


## Parentheses

We **use parentheses for clarity for all binary operators** including `is` operator.

**Examples:**
```csharp
bool result = (this.coreInstance is not null) && this.coreInstance.IsRunning;

if ((i > 0) && ((j == 3) || this.object.Property))

int a = b + (c * d);
```


## Third party code

We **only use third party code when its license is known and it allows free commercial use**. We never simply copy and paste third party code into our code base but **always reformat it and adjust it** so that it complies with our code style guidelines, logging guidelines, and commenting guidelines.


## Initialization

All **non-static members should be initialized in the constructor**, except for the instance logger. All **static members should be initialized outside of the constructor**.

**Examples:**
```csharp
//- Logger is an exception.
private readonly Logger log = LogManager.GetCurrentClassLogger();

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


## Property getters and setters

We **write getters and setters without explicit body on a single line**. We **write getters and setters with body, on multiple lines**.

**Examples:**
```csharp
public SupportedFeatures SupportedFeatures { get; private set; }

public string? Host
{
    get => this.host ?? string.Empty;
    private set => this.SetValue(nameof(this.Host), ref this.host, value ?? string.Empty);
}
```


## Expression body

We only, but optionally, use expression bodies in case of direct assignment or a single operator or a single short call. In general we prefer not using expression bodies and only if the readability is improved, we may decide to use it.

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
this.log.Debug("* {0}='{1}',{2}='{3}'", nameof(appDataFolder), appDataFolder, nameof(passDataDirToCore), passDataDirToCore);
```


## Class order

In the class we **first write constants, fields and properties** (including those with complex getters and setters), **followed by constructors** (order from the less complex to more complex), **followed by static methods**, **followed by all other methods**. We tend to put common methods, such as `ToString`, `Dispose`, at the end of the class. We tend to put private methods close to the code that uses them.

**Examples:**
```csharp
public class UiActionExecutor: Component, IDisposable
{
    //- First constants, fields, properties.
    private readonly Logger log = LogManager.GetCurrentClassLogger();

    public const string ErrorShutdownInProgress = nameof(UiActionExecutor) + "_" + nameof(ErrorShutdownInProgress);

    private readonly JoinableTaskFactory joinableTaskFactory;

    private readonly object lockObject;

    private readonly Queue<PendingAction> uiQueue;

    //- Then constructors, simpler first.
    public UiActionExecutor() :
        base()
    {
        //- ...
    }

    public UiActionExecutor(JoinableTaskFactory joinableTaskFactory, Shutdown parentShutdown) :
        base(parentShutdown, nameof(UiActionExecutor))
    {
        //- ...
    }

    //- Then static methods.
    public static void Run()
    {
        //- ...
    }

    //- Then non-static methods.
    public override Task<Result<bool, string>> InitializeAsync()
    {
        //- ...
    }

    //- Standard methods such as Dispose tend to be at the end.
    protected virtual void Dispose(bool disposing)
    {
        //- ...
    }

    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
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

## Avoid exceptions, use Result

In all our internal code (code that is not supposed to be consumed by users), **we do not throw exceptions** ourselves. Instead, we use `Result`, `Result<TResult>`, or `Result<TResult, TErrorDetails>` as method return types to inform method callers about failures. The only exception to this rule in internal code is the `SanityCheckException` which we use to signal a fatal condition in the code flow that should have never occurred. Getting `SanityCheckException` should always mean that there is a bug in the code that must be fixed. In user facing API code, however, we use exceptions to inform method callers about error conditions. When our code calls third party code which may throw exceptions, we always catch all possible exceptions on the lowest level and do not let exceptions to be propagated up.

The three `Result` structures are used as follows. Non-generic simple `Result` is intended for methods that do not return any value, but they can fail. `Result<TResult>` is intended for methods that return a value of `TResult` type if they execute successfully, but they can fail too. `Result<TResult, TErrorDetails>` is intended for methods that return a value of `TResult` type if they execute successfully, but they can fail, and the failure description (i.e. `TErrorDetails`) is important to communicate to the user or the method caller. In this case, the error details usually is a `string` message that returns a human readable detailed description of what happened.

In all these methods where we use one of the `Result*` structures, instead of failing with an exception, we create custom error codes in the class (or interface) that declares such a method, unless the method can only fail due to calls to other methods where the dedicated error codes are already defined.

**Example:**
```csharp
/// <summary>Error indicating that an invalid value was provided for a command line option.</summary>
public const string ErrorInvalidCommandLineValue = nameof(Program) + "_" + nameof(ErrorInvalidCommandLineValue);
```

In this example, we have an error code defined in the `Program` class. Note the special construction of the constant. It starts with a prefix of the class name that introduces it, followed by underscore, followed by the name of the error identifier. Then we can write code like this in a method:

**Example:**
```csharp
result = Result<bool, string>.Error(ErrorInvalidCommandLineValue, "Some message here.");
```

It is perfectly fine to re-use an error code from a method of the same class for a similar failure in different method in the same class. In general, we do not create a special error code for every error, but rather we create a single error code for a single type of error. Let's say we have a method that validates arguments, we do not create error code for each invalid argument, instead we create one `ErrorInvalidArgument` code for all such instances. Then we create a second error code `ErrorArgumentMissing` for cases where a mandatory argument was not given, but again, not one error for each argument.



When a method returns any of the `Result*` structures, we document its return value in a special way.

**Example:**
```csharp
/// <returns>
/// On success: Returns list of all script names of dependencies of the given script.
/// <para><see cref="ScriptDependenciesBase.ErrorDependenciesFileNotFound"/>: Returned if the dependencies file does not exist.</para>
/// <para><see cref="ScriptDependenciesBase.ErrorDependenciesFileInvalid"/>: Returned if the dependencies file can not be read or has invalid format.</para>
/// <para><see cref="ErrorInDependencyNotFound"/>: Returned if the dependencies file contains a dependency that can not be found.</para>
/// </returns>
```

We start with what happens when the function succeeds followed by enumeration of each error code in a separate paragraph. Note that in this example only the last error `ErrorInDependencyNotFound` belongs to the class with the method in question. The other two errors codes originate in the class whose methods are called from the method in question. This is how we propagate error codes that can originate deep in the code flow. At the bottom of the code flow a method usually returns only a few different error codes, but as we go to higher levels of the code flow, the methods may return a large number of error codes, all of which should be documented if possible.

Not always it is possible to document all errors. This happens, for example, when we receive a result over a network from a component that we do not have access to. In such a case it is OK to include information about that as the last paragraph of the `<returns>` sections:

**Example:**
```csharp
/// <returns>
/// On success: Returns <c>true</c>.
/// <para><see cref="ErrorFileSystem"/>: Returned if a package file can not be read.</para>
//- ...
/// <para>Other errors returned by the server can also be returned.</para>
/// </returns>
```

In case the result is part of a structure or a class, usually a definition of a network message, we use similar syntax to describe its possible values.

**Example:**
```csharp
/// <summary>
/// Result of the registration process.
/// <para>On success: Set to <see cref="Result.Ok"/>.</para>
/// <para><see cref="SessionResponseMessageBase.ErrorSessionNotFound"/>: Set if the given session token was not recognized.</para>
/// <para><see cref="ErrorUserAlreadyExists"/>: Set if the given username already exists in the database.</para>
/// <para><see cref="AccountRegisterRequest.ErrorUsernameEmpty"/>: Set if an empty user name was used.</para>
/// </summary>
public Result RegistrationResult { get; set; }
```



## Code width

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
this.log.Debug("* {0}.{1}='{2}',{3}={4},{5}={6}", nameof(userAccount), nameof(userAccount.Username), userAccount.Username, nameof(expirationTimeSeconds), expirationTimeSeconds, nameof(defaultExtensionTimeSeconds), defaultExtensionTimeSeconds);
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


## Separated assignments

We **write assignments on separated lines**. Do not combine assigning to a variable with a condition.


**Wrong Example:**
```
string? line = null;

while ((line = streamReader.ReadLine()) is not null)
{
//- ...
}
```

**Good Example:**
```
string? line = streamReader.ReadLine();

while (line is not null)
{
//- ...
    line = streamReader.ReadLine();
}
```

## Multiple method exits

A method can be exited via `return` or `throw`. We [use exceptions very rarely](#avoid-exceptions-use-result) and they should always signal that the method failed. Returns, however, can be used sometimes for failure exits and sometimes for successful exits.

Always **think if it is easily possible to introduce only a single exit from a method**. If we can structure the code of the method in a way it only has a single exit, often the code is easier to understand for someone who reads it later over a code that has multiple exit points. Exception to this is guards that are put at the very beginning of the method body. Therefore, try to either exit the method early, or only at the end. Not always this is easily possible and then it is perfectly acceptable to have multiple exits from a method, but often it needs not too much effort to remove multiple exit points from the middle of the method's body.

**If there are multiple exits from the method, put the successful one at the very end of the method.** Exception here would be very short trivial methods.


**Examples:**
```csharp
//- Example of a method with a guard.
internal virtual async Task<Result<RunningInvocationTask, string>> StartInvocationTaskAsync(ScriptIdentifier scriptIdentifier, string interfaceName, string methodName, byte[] arguments, SessionData? sessionData, bool isCancellable = false, uint instanceId = 0)
{
    this.log.Debug("* {0}='{1}',{2}='{3}',{4}='{5}',|{6}|={7},{8}={9},{10}={11:X8}", nameof(scriptIdentifier), scriptIdentifier, nameof(interfaceName), interfaceName, nameof(methodName), methodName, nameof(arguments), arguments.Length, nameof(isCancellable), isCancellable, nameof(instanceId), instanceId);

    Result<RunningInvocationTask, string> result;

    //- This is a guard, it comes immediately after the entry log.
    Result<IDisposable> protectorResult = this.executionCounter.Enter();
    if (!protectorResult)
    {
        result = Result.FromError(protectorResult);
        this.log.Debug("$<SHUTDOWN>='{0}'", result);
        return result;
    }

    //- ...

    //- Successful exit is at the end.
    this.log.Debug("$='{0}'", result);
    return result;
}
```

**Wrong Example:**
```
//- In this example, the successful result is not at the end of the method and also this method can easily be rewritten to only have a single exit.
public Result<bool, string> X()
{
    //- ...
    try
    {
        //- ...
        result = true;
        this.log.Debug("$='{0}'", result);
        return result;
    }
    catch (Exception e)
    {
        this.log.Error("Enumerating all objects in the database failed with exception: {0}", e.ToString());

        result = Result<bool, string>.Error(ErrorDatabaseUnknown, "Enumerating all objects in the database failed.");
        this.log.Debug("$<EXCEPTION>='{0}'", result);
        return result;
    }
}
```

**Good Example:**
```
//- Fixed the above example. The exit was moved to the very end and we used it for both the successful and the unsuccessful case.
public Result<bool, string> X()
{
    //- ...
    try
    {
        //- ...
        result = true;
    }
    catch (Exception e)
    {
        this.log.Error("Enumerating all objects in the database failed with exception: {0}", e.ToString());
        result = Result<bool, string>.Error(ErrorDatabaseUnknown, "Enumerating all objects in the database failed.");
    }

    this.log.Debug("$='{0}'", result);
    return result;
}
```

## No fire and forget

We **never call async method in fire and forget fashion.** When we are calling an `async` method we always want to await the `Task` it returns. Not always we can await it at the point of the method call, but we should never discard the returning task and forget about it. The component that creates such background running tasks should always make sure that all the background tasks are finished before the component is fully disposed.

Await the task at different point of the code in which it was not created often requires use of joinable task factory, for which we introduce our own interface `Xarcade.SharedLib.Utils.Sync.IJoinableTaskFactory`.