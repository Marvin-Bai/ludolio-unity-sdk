using System;
using UnityEngine;

namespace Ludolio.SDK
{
    /// <summary>
    /// Main SDK class for Ludolio integration. Similar to Steamworks API.
    /// Initialize this SDK at the start of your game to enable authentication and features.
    /// Uses native DLL for secure named pipe communication with Desktop App.
    /// </summary>
    public class LudolioSDK : MonoBehaviour
    {
        private static LudolioSDK instance;

        // Authentication data from command line
        private string sessionToken;
        private string gameId;

        // Events
        public static event Action OnInitialized;
        public static event Action<bool> OnAuthenticationComplete;
        public static event Action OnClientDisconnected;

        /// <summary>
        /// Gets the singleton instance of the SDK
        /// </summary>
        public static LudolioSDK Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject sdkObject = new GameObject("LudolioSDK");
                    instance = sdkObject.AddComponent<LudolioSDK>();
                    DontDestroyOnLoad(sdkObject);
                }
                return instance;
            }
        }

        /// <summary>
        /// Returns true if the SDK has been initialized
        /// </summary>
        public static bool IsInitialized => LudolioNative.Ludolio_IsInitialized();

        /// <summary>
        /// Returns true if the user is authenticated
        /// </summary>
        public static bool IsAuthenticated => LudolioNative.Ludolio_IsAuthenticated();

        /// <summary>
        /// Gets the current user ID
        /// </summary>
        public static string GetUserId() => LudolioNative.Ludolio_GetUserId();

        /// <summary>
        /// Gets the current game ID
        /// </summary>
        public static string GetGameId() => Instance.gameId;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Initialize the SDK. Call this at the start of your game.
        /// </summary>
        /// <param name="appId">Your game's App ID (like Steam's appid)</param>
        /// <returns>True if initialization was successful</returns>
        public static bool Init(int appId)
        {
            if (IsInitialized)
            {
                Debug.LogWarning("[LudolioSDK] Already initialized");
                return true;
            }

            Instance.ReadCommandLineArguments();

            // Validate that we have session token
            bool hasSessionToken = !string.IsNullOrEmpty(Instance.sessionToken);

            if (!hasSessionToken)
            {
                Debug.LogError("[LudolioSDK] Failed to initialize: Missing session token.");
                Debug.LogError("[LudolioSDK] Make sure the game is launched from the Ludolio Desktop Client.");
                Debug.LogError("[LudolioSDK] Required argument: --ludolio-session");

#if !UNITY_EDITOR
                Debug.LogError("[LudolioSDK] Game will quit in 3 seconds due to missing authentication.");
                Application.Quit();
#endif
                return false;
            }

            // Initialize native SDK (it will parse token and verify appId matches)
            bool success = LudolioNative.Ludolio_InitWithAppId(appId, Instance.sessionToken);

            if (!success)
            {
                string error = LudolioNative.Ludolio_GetLastError();
                Debug.LogError($"[LudolioSDK] Failed to initialize: {error}");
                Debug.LogError("[LudolioSDK] This could mean:");
                Debug.LogError("[LudolioSDK] - Invalid or expired session token");
                Debug.LogError("[LudolioSDK] - App ID mismatch (expected {appId})");
                Debug.LogError("[LudolioSDK] - Device ID mismatch (token from different device)");
                Debug.LogError("[LudolioSDK] - Desktop App not running");

#if !UNITY_EDITOR
                Application.Quit();
#endif
                return false;
            }

            // Get gameId from native SDK (extracted from session token)
            Instance.gameId = LudolioNative.Ludolio_GetGameId();

            Debug.Log($"[LudolioSDK] ✓ Initialized successfully for App ID: {appId}");
            Debug.Log($"[LudolioSDK] ✓ Session token verified");
            Debug.Log($"[LudolioSDK] ✓ Connected to Desktop App");

            // Start authentication process
            Instance.Authenticate();

            OnInitialized?.Invoke();
            return true;
        }

        /// <summary>
        /// Shutdown the SDK. Call this when your game is closing.
        /// </summary>
        public static void Shutdown()
        {
            if (IsInitialized)
            {
                Debug.Log("[LudolioSDK] Shutting down");
                LudolioNative.Ludolio_Shutdown();
            }
        }

        private void ReadCommandLineArguments()
        {
            string[] args = Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--ludolio-session" && i + 1 < args.Length)
                {
                    sessionToken = args[i + 1];
                    Debug.Log($"[LudolioSDK] Session token found in command line arguments");
                }
            }
        }

        private void Authenticate()
        {
            LudolioNative.Ludolio_Authenticate((success) =>
            {
                if (success)
                {
                    Debug.Log("[LudolioSDK] Authentication successful!");
                    OnAuthenticationComplete?.Invoke(true);
                }
                else
                {
                    string error = LudolioNative.Ludolio_GetLastError();
                    Debug.LogError($"[LudolioSDK] Authentication failed: {error}");
                    OnAuthenticationComplete?.Invoke(false);

#if !UNITY_EDITOR
                    Application.Quit();
#endif
                }
            });
        }

        /// <summary>
        /// Get the last error message from the native SDK
        /// </summary>
        public static string GetLastError()
        {
            return LudolioNative.Ludolio_GetLastError();
        }

        private void OnApplicationQuit()
        {
            Shutdown();
        }
    }
}

