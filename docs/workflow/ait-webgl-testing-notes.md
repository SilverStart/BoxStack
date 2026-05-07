# AIT WebGL Testing Notes

Date: 2026-05-07

This note captures the current Apps in Toss / Unity WebGL test findings so a new session can continue without rediscovering them.

## Current Symptom

- Resolved: `AIT > Dev Server > Start Server` now launches the local Vite server after adding the AIT embedded pnpm folder to Windows `PATH` and fully restarting Unity Hub/Editor.
- Resolved: In the PC browser, parcel and conveyor/floor images now match the intended prototype assets after copying build-included Resources sprites and rebuilding AIT/WebGL.
- Resolved: phone browser can reach the PC AIT Dev Server on the same LAN.
- Remaining: phone browser testing is still needed for real portrait layout, touch input, safe area, WebGL performance, and the latest physics tuning.

## Dev Server Finding

The AIT Unity menu runs this from `ait-build`:

```powershell
pnpm vite --host
```

The Unity-launched process failed because `pnpm` was not on PATH:

```text
Command failed with exit code 1: pnpm vite --host
pnpm : The term 'pnpm' is not recognized
```

Applied permanent fix:

1. Add this folder to the Windows user or system `PATH`:

```text
C:\Users\Ahneunsung\AppData\Local\.ait-unity-sdk\nodejs\v24.13.0\win-x64
```

2. Fully quit Unity Editor and Unity Hub.
3. Reopen Unity Hub, reopen this project, and run `AIT > Dev Server > Start Server` again.

Unity needs the restart because the Editor inherits environment variables from the process that launched it. If Unity Hub stays open, it may keep the old `PATH`.

The AIT SDK embedded pnpm direct command remains useful as a fallback/debug check:

```powershell
cd C:\unity\BoxStack\ait-build
& "$env:LOCALAPPDATA\.ait-unity-sdk\nodejs\v24.13.0\win-x64\pnpm.cmd" vite --host 0.0.0.0 --clearScreen false
```

Expected server output:

```text
Local:   http://localhost:5173/
Network: http://172.30.1.14:5173/
```

Use these URLs:

- PC: `http://localhost:5173/index.html`
- Phone on the same network: `http://172.30.1.14:5173/index.html`

Phone testing must use the PC LAN IP, not `localhost`. On a phone, `localhost` means the phone itself.

Verified LAN path:

- PC Dev Server IP: `172.30.1.14`
- Phone IP: `172.30.1.40`
- Phone access to `http://172.30.1.14:5173/` works.

If the page works briefly and then connection is refused, re-check whether port `5173` is still listening:

```powershell
Get-NetTCPConnection -LocalPort 5173 -ErrorAction SilentlyContinue
```

## WebGL Visual Mismatch Finding

The current prototype loads sprites with this priority:

1. `Resources.Load<Sprite>("Prototype/Parcel/...")`
2. Editor-only `AssetDatabase.LoadAssetAtPath(...)`
3. Runtime placeholder/solid sprite fallback

The actual PNG files currently live under:

```text
Assets/Art/Prototype/Parcel/
Assets/Art/Prototype/Backgrounds/
```

At the time of diagnosis there was no `Assets/Resources/Prototype/Parcel/` folder. That meant:

- Editor Play can find PNGs through `AssetDatabase`.
- WebGL builds cannot use `AssetDatabase`.
- WebGL falls back to generated placeholder/solid sprites, so parcel boxes and the conveyor/floor can look wrong.

Applied prototype fix:

- Prototype PNGs were copied into `Assets/Resources/Prototype/Parcel/` and `Assets/Resources/Prototype/Backgrounds/`.
- `BoxStackPrototype.LoadPrototypeSprite()` now tries `Resources.Load<Sprite>()` first, then `Resources.Load<Texture2D>()`, before using the Editor-only `AssetDatabase` fallback.

Verification:

- 2026-05-07: After rebuilding AIT/WebGL, PC browser testing confirmed that parcel and conveyor/floor images appear correctly.

Remaining verification:

1. Retest on a phone browser using the PC LAN IP.

## Phone Browser Smoke Test Checklist

Use the `Network:` URL printed by the AIT Dev Server, for example:

```text
http://172.30.1.14:5173/index.html
```

The phone must be on the same network as the PC. Do not use `localhost` on the phone.

- Page loads without connection refused, timeout, or infinite loading.
- Portrait layout keeps the top HUD, stage select, and result popup inside the visible safe area.
- Parcel boxes and the conveyor/floor use the intended image assets, not placeholder rectangles.
- Tap-to-drop input works reliably.
- Stage label opens stage select; locked stages stay disabled.
- Restart, free rescue, and mock reward-ad rescue buttons respond to touch.
- Stages 1, 5, 10, 15, and 20 are playable enough to judge difficulty.
- No obvious freezes, browser crashes, or severe frame drops during stack collapse.
- The small build marker below the top-right HUD area shows the expected value (`B001` for the current prototype build marker).

## Build Marker

`BoxStackPrototype` shows a tiny build marker below the top-right HUD area. The current value is:

```text
B001
```

When changing C# code for mobile WebGL testing, manually increment `PrototypeBuildNumber` before rebuilding so the phone can confirm that it loaded the fresh build instead of a cached old build.

## Current Physics Tuning To Retest

The latest phone test reported that lower boxes still slide sideways too easily. The next WebGL build should verify:

- `PhysicsMaterial2D.friction = 2.4`
- `PhysicsMaterial2D.bounciness = 0`
- `Rigidbody2D.linearDamping = 0.6`
- `Rigidbody2D.angularDamping = 0.8`

Expected result: lower-box sliding should be reduced, while badly stacked towers can still collapse.

## UI Finding

The current HUD, stage select, and result popup are still Unity `OnGUI` prototype UI. `OnGUI` is useful for fast iteration, but WebGL/mobile can show differences in:

- font rendering
- DPI scaling
- touch hit areas
- safe area behavior
- layout clipping

Recent uncommitted code added `Screen.safeArea` conversion for IMGUI placement, but this still needs browser/device playtest. For production-like UI, plan to migrate from `OnGUI` to a runtime UI layer such as UGUI or UI Toolkit after the remaining gameplay/progression prototype checks.

## Current Worktree Note

As of this note, there are uncommitted changes from safe-area layout work and Unity/AIT/WebGL generated metadata/settings. Do not revert them blindly; inspect before committing or splitting commits.

Likely intentional current code/doc changes:

- `Assets/Scripts/Prototype/BoxStackPrototype.cs`
- `production/session-state/active.md`
- `production/progress-dashboard.md`

Likely Unity/AIT generated or import-setting changes observed:

- parcel/background `.png.meta` files
- URP/global/project settings
- `Assets/WebGLTemplates.meta`

## Next Suggested Task

Run the phone browser smoke test using the PC LAN IP from the AIT Dev Server output, then record the result in `production/session-state/active.md` and `production/progress-dashboard.md`.
