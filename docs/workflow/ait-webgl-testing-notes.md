# AIT WebGL Testing Notes

Last updated: 2026-05-18

This note captures the current Apps in Toss / Unity WebGL findings so a new session can continue without rediscovering the same issues.

## Development Validation Policy

For normal C# iteration, AI agents should prefer `dotnet build BoxStack.slnx`. Do not use Unity CLI Connector to run Unity Editor Play Mode only for validation. Actual gameplay feel, UI feel, difficulty, touch behavior, and playability checks are confirmed by the user directly in Unity Editor Play Mode. AIT/WebGL builds are too slow for every development loop, so reserve them for milestone browser/device spot checks such as WebGL-only asset loading, Korean font rendering, phone safe area, touch behavior, performance, and cache/build-marker verification.

## Current Status

- Resolved: `AIT > Dev Server > Start Server` launches the local Vite server after adding the AIT embedded pnpm folder to Windows `PATH` and fully restarting Unity Hub/Editor.
- Resolved: phone browser can reach the PC AIT Dev Server on the same LAN.
- Resolved in PC browser smoke: build-included `Assets/Resources/Prototype/...` sprites make parcel and conveyor/floor images appear correctly.
- Resolved in PC browser smoke: `Assets/Resources/Prototype/Fonts/NotoSansKR-VF.ttf` renders Korean HUD text in WebGL.
- Current runtime UI: UI Toolkit through `BoxStackPrototypeUi`, with `BoxStackPanelSettings` and `BoxStackRuntimeTheme` loaded from `Assets/Resources/Prototype/Ui/`.
- Current font direction: `DNFBitBitv2.otf` is used for game-like HUD/title/button text, `GmarketSansBold.ttf` for supporting UI text, and `NotoSansKR-VF.ttf` remains as Korean fallback.
- Current asset loading boundary: `BoxStackPrototypeAssetLoader` owns prototype config, font resources, parcel/background sprite loading, Editor-only PNG fallback, runtime sprite creation, placeholder parcel creation, solid sprite creation, and visible-alpha rect calculation.
- Current progress storage boundary: `BoxStackStageProgressStore` owns highest-unlocked-stage persistence and currently uses Unity `PlayerPrefs`.
- Current runtime marker: `B074`.
- B071 phone browser testing found a real-device UI clipping issue: the top-center box indicator, clear result popup, and stage select popup can be cut off outside the visible phone screen.
- B073 changes safe-area handling in `BoxStackPrototypeUi` so `Screen.safeArea` pixel coordinates are converted into UI Toolkit panel coordinates before applying `_safeRoot`, overlay bounds, HUD layout, and touch hit-tests.
- B074 changes the stage-select popup layout so mobile height calculations reserve a computed bottom margin inside the safe area and slightly reduce stage tile height/gaps when vertical space is tight.

## Dev Server Finding

The AIT Unity menu runs this from `ait-build`:

```powershell
pnpm vite --host
```

The Unity-launched process previously failed because `pnpm` was not on `PATH`:

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

For ongoing mobile WebGL iteration, prefer the reusable PowerShell script instead of the Unity menu. The user-level script works from any AIT Unity project folder that contains `ait-build/package.json`:

```powershell
Start-AitUnityDevServer.ps1
```

`C:\Users\Ahneunsung\Documents\PowerShell\Scripts` has been added to the user `PATH`; open a new PowerShell window if the command name is not recognized in an already-open shell.

If the script folder is still not visible in the current shell, run it directly:

```powershell
& "$HOME\Documents\PowerShell\Scripts\Start-AitUnityDevServer.ps1"
```

To target a project explicitly:

```powershell
& "$HOME\Documents\PowerShell\Scripts\Start-AitUnityDevServer.ps1" -ProjectRoot C:\unity\BoxStack
```

This runs the Vite server independently from Unity, so rebuilding WebGL in the Editor should not require manually starting the server again. Keep the PowerShell window open, rebuild WebGL in Unity, and refresh the PC or phone browser after the build finishes.

The project-local wrapper remains available too:

```powershell
.\tools\start-ait-dev-server.ps1
```

To verify the script environment without starting the server:

```powershell
Start-AitUnityDevServer.ps1 -ProjectRoot C:\unity\BoxStack -CheckOnly
```

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

## WebGL Visual Asset Finding

The current prototype loads gameplay sprites through `BoxStackPrototypeAssetLoader` with this priority:

1. `Resources.Load<Sprite>("Prototype/...")`
2. `Resources.Load<Texture2D>("Prototype/...")`, then runtime Sprite creation
3. Editor-only source PNG fallback through `AssetDatabase` or file loading
4. Runtime placeholder/solid sprite fallback

The WebGL-compatible prototype copies live under:

```text
Assets/Resources/Prototype/Parcel/
Assets/Resources/Prototype/Backgrounds/
Assets/Resources/Prototype/Fonts/
Assets/Resources/Prototype/Ui/
```

The original source art still lives under:

```text
Assets/Art/Prototype/Parcel/
Assets/Art/Prototype/Backgrounds/
```

Prototype policy:

- Keep the duplicate parcel/background PNGs in both `Assets/Art/Prototype/...` and `Assets/Resources/Prototype/...` during the prototype phase.
- The `Resources` copies are the WebGL build-included runtime path; deleting them can reintroduce sprite parity failures even though Editor Play has an `AssetDatabase` fallback.
- The `Assets/Art/Prototype/...` copies remain the source-art/reference location.
- Do not replace this duplicate setup during active prototype iteration.
- Productization default: move small static MVP assets to serialized scene/prefab/ScriptableObject references first.
- Introduce Addressables only if remote downloads, catalog updates, skin/stage-pack asset groups, memory/build-size pressure, or App-in-Toss packaging policy requires it.
- The current `Packages/manifest.json` does not include `com.unity.addressables`, so do not add the package until one of those productization triggers exists.
- Decision note: `docs/architecture/boxstack-prototype-asset-loading-decision.md`.

## WebGL Stage Progress Storage Finding

The current prototype uses `BoxStackStageProgressStore` and Unity `PlayerPrefs` for one value: highest unlocked stage.

Prototype policy:

- Keep `PlayerPrefs` during the prototype because the saved data is only one local integer.
- Do not add an App-in-Toss storage wrapper until target package testing or production policy proves that `PlayerPrefs` is insufficient.
- The AIT WebGL template currently exposes `aitSetStorageData`, `aitGetStorageData`, and `aitRemoveStorageData` in `Assets/WebGLTemplates/AITTemplate/Runtime/appsintoss-unity-bridge.js`; those helpers use browser `localStorage` with an `ait_` prefix.
- Treat that JavaScript bridge as a future integration candidate, not as the current C# runtime path.
- If production storage is needed later, keep the stage unlock flow unchanged and replace only the internals of `BoxStackStageProgressStore`, or add a small backend abstraction behind it.
- Decision note: `docs/architecture/boxstack-stage-progress-storage-decision.md`.

Verification:

- 2026-05-07: After rebuilding AIT/WebGL, PC browser testing confirmed that parcel and conveyor/floor images appear correctly.
- 2026-05-10: B014 PC browser smoke confirmed Korean text, parcel/conveyor art, and tap-to-drop behavior.
- 2026-05-13: B022 Editor Play verification confirmed `BoxStackPrototypeAssetLoader` loads config, Korean font, floor sprite, and 3 box visuals.
- 2026-05-13: B023 Editor Play verification confirmed `BoxStackPrototypeBoxVisualCatalog` is the active visual catalog, floor sprite `parcel_stack_base_01` loads, one `UIDocument` exists, and the UI marker shows `B023`.
- 2026-05-13: B024 Editor Play verification confirmed `BoxStackPrototypeUiStateFactory` builds the runtime UI state after the C# split, one `UIDocument` exists, and the UI marker shows `B024`.
- 2026-05-14: B025 Editor Play verification on Unity CLI Connector port `8090` confirmed one `UIDocument`, UI marker `B025`, and no `_statusLabel` field on `BoxStackPrototypeUi`; the right-side progress rail no longer creates the bottom status text label that could overlap the progress UI.
- 2026-05-14: B026 changes the HUD progress display from the right-side rail to a top-center horizontal panel. Placed boxes use filled square slots, remaining boxes use outline-only square slots, and the old numeric count text is removed. Editor Play verification on Unity CLI Connector port `8090` confirmed one `UIDocument`, UI marker `B026`, no `_countLabel`/`_progressFill` fields, Row progress layout, and 4 progress slots on stage 1.
- 2026-05-14: B027 adds game-oriented runtime fonts. `BoxStackPrototypeAssetLoader` loads DNF BitBit v2, Gmarket Sans Bold, and Noto Sans KR fallback from `Assets/Resources/Prototype/Fonts/`; `BoxStackPrototypeUi` applies DNF BitBit v2 to HUD/title/button text and Gmarket Sans Bold to supporting body text. `dotnet build BoxStack.slnx` passed with 0 warnings/errors, Unity generated the new font `.meta` files, and Editor Play verification confirmed all three fonts load, the UI owns the expected font references, one `UIDocument` exists, and the marker shows `B027`.

B071/B073/B074 milestone verification:

1. 2026-05-17 user-led phone WebGL check found that B071 loads, but the top-center box indicator, clear result popup, and stage select popup are clipped outside the visible phone screen.
2. B073 corrects safe-area pixel-to-panel coordinate conversion.
3. B074 adds stage-select bottom-spacing polish after the user found the stage-select popup still feels too close to the bottom edge on phone.
4. 2026-05-18 user-led phone WebGL check accepted B074 stage-select bottom spacing and safe-area clipping for the current prototype baseline.

## Phone Browser Smoke Test Checklist

Use the `Network:` URL printed by the AIT Dev Server, for example:

```text
http://172.30.1.14:5173/index.html
```

The phone must be on the same network as the PC. Do not use `localhost` on the phone.

- Page loads without connection refused, timeout, or infinite loading.
- Portrait layout keeps the HUD, stage select, and result popup inside the visible safe area.
- UI Toolkit labels render Korean text in HUD, stage select, result popup, and build marker states.
- Parcel boxes and the conveyor/floor use the intended image assets, not placeholder rectangles.
- Tap-to-drop input works reliably.
- Stage label opens stage select; locked stages stay disabled.
- `진행 초기화` and `전체 해금` appear only in Editor/development builds.
- Restart, next-stage, and stage select buttons respond to touch.
- Stages 1, 5, 10, 15, and 20 are playable enough to judge difficulty.
- No obvious freezes, browser crashes, or severe frame drops during stack collapse.
- The small build marker below the top-right HUD area shows the expected value for the runtime build being tested.

## Build Marker

`BoxStackPrototype` shows a tiny build marker below the top-right HUD area. The current runtime marker after the latest C# change is:

```text
B074
```

When changing C# code for mobile WebGL testing, manually increment `PrototypeBuildNumber` before rebuilding so the phone can confirm that it loaded the fresh build instead of a cached old build. Documentation-only or design-asset-only commits do not require a build marker increment.

## WebGL Korean Font Fix

WebGL text can lose Korean glyphs if the build relies on Unity's default runtime font. The current prototype includes:

```text
Assets/Resources/Prototype/Fonts/DNFBitBitv2.otf
Assets/Resources/Prototype/Fonts/GmarketSansBold.ttf
Assets/Resources/Prototype/Fonts/NotoSansKR-VF.ttf
Assets/Resources/Prototype/Fonts/THIRD_PARTY_FONTS.txt
```

`BoxStackPrototypeAssetLoader` loads it with:

```text
Resources.Load<Font>("Prototype/Fonts/DNFBitBitv2")
Resources.Load<Font>("Prototype/Fonts/GmarketSansBold")
Resources.Load<Font>("Prototype/Fonts/NotoSansKR-VF")
```

`BoxStackPrototypeUi` applies DNF BitBit v2 to the stronger game/HUD text, Gmarket Sans Bold to supporting body text, and Noto Sans KR as fallback if either primary font is missing.
To make UI Toolkit rendering pick up the intended font reliably, the runtime text helper assigns both `unityFont` and `unityFontDefinition`. B028 Editor Play verification confirmed matching inline/resolved font and font-definition values for DNF BitBit v2 and Gmarket Sans Bold text elements.

B071/B073/B074 milestone verification:

1. B071 had no newly reported Korean text/font issue, but UI clipping prevents accepting the phone WebGL milestone.
2. Reconfirm Korean text and font rendering during the B074 phone WebGL stage-select bottom-spacing check.

## Current Physics Tuning To Retest

The approved B008 physics baseline remains current:

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

The current HUD, stage select, and result popup are runtime UI Toolkit, not `OnGUI`/IMGUI. `BoxStackPrototypeUi` owns layout, safe-area placement, input blocking, game/display font assignment, result popup, stage select, and the current Stack-like HUD structure.

Current UI resources:

- `Assets/Resources/Prototype/Ui/BoxStackPanelSettings.asset`
- `Assets/Resources/Prototype/Ui/BoxStackRuntimeTheme.tss`
- `design/ui/delivery-arcade-assets/`

The `design/ui/delivery-arcade-assets/` PNG mini pack is a design-side prototype skin. It is committed for review, but it is not wired into the runtime UI yet.

## Current Worktree Note

As of B074, the runtime UI uses the accepted Stack-like 2D direction: compact stage badge, top-center progress panel, theme-aware stage select, minimal result popup, no undo UI, and no central landing feedback toast. B073 converts phone `Screen.safeArea` pixels into UI Toolkit panel coordinates before laying out safe-root UI, and B074 adds safe-height-aware stage-select bottom spacing. Game-oriented free fonts are still applied through both `unityFont` and `unityFontDefinition`: DNF BitBit v2 for HUD/title/button text and Gmarket Sans Bold for supporting text. The Delivery Arcade UI PNG mini pack still exists only as design reference under `design/ui/delivery-arcade-assets/`.

## Next Suggested Task

For normal development, use `dotnet build BoxStack.slnx` for AI-side C# validation and leave manual playability checks to the user in Unity Editor Play Mode.

The B074 phone WebGL safe-area checkpoint is accepted. The next WebGL/browser checkpoint should be chosen after the next runtime code or asset-loading change. Documentation-only decisions do not require a new WebGL rebuild.
