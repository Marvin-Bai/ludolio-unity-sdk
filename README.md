# Ludolio Unity SDK

Official Unity SDK for Ludolio integration. This SDK provides a Steamworks-like API for Unity games to integrate with the Ludolio platform.

## Features

- 🔐 **Automatic Authentication** - Seamless authentication with the Ludolio client
- 🏆 **Achievements System** - Unlock and track player achievements
- 👤 **User Information** - Access current user data
- 🔄 **Auto-Sync** - Automatic synchronization with the Ludolio client
- 🛡️ **DRM Protection** - Built-in validation and time-based access control
- 🎮 **Process Lifecycle** - Automatic game shutdown when client closes (like Steam)

## Installation

### Option 1: Unity Package Manager (Recommended)

1. Open Unity Package Manager (`Window > Package Manager`)
2. Click the `+` button in the top-left corner
3. Select `Add package from git URL`
4. Enter: `https://github.com/Marvin-Bai/ludolio-unity-sdk.git`

### Option 2: Manual Installation

1. Download the latest release from [GitHub Releases](https://github.com/Marvin-Bai/ludolio-unity-sdk/releases)
2. Extract to your Unity project's `Packages` folder

## Quick Start

### 1. Initialize the SDK

Add this script to a GameObject in your first scene:

```csharp
using UnityEngine;
using Ludolio.SDK;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        // Initialize SDK with your App ID
        if (LudolioSDK.Init(12345))
        {
            Debug.Log("Ludolio SDK initialized!");
        }
    }

    void OnDestroy()
    {
        LudolioSDK.Shutdown();
    }
}
```

### 2. Handle Authentication

```csharp
void Start()
{
    LudolioSDK.OnAuthenticationComplete += OnAuthComplete;
    LudolioSDK.Init(12345);
}

void OnAuthComplete(bool success)
{
    if (success)
    {
        Debug.Log("Player authenticated!");
        // Start your game
    }
    else
    {
        Debug.LogError("Authentication failed!");
    }
}
```

### 3. Unlock Achievements

```csharp
// Unlock an achievement
LudolioAchievements.UnlockAchievement("first_win", success =>
{
    if (success)
    {
        Debug.Log("Achievement unlocked!");
    }
});

// Set progress for progressive achievements
LudolioAchievements.SetAchievementProgress("collect_100_coins", 0.5f);
```

### 4. Get User Information

```csharp
LudolioUser.GetUserInfo(userInfo =>
{
    if (userInfo != null)
    {
        Debug.Log($"Welcome, {userInfo.name}!");
        Debug.Log($"Email: {userInfo.email}");
    }
});

// Or use quick accessors
string userId = LudolioUser.GetUserId();
string userName = LudolioUser.GetUserName();
```

## API Reference

### LudolioSDK

Main SDK class for initialization and core functionality.

#### Methods

- `static bool Init(int appId)` - Initialize the SDK with your App ID
- `static void Shutdown()` - Shutdown the SDK (call on game exit)
- `static bool IsInitialized` - Check if SDK is initialized
- `static bool IsAuthenticated` - Check if user is authenticated
- `static string GetUserId()` - Get current user ID
- `static string GetGameId()` - Get current game ID

#### Events

- `OnInitialized` - Fired when SDK initialization completes
- `OnAuthenticationComplete(bool success)` - Fired when authentication completes
- `OnClientDisconnected` - Fired when Ludolio client disconnects

### LudolioAchievements

Achievement management API.

#### Methods

- `static void UnlockAchievement(string achievementId, Action<bool> callback = null)`
- `static void SetAchievementProgress(string achievementId, float progress, Action<bool> callback = null)`
- `static void GetAchievements(Action<List<AchievementData>> callback)`
- `static bool IsAchievementUnlocked(string achievementId)`
- `static void ClearCache()`

#### Events

- `OnAchievementUnlocked(string achievementId)` - Fired when an achievement is unlocked
- `OnAchievementProgress(string achievementId, float progress)` - Fired when achievement progress updates

### LudolioUser

User information API.

#### Methods

- `static void GetUserInfo(Action<UserInfo> callback)`
- `static string GetUserId()`
- `static string GetUserName()`

- `static void ClearCache()`

## Complete Example

See the included sample in `Samples~/BasicIntegration/LudolioExample.cs` for a complete working example.

```csharp
using UnityEngine;
using Ludolio.SDK;

public class MyGame : MonoBehaviour
{
    void Start()
    {
        // Subscribe to events
        LudolioSDK.OnAuthenticationComplete += OnAuth;
        LudolioAchievements.OnAchievementUnlocked += OnAchievement;

        // Initialize
        LudolioSDK.Init(12345);
    }

    void OnAuth(bool success)
    {
        if (success)
        {
            // Get user info
            LudolioUser.GetUserInfo(user => {
                Debug.Log($"Welcome {user.name}!");
            });

            // Load achievements
            LudolioAchievements.GetAchievements(achievements => {
                Debug.Log($"You have {achievements.Count} achievements");
            });
        }
    }

    void OnAchievement(string id)
    {
        Debug.Log($"Achievement unlocked: {id}");
    }

    void OnDestroy()
    {
        LudolioSDK.Shutdown();
    }
}
```

## How It Works

1. **Game Launch**: The Ludolio client launches your game with authentication parameters
2. **SDK Init**: Your game initializes the SDK, which reads the command-line arguments
3. **Authentication**: SDK validates the token with the local Ludolio client API
4. **Gameplay**: Your game can now use all SDK features (achievements, user info, etc.)
5. **Lifecycle**: SDK monitors the client connection and closes the game if client disconnects



## Requirements

- Unity 2019.4 or later
- Ludolio Desktop Client installed
- Game must be launched from Ludolio client

## Testing

For development/testing without the client:

1. The SDK will log errors if authentication data is missing
2. You can mock the local API server for testing
3. See `UNITY_SDK_INTEGRATION.md` in the desktop-client for details

## Support

- **Documentation**: [GitHub Wiki](https://github.com/Marvin-Bai/ludolio-unity-sdk/wiki)
- **Issues**: [GitHub Issues](https://github.com/Marvin-Bai/ludolio-unity-sdk/issues)
- **Examples**: Check the `Samples~` folder

## License

See LICENSE file for details.

## Changelog

See CHANGELOG.md for version history.

