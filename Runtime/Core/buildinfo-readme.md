# BuildInfo

Provides build version and timestamp information at runtime. Useful for displaying version info in-game, debugging, and tracking deployed builds.

## When to Use

- **Version Display** — Show version in main menu, settings, or splash screen
- **Bug Reports** — Include build info in crash reports
- **Feature Flags** — Enable features based on build type
- **Analytics** — Track which versions players are using
- **Support** — Identify exact build when helping players

---

## Quick Start

### 1. Create Settings Asset

1. Right-click in Project window
2. **Create > Capriccioso > Build Info Settings**
3. Move to a **Resources** folder (e.g., `Assets/Resources/BuildInfoSettings.asset`)

### 2. Configure in Inspector

| Field | Description | Example |
|-------|-------------|---------|
| Version | Semantic version | `1.2.3` |
| Build Number | CI/CD build counter | `456` |
| Build Date | When built | `2025-01-13 14:30:00` |
| Git Branch | Source branch | `main` |
| Git Commit | Short commit hash | `a1b2c3d` |

### 3. Access at Runtime

```csharp
using Capriccioso;
using UnityEngine;
using TMPro;

public class VersionDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text _versionText;
    
    private void Start()
    {
        // Simple version display
        _versionText.text = BuildInfo.FullVersion;
        // Output: "1.2.3 (Build 456)"
        
        // Or just the version
        _versionText.text = $"v{BuildInfo.Version}";
        // Output: "v1.2.3"
    }
}
```

---

## Available Properties

| Property | Type | Description |
|----------|------|-------------|
| `Version` | string | Semantic version (e.g., "1.2.3") |
| `BuildNumber` | int | CI/CD build number |
| `FullVersion` | string | Version with build number |
| `BuildDate` | string | When build was created |
| `GitBranch` | string | Git branch name |
| `GitCommit` | string | Git commit hash (short) |
| `IsDebugBuild` | bool | True if development build |
| `UnityVersion` | string | Unity version used |
| `Platform` | RuntimePlatform | Target platform |
| `CompanyName` | string | From Player Settings |
| `ProductName` | string | From Player Settings |

---

## Usage Examples

### Display in UI

```csharp
// Main menu version
versionLabel.text = BuildInfo.FullVersion;

// Detailed info screen
infoText.text = BuildInfo.GetFormattedBuildInfo();
/* Output:
My Game v1.2.3 (Build 456)
Built: 2025-01-13 14:30:00
Unity: 6000.0.0f1
Platform: WindowsPlayer
Branch: main
Commit: a1b2c3d
*/
```

### Debug Features

```csharp
private void Start()
{
    if (BuildInfo.IsDebugBuild)
    {
        EnableDebugConsole();
        EnableCheats();
        ShowFpsCounter();
    }
}
```

### Bug Reports

```csharp
public string GenerateBugReport()
{
    return $@"
=== Bug Report ===
{BuildInfo.GetFormattedBuildInfo()}

Device: {SystemInfo.deviceModel}
OS: {SystemInfo.operatingSystem}
GPU: {SystemInfo.graphicsDeviceName}
RAM: {SystemInfo.systemMemorySize} MB

Description: [User description here]
";
}
```

### Log on Startup

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
private static void LogVersion()
{
    BuildInfo.LogBuildInfo();
}

/* Console output:
[MajorAction] BUILD INFO
[Info] Product: My Game by My Company
[Info] Version: 1.2.3 (Build 456)
[Info] Build Date: 2025-01-13 14:30:00
[Info] Unity: 6000.0.0f1
[Info] Platform: WindowsPlayer
[Info] Git: main @ a1b2c3d
[Info] Debug Build: False
*/
```

---

## CI/CD Integration

### Pre-Build Script

Create an Editor script to auto-populate build info:

```csharp
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System;
using System.Diagnostics;

public class BuildInfoUpdater : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;
    
    public void OnPreprocessBuild(BuildReport report)
    {
        var settings = Resources.Load<BuildInfoSettings>("BuildInfoSettings");
        if (settings == null)
        {
            UnityEngine.Debug.LogWarning("BuildInfoSettings not found in Resources!");
            return;
        }
        
        // Get git info
        string branch = RunGit("rev-parse --abbrev-ref HEAD");
        string commit = RunGit("rev-parse --short HEAD");
        
        // Get build number from environment (CI/CD)
        string buildNumberStr = Environment.GetEnvironmentVariable("BUILD_NUMBER") ?? "0";
        int.TryParse(buildNumberStr, out int buildNumber);
        
        // Update settings
        settings.SetBuildInfo(
            version: PlayerSettings.bundleVersion,
            buildNumber: buildNumber,
            buildDate: DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            branch: branch,
            commit: commit
        );
        
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
    }
    
    private static string RunGit(string args)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            return process.StandardOutput.ReadToEnd().Trim();
        }
        catch
        {
            return "Unknown";
        }
    }
}
```

### GitHub Actions Example

```yaml
- name: Build Unity Project
  env:
    BUILD_NUMBER: ${{ github.run_number }}
  run: |
    unity-builder --projectPath . --buildTarget StandaloneWindows64
```

---

## Programmatic Updates

Update settings from code (useful for CI/CD):

```csharp
// In an Editor script
var settings = Resources.Load<BuildInfoSettings>("BuildInfoSettings");
settings.SetBuildInfo(
    version: "1.2.3",
    buildNumber: 456,
    buildDate: "2025-01-13 14:30:00",
    branch: "main",
    commit: "a1b2c3d"
);

// Or just update the build date
settings.SetBuildDateNow();
```

---

## Best Practices

### ✅ Do

- Place `BuildInfoSettings.asset` in a **Resources** folder
- Automate build info population in CI/CD
- Include build info in crash reports and analytics
- Log build info on startup for debugging

### ❌ Don't

- Manually update build info for every build
- Forget to include `BuildInfoSettings` in builds
- Rely solely on `Application.version` (limited info)
