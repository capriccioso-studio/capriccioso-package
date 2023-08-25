using UnityEngine;
using System.Runtime.CompilerServices;
using System.IO;

namespace Capriccioso
{
    /// <summary>
    /// The colored logging tool from Capriccioso
    /// </summary>
    public class CLogger : MonoSingleton<CLogger>, ICLogger
    {
        [SerializeField] private bool _logEnabled = true;
        private bool _isInitialized = false;

        public void Init()
        {
            if (!_isInitialized)
            {
                Debug.developerConsoleVisible = true;
                _isInitialized = true;
            }
        }

        public void Log(string message, Object context = null, [CallerFilePath] string path = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (!_logEnabled) return;
            CLogger.Instance.Init();
            string file = Path.GetFileName(path);
            string log = $"<color={LogColors.Log}><b>[{file} @ {caller}():{lineNumber}]</b> LOGG </color> - {message}";
            Debug.Log(log, context);
        }

        public void LogInfo(string message, Object context = null, [CallerFilePath] string path = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (!_logEnabled) return;
            CLogger.Instance.Init();
            string file = Path.GetFileName(path);
            string log = $"<color={LogColors.Info}><b>[{file} @ {caller}():{lineNumber}]</b> INFO </color> - {message}";
            Debug.Log(log, context);
        }
        
        public void LogSuccess(string message, Object context = null, [CallerFilePath] string path = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (!_logEnabled) return;
            CLogger.Instance.Init();
            string file = Path.GetFileName(path);
            string log = $"<color={LogColors.Success}><b>[{file} @ {caller}():{lineNumber}]</b> GOOD </color> - {message}";
            Debug.Log(log, context);
        }

        public void LogWarning(string message, Object context = null, [CallerFilePath] string path = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (!_logEnabled) return;
            CLogger.Instance.Init();
            string file = Path.GetFileName(path);
            string log = $"<color={LogColors.Warning}><b>[{file} @ {caller}():{lineNumber}]</b> WARN </color> - {message}";
            Debug.Log(log, context);
        }

        public void LogError(string message, Object context = null, [CallerFilePath] string path = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            if (!_logEnabled) return;
            CLogger.Instance.Init();
            string file = Path.GetFileName(path);
            string log = $"<color={LogColors.Error}><b>[{file} @ {caller}():{lineNumber}]</b> ERRR </color> - {message}";
            Debug.Log(log, context);
        }


    }

}