# General Guidelines

This file discusses some general guidelines for contributors.


## Table of Contents

  * [Workflow](#workflow)
  * [Performance](#performance)
  * [Correctness](#correctness)

## Performance

We only **aim for asymptotic optimality** and only with structures that are designed to contain a non-trivial number of elements. We do not optimize the code for performance otherwise unless a bottleneck is identified.


## Correctness

We **aim for theoretical correctness of the code**. This includes avoiding even very rare race conditions with proper locking or other mechanisms.