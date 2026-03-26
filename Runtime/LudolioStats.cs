using System;
using System.Collections.Generic;
using UnityEngine;
using AOT;

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

        // Request state for IL2CPP-safe static callbacks.
        // RequestStats is coalesced; StoreStats is serialized.
        private static readonly List<Action<bool>> pendingRequestStatsCallbacks = new List<Action<bool>>();
        private static bool isRequestStatsInProgress;

        private sealed class StoreStatsRequest
        {
            public readonly Action<bool> Callback;

            public StoreStatsRequest(Action<bool> callback)
            {
                Callback = callback;
            }
        }

        private static readonly Queue<StoreStatsRequest> pendingStoreStatsRequests = new Queue<StoreStatsRequest>();
        private static StoreStatsRequest activeStoreStatsRequest;
        private static readonly object callbackLock = new object();

        private static void StartNextStoreStatsRequest()
        {
            bool shouldStart;

            lock (callbackLock)
            {
                shouldStart = activeStoreStatsRequest == null && pendingStoreStatsRequests.Count > 0;
                if (shouldStart)
                {
                    activeStoreStatsRequest = pendingStoreStatsRequests.Dequeue();
                }
            }

            if (shouldStart)
            {
                LudolioNative.Ludolio_StoreStats(OnStoreStatsCallbackStatic);
            }
        }

        /// <summary>
        /// IL2CPP-safe static callback for RequestStats.
        /// </summary>
        [MonoPInvokeCallback(typeof(LudolioNative.StatsCallback))]
        private static void OnRequestStatsCallbackStatic(string jsonData)
        {
            // Capture data needed on the main thread (native strings may not survive cross-thread)
            bool success = !string.IsNullOrEmpty(jsonData);
            string error = success ? null : LudolioSDK.GetLastError();

            List<Action<bool>> callbacks;

            lock (callbackLock)
            {
                callbacks = new List<Action<bool>>(pendingRequestStatsCallbacks);
                pendingRequestStatsCallbacks.Clear();
                isRequestStatsInProgress = false;
            }

            // Marshal to main thread since this callback fires on a background thread
            LudolioSDK.EnqueueMainThread(() =>
            {
                if (success)
                {
                    statsLoaded = true;
                    Debug.Log("[LudolioStats] Stats loaded successfully");
                    OnStatsReceived?.Invoke();
                }
                else
                {
                    Debug.LogError($"[LudolioStats] Failed to load stats: {error}");
                }

                foreach (var callback in callbacks)
                {
                    callback?.Invoke(success);
                }
            });
        }

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

            bool shouldStartRequest = false;

            lock (callbackLock)
            {
                pendingRequestStatsCallbacks.Add(callback);
                if (!isRequestStatsInProgress)
                {
                    isRequestStatsInProgress = true;
                    shouldStartRequest = true;
                }
            }

            if (shouldStartRequest)
            {
                LudolioNative.Ludolio_RequestStats(OnRequestStatsCallbackStatic);
            }
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
        /// IL2CPP-safe static callback for StoreStats.
        /// </summary>
        [MonoPInvokeCallback(typeof(LudolioNative.StoreStatsCallback))]
        private static void OnStoreStatsCallbackStatic(bool success, string jsonData)
        {
            // Capture data needed on the main thread
            bool capturedSuccess = success;
            string capturedData = jsonData;

            StoreStatsRequest request;
            bool shouldStartNext;

            lock (callbackLock)
            {
                request = activeStoreStatsRequest;
                activeStoreStatsRequest = null;
                shouldStartNext = pendingStoreStatsRequests.Count > 0;
            }

            // Marshal to main thread since this callback fires on a background thread
            LudolioSDK.EnqueueMainThread(() =>
            {
                if (capturedSuccess)
                {
                    Debug.Log("[LudolioStats] Stats stored successfully");
                    OnStatsStored?.Invoke();
                }
                else
                {
                    Debug.LogError($"[LudolioStats] Failed to store stats: {capturedData}");
                    OnStatsStoreFailed?.Invoke(capturedData);
                }

                request?.Callback?.Invoke(capturedSuccess);

                if (shouldStartNext)
                {
                    StartNextStoreStatsRequest();
                }
            });
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

            bool shouldStartRequest;

            lock (callbackLock)
            {
                pendingStoreStatsRequests.Enqueue(new StoreStatsRequest(callback));
                shouldStartRequest = activeStoreStatsRequest == null;
            }

            if (shouldStartRequest)
            {
                StartNextStoreStatsRequest();
            }
        }

        /// <summary>
        /// Reset the stats loaded flag. Called internally when SDK shuts down.
        /// </summary>
        internal static void Reset()
        {
            statsLoaded = false;

            lock (callbackLock)
            {
                pendingRequestStatsCallbacks.Clear();
                isRequestStatsInProgress = false;
                pendingStoreStatsRequests.Clear();
                activeStoreStatsRequest = null;
            }
        }
    }
}

