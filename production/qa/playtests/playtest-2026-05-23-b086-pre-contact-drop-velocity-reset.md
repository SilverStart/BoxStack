# B086 Pre-Contact Drop Velocity Reset Check

## Session Info

- Date: 2026-05-24
- Runtime marker: B086
- Platform: User-led B086 gameplay check
- Input: mouse/touch emulation or real touch
- Session type: physics feel check

## Purpose

B086 keeps the existing falling speed cap but reduces the final impact when the falling box is just above an already placed box.

The implementation resets only the falling box's y velocity to `0` once before box-to-box contact. It does not clamp placed boxes, does not change placed-box x velocity, and does not use Rigidbody constraints.

## Current Tuning

- `DroppingGravityScale`: `1.1`
- `MaxDroppingFallSpeed`: `4.5`
- `DropPreContactVelocityResetDistance`: `0.12`
- `LinearDamping`: `0.6`
- `AngularDamping`: `0.8`
- `ParcelFriction`: `8`
- `FloorFriction`: `8`

## Quick Setup

1. Open `SampleScene` in Unity Editor.
2. Start Play Mode.
3. Confirm the small build marker shows `B086`.
4. Use stage select and, if needed, `전체 해금`.
5. Play stages 1, 10, 18, and 20 at least twice.

## Evaluation Points

- Existing boxes are pushed sideways less when a new box lands on them.
- The drop still feels quick enough; it should not look like the box pauses in midair.
- The tower can still wobble, rotate, and collapse when placement is poor.
- B084 failure still occurs when two or more boxes touch the floor.
- The player still understands why a failure happened.
- The 5-second clear validation still feels fair.
- Stage 18-20 difficulty still feels skill-based rather than random.

## Stage Notes

| Stage | Target Boxes | Result | Sideways shove reduced | Drop still natural | Still wobbles/collapses | Failure readability | Clear feel | Memo |
|---|---:|---|---|---|---|---|---|---|
| 1 | 4 | Accepted overall | Yes | Yes | Yes | Yes | Yes | User confirmed B086 feels much better; no separate stage memo. |
| 10 | 9 | Accepted overall | Yes | Yes | Yes | Yes | Yes | User confirmed B086 feels much better; no separate stage memo. |
| 18 | 12 | Accepted overall | Yes | Yes | Yes | Yes | Yes | User confirmed B086 feels much better; no separate stage memo. |
| 20 | 12 | Accepted overall | Yes | Yes | Yes | Yes | Yes | User confirmed B086 feels much better; no separate stage memo. |

## Verdict

Accepted. User-led check found B086 much better than the rejected B085 approach. Keep B086 as the current physics feel baseline unless later mobile/WebGL testing reveals a regression.

## Decision Rule

- Accept: sideways shove is reduced, but the falling box still feels natural and the tower can still wobble/collapse.
- Hold: improvement is visible, but one or two stages need more repetitions before accepting.
- Revise: the falling box appears to pause, late-stage difficulty becomes too easy, or failure readability gets worse.
