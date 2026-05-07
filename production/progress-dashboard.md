# Progress Dashboard

Last updated: 2026-05-08

## Next Immediate Action

Rebuild WebGL and retest the physics tuning on the phone. Confirm `B001` is visible, then check whether lower-box sliding is reduced.

## Prototype / Playtest History

- 2026-05-02: Prototype harness created. No playable prototype or playtest verdict yet.
- 2026-05-02: First playable loop implemented as `Assets/Scripts/Prototype/BoxStackPrototype.cs`; playtest verdict pending.
- 2026-05-02: Camera follow added so taller stacks keep the active box and landing area in view; playtest verdict pending.
- 2026-05-02: Box visuals inset with thin edge frames so stack contact reads more clearly; playtest verdict pending.
- 2026-05-02: MainCamera rotation locked; camera now follows stack height by vertical movement only.
- 2026-05-02: Prototype converted from 3D cubes to 2D/2.5D parcel-box sprites using `Rigidbody2D` and `BoxCollider2D`; AI PNG asset pipeline now fits the direction.
- 2026-05-02: Basic stacking system judged good enough to continue; next prototype phase shifts from mechanics validation to 2D visual direction and PNG asset replacement.
- 2026-05-02: Lightweight art brief added and prototype code wired to load named parcel PNGs from `Assets/Art/Prototype/Parcel/` in Editor Play, with placeholder fallback.
- 2026-05-02: First AI-generated parcel PNG set added: basic, wide, tall, and stack base sprites with transparent backgrounds and Sprite importer metadata.
- 2026-05-02: Parcel PNG scaling/colliders changed to use visible alpha bounds, so transparent image padding no longer makes stacked boxes appear to float apart.
- 2026-05-03: First AI-generated logistics center background added and wired behind the prototype as a camera-fixed SpriteRenderer.
- 2026-05-03: Camera minimum y changed to derive from the floor bottom plus orthographic size with a small bottom padding, so portrait play cannot drift below the conveyor/floor baseline.
- 2026-05-04: Top HUD changed from one text line to game-like pill UI: box count pill, progress bar, and run/status pill.
- 2026-05-04: Four HUD mockup variants generated for selection: Toss Minimal, Delivery Tracker, Casual Game, and No Top HUD.
- 2026-05-04: HUD variant 01 Toss Minimal applied to the prototype as a single quiet top bar with box count, progress, and compact status text.
- 2026-05-04: 20-stage fixed-clear difficulty plan recorded at `design/quick-specs/20-stage-difficulty-plan-2026-05-04.md`; infinite stacking deferred to a later challenge-mode experiment.
- 2026-05-04: Success/fail result popup added so one play session has a clear ending and restart path.
- 2026-05-04: Logistics center image background disabled for focus testing; prototype now uses a parcel-brown solid camera background.
- 2026-05-05: Clear condition changed from box count only to single-column stacking; a settled box outside tolerance now fails immediately.
- 2026-05-05: Success now requires the completed 8-box stack to survive a 5-second validation window before the clear popup appears.
- 2026-05-05: Single-column tolerance relaxed from `0.45` to `0.75` so visually stable stacks are less likely to fail unfairly.
- 2026-05-05: Box `gravityScale` lowered from `2.4` to `1.6` to reduce excessive lower-box movement when stacks grow.
- 2026-05-05: Result popup restart now requires completing the button click on the button instead of any screen tap.
- 2026-05-05: Hardcoded `StageConfig` progression wired into the prototype with 20 fixed-clear stages, configured box sequences, speed/range multipliers, HUD stage display, and keyboard stage navigation.
- 2026-05-05: Stage-clear result popup now advances to the next stage; final stage clear loops back to stage 1.
- 2026-05-05: HUD stage label now opens a lightweight 20-stage select overlay; selecting a stage restarts the run on that stage.
- 2026-05-05: Playtest found stages 10+ significantly harder; prototype now tests a one-use undo skill that removes the last placed box instead of lowering the difficulty table.
- 2026-05-06: Undo test direction changed to a fail-popup rescue: the prototype captures a pre-drop stack snapshot, freezes the failed stack, and lets the player restore once per stage.
- 2026-05-06: Failure rescue UX changed toward a reward-ad prototype flow: the fail popup now offers "광고 보고 되돌리기" with a short mock confirmation delay before restoring the pre-drop stack snapshot.
- 2026-05-06: Fail popup now shows rescue status text, so playtests can verify whether players understand "available", "checking", and "used" states.
- 2026-05-06: Failure rescue popup copy clarified so the rescue ticket state appears before the reward-ad action: available, checking, and used states now use explicit stage-ticket wording.
- 2026-05-06: Failure rescue copy shortened toward user-facing wording: "한 번 되돌릴 수 있어요", "복구권 1회 남음", and "광고 보고 이어하기".
- 2026-05-06: Failure rescue policy changed to free-first: each stage now grants one free snapshot restore, then one mock reward-ad restore, then no further rescue.
- 2026-05-06: Stage progression now stores the highest unlocked stage with PlayerPrefs, disables locked stages in stage select, and unlocks the next stage on clear.
- 2026-05-06: Stage select now includes prototype-only progression controls for fast playtesting: reset unlock progress to stage 1 or unlock all 20 stages.
- 2026-05-06: Failure detection now also checks delayed stack collapse during normal play and drop resolution, so gravity-driven collapse can show the fail popup immediately.
- 2026-05-06: Runtime `PhysicsMaterial2D` added to parcel boxes and the floor with `friction = 1.2`, `bounciness = 0` to reduce unwanted sliding while keeping gravity collapse.
- 2026-05-06: Stage select progress test controls are now limited to Editor/development builds so reset/unlock helpers do not appear in normal user builds.
- 2026-05-06: Editor check confirmed stage select progress controls remain visible for playtesting; HUD, stage select, and result popup layout now use `Screen.safeArea` converted into IMGUI coordinates.
- 2026-05-07: AIT/WebGL browser testing found two blockers: Unity-launched Dev Server cannot find `pnpm`, and WebGL cannot load prototype PNGs through the Editor-only `AssetDatabase` fallback. Details and workaround are in `docs/workflow/ait-webgl-testing-notes.md`.
- 2026-05-07: Prototype PNGs were copied into `Assets/Resources/Prototype/...`, and the runtime loader now falls back from `Resources.Load<Sprite>` to `Resources.Load<Texture2D>` so WebGL builds can include parcel and floor sprites.
- 2026-05-07: After rebuilding AIT/WebGL, PC browser testing confirmed that parcel and conveyor/floor images now appear correctly.
- 2026-05-07: `AIT > Dev Server > Start Server` now works from the Unity menu after adding the AIT embedded pnpm folder to Windows `PATH` and fully restarting Unity Hub/Editor.
- 2026-05-08: Phone browser can reach the PC AIT Dev Server on the LAN (`172.30.1.14:5173`).
- 2026-05-08: Lower-box sliding felt too strong on phone WebGL, so parcel friction was raised to `2.4` and Rigidbody2D damping was raised to `linearDamping = 0.6`, `angularDamping = 0.8` for the next build.
- 2026-05-08: A small `B001` build marker was added below the top-right HUD area so phone tests can confirm a fresh WebGL build is loaded.

## Current Decisions

- Project mode is casual game, prototype-first.
- Keep planning lightweight: active state plus dashboard, no heavy GDD/ADR/review workflow by default.
- Record playtest outcomes briefly in this dashboard and `production/session-state/active.md`.
- Target launch context is Toss app-in-app, so prototype direction favors lightweight 2D/2.5D sprites over modeled 3D assets.
- Current prototype uses timing-based drops, runtime placeholder parcel sprites, 2D physics stacking, rotation-locked orthographic camera follow, stage-specific target counts, and quick restart.
- Next asset direction is AI-generated 2D PNGs with clear silhouettes, visible contact edges, transparent backgrounds, and mobile-readable parcel details.
- Expected prototype sprite names are `parcel_box_basic_01.png`, `parcel_box_wide_01.png`, `parcel_box_tall_01.png`, and `parcel_stack_base_01.png`.
- Prototype sprite loading also falls back from Sprite assets to Texture2D-to-Sprite creation, so Play mode remains tolerant of importer refresh timing.
- Parcel PNG visuals and `BoxCollider2D` sizes are based on visible alpha bounds rather than the full transparent source image rectangle.
- Unity CLI Connector is installed and verified; use `docs/workflow/unity-cli-connector.md` for live Editor inspection, asset refresh, Play/Stop, console reads, screenshots, and injected C# checks.
- Background direction is a bright 2D parcel logistics center with a calm central play lane, not a full 3D modeled warehouse.
- Current background test uses a parcel-brown solid color because the logistics center image was distracting during play.
- Camera bottom clamp is tied to the prototype floor constants instead of a fixed `CameraBaseY` number.
- First game UI direction is Toss Minimal: a compact top bar with progress feedback plus a central success/fail result popup for run completion.
- Main progression direction is fixed-count clear stages, not infinite stacking, for the first Toss app-in-app MVP.
- Clear requires every settled box to stay within the single-column x tolerance from the first placed box; crossing the tolerance fails immediately, and the completed stage stack must survive 5 seconds before success.
- Stage implementation uses hardcoded prototype `StageConfig` data, a HUD-opened stage select overlay, and a result-popup next-stage flow; playtest stages 1, 5, 10, 15, and 20 before tuning the full run.
- Later-stage difficulty should first be tested with a free-first fail-popup rescue, because it softens difficulty before introducing the reward-ad recovery prompt.
- Actual ad SDK integration is deferred; current prototype only tests the UX timing and player expectation with a mock confirmation delay.
- The fail-popup rescue state should use short user-facing copy while distinguishing free continuation from reward-ad continuation.
- Ad/free rescue use should not reduce clear rewards or visible stage progress, because that could discourage players from watching an ad.
- Stage unlock state uses PlayerPrefs for prototype speed; replace or wrap it later if App-in-Toss storage policy requires a different persistence layer.
- Stage select may expose prototype progress test controls while validating difficulty and unlock persistence, but only in Editor/development builds.
- Prototype IMGUI layout should respect mobile safe area for HUD, stage select, and result popup placement before App-in-Toss package testing.
- WebGL visual parity with Editor Play should use build-included prototype sprites; the current prototype uses duplicate PNGs under `Assets/Resources/Prototype/...` for speed.
- The Unity AIT Dev Server menu depends on the embedded AIT pnpm folder (`C:\Users\Ahneunsung\AppData\Local\.ait-unity-sdk\nodejs\v24.13.0\win-x64`) being available on Windows `PATH`.
- Use a tiny in-game build marker (`B001`, then increment manually) during mobile WebGL tests to distinguish a fresh build from a cached old build.

## Open Questions

- Which parcel-box visual style best fits Toss app-in-app: clean fintech-casual, cute toy-like, or lightly realistic delivery packaging?
- How many box variations are needed before the stack stops feeling repetitive?
- Should the next playtest measure visual clarity, replay desire, or perceived brand fit?
- Should the background stay screen-fixed for prototype readability, or eventually scroll/parallax with stack height?
- What bottom padding feels best on the target portrait app-in-app viewport?
- Does the safe-area-aware IMGUI layout keep HUD and popups clear of Toss webview/status-bar insets on real devices?
- Is the duplicate `Assets/Resources/Prototype/...` copy acceptable through the prototype phase, or should it be replaced later with serialized/build-included asset references?
- Does the phone browser load the AIT/WebGL build reliably through the PC LAN IP?
- Does the top pill HUD feel Toss-app-like enough, or does it need to be quieter?
- Is the HUD stage-label click plus stage select overlay enough for prototype stage testing?
- Does the parcel-brown solid background improve focus compared with the logistics center image?
- Are the current single-column tolerance (`0.75`) and 5-second clear validation window fair enough across stages 1, 5, 10, 15, and 20?
- Does immediate collapse detection feel fair, or does it punish harmless physics wobble too quickly?
- Does `friction = 2.4` plus stronger damping reduce the unwanted slide without making the stack feel glued together?
- Does one free rescue plus one mock reward-ad rescue make stages 10+ feel fair without removing too much challenge?
- Does the mock reward-ad confirmation delay feel natural, or does it interrupt retry flow too much?
- Is PlayerPrefs enough for prototype progression testing before App-in-Toss storage requirements are confirmed?
- Are the Editor/development-only `진행 초기화` and `전체 해금` controls enough for fast stage testing, or should they move behind a less visible debug gesture later?

## Risks

- Dashboard staleness if `production/session-state/active.md` changes without this file being updated.
- AI-generated assets may look inconsistent unless the first prompt set tightly controls angle, outline, color, and export rules.
- Visual polish may hide physics readability issues if box silhouettes and contact edges are not kept clear.
- Background detail may reduce falling-box readability if the center play lane feels too busy on mobile.
- Camera floor clamp can make the starting view feel too high if the target portrait aspect ratio changes significantly.
- Runtime `OnGUI` HUD is still prototype-only; production UI should move to a proper Unity UI layer.
- Safe area is handled in the prototype IMGUI layer, but real App-in-Toss WebGL/device testing is still needed because Editor Game view may not emulate every inset.
- AIT Dev Server now launches from the Unity menu after the PATH fix, but this depends on the AIT embedded pnpm folder remaining available on Windows `PATH`.
- WebGL sprite parity is confirmed in PC browser after the AIT/WebGL rebuild, but phone browser testing is still needed.
- Later 12-box stages may feel random if physics instability dominates player timing skill.
- Snapshot-based rescue should feel fair, but one free plus one ad rescue may be too forgiving if it removes too much risk from hard stages.
- Reward-style rescue can still feel monetized too early if the second failure prompt appears before the player accepts the stage as fair.
- PlayerPrefs is fine for local prototype persistence, but App-in-Toss production storage requirements may require replacing it later.
- Prototype progression controls are useful for playtest speed, and are now hidden from non-development user builds; confirm this again before any App-in-Toss package test.
- Unity batchmode import could not run while the project was already open, so in-editor refresh/play verification is still needed.
- Unity CLI Connector requires the Unity Editor to be open with this project loaded; if port `8090` does not respond, scan `8091` through `8099`.
