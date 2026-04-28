# Changelog

All notable changes to the Ludolio Unity SDK will be documented in this file.

## [0.4.4] - 2026-04-27

### Fixed
- **`AchievementData` now exposes the dashboard API Name.** `GetAchievements`
  previously populated `AchievementData.id` with the desktop client's internal
  database row ID (a UUID) rather than the achievement's API Name, leaving games
  unable to reconcile results back to the identifier accepted by
  `UnlockAchievement` / `IsAchievementUnlocked`. The class now exposes
  `achievementId` (the API Name), along with `gameId`, `lockedIconUrl`, and
  `unlockedIconUrl`. The legacy `id` and `icon` fields are marked `[Obsolete]`.
  Newer desktop clients may temporarily populate them as compatibility aliases
  for older SDKs, but new code should not read them.
- **`IsAchievementUnlocked` local cache fallback now works.** The cache populated
  by `GetAchievements` was keyed by the internal DB UUID while lookups used the
  API Name, so the fallback always missed. The cache is now keyed by
  `achievementId` (API Name), matching the unlock path.

### Changed
- The IPC `GET_ACHIEVEMENTS` response shape from the desktop client was tightened
  to an explicit, documented contract. Server-internal fields
  (`additionalDescription`, `metadata`, `createdAt`) are no longer included.
  A temporary `id = achievementId` and `icon = best available icon URL` alias is
  returned only so older SDK versions do not break during migration.
- `ClearCache()` now clears both the managed Unity cache and the native SDK
  achievement cache.

### Migration
- Replace any reads of `AchievementData.id` with `AchievementData.achievementId`.
- Replace any reads of `AchievementData.icon` with `AchievementData.unlockedIconUrl`
  (or `lockedIconUrl` for the locked state).
- The compatibility aliases are temporary and should be removed in the next
  breaking SDK/Desktop IPC release.

## [0.4.1] - 2026-02-18

### Fixed
- Fixed windows build error

## [0.4.0] - 2026-02-18

### Added
- **HMAC-signed achievement unlock tokens** for enhanced security

## [0.3.1] - 2026-02-17

### Fixed
- Fixed `Init()` signature in documentation (was `Init(string gameId)`, corrected to `Init(int appId)`)
- Removed non-existent `GetUserEmail()` from documentation
- Added missing `LudolioStats` API section to documentation
- Updated `game-sdk-native/README.md` project structure and API overview
- Updated version references in documentation from v0.1.0 to v0.3.1
- Added missing CHANGELOG entries for 0.2.0 and 0.3.0
- Reduced excessive emoji usage in template README

## [0.3.0] - 2026-02-16

### Added
- **LudolioStats API** for player statistics tracking
- Steamworks-style stats pattern: `RequestStats` → `GetStat`/`SetStat` → `StoreStats`
- Support for integer and float stat types (`GetStatInt`, `GetStatFloat`, `SetStatInt`, `SetStatFloat`)
- Stats events: `OnStatsReceived`, `OnStatsStored`, `OnStatsStoreFailed`
- Native SDK stats IPC endpoints
- Updated documentation with complete stats API reference
- Updated SDK README with stats quick reference

### Changed
- Updated native DLL with stats support
- Updated `LudolioNative.cs` with stats P/Invoke bindings
