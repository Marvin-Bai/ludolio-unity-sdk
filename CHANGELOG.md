# Changelog

All notable changes to the Ludolio Unity SDK will be documented in this file.

## [0.1.0] - 2025-10-14

### Changed
- **BREAKING CHANGE**: Complete rebranding from "Games Store" to "Ludolio"
- Renamed namespace: `GamesStore.SDK` → `Ludolio.SDK`
- Renamed classes:
  - `GamesStoreSDK` → `LudolioSDK`
  - `GamesStoreAchievements` → `LudolioAchievements`
  - `GamesStoreUser` → `LudolioUser`
- Updated command-line arguments:
  - `--edu-steam-token` → `--ludolio-token`
  - `--edu-steam-user` → `--ludolio-user`
  - `--edu-steam-client-port` → `--ludolio-client-port`
- Updated package name: `com.gamesstore.sdk` → `com.ludolio.sdk`
- Updated repository: `games-store-unity-sdk` → `ludolio-unity-sdk`
- All debug logs now use `[Ludolio*]` prefixes
- All error messages reference "Ludolio client"

### Migration Guide
To migrate from Games Store SDK to Ludolio SDK:
1. Update package reference to `com.ludolio.sdk`
2. Replace `using GamesStore.SDK` with `using Ludolio.SDK`
3. Replace all class names: `GamesStore*` → `Ludolio*`
4. Rebuild your game

## [0.0.5] - 2025-10-10

### Security
- **BREAKING CHANGE**: SDK now enforces strict validation of ALL required command-line arguments
- Added validation for `--ludolio-client-port` argument (previously only checked token and userId)
- Game now quits automatically after 3 seconds if any required argument is missing
- Added detailed error messages showing which arguments are missing
- Improved security to prevent unauthorized game access by manipulating command-line arguments

### Changed
- `LudolioSDK.Init()` now validates all three required arguments: token, userId, AND clientPort
- Added `QuitAfterDelay()` coroutine for graceful shutdown when initialization fails
- Enhanced debug logging to show which arguments are present/missing during initialization

### Fixed
- Fixed security vulnerability where games could run with partial authentication data
- Fixed issue where games would continue running if launched with only token argument
- Fixed issue where games would continue running if launched with invalid port number

## [0.0.4] - 2025-10-10

### Fixed
- Fixed Runtime assembly definition to work on all platforms (removed Editor exclusion)
- Runtime scripts are now properly available in game code, not just Editor scripts

## [0.0.3] - 2025-10-10

### Fixed
- Fixed package folder structure to match Unity package naming convention (renamed from `root` to `com.ludolio.sdk`)
- This fixes auto-referencing issues where the SDK namespace wasn't automatically available in projects

## [1.0.0] - 2025-10-09

### Added
- Initial release of Ludolio Unity SDK
- Core SDK initialization and authentication system
- Achievements API with unlock and progress tracking
- User information API
- Automatic token validation with Ludolio client
- Process lifecycle management (auto-close when client disconnects)
- Health check monitoring
- Steamworks-like API design for easy integration
- Complete example scene and documentation
- Support for Unity 2019.4 and later

### Features
- **LudolioSDK**: Main SDK class for initialization and authentication
- **LudolioAchievements**: Achievement management with events
- **LudolioUser**: User information retrieval
- Event-driven architecture for SDK callbacks
- Automatic command-line argument parsing
- Local API communication with Ludolio client
- Comprehensive error handling and logging

### Documentation
- Complete README with API reference
- Quick start guide
- Code examples and samples
- Integration instructions