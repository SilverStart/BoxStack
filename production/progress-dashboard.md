# Progress Dashboard

Last updated: 2026-05-02

## Next Immediate Action

Open `SampleScene`, press Play, and verify the 2D parcel-box stack reads clearly as a Toss app-in-app prototype.

## Prototype / Playtest History

- 2026-05-02: Prototype harness created. No playable prototype or playtest verdict yet.
- 2026-05-02: First playable loop implemented as `Assets/Scripts/Prototype/BoxStackPrototype.cs`; playtest verdict pending.
- 2026-05-02: Camera follow added so taller stacks keep the active box and landing area in view; playtest verdict pending.
- 2026-05-02: Box visuals inset with thin edge frames so stack contact reads more clearly; playtest verdict pending.
- 2026-05-02: MainCamera rotation locked; camera now follows stack height by vertical movement only.
- 2026-05-02: Prototype converted from 3D cubes to 2D/2.5D parcel-box sprites using `Rigidbody2D` and `BoxCollider2D`; AI PNG asset pipeline now fits the direction.

## Current Decisions

- Project mode is casual game, prototype-first.
- Keep planning lightweight: active state plus dashboard, no heavy GDD/ADR/review workflow by default.
- Record playtest outcomes briefly in this dashboard and `production/session-state/active.md`.
- Target launch context is Toss app-in-app, so prototype direction favors lightweight 2D/2.5D sprites over modeled 3D assets.
- Current prototype uses timing-based drops, runtime placeholder parcel sprites, 2D physics stacking, rotation-locked orthographic camera follow, 8-box target, and quick restart.

## Open Questions

- Is timing-only control enough, or should drag/aim be added?
- Is 8 boxes the right first target count?
- Is the 2D/2.5D parcel sprite direction strong enough for AI PNG asset production?
- Does vertical-only orthographic camera follow keep the drop target readable enough?
- First playtest success criteria.

## Risks

- 2D physics feel still needs in-editor play verification.
- Placeholder parcel sprite needs replacement with AI-generated PNG assets after core loop validation.
- Dashboard staleness if `production/session-state/active.md` changes without this file being updated.
- Physics tuning may make the stack feel too random or too easy before playtest feedback exists.
