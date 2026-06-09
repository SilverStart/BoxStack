// 프로토타입 UI 상태 팩토리 - 게임 로직에서 UI 문구와 버튼 상태 조립을 분리한다.

using System;
internal sealed class BoxStackPrototypeUiStateFactory
{
    private readonly BoxStackResultPresentation _resultPresentation = new BoxStackResultPresentation();
    private readonly BoxStackStagePresentation _stagePresentation = new BoxStackStagePresentation();

    internal BoxStackPrototypeUi.UiState Create(UiContext context)
    {
        bool won = context.State == BoxStackPrototypeState.Won;
        bool resultVisible = IsResultState(context.State);
        bool hasNextStage = won && HasNextStage(context.CurrentStageIndex, context.StageCount);
        BoxStackResultPresentationState result = resultVisible
            ? _resultPresentation.Create(won, hasNextStage, context.LastClearWasNewBest, context.StatusText)
            : new BoxStackResultPresentationState(string.Empty, string.Empty, string.Empty);
        BoxStackStagePresentationState stageState = _stagePresentation.Create(
            context.StageCount,
            context.GetStage,
            context.CurrentStageIndex,
            context.HighestUnlockedStageIndex);

        return new BoxStackPrototypeUi.UiState(
            context.BuildNumber,
            stageState.CurrentStageLabel,
            context.PlacedBoxes,
            context.TargetBoxes,
            context.State == BoxStackPrototypeState.StageSelect,
            resultVisible,
            result.Title,
            result.Body,
            result.ButtonLabel,
            context.ShowProgressControls,
            context.Palette,
            stageState.Stages);
    }

    private static bool HasNextStage(int currentStageIndex, int stageCount)
    {
        return currentStageIndex < stageCount - 1;
    }

    private static bool IsResultState(BoxStackPrototypeState state)
    {
        return state == BoxStackPrototypeState.Won || state == BoxStackPrototypeState.Failed;
    }

    internal readonly struct UiContext
    {
        internal UiContext(
            int buildNumber,
            int stageCount,
            Func<int, BoxStackPrototypeConfig.StageSettings> getStage,
            int currentStageIndex,
            int highestUnlockedStageIndex,
            int placedBoxes,
            int targetBoxes,
            BoxStackPrototypeState state,
            string statusText,
            bool lastClearWasNewBest,
            bool showProgressControls,
            BoxStackPrototypePalette palette)
        {
            BuildNumber = buildNumber;
            StageCount = stageCount;
            GetStage = getStage;
            CurrentStageIndex = currentStageIndex;
            HighestUnlockedStageIndex = highestUnlockedStageIndex;
            PlacedBoxes = placedBoxes;
            TargetBoxes = targetBoxes;
            State = state;
            StatusText = statusText;
            LastClearWasNewBest = lastClearWasNewBest;
            ShowProgressControls = showProgressControls;
            Palette = palette;
        }

        internal int BuildNumber { get; }
        internal int StageCount { get; }
        internal Func<int, BoxStackPrototypeConfig.StageSettings> GetStage { get; }
        internal int CurrentStageIndex { get; }
        internal int HighestUnlockedStageIndex { get; }
        internal int PlacedBoxes { get; }
        internal int TargetBoxes { get; }
        internal BoxStackPrototypeState State { get; }
        internal string StatusText { get; }
        internal bool LastClearWasNewBest { get; }
        internal bool ShowProgressControls { get; }
        internal BoxStackPrototypePalette Palette { get; }
    }
}
