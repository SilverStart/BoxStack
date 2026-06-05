using System;
using UnityEngine;

internal sealed class BoxStackRuntimeHosts
{
    private const string UiObjectName = "BoxStack Prototype UI";
    private const string AudioObjectName = "BoxStack Prototype Audio";

    private readonly BoxStackPrototypeAssetLoader _assetLoader;
    private readonly Transform _parent;

    internal BoxStackRuntimeHosts(BoxStackPrototypeAssetLoader assetLoader, Transform parent)
    {
        _assetLoader = assetLoader;
        _parent = parent;
    }

    internal BoxStackPrototypeUi Ui { get; private set; }
    internal BoxStackPrototypeAudio Audio { get; private set; }

    internal void EnsureUi(UiCallbacks callbacks)
    {
        if (Ui != null)
        {
            return;
        }

        Font displayFont = _assetLoader.LoadDisplayFont();
        Font bodyFont = _assetLoader.LoadBodyFont();
        Font fallbackFont = _assetLoader.LoadKoreanFallbackFont();

        var uiObject = new GameObject(UiObjectName);
        uiObject.transform.SetParent(_parent, false);
        uiObject.SetActive(false);
        Ui = uiObject.AddComponent<BoxStackPrototypeUi>();
        Ui.Initialize(
            displayFont,
            bodyFont,
            fallbackFont,
            callbacks.OpenStageSelect,
            callbacks.SelectStage,
            callbacks.CloseStageSelect,
            callbacks.ResetProgress,
            callbacks.UnlockAllStages,
            callbacks.HandleResult);
    }

    internal void EnsureAudio()
    {
        if (Audio != null)
        {
            return;
        }

        var audioObject = new GameObject(AudioObjectName);
        audioObject.transform.SetParent(_parent, false);
        Audio = audioObject.AddComponent<BoxStackPrototypeAudio>();
    }

    internal readonly struct UiCallbacks
    {
        internal UiCallbacks(
            Action openStageSelect,
            Action<int> selectStage,
            Action closeStageSelect,
            Action resetProgress,
            Action unlockAllStages,
            Action handleResult)
        {
            OpenStageSelect = openStageSelect;
            SelectStage = selectStage;
            CloseStageSelect = closeStageSelect;
            ResetProgress = resetProgress;
            UnlockAllStages = unlockAllStages;
            HandleResult = handleResult;
        }

        internal Action OpenStageSelect { get; }
        internal Action<int> SelectStage { get; }
        internal Action CloseStageSelect { get; }
        internal Action ResetProgress { get; }
        internal Action UnlockAllStages { get; }
        internal Action HandleResult { get; }
    }
}
