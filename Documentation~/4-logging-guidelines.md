# Logging Guidelines

This file discusses how do we insert logging into the code and how do we format the log messages in order to make it easy to track the code flow in log files.

## Table of Contents

  * [CLogger](#clogger)
  * [Classes](#classes)
  * [Log coverage](#log-coverage)
  * [Method start/end logging](#method-startend-logging)
  * [Performance](#performance)
  * [Exceptions](#exceptions)
  * [Log levels](#log-levels)

## CLogger

We use Capriccioso's Logger `CLogger` for colored and instance-tracking logs. CLogger is a static utility that can be called globally. We use the following colors (inspired by Bootstrap colors):

### Log Levels

| Level | Color | Usage |
|-------|-------|-------|
| LOGG | Gray (#6C757D) | Normal log messages |
| INFO | Blue (#007BFF) | Start of notable events, state changes |
| GOOD | Green (#28A745) | Successful operations, especially backend/API calls |
| WARN | Yellow (#FFC107) | Non-ideal but non-problematic code paths |
| ERRR | Red (#DC3545) | Errors and exceptions |
| EVENT | Magenta (#FF00EA) | Event triggers and pub/sub |
| DB INFO | Cyan (#99CCFF) | Backend/API informational |
| DB GOOD | Light Green (#98FF39) | Successful backend/API operations |
| DB ERRR | Pink (#FF0066) | Backend/API errors |
| WAH | Bright Yellow (#FFFF00) | Major actions and milestones |
| DATA | Purple (#563D7C) | Data dumps and class content |

### Usage Examples

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

## Log coverage

We **try to write logs so that the log file can be used to trace the flow of the execution**. This means that we try to put logs at places that are crucial for the program to make the decision where to go next. Thus we want to log information related to branching, number of loops taken, etc. At the same time, however, we try to keep the number of logs as small as possible to achieve that goal.

## Classes

With each class the developer must **decide whether or not the class should be logged**. If a class does not contain any logic, there is no reason to log it. If a class contains just a small amount of logic and its methods are short and simple, it may not need logging. All other classes should be logged. **Static classes use static loggers, non-static classes use instance loggers**. Sometimes a non-static class contains a complex static method, in which case it may contain both a static and a non-static logger. 

Classes should have an override for `ToString()` to help with Sentry debugging:

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

## Method start/end logging

In a class with a logger, the developer must **decide for each method whether it is to be logged or not**. **If it is logged, the method must contain so called entry and exit logs**.

There is always at most one entry log at the start of the method. Most of the time it is the first line of the method body. Its role is to capture the input that goes into the method. Analogically, the exit's log purpose is to capture method's return values and inform about exiting the method.

The entry log's syntax is as follows. The formatting string starts with `* ` and then continues with a list of method input arguments, each represented by `name=value` string. Name of the argument is simply obtained using `nameof`. 

**Example:**
```csharp
public async Task<Result> ConnectAsync(IPAddress address, int port)
{
    CLogger.LogInfo($"* {nameof(address)}='{address}', {nameof(port)}={port}");
    
    // Method body...
    
    CLogger.LogSuccess($"$ Connection established");
    return result;
}
```

The exit log starts with `$`, optionally followed by a tag and/or list of outputs. In case of a method with multiple different exit points (`return` or `throw`), the tag is used to differentiate among them.

**Examples:**
```csharp
// Exit log with a tag
if (this.disposedValue)
{
    CLogger.LogInfo("$<ALREADY_DISPOSED>");
    return;
}

// Exit log with a return value
result = true;
CLogger.LogSuccess($"$='{result}'");
return result;

// Exit log with a tag and a return value
CLogger.LogWarning($"$<INVALID_RPC_INTERFACE>='{result}'");
return result;
```

## Performance

Avoid logging in tight loops or performance-critical code paths. Use conditional logging or log aggregation for high-frequency events.

## Exceptions

Always log exceptions with full stack traces using `CLogger.LogError`:

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

## Log levels

Choose the appropriate log level based on the situation:

- **Log**: Routine operations, debugging info
- **Info**: Notable events, state transitions
- **Success**: Completed operations, confirmations
- **Warning**: Recoverable issues, suboptimal paths
- **Error**: Failures requiring attention
- **MajorAction**: Significant milestones (game start, level complete)
- **Event**: Event system triggers
