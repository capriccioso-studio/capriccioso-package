# Logging Guidelines

This file discusses how do we insert logging into the code and how do we format the log messages in order to make it easy to track the code flow in log files.

## Table of Contents

  * [Classes](#classes)
  * [Method start/end logging](#method-startend-logging)
  * [Values format](#values-format)
  * [Bounded logs](#bounded-logs)
  * [Performance](#performance)
  * [Exceptions](#exceptions)
  * [Method body logging](#method-body-logging)
  * [Log coverage](#log-coverage)
  * [MDLC](#mdlc)
  * [Log levels](#log-levels)
  * [Log file location](#log-file-location)

## Classes

With each class the developer must **decide whether or not the class should be logged**. If a class does not contain any logic, there is no reason to log it. If a class contains just a small amount of logic and its methods are short and simple, it may not need logging. All other classes should be logged. **Static classes use static loggers, non-static classes use instance loggers**. Sometimes a non-static class contains a complex static method, in which case it may contain both a static and a non-static logger. 

Classes should have an override for ToString to help with Sentry debugging 

**Examples:**
```csharp
//- In case both static and non-static loggers are present, class logger is called "clog".
public class Bullet
{
    public string name;
    public float damage;

    public override string ToString()
    {
        base.ToString();
        return $"Name : {name} \n" +
                "damage : {damage}";
    }

```

## Method start/end logging

In a class with a logger, the developer must **decide for each method whether it is to be logged or not**. **If it is logged, the method must contain so called entry and exit logs**.

There is always at most one entry log at the start of the method. Most of the time it is the first line of the method body. Its role is to capture the input that goes into the method. Analogically, the exit's log purpose is to capture method's return values and inform about exiting the method.

The entry log's syntax is as follows. The formatting string starts with `* ` and then continues with a list of method input arguments, each represented by `name=value` string. Name of the argument is simply obtained using `nameof`. Value format will be explained below.

**Example:**
```csharp
public async Task<Result> ConnectAsync(IPAddress address, int port)
{
    this.log.Debug("* {0}='{1}',{2}={3}", nameof(address), address, nameof(port), port);

```

Note that there is an empty line after the entry log.

The exit log starts with `$`, optionally followed by a tag and/or list of outputs. In case of a method with multiple different exit points (`return` or `throw`), the tag is used to differentiate among them. The default exit point (usually the one where the method ends successfully) is written without the tag. The tag is written as `<UPPER_CASE_TAG_NAME>`.

In case the method returns a value, `=value` follows immediately after the `$` sign or the tag. In case the method has output parameters, they are logged after the return value (if present) using the same syntax as input arguments are logged.

We usually have an empty line before the exit log line. Exceptions are when the exit log line is at the very start of the block, in which case there must not be an empty line; or when there is a single assignment of the return value which is by itself at the start of the block or preceded with an empty line.

**Examples:**
```csharp
//- Exit log with a tag. No empty line before the exit log here because of the start of the block.
if (this.disposedValue)
{
    this.log.Debug("$<ALREADY_DISPOSED>");
    return;
}


//- Exit log with a return value. No empty line right before the exit log here as there is the result assignment which itself is preceded by an empty line.
    //- ...

    result = true;
    this.log.Debug("$='{0}'", result);
    return result;
}

//- Exit log with a tag and a return value.
    //- ...
    this.log.Debug("$<INVALID_RPC_INTERFACE>='{0}'", result);
    return result;
}

//- Exit log with a return value and an out parameter.
private bool TryParseRunningTaskId(string taskIdStr, [NotNullWhen(true)] out RunningTaskId? runningTaskId)

    //- ...

    if (runningTaskId is not null) this.log.Debug("$={0},{1}='{2}'", result, nameof(runningTaskId), runningTaskId);
    else this.log.Debug("$={0}", result);
    return result;
}

//- Example of a method with multiple exit points, where the default one does not have the tag.
    //- ...
    
    Result<AccountRegisterResponse> result = Result.Error(ErrorNotConnected);
    if (!this.Connected || (this.requestManager is null))
    {
        this.log.Debug("$<NOT_CONNECTED>='{0}'", result);
        return result;
    }
    //- ...

    this.log.Debug("$='{0}'", result);
    return result;
}
```

## Values format

Logging of values (including values of parameters and return values) has several possible formats.

Immediate values such as numbers, enumerations, booleans, instances of `TimeSpan` or `DateTime`, etc. are logged simply as the raw values with additional syntax: `value`.

**Example:**
```csharp
//- Boolean value is logged without additional formatting.

bool result = false;

//- ...

this.log.Debug("$={0}", result);
```

Strings, characters, instances of classes with more complex `ToString` outputs (such as `IPAddress`) are logged as `'value'`.

**Examples:**
```csharp
//- More complex values use "'".
Result result = await sender.SendResponseAsync(startScriptResponse, requestMessage, cancellationToken).ConfigureAwait(false);

this.log.Debug("$='{0}'", result);


//- Another example of more complex value.
private async Task<Result> ProcessStopScriptRequestAsync(IncomingTcpClient sender, RequestMessageBase requestMessage, CancellationToken cancellationToken)
{
    this.log.Debug("* {0}='{1}'", nameof(sender), sender);


//- Strings also use "'".
public App(string appDataFolder, bool passDataDirToCore, ParsedCommandLine parsedCommandLine)
{
    this.log.Debug("* {0}='{1}',{2}={3}", nameof(appDataFolder), appDataFolder, nameof(passDataDirToCore), passDataDirToCore);
```

In case of lists and arrays, sometimes only the number of their elements is logged, using `|value|` syntax, and sometimes (in case of small sets of strings or numbers) whole contents are logged using `LogJoin` extension method. Note that using `LogJoin` method requires extra logging level condition before the log itself (see rule 5 below).

**Examples:**
```csharp
//- Using LogJoin for array of strings with logging level condtion.
public static void Main(string[] args)
{
    if (log.IsInfoEnabled)
        log.Info("* {0}={1}", nameof(args), args.LogJoin());


//- Logging number of elements of an array uses "|value|" syntax.
public virtual Result<bool, string> InstallScript(DatabaseId accountId, byte[] scriptPackageBytes, bool deleteConflicting)
{
    this.log.Debug("* {0}='{1}',|{2}|={3},{4}={5}", nameof(accountId), accountId, nameof(scriptPackageBytes), scriptPackageBytes.Length, nameof(deleteConflicting), deleteConflicting);
```


## Bounded logs

**When logging potentially large objects we always want to make sure that the log itself is bounded**. To guarantee that we use two extension methods `BoundedString` and `ToBoundedString`. Use `BoundedString` to limit the length of a string, whereas `ToBoundedString` is available for all objects and it first calls their own `ToString` followed by `BoundedString` which limits the output.

**Examples:**
```csharp
//- Using "BoundedString" for untrusted user input parameter.
if (this.log.IsDebugEnabled)
    this.log.Debug("* {0}='{1}'", nameof(taskIdStr), taskIdStr.BoundedString());


//- Using `ToBoundedString` to make sure the resulting string is not too long.
return string.Format(
    "[{0}={1},{2}=`{3}`,{4}=`{5}`]",
    nameof(this.Id), this.Id,
    nameof(this.SessionToken), this.SessionToken.BoundedString(),
    nameof(this.ClientScriptIdentifier), this.ClientScriptIdentifier.ToBoundedString()
);
```


## Performance

While in other code we usually only aim for asymptotic optimality, in case of logs, we want to **make sure that logs, which are not enabled by the current logging settings, do not consume any unnecessary resources**. This has two consequences. First, we avoid using interpolated strings in logs. Second, in case a parameter to the log string formatting is a call to a method, such as `BoundedString`, `ToBoundedString`, `LogJoin`, etc., the whole log line needs to executed only if the logging is enabled on the given logging level. The only exception is logging of exceptions in catch clause.

**Examples:**
```csharp
//- Using "BoundedString" requires logging level condition in front.
if (this.log.IsDebugEnabled)
    this.log.Debug("* {0}='{1}'", nameof(taskIdStr), taskIdStr.BoundedString());


//- Using `LogJoin` requires logging level condition in front.
if (this.log.IsInfoEnabled)
    this.log.Info("* {0}={1}", nameof(args), args.LogJoin());


//- Exception logging is special.
this.log.Error("Exception occurred while attempting to connect to pipes: {0}", e.ToString());
```


## Exceptions

When an exception is caught and logged, we convert the exception to string explicitly.

**Example:**
```csharp
this.log.Error("Exception occurred while attempting to connect to pipes: {0}", e.ToString());
```


## Method body logging

Entry and exit logs of methods have special format as described above. Other **logs that appear inside of body of the method should be written as if they were code comments**. This means proper English sentences should be used. We try to avoid using `nameof` in these logs. This helps with readability of the logs as well as it helps reducing the amount of comments in the code where often a well explained log line servers the purpose of the comment and the comment is omitted.

**Examples:**
```csharp
//- This log serves also as a comment.
this.log.Debug("Delete old events when all events are new and the count is small. Nothing should be deleted.");
{
    this.AddEventsAndAssert(storage);

    Result<int> deleteOldResult = storage.DeleteOldEventsBatch();
    AssertResult.TrueGetValue(deleteOldResult, out int deletedCount);
    Assert.Equal(0, deletedCount);

    // Since nothing was deleted, all events should be there.
    List<DatabaseScriptEvent> allRemovedEventsResult = this.ClearStorage(storage);
    Assert.Equal(scriptEvents.Length, allRemovedEventsResult.Count);
}


//- A proper English sentence is used for in-body comments as opposed to something like this.log.Debug("{0}={1}", nameof(this.storageFolder), this.storageFolder).
this.storageFolder = FileUtils.NormalizeFullPath(Path.Combine(appDataFolder, UserStorageFolderName));
this.log.Debug("Storage folder set to '{0}'.", this.storageFolder);
```


## Log coverage

We **try to write logs so that the log file can be used to trace the flow of the execution**. This means that we try to put logs at places that are crucial for the program to make the decision where to go next. Thus we want to log information related to branching, number of loops taken, etc. At the same time, however, we try to keep the number of logs as small as possible to achieve that goal.


## MDLC

NLog supports so called Mapped Diagnostics Logical Context, which allows tracking execution flow of the code in async environment. Whenever the code starts a new separated thread or a background task (that is not awaited), we use `SetMdlc` extension method to separate the task from others. `SetMdlc` should be the first code the new tasks executes, thus it is inserted even in front of the method entry log.

**Example:**
```csharp
//- This code starts a new background task.
JoinableTask clientTask = this.joinableTaskFactory.RunAsync(async () => await this.ClientLifetimeAsync(client).ConfigureAwait(false), JoinableTaskCreationOptions.LongRunning);

//- Then the task itself starts with "SetMdlc" call, which is put even in front of the method entry log.
private async Task ClientLifetimeAsync(IncomingTcpClient client)
{
    this.log.SetMdlc();
    this.log.Trace("* {0}='{1}'", nameof(client), client);
```

In tests we use a special variant of `SetMdlc` call which assigns the new context a custom name. The name of the test method is used as the context name.

**Example:**
```csharp
[Fact]
public void VerifyPassword()
{
    //- New MDLC context is created as the first thing in the test method and the test's name is used as the context's name.
    this.log.SetMdlc(nameof(this.VerifyPassword));
    this.log.Debug("*");
```

## Log levels

There are six log levels in NLog and we **use them in following situations**:

* Trace &ndash; Very detailed information that a developer usually wants to see only in case of actively debugging or developing the code in which the log is. Also used to trace a flow of methods that are called very frequently (i.e. many times per second).
* Debug &ndash; Useful logs to trace the flow of the code. From logs on this level, one should be able to understand what and why something happened in the program.
* Info &ndash; Important information that is often also reported to the user of the program.
* Warn &ndash; Use this log level when the program reached a condition that is unusual, but does not immediately indicate an error.
* Error &ndash; Error is something that should not happen and it means the program could not do what it was supposed to do. Often the program should be able to, at least partially, recover from an error.
* Fatal &ndash; Fatal error means that the program can not recover from a problem and can not continue to operate.

These levels are ordered. This is useful because we can set the configuration of NLog only to only log on certain level and higher. For example if we set minimum log level to `Info`, we would only see logs logged on levels `Info`, `Warn`, `Error`, and `Fatal`.

## Log file location

With the default `NLog.config` file we use, **the logs are stored in `logs` folder under the folder which contains the executed assembly**.

