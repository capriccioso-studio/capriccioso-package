# General Guidelines

This file discusses some general guidelines for contributors.


## Table of Contents

  * [Workflow](#workflow)
  * [Performance](#performance)
  * [Correctness](#correctness)

## Performance

We only **aim for asymptotic optimality** and only with structures that are designed to contain a non-trivial number of elements. We do not optimize the code for performance otherwise unless a bottleneck is identified. Otherwise, code readability is of higher priority than optimization.


## Correctness

We **aim for theoretical correctness of the code**. This includes avoiding even very rare race conditions with proper locking or other mechanisms. In Unity development, this means we avoid any cases of `WaitForSeconds` or `Invoke` as much as possible. Anything that needs to wait for a coroutine to finish should either be a callback or a `Task`.

## Asset Folder Structure

We always place project-generated content on top, so we use underscores (_) to indicate that a folder contains internally-developed code or assets. 
Generally, we use this structure:

### Assets 
  - _Animations
  - _Prefabs
  - _Resources (for assetbundles/resources for testing that should be from the backend)
  - _Scenes
  - _Scripts
  - _Sounds
  - _Sprites

### Feel free to use the CreateFolders menu to automate this process