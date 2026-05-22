# B085 Placed-Box Lateral Stabilization Check

## Session Info

- Date: 2026-05-21
- Runtime marker: B085 experiment, reverted to B084 after review
- Platform: Phone WebGL
- Input: mouse/touch emulation or real touch
- Session type: physics feel check

## Purpose

B085 attempted to reduce the feeling that each newly dropped box easily pushes the existing tower sideways.

The implementation only affects already placed boxes: their x-axis velocity is clamped and damped after impacts. The falling box, vertical motion, angular velocity, B084 floor-contact failure rule, 5-second clear validation, UI, stage data, and visuals are unchanged.

## Current Tuning

- `PlacedBoxMaxLateralVelocity`: `0.15`
- `PlacedBoxLateralDamping`: `8`
- `DroppingGravityScale`: `1.1`
- `MaxDroppingFallSpeed`: `4.5`
- `LinearDamping`: `0.6`
- `AngularDamping`: `0.8`

## Quick Setup Used

1. Open `SampleScene` in Unity Editor.
2. Start Play Mode.
3. Confirm the small build marker shows `B085`.
4. Use stage select and, if needed, `전체 해금`.
5. Play stages 1, 10, 18, and 20 at least twice.

## Evaluation Points

- Existing boxes no longer slide sideways too easily after a new box lands.
- The tower can still rotate, wobble, and collapse when placement is poor.
- B085 does not feel like invisible X-axis locking.
- B084 failure still occurs when two or more boxes touch the floor.
- The player still understands why a failure happened.
- The 5-second clear validation still feels fair.
- Stage 18-20 difficulty still feels skill-based rather than random.

## Stage Notes

| Stage | Target Boxes | Result | Sideways shove reduced | Still wobbles/collapses | Failure readability | Clear feel | Memo |
|---|---:|---|---|---|---|---|---|
| 1 | 4 | Not accepted | Yes | Reduced too much | Acceptable | Not accepted | Felt unnatural because placed boxes shed sideways motion too aggressively. |
| 10 | 9 | Not accepted | Yes | Reduced too much | Acceptable | Not accepted | Lateral stabilization read as artificial rather than physical. |
| 18 | 12 | Not accepted | Yes | Reduced too much | Acceptable | Not accepted | Late-stack behavior did not feel natural enough to keep. |
| 20 | 12 | Not accepted | Yes | Reduced too much | Acceptable | Not accepted | Same issue as stage 18; do not keep this tuning direction. |

## Verdict

Rejected. User-led mobile testing found the placed-box x-axis velocity damping/clamp unnatural. Revert B085 runtime code and return the active baseline to B084.

## Decision Rule

- Accept: sideways shove is noticeably reduced, but the game still has wobble, collapse, and readable failure.
- Hold: improvement is visible, but one or two stages need more repetitions before accepting.
- Revise: tower feels locked, failure becomes unclear, or late-stage difficulty becomes too easy/too hard.
