<!-- STATUS -->
Epic: Prototype Harness
Feature: BoxStack 2D App-in-App Prototype
Task: Playtest first AI parcel PNG set
<!-- /STATUS -->

# Active Session State

This file is the source of truth for the current prototype status. Keep it short and update it whenever the playable loop, next action, risks, or playtest verdict changes.

## Project Mode

Casual game, prototype-first. Skip heavy GDD/ADR/review workflow unless explicitly requested.

## Current Prototype

- Name: BoxStack playable prototype
- State: Core 2D stacking system validated as good enough for the next prototype phase.
- Goal: Turn the validated timing-based parcel-box stacking loop into a Toss app-in-app style 2D/2.5D vertical slice using AI-generated PNG assets.
- Current implementation: `Assets/Scripts/Prototype/BoxStackPrototype.cs` auto-bootstraps an orthographic camera, 2D floor, `Rigidbody2D`/`BoxCollider2D` stacking, input, win/fail state, simple HUD, and rotation-locked vertical camera follow in `SampleScene`. Imported parcel PNGs are scaled and collided by visible alpha bounds so transparent padding does not create floating gaps.
- Asset pipeline: Prototype art brief exists at `design/assets/boxstack-prototype-art-brief.md`; first AI-generated parcel PNG set is in `Assets/Art/Prototype/Parcel/` with Sprite importer `.meta` files and runtime placeholder fallback if loading fails.
- Unity editor control: Unity CLI Connector is installed and verified. See `docs/workflow/unity-cli-connector.md`; use it for live Editor C# inspection, asset refresh, Play/Stop, console reads, and screenshots when Unity is already open.
- Playtest verdict: Basic game system and box-stacking feel are acceptable for prototype continuation.

## Next Action

Open Unity, let assets refresh if needed, then press Play in `SampleScene` to check first PNG readability, stacking feel, and Toss app-in-app fit.

## Open Questions

- Which parcel-box visual style best fits a Toss app-in-app experience: clean fintech-casual, cute toy-like, or lightly realistic delivery packaging?
- How many box variations are needed before the stack stops feeling repetitive?
- Should the next playtest measure visual clarity, replay desire, or perceived brand fit?
- Should the first visual pass include background art now, or keep the background simple until box readability is proven?

## Risks

- Progress dashboard can become stale if active session state changes without a matching dashboard update.
- AI-generated assets may look inconsistent unless the first prompt set tightly controls angle, outline, color, and export rules.
- Visual polish may hide physics readability issues if box silhouettes and contact edges are not kept clear.
- Unity batchmode import could not run while the project was already open, so in-editor refresh/play verification is still needed.
- Unity CLI Connector requires the Unity Editor to be open with this project loaded; if port `8090` does not respond, scan `8091` through `8099`.
