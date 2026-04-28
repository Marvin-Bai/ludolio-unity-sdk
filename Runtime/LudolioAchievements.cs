using System;
using System.Collections.Generic;
using UnityEngine;
using AOT;

namespace Ludolio.SDK
{
    /// <summary>
    /// Achievements API for Ludolio. Similar to Steamworks achievements.
    /// Uses native DLL for secure communication with Desktop App.
    /// IL2CPP compatible: all native callbacks use static methods with [MonoPInvokeCallback].
    /// </summary>
    public static class LudolioAchievements
    {
        public static event Action<string> OnAchievementUnlocked;

        private static Dictionary<string, AchievementData> cachedAchievements = new Dictionary<string, AchievementData>();

        // Request state for IL2CPP-safe static callbacks.
        // We serialize / coalesce requests so static native callbacks never lose managed callbacks.
        private sealed class UnlockRequest
        {
            public readonly string AchievementId;
            public readonly Action<bool> Callback;

            public UnlockRequest(string achievementId, Action<bool> callback)
            {
                AchievementId = achievementId;
                Callback = callback;
            }
        }

        private static readonly Queue<UnlockRequest> pendingUnlockRequests = new Queue<UnlockRequest>();
        private static UnlockRequest activeUnlockRequest;
        private static readonly List<Action<List<AchievementData>>> pendingListCallbacks = new List<Action<List<AchievementData>>>();
        private static bool isGetAchievementsInProgress;
        private static readonly object callbackLock = new object();

        private static void StartNextUnlockRequest()
        {
            string achievementId;

            lock (callbackLock)
            {
                if (activeUnlockRequest != null || pendingUnlockRequests.Count == 0)
                {
                    return;
                }

                activeUnlockRequest = pendingUnlockRequests.Dequeue();
                achievementId = activeUnlockRequest.AchievementId;
            }

            LudolioNative.Ludolio_UnlockAchievement(achievementId, OnUnlockAchievementCallbackStatic);
        }

        /// <summary>
        /// IL2CPP-safe static callback for achievement unlock.
        /// Native code calls this directly via function pointer.
        /// </summary>
        [MonoPInvokeCallback(typeof(LudolioNative.AchievementCallback))]
        private static void OnUnlockAchievementCallbackStatic(bool success)
        {
            UnlockRequest request;
            bool shouldStartNext;

            lock (callbackLock)
            {
                request = activeUnlockRequest;
                activeUnlockRequest = null;
                shouldStartNext = pendingUnlockRequests.Count > 0;
            }

            string achievementId = request != null ? request.AchievementId : null;
            Action<bool> callback = request != null ? request.Callback : null;

            if (success)
            {
                Debug.Log($"[LudolioAchievements] Achievement unlocked: {achievementId}");

                // Update cache
                if (achievementId != null && cachedAchievements.ContainsKey(achievementId))
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

            if (shouldStartNext)
            {
                StartNextUnlockRequest();
            }
        }

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

            bool shouldStartRequest;

            lock (callbackLock)
            {
                pendingUnlockRequests.Enqueue(new UnlockRequest(achievementId, callback));
                shouldStartRequest = activeUnlockRequest == null;
            }

            if (shouldStartRequest)
            {
                StartNextUnlockRequest();
            }
        }

        /// <summary>
        /// IL2CPP-safe static callback for achievement list.
        /// Native code calls this directly via function pointer.
        /// </summary>
        [MonoPInvokeCallback(typeof(LudolioNative.AchievementListCallback))]
        private static void OnAchievementListCallbackStatic(string jsonData)
        {
            List<Action<List<AchievementData>>> callbacks;

            lock (callbackLock)
            {
                callbacks = new List<Action<List<AchievementData>>>(pendingListCallbacks);
                pendingListCallbacks.Clear();
                isGetAchievementsInProgress = false;
            }

            if (string.IsNullOrEmpty(jsonData))
            {
                string error = LudolioSDK.GetLastError();
                Debug.LogError($"[LudolioAchievements] Failed to get achievements: {error}");
                foreach (var callback in callbacks)
                {
                    callback?.Invoke(null);
                }
                return;
            }

            try
            {
                // Parse JSON response
                var response = JsonUtility.FromJson<AchievementsResponse>("{\"achievements\":" + jsonData + "}");
                var achievements = response != null && response.achievements != null
                    ? response.achievements
                    : new List<AchievementData>();

                // Update cache. Keyed by achievementId (the API Name) — this is the
                // identifier games pass to UnlockAchievement / IsAchievementUnlocked.
                cachedAchievements.Clear();
                foreach (var achievement in achievements)
                {
                    if (string.IsNullOrEmpty(achievement.achievementId))
                    {
                        continue;
                    }
                    cachedAchievements[achievement.achievementId] = achievement;
                }

                Debug.Log($"[LudolioAchievements] Loaded {achievements.Count} achievements");
                foreach (var callback in callbacks)
                {
                    callback?.Invoke(achievements);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LudolioAchievements] Failed to parse achievements: {ex.Message}");
                foreach (var callback in callbacks)
                {
                    callback?.Invoke(null);
                }
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

            bool shouldStartRequest = false;

            lock (callbackLock)
            {
                pendingListCallbacks.Add(callback);
                if (!isGetAchievementsInProgress)
                {
                    isGetAchievementsInProgress = true;
                    shouldStartRequest = true;
                }
            }

            if (shouldStartRequest)
            {
                LudolioNative.Ludolio_GetAchievements(OnAchievementListCallbackStatic);
            }
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
            LudolioNative.Ludolio_ClearAchievementCache();
        }

        internal static void Reset()
        {
            cachedAchievements.Clear();

            lock (callbackLock)
            {
                pendingUnlockRequests.Clear();
                activeUnlockRequest = null;
                pendingListCallbacks.Clear();
                isGetAchievementsInProgress = false;
            }
        }

        // Data structures

        /// <summary>
        /// Represents a single achievement returned by <see cref="GetAchievements"/>.
        /// </summary>
        /// <remarks>
        /// Use <see cref="achievementId"/> (the "API Name" shown on the developer
        /// dashboard) when calling <see cref="UnlockAchievement"/> or
        /// <see cref="IsAchievementUnlocked"/>. The legacy <see cref="id"/> and
        /// <see cref="icon"/> fields are populated only for backwards compatibility
        /// and should not be relied upon in new code.
        /// </remarks>
        [Serializable]
        public class AchievementData
        {
            /// <summary>
            /// The achievement's API Name as defined on the developer dashboard
            /// (e.g. "first_win"). This is the identifier accepted by
            /// <see cref="UnlockAchievement"/> and <see cref="IsAchievementUnlocked"/>.
            /// </summary>
            public string achievementId;

            /// <summary>The game ID this achievement belongs to.</summary>
            public string gameId;

            /// <summary>Display name shown to the player.</summary>
            public string name;

            /// <summary>Description shown to the player.</summary>
            public string description;

            /// <summary>URL of the icon shown when the achievement is locked.</summary>
            public string lockedIconUrl;

            /// <summary>URL of the icon shown when the achievement is unlocked.</summary>
            public string unlockedIconUrl;

            /// <summary>Whether the current user has unlocked this achievement.</summary>
            public bool unlocked;

            /// <summary>ISO-8601 timestamp of the unlock, or null if locked.</summary>
            public string unlockedAt;

            // ── Deprecated fields (kept for source-compatibility with SDK <= 0.4.3) ──

            /// <summary>
            /// Deprecated. Previously held the desktop client's internal database row ID
            /// (a UUID), which is not the same as the dashboard API Name and cannot be used
            /// to unlock achievements. Use <see cref="achievementId"/> instead. Newer
            /// desktop clients may temporarily populate this as an alias of
            /// <see cref="achievementId"/> for backwards compatibility only.
            /// </summary>
            [Obsolete("Use achievementId — the dashboard API Name. The 'id' field is only a temporary compatibility alias.", false)]
            public string id;

            /// <summary>
            /// Deprecated. The desktop client returns separate <see cref="lockedIconUrl"/>
            /// and <see cref="unlockedIconUrl"/> values. Newer desktop clients may
            /// temporarily populate this as a best-effort icon URL alias for backwards
            /// compatibility only.
            /// </summary>
            [Obsolete("Use lockedIconUrl / unlockedIconUrl. The 'icon' field is only a temporary compatibility alias.", false)]
            public string icon;
        }

        [Serializable]
        private class AchievementsResponse
        {
            public List<AchievementData> achievements;
        }
    }
}

