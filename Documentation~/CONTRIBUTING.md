# Contributing to Capriccioso

This document outlines the contribution workflow, branching strategy, deployment process, and versioning system for Capriccioso projects.

## Table of Contents

- [Branch Structure](#branch-structure)
- [Contribution Workflow](#contribution-workflow)
- [Deployment Environments](#deployment-environments)
- [Release Plan](#release-plan)
- [Versioning](#versioning)

---

## Branch Structure

The `main` branch should always be protected and treated as `release` or `production`. No one should push directly to `main`. The `dev` branch is the development stable branch. New branches, features, and fixes should always branch out of `dev`.

### Sample Structure

```
main
dev
├── feature/pm-2069/describe-the-feature
├── fix/pm-1590/describe-the-fix
```

---

## Contribution Workflow

1. Create a new branch from `dev` for each ticket
2. Keep branch diffs as small as possible
3. Commit to each branch as often as necessary
4. Create a Pull Request back to `dev`
5. Ensure code passes all checks before requesting review

---

## Deployment Environments

### Development Environment

**Purpose:** Where initial development occurs. Developers write and test new code, features, and fixes.

**Characteristics:**
- Dynamic with frequent code changes
- Isolated from user-facing environments
- Includes advanced debugging tools and detailed logging

### Staging Environment

**Purpose:** Pre-production environment for Quality Assurance (QA) before production.

**Characteristics:**
- Mirrors production configuration (hardware, software, database)
- Used for performance testing, load testing, and environment-specific bugs
- Sometimes used for limited beta testing

### Production Environment

**Purpose:** The live environment where the game is available to end-users.

**Characteristics:**
- Optimized for stability and security
- Equipped with performance monitoring and error logging
- Post-launch updates deployed after thorough testing

---

## Release Plan

At the end of each sprint:

1. **Finalize Development:** Complete coding tasks and feature implementation
2. **Unit Testing:** Run tests to verify code correctness
3. **Code Integration:** Merge code into the main branch, ensuring compatibility
4. **QA Deployment:** Deploy to staging environment for thorough testing
5. **Production Release:** After QA approval, deploy to production

---

## Versioning

We follow standard **Semantic Versioning** with the `MAJOR.MINOR.PATCH` format.

### MAJOR

Increment when there are significant, breaking changes or major feature releases. When MAJOR increments, both MINOR and PATCH reset to 0.

**Example:** `1.4.8` → `2.0.0`

### MINOR

Increment when adding new features that are backwards-compatible. PATCH resets to 0 when MINOR increments.

**Example:** `1.2.5` → `1.3.0`

### PATCH

Increment for backwards-compatible bug fixes and minor improvements.

**Example:** `1.2.5` → `1.2.6`

### Examples

| Version | Meaning |
|---------|---------|
| `0.1.0` | Initial development release |
| `1.0.0` | First stable release |
| `1.1.0` | New feature added |
| `1.1.1` | Bug fix |
| `2.0.0` | Major update with breaking changes |
