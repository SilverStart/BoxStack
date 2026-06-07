using System;

internal enum BoxStackStageFlowAction
{
    None,
    RefreshUi,
    RestartRun,
    ApplyPaletteAndRestartRun
}

internal readonly struct BoxStackStageFlowResult
{
    internal BoxStackStageFlowResult(BoxStackStageFlowAction action)
    {
        Action = action;
    }

    private BoxStackStageFlowAction Action { get; }

    internal bool HasAction
    {
        get { return Action != BoxStackStageFlowAction.None; }
    }

    internal bool ShouldApplyPalette
    {
        get { return Action == BoxStackStageFlowAction.ApplyPaletteAndRestartRun; }
    }

    internal bool ShouldRestartRun
    {
        get
        {
            return Action == BoxStackStageFlowAction.RestartRun
                || Action == BoxStackStageFlowAction.ApplyPaletteAndRestartRun;
        }
    }

    internal bool ShouldRefreshUi
    {
        get { return Action == BoxStackStageFlowAction.RefreshUi; }
    }
}

internal sealed class BoxStackStageFlow
{
    internal BoxStackStageFlowResult Move(BoxStackStageProgress stageProgress, int direction)
    {
        if (!stageProgress.TryMove(direction))
        {
            return None();
        }

        return ApplyPaletteAndRestartRun();
    }

    internal BoxStackStageFlowResult Select(BoxStackStageProgress stageProgress, int stageIndex, int stageCount)
    {
        if (!stageProgress.TrySelect(stageIndex, stageCount, out bool stageChanged))
        {
            return None();
        }

        return stageChanged
            ? ApplyPaletteAndRestartRun()
            : RestartRun();
    }

    internal BoxStackStageFlowResult Reset(BoxStackStageProgress stageProgress, Func<int, int> getStageNumber)
    {
        stageProgress.Reset(getStageNumber);
        return ApplyPaletteAndRestartRun();
    }

    internal BoxStackStageFlowResult UnlockAll(
        BoxStackStageProgress stageProgress,
        int stageCount,
        Func<int, int> getStageNumber)
    {
        stageProgress.UnlockAll(stageCount, getStageNumber);
        return RefreshUi();
    }

    internal BoxStackStageFlowResult UnlockNext(
        BoxStackStageProgress stageProgress,
        int stageCount,
        Func<int, int> getStageNumber)
    {
        stageProgress.UnlockNextStage(stageCount, getStageNumber);
        return None();
    }

    private static BoxStackStageFlowResult None()
    {
        return new BoxStackStageFlowResult(BoxStackStageFlowAction.None);
    }

    private static BoxStackStageFlowResult RefreshUi()
    {
        return new BoxStackStageFlowResult(BoxStackStageFlowAction.RefreshUi);
    }

    private static BoxStackStageFlowResult RestartRun()
    {
        return new BoxStackStageFlowResult(BoxStackStageFlowAction.RestartRun);
    }

    private static BoxStackStageFlowResult ApplyPaletteAndRestartRun()
    {
        return new BoxStackStageFlowResult(BoxStackStageFlowAction.ApplyPaletteAndRestartRun);
    }
}
