using UnityEngine;
using System.Runtime.CompilerServices;
using System.IO;

namespace Capriccioso
{
    /// <summary>
    /// The advanced logging tool from the Capriccioso arsenal
    /// </summary>
    public interface ICLogger
    {   
        /// <summary>
        /// Initializes the logger, if it hasn't been already
        /// </summary>
        void Init();

        /// <summary>
        /// Logs a normal message to the console with the file name, line number, and method name
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="context">The gameObject where this is from (just put in "this", or "gameObject") to make this work</param>
        /// <param name="path">Which file this is coming from</param>
        /// <param name="lineNumber">Which line number this is coming from</param>
        /// <param name="caller">Which function is calling this</param>
        void Log(string message, Object context = null, [CallerFilePath] string path = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null);

        /// <summary>
        /// Logs a blue INFO message to the console with the file name, line number, and method name
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="context">The gameObject where this is from (just put in "this", or "gameObject") to make this work</param>
        /// <param name="path">Which file this is coming from</param>
        /// <param name="lineNumber">Which line number this is coming from</param>
        /// <param name="caller">Which function is calling this</param>
        void LogInfo(string message, Object context = null, [CallerFilePath] string path = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null);

        /// <summary>
        /// Logs a green GOOD message to the console with the file name, line number, and method name
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="context">The gameObject where this is from (just put in "this", or "gameObject") to make this work</param>
        /// <param name="path">Which file this is coming from</param>
        /// <param name="lineNumber">Which line number this is coming from</param>
        /// <param name="caller">Which function is calling this</param>
        void LogSuccess(string message, Object context = null, [CallerFilePath] string path = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null);

        /// <summary>
        /// Logs a yellow WARN message to the console with the file name, line number, and method name
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="context">The gameObject where this is from (just put in "this", or "gameObject") to make this work</param>
        /// <param name="path">Which file this is coming from</param>
        /// <param name="lineNumber">Which line number this is coming from</param>
        /// <param name="caller">Which function is calling this</param>
        void LogWarning(string message, Object context = null, [CallerFilePath] string path = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null);

        /// <summary>
        /// Logs a red ERROR message to the console with the file name, line number, and method name
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="context">The gameObject where this is from (just put in "this", or "gameObject") to make this work</param>
        /// <param name="path">Which file this is coming from</param>
        /// <param name="lineNumber">Which line number this is coming from</param>
        /// <param name="caller">Which function is calling this</param>
        void LogError(string message, Object context = null, [CallerFilePath] string path = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null);
    }
}