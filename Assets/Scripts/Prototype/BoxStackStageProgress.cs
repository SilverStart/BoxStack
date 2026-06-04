// 스테이지 진행 상태 - 선택/해금 흐름을 메인 런타임에서 분리한다.

using System;
using UnityEngine;

internal sealed class BoxStackStageProgress
{
    private readonly BoxStackStageProgressStore _store;
    private int _currentStageIndex;
    private int _highestUnlockedStageIndex;

    internal BoxStackStageProgress(BoxStackStageProgressStore store)
    {
        _store = store;
    }

    internal int CurrentStageIndex
    {
        get { return _currentStageIndex; }
    }

    internal int HighestUnlockedStageIndex
    {
        get { return _highestUnlockedStageIndex; }
    }

    internal void Load(int stageCount)
    {
        _highestUnlockedStageIndex = _store.LoadHighestUnlockedStageIndex(stageCount);
        _currentStageIndex = Mathf.Clamp(_currentStageIndex, 0, _highestUnlockedStageIndex);
    }

    internal bool TryMove(int direction)
    {
        int nextStageIndex = Mathf.Clamp(_currentStageIndex + direction, 0, _highestUnlockedStageIndex);
        if (nextStageIndex == _currentStageIndex)
        {
            return false;
        }

        _currentStageIndex = nextStageIndex;
        return true;
    }

    internal bool TrySelect(int stageIndex, int stageCount, out bool stageChanged)
    {
        stageChanged = false;
        stageIndex = Mathf.Clamp(stageIndex, 0, Mathf.Max(0, stageCount - 1));
        if (!IsStageUnlocked(stageIndex))
        {
            return false;
        }

        stageChanged = stageIndex != _currentStageIndex;
        _currentStageIndex = stageIndex;
        return true;
    }

    internal void SelectFirstStage()
    {
        _currentStageIndex = 0;
    }

    internal bool HasNextStage(int stageCount)
    {
        return _currentStageIndex < stageCount - 1;
    }

    internal bool IsStageUnlocked(int stageIndex)
    {
        return stageIndex <= _highestUnlockedStageIndex;
    }

    internal void Reset(Func<int, int> getStageNumber)
    {
        _highestUnlockedStageIndex = 0;
        _currentStageIndex = 0;
        Save(getStageNumber);
    }

    internal void UnlockAll(int stageCount, Func<int, int> getStageNumber)
    {
        _highestUnlockedStageIndex = Mathf.Max(0, stageCount - 1);
        Save(getStageNumber);
    }

    internal bool UnlockNextStage(int stageCount, Func<int, int> getStageNumber)
    {
        if (!HasNextStage(stageCount) || _highestUnlockedStageIndex > _currentStageIndex)
        {
            return false;
        }

        _highestUnlockedStageIndex = Mathf.Clamp(_currentStageIndex + 1, 0, Mathf.Max(0, stageCount - 1));
        Save(getStageNumber);
        return true;
    }

    private void Save(Func<int, int> getStageNumber)
    {
        _store.SaveHighestUnlockedStageNumber(getStageNumber(_highestUnlockedStageIndex));
    }
}
