using System.Collections.Generic;
using UnityEngine;

internal sealed class BoxStackDroppedBoxSettlement
{
    internal bool Settle(
        GameObject droppedBox,
        Color placedTint,
        BoxStackBoxFactory boxFactory,
        List<GameObject> placedBoxes,
        BoxStackPrototypeConfig.TuningSettings tuning,
        out Color appliedPlacedTint)
    {
        appliedPlacedTint = placedTint;

        bool placedTintApplied = boxFactory != null && boxFactory.TryApplyPlacedTint(droppedBox, placedTint);

        if (droppedBox != null && droppedBox.TryGetComponent(out Rigidbody2D body))
        {
            body.gravityScale = tuning.SettledGravityScale;
        }

        if (droppedBox != null && !placedBoxes.Contains(droppedBox))
        {
            placedBoxes.Add(droppedBox);
        }

        return placedTintApplied;
    }
}
