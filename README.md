# Ludolio Unity SDK

Official Unity SDK for Ludolio integration. This SDK provides a Steamworks-like API for Unity games to integrate with the Ludolio platform.

## Quick Reference

Minimal setup for DRM:

It is recommended to setup Ludolio SDK as early as possible in the game lifecycle. The SDK will automatically handle game shutdown if launched outside of the Ludolio Desktop Client.

```csharp
void Start() {
    LudolioSDK.OnAuthenticationComplete += (success) => {
        if (success) Debug.Log("Ready to play!");
    };
    LudolioSDK.Init(YOUR_APP_ID); //You can find your App ID in the Ludolio Developer Dashboard.
}
```

Unlock achievement:

```csharp
LudolioAchievements.UnlockAchievement("ACH_FIRST_WIN");
```

Track stats (Steamworks-style):

```csharp
LudolioStats.RequestStats();       // Load stats first
LudolioStats.SetStatInt("kills", 10);  // Set value
LudolioStats.StoreStats();         // Save to server
```

**Important**: Always wait for `OnAuthenticationComplete` before accessing user data or SDK features.

## Features

- **Automatic Authentication** - Seamless authentication with the Ludolio Desktop Client
- **Achievements System** - Unlock and track player achievements
- **Stats Tracking** - Track player statistics (Steamworks-style GetStat/SetStat/StoreStats)
- **User Information** - Access current user data
- **Process Lifecycle** - Automatic game shutdown when Desktop Client closes

## Requirements

- Unity 2019.4 or later
- Windows (Windows-only)


## Installation

### Unity Package Manager (Recommended)

1. Open Unity Package Manager (`Window > Package Manager`)
2. Click the `+` button in the top-left corner
3. Select `Add package from git URL`
4. Enter the SDK repository URL `https://github.com/Marvin-Bai/ludolio-unity-sdk.git`

## Quick Start

### 1. Basic Setup

Add this script to a GameObject in your first scene:

```csharp
using UnityEngine;
using Ludolio.SDK;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private int appId = 1000; // Your App ID from Ludolio

    void Start()
    {
        // Subscribe to authentication event BEFORE initializing
        LudolioSDK.OnAuthenticationComplete += OnAuthenticationComplete;

        // Initialize SDK with your App ID
        if (LudolioSDK.Init(appId))
        {
            Debug.Log("Ludolio SDK initialization started...");
        }
        else
        {
            Debug.LogError("Failed to initialize Ludolio SDK!");
        }
    }

    private void OnAuthenticationComplete(bool success)
    {
        if (success)
        {
            string userId = LudolioSDK.GetUserId();
            Debug.Log("Authenticated! User ID: " + userId);

            // SDK is now ready - enable game features
        }
        else
        {
            Debug.LogError("Authentication failed!");
        }
    }

    void OnDestroy()
    {
        // Clean up event subscription
        LudolioSDK.OnAuthenticationComplete -= OnAuthenticationComplete;
        LudolioSDK.Shutdown();
    }
}
```

### 2. Using Achievements

Unlock achievements when players accomplish goals:

```csharp
using UnityEngine;
using Ludolio.SDK;

public class EnemyKillTracker : MonoBehaviour
{
    private int killCount = 0;

    public void OnEnemyKilled()
    {
        killCount++;

        // Unlock achievement when player reaches 10 kills
        if (killCount == 10)
        {
            LudolioAchievements.UnlockAchievement("ACH_10_KILLS");
        }
    }
}
```

With callback for confirmation:

```csharp
LudolioAchievements.UnlockAchievement("ACH_FIRST_WIN", success =>
{
    if (success)
    {
        Debug.Log("Achievement unlocked!");
    }
    else
    {
        Debug.LogError("Failed to unlock achievement");
    }
});
```

### 3. Using Stats

Stats work like Steamworks: request from server, get/set locally, then store back.

**Step 1: Load stats after authentication**

```csharp
private void OnAuthenticationComplete(bool success)
{
    if (success)
    {
        // Load stats from server
        LudolioStats.RequestStats(statsSuccess =>
        {
            if (statsSuccess)
            {
                Debug.Log("Stats loaded successfully!");

                // Read current values
                if (LudolioStats.GetStatInt("kills", out int kills))
                {
                    Debug.Log("Total kills: " + kills);
                }
            }
            else
            {
                Debug.LogError("Failed to load stats!");
            }
        });
    }
}
```

**Step 2: Update stats during gameplay**

```csharp
public void OnEnemyKilled()
{
    // Get current value, increment, and set new value
    if (LudolioStats.GetStatInt("kills", out int currentKills))
    {
        LudolioStats.SetStatInt("kills", currentKills + 1);
        LudolioStats.StoreStats(); // Save to server
    }
}
```

**Step 3: Store stats at key moments**

```csharp
// Call StoreStats when:
// - Player completes a level
// - Player pauses the game
// - At regular intervals during gameplay

LudolioStats.StoreStats(success =>
{
    if (success)
    {
        Debug.Log("Stats saved to server");
    }
});
```

### 4. Getting User Information

```csharp
private void OnAuthenticationComplete(bool success)
{
    if (success)
    {
        // Quick accessor for user ID
        string userId = LudolioSDK.GetUserId();

        // Or get full user info
        LudolioUser.GetUserInfo(userInfo =>
        {
            if (userInfo != null)
            {
                Debug.Log("Welcome, " + userInfo.name);
            }
        });
    }
}
```

**Important**: User data is only available after the `OnAuthenticationComplete` event fires with `success = true`. Do not call these methods immediately after `Init()`.

## API Reference

### LudolioSDK

Main SDK class for initialization and core functionality.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsInitialized` | `bool` | Returns `true` if SDK is initialized |
| `IsAuthenticated` | `bool` | Returns `true` if user is authenticated |

#### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `Init(int appId)` | `bool` | Initialize the SDK with your App ID |
| `Shutdown()` | `void` | Shutdown the SDK (call in `OnDestroy()`) |
| `GetUserId()` | `string` | Get current user ID (returns `null` if not authenticated) |
| `GetGameId()` | `string` | Get current game ID |
| `GetLastError()` | `string` | Get the last error message from the SDK |

#### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnInitialized` | `Action` | Fired when SDK initialization completes |
| `OnAuthenticationComplete` | `Action<bool>` | Fired when authentication completes |
| `OnClientDisconnected` | `Action` | Fired when Desktop Client disconnects |

### LudolioAchievements

Achievement management API.

#### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `UnlockAchievement(string achievementId, Action<bool> callback = null)` | `void` | Unlock an achievement |
| `GetAchievements(Action<List<AchievementData>> callback)` | `void` | Get all achievements for the game |
| `IsAchievementUnlocked(string achievementId)` | `bool` | Check if achievement is unlocked (from cache) |
| `ClearCache()` | `void` | Clear the local achievement cache |

#### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnAchievementUnlocked` | `Action<string>` | Fired when an achievement is unlocked |

#### Data Types

```csharp
public class AchievementData
{
    public string id;
    public string name;
    public string description;
    public string icon;
    public bool unlocked;
    public string unlockedAt;
}
```

### LudolioStats

Stats tracking API (Steamworks-style).

#### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `RequestStats(Action<bool> callback = null)` | `void` | Load stats from server (call before Get/Set) |
| `GetStatInt(string statId, out int value)` | `bool` | Get integer stat value |
| `GetStatFloat(string statId, out float value)` | `bool` | Get float stat value |
| `SetStatInt(string statId, int value)` | `bool` | Set integer stat (cached locally) |
| `SetStatFloat(string statId, float value)` | `bool` | Set float stat (cached locally) |
| `StoreStats(Action<bool> callback = null)` | `void` | Upload modified stats to server |

#### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnStatsReceived` | `Action` | Fired when stats are loaded from server |
| `OnStatsStored` | `Action` | Fired when stats are successfully stored |
| `OnStatsStoreFailed` | `Action<string>` | Fired when stats storage fails |

### LudolioUser

User information API.

#### Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `GetUserInfo(Action<UserInfo> callback)` | `void` | Get full user information |
| `GetUserId()` | `string` | Get current user ID |
| `GetUserName()` | `string` | Get current user name |
| `ClearCache()` | `void` | Clear cached user info |

#### Data Types

```csharp
public class UserInfo
{
    public string id;
    public string name;
    public string email;
}
```

## Complete Example

The following example demonstrates initializing the SDK, handling authentication, loading stats, and tracking gameplay events:

```csharp
using UnityEngine;
using Ludolio.SDK;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int appId = 1000;
    private bool isReady = false;

    void Start()
    {
        // Subscribe to events BEFORE initializing
        LudolioSDK.OnAuthenticationComplete += OnAuthenticationComplete;
        LudolioAchievements.OnAchievementUnlocked += OnAchievementUnlocked;

        // Initialize SDK
        if (!LudolioSDK.Init(appId))
        {
            Debug.LogError("Failed to initialize Ludolio SDK!");
        }
    }

    private void OnAuthenticationComplete(bool success)
    {
        if (success)
        {
            Debug.Log("Authentication successful! User: " + LudolioSDK.GetUserId());

            // Load stats from server
            LudolioStats.RequestStats(statsSuccess =>
            {
                if (statsSuccess)
                {
                    isReady = true;
                    Debug.Log("Stats loaded - game ready!");

                    if (LudolioStats.GetStatInt("kills", out int kills))
                    {
                        Debug.Log("Total kills: " + kills);
                    }
                }
            });
        }
        else
        {
            Debug.LogError("Authentication failed!");
        }
    }

    private void OnAchievementUnlocked(string achievementId)
    {
        Debug.Log("Achievement unlocked: " + achievementId);
    }

    // Call this when an enemy is killed
    public void OnEnemyKilled()
    {
        if (!isReady) return;

        // Update kill stat
        if (LudolioStats.GetStatInt("kills", out int currentKills))
        {
            int newKills = currentKills + 1;
            LudolioStats.SetStatInt("kills", newKills);
            LudolioStats.StoreStats();

            // Unlock achievement at 10 kills
            if (newKills == 10)
            {
                LudolioAchievements.UnlockAchievement("ACH_10_KILLS");
            }
        }
    }

    void OnDestroy()
    {
        // Clean up event subscriptions
        LudolioSDK.OnAuthenticationComplete -= OnAuthenticationComplete;
        LudolioAchievements.OnAchievementUnlocked -= OnAchievementUnlocked;

        // Shutdown SDK
        LudolioSDK.Shutdown();
    }
}
```


## Common Pitfalls

### Incorrect: Calling GetUserId() immediately after Init()

```csharp
void Start()
{
    LudolioSDK.Init(1000);
    string userId = LudolioSDK.GetUserId(); // Returns null - authentication not complete
}
```

### Correct: Wait for OnAuthenticationComplete

```csharp
void Start()
{
    LudolioSDK.OnAuthenticationComplete += OnAuthComplete;
    LudolioSDK.Init(1000);
}

void OnAuthComplete(bool success)
{
    if (success)
    {
        string userId = LudolioSDK.GetUserId(); // Now returns valid user ID
    }
}
```

### Incorrect: Forgetting to unsubscribe from events

```csharp
void OnDestroy()
{
    LudolioSDK.Shutdown(); // Memory leak - event still subscribed
}
```

### Correct: Clean up event subscriptions

```csharp
void OnDestroy()
{
    LudolioSDK.OnAuthenticationComplete -= OnAuthComplete;
    LudolioSDK.Shutdown();
}
```

### Incorrect: Using stats before RequestStats completes

```csharp
void OnAuthComplete(bool success)
{
    if (success)
    {
        LudolioStats.RequestStats();
        LudolioStats.GetStatInt("kills", out int kills); // Fails - stats not loaded yet
    }
}
```

### Correct: Wait for RequestStats callback

```csharp
void OnAuthComplete(bool success)
{
    if (success)
    {
        LudolioStats.RequestStats(statsSuccess =>
        {
            if (statsSuccess)
            {
                LudolioStats.GetStatInt("kills", out int kills); // Works correctly
            }
        });
    }
}
```

## Testing

For development and testing:

1. The SDK will log errors if authentication data is missing
2. For production builds, the game will quit if not launched from the Ludolio Desktop Client
3. Please contact us if you face any issues during integration
