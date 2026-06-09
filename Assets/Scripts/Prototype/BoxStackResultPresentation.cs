internal readonly struct BoxStackResultPresentationState
{
    internal BoxStackResultPresentationState(string title, string body, string buttonLabel)
    {
        Title = title;
        Body = body;
        ButtonLabel = buttonLabel;
    }

    internal string Title { get; }
    internal string Body { get; }
    internal string ButtonLabel { get; }
}

internal sealed class BoxStackResultPresentation
{
    internal BoxStackResultPresentationState Create(
        bool won,
        bool hasNextStage,
        bool lastClearWasNewBest,
        string statusText)
    {
        return new BoxStackResultPresentationState(
            GetTitle(won, hasNextStage),
            GetBody(won, lastClearWasNewBest, statusText),
            GetButtonLabel(won, hasNextStage));
    }

    private static string GetTitle(bool won, bool hasNextStage)
    {
        if (!won)
        {
            return "실패";
        }

        return hasNextStage ? "클리어!" : "전체 클리어!";
    }

    private static string GetBody(bool won, bool lastClearWasNewBest, string statusText)
    {
        if (won)
        {
            return lastClearWasNewBest ? "탑이 안정됐어요\n최고 기록 갱신" : "탑이 안정됐어요";
        }

        if (statusText == "STACK SPREAD")
        {
            return "탑이 무너졌어요";
        }

        return "탑 밖으로 박스가 떨어졌어요";
    }

    private static string GetButtonLabel(bool won, bool hasNextStage)
    {
        if (!won)
        {
            return "다시 도전";
        }

        return hasNextStage ? "다음 스테이지" : "처음부터";
    }
}
