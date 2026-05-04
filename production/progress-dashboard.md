# Progress Dashboard

Last updated: 2026-05-05

## Next Immediate Action

Playtest the HUD stage-label click, stage select overlay, stage selection restart behavior, stage-clear next-stage flow, then test stages 1, 5, 10, 15, and 20 using the wired `StageConfig` table in `Assets/Scripts/Prototype/BoxStackPrototype.cs`; tune target counts, box sequences, speed multipliers, and range multipliers if the difficulty curve feels unfair.

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

## Open Questions

- Which parcel-box visual style best fits Toss app-in-app: clean fintech-casual, cute toy-like, or lightly realistic delivery packaging?
- How many box variations are needed before the stack stops feeling repetitive?
- Should the next playtest measure visual clarity, replay desire, or perceived brand fit?
- Should the background stay screen-fixed for prototype readability, or eventually scroll/parallax with stack height?
- What bottom padding feels best on the target portrait app-in-app viewport?
- Does the top pill HUD feel Toss-app-like enough, or does it need to be quieter?
- Is the HUD stage-label click plus stage select overlay enough for prototype stage testing?
- Does the parcel-brown solid background improve focus compared with the logistics center image?
- Are the current single-column tolerance (`0.75`) and 5-second clear validation window fair enough across stages 1, 5, 10, 15, and 20?

## Risks

- Dashboard staleness if `production/session-state/active.md` changes without this file being updated.
- AI-generated assets may look inconsistent unless the first prompt set tightly controls angle, outline, color, and export rules.
- Visual polish may hide physics readability issues if box silhouettes and contact edges are not kept clear.
- Background detail may reduce falling-box readability if the center play lane feels too busy on mobile.
- Camera floor clamp can make the starting view feel too high if the target portrait aspect ratio changes significantly.
- Runtime `OnGUI` HUD is still prototype-only; production UI should move to a proper Unity UI layer.
- Later 12-box stages may feel random if physics instability dominates player timing skill.
- Unity batchmode import could not run while the project was already open, so in-editor refresh/play verification is still needed.
- Unity CLI Connector requires the Unity Editor to be open with this project loaded; if port `8090` does not respond, scan `8091` through `8099`.
