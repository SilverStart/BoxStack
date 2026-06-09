using System;

internal readonly struct BoxStackStagePresentationState
{
    internal BoxStackStagePresentationState(string currentStageLabel, BoxStackPrototypeUi.StageButtonState[] stages)
    {
        CurrentStageLabel = currentStageLabel;
        Stages = stages;
    }

    internal string CurrentStageLabel { get; }
    internal BoxStackPrototypeUi.StageButtonState[] Stages { get; }
}

internal sealed class BoxStackStagePresentation
{
    internal BoxStackStagePresentationState Create(
        int stageCount,
        Func<int, BoxStackPrototypeConfig.StageSettings> getStage,
        int currentStageIndex,
        int highestUnlockedStageIndex)
    {
        var stages = new BoxStackPrototypeUi.StageButtonState[stageCount];

        for (int i = 0; i < stageCount; i++)
        {
            BoxStackPrototypeConfig.StageSettings stage = getStage(i);
            stages[i] = new BoxStackPrototypeUi.StageButtonState(
                stage.Number,
                i <= highestUnlockedStageIndex,
                i == currentStageIndex);
        }

        BoxStackPrototypeConfig.StageSettings currentStage = getStage(currentStageIndex);
        return new BoxStackStagePresentationState($"{currentStage.Number:00}", stages);
    }
}
