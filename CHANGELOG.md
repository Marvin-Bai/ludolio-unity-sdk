# Changelog

All notable changes to the Games Store Unity SDK will be documented in this file.

## [0.0.5] - 2025-10-10

### Security
- **BREAKING CHANGE**: SDK now enforces strict validation of ALL required command-line arguments
- Added validation for `--edu-steam-client-port` argument (previously only checked token and userId)
- Game now quits automatically after 3 seconds if any required argument is missing
- Added detailed error messages showing which arguments are missing
- Improved security to prevent unauthorized game access by manipulating command-line arguments

### Changed
- `GamesStoreSDK.Init()` now validates all three required arguments: token, userId, AND clientPort
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
- Initial release of Games Store Unity SDK
- Core SDK initialization and authentication system
- Achievements API with unlock and progress tracking
- User information API
- Automatic token validation with Games Store client
- Process lifecycle management (auto-close when client disconnects)
- Health check monitoring
- Steamworks-like API design for easy integration
- Complete example scene and documentation
- Support for Unity 2019.4 and later

### Features
- **GamesStoreSDK**: Main SDK class for initialization and authentication
- **GamesStoreAchievements**: Achievement management with events
- **GamesStoreUser**: User information retrieval
- Event-driven architecture for SDK callbacks
- Automatic command-line argument parsing
- Local API communication with Games Store client
- Comprehensive error handling and logging

### Documentation
- Complete README with API reference
- Quick start guide
- Code examples and samples
- Integration instructions