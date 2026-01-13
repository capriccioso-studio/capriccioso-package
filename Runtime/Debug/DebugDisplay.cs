using System.Text;
using UnityEngine;

namespace Capriccioso.Runtime.Debug
{
    /// <summary>
    /// Runtime debug overlay displaying FPS, memory usage, and other metrics.
    /// Add to your scene or let DebugDisplayService auto-create it.
    /// </summary>
    /// <example>
    /// <code>
    /// // Option 1: Add DebugDisplay component to a GameObject in your scene
    /// //           Configure what to show via inspector
    /// 
    /// // Option 2: Use the service (auto-creates if needed)
    /// DebugDisplayService.Instance.Show();
    /// DebugDisplayService.Instance.Hide();
    /// DebugDisplayService.Instance.Toggle();
    /// 
    /// // Configure at runtime
    /// DebugDisplayService.Instance.ShowFPS = true;
    /// DebugDisplayService.Instance.ShowMemory = true;
    /// DebugDisplayService.Instance.ShowBuildInfo = false;
    /// 
    /// // Toggle with keyboard (configured in inspector or via code)
    /// // Default: F3 to toggle debug display
    /// </code>
    /// </example>
    public class DebugDisplay : MonoBehaviour
    {
        #region Settings

        [Header("Display Options")]
        [Tooltip("Show FPS counter.")]
        [SerializeField] private bool _showFps = true;
        
        [Tooltip("Show memory usage.")]
        [SerializeField] private bool _showMemory = true;
        
        [Tooltip("Show build/version info.")]
        [SerializeField] private bool _showBuildInfo = false;
        
        [Tooltip("Show system info.")]
        [SerializeField] private bool _showSystemInfo = false;

        [Header("Appearance")]
        [Tooltip("Position of the debug overlay.")]
        [SerializeField] private TextAnchor _anchor = TextAnchor.UpperLeft;
        
        [Tooltip("Padding from screen edge.")]
        [SerializeField] private Vector2 _padding = new Vector2(10f, 10f);
        
        [Tooltip("Font size for debug text.")]
        [SerializeField] private int _fontSize = 14;
        
        [Tooltip("Background color for readability.")]
        [SerializeField] private Color _backgroundColor = new Color(0f, 0f, 0f, 0.7f);
        
        [Tooltip("Text color.")]
        [SerializeField] private Color _textColor = Color.white;

        [Header("Thresholds")]
        [Tooltip("FPS below this shows in yellow.")]
        [SerializeField] private int _fpsWarningThreshold = 30;
        
        [Tooltip("FPS below this shows in red.")]
        [SerializeField] private int _fpsCriticalThreshold = 15;

        [Header("Controls")]
        [Tooltip("Key to toggle debug display.")]
        [SerializeField] private KeyCode _toggleKey = KeyCode.F3;
        
        [Tooltip("Start visible.")]
        [SerializeField] private bool _startVisible = true;

        #endregion

        #region Private Fields

        private bool _isVisible;
        private float _deltaTime;
        private float _fpsUpdateTimer;
        private float _memoryUpdateTimer;
        private int _currentFps;
        private float _memoryUsageMb;
        private GUIStyle _style;
        private GUIStyle _backgroundStyle;
        private Texture2D _backgroundTexture;
        private readonly StringBuilder _displayBuilder = new StringBuilder(256);

        #endregion

        #region Properties

        public bool ShowFPS { get => _showFps; set => _showFps = value; }
        public bool ShowMemory { get => _showMemory; set => _showMemory = value; }
        public bool ShowBuildInfo { get => _showBuildInfo; set => _showBuildInfo = value; }
        public bool ShowSystemInfo { get => _showSystemInfo; set => _showSystemInfo = value; }
        public bool IsVisible => _isVisible;

        #endregion

        #region Unity Events

        private void Awake()
        {
            _isVisible = _startVisible;
            CreateStyles();
        }

        private void Update()
        {
            // Toggle visibility
            if (Input.GetKeyDown(_toggleKey))
            {
                Toggle();
            }

            if (!_isVisible) return;

            // Update FPS
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
            _fpsUpdateTimer += Time.unscaledDeltaTime;
            
            if (_fpsUpdateTimer >= Constants.FpsUpdateInterval)
            {
                _currentFps = Mathf.RoundToInt(1f / _deltaTime);
                _fpsUpdateTimer = 0f;
            }

            // Update memory
            _memoryUpdateTimer += Time.unscaledDeltaTime;
            if (_memoryUpdateTimer >= Constants.MemoryUpdateInterval)
            {
                _memoryUsageMb = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
                _memoryUpdateTimer = 0f;
            }
        }

        private void OnGUI()
        {
            if (!_isVisible) return;

            BuildDisplayText();

            Rect rect = CalculateRect();
            
            // Draw background
            GUI.Box(rect, GUIContent.none, _backgroundStyle);
            
            // Draw text
            GUI.Label(rect, _displayBuilder.ToString(), _style);
        }

        private void OnDestroy()
        {
            if (_backgroundTexture != null)
            {
                Destroy(_backgroundTexture);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>Shows the debug overlay.</summary>
        public void Show() => _isVisible = true;

        /// <summary>Hides the debug overlay.</summary>
        public void Hide() => _isVisible = false;

        /// <summary>Toggles the debug overlay visibility.</summary>
        public void Toggle() => _isVisible = !_isVisible;

        #endregion

        #region Private Methods

        private void CreateStyles()
        {
            _backgroundTexture = new Texture2D(1, 1);
            _backgroundTexture.SetPixel(0, 0, _backgroundColor);
            _backgroundTexture.Apply();

            _style = new GUIStyle
            {
                fontSize = _fontSize,
                padding = new RectOffset(8, 8, 4, 4),
                normal = { textColor = _textColor },
                alignment = _anchor
            };

            _backgroundStyle = new GUIStyle
            {
                normal = { background = _backgroundTexture }
            };
        }

        private void BuildDisplayText()
        {
            _displayBuilder.Clear();

            if (_showFps)
            {
                string fpsColor = GetFpsColor();
                _displayBuilder.AppendLine($"<color={fpsColor}>FPS: {_currentFps}</color>");
            }

            if (_showMemory)
            {
                _displayBuilder.AppendLine($"Memory: {_memoryUsageMb:F1} MB");
            }

            if (_showBuildInfo)
            {
                _displayBuilder.AppendLine($"Version: {BuildInfo.FullVersion}");
                if (!string.IsNullOrEmpty(BuildInfo.GitBranch) && BuildInfo.GitBranch != "Unknown")
                {
                    _displayBuilder.AppendLine($"Branch: {BuildInfo.GitBranch}");
                }
            }

            if (_showSystemInfo)
            {
                _displayBuilder.AppendLine($"Platform: {Application.platform}");
                _displayBuilder.AppendLine($"Unity: {Application.unityVersion}");
            }

            // Remove trailing newline
            if (_displayBuilder.Length > 0 && _displayBuilder[_displayBuilder.Length - 1] == '\n')
            {
                _displayBuilder.Length--;
            }
        }

        private string GetFpsColor()
        {
            if (_currentFps < _fpsCriticalThreshold)
                return "#FF4444"; // Red
            if (_currentFps < _fpsWarningThreshold)
                return "#FFFF44"; // Yellow
            return "#44FF44"; // Green
        }

        private Rect CalculateRect()
        {
            float width = 200f;
            float lineCount = 0;
            if (_showFps) lineCount++;
            if (_showMemory) lineCount++;
            if (_showBuildInfo) lineCount += 2;
            if (_showSystemInfo) lineCount += 2;
            float height = lineCount * (_fontSize + 4) + 16;

            float x = _padding.x;
            float y = _padding.y;

            switch (_anchor)
            {
                case TextAnchor.UpperRight:
                case TextAnchor.MiddleRight:
                case TextAnchor.LowerRight:
                    x = Screen.width - width - _padding.x;
                    break;
                case TextAnchor.UpperCenter:
                case TextAnchor.MiddleCenter:
                case TextAnchor.LowerCenter:
                    x = (Screen.width - width) / 2f;
                    break;
            }

            switch (_anchor)
            {
                case TextAnchor.LowerLeft:
                case TextAnchor.LowerCenter:
                case TextAnchor.LowerRight:
                    y = Screen.height - height - _padding.y;
                    break;
                case TextAnchor.MiddleLeft:
                case TextAnchor.MiddleCenter:
                case TextAnchor.MiddleRight:
                    y = (Screen.height - height) / 2f;
                    break;
            }

            return new Rect(x, y, width, height);
        }

        #endregion
    }

    /// <summary>
    /// Singleton service to access DebugDisplay from anywhere.
    /// Auto-creates a DebugDisplay if one doesn't exist.
    /// </summary>
    public class DebugDisplayService : MonoSingleton<DebugDisplayService>
    {
        private DebugDisplay _display;

        protected override bool Awake()
        {
            if (!base.Awake()) return false;

            // Find or create DebugDisplay
            _display = FindAnyObjectByType<DebugDisplay>();
            if (_display == null)
            {
                _display = gameObject.AddComponent<DebugDisplay>();
            }

            return true;
        }

        /// <summary>Shows the debug overlay.</summary>
        public void Show() => _display?.Show();

        /// <summary>Hides the debug overlay.</summary>
        public void Hide() => _display?.Hide();

        /// <summary>Toggles the debug overlay visibility.</summary>
        public void Toggle() => _display?.Toggle();

        /// <summary>Whether the overlay is currently visible.</summary>
        public bool IsVisible => _display != null && _display.IsVisible;

        /// <summary>Show/hide FPS counter.</summary>
        public bool ShowFPS
        {
            get => _display != null && _display.ShowFPS;
            set { if (_display != null) _display.ShowFPS = value; }
        }

        /// <summary>Show/hide memory display.</summary>
        public bool ShowMemory
        {
            get => _display != null && _display.ShowMemory;
            set { if (_display != null) _display.ShowMemory = value; }
        }

        /// <summary>Show/hide build info.</summary>
        public bool ShowBuildInfo
        {
            get => _display != null && _display.ShowBuildInfo;
            set { if (_display != null) _display.ShowBuildInfo = value; }
        }

        /// <summary>Show/hide system info.</summary>
        public bool ShowSystemInfo
        {
            get => _display != null && _display.ShowSystemInfo;
            set { if (_display != null) _display.ShowSystemInfo = value; }
        }
    }
}
