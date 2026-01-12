using UnityEngine;
using System.Runtime.CompilerServices;
using System.IO;
using System;
using Object = UnityEngine.Object;

namespace Capriccioso
{
    /// <summary>
    /// Static colored logging utility for Capriccioso.
    /// Provides multiple log levels with color-coded output for Unity Editor
    /// and ANSI-colored output for server/batch mode.
    /// </summary>
    /// <example>
    /// <code>
    /// // Basic logging at different levels
    /// CLogger.Log("This is a normal log message");
    /// CLogger.LogInfo("Player connected to server");
    /// CLogger.LogSuccess("Level completed successfully");
    /// CLogger.LogWarning("Low memory detected");
    /// CLogger.LogError("Failed to load asset");
    /// 
    /// // Backend-specific logging for API calls
    /// CLogger.LogBackendInfo("Fetching user data from API");
    /// CLogger.LogBackendSuccess("User data retrieved successfully");
    /// CLogger.LogBackendError("API request failed: 404");
    /// 
    /// // Major action logging (appears in uppercase)
    /// CLogger.LogMajorAction("Game Started");
    /// 
    /// // Event logging
    /// CLogger.LogEvent("OnPlayerDeath triggered");
    /// 
    /// // Logging with context object (clickable in Unity console)
    /// CLogger.LogInfo("Player spawned", playerGameObject);
    /// </code>
    /// </example>
    public static class CLogger
    {
        /// <summary>Enable or disable all logging.</summary>
        public static bool LogEnabled = true;
        
        /// <summary>Enable or disable file logging in server mode.</summary>
        public static bool FileLoggingEnabled = true;
        
        private static bool s_isInitialized = false;
        private static string s_logFilePath = null;
        private static readonly object s_fileLock = new object();

        /// <summary>
        /// Returns true when running in server/batch mode (not in editor).
        /// Used to switch between rich text and ANSI color formatting.
        /// </summary>
        private static bool IsServerMode => Application.isBatchMode || !Application.isEditor;

        /// <summary>
        /// Initializes the logger. Called automatically on first log.
        /// Sets up file logging for server mode.
        /// </summary>
        public static void Init()
        {
            if (s_isInitialized) return;
            
            Debug.developerConsoleVisible = true;
            InitializeLogFile();
            s_isInitialized = true;
        }

        /// <summary>
        /// Resets the logger state. Called automatically on domain reload.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            s_isInitialized = false;
            s_logFilePath = null;
        }

        #region Log Methods

        /// <summary>
        /// Logs a normal message (gray).
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="context">Optional Unity object for console click-through.</param>
        public static void Log(string message, Object context = null, 
            [CallerFilePath] string path = null, 
            [CallerLineNumber] int lineNumber = 0, 
            [CallerMemberName] string caller = null)
        {
            if (!LogEnabled) return;
            Init();
            string file = Path.GetFileName(path);
            string log = FormatLogMessage("LOGG", Constants.LogColors.Log, message, file, caller, lineNumber);
            Debug.Log(log, context);
        }

        /// <summary>
        /// Logs an informational message (blue).
        /// Use for notable events and state changes.
        /// </summary>
        public static void LogInfo(string message, Object context = null, 
            [CallerFilePath] string path = null, 
            [CallerLineNumber] int lineNumber = 0, 
            [CallerMemberName] string caller = null)
        {
            if (!LogEnabled) return;
            Init();
            string file = Path.GetFileName(path);
            string log = FormatLogMessage("INFO", Constants.LogColors.Info, message, file, caller, lineNumber);
            Debug.Log(log, context);
        }

        /// <summary>
        /// Logs a backend informational message (cyan).
        /// Use for API/database operations starting.
        /// </summary>
        public static void LogBackendInfo(string message, Object context = null, 
            [CallerFilePath] string path = null, 
            [CallerLineNumber] int lineNumber = 0, 
            [CallerMemberName] string caller = null)
        {
            if (!LogEnabled) return;
            Init();
            string file = Path.GetFileName(path);
            string log = FormatLogMessage("DB INFO", Constants.LogColors.BackendInfo, message, file, caller, lineNumber);
            Debug.Log(log, context);
        }

        /// <summary>
        /// Logs a success message (green).
        /// Use when operations complete successfully.
        /// </summary>
        public static void LogSuccess(string message, Object context = null, 
            [CallerFilePath] string path = null, 
            [CallerLineNumber] int lineNumber = 0, 
            [CallerMemberName] string caller = null)
        {
            if (!LogEnabled) return;
            Init();
            string file = Path.GetFileName(path);
            string log = FormatLogMessage("GOOD", Constants.LogColors.Success, message, file, caller, lineNumber);
            Debug.Log(log, context);
        }

        /// <summary>
        /// Logs a backend success message (light green).
        /// Use for successful API/database operations.
        /// </summary>
        public static void LogBackendSuccess(string message, Object context = null, 
            [CallerFilePath] string path = null, 
            [CallerLineNumber] int lineNumber = 0, 
            [CallerMemberName] string caller = null)
        {
            if (!LogEnabled) return;
            Init();
            string file = Path.GetFileName(path);
            string log = FormatLogMessage("DB GOOD", Constants.LogColors.BackendSuccess, message, file, caller, lineNumber);
            Debug.Log(log, context);
        }

        /// <summary>
        /// Logs an event message (magenta).
        /// Use when events are triggered.
        /// </summary>
        public static void LogEvent(string message, Object context = null, 
            [CallerFilePath] string path = null, 
            [CallerLineNumber] int lineNumber = 0, 
            [CallerMemberName] string caller = null)
        {
            if (!LogEnabled) return;
            Init();
            string file = Path.GetFileName(path);
            string log = FormatLogMessage("EVENT", Constants.LogColors.Event, message, file, caller, lineNumber);
            Debug.Log(log, context);
        }

        /// <summary>
        /// Logs a warning message (yellow).
        /// Use for non-ideal but non-problematic situations.
        /// </summary>
        public static void LogWarning(string message, Object context = null, 
            [CallerFilePath] string path = null, 
            [CallerLineNumber] int lineNumber = 0, 
            [CallerMemberName] string caller = null)
        {
            if (!LogEnabled) return;
            Init();
            string file = Path.GetFileName(path);
            string log = FormatLogMessage("WARN", Constants.LogColors.Warning, message, file, caller, lineNumber);
            Debug.LogWarning(log, context);
        }

        /// <summary>
        /// Logs an error message (red).
        /// Use for errors and exceptions.
        /// </summary>
        public static void LogError(string message, Object context = null, 
            [CallerFilePath] string path = null, 
            [CallerLineNumber] int lineNumber = 0, 
            [CallerMemberName] string caller = null)
        {
            if (!LogEnabled) return;
            Init();
            string file = Path.GetFileName(path);
            string log = FormatLogMessage("ERRR", Constants.LogColors.Error, message, file, caller, lineNumber);
            Debug.LogError(log, context);
        }

        /// <summary>
        /// Logs a backend error message (pink).
        /// Use for API/database errors.
        /// </summary>
        public static void LogBackendError(string message, Object context = null, 
            [CallerFilePath] string path = null, 
            [CallerLineNumber] int lineNumber = 0, 
            [CallerMemberName] string caller = null)
        {
            if (!LogEnabled) return;
            Init();
            string file = Path.GetFileName(path);
            string log = FormatLogMessage("DB ERRR", Constants.LogColors.BackendError, message, file, caller, lineNumber);
            Debug.LogError(log, context);
        }

        /// <summary>
        /// Logs a major action message (bright yellow, uppercase).
        /// Use for significant milestones and important events.
        /// </summary>
        public static void LogMajorAction(string message, Object context = null, 
            [CallerFilePath] string path = null, 
            [CallerLineNumber] int lineNumber = 0, 
            [CallerMemberName] string caller = null)
        {
            if (!LogEnabled) return;
            Init();
            string file = Path.GetFileName(path);
            string log = FormatLogMessage("WAH", Constants.LogColors.MajorAction, message.ToUpper(), file, caller, lineNumber);
            Debug.Log(log, context);
        }

        /// <summary>
        /// Logs data/class dump message (purple).
        /// Use for dumping object contents for debugging.
        /// </summary>
        public static void LogData(string message, Object context = null, 
            [CallerFilePath] string path = null, 
            [CallerLineNumber] int lineNumber = 0, 
            [CallerMemberName] string caller = null)
        {
            if (!LogEnabled) return;
            Init();
            string file = Path.GetFileName(path);
            string log = FormatLogMessage("DATA", Constants.LogColors.Data, message, file, caller, lineNumber);
            Debug.Log(log, context);
        }

        #endregion

        #region Private Methods

        private static void InitializeLogFile()
        {
            if (!IsServerMode || !FileLoggingEnabled) return;
            
            try
            {
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                s_logFilePath = Path.Combine(projectRoot, $"server_log_{timestamp}.txt");
                
                string header = $"=== Capriccioso Server Log ===\n" +
                              $"Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                              $"Unity Version: {Application.unityVersion}\n" +
                              $"Platform: {Application.platform}\n" +
                              $"==========================================\n\n";
                
                File.WriteAllText(s_logFilePath, header);
                Debug.Log($"Log file initialized: {s_logFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize log file: {ex.Message}");
                s_logFilePath = null;
            }
        }

        private static void WriteToFile(string message)
        {
            if (!IsServerMode || string.IsNullOrEmpty(s_logFilePath) || !FileLoggingEnabled) return;
            
            try
            {
                lock (s_fileLock)
                {
                    File.AppendAllText(s_logFilePath, message + "\n");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to write to log file: {ex.Message}");
            }
        }

        private static string GetAnsiColor(string hexColor)
        {
            return hexColor switch
            {
                Constants.LogColors.Log => Constants.AnsiColors.BrightBlack,
                Constants.LogColors.Info => Constants.AnsiColors.BrightBlue,
                Constants.LogColors.BackendInfo => Constants.AnsiColors.BrightCyan,
                Constants.LogColors.Success => Constants.AnsiColors.BrightGreen,
                Constants.LogColors.BackendSuccess => Constants.AnsiColors.BrightGreen,
                Constants.LogColors.Warning => Constants.AnsiColors.BrightYellow,
                Constants.LogColors.Error => Constants.AnsiColors.BrightRed,
                Constants.LogColors.BackendError => Constants.AnsiColors.BrightMagenta,
                Constants.LogColors.MajorAction => Constants.AnsiColors.BrightYellow,
                Constants.LogColors.Event => Constants.AnsiColors.BrightMagenta,
                Constants.LogColors.Data => Constants.AnsiColors.BrightMagenta,
                _ => Constants.AnsiColors.BrightWhite
            };
        }

        private static string FormatLogMessage(string level, string color, string message, 
            string file, string caller, int lineNumber)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            if (IsServerMode)
            {
                string ansiColor = GetAnsiColor(color);
                string reset = Constants.AnsiColors.Reset;
                string formattedMessage = $"{ansiColor}[{timestamp}][{caller}():{lineNumber}][{level}]{reset} {message}";
                
                string fileMessage = $"[{timestamp}][{file}::{caller}():{lineNumber}][{level}] {message}";
                WriteToFile(fileMessage);
                
                return formattedMessage;
            }
            else
            {
                return $"<color={color}><b>[{file} @ {caller}():{lineNumber}]</b> {level} </color> - {message}";
            }
        }

        #endregion
    }
}
