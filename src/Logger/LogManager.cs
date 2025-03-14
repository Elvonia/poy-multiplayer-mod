using System;
using System.Runtime.CompilerServices;

#if DEBUG_BEPINEX

using BepInEx;
using BepInEx.Logging;

#elif DEBUG_MELONLOADER

using MelonLoader;

#endif

namespace Multiplayer.Logger
{
    public static class LogManager
    {

#if DEBUG_BEPINEX
        
        private static readonly ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource("Multiplayer Mod Test");
        private static readonly Action<string> logInfo = logger.LogInfo;
        private static readonly Action<string> logWarning = logger.LogWarning;
        private static readonly Action<string> logError = logger.LogError;
        private static readonly Action<string> logDebug = logger.LogDebug;

        public static void Info(string message, [CallerMemberName] string caller = "")
        => logInfo($"[{caller}] {message}");

        public static void Warning(string message, [CallerMemberName] string caller = "")
            => logWarning($"[{caller}] {message}");

        public static void Error(string message, [CallerMemberName] string caller = "")
            => logError($"[{caller}] {message}");

        public static void Debug(string message, [CallerMemberName] string caller = "")
            => logDebug($"[{caller}] {message}");

#elif DEBUG_MELONLOADER

        private static readonly MelonLogger.Instance logger = new MelonLogger.Instance("Multiplayer Mod Test");
        private static readonly Action<string> logInfo = logger.Msg;
        private static readonly Action<string> logWarning = logger.Warning;
        private static readonly Action<string> logError = logger.Error;

        private static readonly Action<string> logDebug = message => logger.Msg($"[DEBUG] {message}");

        public static void Info(string message, [CallerMemberName] string caller = "")
        => logInfo($"[{caller}] {message}");

        public static void Warning(string message, [CallerMemberName] string caller = "")
            => logWarning($"[{caller}] {message}");

        public static void Error(string message, [CallerMemberName] string caller = "")
            => logError($"[{caller}] {message}");

        public static void Debug(string message, [CallerMemberName] string caller = "")
            => logDebug($"[{caller}] {message}");

#else

        public static void Info(string message)
        {
            return;
        }

        public static void Warning(string message)
        {
            return;
        }

        public static void Error(string message)
        {
            return;
        }

        public static void Debug(string message) 
        {
            return;
        }

#endif

    }
}
