using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Ludolio.SDK
{
    /// <summary>
    /// Main SDK class for Ludolio integration. Similar to Steamworks API.
    /// Initialize this SDK at the start of your game to enable authentication and features.
    /// </summary>
    public class LudolioSDK : MonoBehaviour
    {
        private static LudolioSDK instance;
        
        // Authentication data
        private string authToken;
        private string userId;
        private int clientPort = 3000;
        private string gameId;
        
        // State
        private bool isInitialized = false;
        private bool isAuthenticated = false;
        
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
        public static bool IsInitialized => Instance.isInitialized;
        
        /// <summary>
        /// Returns true if the user is authenticated
        /// </summary>
        public static bool IsAuthenticated => Instance.isAuthenticated;
        
        /// <summary>
        /// Gets the current user ID
        /// </summary>
        public static string GetUserId() => Instance.userId;
        
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
        /// <param name="gameId">Your game's unique identifier</param>
        /// <returns>True if initialization was successful</returns>
        public static bool Init(string gameId)
        {
            if (Instance.isInitialized)
            {
                Debug.LogWarning("[LudolioSDK] Already initialized");
                return true;
            }

            Instance.gameId = gameId;
            Instance.ReadCommandLineArguments();

            // Validate that we have ALL required data
            bool hasToken = !string.IsNullOrEmpty(Instance.authToken);
            bool hasUserId = !string.IsNullOrEmpty(Instance.userId);
            bool hasValidPort = Instance.clientPort > 0 && Instance.clientPort < 65536;

            if (!hasToken || !hasUserId || !hasValidPort)
            {
                Debug.LogError("[LudolioSDK] Failed to initialize: Missing required authentication data.");
                Debug.LogError($"[LudolioSDK] - Token: {(hasToken ? "✓" : "✗ MISSING")}");
                Debug.LogError($"[LudolioSDK] - User ID: {(hasUserId ? "✓" : "✗ MISSING")}");
                Debug.LogError($"[LudolioSDK] - Client Port: {(hasValidPort ? "✓" : "✗ MISSING/INVALID")}");
                Debug.LogError("[LudolioSDK] Make sure the game is launched from the Ludolio client.");
                Debug.LogError("[LudolioSDK] Required arguments: --ludolio-token, --ludolio-user, --ludolio-client-port");

                #if !UNITY_EDITOR
                Debug.LogError("[LudolioSDK] Game will quit in 3 seconds due to missing authentication.");
                Instance.StartCoroutine(Instance.QuitAfterDelay(1f));
                #endif

                return false;
            }

            Instance.isInitialized = true;
            Debug.Log($"[LudolioSDK] Initialized successfully for game: {gameId}");
            Debug.Log($"[LudolioSDK] User ID: {Instance.userId}");
            Debug.Log($"[LudolioSDK] Client Port: {Instance.clientPort}");

            // Start authentication process
            Instance.StartCoroutine(Instance.AuthenticateCoroutine());

            // Start periodic health check
            Instance.StartCoroutine(Instance.PeriodicHealthCheck());

            OnInitialized?.Invoke();
            return true;
        }
        
        /// <summary>
        /// Shutdown the SDK. Call this when your game is closing.
        /// </summary>
        public static void Shutdown()
        {
            if (Instance.isInitialized)
            {
                Debug.Log("[LudolioSDK] Shutting down");
                Instance.isInitialized = false;
                Instance.isAuthenticated = false;
            }
        }

        private void ReadCommandLineArguments()
        {
            string[] args = Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--ludolio-token" && i + 1 < args.Length)
                {
                    authToken = args[i + 1];
                }
                else if (args[i] == "--ludolio-user" && i + 1 < args.Length)
                {
                    userId = args[i + 1];
                }
                else if (args[i] == "--ludolio-client-port" && i + 1 < args.Length)
                {
                    if (int.TryParse(args[i + 1], out int port))
                    {
                        clientPort = port;
                    }
                }
            }

            Debug.Log($"[LudolioSDK] Command line args parsed - Port: {clientPort}, User: {userId}");
        }
        
        private IEnumerator AuthenticateCoroutine()
        {
            string url = $"http://localhost:{clientPort}/api/validate-token";
            
            var requestData = new TokenValidationRequest
            {
                token = authToken,
                userId = userId,
                gameId = gameId
            };
            
            string jsonData = JsonUtility.ToJson(requestData);
            
            using (UnityWebRequest request = UnityWebRequest.Post(url, jsonData, "application/json"))
            {
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<TokenValidationResponse>(request.downloadHandler.text);
                    
                    if (response.valid)
                    {
                        isAuthenticated = true;
                        Debug.Log("[LudolioSDK] Authentication successful!");
                        OnAuthenticationComplete?.Invoke(true);
                    }
                    else
                    {
                        Debug.LogError($"[LudolioSDK] Authentication failed: {response.message}");
                        OnAuthenticationComplete?.Invoke(false);
                        Application.Quit();
                    }
                }
                else
                {
                    Debug.LogError($"[LudolioSDK] Authentication request failed: {request.error}");
                    OnAuthenticationComplete?.Invoke(false);
                    Application.Quit();
                }
            }
        }
        
        private IEnumerator PeriodicHealthCheck()
        {
            while (isInitialized)
            {
                yield return new WaitForSeconds(30f); // Check every 30 seconds

                string url = $"http://localhost:{clientPort}/api/health";

                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    request.timeout = 5;
                    yield return request.SendWebRequest();

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogWarning("[LudolioSDK] Client disconnected. Closing game...");
                        OnClientDisconnected?.Invoke();
                        Application.Quit();
                        yield break;
                    }
                }
            }
        }

        private IEnumerator QuitAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Debug.LogError("[LudolioSDK] Quitting game due to authentication failure.");
            Application.Quit();
        }

        internal string GetAuthToken() => authToken;
        internal int GetClientPort() => clientPort;
        
        [Serializable]
        private class TokenValidationRequest
        {
            public string token;
            public string userId;
            public string gameId;
        }
        
        [Serializable]
        private class TokenValidationResponse
        {
            public bool valid;
            public string message;
        }
    }
}

