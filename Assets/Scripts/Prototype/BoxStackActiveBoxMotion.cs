using UnityEngine;

internal sealed class BoxStackActiveBoxMotion
{
    internal void Move(
        GameObject activeBox,
        Camera camera,
        float spawnHeight,
        float moveStartedAt,
        float currentTime,
        BoxStackPrototypeConfig.StageSettings stage,
        BoxStackPrototypeConfig.TuningSettings tuning,
        float fallbackBoxSize)
    {
        if (activeBox == null)
        {
            return;
        }

        float elapsed = currentTime - moveStartedAt;
        float stageMoveRange = tuning.BaseMoveRange * stage.RangeMultiplier;
        float moveRange = Mathf.Min(
            stageMoveRange,
            GetScreenSafeMoveRange(activeBox, camera, tuning, fallbackBoxSize));
        if (moveRange <= 0f)
        {
            activeBox.transform.position = new Vector3(0f, spawnHeight, 0f);
            return;
        }

        float moveSpeed = tuning.BaseMoveSpeed * stage.SpeedMultiplier;
        float rangeSpeedCompensation = stageMoveRange / moveRange;
        float cyclePosition = Mathf.Repeat(
            (elapsed * moveSpeed * rangeSpeedCompensation / (2f * Mathf.PI)) + 0.25f,
            1f);
        float normalizedX = cyclePosition < 0.5f
            ? -1f + (cyclePosition * 4f)
            : 3f - (cyclePosition * 4f);
        float x = normalizedX * moveRange;
        activeBox.transform.position = new Vector3(x, spawnHeight, 0f);
    }

    private static float GetScreenSafeMoveRange(
        GameObject activeBox,
        Camera camera,
        BoxStackPrototypeConfig.TuningSettings tuning,
        float fallbackBoxSize)
    {
        if (camera == null)
        {
            return tuning.BaseMoveRange;
        }

        float cameraHalfWidth = camera.orthographicSize * camera.aspect;
        return Mathf.Max(
            0f,
            cameraHalfWidth - GetActiveBoxHalfWidth(activeBox, fallbackBoxSize) - tuning.MoveRangeScreenPadding);
    }

    private static float GetActiveBoxHalfWidth(GameObject activeBox, float fallbackBoxSize)
    {
        if (activeBox != null && activeBox.TryGetComponent(out BoxCollider2D collider))
        {
            return collider.size.x * Mathf.Abs(activeBox.transform.lossyScale.x) * 0.5f;
        }

        return fallbackBoxSize * 0.5f;
    }
}
