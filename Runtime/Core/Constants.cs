using System;

namespace Capriccioso.Runtime.Core
{
    /// <summary>
    /// Centralized constants for the Capriccioso package.
    /// Contains log colors, common values, and configuration defaults.
    /// </summary>
    public static class Constants
    {
        #region Log Colors

        /// <summary>
        /// Color definitions for CLogger output.
        /// Uses hex color codes compatible with Unity's rich text.
        /// </summary>
        public static class LogColors
        {
            /// <summary>Gray - Normal log messages.</summary>
            public const string Log = "#6C757D";
            
            /// <summary>Blue - Informational messages, start of notable events.</summary>
            public const string Info = "#007BFF";
            
            /// <summary>Cyan - Backend/API informational messages.</summary>
            public const string BackendInfo = "#99CCFF";
            
            /// <summary>Green - Successful operations.</summary>
            public const string Success = "#28A745";
            
            /// <summary>Light Green - Successful backend/API operations.</summary>
            public const string BackendSuccess = "#98FF39";
            
            /// <summary>Purple - Data dumps and class content logging.</summary>
            public const string Data = "#563D7C";
            
            /// <summary>Teal - Test-related messages.</summary>
            public const string Test = "#17A2B8";
            
            /// <summary>Yellow - Warnings, non-ideal but non-problematic paths.</summary>
            public const string Warning = "#FFC107";
            
            /// <summary>Red - Errors and exceptions.</summary>
            public const string Error = "#DC3545";
            
            /// <summary>Pink - Backend/API errors.</summary>
            public const string BackendError = "#FF0066";
            
            /// <summary>Bright Yellow - Major actions and milestones.</summary>
            public const string MajorAction = "#FFFF00";
            
            /// <summary>Magenta - Event-related messages.</summary>
            public const string Event = "#FF00EA";
        }

        #endregion

        #region ANSI Colors (Server/Console)

        /// <summary>
        /// ANSI escape codes for server/console colored output.
        /// Used when running in batch mode (e.g., Pterodactyl servers).
        /// </summary>
        public static class AnsiColors
        {
            public const string Reset = "\u001b[0m";
            public const string BrightBlack = "\u001b[90m";
            public const string BrightRed = "\u001b[91m";
            public const string BrightGreen = "\u001b[92m";
            public const string BrightYellow = "\u001b[93m";
            public const string BrightBlue = "\u001b[94m";
            public const string BrightMagenta = "\u001b[95m";
            public const string BrightCyan = "\u001b[96m";
            public const string BrightWhite = "\u001b[97m";
        }

        #endregion

        #region Timing

        /// <summary>Default timer tick interval in seconds.</summary>
        public const float DefaultTimerInterval = 1f;

        /// <summary>Default cooldown duration in seconds.</summary>
        public const float DefaultCooldownDuration = 1f;

        #endregion

        #region Pooling

        /// <summary>Default initial pool size.</summary>
        public const int DefaultPoolSize = 10;

        /// <summary>Default maximum pool size.</summary>
        public const int DefaultMaxPoolSize = 100;

        #endregion

        #region Scene Loading

        /// <summary>Minimum time to show loading screen (seconds).</summary>
        public const float MinLoadingScreenTime = 0.5f;

        /// <summary>Progress threshold to consider scene "ready" (0.9 = 90%).</summary>
        public const float SceneLoadedThreshold = 0.9f;

        #endregion

        #region Debug Display

        /// <summary>FPS update interval in seconds.</summary>
        public const float FpsUpdateInterval = 0.5f;

        /// <summary>Memory display update interval in seconds.</summary>
        public const float MemoryUpdateInterval = 1f;

        #endregion
    }
}
