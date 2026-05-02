# Progress Dashboard

Last updated: 2026-05-02

## Next Immediate Action

Open Unity, let assets refresh if needed, then press Play in `SampleScene` to check first PNG readability, stacking feel, and Toss app-in-app fit.

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

## Open Questions

- Which parcel-box visual style best fits Toss app-in-app: clean fintech-casual, cute toy-like, or lightly realistic delivery packaging?
- How many box variations are needed before the stack stops feeling repetitive?
- Should the next playtest measure visual clarity, replay desire, or perceived brand fit?
- Should the first visual pass include background art now, or keep the background simple until box readability is proven?

## Risks

- Dashboard staleness if `production/session-state/active.md` changes without this file being updated.
- AI-generated assets may look inconsistent unless the first prompt set tightly controls angle, outline, color, and export rules.
- Visual polish may hide physics readability issues if box silhouettes and contact edges are not kept clear.
- Unity batchmode import could not run while the project was already open, so in-editor refresh/play verification is still needed.
- Unity CLI Connector requires the Unity Editor to be open with this project loaded; if port `8090` does not respond, scan `8091` through `8099`.
