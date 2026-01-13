using UnityEngine;

namespace Capriccioso.Runtime.Logging
{
    /// <summary>
    /// ScriptableObject for configuring CLogger settings at runtime.
    /// Create an instance via Assets > Create > Capriccioso > Logger Settings.
    /// </summary>
    [CreateAssetMenu(fileName = "CLoggerSettings", menuName = "Capriccioso/Logger Settings")]
    public class CLoggerSettings : ScriptableObject
    {
        [Header("General")]
        [Tooltip("Enable or disable all logging globally.")]
        [SerializeField] private bool _logEnabled = true;
        
        [Tooltip("Enable file logging in server/batch mode.")]
        [SerializeField] private bool _fileLoggingEnabled = true;

        [Header("Log Level Filters")]
        [Tooltip("Show normal log messages.")]
        [SerializeField] private bool _showLog = true;
        
        [Tooltip("Show info messages.")]
        [SerializeField] private bool _showInfo = true;
        
        [Tooltip("Show success messages.")]
        [SerializeField] private bool _showSuccess = true;
        
        [Tooltip("Show warning messages.")]
        [SerializeField] private bool _showWarning = true;
        
        [Tooltip("Show error messages.")]
        [SerializeField] private bool _showError = true;
        
        [Tooltip("Show backend-related messages.")]
        [SerializeField] private bool _showBackend = true;
        
        [Tooltip("Show event messages.")]
        [SerializeField] private bool _showEvents = true;
        
        [Tooltip("Show major action messages.")]
        [SerializeField] private bool _showMajorActions = true;

        #region Properties

        public bool LogEnabled => _logEnabled;
        public bool FileLoggingEnabled => _fileLoggingEnabled;
        public bool ShowLog => _showLog;
        public bool ShowInfo => _showInfo;
        public bool ShowSuccess => _showSuccess;
        public bool ShowWarning => _showWarning;
        public bool ShowError => _showError;
        public bool ShowBackend => _showBackend;
        public bool ShowEvents => _showEvents;
        public bool ShowMajorActions => _showMajorActions;

        #endregion

        /// <summary>
        /// Applies these settings to the CLogger.
        /// Call this after loading the settings asset.
        /// </summary>
        public void Apply()
        {
            CLogger.LogEnabled = _logEnabled;
            CLogger.FileLoggingEnabled = _fileLoggingEnabled;
            CLogger.LogInfo($"CLogger settings applied: LogEnabled={_logEnabled}");
        }

        private void OnValidate()
        {
            // Auto-apply settings when changed in inspector during play mode
            if (Application.isPlaying)
            {
                Apply();
            }
        }
    }
}
