using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Ludolio.SDK
{
    /// <summary>
    /// User API for Ludolio. Get information about the current user.
    /// </summary>
    public static class LudolioUser
    {
        private static UserInfo cachedUserInfo;

        /// <summary>
        /// Get information about the current user
        /// </summary>
        /// <param name="callback">Callback with user information</param>
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

            // Return cached data if available
            if (cachedUserInfo != null)
            {
                callback?.Invoke(cachedUserInfo);
                return;
            }

            LudolioSDK.Instance.StartCoroutine(GetUserInfoCoroutine(callback));
        }

        /// <summary>
        /// Get the current user's ID
        /// </summary>
        /// <returns>User ID string</returns>
        public static string GetUserId()
        {
            return LudolioSDK.GetUserId();
        }
        
        /// <summary>
        /// Get the current user's name (from cached data)
        /// </summary>
        /// <returns>User name or null if not cached</returns>
        public static string GetUserName()
        {
            return cachedUserInfo?.name;
        }
        
        /// <summary>
        /// Get the current user's email (from cached data)
        /// </summary>
        /// <returns>User email or null if not cached</returns>
        public static string GetUserEmail()
        {
            return cachedUserInfo?.email;
        }
        
        /// <summary>
        /// Clear the cached user info
        /// </summary>
        public static void ClearCache()
        {
            cachedUserInfo = null;
        }
        
        private static IEnumerator GetUserInfoCoroutine(Action<UserInfo> callback)
        {
            int port = LudolioSDK.Instance.GetClientPort();
            string url = $"http://localhost:{port}/api/user/info";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                // Add authorization header
                string token = LudolioSDK.Instance.GetAuthToken();
                request.SetRequestHeader("Authorization", $"Bearer {token}");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<UserInfoResponse>(request.downloadHandler.text);
                    cachedUserInfo = response.user;

                    Debug.Log($"[LudolioUser] User info loaded: {cachedUserInfo.name}");
                    callback?.Invoke(cachedUserInfo);
                }
                else
                {
                    Debug.LogError($"[LudolioUser] Failed to get user info: {request.error}");
                    callback?.Invoke(null);
                }
            }
        }
        
        [Serializable]
        private class UserInfoResponse
        {
            public UserInfo user;
        }
    }
    
    /// <summary>
    /// User information data structure
    /// </summary>
    [Serializable]
    public class UserInfo
    {
        public string id;
        public string email;
        public string name;
        public string role;
        public string avatarUrl;
    }
}

