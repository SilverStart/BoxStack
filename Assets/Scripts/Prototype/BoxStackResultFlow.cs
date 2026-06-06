internal enum BoxStackResultAction
{
    None,
    AdvanceToNextStage,
    RestartFromFirstStage,
    RestartCurrentStage
}

internal sealed class BoxStackResultFlow
{
    internal BoxStackResultAction GetResultButtonAction(BoxStackPrototypeState state, bool hasNextStage)
    {
        if (state == BoxStackPrototypeState.Won)
        {
            return hasNextStage
                ? BoxStackResultAction.AdvanceToNextStage
                : BoxStackResultAction.RestartFromFirstStage;
        }

        if (state == BoxStackPrototypeState.Failed)
        {
            return BoxStackResultAction.RestartCurrentStage;
        }

        return BoxStackResultAction.None;
    }
}
