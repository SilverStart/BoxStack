# AIT WebGL Testing Notes

Last updated: 2026-05-13

This note captures the current Apps in Toss / Unity WebGL findings so a new session can continue without rediscovering the same issues.

## Development Validation Policy

Use Unity Editor Play Mode for normal UI/gameplay iteration. AIT/WebGL builds are too slow for every development loop, so reserve them for milestone browser/device spot checks such as WebGL-only asset loading, Korean font rendering, phone safe area, touch behavior, performance, and cache/build-marker verification.

## Current Status

- Resolved: `AIT > Dev Server > Start Server` launches the local Vite server after adding the AIT embedded pnpm folder to Windows `PATH` and fully restarting Unity Hub/Editor.
- Resolved: phone browser can reach the PC AIT Dev Server on the same LAN.
- Resolved in PC browser smoke: build-included `Assets/Resources/Prototype/...` sprites make parcel and conveyor/floor images appear correctly.
- Resolved in PC browser smoke: `Assets/Resources/Prototype/Fonts/NotoSansKR-VF.ttf` renders Korean HUD text in WebGL.
- Current runtime UI: UI Toolkit through `BoxStackPrototypeUi`, with `BoxStackPanelSettings` and `BoxStackRuntimeTheme` loaded from `Assets/Resources/Prototype/Ui/`.
- Current asset loading boundary: `BoxStackPrototypeAssetLoader` owns prototype config, font, parcel/background sprite loading, Editor-only PNG fallback, runtime sprite creation, placeholder parcel creation, solid sprite creation, and visible-alpha rect calculation.
- Remaining milestone spot check: phone browser testing is still useful for real portrait layout, touch input, safe area, WebGL performance, and cache/build-marker verification.

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

Verification:

- 2026-05-07: After rebuilding AIT/WebGL, PC browser testing confirmed that parcel and conveyor/floor images appear correctly.
- 2026-05-10: B014 PC browser smoke confirmed Korean text, parcel/conveyor art, and tap-to-drop behavior.
- 2026-05-13: B022 Editor Play verification confirmed `BoxStackPrototypeAssetLoader` loads config, Korean font, floor sprite, and 3 box visuals.
- 2026-05-13: B023 Editor Play verification confirmed `BoxStackPrototypeBoxVisualCatalog` is the active visual catalog, floor sprite `parcel_stack_base_01` loads, one `UIDocument` exists, and the UI marker shows `B023`.
- 2026-05-13: B024 Editor Play verification confirmed `BoxStackPrototypeUiStateFactory` builds the runtime UI state after the C# split, one `UIDocument` exists, and the UI marker shows `B024`.
- 2026-05-14: B025 Editor Play verification on Unity CLI Connector port `8090` confirmed one `UIDocument`, UI marker `B025`, and no `_statusLabel` field on `BoxStackPrototypeUi`; the right-side progress rail no longer creates the bottom status text label that could overlap the progress UI.
- 2026-05-14: B026 changes the HUD progress display from the right-side rail to a top-center horizontal panel. Placed boxes use filled square slots, remaining boxes use outline-only square slots, and the old numeric count text is removed. Editor Play verification on Unity CLI Connector port `8090` confirmed one `UIDocument`, UI marker `B026`, no `_countLabel`/`_progressFill` fields, Row progress layout, and 4 progress slots on stage 1.

Remaining verification:

1. Retest on a phone browser using the PC LAN IP at the next milestone checkpoint.

## Phone Browser Smoke Test Checklist

Use the `Network:` URL printed by the AIT Dev Server, for example:

```text
http://172.30.1.14:5173/index.html
```

The phone must be on the same network as the PC. Do not use `localhost` on the phone.

- Page loads without connection refused, timeout, or infinite loading.
- Portrait layout keeps the HUD, stage select, result popup, and undo button inside the visible safe area.
- UI Toolkit labels render Korean text in HUD, stage select, result popup, and undo states.
- Parcel boxes and the conveyor/floor use the intended image assets, not placeholder rectangles.
- Tap-to-drop input works reliably.
- Stage label opens stage select; locked stages stay disabled.
- `진행 초기화` and `전체 해금` appear only in Editor/development builds.
- Restart, stage select, and undo buttons respond to touch.
- Stages 1, 5, 10, 15, and 20 are playable enough to judge difficulty.
- No obvious freezes, browser crashes, or severe frame drops during stack collapse.
- The small build marker below the top-right HUD area shows the expected value for the runtime build being tested.

## Build Marker

`BoxStackPrototype` shows a tiny build marker below the top-right HUD area. The current runtime marker after the latest C# change is:

```text
B026
```

When changing C# code for mobile WebGL testing, manually increment `PrototypeBuildNumber` before rebuilding so the phone can confirm that it loaded the fresh build instead of a cached old build. Documentation-only or design-asset-only commits do not require a build marker increment.

## WebGL Korean Font Fix

WebGL text can lose Korean glyphs if the build relies on Unity's default runtime font. The current prototype includes:

```text
Assets/Resources/Prototype/Fonts/NotoSansKR-VF.ttf
```

`BoxStackPrototypeAssetLoader` loads it with:

```text
Resources.Load<Font>("Prototype/Fonts/NotoSansKR-VF")
```

`BoxStackPrototypeUi` then applies it to UI Toolkit text through the runtime UI layer.

Remaining milestone verification:

1. Rebuild WebGL.
2. Open the browser build.
3. Confirm Korean text appears in HUD, stage select, result popup, undo button, and build marker states.
4. Confirm the build marker matches the expected runtime marker for that test build.

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

The current HUD, stage select, result popup, and undo button are runtime UI Toolkit, not `OnGUI`/IMGUI. `BoxStackPrototypeUi` owns layout, safe-area placement, input blocking, Korean font assignment, result popup, stage select, and the Delivery Arcade HUD structure.

Current UI resources:

- `Assets/Resources/Prototype/Ui/BoxStackPanelSettings.asset`
- `Assets/Resources/Prototype/Ui/BoxStackRuntimeTheme.tss`
- `design/ui/delivery-arcade-assets/`

The `design/ui/delivery-arcade-assets/` PNG mini pack is a design-side prototype skin. It is committed for review, but it is not wired into the runtime UI yet.

## Current Worktree Note

As of B026, the runtime UI keeps the B016/B024 Delivery Arcade vector layout but moves placed-box progress into a top-center horizontal slot panel inspired by the B/C mockups. The Delivery Arcade UI PNG mini pack still exists only as design reference under `design/ui/delivery-arcade-assets/`.

## Next Suggested Task

For normal development, continue using Unity Editor Play Mode. The next runtime-facing task should be either:

1. Apply or intentionally defer the `design/ui/delivery-arcade-assets/` PNG skin in `BoxStackPrototypeUi`.
2. Continue productization by extracting another small boundary from `BoxStackPrototype.cs`.

Rebuild WebGL only at the next milestone browser/device checkpoint.
