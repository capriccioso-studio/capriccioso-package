# Logging Guidelines

This file discusses how do we insert logging into the code and how do we format the log messages in order to make it easy to track the code flow in log files.

## Table of Contents

  * [CLogger](#classes)
  * [Classes](#classes)
  * [Log coverage](#log-coverage)
  * [Method start/end logging](#method-startend-logging)
  * [Performance](#performance)
  * [Exceptions](#exceptions)
  * [Log levels](#log-levels)

## CLogger
    We use Capriccioso's Logger `CLogger` for colored and instance-tracking logs. CLogger is a service that can be called globally and log globally. We use the following colors (inspired by Bootstrap colors)

### LOGG - white, indicates a normal log.
### INFO - blue, used to indicate the start of notable events.
### GOOD - green, used to indicate a successful operation. Notably used for successful backend/API calls
### WARN - yellow, used to indicate a non-ideal but non-problematic code path
### ERRR - red, used to indicate errors/exceptions thrown
### INFO - purple, used to log the content of the entire class   

## Log coverage

We **try to write logs so that the log file can be used to trace the flow of the execution**. This means that we try to put logs at places that are crucial for the program to make the decision where to go next. Thus we want to log information related to branching, number of loops taken, etc. At the same time, however, we try to keep the number of logs as small as possible to achieve that goal.

## Classes

With each class the developer must **decide whether or not the class should be logged**. If a class does not contain any logic, there is no reason to log it. If a class contains just a small amount of logic and its methods are short and simple, it may not need logging. All other classes should be logged. **Static classes use static loggers, non-static classes use instance loggers**. Sometimes a non-static class contains a complex static method, in which case it may contain both a static and a non-static logger. 

Classes should have an override for `ToString()` to help with Sentry debugging 

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
    CLogger.Instance.Debug("* {0}='{1}',{2}={3}", nameof(address), address, nameof(port), port);

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
    CLogger.Instance.Debug("$<ALREADY_DISPOSED>");
    return;
}


//- Exit log with a return value. No empty line right before the exit log here as there is the result assignment which itself is preceded by an empty line.
    //- ...

    result = true;
    CLogger.Instance.Debug("$='{0}'", result);
    return result;
}

//- Exit log with a tag and a return value.
    //- ...
    CLogger.Instance.Debug("$<INVALID_RPC_INTERFACE>='{0}'", result);
    return result;
}

//- Exit log with a return value and an out parameter.
private bool TryParseRunningTaskId(string taskIdStr, [NotNullWhen(true)] out RunningTaskId? runningTaskId)

    //- ...

    if (runningTaskId is not null) CLogger.Instance.Debug("$={0},{1}='{2}'", result, nameof(runningTaskId), runningTaskId);
    else CLogger.Instance.Debug("$={0}", result);
    return result;
}

//- Example of a method with multiple exit points, where the default one does not have the tag.
    //- ...
    
    Result<AccountRegisterResponse> result = Result.Error(ErrorNotConnected);
    if (!this.Connected || (this.requestManager is null))
    {
        CLogger.Instance.Debug("$<NOT_CONNECTED>='{0}'", result);
        return result;
    }
    //- ...

    CLogger.Instance.Debug("$='{0}'", result);
    return result;
}
```


## Performance

While in other code we usually only aim for asymptotic optimality, in case of logs, we want to **make sure that logs, which are not enabled by the current logging settings, do not consume any unnecessary resources**. This has two consequences. First, we avoid using interpolated strings in logs. Second, in case a parameter to the log string formatting is a call to a method, such as `BoundedString`, `ToBoundedString`, `LogJoin`, etc., the whole log line needs to executed only if the logging is enabled on the given logging level. The only exception is logging of exceptions in catch clause.

**Examples:**
```csharp
//- Using "BoundedString" requires logging level condition in front.
if (CLogger.Instance.IsDebugEnabled)
    CLogger.Instance.Debug("* {0}='{1}'", nameof(taskIdStr), taskIdStr.BoundedString());


//- Using `LogJoin` requires logging level condition in front.
if (CLogger.Instance.IsInfoEnabled)
    CLogger.Instance.Info("* {0}={1}", nameof(args), args.LogJoin());


//- Exception logging is special.
CLogger.Instance.Error("Exception occurred while attempting to connect to pipes: {0}", e.ToString());
```


## Exceptions

When an exception is caught and logged, we convert the exception to string explicitly.

**Example:**
```csharp
CLogger.Instance.Error("Exception occurred while attempting to connect to pipes: {0}", e.ToString());
```
