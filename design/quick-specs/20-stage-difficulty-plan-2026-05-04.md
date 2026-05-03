# Quick Design Spec: 20 Stage Difficulty Plan

**Type**: Tuning / Small Addition
**System**: BoxStack stage progression
**Date**: 2026-05-04
**Context**: Casual Toss app-in-app prototype. Core stacking feel is validated; next step is turning the single prototype target into a 20-stage fixed-clear progression.

## Direction

Use fixed-count clear stages as the main mode. Infinite stacking is not the MVP default because 2D physics instability can make long runs feel unfair. An infinite/challenge mode can be tested later after the fixed-clear game is stable.

Main rule:

- Each stage defines a target number of parcel boxes.
- The player clears the stage by stacking all boxes without losing the stack.
- Difficulty rises through box count, box sequence, move speed multiplier, and move range multiplier.
- Avoid new obstacles or special boxes for first release; keep the first 20 stages focused on validating the core stacking loop.

## Box Codes

| Code | Meaning |
|---|---|
| `B` | Basic box |
| `W` | Wide box |
| `T` | Tall box |

Current prototype baseline:

| Parameter | Baseline |
|---|---:|
| `MoveSpeed` | `1.85` |
| `MoveRange` | `2.45` |

Stage multipliers apply to those baseline values.

## Stage Plan

| Stage | Target | Box Sequence | Speed | Range | Intent |
|---:|---:|---|---:|---:|---|
| 1 | 4 | `B B B B` | 0.75x | 0.75x | First clear experience |
| 2 | 5 | `B B B B B` | 0.80x | 0.80x | Learn base timing |
| 3 | 6 | `B B W B B B` | 0.85x | 0.85x | Introduce stable wide box |
| 4 | 6 | `B T B B W B` | 0.90x | 0.90x | Introduce tall box |
| 5 | 7 | `B B W B T B B` | 0.95x | 0.95x | Confirm base rules |
| 6 | 7 | `W B B T B B W` | 1.00x | 1.00x | Prototype baseline checkpoint |
| 7 | 8 | `B B T B W B B B` | 1.00x | 1.00x | First 8-box goal |
| 8 | 8 | `W B T B B W B T` | 1.05x | 1.00x | Box order variation |
| 9 | 8 | `B T T B W B B W` | 1.05x | 1.05x | More tall-box risk |
| 10 | 9 | `B B W T B B W B B` | 1.10x | 1.05x | Midpoint gate |
| 11 | 9 | `W T B B T B W B B` | 1.10x | 1.10x | Stable/unstable rhythm |
| 12 | 9 | `B W B T W B T B B` | 1.15x | 1.10x | Start timing pressure |
| 13 | 10 | `B B T W B T B W B B` | 1.15x | 1.15x | First 10-box goal |
| 14 | 10 | `W B T B T W B B T B` | 1.20x | 1.15x | Tall-box accumulation |
| 15 | 10 | `B T W B B T W T B B` | 1.20x | 1.20x | Late-game entry |
| 16 | 11 | `W B B T W B T B W B B` | 1.25x | 1.20x | Longer stage adaptation |
| 17 | 11 | `B T B W T B B W T B B` | 1.25x | 1.25x | Wobble management |
| 18 | 12 | `B W T B B W T B T W B B` | 1.30x | 1.25x | Pre-final gate |
| 19 | 12 | `W T B T W B B T W B T B` | 1.35x | 1.30x | Hard delivery |
| 20 | 12 | `B T W T B W T B W T B B` | 1.40x | 1.30x | Launch final stage |

## Star Rating Draft

| Stars | Rule |
|---|---|
| 1 | Clear the stage target |
| 2 | Clear with no restart |
| 3 | Clear with no restart and either under time target or with low wobble |

Prototype note: implement clear/fail first. Add stars only after stage progression feels stable.

## Implementation Order

1. Add a lightweight `StageConfig` structure to `BoxStackPrototype.cs`.
2. Store the 20-stage table in code first for prototype speed.
3. Replace `TargetBoxes`, `MoveSpeed`, and `MoveRange` usage with values from the current stage config.
4. Make `SpawnNextBox()` follow the configured box sequence instead of round-robin visual selection.
5. Add simple stage navigation for playtest: previous/next stage keys or temporary buttons.
6. Show current stage in the Toss Minimal HUD.
7. Playtest only stages 1, 5, 10, 15, and 20 first.
8. Tune multipliers and box sequences based on those checkpoints.
9. After the curve feels good, playtest the full 1-20 run.
10. Move stage data out of hardcoded prototype code only if the game moves beyond prototype validation.

## Acceptance Criteria For First Implementation

- [ ] Stage 1 through 20 are selectable during Editor Play.
- [ ] Each stage uses its own target count, box sequence, speed multiplier, and range multiplier.
- [ ] Stage clear occurs when the configured target count is reached.
- [ ] The HUD shows current stage and box progress.
- [ ] Stages 1, 5, 10, 15, and 20 can be tested without code edits.
- [ ] Existing restart, fail, camera follow, background, and parcel alpha-bounds behavior still work.

## Open Questions

- Should stage navigation be visible in the prototype HUD, or only keyboard-only for internal testing?
- Should 3-star scoring use time, wobble, or landing accuracy?
- Should failing a stage reset only that stage, or return to a stage select screen later?
- How many full 1-20 clears should be tested before considering the curve stable?

## Risks

- Physics instability may make later 12-box stages feel random if tall boxes stack too often.
- Speed/range multipliers may need portrait-device retuning because Editor Game view aspect ratio changes feel.
- Hardcoded stage data is acceptable for prototype speed, but should move to data assets before production.
