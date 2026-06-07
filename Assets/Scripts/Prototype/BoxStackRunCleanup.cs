using System.Collections.Generic;
using UnityEngine;

internal sealed class BoxStackRunCleanup
{
    internal void ClearRunObjects(
        ref GameObject activeBox,
        ref GameObject droppingBox,
        List<GameObject> placedBoxes)
    {
        DestroyIfPresent(activeBox);
        DestroyIfPresent(droppingBox);

        foreach (GameObject box in placedBoxes)
        {
            DestroyIfPresent(box);
        }

        placedBoxes.Clear();
        activeBox = null;
        droppingBox = null;
    }

    private static void DestroyIfPresent(GameObject gameObject)
    {
        if (gameObject != null)
        {
            Object.Destroy(gameObject);
        }
    }
}
