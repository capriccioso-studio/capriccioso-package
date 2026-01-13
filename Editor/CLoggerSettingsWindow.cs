using UnityEditor;
using UnityEngine;
using Capriccioso.Runtime.Logging;

namespace Capriccioso.Editor
{
    /// <summary>
    /// Editor window for configuring CLogger settings.
    /// Access via Window > Capriccioso > Logger Settings.
    /// </summary>
    public class CLoggerSettingsWindow : EditorWindow
    {
        private CLoggerSettings _settings;
        private SerializedObject _serializedSettings;
        private Vector2 _scrollPosition;
        
        private bool _showRuntimeSettings = true;
        private bool _showLogLevels = true;
        private bool _showColorPreview = true;

        [MenuItem("Window/Capriccioso/Logger Settings")]
        public static void ShowWindow()
        {
            CLoggerSettingsWindow window = GetWindow<CLoggerSettingsWindow>("CLogger Settings");
            window.minSize = new Vector2(350, 400);
            window.Show();
        }

        private void OnEnable()
        {
            LoadOrCreateSettings();
        }

        private void LoadOrCreateSettings()
        {
            // Try to find existing settings
            string[] guids = AssetDatabase.FindAssets("t:CLoggerSettings");
            
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _settings = AssetDatabase.LoadAssetAtPath<CLoggerSettings>(path);
            }
            
            if (_settings != null)
            {
                _serializedSettings = new SerializedObject(_settings);
            }
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            EditorGUILayout.Space(10);
            DrawHeader();
            EditorGUILayout.Space(10);

            if (_settings == null)
            {
                DrawNoSettingsMessage();
            }
            else
            {
                _serializedSettings.Update();
                
                DrawRuntimeSettings();
                EditorGUILayout.Space(10);
                
                DrawLogLevelFilters();
                EditorGUILayout.Space(10);
                
                DrawColorPreview();
                EditorGUILayout.Space(10);
                
                DrawQuickActions();
                
                _serializedSettings.ApplyModifiedProperties();
            }
            
            EditorGUILayout.Space(10);
            DrawRuntimeControls();
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("🎵 CLogger Settings", titleStyle);
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawNoSettingsMessage()
        {
            EditorGUILayout.HelpBox(
                "No CLoggerSettings asset found.\n\n" +
                "Create one to persist logger configuration across sessions.",
                MessageType.Info);
            
            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Create CLogger Settings Asset", GUILayout.Height(30)))
            {
                CreateSettingsAsset();
            }
        }

        private void CreateSettingsAsset()
        {
            // Ensure Resources folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            
            _settings = CreateInstance<CLoggerSettings>();
            AssetDatabase.CreateAsset(_settings, "Assets/Resources/CLoggerSettings.asset");
            AssetDatabase.SaveAssets();
            
            _serializedSettings = new SerializedObject(_settings);
            
            EditorGUIUtility.PingObject(_settings);
            Debug.Log("CLoggerSettings asset created at Assets/Resources/CLoggerSettings.asset");
        }

        private void DrawRuntimeSettings()
        {
            _showRuntimeSettings = EditorGUILayout.BeginFoldoutHeaderGroup(_showRuntimeSettings, "General Settings");
            
            if (_showRuntimeSettings)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(_serializedSettings.FindProperty("_logEnabled"), 
                    new GUIContent("Enable Logging", "Master switch to enable/disable all logging."));
                
                EditorGUILayout.PropertyField(_serializedSettings.FindProperty("_fileLoggingEnabled"), 
                    new GUIContent("File Logging", "Enable logging to file in server/batch mode."));
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawLogLevelFilters()
        {
            _showLogLevels = EditorGUILayout.BeginFoldoutHeaderGroup(_showLogLevels, "Log Level Filters");
            
            if (_showLogLevels)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(_serializedSettings.FindProperty("_showLog"), 
                    new GUIContent("Show Log", "Normal log messages (LOGG)."));
                
                EditorGUILayout.PropertyField(_serializedSettings.FindProperty("_showInfo"), 
                    new GUIContent("Show Info", "Informational messages (INFO)."));
                
                EditorGUILayout.PropertyField(_serializedSettings.FindProperty("_showSuccess"), 
                    new GUIContent("Show Success", "Success messages (GOOD)."));
                
                EditorGUILayout.PropertyField(_serializedSettings.FindProperty("_showWarning"), 
                    new GUIContent("Show Warning", "Warning messages (WARN)."));
                
                EditorGUILayout.PropertyField(_serializedSettings.FindProperty("_showError"), 
                    new GUIContent("Show Error", "Error messages (ERRR)."));
                
                EditorGUILayout.PropertyField(_serializedSettings.FindProperty("_showBackend"), 
                    new GUIContent("Show Backend", "Backend/API related messages."));
                
                EditorGUILayout.PropertyField(_serializedSettings.FindProperty("_showEvents"), 
                    new GUIContent("Show Events", "Event messages."));
                
                EditorGUILayout.PropertyField(_serializedSettings.FindProperty("_showMajorActions"), 
                    new GUIContent("Show Major Actions", "Major action messages."));
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawColorPreview()
        {
            _showColorPreview = EditorGUILayout.BeginFoldoutHeaderGroup(_showColorPreview, "Color Preview");
            
            if (_showColorPreview)
            {
                EditorGUI.indentLevel++;
                
                DrawColorSwatch("LOGG", Constants.LogColors.Log);
                DrawColorSwatch("INFO", Constants.LogColors.Info);
                DrawColorSwatch("GOOD", Constants.LogColors.Success);
                DrawColorSwatch("WARN", Constants.LogColors.Warning);
                DrawColorSwatch("ERRR", Constants.LogColors.Error);
                DrawColorSwatch("EVENT", Constants.LogColors.Event);
                DrawColorSwatch("DB INFO", Constants.LogColors.BackendInfo);
                DrawColorSwatch("DB GOOD", Constants.LogColors.BackendSuccess);
                DrawColorSwatch("DB ERRR", Constants.LogColors.BackendError);
                DrawColorSwatch("WAH", Constants.LogColors.MajorAction);
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawColorSwatch(string label, string hexColor)
        {
            EditorGUILayout.BeginHorizontal();
            
            // Parse hex color
            ColorUtility.TryParseHtmlString(hexColor, out Color color);
            
            EditorGUILayout.LabelField(label, GUILayout.Width(80));
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ColorField(GUIContent.none, color, false, false, false, GUILayout.Width(60));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.LabelField(hexColor, EditorStyles.miniLabel);
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawQuickActions()
        {
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Enable All"))
            {
                SetAllFilters(true);
            }
            
            if (GUILayout.Button("Disable All"))
            {
                SetAllFilters(false);
            }
            
            if (GUILayout.Button("Errors Only"))
            {
                SetAllFilters(false);
                _serializedSettings.FindProperty("_showError").boolValue = true;
                _serializedSettings.FindProperty("_showWarning").boolValue = true;
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void SetAllFilters(bool value)
        {
            _serializedSettings.FindProperty("_showLog").boolValue = value;
            _serializedSettings.FindProperty("_showInfo").boolValue = value;
            _serializedSettings.FindProperty("_showSuccess").boolValue = value;
            _serializedSettings.FindProperty("_showWarning").boolValue = value;
            _serializedSettings.FindProperty("_showError").boolValue = value;
            _serializedSettings.FindProperty("_showBackend").boolValue = value;
            _serializedSettings.FindProperty("_showEvents").boolValue = value;
            _serializedSettings.FindProperty("_showMajorActions").boolValue = value;
        }

        private void DrawRuntimeControls()
        {
            EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            
            bool currentEnabled = CLogger.LogEnabled;
            bool newEnabled = EditorGUILayout.Toggle("Logging Enabled", currentEnabled);
            if (newEnabled != currentEnabled)
            {
                CLogger.LogEnabled = newEnabled;
            }
            
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.EndHorizontal();
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to control logging at runtime.", MessageType.None);
            }
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Test Log"))
            {
                CLogger.Log("Test log message");
            }
            
            if (GUILayout.Button("Test Info"))
            {
                CLogger.LogInfo("Test info message");
            }
            
            if (GUILayout.Button("Test Success"))
            {
                CLogger.LogSuccess("Test success message");
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Test Warning"))
            {
                CLogger.LogWarning("Test warning message");
            }
            
            if (GUILayout.Button("Test Error"))
            {
                CLogger.LogError("Test error message");
            }
            
            if (GUILayout.Button("Test Event"))
            {
                CLogger.LogEvent("Test event message");
            }
            
            EditorGUILayout.EndHorizontal();
        }
    }
}
