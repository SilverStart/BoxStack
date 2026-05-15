using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

internal sealed class BoxStackPrototypeUi : MonoBehaviour
{
    private const string RuntimePanelSettingsAssetPath = "Assets/Resources/Prototype/Ui/BoxStackPanelSettings.asset";
    private const string RuntimePanelSettingsResourcePath = "Prototype/Ui/BoxStackPanelSettings";
    private const string RuntimeThemeResourcePath = "Prototype/Ui/BoxStackRuntimeTheme";
    private const float HudHorizontalPadding = 24f;
    private const float HudTopPadding = 12f;
    private const float StageBadgeWidth = 82f;
    private const float StageBadgeHeight = 54f;
    private const float UndoTicketWidth = 82f;
    private const float UndoTicketHeight = 48f;
    private const float ProgressPanelWidth = 176f;
    private const float ProgressPanelHeight = 54f;
    private const float ProgressSlotSize = 10f;
    private const float ProgressSlotGap = 3f;
    private const float StagePanelMaxWidth = 420f;
    private const float ResultPanelMaxWidth = 360f;

    private static readonly Color HudBackgroundColor = new Color(0.05f, 0.08f, 0.16f, 0.36f);
    private static readonly Color HudBorderColor = new Color(1.00f, 1.00f, 1.00f, 0.28f);
    private static readonly Color HudShadowColor = new Color(0.02f, 0.03f, 0.08f, 0.20f);
    private static readonly Color ProgressFillColor = new Color(0.36f, 0.96f, 1.00f, 0.96f);
    private static readonly Color ProgressNodeColor = new Color(1.00f, 0.88f, 0.34f, 0.98f);
    private static readonly Color PrimaryButtonColor = new Color(0.24f, 0.86f, 1.00f, 0.88f);
    private static readonly Color DisabledButtonColor = new Color(0.25f, 0.30f, 0.42f, 0.52f);
    private static readonly Color TextColor = new Color(1.00f, 1.00f, 1.00f, 0.96f);
    private static readonly Color SubTextColor = new Color(0.80f, 0.88f, 1.00f, 0.78f);

    private readonly List<Button> _stageButtons = new List<Button>();
    private readonly List<VisualElement> _progressNodes = new List<VisualElement>();

    private UIDocument _document;
    private PanelSettings _panelSettings;
    private ThemeStyleSheet _themeStyleSheet;
    private bool _ownsPanelSettings;
    private bool _ownsThemeStyleSheet;
    private VisualElement _root;
    private VisualElement _safeRoot;
    private VisualElement _hudShadow;
    private Button _stageButton;
    private VisualElement _progressRail;
    private VisualElement _feedbackToast;
    private Label _feedbackLabel;
    private Button _undoButton;
    private Label _buildLabel;
    private VisualElement _stageOverlay;
    private VisualElement _stagePanel;
    private VisualElement _stageGrid;
    private Button _resetProgressButton;
    private Button _unlockAllButton;
    private VisualElement _progressControls;
    private VisualElement _resultOverlay;
    private Label _resultStamp;
    private Label _resultTitle;
    private Label _resultBody;
    private Button _resultButton;
    private VisualElement _resultPanel;
    private Font _displayFont;
    private Font _bodyFont;
    private Font _fallbackFont;
    private Action _openStageSelect;
    private Action _undo;
    private Action<int> _selectStage;
    private Action _closeStageSelect;
    private Action _resetProgress;
    private Action _unlockAllStages;
    private Action _handleResult;
    private int _stageButtonCount = -1;

    internal readonly struct StageButtonState
    {
        internal StageButtonState(int number, int targetBoxes, bool unlocked, bool selected)
        {
            Number = number;
            TargetBoxes = targetBoxes;
            Unlocked = unlocked;
            Selected = selected;
        }

        internal int Number { get; }
        internal int TargetBoxes { get; }
        internal bool Unlocked { get; }
        internal bool Selected { get; }
    }

    internal readonly struct UiState
    {
        internal UiState(
            int buildNumber,
            string stageLabel,
            int placedBoxes,
            int targetBoxes,
            string feedbackLabel,
            bool stageSelectOpen,
            bool canUseUndo,
            int undoCount,
            bool resultVisible,
            string resultTitle,
            string resultBody,
            string resultButtonLabel,
            bool showProgressControls,
            BoxStackPrototypePalette palette,
            StageButtonState[] stages)
        {
            BuildNumber = buildNumber;
            StageLabel = stageLabel;
            PlacedBoxes = placedBoxes;
            TargetBoxes = targetBoxes;
            FeedbackLabel = feedbackLabel;
            StageSelectOpen = stageSelectOpen;
            CanUseUndo = canUseUndo;
            UndoCount = undoCount;
            ResultVisible = resultVisible;
            ResultTitle = resultTitle;
            ResultBody = resultBody;
            ResultButtonLabel = resultButtonLabel;
            ShowProgressControls = showProgressControls;
            Palette = palette;
            Stages = stages;
        }

        internal int BuildNumber { get; }
        internal string StageLabel { get; }
        internal int PlacedBoxes { get; }
        internal int TargetBoxes { get; }
        internal string FeedbackLabel { get; }
        internal bool StageSelectOpen { get; }
        internal bool CanUseUndo { get; }
        internal int UndoCount { get; }
        internal bool ResultVisible { get; }
        internal string ResultTitle { get; }
        internal string ResultBody { get; }
        internal string ResultButtonLabel { get; }
        internal bool ShowProgressControls { get; }
        internal BoxStackPrototypePalette Palette { get; }
        internal StageButtonState[] Stages { get; }
    }

    internal void Initialize(
        Font displayFont,
        Font bodyFont,
        Font fallbackFont,
        Action openStageSelect,
        Action undo,
        Action<int> selectStage,
        Action closeStageSelect,
        Action resetProgress,
        Action unlockAllStages,
        Action handleResult)
    {
        _displayFont = displayFont;
        _bodyFont = bodyFont;
        _fallbackFont = fallbackFont;
        _openStageSelect = openStageSelect;
        _undo = undo;
        _selectStage = selectStage;
        _closeStageSelect = closeStageSelect;
        _resetProgress = resetProgress;
        _unlockAllStages = unlockAllStages;
        _handleResult = handleResult;

        CreateDocument();
        BuildHud();
        BuildStageOverlay();
        BuildResultOverlay();
    }

    internal void Refresh(UiState state)
    {
        if (_root == null)
        {
            return;
        }

        ApplySafeArea();
        ApplyPalette(state.Palette);

        _stageButton.text = $"STAGE\n{state.StageLabel}";
        _feedbackLabel.text = state.FeedbackLabel;
        SetElementVisible(_feedbackToast, !string.IsNullOrWhiteSpace(state.FeedbackLabel));
        _undoButton.text = $"되돌리기 {state.UndoCount}";
        _undoButton.SetEnabled(state.CanUseUndo);
        ApplyButtonColors(_undoButton, state.CanUseUndo ? state.Palette.Accent : DisabledButtonColor, Color.white);
        _buildLabel.text = $"B{state.BuildNumber:000}";

        SetOverlayVisible(_stageOverlay, state.StageSelectOpen);
        SetOverlayVisible(_resultOverlay, state.ResultVisible);
        _progressControls.style.display = state.ShowProgressControls ? DisplayStyle.Flex : DisplayStyle.None;

        RefreshStageButtons(state.Stages, state.Palette);
        RefreshProgressRail(state.PlacedBoxes, state.TargetBoxes, state.Palette);

        if (state.ResultVisible)
        {
            bool failed = state.ResultTitle.IndexOf("실패", StringComparison.Ordinal) >= 0;
            _resultStamp.text = failed ? "MISS" : "CLEAR";
            ApplyPanelStyle(_resultStamp, failed ? new Color(1.00f, 0.28f, 0.32f, 0.90f) : state.Palette.Accent, new Color(1f, 1f, 1f, 0.24f), 16f);
            _resultTitle.text = state.ResultTitle;
            _resultBody.text = state.ResultBody;
            _resultButton.text = state.ResultButtonLabel;
        }
    }

    internal bool ContainsBlockingScreenPoint(Vector2 screenPosition)
    {
        if (_root == null)
        {
            return false;
        }

        if (IsDisplayed(_stageOverlay) || IsDisplayed(_resultOverlay))
        {
            return true;
        }

        var uiPoint = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
        return _stageButton.worldBound.Contains(uiPoint) || _undoButton.worldBound.Contains(uiPoint);
    }

    private void OnDestroy()
    {
        if (_ownsPanelSettings && _panelSettings != null)
        {
            UnityEngine.Object.Destroy(_panelSettings);
        }

        if (_ownsThemeStyleSheet && _themeStyleSheet != null)
        {
            UnityEngine.Object.Destroy(_themeStyleSheet);
        }
    }

    private void CreateDocument()
    {
        bool shouldActivateAfterSetup = !gameObject.activeSelf;
        _panelSettings = LoadPanelSettings();
        if (_panelSettings == null)
        {
            _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            _panelSettings.name = "BoxStack Prototype Panel Settings";
            _ownsPanelSettings = true;
        }

        if (_panelSettings.themeStyleSheet == null)
        {
            _themeStyleSheet = Resources.Load<ThemeStyleSheet>(RuntimeThemeResourcePath);
            if (_themeStyleSheet == null)
            {
                _themeStyleSheet = ScriptableObject.CreateInstance<ThemeStyleSheet>();
                _themeStyleSheet.name = "BoxStack Prototype Runtime Theme";
                _ownsThemeStyleSheet = true;
            }

            _panelSettings.themeStyleSheet = _themeStyleSheet;
        }

        _document = gameObject.AddComponent<UIDocument>();
        _document.panelSettings = _panelSettings;
        if (shouldActivateAfterSetup)
        {
            gameObject.SetActive(true);
        }

        _root = _document.rootVisualElement;
        _root.pickingMode = PickingMode.Ignore;
        _root.style.position = Position.Absolute;
        _root.style.left = 0f;
        _root.style.top = 0f;
        _root.style.right = 0f;
        _root.style.bottom = 0f;

        _safeRoot = new VisualElement { pickingMode = PickingMode.Ignore };
        _safeRoot.style.position = Position.Absolute;
        _root.Add(_safeRoot);
    }

    private static PanelSettings LoadPanelSettings()
    {
        PanelSettings panelSettings = Resources.Load<PanelSettings>(RuntimePanelSettingsResourcePath);
#if UNITY_EDITOR
        if (panelSettings == null)
        {
            panelSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<PanelSettings>(RuntimePanelSettingsAssetPath);
        }
#endif
        return panelSettings;
    }

    private void BuildHud()
    {
        _hudShadow = new VisualElement { pickingMode = PickingMode.Ignore };
        _hudShadow.style.position = Position.Absolute;
        _hudShadow.style.left = HudHorizontalPadding;
        _hudShadow.style.top = HudTopPadding + 2f;
        _hudShadow.style.width = StageBadgeWidth;
        _hudShadow.style.height = StageBadgeHeight;
        ApplyPanelStyle(_hudShadow, HudShadowColor, Color.clear, 14f);
        _safeRoot.Add(_hudShadow);

        _stageButton = new Button(() => _openStageSelect?.Invoke());
        _stageButton.style.position = Position.Absolute;
        _stageButton.style.left = HudHorizontalPadding;
        _stageButton.style.top = HudTopPadding;
        _stageButton.style.width = StageBadgeWidth;
        _stageButton.style.height = StageBadgeHeight;
        _stageButton.style.marginLeft = 0f;
        _stageButton.style.marginRight = 0f;
        _stageButton.style.marginTop = 0f;
        _stageButton.style.marginBottom = 0f;
        _stageButton.style.paddingLeft = 12f;
        _stageButton.style.paddingRight = 12f;
        _stageButton.style.whiteSpace = WhiteSpace.Normal;
        ApplyPanelStyle(_stageButton, HudBackgroundColor, HudBorderColor, 14f);
        ApplyDisplayText(_stageButton, 13, FontStyle.Bold, TextColor, TextAnchor.MiddleCenter);
        _safeRoot.Add(_stageButton);

        _progressRail = new VisualElement { pickingMode = PickingMode.Ignore };
        _progressRail.style.position = Position.Absolute;
        _progressRail.style.left = new Length(50f, LengthUnit.Percent);
        _progressRail.style.top = HudTopPadding;
        _progressRail.style.translate = new Translate(new Length(-50f, LengthUnit.Percent), 0f);
        _progressRail.style.width = ProgressPanelWidth;
        _progressRail.style.height = ProgressPanelHeight;
        _progressRail.style.flexDirection = FlexDirection.Row;
        _progressRail.style.alignItems = Align.Center;
        _progressRail.style.justifyContent = Justify.Center;
        _progressRail.style.paddingLeft = 8f;
        _progressRail.style.paddingRight = 8f;
        ApplyPanelStyle(_progressRail, new Color(0.04f, 0.07f, 0.15f, 0.32f), HudBorderColor, 18f);
        _safeRoot.Add(_progressRail);

        _feedbackToast = new VisualElement { pickingMode = PickingMode.Ignore };
        _feedbackToast.style.position = Position.Absolute;
        _feedbackToast.style.left = 0f;
        _feedbackToast.style.right = 0f;
        _feedbackToast.style.top = 142f;
        _feedbackToast.style.height = 42f;
        _feedbackToast.style.alignItems = Align.Center;
        _safeRoot.Add(_feedbackToast);

        _feedbackLabel = new Label();
        _feedbackLabel.style.width = 154f;
        _feedbackLabel.style.height = 42f;
        ApplyPanelStyle(_feedbackLabel, new Color(1.00f, 1.00f, 1.00f, 0.18f), new Color(1f, 1f, 1f, 0.34f), 21f);
        ApplyDisplayText(_feedbackLabel, 18, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        _feedbackToast.Add(_feedbackLabel);

        _undoButton = new Button(() => _undo?.Invoke());
        _undoButton.style.position = Position.Absolute;
        _undoButton.style.right = HudHorizontalPadding;
        _undoButton.style.top = HudTopPadding;
        _undoButton.style.width = UndoTicketWidth;
        _undoButton.style.height = UndoTicketHeight;
        _undoButton.style.marginLeft = 0f;
        _undoButton.style.marginRight = 0f;
        _undoButton.style.marginTop = 0f;
        _undoButton.style.marginBottom = 0f;
        ApplyButtonColors(_undoButton, PrimaryButtonColor, Color.white);
        ApplyDisplayText(_undoButton, 13, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        _safeRoot.Add(_undoButton);

        _buildLabel = new Label();
        _buildLabel.style.position = Position.Absolute;
        _buildLabel.style.right = HudHorizontalPadding + 10f;
        _buildLabel.style.top = HudTopPadding + UndoTicketHeight + 5f;
        _buildLabel.style.width = 64f;
        _buildLabel.style.height = 20f;
        ApplyDisplayText(_buildLabel, 12, FontStyle.Bold, new Color(1f, 1f, 1f, 0.34f), TextAnchor.MiddleRight);
        _safeRoot.Add(_buildLabel);
    }

    private void BuildStageOverlay()
    {
        _stageOverlay = CreateOverlay();
        _stageOverlay.style.justifyContent = Justify.FlexStart;
        _safeRoot.Add(_stageOverlay);

        _stagePanel = new VisualElement();
        _stagePanel.style.width = new Length(92f, LengthUnit.Percent);
        _stagePanel.style.maxWidth = StagePanelMaxWidth;
        _stagePanel.style.minHeight = 430f;
        _stagePanel.style.maxHeight = 560f;
        _stagePanel.style.marginTop = 156f;
        _stagePanel.style.paddingLeft = 24f;
        _stagePanel.style.paddingRight = 24f;
        _stagePanel.style.paddingTop = 20f;
        _stagePanel.style.paddingBottom = 20f;
        ApplyPanelStyle(_stagePanel, new Color(0.05f, 0.08f, 0.18f, 0.92f), HudBorderColor, 24f);
        _stageOverlay.Add(_stagePanel);

        var title = new Label("스테이지 선택");
        ApplyDisplayText(title, 24, FontStyle.Bold, TextColor, TextAnchor.MiddleCenter);
        title.style.height = 32f;
        _stagePanel.Add(title);

        var body = new Label("컬러 스택");
        ApplyBodyText(body, 14, FontStyle.Normal, SubTextColor, TextAnchor.MiddleCenter);
        body.style.height = 22f;
        _stagePanel.Add(body);

        _stageGrid = new VisualElement { pickingMode = PickingMode.Position };
        _stageGrid.style.marginTop = 16f;
        _stagePanel.Add(_stageGrid);

        _progressControls = new VisualElement { pickingMode = PickingMode.Position };
        _progressControls.style.flexDirection = FlexDirection.Row;
        _progressControls.style.marginTop = 18f;
        _progressControls.style.height = 34f;
        _stagePanel.Add(_progressControls);

        _resetProgressButton = CreatePanelButton("진행 초기화", () => _resetProgress?.Invoke(), 14);
        _resetProgressButton.style.flexGrow = 1f;
        _resetProgressButton.style.marginRight = 4f;
        _progressControls.Add(_resetProgressButton);

        _unlockAllButton = CreatePanelButton("전체 해금", () => _unlockAllStages?.Invoke(), 14);
        _unlockAllButton.style.flexGrow = 1f;
        _unlockAllButton.style.marginLeft = 4f;
        _progressControls.Add(_unlockAllButton);

        var closeButton = CreatePanelButton("닫기", () => _closeStageSelect?.Invoke(), 20);
        closeButton.style.height = 46f;
        closeButton.style.marginLeft = 48f;
        closeButton.style.marginRight = 48f;
        closeButton.style.marginTop = 20f;
        _stagePanel.Add(closeButton);
    }

    private void BuildResultOverlay()
    {
        _resultOverlay = CreateOverlay();
        _resultOverlay.style.justifyContent = Justify.Center;
        _safeRoot.Add(_resultOverlay);

        _resultPanel = new VisualElement();
        _resultPanel.style.width = new Length(84f, LengthUnit.Percent);
        _resultPanel.style.maxWidth = ResultPanelMaxWidth;
        _resultPanel.style.minHeight = 240f;
        _resultPanel.style.paddingLeft = 28f;
        _resultPanel.style.paddingRight = 28f;
        _resultPanel.style.paddingTop = 24f;
        _resultPanel.style.paddingBottom = 24f;
        ApplyPanelStyle(_resultPanel, new Color(0.05f, 0.08f, 0.18f, 0.94f), HudBorderColor, 18f);
        _resultOverlay.Add(_resultPanel);

        _resultStamp = new Label();
        _resultStamp.style.alignSelf = Align.Center;
        _resultStamp.style.width = 132f;
        _resultStamp.style.height = 32f;
        _resultStamp.style.marginBottom = 14f;
        ApplyDisplayText(_resultStamp, 15, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        _resultPanel.Add(_resultStamp);

        _resultTitle = new Label();
        _resultTitle.style.height = 44f;
        ApplyDisplayText(_resultTitle, 28, FontStyle.Bold, TextColor, TextAnchor.MiddleCenter);
        _resultPanel.Add(_resultTitle);

        _resultBody = new Label();
        _resultBody.style.minHeight = 64f;
        _resultBody.style.whiteSpace = WhiteSpace.Normal;
        ApplyBodyText(_resultBody, 18, FontStyle.Normal, SubTextColor, TextAnchor.MiddleCenter);
        _resultPanel.Add(_resultBody);

        _resultButton = CreatePanelButton(string.Empty, () => _handleResult?.Invoke(), 20);
        _resultButton.style.height = 54f;
        _resultButton.style.marginLeft = 26f;
        _resultButton.style.marginRight = 26f;
        _resultButton.style.marginTop = 22f;
        _resultPanel.Add(_resultButton);
    }

    private void RefreshStageButtons(StageButtonState[] stages, BoxStackPrototypePalette palette)
    {
        if (stages == null)
        {
            return;
        }

        if (_stageButtonCount != stages.Length)
        {
            RebuildStageButtons(stages.Length);
        }

        for (int i = 0; i < stages.Length; i++)
        {
            StageButtonState stage = stages[i];
            Button button = _stageButtons[i];
            button.text = stage.Unlocked ? $"ST {stage.Number:00}\n{stage.TargetBoxes}개" : $"ST {stage.Number:00}\n잠금";
            button.SetEnabled(stage.Unlocked);
            if (stage.Selected)
            {
                ApplyButtonColors(button, palette.Accent, Color.white);
            }
            else
            {
                ApplyButtonColors(button, stage.Unlocked ? palette.PanelBackground : DisabledButtonColor, stage.Unlocked ? TextColor : new Color(1f, 1f, 1f, 0.62f));
            }
        }
    }

    private void RefreshProgressRail(int placedBoxes, int targetBoxes, BoxStackPrototypePalette palette)
    {
        if (_progressRail == null)
        {
            return;
        }

        int nodeCount = Mathf.Max(1, targetBoxes);
        if (_progressNodes.Count != nodeCount)
        {
            RebuildProgressRail(nodeCount);
        }

        for (int i = 0; i < _progressNodes.Count; i++)
        {
            bool completed = i < placedBoxes;
            ApplyPanelStyle(
                _progressNodes[i],
                completed ? palette.SecondaryAccent : Color.clear,
                completed ? new Color(1f, 1f, 1f, 0.38f) : new Color(1f, 1f, 1f, 0.46f),
                3f);
        }
    }

    private void ApplyPalette(BoxStackPrototypePalette palette)
    {
        Color border = new Color(1f, 1f, 1f, 0.28f);
        ApplyPanelStyle(_stageButton, palette.PanelBackground, border, 14f);
        ApplyPanelStyle(_progressRail, WithAlpha(palette.PanelBackground, 0.34f), border, 18f);
        ApplyPanelStyle(_feedbackLabel, WithAlpha(palette.Accent, 0.22f), new Color(1f, 1f, 1f, 0.34f), 21f);

        if (_stagePanel != null)
        {
            ApplyPanelStyle(_stagePanel, WithAlpha(palette.PanelBackground, 0.92f), border, 24f);
        }

        if (_resultPanel != null)
        {
            ApplyPanelStyle(_resultPanel, WithAlpha(palette.PanelBackground, 0.94f), border, 18f);
        }
    }

    private void RebuildProgressRail(int nodeCount)
    {
        for (int i = 0; i < _progressNodes.Count; i++)
        {
            _progressNodes[i].RemoveFromHierarchy();
        }

        _progressNodes.Clear();
        for (int i = 0; i < nodeCount; i++)
        {
            var node = new VisualElement { pickingMode = PickingMode.Ignore };
            node.style.width = ProgressSlotSize;
            node.style.height = ProgressSlotSize;
            node.style.marginLeft = i == 0 ? 0f : ProgressSlotGap;
            node.style.flexShrink = 1f;
            _progressRail.Add(node);
            _progressNodes.Add(node);
        }
    }

    private void RebuildStageButtons(int stageCount)
    {
        _stageGrid.Clear();
        _stageButtons.Clear();
        _stageButtonCount = stageCount;

        const int columns = 4;
        int rows = Mathf.CeilToInt(stageCount / (float)columns);
        for (int row = 0; row < rows; row++)
        {
            var rowElement = new VisualElement { pickingMode = PickingMode.Position };
            rowElement.style.flexDirection = FlexDirection.Row;
            rowElement.style.height = 46f;
            rowElement.style.marginBottom = row == rows - 1 ? 0f : 8f;
            _stageGrid.Add(rowElement);

            for (int column = 0; column < columns; column++)
            {
                int index = (row * columns) + column;
                if (index >= stageCount)
                {
                    var spacer = new VisualElement { pickingMode = PickingMode.Ignore };
                    spacer.style.flexGrow = 1f;
                    spacer.style.marginLeft = 4f;
                    spacer.style.marginRight = 4f;
                    rowElement.Add(spacer);
                    continue;
                }

                int selectedIndex = index;
                Button button = CreatePanelButton(string.Empty, () => _selectStage?.Invoke(selectedIndex), 14);
                button.style.flexGrow = 1f;
                button.style.marginLeft = 4f;
                button.style.marginRight = 4f;
                button.style.whiteSpace = WhiteSpace.Normal;
                rowElement.Add(button);
                _stageButtons.Add(button);
            }
        }
    }

    private void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        if (safeArea.width <= 0f || safeArea.height <= 0f)
        {
            safeArea = new Rect(0f, 0f, Screen.width, Screen.height);
        }

        _safeRoot.style.left = safeArea.x;
        _safeRoot.style.top = Screen.height - safeArea.yMax;
        _safeRoot.style.width = safeArea.width;
        _safeRoot.style.height = safeArea.height;
        ApplyOverlayBounds(_stageOverlay, safeArea.width, safeArea.height);
        ApplyOverlayBounds(_resultOverlay, safeArea.width, safeArea.height);
    }

    private VisualElement CreateOverlay()
    {
        var overlay = new VisualElement();
        overlay.style.position = Position.Absolute;
        overlay.style.left = 0f;
        overlay.style.top = 0f;
        overlay.style.width = new Length(100f, LengthUnit.Percent);
        overlay.style.height = new Length(100f, LengthUnit.Percent);
        overlay.style.minWidth = 1f;
        overlay.style.minHeight = 1f;
        overlay.style.alignItems = Align.Center;
        overlay.style.backgroundColor = new Color(0f, 0f, 0f, 0.28f);
        overlay.pickingMode = PickingMode.Position;
        return overlay;
    }

    private Button CreatePanelButton(string text, Action clicked, int fontSize)
    {
        var button = new Button(() => clicked?.Invoke()) { text = text };
        button.style.height = 46f;
        button.style.paddingLeft = 8f;
        button.style.paddingRight = 8f;
        button.style.marginTop = 0f;
        button.style.marginBottom = 0f;
        ApplyButtonColors(button, PrimaryButtonColor, Color.white);
        ApplyDisplayText(button, fontSize, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        return button;
    }

    private void ApplyPanelStyle(VisualElement element, Color background, Color border, float radius)
    {
        element.style.backgroundColor = background;
        element.style.borderTopColor = border;
        element.style.borderRightColor = border;
        element.style.borderBottomColor = border;
        element.style.borderLeftColor = border;
        element.style.borderTopWidth = border.a > 0f ? 2f : 0f;
        element.style.borderRightWidth = border.a > 0f ? 2f : 0f;
        element.style.borderBottomWidth = border.a > 0f ? 2f : 0f;
        element.style.borderLeftWidth = border.a > 0f ? 2f : 0f;
        element.style.borderTopLeftRadius = radius;
        element.style.borderTopRightRadius = radius;
        element.style.borderBottomRightRadius = radius;
        element.style.borderBottomLeftRadius = radius;
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    private void ApplyButtonColors(Button button, Color background, Color text)
    {
        ApplyPanelStyle(button, background, new Color(1f, 1f, 1f, 0.18f), 18f);
        button.style.color = text;
    }

    private void ApplyDisplayText(TextElement element, int fontSize, FontStyle fontStyle, Color color, TextAnchor alignment)
    {
        ApplyText(element, _displayFont != null ? _displayFont : _fallbackFont, fontSize, fontStyle, color, alignment);
    }

    private void ApplyBodyText(TextElement element, int fontSize, FontStyle fontStyle, Color color, TextAnchor alignment)
    {
        ApplyText(element, _bodyFont != null ? _bodyFont : _fallbackFont, fontSize, fontStyle, color, alignment);
    }

    private static void ApplyText(TextElement element, Font font, int fontSize, FontStyle fontStyle, Color color, TextAnchor alignment)
    {
        if (font != null)
        {
            element.style.unityFont = font;
            element.style.unityFontDefinition = FontDefinition.FromFont(font);
        }

        element.style.fontSize = fontSize;
        element.style.unityFontStyleAndWeight = fontStyle;
        element.style.color = color;
        element.style.unityTextAlign = alignment;
    }

    private static bool IsDisplayed(VisualElement element)
    {
        return element != null
            && element.resolvedStyle.display != DisplayStyle.None
            && element.resolvedStyle.opacity > 0.5f;
    }

    private static void ApplyOverlayBounds(VisualElement overlay, float width, float height)
    {
        if (overlay == null)
        {
            return;
        }

        overlay.style.width = width;
        overlay.style.height = height;
        overlay.style.minWidth = width;
        overlay.style.minHeight = height;
    }

    private static void SetOverlayVisible(VisualElement overlay, bool visible)
    {
        if (overlay == null)
        {
            return;
        }

        overlay.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        overlay.style.visibility = visible ? Visibility.Visible : Visibility.Hidden;
        overlay.style.opacity = visible ? 1f : 0f;
        overlay.pickingMode = visible ? PickingMode.Position : PickingMode.Ignore;
        for (int i = 0; i < overlay.childCount; i++)
        {
            overlay.ElementAt(i).style.visibility = Visibility.Visible;
        }
    }

    private static void SetElementVisible(VisualElement element, bool visible)
    {
        if (element == null)
        {
            return;
        }

        element.style.visibility = visible ? Visibility.Visible : Visibility.Hidden;
        element.style.opacity = visible ? 1f : 0f;
    }
}
