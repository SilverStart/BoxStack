<!-- STATUS -->
Epic: Prototype Harness
Feature: BoxStack 2D App-in-App Prototype
Task: Retest WebGL floor-friction tuning on phone
<!-- /STATUS -->

# Active Session State

This file is the source of truth for the current prototype status. Keep it short and update it whenever the playable loop, next action, risks, or playtest verdict changes.

## Project Mode

Casual game, prototype-first. Skip heavy GDD/ADR/review workflow unless explicitly requested.

## Current Prototype

- Name: BoxStack playable prototype
- State: Core 2D stacking system validated as good enough for the next prototype phase.
- Goal: Turn the validated timing-based parcel-box stacking loop into a Toss app-in-app style 2D/2.5D vertical slice using AI-generated PNG assets.
- Current implementation: `Assets/Scripts/Prototype/BoxStackPrototype.cs` auto-bootstraps an orthographic camera, 2D floor, `Rigidbody2D`/`BoxCollider2D` stacking, input, win/fail state, Toss Minimal top HUD, success/fail result popup, 20 hardcoded fixed-clear stages, stage select overlay, stage-clear next-stage flow, PlayerPrefs-based highest-stage unlock persistence, Editor/development-build-only progress test controls (`진행 초기화`, `전체 해금`), mobile safe-area-aware IMGUI layout, a free-first failure rescue policy with one free restore plus one mock reward-ad restore per stage, user-facing fail-popup rescue copy (`한 번 되돌릴 수 있어요`, `복구권 1회 남음`, `바로 이어하기`, `광고 보고 이어하기`), runtime parcel contact material (`friction = 2.4`, `bounciness = 0`), separate floor contact material (`friction = 6.0`, `bounciness = 0`), stronger damping (`linearDamping = 0.6`, `angularDamping = 0.8`), and rotation-locked vertical camera follow in `SampleScene`. Clear requires boxes to stay in a single vertical column, using the first placed box as the center-x reference; any box that settles outside tolerance fails immediately, including delayed gravity collapse while playing or resolving a drop. After the stage target count is stacked, the stack must survive a 5-second validation window before success. Box `gravityScale` is currently `1.6` after lowering it to reduce excessive movement in lower boxes. The camera minimum y is derived from the floor bottom plus orthographic size with a small bottom padding, so the view does not drift below the conveyor/floor baseline. Imported parcel PNGs are scaled and collided by visible alpha bounds so transparent padding does not create floating gaps.
- Asset pipeline: Prototype art brief exists at `design/assets/boxstack-prototype-art-brief.md`; first AI-generated parcel PNG set is in `Assets/Art/Prototype/Parcel/`, and a logistics center background is in `Assets/Art/Prototype/Backgrounds/logistics_center_bg_01.png`. WebGL-build-included copies now live under `Assets/Resources/Prototype/...` for `Resources.Load`. The logistics center background is currently disabled for readability testing; the prototype uses a parcel-brown solid camera background instead. Runtime placeholders still cover missing parcel assets.
- UI exploration: Four HUD mockup variants were generated in `design/ui/hud-variants/`; variant 01 Toss Minimal is now applied to the prototype HUD for playtest. Result popup is currently an `OnGUI` prototype overlay with restart by button release or `R`. WebGL Korean text fallback now uses `Assets/Resources/Prototype/Fonts/NotoSansKR-VF.ttf` applied to prototype GUI styles. A tiny `B003` prototype build marker is shown below the top-right HUD area so mobile WebGL rebuild/caching can be checked quickly.
- Stage plan: A 20-stage fixed-clear difficulty plan is recorded at `design/quick-specs/20-stage-difficulty-plan-2026-05-04.md` and now wired into the prototype as hardcoded `StageConfig` data. Stage navigation is available during Editor Play with previous/next keyboard controls within the unlocked range, and tapping/clicking the HUD stage label opens a simple 20-stage select overlay where locked stages are disabled. After playtesting, stages 10+ feel significantly harder, so the prototype now tests a stage-limited failure rescue undo instead of lowering the difficulty table. Main mode should use fixed box-count clears; infinite stacking is deferred as a later challenge-mode experiment.
- Unity editor control: Unity CLI Connector is installed and verified. See `docs/workflow/unity-cli-connector.md`; use it for live Editor C# inspection, asset refresh, Play/Stop, console reads, and screenshots when Unity is already open.
- AIT/WebGL test notes: `docs/workflow/ait-webgl-testing-notes.md` records the current Dev Server and WebGL visual mismatch diagnosis. Browser testing confirmed that the copied `Assets/Resources/Prototype/...` sprites now make parcel and conveyor/floor images appear correctly after rebuilding. `AIT > Dev Server > Start Server` now works after adding the AIT embedded pnpm folder to Windows `PATH` and fully restarting Unity Hub/Editor. Phone LAN access works from `172.30.1.40` to the PC Dev Server at `172.30.1.14:5173`.
- Commit checkpoint: WebGL resource loading, mobile safe-area UI, physics tuning, build marker, AIT/WebGL notes, and AIT WebGL build settings were split into focused Korean commits through `94a6549 AIT 웹GL 빌드 설정 정리`. The worktree was clean immediately after that commit checkpoint.
- Playtest verdict: Basic game system and box-stacking feel are acceptable for prototype continuation.

## Next Action

Rebuild WebGL and retest on the phone. Confirm the visible build marker shows `B003`, then check whether floor-only `friction = 6.0` keeps the bottom box from sliding while parcel `friction = 2.4`, `linearDamping = 0.6`, and `angularDamping = 0.8` still allow natural stack collapse.

## Open Questions

- Which parcel-box visual style best fits a Toss app-in-app experience: clean fintech-casual, cute toy-like, or lightly realistic delivery packaging?
- How many box variations are needed before the stack stops feeling repetitive?
- Should the next playtest measure visual clarity, replay desire, or perceived brand fit?
- Should the background stay screen-fixed for prototype readability, or eventually scroll/parallax with stack height?
- What bottom padding feels best on the target portrait app-in-app viewport?
- Does the safe-area-aware IMGUI layout keep HUD and popups clear of Toss webview/status-bar insets on real devices?
- Is the duplicate `Assets/Resources/Prototype/...` copy acceptable through the prototype phase, or should it be replaced later with serialized/build-included asset references?
- Does the phone browser load the AIT/WebGL build reliably through the PC LAN IP?
- Does the WebGL build render Korean text correctly across HUD, stage select, result popup, and rescue button states?
- Does the top pill HUD feel Toss-app-like enough, or does it need to be quieter?
- Is the HUD stage-label click plus stage select overlay enough for prototype stage testing?
- Does the parcel-brown solid background improve focus compared with the logistics center image?
- Are the current single-column tolerance (`0.75`) and 5-second clear validation window fair enough across stages 1, 5, 10, 15, and 20?
- Does immediate collapse detection feel fair, or does it punish harmless physics wobble too quickly?
- Does floor-only `friction = 6.0` stop the bottom box from sliding without making the stack feel glued together?
- Does one free rescue plus one mock reward-ad rescue make stages 10+ feel fair without removing too much challenge?
- Does the mock reward-ad confirmation delay feel natural, or does it interrupt retry flow too much?
- Is PlayerPrefs enough for prototype progression testing before App-in-Toss storage requirements are confirmed?
- Are the Editor/development-only `진행 초기화` and `전체 해금` controls enough for fast stage testing, or should they move behind a less visible debug gesture later?

## Risks

- Progress dashboard can become stale if active session state changes without a matching dashboard update.
- AI-generated assets may look inconsistent unless the first prompt set tightly controls angle, outline, color, and export rules.
- Visual polish may hide physics readability issues if box silhouettes and contact edges are not kept clear.
- Background detail may reduce falling-box readability if the center play lane feels too busy on mobile.
- Camera floor clamp can make the starting view feel too high if the target portrait aspect ratio changes significantly.
- Runtime `OnGUI` HUD is still prototype-only; production UI should move to a proper Unity UI layer.
- The prototype now includes a full Korean font in `Resources`, which is acceptable for fast WebGL validation but adds build weight until UI/font handling is replaced or subsetted.
- Safe area is handled in the prototype IMGUI layer, but real App-in-Toss WebGL/device testing is still needed because Editor Game view may not emulate every inset.
- AIT Dev Server now launches from the Unity menu after the PATH fix, but this depends on the AIT embedded pnpm folder remaining available on Windows `PATH`.
- WebGL sprite parity is confirmed in PC browser after the AIT/WebGL rebuild, but phone browser testing is still needed.
- Later 12-box stages may feel random if physics instability dominates player timing skill.
- Snapshot-based rescue should feel fair, but it may feel too powerful if it removes all risk from hard stages.
- Reward-style rescue can make failure feel monetized too early if the base difficulty is not already perceived as fair.
- PlayerPrefs is fine for local prototype persistence, but App-in-Toss production storage requirements may require replacing it later.
- Prototype progression controls are useful for playtest speed, and are now hidden from non-development user builds; confirm this again before any App-in-Toss package test.
- Unity batchmode import could not run while the project was already open, so in-editor refresh/play verification is still needed.
- Unity CLI Connector requires the Unity Editor to be open with this project loaded; if port `8090` does not respond, scan `8091` through `8099`.
