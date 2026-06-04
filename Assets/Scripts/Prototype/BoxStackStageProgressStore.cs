// 스테이지 진행 저장소 - App-in-Toss 저장 방식으로 교체하기 위한 경계.

using UnityEngine;

internal sealed class BoxStackStageProgressStore
{
    private const string HighestUnlockedStageKey = "BoxStackPrototype.HighestUnlockedStage";

    internal int LoadHighestUnlockedStageIndex(int stageCount)
    {
        int maxStageIndex = Mathf.Max(0, stageCount - 1);
        int unlockedStageNumber = PlayerPrefs.GetInt(HighestUnlockedStageKey, 1);
        return Mathf.Clamp(unlockedStageNumber - 1, 0, maxStageIndex);
    }

    internal void SaveHighestUnlockedStageNumber(int stageNumber)
    {
        PlayerPrefs.SetInt(HighestUnlockedStageKey, stageNumber);
        PlayerPrefs.Save();
    }
}
