internal enum BoxStackPostDropAction
{
    ContinuePlaying,
    BeginClearValidation
}

internal sealed class BoxStackPostDropFlow
{
    internal BoxStackPostDropAction GetNextAction(int placedBoxCount, int targetBoxCount)
    {
        return placedBoxCount >= targetBoxCount
            ? BoxStackPostDropAction.BeginClearValidation
            : BoxStackPostDropAction.ContinuePlaying;
    }
}
