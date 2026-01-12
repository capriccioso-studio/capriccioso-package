# Game Versioning System

Our game versioning system follows the `MAJOR.MINOR.PATCH` format, designed to provide a clear and systematic approach to tracking the development and release of our game. 

## MAJOR

This number is incremented by 1 when the game is first released or when there are significant game-changing updates. It is the only time when both `MINOR` and `PATCH` numbers reset to 0. For example, moving from a late-stage game version like `1.4.8` to the next major release will change the version to `2.0.0`.

## MINOR

This number reflects the stage of development and changes more frequently as the game progresses through different phases but does not reset unless there's an increment in the `MAJOR` number. The stages are as follows:

- `0`: Pre-alpha - Early development and feature testing. Examples: `0.0.1`, `0.0.2`, ... `0.0.9`.
- `1`: Alpha - All basic game features are established. Examples: `0.1.10`, `0.1.11`, ... `0.1.20`.
- `2`: Beta - Expanded features and gameplay refinement. Examples: `0.2.21`, `0.2.22`, ... `0.2.30`.
- `3`: QA - Focused on balance, polish, and testing. Examples: `0.3.31`, `0.3.32`, ... `0.3.40`.
- `4`: Gold - Final QA phase, game is ready for launch. Examples: `0.4.41`, `0.4.42`, ... `0.4.50`.

## PATCH

This number increments with each build or minor fix and is indicative of ongoing maintenance and updates. It continues to rise throughout the development and post-release phases and does not reset unless there is an increment in the `MAJOR` number. For example, during beta, you might see versions like `0.2.1`, `0.2.2`, and so on.

## Examples in Action

- **Pre-Release Builds**: Starting with `0.0.1`, the `PATCH` number increases with each new build or minor improvement, such as `0.0.2`, `0.0.3`, ... `0.0.10`.
- **Post-Gold and Updates**: After the game reaches its gold phase at, say, version `0.4.50`, further patches might bring versions like `0.4.51`, `0.4.52`, until a significant update or the official release of the game.
- **Major Updates or Sequels**: After significant development, when the game is ready for a major update or sequel, the `MAJOR` number increments, resetting both `MINOR` and `PATCH`. For instance, transitioning from `1.4.50` to `2.0.0` indicates a new, substantial chapter for the game.

By strictly following this versioning system, we ensure that the only reset of `MINOR` and `PATCH` numbers occurs when there is a significant enough change in the game, warranting an increase in the `MAJOR` version. This approach provides clarity and consistency in our development process and communication with players and stakeholders, signifying the scale and scope of each update or release.
