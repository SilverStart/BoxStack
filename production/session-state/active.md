<!-- STATUS -->
Epic: Prototype Harness
Feature: BoxStack 2D App-in-App Prototype
Task: Convert first playable loop to 2D/2.5D parcel stacking
<!-- /STATUS -->

# Active Session State

This file is the source of truth for the current prototype status. Keep it short and update it whenever the playable loop, next action, risks, or playtest verdict changes.

## Project Mode

Casual game, prototype-first. Skip heavy GDD/ADR/review workflow unless explicitly requested.

## Current Prototype

- Name: BoxStack playable prototype
- State: 2D/2.5D playable loop in progress
- Goal: Test whether timing-based parcel-box drops into a 2D physics stack fit a Toss app-in-app casual game and AI-generated PNG asset pipeline.
- Current implementation: `Assets/Scripts/Prototype/BoxStackPrototype.cs` auto-bootstraps an orthographic camera, 2D floor, runtime placeholder parcel-box sprite, `Rigidbody2D`/`BoxCollider2D` stacking, input, win/fail state, simple HUD, and rotation-locked vertical camera follow in `SampleScene`.

## Next Action

Open `SampleScene`, press Play, and verify the 2D parcel-box stack reads clearly as a Toss app-in-app prototype.

## Open Questions

- Is a pure timing-drop interaction enough, or should horizontal drag/aim control be added?
- Is 8 boxes the right first target count?
- Is the 2D/2.5D parcel sprite direction strong enough for AI PNG asset production?
- Does vertical-only orthographic camera follow keep the drop target readable enough?
- Should the first playtest measure clarity, fun, difficulty, or session length?

## Risks

- 2D physics feel still needs in-editor play verification.
- The placeholder parcel sprite needs replacement with AI-generated PNG assets after the core loop is validated.
- Progress dashboard can become stale if active session state changes without a matching dashboard update.
- Physics tuning may make the stack feel too random or too easy before playtest feedback exists.
