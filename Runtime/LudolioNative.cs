using System;
using System.Runtime.InteropServices;

namespace Ludolio.SDK
{
    /// <summary>
    /// Native DLL interface for Ludolio SDK.
    /// This class provides P/Invoke declarations for the native C++ SDK.
    /// </summary>
    internal static class LudolioNative
    {
        private const string DLL_NAME = "LudolioSDK";

        // Callback delegates matching C++ function pointers
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AuthCallback([MarshalAs(UnmanagedType.I1)] bool success);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AchievementCallback([MarshalAs(UnmanagedType.I1)] bool success);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void UserInfoCallback(
            [MarshalAs(UnmanagedType.LPStr)] string userId,
            [MarshalAs(UnmanagedType.LPStr)] string userName,
            [MarshalAs(UnmanagedType.LPStr)] string email
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AchievementListCallback(
            [MarshalAs(UnmanagedType.LPStr)] string jsonData
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void StatsCallback(
            [MarshalAs(UnmanagedType.LPStr)] string jsonData
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SetStatCallback(
            [MarshalAs(UnmanagedType.I1)] bool success,
            [MarshalAs(UnmanagedType.LPStr)] string errorOrValue
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void StoreStatsCallback(
            [MarshalAs(UnmanagedType.I1)] bool success,
            [MarshalAs(UnmanagedType.LPStr)] string jsonData
        );

        // Core SDK functions
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Ludolio_Init(
            [MarshalAs(UnmanagedType.LPStr)] string gameId,
            [MarshalAs(UnmanagedType.LPStr)] string sessionToken
        );

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Ludolio_InitWithAppId(
            int appId,
            [MarshalAs(UnmanagedType.LPStr)] string sessionToken
        );

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Ludolio_GetGameId();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Ludolio_Shutdown();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Ludolio_IsInitialized();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Ludolio_IsAuthenticated();

        // Authentication
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Ludolio_Authenticate(AuthCallback callback);

        // User Info
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Ludolio_GetUserId();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Ludolio_GetUserName();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Ludolio_GetUserInfo(UserInfoCallback callback);

        // Achievements
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Ludolio_UnlockAchievement(
            [MarshalAs(UnmanagedType.LPStr)] string achievementId,
            AchievementCallback callback
        );

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Ludolio_GetAchievements(AchievementListCallback callback);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Ludolio_IsAchievementUnlocked(
            [MarshalAs(UnmanagedType.LPStr)] string achievementId
        );

        // Stats (like Steamworks stats)
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Ludolio_RequestStats(StatsCallback callback);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Ludolio_GetStatInt(
            [MarshalAs(UnmanagedType.LPStr)] string statId,
            out int value
        );

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Ludolio_GetStatFloat(
            [MarshalAs(UnmanagedType.LPStr)] string statId,
            out float value
        );

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Ludolio_SetStatInt(
            [MarshalAs(UnmanagedType.LPStr)] string statId,
            int value
        );

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Ludolio_SetStatFloat(
            [MarshalAs(UnmanagedType.LPStr)] string statId,
            float value
        );

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Ludolio_StoreStats(StoreStatsCallback callback);

        // Utility
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Ludolio_GetLastError();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Ludolio_FreeString(IntPtr str);
    }
}

