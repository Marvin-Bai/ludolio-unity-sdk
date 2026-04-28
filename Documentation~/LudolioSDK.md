# Ludolio Unity SDK Documentation

Complete documentation for integrating Ludolio SDK into your Unity game.

## Table of Contents

1. [Introduction](#introduction)
2. [Installation](#installation)
3. [Quick Start](#quick-start)
4. [API Reference](#api-reference)
5. [Examples](#examples)
6. [Best Practices](#best-practices)
7. [Troubleshooting](#troubleshooting)

## Introduction

The Ludolio Unity SDK provides a Steamworks-like API for Unity games to integrate with the Ludolio platform. It handles:

- **Authentication** - Automatic user authentication via the Ludolio client
- **Achievements** - Unlock and track player achievements
- **User Data** - Access current user information
- **DRM** - Built-in validation and access control
- **Lifecycle** - Automatic game shutdown when client disconnects

### How It Works

1. Student launches your game from the Ludolio desktop client
2. Client passes authentication data via command-line arguments
3. Your game initializes the SDK
4. SDK validates the token with the local Ludolio client
5. Your game can now use all SDK features

## Installation

### Via Unity Package Manager (Recommended)

1. Open Unity Package Manager (`Window > Package Manager`)
2. Click `+` → `Add package from git URL`
3. Enter: `https://github.com/Marvin-Bai/ludolio-unity-sdk.git`

### Via manifest.json

Add to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.ludolio.sdk": "https://github.com/Marvin-Bai/ludolio-unity-sdk.git#v0.3.1"
  }
}
```

### Specific Version

Use a specific version tag:

```
https://github.com/Marvin-Bai/ludolio-unity-sdk.git#v0.3.1
```

## Quick Start

### 1. Create SDK Manager

Create a script that initializes the SDK:

```csharp
using UnityEngine;
using Ludolio.SDK;

public class GameSDKManager : MonoBehaviour
{
    [SerializeField] private int appId = 1000; // Your App ID from Ludolio

    void Start()
    {
        // Subscribe to events
        LudolioSDK.OnAuthenticationComplete += OnAuthComplete;

        // Initialize SDK
        if (!LudolioSDK.Init(appId))
        {
            Debug.LogError("Failed to initialize SDK!");
        }
    }

    void OnAuthComplete(bool success)
    {
        if (success)
        {
            Debug.Log("Ready to play!");
            // Enable gameplay
        }
        else
        {
            Debug.LogError("Authentication failed!");
        }
    }

    void OnDestroy()
    {
        LudolioSDK.Shutdown();
    }
}
```

### 2. Add to Scene

1. Create an empty GameObject in your first scene
2. Name it "GameSDKManager"
3. Add the `GameSDKManager` script
4. Set your game ID in the inspector

### 3. Test

Your game must be launched from the Ludolio client to work properly. The SDK will fail to initialize if launched directly from Unity or standalone.

## API Reference

### LudolioSDK

Main SDK class for initialization and core functionality.

#### Static Methods

##### `Init(int appId)`

Initialize the SDK with your App ID.

```csharp
bool success = LudolioSDK.Init(1000);
```

**Parameters:**
- `appId` - Your game's App ID from the Ludolio Developer Dashboard

**Returns:** `true` if initialization started successfully

**Note:** This starts the initialization process. Subscribe to `OnAuthenticationComplete` to know when authentication is complete.

##### `Shutdown()`

Shutdown the SDK. Call this when your game is closing.

```csharp
void OnDestroy()
{
    LudolioSDK.Shutdown();
}
```

#### Static Properties

##### `IsInitialized`

Check if the SDK has been initialized.

```csharp
if (LudolioSDK.IsInitialized)
{
    // SDK is ready
}
```

##### `IsAuthenticated`

Check if the user is authenticated.

```csharp
if (LudolioSDK.IsAuthenticated)
{
    // User is authenticated, can use SDK features
}
```

#### Static Events

##### `OnInitialized`

Fired when SDK initialization completes.

```csharp
LudolioSDK.OnInitialized += () =>
{
    Debug.Log("SDK initialized!");
};
```

##### `OnAuthenticationComplete(bool success)`

Fired when authentication completes.

```csharp
LudolioSDK.OnAuthenticationComplete += (success) =>
{
    if (success)
    {
        Debug.Log("Authenticated!");
    }
};
```

##### `OnClientDisconnected`

Fired when the Ludolio client disconnects. The game will automatically quit after this event.

```csharp
LudolioSDK.OnClientDisconnected += () =>
{
    Debug.Log("Client disconnected, game will close");
};
```

### LudolioAchievements

Achievement management API.

> **Identifier convention.** The string passed to `UnlockAchievement`,
> `IsAchievementUnlocked`, and the `OnAchievementUnlocked` event is the
> achievement's **API Name** as configured on the developer dashboard
> (e.g. `"first_win"`). On `AchievementData` objects returned by
> `GetAchievements`, the same value is exposed as `achievementId`. Do
> not confuse this with any internal database identifier.

#### Static Methods

##### `UnlockAchievement(string achievementId, Action<bool> callback = null)`

Unlock an achievement. `achievementId` is the dashboard API Name.

```csharp
LudolioAchievements.UnlockAchievement("first_win", success =>
{
    if (success)
    {
        Debug.Log("Achievement unlocked!");
    }
});
```

##### `GetAchievements(Action<List<AchievementData>> callback)`

Get all achievements for the current game, including their unlock status for the
current user. Useful for hydrating local state on launch.

```csharp
LudolioAchievements.GetAchievements(achievements =>
{
    foreach (var achievement in achievements)
    {
        // achievement.achievementId is the dashboard API Name
        Debug.Log($"{achievement.achievementId} ({achievement.name}): {achievement.unlocked}");
    }
});
```

**Example: restoring unlock state on launch**

```csharp
LudolioAchievements.GetAchievements(achievements =>
{
    if (achievements == null) return;

    foreach (var achievement in achievements)
    {
        if (achievement.unlocked)
        {
            // Use the API Name to reconcile with your local progression
            MyGameProgress.MarkAchievementUnlocked(achievement.achievementId);
        }
    }
});
```

##### `IsAchievementUnlocked(string achievementId)`

Check if an achievement is unlocked (from cache). Pass the dashboard API Name.

```csharp
if (LudolioAchievements.IsAchievementUnlocked("first_win"))
{
    // Achievement is unlocked
}
```

> The local cache used by this method is populated by `GetAchievements`. Call
> `GetAchievements` once after authentication if you intend to rely on this
> method without first unlocking achievements during the session.

##### `ClearCache()`

Clear both the managed Unity achievement cache and the native SDK achievement
cache to force a refresh.

```csharp
LudolioAchievements.ClearCache();
```

#### Static Events

##### `OnAchievementUnlocked(string achievementId)`

Fired when an achievement is unlocked. The argument is the dashboard API Name.

```csharp
LudolioAchievements.OnAchievementUnlocked += (achievementId) =>
{
    Debug.Log($"Achievement unlocked: {achievementId}");
    // Show achievement notification UI
};
```

#### Data Types

```csharp
public class AchievementData
{
    public string achievementId;     // Dashboard API Name — use this for UnlockAchievement / IsAchievementUnlocked
    public string gameId;
    public string name;              // Display name
    public string description;
    public string lockedIconUrl;     // Icon URL when locked
    public string unlockedIconUrl;   // Icon URL when unlocked
    public bool   unlocked;
    public string unlockedAt;        // ISO-8601 timestamp, or null if locked

    // Deprecated temporary compatibility aliases — do not use in new code:
    [Obsolete] public string id;     // aliases achievementId, never the DB row UUID
    [Obsolete] public string icon;   // aliases the best available icon URL
}
```

> `id` and `icon` are temporary compatibility aliases for older SDK clients and
> should be removed in the next breaking SDK/Desktop IPC release. New code should
> use `achievementId`, `lockedIconUrl`, and `unlockedIconUrl`.

### LudolioUser

User information API.

#### Static Methods

##### `GetUserInfo(Action<UserInfo> callback)`

Get information about the current user.

```csharp
LudolioUser.GetUserInfo(userInfo =>
{
    if (userInfo != null)
    {
        Debug.Log($"User: {userInfo.name}");
        Debug.Log($"Email: {userInfo.email}");
    }
});
```

##### `GetUserId()`

Get the current user's ID.

```csharp
string userId = LudolioUser.GetUserId();
```

##### `GetUserName()`

Get the current user's name (from cache).

```csharp
string userName = LudolioUser.GetUserName();
```

##### `ClearCache()`

Clear the user info cache.

```csharp
LudolioUser.ClearCache();
```

### LudolioStats

Stats tracking API. Works like Steamworks stats: request from server, get/set locally, then store back.

#### Static Methods

##### `RequestStats(Action<bool> callback = null)`

Load stats from the server. Must be called before `GetStat`/`SetStat`.

```csharp
LudolioStats.RequestStats(success =>
{
    if (success)
    {
        Debug.Log("Stats loaded!");
    }
});
```

##### `GetStatInt(string statId, out int value)`

Get an integer stat value. `RequestStats` must complete first.

```csharp
if (LudolioStats.GetStatInt("kills", out int kills))
{
    Debug.Log($"Total kills: {kills}");
}
```

**Returns:** `true` if the stat was found

##### `GetStatFloat(string statId, out float value)`

Get a float stat value. `RequestStats` must complete first.

```csharp
if (LudolioStats.GetStatFloat("playtime", out float hours))
{
    Debug.Log($"Play time: {hours} hours");
}
```

**Returns:** `true` if the stat was found

##### `SetStatInt(string statId, int value)`

Set an integer stat value. Changes are cached locally until `StoreStats` is called.

```csharp
LudolioStats.SetStatInt("kills", 10);
```

**Returns:** `true` if successful

##### `SetStatFloat(string statId, float value)`

Set a float stat value. Changes are cached locally until `StoreStats` is called.

```csharp
LudolioStats.SetStatFloat("playtime", 2.5f);
```

**Returns:** `true` if successful

##### `StoreStats(Action<bool> callback = null)`

Upload all modified stats to the server.

```csharp
LudolioStats.StoreStats(success =>
{
    if (success)
    {
        Debug.Log("Stats saved!");
    }
});
```

#### Static Events

##### `OnStatsReceived`

Fired when stats are successfully loaded from the server.

```csharp
LudolioStats.OnStatsReceived += () =>
{
    Debug.Log("Stats received from server");
};
```

##### `OnStatsStored`

Fired when stats are successfully stored to the server.

```csharp
LudolioStats.OnStatsStored += () =>
{
    Debug.Log("Stats saved to server");
};
```

##### `OnStatsStoreFailed(string error)`

Fired when stats storage fails.

```csharp
LudolioStats.OnStatsStoreFailed += (error) =>
{
    Debug.LogError($"Failed to store stats: {error}");
};
```

## Examples

See `Samples~/BasicIntegration/LudolioExample.cs` for a complete working example.

## Best Practices

1. **Initialize Early** - Initialize the SDK in your first scene's Awake() or Start()
2. **Handle Events** - Always subscribe to SDK events before calling Init()
3. **Check Authentication** - Wait for OnAuthenticationComplete before enabling gameplay
4. **Shutdown Properly** - Always call Shutdown() in OnDestroy()
5. **Cache User Data** - Call GetUserInfo() once and cache the result
6. **Error Handling** - Handle authentication failures gracefully

## Troubleshooting

### SDK Fails to Initialize

**Problem:** `Init()` returns false

**Solutions:**
- Make sure the game is launched from Ludolio client
- Check that command-line arguments are being passed
- Verify the Ludolio client is running

### Authentication Fails

**Problem:** `OnAuthenticationComplete` receives `false`

**Solutions:**
- Check that the Ludolio client is running
- Verify the local API server is accessible (localhost:3000)
- Check the Unity console for error messages

### Achievements Not Unlocking

**Problem:** `UnlockAchievement()` callback receives `false`

**Solutions:**
- Verify user is authenticated (`IsAuthenticated`)
- Check achievement ID is correct
- Ensure Ludolio client is running
- Check network connectivity to localhost

### Game Doesn't Close When Client Closes

**Problem:** Game continues running after closing Ludolio client

**Solutions:**
- This is handled automatically by the SDK
- Check that SDK is initialized properly
- Verify health check is running (check console logs)

## Support

- **Documentation**: [GitHub Wiki](https://github.com/Marvin-Bai/ludolio-unity-sdk/wiki)
- **Issues**: [GitHub Issues](https://github.com/Marvin-Bai/ludolio-unity-sdk/issues)
- **Examples**: Check the `Samples~` folder in the package

