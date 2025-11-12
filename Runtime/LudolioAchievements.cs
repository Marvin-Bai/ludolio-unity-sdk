using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ludolio.SDK
{
    /// <summary>
    /// Achievements API for Ludolio. Similar to Steamworks achievements.
    /// Uses native DLL for secure communication with Desktop App.
    /// </summary>
    public static class LudolioAchievements
    {
        public static event Action<string> OnAchievementUnlocked;
        public static event Action<string, float> OnAchievementProgress;

        private static Dictionary<string, AchievementData> cachedAchievements = new Dictionary<string, AchievementData>();

        /// <summary>
        /// Unlock an achievement for the current user
        /// </summary>
        /// <param name="achievementId">The unique identifier of the achievement</param>
        /// <param name="callback">Optional callback with success status</param>
        public static void UnlockAchievement(string achievementId, Action<bool> callback = null)
        {
            if (!LudolioSDK.IsInitialized)
            {
                Debug.LogError("[LudolioAchievements] SDK not initialized. Call LudolioSDK.Init() first.");
                callback?.Invoke(false);
                return;
            }

            if (!LudolioSDK.IsAuthenticated)
            {
                Debug.LogError("[LudolioAchievements] User not authenticated.");
                callback?.Invoke(false);
                return;
            }

            // Call native DLL function
            LudolioNative.Ludolio_UnlockAchievement(achievementId, (success) =>
            {
                if (success)
                {
                    Debug.Log($"[LudolioAchievements] Achievement unlocked: {achievementId}");

                    // Update cache
                    if (cachedAchievements.ContainsKey(achievementId))
                    {
                        cachedAchievements[achievementId].unlocked = true;
                    }

                    OnAchievementUnlocked?.Invoke(achievementId);
                }
                else
                {
                    string error = LudolioSDK.GetLastError();
                    Debug.LogError($"[LudolioAchievements] Failed to unlock achievement: {error}");
                }

                callback?.Invoke(success);
            });
        }

        /// <summary>
        /// Set progress for an achievement (for progressive achievements)
        /// </summary>
        /// <param name="achievementId">The unique identifier of the achievement</param>
        /// <param name="progress">Progress value (0.0 to 1.0)</param>
        /// <param name="callback">Optional callback with success status</param>
        public static void SetAchievementProgress(string achievementId, float progress, Action<bool> callback = null)
        {
            if (!LudolioSDK.IsInitialized)
            {
                Debug.LogError("[LudolioAchievements] SDK not initialized.");
                callback?.Invoke(false);
                return;
            }

            progress = Mathf.Clamp01(progress);

            // If progress is 100%, unlock the achievement
            if (progress >= 1.0f)
            {
                UnlockAchievement(achievementId, callback);
            }
            else
            {
                OnAchievementProgress?.Invoke(achievementId, progress);
                callback?.Invoke(true);
            }
        }

        /// <summary>
        /// Get all achievements for the current game
        /// </summary>
        /// <param name="callback">Callback with list of achievements</param>
        public static void GetAchievements(Action<List<AchievementData>> callback)
        {
            if (!LudolioSDK.IsInitialized)
            {
                Debug.LogError("[LudolioAchievements] SDK not initialized.");
                callback?.Invoke(null);
                return;
            }

            if (!LudolioSDK.IsAuthenticated)
            {
                Debug.LogError("[LudolioAchievements] User not authenticated.");
                callback?.Invoke(null);
                return;
            }

            // Call native DLL function
            LudolioNative.Ludolio_GetAchievements((jsonData) =>
            {
                if (string.IsNullOrEmpty(jsonData))
                {
                    string error = LudolioSDK.GetLastError();
                    Debug.LogError($"[LudolioAchievements] Failed to get achievements: {error}");
                    callback?.Invoke(null);
                    return;
                }

                try
                {
                    // Parse JSON response
                    var response = JsonUtility.FromJson<AchievementsResponse>("{\"achievements\":" + jsonData + "}");

                    // Update cache
                    cachedAchievements.Clear();
                    foreach (var achievement in response.achievements)
                    {
                        cachedAchievements[achievement.id] = achievement;
                    }

                    Debug.Log($"[LudolioAchievements] Loaded {response.achievements.Count} achievements");
                    callback?.Invoke(response.achievements);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[LudolioAchievements] Failed to parse achievements: {ex.Message}");
                    callback?.Invoke(null);
                }
            });
        }

        /// <summary>
        /// Check if an achievement is unlocked (from cache)
        /// </summary>
        /// <param name="achievementId">The unique identifier of the achievement</param>
        /// <returns>True if the achievement is unlocked</returns>
        public static bool IsAchievementUnlocked(string achievementId)
        {
            // First check native cache
            if (LudolioNative.Ludolio_IsAchievementUnlocked(achievementId))
            {
                return true;
            }

            // Fallback to local cache
            if (cachedAchievements.TryGetValue(achievementId, out var achievement))
            {
                return achievement.unlocked;
            }

            return false;
        }

        /// <summary>
        /// Clear the local achievement cache
        /// </summary>
        public static void ClearCache()
        {
            cachedAchievements.Clear();
        }

        // Data structures
        [Serializable]
        public class AchievementData
        {
            public string id;
            public string name;
            public string description;
            public string icon;
            public bool unlocked;
            public string unlockedAt;
        }

        [Serializable]
        private class AchievementsResponse
        {
            public List<AchievementData> achievements;
        }
    }
}

