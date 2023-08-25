## Table of Contents

  * [General Guidelines](1-general-guidelines.md)
  * [Code Style Guidelines](2-code-style-guidelines.md)
  * [Commenting Guidelines](3-commenting-guidelines.md)
  * [Logging Guidelines](4-logging-guidelines.md)
  * [Testing Guidelines](5-testing-guidelines.md)

## Branch structure
  Main branch should always be protected, and will be treated as `release` or `production`. No one should push directly to `main`. `dev` branch is development stable branch. New branches,features, and fixes should always branch out of `dev`

### sample structure
   - main
   - dev
     - feature/pm-2069/describe-the-feature
     - fix/pm-1590/describe-the-fix

## Contribution
  We create Pull Requests branching from `dev` for each ticket. We try to keep each branch diffs as small as possible, and commit to each branch as often as necessary. 

