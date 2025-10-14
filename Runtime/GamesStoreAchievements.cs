using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Ludolio.SDK
{
    /// <summary>
    /// Achievements API for Ludolio. Similar to Steamworks achievements.
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

            LudolioSDK.Instance.StartCoroutine(UnlockAchievementCoroutine(achievementId, callback));
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

            LudolioSDK.Instance.StartCoroutine(GetAchievementsCoroutine(callback));
        }
        
        /// <summary>
        /// Check if an achievement is unlocked
        /// </summary>
        /// <param name="achievementId">The unique identifier of the achievement</param>
        /// <returns>True if the achievement is unlocked</returns>
        public static bool IsAchievementUnlocked(string achievementId)
        {
            if (cachedAchievements.TryGetValue(achievementId, out AchievementData data))
            {
                return data.unlocked;
            }
            return false;
        }
        
        /// <summary>
        /// Clear the achievement cache. Call this to refresh achievement data.
        /// </summary>
        public static void ClearCache()
        {
            cachedAchievements.Clear();
        }
        
        private static IEnumerator UnlockAchievementCoroutine(string achievementId, Action<bool> callback)
        {
            int port = LudolioSDK.Instance.GetClientPort();
            string url = $"http://localhost:{port}/api/achievements/unlock";

            var requestData = new UnlockAchievementRequest
            {
                achievementId = achievementId,
                gameId = LudolioSDK.GetGameId(),
                userId = LudolioSDK.GetUserId(),
                timestamp = DateTime.UtcNow.ToString("o")
            };

            string jsonData = JsonUtility.ToJson(requestData);

            using (UnityWebRequest request = UnityWebRequest.Post(url, jsonData, "application/json"))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[LudolioAchievements] Achievement unlocked: {achievementId}");

                    // Update cache
                    if (cachedAchievements.ContainsKey(achievementId))
                    {
                        cachedAchievements[achievementId].unlocked = true;
                    }

                    OnAchievementUnlocked?.Invoke(achievementId);
                    callback?.Invoke(true);
                }
                else
                {
                    Debug.LogError($"[LudolioAchievements] Failed to unlock achievement: {request.error}");
                    callback?.Invoke(false);
                }
            }
        }
        
        private static IEnumerator GetAchievementsCoroutine(Action<List<AchievementData>> callback)
        {
            int port = LudolioSDK.Instance.GetClientPort();
            string gameId = LudolioSDK.GetGameId();
            string userId = LudolioSDK.GetUserId();
            string url = $"http://localhost:{port}/api/achievements/{gameId}/{userId}";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<AchievementsResponse>(request.downloadHandler.text);

                    // Update cache
                    cachedAchievements.Clear();
                    foreach (var achievement in response.achievements)
                    {
                        cachedAchievements[achievement.id] = achievement;
                    }

                    Debug.Log($"[LudolioAchievements] Loaded {response.achievements.Count} achievements");
                    callback?.Invoke(response.achievements);
                }
                else
                {
                    Debug.LogError($"[LudolioAchievements] Failed to get achievements: {request.error}");
                    callback?.Invoke(null);
                }
            }
        }
        
        [Serializable]
        private class UnlockAchievementRequest
        {
            public string achievementId;
            public string gameId;
            public string userId;
            public string timestamp;
        }
        
        [Serializable]
        private class AchievementsResponse
        {
            public List<AchievementData> achievements;
        }
    }
    
    /// <summary>
    /// Data structure for an achievement
    /// </summary>
    [Serializable]
    public class AchievementData
    {
        public string id;
        public string name;
        public string description;
        public bool unlocked;
        public string unlockedAt;
        public string iconUrl;
    }
}

