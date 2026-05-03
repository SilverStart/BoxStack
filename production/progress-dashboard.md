# Progress Dashboard

Last updated: 2026-05-04

## Next Immediate Action

Implement `StageConfig` in `Assets/Scripts/Prototype/BoxStackPrototype.cs`, wire the 20-stage table from `design/quick-specs/20-stage-difficulty-plan-2026-05-04.md`, then playtest stages 1, 5, 10, 15, and 20.

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

## Current Decisions

- Project mode is casual game, prototype-first.
- Keep planning lightweight: active state plus dashboard, no heavy GDD/ADR/review workflow by default.
- Record playtest outcomes briefly in this dashboard and `production/session-state/active.md`.
- Target launch context is Toss app-in-app, so prototype direction favors lightweight 2D/2.5D sprites over modeled 3D assets.
- Current prototype uses timing-based drops, runtime placeholder parcel sprites, 2D physics stacking, rotation-locked orthographic camera follow, 8-box target, and quick restart.
- Next asset direction is AI-generated 2D PNGs with clear silhouettes, visible contact edges, transparent backgrounds, and mobile-readable parcel details.
- Expected prototype sprite names are `parcel_box_basic_01.png`, `parcel_box_wide_01.png`, `parcel_box_tall_01.png`, and `parcel_stack_base_01.png`.
- Prototype sprite loading also falls back from Sprite assets to Texture2D-to-Sprite creation, so Play mode remains tolerant of importer refresh timing.
- Parcel PNG visuals and `BoxCollider2D` sizes are based on visible alpha bounds rather than the full transparent source image rectangle.
- Unity CLI Connector is installed and verified; use `docs/workflow/unity-cli-connector.md` for live Editor inspection, asset refresh, Play/Stop, console reads, screenshots, and injected C# checks.
- Background direction is a bright 2D parcel logistics center with a calm central play lane, not a full 3D modeled warehouse.
- Camera bottom clamp is tied to the prototype floor constants instead of a fixed `CameraBaseY` number.
- First game UI direction is Toss Minimal: a compact top bar with progress feedback, with heavier result UI deferred.
- Main progression direction is fixed-count clear stages, not infinite stacking, for the first Toss app-in-app MVP.
- Stage implementation should start with hardcoded prototype `StageConfig` data, then playtest stages 1, 5, 10, 15, and 20 before filling/tuning the full run.

## Open Questions

- Which parcel-box visual style best fits Toss app-in-app: clean fintech-casual, cute toy-like, or lightly realistic delivery packaging?
- How many box variations are needed before the stack stops feeling repetitive?
- Should the next playtest measure visual clarity, replay desire, or perceived brand fit?
- Should the background stay screen-fixed for prototype readability, or eventually scroll/parallax with stack height?
- What bottom padding feels best on the target portrait app-in-app viewport?
- Does the top pill HUD feel Toss-app-like enough, or does it need to be quieter?
- Should temporary stage navigation be keyboard-only, HUD-visible, or both during prototype playtests?

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
