# Ludolio Unity SDK

Official Unity SDK for Ludolio integration. This SDK provides a Steamworks-like API for Unity games to integrate with the Ludolio platform.

## 🚀 Quick Reference

**Minimal setup (most games only need this):**

```csharp
void Start() {
    LudolioSDK.OnAuthenticationComplete += (success) => {
        if (success) Debug.Log("Ready to play!");
    };
    LudolioSDK.Init(YOUR_APP_ID);
}
```

**Unlock achievement:**
```csharp
LudolioAchievements.UnlockAchievement("first_win");
```

**Track stats (like Steamworks):**
```csharp
LudolioStats.RequestStats();  // Load stats first
LudolioStats.SetStatInt("wins", 10);  // Set value
LudolioStats.StoreStats();  // Save to server
```

**Get user info (optional):**
```csharp
LudolioUser.GetUserInfo(user => Debug.Log($"Welcome {user.name}!"));
```

> 💡 **Remember**: Always wait for `OnAuthenticationComplete` before accessing user data!

## Features

- 🔐 **Automatic Authentication** - Seamless authentication with the Ludolio client
- 🏆 **Achievements System** - Unlock and track player achievements
- 📊 **Stats Tracking** - Track player statistics (like Steamworks GetStat/SetStat/StoreStats)
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

### 1. Basic Setup (Minimal - Most Common)

For most games, you just need to initialize the SDK and wait for authentication. Add this script to a GameObject in your first scene:

```csharp
using UnityEngine;
using Ludolio.SDK;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int appId = 12345; // Your App ID from Ludolio

    void Start()
    {
        // Subscribe to authentication event
        LudolioSDK.OnAuthenticationComplete += OnAuthComplete;

        // Initialize SDK
        LudolioSDK.Init(appId);
    }

    void OnAuthComplete(bool success)
    {
        if (success)
        {
            Debug.Log("Player authenticated! Game ready.");
            // Enable your game UI/gameplay here
        }
        else
        {
            Debug.LogError("Authentication failed!");
            // Handle authentication failure
        }
    }

    void OnDestroy()
    {
        // Clean up
        LudolioSDK.OnAuthenticationComplete -= OnAuthComplete;
        LudolioSDK.Shutdown();
    }
}
```

**That's it!** For most games, this is all you need. The SDK handles authentication automatically.

### 2. Unlock Achievements (Optional)

```csharp
// Simple achievement unlock
LudolioAchievements.UnlockAchievement("first_win");

// With callback to know when it's done
LudolioAchievements.UnlockAchievement("first_win", success =>
{
    if (success)
    {
        Debug.Log("Achievement unlocked!");
    }
});
```

### 3. Track Stats (Optional)

Stats work like Steamworks - you must request stats from the server first, then get/set values locally, and finally store them back.

```csharp
// 1. Request stats from server (call once after authentication)
LudolioStats.RequestStats(success =>
{
    if (success)
    {
        Debug.Log("Stats loaded!");

        // 2. Get current stat values
        if (LudolioStats.GetStatInt("games_played", out int gamesPlayed))
        {
            Debug.Log($"Games played: {gamesPlayed}");
        }

        if (LudolioStats.GetStatFloat("total_distance", out float distance))
        {
            Debug.Log($"Total distance: {distance}");
        }
    }
});

// 3. Set stat values (cached locally until StoreStats is called)
LudolioStats.SetStatInt("games_played", 5);
LudolioStats.SetStatFloat("total_distance", 1234.5f);

// 4. Store stats to server (call periodically or at key moments)
LudolioStats.StoreStats(success =>
{
    if (success) Debug.Log("Stats saved!");
});
```

### 4. Get User Information (Optional)

Most games don't need this, but if you want to display user info:

```csharp
// Get full user info (only call AFTER authentication completes)
void OnAuthComplete(bool success)
{
    if (success)
    {
        LudolioUser.GetUserInfo(userInfo =>
        {
            if (userInfo != null)
            {
                Debug.Log($"Welcome, {userInfo.name}!");
                // Display user name in UI
            }
        });
    }
}

// Quick accessors (only available after authentication)
string userId = LudolioSDK.GetUserId();
string userName = LudolioUser.GetUserName();
```

> ⚠️ **Important**: User data is only available **after** the `OnAuthenticationComplete` event fires with `success = true`. Don't call these methods in `Start()` immediately after `Init()`.

## API Reference

### LudolioSDK

Main SDK class for initialization and core functionality.

#### Methods

- `static bool Init(int appId)` - Initialize the SDK with your App ID. Returns `true` if initialization started successfully.
- `static void Shutdown()` - Shutdown the SDK (call in `OnDestroy()`)
- `static bool IsInitialized` - Check if SDK is initialized
- `static bool IsAuthenticated` - Check if user is authenticated (only `true` after `OnAuthenticationComplete` fires)
- `static string GetUserId()` - Get current user ID (only available after authentication completes, returns `null` otherwise)
- `static string GetGameId()` - Get current game ID
- `static string GetLastError()` - Get the last error message from the SDK

#### Events

- `OnInitialized` - Fired when SDK initialization completes
- `OnAuthenticationComplete(bool success)` - **Most important event!** Fired when authentication completes. Wait for this before accessing user data.
- `OnClientDisconnected` - Fired when Ludolio Desktop Client disconnects (game will close automatically)

### LudolioAchievements

Achievement management API.

#### Methods

- `static void UnlockAchievement(string achievementId, Action<bool> callback = null)`
- `static void GetAchievements(Action<List<AchievementData>> callback)`
- `static bool IsAchievementUnlocked(string achievementId)`
- `static void ClearCache()`

#### Events

- `OnAchievementUnlocked(string achievementId)` - Fired when an achievement is unlocked

### LudolioStats

Stats tracking API (like Steamworks stats).

#### Methods

- `static void RequestStats(Action<bool> callback = null)` - Load stats from server. Must call before Get/Set.
- `static bool GetStatInt(string statId, out int value)` - Get integer stat value
- `static bool GetStatFloat(string statId, out float value)` - Get float stat value
- `static bool SetStatInt(string statId, int value)` - Set integer stat (cached locally)
- `static bool SetStatFloat(string statId, float value)` - Set float stat (cached locally)
- `static void StoreStats(Action<bool> callback = null)` - Upload modified stats to server

#### Events

- `OnStatsReceived` - Fired when stats are loaded from server
- `OnStatsStored` - Fired when stats are successfully stored
- `OnStatsStoreFailed(string error)` - Fired when stats storage fails

### LudolioUser

User information API.

#### Methods

- `static void GetUserInfo(Action<UserInfo> callback)`
- `static string GetUserId()`
- `static string GetUserName()`

- `static void ClearCache()`

## Complete Example with Achievements and Stats

See the included sample in `Samples~/BasicIntegration/LudolioExample.cs` for a complete working example.

```csharp
using UnityEngine;
using Ludolio.SDK;

public class MyGame : MonoBehaviour
{
    [SerializeField] private int appId = 12345;
    private bool isReady = false;

    void Start()
    {
        // Subscribe to events
        LudolioSDK.OnAuthenticationComplete += OnAuth;
        LudolioAchievements.OnAchievementUnlocked += OnAchievementUnlocked;

        // Initialize SDK
        LudolioSDK.Init(appId);
    }

    void OnAuth(bool success)
    {
        if (success)
        {
            Debug.Log("Authentication successful!");
            isReady = true;

            // Optional: Get user info to display welcome message
            LudolioUser.GetUserInfo(user => {
                if (user != null)
                {
                    Debug.Log($"Welcome {user.name}!");
                }
            });

            // Optional: Load achievements to show progress
            LudolioAchievements.GetAchievements(achievements => {
                if (achievements != null)
                {
                    Debug.Log($"You have {achievements.Count} achievements");
                }
            });

            // Optional: Load stats
            LudolioStats.RequestStats(statsSuccess => {
                if (statsSuccess)
                {
                    if (LudolioStats.GetStatInt("games_played", out int games))
                    {
                        Debug.Log($"Games played: {games}");
                    }
                }
            });
        }
        else
        {
            Debug.LogError("Authentication failed!");
        }
    }

    void OnAchievementUnlocked(string achievementId)
    {
        Debug.Log($"Achievement unlocked: {achievementId}");
        // Show achievement notification UI
    }

    // Example: Unlock achievement and update stats when player wins
    public void OnPlayerWin()
    {
        if (isReady)
        {
            // Unlock achievement
            LudolioAchievements.UnlockAchievement("first_win");

            // Update stats
            if (LudolioStats.GetStatInt("wins", out int currentWins))
            {
                LudolioStats.SetStatInt("wins", currentWins + 1);
                LudolioStats.StoreStats(); // Save to server
            }
        }
    }

    void OnDestroy()
    {
        // Clean up event subscriptions
        LudolioSDK.OnAuthenticationComplete -= OnAuth;
        LudolioAchievements.OnAchievementUnlocked -= OnAchievementUnlocked;

        // Shutdown SDK
        LudolioSDK.Shutdown();
    }
}
```

## How It Works

1. **Game Launch**: The Ludolio Desktop Client launches your game with a secure session token
2. **SDK Init**: Your game calls `LudolioSDK.Init()` which reads the session token
3. **Authentication**: SDK validates the token with the Desktop Client via secure named pipe (happens in ~100ms)
4. **Callback**: `OnAuthenticationComplete` event fires when authentication succeeds
5. **Gameplay**: Your game can now use all SDK features (achievements, user info, etc.)
6. **Lifecycle**: SDK monitors the client connection and closes the game if client disconnects

### Authentication Flow

```
Your Game Start()
      ↓
Subscribe to OnAuthenticationComplete
      ↓
Call LudolioSDK.Init(appId)
      ↓
[SDK connects to Desktop Client]
      ↓
[Authentication happens (~100ms)]
      ↓
OnAuthenticationComplete(true) ← Your callback fires
      ↓
Game is ready! Enable gameplay
```

> 💡 **Tip**: Authentication is very fast (typically <100ms), so you can show a simple "Connecting..." message or just let it happen in the background.



## Common Pitfalls ⚠️

### ❌ DON'T: Call GetUserId() immediately after Init()

```csharp
void Start()
{
    LudolioSDK.Init(12345);
    string userId = LudolioSDK.GetUserId(); // ❌ Returns null! Authentication not done yet
}
```

### ✅ DO: Wait for OnAuthenticationComplete

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
        string userId = LudolioSDK.GetUserId(); // ✅ Now it works!
    }
}
```

### ❌ DON'T: Forget to unsubscribe from events

```csharp
void OnDestroy()
{
    LudolioSDK.Shutdown(); // ❌ Memory leak! Event still subscribed
}
```

### ✅ DO: Clean up event subscriptions

```csharp
void OnDestroy()
{
    LudolioSDK.OnAuthenticationComplete -= OnAuthComplete; // ✅ Clean up
    LudolioSDK.Shutdown();
}
```

## Requirements

- Unity 2019.4 or later
- Windows (currently Windows-only, macOS/Linux support coming soon)
- Ludolio Desktop Client installed
- Game must be launched from Ludolio Desktop Client

## Testing

For development/testing without the client:

1. The SDK will log errors if authentication data is missing
2. In the Unity Editor, the SDK will fail gracefully without crashing
3. For production builds, the game will quit if not launched from the Ludolio client
4. See `UNITY_SDK_INTEGRATION.md` in the desktop-client for testing details

## Support

- **Documentation**: [GitHub Wiki](https://github.com/Marvin-Bai/ludolio-unity-sdk/wiki)
- **Issues**: [GitHub Issues](https://github.com/Marvin-Bai/ludolio-unity-sdk/issues)
- **Examples**: Check the `Samples~` folder

## License

See LICENSE file for details.

## Changelog

See CHANGELOG.md for version history.

