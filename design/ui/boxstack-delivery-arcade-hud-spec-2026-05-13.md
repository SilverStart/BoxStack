# BoxStack Delivery Arcade HUD Spec

Last updated: 2026-05-13
Status: Approved direction, ready for implementation planning
Related mockup: `design/ui/mockups/boxstack-ui-directions-2026-05-13.png`
Source research: `design/ui/boxstack-game-ui-research-2026-05-13.md`

## Direction

Use **Delivery Arcade** as the next HUD direction.

The goal is to keep the Toss app-in-app experience clean and lightweight, while making the screen read as a casual game within the first three seconds. The UI should feel like a playful parcel delivery/stacking interface, not like a normal app header.

## Design Intent

- Make the player's progress feel like a delivery route being completed.
- Move away from one large white top app bar.
- Keep the central play lane readable for falling boxes and stack wobble.
- Add immediate feedback near the stack so good timing feels rewarding.
- Preserve the current prototype's practical information: stage, placed/target boxes, undo availability, result state, and stage select access.

## Visual Tone

- Clean 2D/2.5D casual game UI.
- Parcel, shipping label, ticket, stamp, and delivery route motifs.
- Teal and orange accents on warm cardboard/neutral UI surfaces.
- Rounded but game-like panels, not generic app cards.
- Soft shadow and mild outline for readability over backgrounds.
- Avoid heavy realism, dark warehouse styling, cluttered UI, or ad-game excess.

## Layout

### Top Left: Stage Label

Replace the current top-bar stage text with a compact shipping-label badge.

Content:

- Stage number
- Optional tiny route/label icon

Behavior:

- Tapping the badge opens the existing stage select overlay.
- Badge should remain within the safe area.
- Badge should not span the full width of the screen.

Suggested copy:

- `STAGE 5`
- Korean implementation option: `5단계`

### Top Right: Undo Ticket

Replace the text-heavy undo button with a small ticket-style action.

Content:

- Undo icon or curved-arrow symbol
- Remaining count, usually `1`

Behavior:

- Enabled when the current undo is available.
- Disabled state should still be visible but subdued.
- Should not look like a normal form button.

Suggested copy:

- `UNDO 1`
- Korean implementation option: `되돌리기 1`

### Side Rail: Delivery Progress

Add a vertical progress rail on the left or right edge of the safe play area.

Content:

- One node per required box, or grouped nodes when target count is high.
- Completed boxes use filled teal/orange nodes.
- Remaining boxes use pale nodes.
- Current target can be emphasized with a small parcel icon.

Behavior:

- Updates as boxes settle.
- Must not overlap the falling box lane.
- Must be far enough from the center that touch-to-drop still feels open.

Default placement:

- Right side for left-hand visibility of the stack.
- If it competes with the undo ticket or small-phone safe area, move to the left side.

### Center Feedback Toast

Add a short-lived landing feedback badge near the stack.

Feedback examples:

- `PERFECT`
- `NICE`
- `CAREFUL`
- `DELIVERED`

Korean implementation options:

- `완벽 적재`
- `좋아요`
- `조심!`
- `배송 완료`

Behavior:

- Appears after a box lands or when a clear/fail state begins.
- Should be brief and non-blocking.
- Should not cover the active falling box for longer than the feedback moment.
- For the first implementation, it can be state-driven with simple fade/scale later.

### Bottom Prompt

Keep the bottom action prompt, but make it feel less like placeholder UI.

Current:

- `TAP TO DROP`

Suggested direction:

- Use a small conveyor/platform-integrated prompt.
- Text can stay simple.
- The prompt should be secondary to the game object, not the largest UI element.

Korean implementation option:

- `탭해서 떨어뜨리기`

### Result Popup

Replace the plain modal feel with a delivery completion card.

Clear state:

- Stamped delivery card or receipt shape.
- Strong title: `배송 완료` or `CLEAR`
- Shows stage result and next action.

Fail state:

- Tilted/failed delivery card or caution stamp.
- Strong title: `적재 실패` or `FAILED`
- Shows restart action.

Behavior:

- Preserve current result actions.
- Do not reintroduce reward-ad rescue in this visual pass.

## Stage Select Overlay

Keep the current overlay behavior, but restyle it later as a route board or delivery manifest.

Priority for first pass:

- HUD and feedback first.
- Stage select restyle second, unless the current overlay strongly clashes after HUD changes.

## Implementation Notes

First implementation should be visual/structural only.

Do:

- Reuse the existing `BoxStackPrototypeUi` state inputs.
- Preserve safe-area handling.
- Preserve stage select, undo, result, and input blocking behavior.
- Keep text sizes mobile-readable.
- Keep the current gameplay and physics untouched.

Do not:

- Add scoring, stars, combo rules, or new gameplay rewards yet.
- Add reward-ad or rescue UI back into the fail popup.
- Change stage difficulty or progression rules.
- Introduce complex animation before the static layout reads correctly.

## Suggested Implementation Order

1. Replace the single full-width top HUD bar with separated stage and undo badges.
2. Add a side delivery progress rail using the existing progress state.
3. Add a central landing feedback element with placeholder state wiring.
4. Restyle result popup as a delivery completion/failure card.
5. Run Editor Play verification on stage select, undo, result popup, and touch blocking.
6. Only after static layout acceptance, add small feedback animation.

## Acceptance Criteria

- The first screen reads as a game, not a normal app header.
- Stage, box progress, and undo availability remain clear.
- The central play lane remains unobstructed.
- The side rail does not block touch-to-drop or overlap small-phone safe areas.
- Clear/fail result states still have obvious actions.
- Existing stage select and undo behavior continue to work.
- No gameplay tuning changes are bundled into the UI pass.

## Open Questions

- Should the delivery progress rail live on the left or right side for the target phone viewport?
- Should Korean or English HUD text be used for the next visual playtest?
- Should the landing feedback be based on actual placement quality now, or use simple generic feedback until scoring exists?
- Should the solid parcel-brown background remain during this UI pass, or should a simplified warehouse backdrop return after HUD readability is checked?
