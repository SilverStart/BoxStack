# AIT WebGL Testing Notes

Date: 2026-05-07

This note captures the current Apps in Toss / Unity WebGL test findings so a new session can continue without rediscovering them.

## Current Symptom

- Resolved: `AIT > Dev Server > Start Server` now launches the local Vite server after adding the AIT embedded pnpm folder to Windows `PATH` and fully restarting Unity Hub/Editor.
- Resolved: In the PC browser, parcel and conveyor/floor images now match the intended prototype assets after copying build-included Resources sprites and rebuilding AIT/WebGL.
- Resolved: phone browser can reach the PC AIT Dev Server on the same LAN.
- Pending verification: the prototype now includes a Korean-capable `NotoSansKR-VF` font under `Assets/Resources/Prototype/Fonts/` and applies it to the IMGUI styles, so WebGL Korean text should render instead of disappearing.
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
- Restart and free rescue buttons respond to touch.
- Korean HUD, stage select, result popup, and rescue button text is visible in WebGL.
- Stages 1, 5, 10, 15, and 20 are playable enough to judge difficulty.
- No obvious freezes, browser crashes, or severe frame drops during stack collapse.
- The small build marker below the top-right HUD area shows the expected value (`B008` for the current prototype build marker).

## Build Marker

`BoxStackPrototype` shows a tiny build marker below the top-right HUD area. The current value is:

```text
B008
```

When changing C# code for mobile WebGL testing, manually increment `PrototypeBuildNumber` before rebuilding so the phone can confirm that it loaded the fresh build instead of a cached old build.

## WebGL Korean Font Fix

`OnGUI` text can lose Korean glyphs in WebGL if the build relies on Unity's default runtime font. The current prototype includes `Assets/Resources/Prototype/Fonts/NotoSansKR-VF.ttf` and loads it with `Resources.Load<Font>("Prototype/Fonts/NotoSansKR-VF")`, then applies it to HUD, stage select, result popup, rescue buttons, and the build marker styles.

Remaining verification:

1. Rebuild WebGL.
2. Open the browser build and confirm Korean text appears in all prototype UI states.
3. Confirm the build marker shows `B008`, not `B007`.

## Current Physics Tuning To Retest

The latest phone tests found that high friction helps, but falling-box impact still pushes the lower stack too hard. The next WebGL build keeps high friction, softens the active drop, and should verify:

- parcel `PhysicsMaterial2D.friction = 8.0`
- floor `PhysicsMaterial2D.friction = 8.0`
- no settled-box X/rotation constraints are applied
- `PhysicsMaterial2D.bounciness = 0`
- dropping boxes temporarily use `gravityScale = 1.1`
- dropping fall speed is capped at `4.5`
- settled boxes use `gravityScale = 1.6`
- `Rigidbody2D.linearDamping = 0.6`
- `Rigidbody2D.angularDamping = 0.8`

Expected result: impact-driven sideways sliding should be reduced, while towers can still rotate/tip/collapse when badly stacked.

## UI Finding

The current HUD, stage select, and result popup are still Unity `OnGUI` prototype UI. `OnGUI` is useful for fast iteration, but WebGL/mobile can show differences in:

- font rendering
- DPI scaling
- touch hit areas
- safe area behavior
- layout clipping

Committed prototype code now uses `Screen.safeArea` conversion for IMGUI placement, but this still needs browser/device playtest. For production-like UI, plan to migrate from `OnGUI` to a runtime UI layer such as UGUI or UI Toolkit after the remaining gameplay/progression prototype checks.

## Current Worktree Note

As of the commit checkpoint on 2026-05-08, the related changes were split into focused commits and the worktree was clean immediately afterward.

Recent checkpoint commits:

- `8aa7158 웹GL 프로토타입 리소스 로딩 수정`
- `daeb9f4 모바일 안전 영역에 맞춰 프로토타입 UI 보정`
- `0884502 박스 미끄러짐 완화 물리값 조정`
- `b0e12b0 프로토타입 빌드 번호 표시 추가`
- `16e0c1c AIT 웹GL 테스트 절차 기록`
- `94a6549 AIT 웹GL 빌드 설정 정리`

## Next Suggested Task

Rebuild WebGL, open the phone browser through the PC LAN IP, confirm `B008` is visible, then retest whether softened drops reduce unfair impact sliding without making the whole stack feel floaty.
