# Delivery Arcade UI Asset Mini Pack

Last updated: 2026-05-13
Status: Prototype asset pass
Related spec: `design/ui/boxstack-delivery-arcade-hud-spec-2026-05-13.md`

## Purpose

This folder contains the first PNG mini pack for the Delivery Arcade HUD direction.

The implementation direction is hybrid:

- UI Toolkit handles layout, safe area, input blocking, text, and state changes.
- These PNGs provide the game-like visual skin: labels, tickets, rails, stamps, and result cards.

## Handoff

These assets are a sidecar design update from the UI polish discussion. They are not wired into runtime UI yet, and they should not block the current B021 stage progress storage checkpoint.

When the main implementation session returns to UI polish, review these PNGs first and decide whether to:

1. Apply them as-is through UI Toolkit `backgroundImage`.
2. Regenerate or refine the set before runtime integration.
3. Keep the current B016 vector-like Delivery Arcade UI and defer PNG skinning.

## Assets

| File | Intended Use |
|---|---|
| `ui_stage_label_badge.png` | Top-left stage/shipping-label badge background |
| `ui_undo_ticket.png` | Top-right undo ticket background/icon base |
| `ui_progress_rail.png` | Side delivery progress rail base |
| `ui_progress_node_empty.png` | Empty parcel progress node |
| `ui_progress_node_filled.png` | Completed parcel progress node |
| `ui_feedback_stamp_perfect.png` | Center landing feedback stamp background |
| `ui_result_card_clear.png` | Clear result popup card background |
| `ui_result_card_fail.png` | Fail result popup card background |

## Usage Notes

- Treat these as prototype direction assets, not final production art.
- Text should be rendered by UI Toolkit labels on top of the PNGs, not baked into the images.
- Keep gameplay rules unchanged when applying these assets.
- Keep the central play lane clear; do not let badges or rails cover the falling box path.
- Preserve current safe-area handling and UI input blocking.
- Import into Unity as Sprite or Texture2D depending on the UI Toolkit path chosen for the implementation pass.

## UI Toolkit Mapping

Suggested mapping:

- `ui_stage_label_badge.png` becomes the `backgroundImage` of the stage badge `Button`.
- `ui_undo_ticket.png` becomes the `backgroundImage` of the undo `Button`.
- `ui_progress_rail.png` becomes a side `VisualElement` background.
- `ui_progress_node_empty.png` and `ui_progress_node_filled.png` can be repeated as child `VisualElement` backgrounds.
- `ui_feedback_stamp_perfect.png` becomes the landing feedback toast background.
- `ui_result_card_clear.png` and `ui_result_card_fail.png` become result popup panel backgrounds.

## First Implementation Constraints

Do:

- Keep all current B015 UI behaviors intact.
- Add visual skinning before adding animation.
- Use existing `UiState` values where possible.
- Add only the smallest additional UI state needed for feedback text visibility.

Do not:

- Add scoring, combo rules, stars, or new progression systems in this pass.
- Reintroduce reward-ad rescue UI.
- Change physics, stage rules, unlock rules, or difficulty values.
- Commit to these assets as final art before a playtest.

## Review Questions

- Do these assets make the HUD feel more like a game without becoming too noisy?
- Should the delivery rail be placed left or right on the target phone viewport?
- Should the result cards keep this paper/shipping style or become more compact?
- Should the feedback stamp be text-only with this backing asset, or should future art include separate stamp words?
