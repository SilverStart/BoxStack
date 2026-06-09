internal sealed class BoxStackDropStabilityWait
{
    private float _resolveStartedAt;
    private float _stableStartedAt;

    internal void Begin(float time)
    {
        _resolveStartedAt = time;
        _stableStartedAt = -1f;
    }

    internal bool MinimumResolveTimeElapsed(float time, BoxStackPrototypeConfig.TuningSettings tuning)
    {
        return time - _resolveStartedAt >= tuning.DropMinimumResolveSeconds;
    }

    internal bool MarkStableAndIsComplete(float time, BoxStackPrototypeConfig.TuningSettings tuning)
    {
        if (_stableStartedAt < 0f)
        {
            _stableStartedAt = time;
        }

        return time - _stableStartedAt >= tuning.DropStableSeconds;
    }

    internal void ResetStable()
    {
        _stableStartedAt = -1f;
    }
}
