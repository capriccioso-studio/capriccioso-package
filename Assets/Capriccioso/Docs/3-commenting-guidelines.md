# Commenting Guidelines

This file discusses how to add comments and documentation to the code.

## Table of Contents

  * [Proper English](#proper-english)
  * [Third party code](#third-party-code)
  * [XML doc](#xml-doc)
  * [Added value of XML doc](#added-value-of-xml-doc)
  * [inheritdoc](#inheritdoc)
  * [Tags](#tags)
  * [No empty lines](#no-empty-lines)
  * [Locks](#locks)
  * [Inline comments](#inline-comments)
  * [Added value of inline comment](#added-value-of-inline-comment)
  * [Summaries and remarks](#summaries-and-remarks)
  * [Parameters](#parameters)
  * [Same object, same comment](#same-object-same-comment)
  * [Values and literals](#values-and-literals)

## Proper English

All out comments and documentation are in English and **we aim for proper English language**, but we allow omitting articles at the beginning of the comment, especially in case of method parameters and class properties. Proper sentences should always be used. This includes ending the comment sentences with `.`, `?`, or `!`.

**Examples:**
```csharp
//- Article is omitted.
/// <summary>Queue of actions to be executed in a UI context.</summary>
private readonly Queue<PendingAction> uiQueue;

/// <param name="joinableTaskFactory">Factory for starting async task running on the background.</param>
public UiActionExecutor(JoinableTaskFactory joinableTaskFactory, Shutdown parentShutdown):
```


## XML doc

We **use XML Documentation for everything except inline comments**. This includes private classes and private members of classes.

An XML documentation comment starts with triple slash followed by a single space. Then the body of the comment follows.

Generally, we should use a plugin that speeds up creation of XML docs such as C# XML Documentation Comments


## Added value of XML doc

We try to **describe each element with a valuable comment** and we try to **avoid repeating the type or nature of the commented element**, such as that it is a class in its `<summary>`, or that it returns an instance of a certain type in its `<returns>` section. If, and only if the element name perfectly describes what it is for, then we don't add a description.

## No empty lines

We **avoid empty lines in XML documentation**. Use `<para>` instead. Do not use `<br>`.


## Inline comments

**Double slash (`//`) inline comments should be on a separate line** above the relevant code. Rare exceptions are allowed in initializers where it is sometimes more practical to put the comment at the end of the line.

Inline comments start with two slashes followed by a single space. Then the body of the comment follows.

**Examples:**
```csharp
private static readonly Dictionary<string, Type> typeMapping = new()
{
    { BinanceConstants.FilterTypePriceFilter, typeof(BinancePriceFilter) },
    { BinanceConstants.FilterTypePercentPrice, typeof(BinancePercentPriceFilter) }, // Here an inline comment at the end of the line is allowed.
    { BinanceConstants.FilterTypeLotSize, typeof(BinanceLotSizeFilter) },
    { BinanceConstants.FilterTypeMinNotional, typeof(BinanceMinNotionalFilter) },
```

## Added value of inline comment

**Do not write to inline comments what is obvious from the code or from the log**. If a comment is needed in the code, it must give a new information to the reader that the reader may otherwise not have without the comment.


## Summaries and remarks

**Summaries should start with a short paragraph** describing the commented element and followed by as much explanation as needed to provide a valuable information about the commented element to the reader. In case of class summaries, this could include documentation of how the class is supposed to be used, how the class works in the context of other components, bigger design picture etc. However, **implementation details of the element always belong to the `<remarks>` section**. **Summaries should also include description of constraints**, if any, such as maximum values or lengths etc.

For normal class constructors in the production code we use the following typical summary:

```csharp
/// <summary>
/// Creates a new instance of the object.
/// </summary>
```

For special class constructors, such as empty constructors needed by serializers, we can modify the summary and add an explanatory remark as follows:

```csharp
/// <summary>
/// Creates an empty instance of the object.
/// </summary>
/// <remarks>This constructor is used by database serializer.</remarks>
```

For constructor of test classes we use the following typical summary:

```csharp
/// <summary>
/// Initializes common objects used by tests.
/// </summary>
``` 

## Parameters

**Parameter comments should include description of constraints**, if any, such as maximum values or lengths etc. If a parameter is optional, it should be mentioned. Comments of constructor parameters that are assigned to local members should have the same comment as the local member.

**Examples:**
```csharp
//- Constraint on value of the parameter must be mentioned.
/// <param name="error">Error code of the failed operation, must not be equal to <see cref="Result.Success"/>.</param>

//- Do mention that parameter is optional.
/// <param name="scriptId">Optionally, identifier of the script to be assigned to the script box instance.</param>
```


## Same object, same comment

If multiple classes contain the same object (such as through dependency injection), **the XML documentation of the local members should be identical**.


## Values and literals

Inside of the XML documentation comments we **use `<c>` tag for writing values and literals**.

**Examples:**
```csharp
//- Notice that "true" and "false" are inside <c> tags.
/// <summary>Set to <c>true</c> if the object was disposed already, <c>false</c> otherwise. Used by the Dispose pattern.</summary>
private bool disposedValue;

//- Notice that "0" and "null" are inside <c> tags.
/// <param name="eventAgeThresholdSeconds">
/// Optionally, age of the trade script's events, in seconds, in the database after which the event is going to be removed.
/// <para>If the value is <c>0</c>, the events should not be deleted based on their age.</para>
/// <para>If the value is <c>null</c> and the index information for the given trade script does not exist yet, the default value will be used.</para>
/// <para>If the value is <c>null</c> and the index information for the given trade script already exists, the current value of the setting will be preserved.</para>
/// </param>
```
