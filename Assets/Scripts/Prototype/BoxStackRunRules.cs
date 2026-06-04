// Prototype run rules - keeps accepted clear/failure checks outside the main prototype controller.

using System.Collections.Generic;
using UnityEngine;

internal sealed class BoxStackRunRules
{
    internal const string MissedStatus = "MISSED";
    internal const string StackLostStatus = "STACK LOST";
    internal const string StackSpreadStatus = "STACK SPREAD";

    internal bool TryEvaluateDroppedBoxFailure(
        GameObject droppedBox,
        IReadOnlyList<GameObject> placedBoxes,
        Collider2D floorCollider,
        BoxStackPrototypeConfig.TuningSettings tuning,
        out string status)
    {
        if (BoxIsLost(droppedBox, tuning))
        {
            status = MissedStatus;
            return true;
        }

        return TryEvaluateStackFailure(placedBoxes, droppedBox, floorCollider, tuning, out status);
    }

    internal bool TryEvaluateStackFailure(
        IReadOnlyList<GameObject> placedBoxes,
        GameObject extraBox,
        Collider2D floorCollider,
        BoxStackPrototypeConfig.TuningSettings tuning,
        out string status)
    {
        if (AnyBoxLost(placedBoxes, tuning))
        {
            status = StackLostStatus;
            return true;
        }

        if (StackHasMultipleFloorContacts(placedBoxes, extraBox, floorCollider))
        {
            status = StackSpreadStatus;
            return true;
        }

        status = string.Empty;
        return false;
    }

    internal bool AnyBoxLost(IReadOnlyList<GameObject> placedBoxes, BoxStackPrototypeConfig.TuningSettings tuning)
    {
        for (int i = 0; i < placedBoxes.Count; i++)
        {
            if (BoxIsLost(placedBoxes[i], tuning))
            {
                return true;
            }
        }

        return false;
    }

    internal bool StackHasMultipleFloorContacts(
        IReadOnlyList<GameObject> placedBoxes,
        GameObject extraBox,
        Collider2D floorCollider)
    {
        if (floorCollider == null)
        {
            return false;
        }

        int floorContactCount = 0;
        for (int i = 0; i < placedBoxes.Count; i++)
        {
            if (BoxIsTouchingFloor(placedBoxes[i], floorCollider))
            {
                floorContactCount++;
                if (floorContactCount >= 2)
                {
                    return true;
                }
            }
        }

        if (BoxIsTouchingFloor(extraBox, floorCollider))
        {
            floorContactCount++;
        }

        return floorContactCount >= 2;
    }

    internal bool BoxIsLost(GameObject box, BoxStackPrototypeConfig.TuningSettings tuning)
    {
        if (box == null)
        {
            return true;
        }

        Vector3 position = box.transform.position;
        return position.y < tuning.LostHeight || Mathf.Abs(position.x) > tuning.LostHorizontalDistance;
    }

    private static bool BoxIsTouchingFloor(GameObject box, Collider2D floorCollider)
    {
        if (box == null || floorCollider == null)
        {
            return false;
        }

        var collider = box.GetComponent<Collider2D>();
        return collider != null && collider.IsTouching(floorCollider);
    }
}
