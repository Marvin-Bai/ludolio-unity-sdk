# Changelog

All notable changes to the Ludolio Unity SDK will be documented in this file.

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
