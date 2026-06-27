# Changelog

## 2.0.1 - 2026-06-27

- Bumped package version for the OpenBullet 2.0.1 release train.

## 2.0.0 - 2026-06-06

Changes since `1.0.6`, released on 2022-03-09:

- Retargeted the package to .NET 10.
- Added cancellation token support to parallelizer control methods.
- Reworked `TaskBasedParallelizer` for more reliable scheduling and completion handling.
- Improved CPM calculation and CPM limiting behavior.
- Fixed degree-of-parallelism edge cases, including operation with DoP set to 1.
- Reduced CPU spinning while waiting under CPM limits.
- Fixed timing-sensitive stop, abort, start, reset, and completion state transitions.
- Marked `ThreadBasedParallelizer` as obsolete.
- Added and refactored regression tests for task-based, thread-based, and parallel-based parallelizers.
