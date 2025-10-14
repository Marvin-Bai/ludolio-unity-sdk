using UnityEngine;
using Ludolio.SDK;

/// <summary>
/// Example script showing how to integrate Ludolio SDK into your Unity game.
/// This is similar to how you would use Steamworks.NET
/// </summary>
public class LudolioExample : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Your game's unique ID from Ludolio")]
    public string gameId = "your-game-id";

    [Header("Test Achievement")]
    public string testAchievementId = "first_win";

    private bool isInitialized = false;

    void Start()
    {
        // Subscribe to SDK events
        LudolioSDK.OnInitialized += OnSDKInitialized;
        LudolioSDK.OnAuthenticationComplete += OnAuthenticationComplete;
        LudolioSDK.OnClientDisconnected += OnClientDisconnected;

        LudolioAchievements.OnAchievementUnlocked += OnAchievementUnlocked;

        // Initialize the SDK - similar to SteamAPI.Init()
        if (LudolioSDK.Init(gameId))
        {
            Debug.Log("Ludolio SDK initialization started...");
        }
        else
        {
            Debug.LogError("Failed to initialize Ludolio SDK!");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        LudolioSDK.OnInitialized -= OnSDKInitialized;
        LudolioSDK.OnAuthenticationComplete -= OnAuthenticationComplete;
        LudolioSDK.OnClientDisconnected -= OnClientDisconnected;

        LudolioAchievements.OnAchievementUnlocked -= OnAchievementUnlocked;

        // Shutdown the SDK - similar to SteamAPI.Shutdown()
        LudolioSDK.Shutdown();
    }
    
    private void OnSDKInitialized()
    {
        Debug.Log("SDK Initialized!");
    }
    
    private void OnAuthenticationComplete(bool success)
    {
        if (success)
        {
            isInitialized = true;
            Debug.Log("Authentication successful!");

            // Get user information
            LudolioUser.GetUserInfo(userInfo =>
            {
                if (userInfo != null)
                {
                    Debug.Log($"Welcome, {userInfo.name}!");
                }
            });

            // Load achievements
            LudolioAchievements.GetAchievements(achievements =>
            {
                if (achievements != null)
                {
                    Debug.Log($"Loaded {achievements.Count} achievements");
                    foreach (var achievement in achievements)
                    {
                        Debug.Log($"- {achievement.name}: {(achievement.unlocked ? "Unlocked" : "Locked")}");
                    }
                }
            });
        }
        else
        {
            Debug.LogError("Authentication failed!");
        }
    }

    private void OnClientDisconnected()
    {
        Debug.LogWarning("Ludolio client disconnected. Game will close.");
    }

    private void OnAchievementUnlocked(string achievementId)
    {
        Debug.Log($"Achievement unlocked: {achievementId}");
        // Show achievement notification UI here
    }

    // Example: Call this when player wins their first game
    public void UnlockFirstWinAchievement()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("SDK not ready yet!");
            return;
        }

        LudolioAchievements.UnlockAchievement(testAchievementId, success =>
        {
            if (success)
            {
                Debug.Log("Achievement unlocked successfully!");
            }
            else
            {
                Debug.LogError("Failed to unlock achievement");
            }
        });
    }

    // Example: Set progress for a progressive achievement
    public void SetAchievementProgress(string achievementId, float progress)
    {
        if (!isInitialized) return;

        LudolioAchievements.SetAchievementProgress(achievementId, progress, success =>
        {
            if (success)
            {
                Debug.Log($"Achievement progress set to {progress * 100}%");
            }
        });
    }

    // Example GUI for testing
    void OnGUI()
    {
        if (!isInitialized)
        {
            GUI.Label(new Rect(10, 10, 300, 30), "Waiting for SDK initialization...");
            return;
        }

        GUI.Label(new Rect(10, 10, 300, 30), $"User: {LudolioUser.GetUserName() ?? "Loading..."}");
        GUI.Label(new Rect(10, 40, 300, 30), $"User ID: {LudolioUser.GetUserId()}");

        if (GUI.Button(new Rect(10, 80, 200, 30), "Unlock Test Achievement"))
        {
            UnlockFirstWinAchievement();
        }

        if (GUI.Button(new Rect(10, 120, 200, 30), "Set Progress 50%"))
        {
            SetAchievementProgress(testAchievementId, 0.5f);
        }

        if (GUI.Button(new Rect(10, 160, 200, 30), "Refresh User Info"))
        {
            LudolioUser.ClearCache();
            LudolioUser.GetUserInfo(userInfo =>
            {
                Debug.Log($"User info refreshed: {userInfo?.name}");
            });
        }
    }
}

