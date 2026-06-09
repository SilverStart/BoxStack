internal readonly struct BoxStackRunEndResult
{
    internal BoxStackRunEndResult(
        bool shouldEnd,
        BoxStackPrototypeState state,
        string statusText,
        bool lastClearWasNewBest,
        bool shouldUnlockNextStage)
    {
        ShouldEnd = shouldEnd;
        State = state;
        StatusText = statusText;
        LastClearWasNewBest = lastClearWasNewBest;
        ShouldUnlockNextStage = shouldUnlockNextStage;
    }

    internal bool ShouldEnd { get; }
    internal BoxStackPrototypeState State { get; }
    internal string StatusText { get; }
    internal bool LastClearWasNewBest { get; }
    internal bool ShouldUnlockNextStage { get; }
}

internal sealed class BoxStackRunEndFlow
{
    internal BoxStackRunEndResult GetEndResult(
        BoxStackPrototypeState currentState,
        bool won,
        string statusText,
        int currentStageIndex,
        int highestUnlockedStageIndex)
    {
        if (currentState == BoxStackPrototypeState.Won || currentState == BoxStackPrototypeState.Failed)
        {
            return new BoxStackRunEndResult(false, currentState, statusText, false, false);
        }

        return new BoxStackRunEndResult(
            true,
            won ? BoxStackPrototypeState.Won : BoxStackPrototypeState.Failed,
            statusText,
            won && currentStageIndex >= highestUnlockedStageIndex,
            won);
    }
}
