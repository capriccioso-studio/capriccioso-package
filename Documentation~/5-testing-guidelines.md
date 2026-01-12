# Testing Guidelines

This file discusses how do we test the code and how do we write the tests.

## Table of Contents

  * [Test types](#test-types)
  * [Complex tests](#complex-tests)
  * [Results in tests](#results-in-tests)
  * [Tests with assets](#tests-with-assets)


## Test types

We **use unit tests and integration tests**. Every non-trivial code deserves a test. Commonly, when appropriate, we create both unit tests, for testing functionality of small separated parts, and integration tests to test how the code behaves in the context of other components. 

## Unity Test Framework

For Unity projects, we use the Unity Test Framework which is based on NUnit. Tests are organized in:

- **Edit Mode Tests**: Run without entering play mode, suitable for testing pure C# logic
- **Play Mode Tests**: Run in play mode, suitable for testing MonoBehaviours and Unity-specific functionality

## Test Structure

Follow the Arrange-Act-Assert pattern:

```csharp
[Test]
public void Timer_Tick_DecreasesRemainingTime()
{
    // Arrange
    Timer timer = new Timer(5f);
    timer.Start();
    
    // Act
    timer.Tick(1f);
    
    // Assert
    Assert.AreEqual(4f, timer.RemainingTime, 0.001f);
}
```

## Naming Convention

Test method names should describe what is being tested:
- `MethodName_Scenario_ExpectedResult`
- Example: `ServiceLocator_RegisterDuplicate_ThrowsException`

## Complex tests

For complex scenarios involving multiple systems, create integration tests that verify the interaction between components.

## Results in tests

Always verify expected outcomes with appropriate assertions. Use meaningful assertion messages.

## Tests with assets

When tests require Unity assets (prefabs, ScriptableObjects), place them in a dedicated test resources folder and load them during test setup.
