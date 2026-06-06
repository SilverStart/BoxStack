internal sealed class BoxStackClearValidation
{
    private float _endTime;
    private bool _active;

    internal void Begin(float currentTime, float duration)
    {
        _endTime = currentTime + duration;
        _active = true;
    }

    internal void Reset()
    {
        _endTime = 0f;
        _active = false;
    }

    internal bool IsComplete(float currentTime)
    {
        return _active && currentTime >= _endTime;
    }
}
