# B012 Screen-Clamped Speed Playtest

## Session Info

- Date: 2026-05-09
- Build marker: B012
- Baseline commit: `ceba02c 낙하 충격 완화로 박스 밀림 조정`
- Platform: Mobile WebGL / AIT Dev Server
- Input: Touch
- Session type: Targeted representative-stage test

## Test Focus

Confirm that centered constant-speed pre-drop movement stays inside the visible screen across harder stages while preserving the intended later-stage speed feel and the approved B008 physics baseline.

Baseline values:

- Parcel friction: `8.0`
- Floor friction: `8.0`
- Settled gravity scale: `1.6`
- Dropping gravity scale: `1.1`
- Falling speed cap: `4.5`
- Free rescues per stage: `1`
- Clear validation: `5.0s`
- Single-column tolerance: `0.75`
- Pre-drop horizontal movement: Starts at screen center, moves right first, keeps constant speed between left and right endpoints, clamps movement range to the visible camera width, and compensates speed against the unclamped stage range so later stages do not feel slower after clamping

## Stage Checklist

Use this table while testing stages `1`, `5`, `10`, `15`, and `20`.

| Stage | Result | Difficulty | Movement stays visible | Movement feel | Drop impact | Lower-box slide | Failure feel | Rescue value | Replay desire | Notes |
|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Pass / Fail / Retry | Too Easy / Just Right / Too Hard | Yes / No / Edge clipped | Better / Worse / Same | Weak / Just Right / Strong | None / Mild / Severe | Fair / Harsh / Unclear | Not needed / Helpful / Too forgiving | Yes / Maybe / No |  |
| 5 | Pass / Fail / Retry | Too Easy / Just Right / Too Hard | Yes / No / Edge clipped | Better / Worse / Same | Weak / Just Right / Strong | None / Mild / Severe | Fair / Harsh / Unclear | Not needed / Helpful / Too forgiving | Yes / Maybe / No |  |
| 10 | Pass / Fail / Retry | Too Easy / Just Right / Too Hard | Yes / No / Edge clipped | Better / Worse / Same | Weak / Just Right / Strong | None / Mild / Severe | Fair / Harsh / Unclear | Not needed / Helpful / Too forgiving | Yes / Maybe / No |  |
| 15 | Pass / Fail / Retry | Too Easy / Just Right / Too Hard | Yes / No / Edge clipped | Better / Worse / Same | Weak / Just Right / Strong | None / Mild / Severe | Fair / Harsh / Unclear | Not needed / Helpful / Too forgiving | Yes / Maybe / No |  |
| 20 | Pass / Fail / Retry | Too Easy / Just Right / Too Hard | Yes / No / Edge clipped | Better / Worse / Same | Weak / Just Right / Strong | None / Mild / Severe | Fair / Harsh / Unclear | Not needed / Helpful / Too forgiving | Yes / Maybe / No |  |

## Questions To Answer

- Does starting from screen center feel better than starting from the left endpoint?
- Do all box shapes stay fully visible inside the screen on stages 10, 15, and 20?
- Does stage 20 recover the intended faster movement feel after the B012 speed compensation?
- Does constant-speed endpoint reversal feel more predictable than the old eased sine motion?
- Does B012 still feel weighty enough in early stages?
- Do later stages fail because of player timing, or because physics feels random?
- Does lower-box sliding stay mild enough that failures feel understandable?
- Does immediate collapse detection feel fair, or does it punish harmless wobble?
- Does one free rescue make hard stages more playable without removing the stakes?
- Is the 5-second validation window readable and satisfying after the final drop?

## Findings

### What Worked

- 

### Pain Points

- 

### Bugs Or Oddities

| # | Description | Severity | Reproducible |
|---|---|---|---|
|  |  |  |  |

## Verdict

- Movement change: Keep B012 / Tune range padding / Tune speed / Revert to sine
- Physics baseline: Keep / Tune / Revert
- Difficulty curve: Keep / Tune stages only / Redesign
- Rescue flow: Keep one free rescue / Tune count / Remove for now
- Failure detection: Keep / Loosen / Investigate

## Next Adjustment Candidates

1. 
2. 
3. 
