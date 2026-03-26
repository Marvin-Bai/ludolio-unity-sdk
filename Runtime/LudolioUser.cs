using System;
using UnityEngine;
using System.Runtime.InteropServices;
using AOT;

namespace Ludolio.SDK
{
    /// <summary>
    /// User API for Ludolio. Provides access to current user information.
    /// Uses native DLL for secure communication with Desktop App.
    /// </summary>
    public static class LudolioUser
    {
        private static UserInfo cachedUserInfo = null;
        private static Action<UserInfo> pendingGetUserInfoCallback;
        private static readonly object callbackLock = new object();

        /// <summary>
        /// Get the current user's ID
        /// </summary>
        /// <returns>User ID string</returns>
        public static string GetUserId()
        {
            IntPtr ptr = LudolioNative.Ludolio_GetUserId();
            string id = Marshal.PtrToStringAnsi(ptr);
            LudolioNative.Ludolio_FreeString(ptr);
            return id;
        }

        /// <summary>
        /// Get the current user's name
        /// </summary>
        /// <returns>User name string</returns>
        public static string GetUserName()
        {
            IntPtr ptr = LudolioNative.Ludolio_GetUserName();
            string name = Marshal.PtrToStringAnsi(ptr);
            LudolioNative.Ludolio_FreeString(ptr);
            return name;
        }

        /// <summary>
        /// Get full user information
        /// </summary>
        /// <param name="callback">Callback with user info</param>
        public static void GetUserInfo(Action<UserInfo> callback)
        {
            if (!LudolioSDK.IsInitialized)
            {
                Debug.LogError("[LudolioUser] SDK not initialized.");
                callback?.Invoke(null);
                return;
            }

            if (!LudolioSDK.IsAuthenticated)
            {
                Debug.LogError("[LudolioUser] User not authenticated.");
                callback?.Invoke(null);
                return;
            }

            // Return cached info if available
            if (cachedUserInfo != null)
            {
                callback?.Invoke(cachedUserInfo);
                return;
            }

            // Store callback and use IL2CPP-safe static method
            lock (callbackLock)
            {
                pendingGetUserInfoCallback = callback;
            }

            LudolioNative.Ludolio_GetUserInfo(OnGetUserInfoCallbackStatic);
        }

        /// <summary>
        /// IL2CPP-safe static callback for GetUserInfo.
        /// Native code calls this directly via function pointer.
        /// </summary>
        [MonoPInvokeCallback(typeof(LudolioNative.UserInfoCallback))]
        private static void OnGetUserInfoCallbackStatic(string userId, string userName, string email)
        {
            Action<UserInfo> callback;

            lock (callbackLock)
            {
                callback = pendingGetUserInfoCallback;
                pendingGetUserInfoCallback = null;
            }

            if (string.IsNullOrEmpty(userId))
            {
                string error = LudolioSDK.GetLastError();
                Debug.LogError($"[LudolioUser] Failed to get user info: {error}");
                callback?.Invoke(null);
                return;
            }

            cachedUserInfo = new UserInfo
            {
                id = userId,
                name = userName,
                email = email
            };

            Debug.Log($"[LudolioUser] User info loaded: {userName}");
            callback?.Invoke(cachedUserInfo);
        }

        /// <summary>
        /// Clear the cached user info (forces refresh on next GetUserInfo call)
        /// </summary>
        public static void ClearCache()
        {
            cachedUserInfo = null;
        }

        // Data structures
        [Serializable]
        public class UserInfo
        {
            public string id;
            public string name;
            public string email;
        }
    }
}

