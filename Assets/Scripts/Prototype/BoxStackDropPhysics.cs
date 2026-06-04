using System.Collections.Generic;
using UnityEngine;

internal sealed class BoxStackDropPhysics
{
    private bool _preContactVelocityResetUsed;

    internal void BeginDrop(GameObject box, BoxStackPrototypeConfig.TuningSettings tuning)
    {
        if (box == null)
        {
            return;
        }

        var body = box.GetComponent<Rigidbody2D>();
        if (body == null)
        {
            return;
        }

        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = tuning.DroppingGravityScale;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
        _preContactVelocityResetUsed = false;
    }

    internal void ResetVelocityBeforeStackContact(
        GameObject droppingBox,
        IReadOnlyList<GameObject> placedBoxes,
        BoxStackPrototypeConfig.TuningSettings tuning)
    {
        if (_preContactVelocityResetUsed || droppingBox == null || placedBoxes.Count == 0)
        {
            return;
        }

        float resetDistance = tuning.DropPreContactVelocityResetDistance;
        if (resetDistance <= 0f)
        {
            return;
        }

        var body = droppingBox.GetComponent<Rigidbody2D>();
        if (body == null || body.linearVelocity.y >= 0f)
        {
            return;
        }

        if (!droppingBox.TryGetComponent(out BoxCollider2D droppingCollider))
        {
            return;
        }

        Bounds droppingBounds = droppingCollider.bounds;
        for (int i = 0; i < placedBoxes.Count; i++)
        {
            GameObject placedBox = placedBoxes[i];
            if (placedBox == null || !placedBox.TryGetComponent(out BoxCollider2D placedCollider))
            {
                continue;
            }

            Bounds placedBounds = placedCollider.bounds;
            bool horizontallyOverlaps = droppingBounds.min.x < placedBounds.max.x
                && droppingBounds.max.x > placedBounds.min.x;
            if (!horizontallyOverlaps)
            {
                continue;
            }

            float verticalGap = droppingBounds.min.y - placedBounds.max.y;
            if (verticalGap >= 0f && verticalGap <= resetDistance)
            {
                Vector2 velocity = body.linearVelocity;
                body.linearVelocity = new Vector2(velocity.x, 0f);
                _preContactVelocityResetUsed = true;
                return;
            }
        }
    }

    internal void ClampFallSpeed(GameObject droppingBox, BoxStackPrototypeConfig.TuningSettings tuning)
    {
        if (droppingBox == null)
        {
            return;
        }

        var body = droppingBox.GetComponent<Rigidbody2D>();
        if (body == null)
        {
            return;
        }

        Vector2 velocity = body.linearVelocity;
        float maxDroppingFallSpeed = tuning.MaxDroppingFallSpeed;
        if (velocity.y < -maxDroppingFallSpeed)
        {
            body.linearVelocity = new Vector2(velocity.x, -maxDroppingFallSpeed);
        }
    }

    internal bool StackMotionIsStable(
        GameObject droppedBox,
        IReadOnlyList<GameObject> placedBoxes,
        BoxStackPrototypeConfig.TuningSettings tuning)
    {
        if (!BoxMotionIsStable(droppedBox, tuning))
        {
            return false;
        }

        for (int i = 0; i < placedBoxes.Count; i++)
        {
            if (!BoxMotionIsStable(placedBoxes[i], tuning))
            {
                return false;
            }
        }

        return true;
    }

    internal void FreezePlacedBoxPhysics(IReadOnlyList<GameObject> placedBoxes)
    {
        for (int i = 0; i < placedBoxes.Count; i++)
        {
            GameObject box = placedBoxes[i];
            if (box == null)
            {
                continue;
            }

            var body = box.GetComponent<Rigidbody2D>();
            if (body == null)
            {
                continue;
            }

            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = false;
        }
    }

    private static bool BoxMotionIsStable(GameObject box, BoxStackPrototypeConfig.TuningSettings tuning)
    {
        if (box == null)
        {
            return false;
        }

        var body = box.GetComponent<Rigidbody2D>();
        if (body == null || body.IsSleeping())
        {
            return true;
        }

        float linearThreshold = tuning.StackStopLinearVelocity;
        return body.linearVelocity.sqrMagnitude <= linearThreshold * linearThreshold
            && Mathf.Abs(body.angularVelocity) <= tuning.StackStopAngularVelocity;
    }
}
