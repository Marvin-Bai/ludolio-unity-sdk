using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ludolio.SDK
{
    /// <summary>
    /// Stats API for Ludolio. Similar to Steamworks stats (GetStat, SetStat, StoreStats).
    /// Uses native DLL for secure communication with Desktop App.
    /// </summary>
    public static class LudolioStats
    {
        /// <summary>
        /// Event fired when stats are successfully loaded from the server
        /// </summary>
        public static event Action OnStatsReceived;

        /// <summary>
        /// Event fired when stats are successfully stored to the server
        /// </summary>
        public static event Action OnStatsStored;

        /// <summary>
        /// Event fired when stats storage fails
        /// </summary>
        public static event Action<string> OnStatsStoreFailed;

        private static bool statsLoaded = false;

        // Store active callbacks to prevent garbage collection when passed to native code
        private static readonly List<LudolioNative.StatsCallback> activeStatsCallbacks = new List<LudolioNative.StatsCallback>();
        private static readonly List<LudolioNative.StoreStatsCallback> activeStoreCallbacks = new List<LudolioNative.StoreStatsCallback>();
        private static readonly object callbackLock = new object();

        /// <summary>
        /// Request stats from the server. Must be called before GetStat/SetStat.
        /// Similar to Steamworks RequestCurrentStats().
        /// </summary>
        /// <param name="callback">Optional callback with success status</param>
        public static void RequestStats(Action<bool> callback = null)
        {
            if (!LudolioSDK.IsInitialized)
            {
                Debug.LogError("[LudolioStats] SDK not initialized. Call LudolioSDK.Init() first.");
                callback?.Invoke(false);
                return;
            }

            if (!LudolioSDK.IsAuthenticated)
            {
                Debug.LogError("[LudolioStats] User not authenticated.");
                callback?.Invoke(false);
                return;
            }

            LudolioNative.StatsCallback nativeCallback = null;
            nativeCallback = (jsonData) =>
            {
                lock (callbackLock)
                {
                    activeStatsCallbacks.Remove(nativeCallback);
                }

                bool success = !string.IsNullOrEmpty(jsonData);
                if (success)
                {
                    statsLoaded = true;
                    Debug.Log("[LudolioStats] Stats loaded successfully");
                    OnStatsReceived?.Invoke();
                }
                else
                {
                    string error = LudolioSDK.GetLastError();
                    Debug.LogError($"[LudolioStats] Failed to load stats: {error}");
                }

                callback?.Invoke(success);
            };

            lock (callbackLock)
            {
                activeStatsCallbacks.Add(nativeCallback);
            }

            LudolioNative.Ludolio_RequestStats(nativeCallback);
        }

        /// <summary>
        /// Get an integer stat value. RequestStats must be called first.
        /// Similar to Steamworks GetStat(int).
        /// </summary>
        /// <param name="statId">The API name of the stat</param>
        /// <param name="value">Output value</param>
        /// <returns>True if successful</returns>
        public static bool GetStatInt(string statId, out int value)
        {
            value = 0;

            if (!LudolioSDK.IsInitialized)
            {
                Debug.LogError("[LudolioStats] SDK not initialized.");
                return false;
            }

            if (!statsLoaded)
            {
                Debug.LogError("[LudolioStats] Stats not loaded. Call RequestStats() first.");
                return false;
            }

            return LudolioNative.Ludolio_GetStatInt(statId, out value);
        }

        /// <summary>
        /// Get a float stat value. RequestStats must be called first.
        /// Similar to Steamworks GetStat(float).
        /// </summary>
        /// <param name="statId">The API name of the stat</param>
        /// <param name="value">Output value</param>
        /// <returns>True if successful</returns>
        public static bool GetStatFloat(string statId, out float value)
        {
            value = 0f;

            if (!LudolioSDK.IsInitialized)
            {
                Debug.LogError("[LudolioStats] SDK not initialized.");
                return false;
            }

            if (!statsLoaded)
            {
                Debug.LogError("[LudolioStats] Stats not loaded. Call RequestStats() first.");
                return false;
            }

            return LudolioNative.Ludolio_GetStatFloat(statId, out value);
        }

        /// <summary>
        /// Set an integer stat value. Changes are cached locally until StoreStats is called.
        /// Similar to Steamworks SetStat(int).
        /// </summary>
        /// <param name="statId">The API name of the stat</param>
        /// <param name="value">The value to set</param>
        /// <returns>True if successful</returns>
        public static bool SetStatInt(string statId, int value)
        {
            if (!LudolioSDK.IsInitialized)
            {
                Debug.LogError("[LudolioStats] SDK not initialized.");
                return false;
            }

            if (!statsLoaded)
            {
                Debug.LogError("[LudolioStats] Stats not loaded. Call RequestStats() first.");
                return false;
            }

            return LudolioNative.Ludolio_SetStatInt(statId, value);
        }

        /// <summary>
        /// Set a float stat value. Changes are cached locally until StoreStats is called.
        /// Similar to Steamworks SetStat(float).
        /// </summary>
        /// <param name="statId">The API name of the stat</param>
        /// <param name="value">The value to set</param>
        /// <returns>True if successful</returns>
        public static bool SetStatFloat(string statId, float value)
        {
            if (!LudolioSDK.IsInitialized)
            {
                Debug.LogError("[LudolioStats] SDK not initialized.");
                return false;
            }

            if (!statsLoaded)
            {
                Debug.LogError("[LudolioStats] Stats not loaded. Call RequestStats() first.");
                return false;
            }

            return LudolioNative.Ludolio_SetStatFloat(statId, value);
        }

        /// <summary>
        /// Store all modified stats to the server.
        /// Similar to Steamworks StoreStats().
        /// </summary>
        /// <param name="callback">Optional callback with success status</param>
        public static void StoreStats(Action<bool> callback = null)
        {
            if (!LudolioSDK.IsInitialized)
            {
                Debug.LogError("[LudolioStats] SDK not initialized.");
                callback?.Invoke(false);
                return;
            }

            if (!statsLoaded)
            {
                Debug.LogError("[LudolioStats] Stats not loaded. Call RequestStats() first.");
                callback?.Invoke(false);
                return;
            }

            LudolioNative.StoreStatsCallback nativeCallback = null;
            nativeCallback = (success, jsonData) =>
            {
                lock (callbackLock)
                {
                    activeStoreCallbacks.Remove(nativeCallback);
                }

                if (success)
                {
                    Debug.Log("[LudolioStats] Stats stored successfully");
                    OnStatsStored?.Invoke();
                }
                else
                {
                    Debug.LogError($"[LudolioStats] Failed to store stats: {jsonData}");
                    OnStatsStoreFailed?.Invoke(jsonData);
                }

                callback?.Invoke(success);
            };

            lock (callbackLock)
            {
                activeStoreCallbacks.Add(nativeCallback);
            }

            LudolioNative.Ludolio_StoreStats(nativeCallback);
        }

        /// <summary>
        /// Reset the stats loaded flag. Called internally when SDK shuts down.
        /// </summary>
        internal static void Reset()
        {
            statsLoaded = false;
        }
    }
}

