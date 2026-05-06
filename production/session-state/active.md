<!-- STATUS -->
Epic: Prototype Harness
Feature: BoxStack 2D App-in-App Prototype
Task: Playtest progression controls
<!-- /STATUS -->

# Active Session State

This file is the source of truth for the current prototype status. Keep it short and update it whenever the playable loop, next action, risks, or playtest verdict changes.

## Project Mode

Casual game, prototype-first. Skip heavy GDD/ADR/review workflow unless explicitly requested.

## Current Prototype

- Name: BoxStack playable prototype
- State: Core 2D stacking system validated as good enough for the next prototype phase.
- Goal: Turn the validated timing-based parcel-box stacking loop into a Toss app-in-app style 2D/2.5D vertical slice using AI-generated PNG assets.
- Current implementation: `Assets/Scripts/Prototype/BoxStackPrototype.cs` auto-bootstraps an orthographic camera, 2D floor, `Rigidbody2D`/`BoxCollider2D` stacking, input, win/fail state, Toss Minimal top HUD, success/fail result popup, 20 hardcoded fixed-clear stages, stage select overlay, stage-clear next-stage flow, PlayerPrefs-based highest-stage unlock persistence, prototype-only progress test controls (`진행 초기화`, `전체 해금`), a free-first failure rescue policy with one free restore plus one mock reward-ad restore per stage, user-facing fail-popup rescue copy (`한 번 되돌릴 수 있어요`, `복구권 1회 남음`, `바로 이어하기`, `광고 보고 이어하기`), and rotation-locked vertical camera follow in `SampleScene`. Clear requires boxes to stay in a single vertical column, using the first placed box as the center-x reference; any box that settles outside tolerance fails immediately. After the stage target count is stacked, the stack must survive a 5-second validation window before success. Box `gravityScale` is currently `1.6` after lowering it to reduce excessive movement in lower boxes. The camera minimum y is derived from the floor bottom plus orthographic size with a small bottom padding, so the view does not drift below the conveyor/floor baseline. Imported parcel PNGs are scaled and collided by visible alpha bounds so transparent padding does not create floating gaps.
- Asset pipeline: Prototype art brief exists at `design/assets/boxstack-prototype-art-brief.md`; first AI-generated parcel PNG set is in `Assets/Art/Prototype/Parcel/`, and a logistics center background is in `Assets/Art/Prototype/Backgrounds/logistics_center_bg_01.png`. The logistics center background is currently disabled for readability testing; the prototype uses a parcel-brown solid camera background instead. Runtime placeholders still cover missing parcel assets.
- UI exploration: Four HUD mockup variants were generated in `design/ui/hud-variants/`; variant 01 Toss Minimal is now applied to the prototype HUD for playtest. Result popup is currently an `OnGUI` prototype overlay with restart by button release or `R`.
- Stage plan: A 20-stage fixed-clear difficulty plan is recorded at `design/quick-specs/20-stage-difficulty-plan-2026-05-04.md` and now wired into the prototype as hardcoded `StageConfig` data. Stage navigation is available during Editor Play with previous/next keyboard controls within the unlocked range, and tapping/clicking the HUD stage label opens a simple 20-stage select overlay where locked stages are disabled. After playtesting, stages 10+ feel significantly harder, so the prototype now tests a stage-limited failure rescue undo instead of lowering the difficulty table. Main mode should use fixed box-count clears; infinite stacking is deferred as a later challenge-mode experiment.
- Unity editor control: Unity CLI Connector is installed and verified. See `docs/workflow/unity-cli-connector.md`; use it for live Editor C# inspection, asset refresh, Play/Stop, console reads, and screenshots when Unity is already open.
- Playtest verdict: Basic game system and box-stacking feel are acceptable for prototype continuation.

## Next Action

Playtest prototype progression controls. Confirm `진행 초기화` returns unlock progress to stage 1, `전체 해금` opens all 20 stages for late-stage testing, and the selected unlock state remains available after restarting Play mode.

## Open Questions

- Which parcel-box visual style best fits a Toss app-in-app experience: clean fintech-casual, cute toy-like, or lightly realistic delivery packaging?
- How many box variations are needed before the stack stops feeling repetitive?
- Should the next playtest measure visual clarity, replay desire, or perceived brand fit?
- Should the background stay screen-fixed for prototype readability, or eventually scroll/parallax with stack height?
- What bottom padding feels best on the target portrait app-in-app viewport?
- Does the top pill HUD feel Toss-app-like enough, or does it need to be quieter?
- Is the HUD stage-label click plus stage select overlay enough for prototype stage testing?
- Does the parcel-brown solid background improve focus compared with the logistics center image?
- Are the current single-column tolerance (`0.75`) and 5-second clear validation window fair enough across stages 1, 5, 10, 15, and 20?
- Does one free rescue plus one mock reward-ad rescue make stages 10+ feel fair without removing too much challenge?
- Does the mock reward-ad confirmation delay feel natural, or does it interrupt retry flow too much?
- Is PlayerPrefs enough for prototype progression testing before App-in-Toss storage requirements are confirmed?
- Are the prototype-only `진행 초기화` and `전체 해금` controls enough for fast stage testing, or should they move behind an Editor-only debug gesture later?

## Risks

- Progress dashboard can become stale if active session state changes without a matching dashboard update.
- AI-generated assets may look inconsistent unless the first prompt set tightly controls angle, outline, color, and export rules.
- Visual polish may hide physics readability issues if box silhouettes and contact edges are not kept clear.
- Background detail may reduce falling-box readability if the center play lane feels too busy on mobile.
- Camera floor clamp can make the starting view feel too high if the target portrait aspect ratio changes significantly.
- Runtime `OnGUI` HUD is still prototype-only; production UI should move to a proper Unity UI layer.
- Later 12-box stages may feel random if physics instability dominates player timing skill.
- Snapshot-based rescue should feel fair, but it may feel too powerful if it removes all risk from hard stages.
- Reward-style rescue can make failure feel monetized too early if the base difficulty is not already perceived as fair.
- PlayerPrefs is fine for local prototype persistence, but App-in-Toss production storage requirements may require replacing it later.
- Prototype-only progression controls are useful for playtest speed, but they must be removed or hidden before any user-facing build.
- Unity batchmode import could not run while the project was already open, so in-editor refresh/play verification is still needed.
- Unity CLI Connector requires the Unity Editor to be open with this project loaded; if port `8090` does not respond, scan `8091` through `8099`.
