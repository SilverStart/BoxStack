internal sealed class BoxStackStageSelectSession
{
    private BoxStackPrototypeState _stateBeforeStageSelect;
    private float _timeScaleBeforeStageSelect = 1f;

    internal void Begin(BoxStackPrototypeState currentState, float currentTimeScale)
    {
        _stateBeforeStageSelect = currentState;
        _timeScaleBeforeStageSelect = currentTimeScale;
    }

    internal BoxStackPrototypeState End(out float restoredTimeScale)
    {
        restoredTimeScale = _timeScaleBeforeStageSelect <= 0f ? 1f : _timeScaleBeforeStageSelect;
        return _stateBeforeStageSelect;
    }
}
