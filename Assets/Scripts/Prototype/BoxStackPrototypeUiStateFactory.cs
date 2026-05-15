// 프로토타입 UI 상태 팩토리 - 게임 로직에서 UI 문구와 버튼 상태 조립을 분리한다.

using System;
internal sealed class BoxStackPrototypeUiStateFactory
{
    internal BoxStackPrototypeUi.UiState Create(UiContext context)
    {
        bool won = context.State == BoxStackPrototypeState.Won;
        bool resultVisible = IsResultState(context.State);
        bool hasNextStage = won && HasNextStage(context.CurrentStageIndex, context.StageCount);
        var stages = new BoxStackPrototypeUi.StageButtonState[context.StageCount];

        for (int i = 0; i < context.StageCount; i++)
        {
            BoxStackPrototypeConfig.StageSettings stage = context.GetStage(i);
            stages[i] = new BoxStackPrototypeUi.StageButtonState(
                stage.Number,
                stage.TargetBoxes,
                i <= context.HighestUnlockedStageIndex,
                i == context.CurrentStageIndex);
        }

        BoxStackPrototypeConfig.StageSettings currentStage = context.GetStage(context.CurrentStageIndex);
        return new BoxStackPrototypeUi.UiState(
            context.BuildNumber,
            $"{currentStage.Number:00}",
            context.PlacedBoxes,
            context.TargetBoxes,
            context.State == BoxStackPrototypeState.StageSelect,
            context.CanUseUndo,
            context.UndoCount,
            resultVisible,
            resultVisible ? GetResultTitle(won, hasNextStage) : string.Empty,
            resultVisible ? GetResultBody(won, hasNextStage, currentStage.Number, context.StatusText) : string.Empty,
            resultVisible ? GetResultButtonLabel(won, hasNextStage) : string.Empty,
            context.ShowProgressControls,
            context.Palette,
            stages);
    }

    private static string GetResultTitle(bool won, bool hasNextStage)
    {
        if (!won)
        {
            return "실패";
        }

        return hasNextStage ? "클리어!" : "전체 클리어!";
    }

    private static string GetResultBody(bool won, bool hasNextStage, int stageNumber, string statusText)
    {
        if (won)
        {
            return hasNextStage
                ? $"스테이지 {stageNumber} 클리어\n다음 스테이지가 열렸어요"
                : "20스테이지를 모두 클리어했어요";
        }

        return statusText == "STACK CROOKED"
            ? "한 줄로 쌓이지 않았어요"
            : "박스가 떨어졌어요";
    }

    private static string GetResultButtonLabel(bool won, bool hasNextStage)
    {
        if (!won)
        {
            return "다시 도전";
        }

        return hasNextStage ? "다음 스테이지" : "처음부터";
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
            bool canUseUndo,
            int undoCount,
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
            CanUseUndo = canUseUndo;
            UndoCount = undoCount;
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
        internal bool CanUseUndo { get; }
        internal int UndoCount { get; }
        internal bool ShowProgressControls { get; }
        internal BoxStackPrototypePalette Palette { get; }
    }
}
