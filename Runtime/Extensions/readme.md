# Extension Methods

This module provides convenient extension methods for common Unity types. Extensions allow calling methods directly on objects without static utility classes.

---

## VectorExtensions

Fluent modifications and conversions for `Vector2` and `Vector3`.

### Component Modification

Replace individual vector components without creating vectors manually:

```csharp
using Capriccioso;

// Before: Verbose and error-prone
Vector3 pos = transform.position;
transform.position = new Vector3(0f, pos.y, pos.z);

// After: Clean and readable
transform.position = transform.position.WithX(0f);
transform.position = transform.position.WithY(5f);
transform.position = transform.position.WithZ(10f);

// Change multiple components at once
transform.position = pos.With(x: 0f, z: 10f);  // Keep Y, change X and Z
```

### Ground-Based Calculations

Flatten vectors for horizontal distance/direction calculations:

```csharp
// Get direction to target ignoring height difference
Vector3 toTarget = target.position - transform.position;
Vector3 flatDirection = toTarget.Flat(); // Y = 0

// Calculate ground distance
float groundDistance = toTarget.FlatMagnitude();

// Check if target is in range (horizontal only)
if (groundDistance < attackRange)
{
    Attack();
}
```

### Random Positioning

Add randomness to spawn positions, spread patterns:

```csharp
// Random offset in all directions
Vector3 spawnPos = basePosition.WithRandomOffset(2f);

// Random offset on ground only (no vertical variation)
Vector3 groundSpawn = basePosition.WithRandomOffset(5f, 0f, 5f);

// Random point within radius (uses Unity's Random.insideUnitSphere)
Vector3 patrolPoint = guardPost.position.RandomPointInRadius(10f);
```

### Vector Conversion

Convert between 2D and 3D vectors:

```csharp
// Input handling: 2D input to 3D ground movement
Vector2 input = new Vector2(horizontal, vertical);
Vector3 movement = input.ToVector3XZ(); // (x, 0, y) for ground movement

// UI positioning: 3D world to 2D screen
Vector3 worldPos = enemy.position;
Vector2 screenPos = worldPos.ToVector2XY();

// Top-down to side view
Vector2 topDownPos = transform.position.ToVector2XZ();
```

### Utility Methods

```csharp
// Clamp vector components
Vector3 velocity = rb.linearVelocity.Clamp(-maxSpeed, maxSpeed);

// Absolute value
Vector3 absScale = transform.localScale.Abs();

// Round to integers (for grid snapping)
Vector3 gridPos = transform.position.Round();

// Rotate 2D vector
Vector2 direction = Vector2.right.Rotate(45f); // 45 degrees counter-clockwise

// Get perpendicular
Vector2 normal = direction.Perpendicular();
```

### Practical Examples

**Spread Projectile Pattern:**
```csharp
public void FireSpread(int count, float spreadAngle)
{
    Vector2 forward = transform.up.ToVector2XY();
    float angleStep = spreadAngle / (count - 1);
    float startAngle = -spreadAngle / 2f;
    
    for (int i = 0; i < count; i++)
    {
        Vector2 direction = forward.Rotate(startAngle + angleStep * i);
        SpawnBullet(direction.ToVector3());
    }
}
```

**Ground Enemy AI:**
```csharp
public void ChasePlayer()
{
    Vector3 toPlayer = (player.position - transform.position).Flat();
    
    if (toPlayer.magnitude > stoppingDistance)
    {
        transform.position += toPlayer.normalized * speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(toPlayer);
    }
}
```

---

## TransformExtensions

Convenient methods for Transform manipulation.

### Child Management

```csharp
using Capriccioso;

// Destroy all children (at runtime)
container.DestroyChildren();

// Destroy immediately (in editor scripts)
container.DestroyChildrenImmediate();

// Toggle all children
container.SetChildrenActive(false);  // Hide all
container.SetChildrenActive(true);   // Show all

// Get all children as array
Transform[] children = parent.GetChildren();

// Get random child (e.g., random spawn point)
Transform spawnPoint = spawnContainer.GetRandomChild();

// Find closest child to position
Transform nearestWaypoint = waypointsParent.GetClosestChild(player.position);
```

### Position Setters

Set individual position components without vector recreation:

```csharp
// World position
transform.SetPositionX(0f);   // Center on X
transform.SetPositionY(5f);   // Set height
transform.SetPositionZ(10f);  // Set depth

// Local position
transform.SetLocalPositionX(0f);
transform.SetLocalPositionY(1.5f);
transform.SetLocalPositionZ(0f);
```

### Scale Setters

```csharp
// Uniform scale
transform.SetLocalScale(2f);  // Scale to 2x in all directions

// Individual axes
transform.SetLocalScaleX(1.5f);
transform.SetLocalScaleY(2f);
transform.SetLocalScaleZ(1f);
```

### Reset Methods

```csharp
// Reset local transform (useful after re-parenting)
pooledObject.transform.SetParent(newParent);
pooledObject.transform.ResetLocal();

// Or use the combined method
pooledObject.transform.SetParentAndReset(newParent);

// Reset world position/rotation
transform.ResetWorld();
```

### Look At (Flat)

Look at targets while staying level (no tilting):

```csharp
// Enemy faces player without looking up/down
enemy.LookAtFlat(player.position);

// Or pass transform directly
turret.LookAtFlat(target);
```

### Hierarchy Utilities

```csharp
// Get full path for debugging
Debug.Log(transform.GetHierarchyPath());
// Output: "Canvas/Panel/Button/Text"

// Set parent and reset in one call
weapon.transform.SetParentAndReset(hand);
```

### Practical Examples

**Object Pooling Reset:**
```csharp
public void OnSpawnFromPool()
{
    transform.SetParentAndReset(activeContainer);
}

public void OnReturnToPool()
{
    transform.SetParent(poolContainer);
}
```

**Random Spawn Point Selection:**
```csharp
public Transform GetSpawnPoint(Vector3 nearPosition)
{
    // Get closest spawn point to a reference position
    return spawnPointsParent.GetClosestChild(nearPosition);
    
    // Or random
    return spawnPointsParent.GetRandomChild();
}
```

**UI Cleanup:**
```csharp
public void ClearInventoryUI()
{
    inventoryGrid.DestroyChildren();
}
```

---

## CollectionExtensions

Safe and convenient methods for Lists, Arrays, and Dictionaries.

### Null/Empty Checks

```csharp
using Capriccioso;

List<Enemy> enemies = null;

// Safe null-or-empty check
if (enemies.IsNullOrEmpty())
{
    Debug.Log("No enemies!");
}

// Positive check
if (enemies.HasElements())
{
    ProcessEnemies(enemies);
}
```

### Random Access

```csharp
// Get random element
string[] names = { "Alice", "Bob", "Charlie" };
string randomName = names.GetRandom();

List<Transform> spawnPoints = GetSpawnPoints();
Transform spawnPoint = spawnPoints.GetRandom();

// Safe random with TryGet
if (lootTable.TryGetRandom(out Item item))
{
    DropItem(item);
}

// Remove and return random element
Enemy randomEnemy = activeEnemies.RemoveRandom();
randomEnemy.ApplyDebuff();
```

### Safe Access

```csharp
// Get with fallback (no IndexOutOfRangeException)
string item = inventory.GetOrDefault(10, "Empty");

// First and last with defaults
Enemy firstEnemy = waveEnemies.FirstOrDefault();
Transform lastWaypoint = path.LastOrDefault();
```

### Shuffle

```csharp
// Shuffle in place
List<Card> deck = GetDeck();
deck.Shuffle();

// Get shuffled copy (original unchanged)
List<Question> randomizedQuestions = allQuestions.Shuffled();

// Array shuffle
int[] numbers = { 1, 2, 3, 4, 5 };
numbers.Shuffle();
```

### ForEach with Index

```csharp
// Iterate with index
inventory.ForEach((item, index) => 
{
    Debug.Log($"Slot {index}: {item.Name}");
    uiSlots[index].SetItem(item);
});
```

### Dictionary Extensions

```csharp
// Get or create if missing
var enemyCounts = new Dictionary<EnemyType, int>();
int count = enemyCounts.GetOrAdd(EnemyType.Zombie, () => 0);
enemyCounts[EnemyType.Zombie] = count + 1;

// Get with default (no KeyNotFoundException)
int zombieCount = enemyCounts.GetValueOrDefault(EnemyType.Zombie, 0);
```

### Practical Examples

**Loot Drop System:**
```csharp
public Item RollLoot()
{
    if (lootTable.IsNullOrEmpty()) return null;
    
    // Weighted random could use GetRandom on a weighted list
    return lootTable.GetRandom();
}
```

**Card Game Shuffle:**
```csharp
public void StartGame()
{
    deck = allCards.Shuffled();  // Don't modify master list
    playerHand.Clear();
    
    // Deal cards
    for (int i = 0; i < 5; i++)
    {
        playerHand.Add(deck.RemoveRandom());
    }
}
```

**Wave Spawning:**
```csharp
public void SpawnWave()
{
    if (spawnPoints.IsNullOrEmpty()) return;
    
    // Randomize spawn order
    var shuffledPoints = spawnPoints.Shuffled();
    
    shuffledPoints.ForEach((point, index) => 
    {
        if (index < enemyCount)
        {
            SpawnEnemy(point.position);
        }
    });
}
```

**Safe Array Access:**
```csharp
public Sprite GetAbilityIcon(int abilityIndex)
{
    // Returns defaultIcon if index out of range
    return abilityIcons.GetOrDefault(abilityIndex, defaultIcon);
}
```

---

## Quick Reference

### VectorExtensions

| Method | Description |
|--------|-------------|
| `WithX/Y/Z(float)` | Replace single component |
| `With(x?, y?, z?)` | Replace multiple components |
| `Flat()` | Set Y to 0 |
| `FlatMagnitude()` | Magnitude ignoring Y |
| `WithRandomOffset(float)` | Add random offset |
| `ToVector2XY/XZ()` | Convert Vector3 to Vector2 |
| `ToVector3()` | Convert Vector2 to Vector3 (Z=0) |
| `ToVector3XZ()` | Convert Vector2 to Vector3 (Y=0) |
| `Clamp(min, max)` | Clamp all components |
| `Abs()` | Absolute value of components |
| `Round()` | Round components |
| `RandomPointInRadius(float)` | Random point in sphere/circle |
| `Rotate(degrees)` | Rotate Vector2 |
| `Perpendicular()` | Get perpendicular Vector2 |

### TransformExtensions

| Method | Description |
|--------|-------------|
| `DestroyChildren()` | Destroy all children |
| `DestroyChildrenImmediate()` | Destroy immediately (editor) |
| `SetChildrenActive(bool)` | Enable/disable all children |
| `SetPositionX/Y/Z(float)` | Set world position component |
| `SetLocalPositionX/Y/Z(float)` | Set local position component |
| `SetLocalScale(float)` | Uniform scale |
| `SetLocalScaleX/Y/Z(float)` | Set scale component |
| `ResetLocal()` | Reset local pos/rot/scale |
| `ResetWorld()` | Reset world pos/rot |
| `LookAtFlat(target)` | Look at ignoring Y |
| `GetChildren()` | Get children array |
| `GetClosestChild(Vector3)` | Find nearest child |
| `GetRandomChild()` | Get random child |
| `GetHierarchyPath()` | Get full path string |
| `SetParentAndReset(parent)` | Set parent and reset local |

### CollectionExtensions

| Method | Description |
|--------|-------------|
| `IsNullOrEmpty()` | Check null or empty |
| `HasElements()` | Check has elements |
| `GetRandom()` | Get random element |
| `TryGetRandom(out T)` | Safe random get |
| `RemoveRandom()` | Remove and return random |
| `GetOrDefault(index, default)` | Safe index access |
| `FirstOrDefault()` | First or default |
| `LastOrDefault()` | Last or default |
| `Shuffle()` | Shuffle in place |
| `Shuffled()` | Get shuffled copy |
| `ForEach((item, index))` | Iterate with index |
| `GetOrAdd(key, factory)` | Dict get or create |
| `GetValueOrDefault(key)` | Dict safe get |
