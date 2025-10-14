# Games Store Unity SDK Documentation

Complete documentation for integrating Games Store SDK into your Unity game.

## Table of Contents

1. [Introduction](#introduction)
2. [Installation](#installation)
3. [Quick Start](#quick-start)
4. [API Reference](#api-reference)
5. [Examples](#examples)
6. [Best Practices](#best-practices)
7. [Troubleshooting](#troubleshooting)

## Introduction

The Games Store Unity SDK provides a Steamworks-like API for Unity games to integrate with the Games Store platform. It handles:

- **Authentication** - Automatic user authentication via the Games Store client
- **Achievements** - Unlock and track player achievements
- **User Data** - Access current user information
- **DRM** - Built-in validation and access control
- **Lifecycle** - Automatic game shutdown when client disconnects

### How It Works

1. Student launches your game from the Games Store desktop client
2. Client passes authentication data via command-line arguments
3. Your game initializes the SDK
4. SDK validates the token with the local Games Store client
5. Your game can now use all SDK features

## Installation

### Via Unity Package Manager (Recommended)

1. Open Unity Package Manager (`Window > Package Manager`)
2. Click `+` → `Add package from git URL`
3. Enter: `https://github.com/Marvin-Bai/games-store-unity-sdk.git`

### Via manifest.json

Add to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.gamesstore.sdk": "https://github.com/Marvin-Bai/games-store-unity-sdk.git#v1.0.0"
  }
}
```

### Specific Version

Use a specific version tag:

```
https://github.com/Marvin-Bai/games-store-unity-sdk.git#v1.0.0
```

## Quick Start

### 1. Create SDK Manager

Create a script that initializes the SDK:

```csharp
using UnityEngine;
using GamesStore.SDK;

public class GameSDKManager : MonoBehaviour
{
    [SerializeField] private string gameId = "your-game-id";
    
    void Start()
    {
        // Subscribe to events
        GamesStoreSDK.OnAuthenticationComplete += OnAuthComplete;
        
        // Initialize SDK
        if (!GamesStoreSDK.Init(gameId))
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
        GamesStoreSDK.Shutdown();
    }
}
```

### 2. Add to Scene

1. Create an empty GameObject in your first scene
2. Name it "GameSDKManager"
3. Add the `GameSDKManager` script
4. Set your game ID in the inspector

### 3. Test

Your game must be launched from the Games Store client to work properly. The SDK will fail to initialize if launched directly from Unity or standalone.

## API Reference

### GamesStoreSDK

Main SDK class for initialization and core functionality.

#### Static Methods

##### `Init(string gameId)`

Initialize the SDK with your game ID.

```csharp
bool success = GamesStoreSDK.Init("my-game-id");
```

**Parameters:**
- `gameId` - Your game's unique identifier from Games Store

**Returns:** `true` if initialization started successfully

**Note:** This starts the initialization process. Subscribe to `OnAuthenticationComplete` to know when authentication is complete.

##### `Shutdown()`

Shutdown the SDK. Call this when your game is closing.

```csharp
void OnDestroy()
{
    GamesStoreSDK.Shutdown();
}
```

#### Static Properties

##### `IsInitialized`

Check if the SDK has been initialized.

```csharp
if (GamesStoreSDK.IsInitialized)
{
    // SDK is ready
}
```

##### `IsAuthenticated`

Check if the user is authenticated.

```csharp
if (GamesStoreSDK.IsAuthenticated)
{
    // User is authenticated, can use SDK features
}
```

#### Static Events

##### `OnInitialized`

Fired when SDK initialization completes.

```csharp
GamesStoreSDK.OnInitialized += () =>
{
    Debug.Log("SDK initialized!");
};
```

##### `OnAuthenticationComplete(bool success)`

Fired when authentication completes.

```csharp
GamesStoreSDK.OnAuthenticationComplete += (success) =>
{
    if (success)
    {
        Debug.Log("Authenticated!");
    }
};
```

##### `OnClientDisconnected`

Fired when the Games Store client disconnects. The game will automatically quit after this event.

```csharp
GamesStoreSDK.OnClientDisconnected += () =>
{
    Debug.Log("Client disconnected, game will close");
};
```

### GamesStoreAchievements

Achievement management API.

#### Static Methods

##### `UnlockAchievement(string achievementId, Action<bool> callback = null)`

Unlock an achievement.

```csharp
GamesStoreAchievements.UnlockAchievement("first_win", success =>
{
    if (success)
    {
        Debug.Log("Achievement unlocked!");
    }
});
```

##### `SetAchievementProgress(string achievementId, float progress, Action<bool> callback = null)`

Set progress for a progressive achievement (0.0 to 1.0).

```csharp
// Set to 50%
GamesStoreAchievements.SetAchievementProgress("collect_100_coins", 0.5f);

// Set to 100% (will unlock the achievement)
GamesStoreAchievements.SetAchievementProgress("collect_100_coins", 1.0f);
```

##### `GetAchievements(Action<List<AchievementData>> callback)`

Get all achievements for the current game.

```csharp
GamesStoreAchievements.GetAchievements(achievements =>
{
    foreach (var achievement in achievements)
    {
        Debug.Log($"{achievement.name}: {achievement.unlocked}");
    }
});
```

##### `IsAchievementUnlocked(string achievementId)`

Check if an achievement is unlocked (from cache).

```csharp
if (GamesStoreAchievements.IsAchievementUnlocked("first_win"))
{
    // Achievement is unlocked
}
```

##### `ClearCache()`

Clear the achievement cache to force a refresh.

```csharp
GamesStoreAchievements.ClearCache();
```

#### Static Events

##### `OnAchievementUnlocked(string achievementId)`

Fired when an achievement is unlocked.

```csharp
GamesStoreAchievements.OnAchievementUnlocked += (achievementId) =>
{
    Debug.Log($"Achievement unlocked: {achievementId}");
    // Show achievement notification UI
};
```

##### `OnAchievementProgress(string achievementId, float progress)`

Fired when achievement progress updates.

```csharp
GamesStoreAchievements.OnAchievementProgress += (id, progress) =>
{
    Debug.Log($"{id}: {progress * 100}%");
};
```

### GamesStoreUser

User information API.

#### Static Methods

##### `GetUserInfo(Action<UserInfo> callback)`

Get information about the current user.

```csharp
GamesStoreUser.GetUserInfo(userInfo =>
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
string userId = GamesStoreUser.GetUserId();
```

##### `GetUserName()`

Get the current user's name (from cache).

```csharp
string userName = GamesStoreUser.GetUserName();
```

##### `GetUserEmail()`

Get the current user's email (from cache).

```csharp
string email = GamesStoreUser.GetUserEmail();
```

##### `ClearCache()`

Clear the user info cache.

```csharp
GamesStoreUser.ClearCache();
```

## Examples

See `Samples~/BasicIntegration/GamesStoreExample.cs` for a complete working example.

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
- Make sure the game is launched from Games Store client
- Check that command-line arguments are being passed
- Verify the Games Store client is running

### Authentication Fails

**Problem:** `OnAuthenticationComplete` receives `false`

**Solutions:**
- Check that the Games Store client is running
- Verify the local API server is accessible (localhost:3000)
- Check the Unity console for error messages

### Achievements Not Unlocking

**Problem:** `UnlockAchievement()` callback receives `false`

**Solutions:**
- Verify user is authenticated (`IsAuthenticated`)
- Check achievement ID is correct
- Ensure Games Store client is running
- Check network connectivity to localhost

### Game Doesn't Close When Client Closes

**Problem:** Game continues running after closing Games Store client

**Solutions:**
- This is handled automatically by the SDK
- Check that SDK is initialized properly
- Verify health check is running (check console logs)

## Support

- **Documentation**: [GitHub Wiki](https://github.com/Marvin-Bai/games-store-unity-sdk/wiki)
- **Issues**: [GitHub Issues](https://github.com/Marvin-Bai/games-store-unity-sdk/issues)
- **Examples**: Check the `Samples~` folder in the package

