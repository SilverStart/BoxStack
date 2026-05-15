# Unity CLI Connector

Last updated: 2026-05-15

## Purpose

Use Unity CLI Connector when Codex needs to inspect or control the currently open Unity Editor without relying on batchmode.

This is especially useful for this project because Unity may already be open, which blocks batchmode import/play checks. The connector lets Codex send commands to the live editor instead.

## Project Usage Policy

- Do not use Unity CLI Connector only to run Play Mode verification.
- Normal C# validation should prefer `dotnet build BoxStack.slnx`.
- Gameplay feel, UI feel, difficulty, touch input, and manual playability checks
  are confirmed by the user directly in Unity Editor Play Mode.
- Use this connector only when an AI agent must directly control or inspect the
  currently open Unity Editor to continue the task.
- Keep WebGL/mobile checks for milestone spot checks, not routine iteration.

## Package

- Package: `com.youngwoocho02.unity-cli-connector`
- Version observed: `0.3.15`
- Source in `Packages/manifest.json`: `https://github.com/youngwoocho02/unity-cli.git?path=unity-connector`
- Package cache observed at: `Library/PackageCache/com.youngwoocho02.unity-cli-connector@3b0c71caf484`

## Runtime Behavior

- The connector starts an HTTP server from inside the Unity Editor.
- Default port: `127.0.0.1:8090`
- If the port is busy, it tries `8091` through `8099`.
- Endpoint: `POST /command`
- Body shape:

```json
{
  "command": "list",
  "params": {}
}
```

Unity must be open with this project loaded. If the editor is closed or the package has not finished compiling, the endpoint will not respond.

## Confirmed Tools

The `list` command returned these tools in this project:

- `screenshot`: capture Scene or Game view screenshots.
- `exec`: execute arbitrary C# code inside the Unity Editor.
- `menu`: execute a Unity menu item by path.
- `manage_editor`: play, stop, pause, refresh, active tool, tags, and layers.
- `profiler`: read or control profiler data.
- `console`: read or clear Unity console logs.
- `refresh_unity`: refresh assets and optionally request compilation.
- `reserialize`: force reserialize assets.
- `run_tests`: run EditMode or PlayMode tests.

## PowerShell Examples

### Find The Active Port

```powershell
$ports = 8090..8099
foreach ($port in $ports) {
  try {
    $body = @{ command = 'list'; params = @{} } | ConvertTo-Json -Compress
    $response = Invoke-RestMethod -Uri "http://127.0.0.1:$port/command" -Method Post -Body $body -ContentType 'application/json' -TimeoutSec 2
    "PORT=$port"
    $response | ConvertTo-Json -Depth 8
    break
  } catch {
  }
}
```

### Execute Read-Only C# In The Editor

```powershell
$code = @'
return new Dictionary<string, object> {
    { "unityVersion", Application.unityVersion },
    { "projectPath", Application.dataPath },
    { "isPlaying", EditorApplication.isPlaying },
    { "activeScene", UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path },
    { "mainCamera", Camera.main != null ? Camera.main.name : "<none>" },
    { "objectCount", UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Length }
};
'@

$body = @{ command = 'exec'; params = @{ code = $code } } | ConvertTo-Json -Depth 5 -Compress
Invoke-RestMethod -Uri 'http://127.0.0.1:8090/command' -Method Post -Body $body -ContentType 'application/json'
```

### Refresh Assets

```powershell
$body = @{ command = 'refresh_unity'; params = @{ mode = 'force'; compile = 'request' } } | ConvertTo-Json -Depth 5 -Compress
Invoke-RestMethod -Uri 'http://127.0.0.1:8090/command' -Method Post -Body $body -ContentType 'application/json'
```

### Start Or Stop Play Mode

```powershell
$body = @{ command = 'manage_editor'; params = @{ action = 'play'; wait_for_completion = $true } } | ConvertTo-Json -Depth 5 -Compress
Invoke-RestMethod -Uri 'http://127.0.0.1:8090/command' -Method Post -Body $body -ContentType 'application/json'

$body = @{ command = 'manage_editor'; params = @{ action = 'stop'; wait_for_completion = $true } } | ConvertTo-Json -Depth 5 -Compress
Invoke-RestMethod -Uri 'http://127.0.0.1:8090/command' -Method Post -Body $body -ContentType 'application/json'
```

### Read Console Logs

```powershell
$body = @{ command = 'console'; params = @{ type = 'error,warning,log'; lines = 20; stacktrace = 'none' } } | ConvertTo-Json -Depth 5 -Compress
Invoke-RestMethod -Uri 'http://127.0.0.1:8090/command' -Method Post -Body $body -ContentType 'application/json'
```

## Observed Verification

On 2026-05-02 the connector responded on port `8090`.

Observed `exec` result:

- Unity version: `6000.0.73f1`
- Project path: `C:/unity/BoxStack/Assets`
- Active scene: `Assets/Scenes/SampleScene.unity`
- Play mode: `false`
- Main Camera: `Main Camera`

Observed console entries:

- `[UnityCliConnector] HTTP server started on port 8090`
- `Unity CLI Connector is already up-to-date.`

## Usage Notes

- Use this connector for live-editor control, screenshots, asset refreshes, and
  small C# inspection scripts only when the task genuinely requires editor
  control.
- Skip Play Mode checks through this connector when they are only validation;
  the user will perform real gameplay checks manually.
- Keep injected C# small and purpose-specific.
- For project file edits, still modify files in the workspace first, then use the connector to refresh or verify in Unity.
- If a command fails, first check whether Unity is open and whether the package has compiled.
- If port `8090` fails, scan `8091` through `8099`.
