using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

internal sealed class BoxStackPrototypeUi : MonoBehaviour
{
    private const string RuntimeThemeResourcePath = "Prototype/Ui/BoxStackRuntimeTheme";
    private const float HudHorizontalPadding = 24f;
    private const float HudTopPadding = 12f;
    private const float HudBarHeight = 68f;
    private const float UndoButtonTop = 90f;
    private const float UndoButtonHeight = 42f;
    private const float StagePanelMaxWidth = 420f;
    private const float ResultPanelMaxWidth = 360f;

    private static readonly Color HudBackgroundColor = new Color(1f, 1f, 1f, 0.86f);
    private static readonly Color HudBorderColor = new Color(0.73f, 0.52f, 0.28f, 0.72f);
    private static readonly Color HudShadowColor = new Color(0.04f, 0.06f, 0.08f, 0.22f);
    private static readonly Color ProgressTrackColor = new Color(0.90f, 0.78f, 0.58f, 0.75f);
    private static readonly Color ProgressFillColor = new Color(0.58f, 0.38f, 0.18f, 0.95f);
    private static readonly Color PrimaryButtonColor = new Color(0.58f, 0.38f, 0.18f, 0.96f);
    private static readonly Color DisabledButtonColor = new Color(0.55f, 0.48f, 0.38f, 0.56f);
    private static readonly Color LabelColor = new Color(0.37f, 0.42f, 0.49f, 0.92f);
    private static readonly Color TextColor = new Color(0.09f, 0.11f, 0.14f);
    private static readonly Color SubTextColor = new Color(0.38f, 0.43f, 0.50f);

    private readonly List<Button> _stageButtons = new List<Button>();

    private UIDocument _document;
    private PanelSettings _panelSettings;
    private ThemeStyleSheet _themeStyleSheet;
    private bool _ownsThemeStyleSheet;
    private VisualElement _root;
    private VisualElement _safeRoot;
    private VisualElement _hudShadow;
    private VisualElement _hudBar;
    private Button _stageButton;
    private Label _countLabel;
    private VisualElement _progressFill;
    private Label _statusLabel;
    private Button _undoButton;
    private Label _buildLabel;
    private VisualElement _stageOverlay;
    private VisualElement _stagePanel;
    private VisualElement _stageGrid;
    private Button _resetProgressButton;
    private Button _unlockAllButton;
    private VisualElement _progressControls;
    private VisualElement _resultOverlay;
    private Label _resultTitle;
    private Label _resultBody;
    private Button _resultButton;
    private Font _font;
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
            float progress,
            string statusLabel,
            bool stageSelectOpen,
            bool canUseUndo,
            int undoCount,
            bool resultVisible,
            string resultTitle,
            string resultBody,
            string resultButtonLabel,
            bool showProgressControls,
            StageButtonState[] stages)
        {
            BuildNumber = buildNumber;
            StageLabel = stageLabel;
            PlacedBoxes = placedBoxes;
            TargetBoxes = targetBoxes;
            Progress = progress;
            StatusLabel = statusLabel;
            StageSelectOpen = stageSelectOpen;
            CanUseUndo = canUseUndo;
            UndoCount = undoCount;
            ResultVisible = resultVisible;
            ResultTitle = resultTitle;
            ResultBody = resultBody;
            ResultButtonLabel = resultButtonLabel;
            ShowProgressControls = showProgressControls;
            Stages = stages;
        }

        internal int BuildNumber { get; }
        internal string StageLabel { get; }
        internal int PlacedBoxes { get; }
        internal int TargetBoxes { get; }
        internal float Progress { get; }
        internal string StatusLabel { get; }
        internal bool StageSelectOpen { get; }
        internal bool CanUseUndo { get; }
        internal int UndoCount { get; }
        internal bool ResultVisible { get; }
        internal string ResultTitle { get; }
        internal string ResultBody { get; }
        internal string ResultButtonLabel { get; }
        internal bool ShowProgressControls { get; }
        internal StageButtonState[] Stages { get; }
    }

    internal void Initialize(
        Font font,
        Action openStageSelect,
        Action undo,
        Action<int> selectStage,
        Action closeStageSelect,
        Action resetProgress,
        Action unlockAllStages,
        Action handleResult)
    {
        _font = font;
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

        _stageButton.text = state.StageLabel;
        _countLabel.text = $"{state.PlacedBoxes} / {state.TargetBoxes}";
        _progressFill.style.width = new Length(Mathf.Clamp01(state.Progress) * 100f, LengthUnit.Percent);
        _statusLabel.text = state.StatusLabel;
        _undoButton.text = $"되돌리기 {state.UndoCount}";
        _undoButton.SetEnabled(state.CanUseUndo);
        ApplyButtonColors(_undoButton, state.CanUseUndo ? PrimaryButtonColor : DisabledButtonColor, Color.white);
        _buildLabel.text = $"B{state.BuildNumber:000}";

        SetOverlayVisible(_stageOverlay, state.StageSelectOpen);
        SetOverlayVisible(_resultOverlay, state.ResultVisible);
        _progressControls.style.display = state.ShowProgressControls ? DisplayStyle.Flex : DisplayStyle.None;

        RefreshStageButtons(state.Stages);

        if (state.ResultVisible)
        {
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
        if (_panelSettings != null)
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
        _panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        _panelSettings.name = "BoxStack Prototype Panel Settings";
        _themeStyleSheet = Resources.Load<ThemeStyleSheet>(RuntimeThemeResourcePath);
        if (_themeStyleSheet == null)
        {
            _themeStyleSheet = ScriptableObject.CreateInstance<ThemeStyleSheet>();
            _themeStyleSheet.name = "BoxStack Prototype Runtime Theme";
            _ownsThemeStyleSheet = true;
        }

        _panelSettings.themeStyleSheet = _themeStyleSheet;

        _document = gameObject.AddComponent<UIDocument>();
        _document.panelSettings = _panelSettings;
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

    private void BuildHud()
    {
        _hudShadow = new VisualElement { pickingMode = PickingMode.Ignore };
        _hudShadow.style.position = Position.Absolute;
        _hudShadow.style.left = HudHorizontalPadding;
        _hudShadow.style.right = HudHorizontalPadding;
        _hudShadow.style.top = HudTopPadding + 2f;
        _hudShadow.style.height = HudBarHeight;
        ApplyPanelStyle(_hudShadow, HudShadowColor, Color.clear, 24f);
        _safeRoot.Add(_hudShadow);

        _hudBar = new VisualElement { pickingMode = PickingMode.Ignore };
        _hudBar.style.position = Position.Absolute;
        _hudBar.style.left = HudHorizontalPadding;
        _hudBar.style.right = HudHorizontalPadding;
        _hudBar.style.top = HudTopPadding;
        _hudBar.style.height = HudBarHeight;
        _hudBar.style.flexDirection = FlexDirection.Row;
        _hudBar.style.alignItems = Align.Center;
        _hudBar.style.paddingLeft = 22f;
        _hudBar.style.paddingRight = 22f;
        ApplyPanelStyle(_hudBar, HudBackgroundColor, HudBorderColor, 24f);
        _safeRoot.Add(_hudBar);

        var stageStack = new VisualElement { pickingMode = PickingMode.Ignore };
        stageStack.style.width = 132f;
        stageStack.style.flexShrink = 0f;
        stageStack.style.justifyContent = Justify.Center;
        _hudBar.Add(stageStack);

        _stageButton = new Button(() => _openStageSelect?.Invoke());
        _stageButton.style.height = 24f;
        _stageButton.style.marginLeft = 0f;
        _stageButton.style.marginRight = 0f;
        _stageButton.style.marginTop = 0f;
        _stageButton.style.marginBottom = 0f;
        _stageButton.style.paddingLeft = 0f;
        _stageButton.style.paddingRight = 0f;
        _stageButton.style.backgroundColor = Color.clear;
        _stageButton.style.borderTopWidth = 0f;
        _stageButton.style.borderRightWidth = 0f;
        _stageButton.style.borderBottomWidth = 0f;
        _stageButton.style.borderLeftWidth = 0f;
        ApplyText(_stageButton, 18, FontStyle.Normal, LabelColor, TextAnchor.MiddleLeft);
        stageStack.Add(_stageButton);

        _countLabel = new Label();
        ApplyText(_countLabel, 26, FontStyle.Bold, new Color(0.11f, 0.13f, 0.16f), TextAnchor.MiddleLeft);
        _countLabel.style.height = 34f;
        stageStack.Add(_countLabel);

        var progressTrack = new VisualElement { pickingMode = PickingMode.Ignore };
        progressTrack.style.flexGrow = 1f;
        progressTrack.style.height = 14f;
        progressTrack.style.marginLeft = 24f;
        progressTrack.style.marginRight = 24f;
        ApplyPanelStyle(progressTrack, ProgressTrackColor, Color.clear, 7f);
        _hudBar.Add(progressTrack);

        _progressFill = new VisualElement { pickingMode = PickingMode.Ignore };
        _progressFill.style.height = 14f;
        ApplyPanelStyle(_progressFill, ProgressFillColor, Color.clear, 7f);
        progressTrack.Add(_progressFill);

        _statusLabel = new Label();
        _statusLabel.style.width = 82f;
        _statusLabel.style.flexShrink = 0f;
        ApplyText(_statusLabel, 18, FontStyle.Normal, LabelColor, TextAnchor.MiddleCenter);
        _hudBar.Add(_statusLabel);

        _undoButton = new Button(() => _undo?.Invoke());
        _undoButton.style.position = Position.Absolute;
        _undoButton.style.left = HudHorizontalPadding + 22f;
        _undoButton.style.top = UndoButtonTop;
        _undoButton.style.width = 112f;
        _undoButton.style.height = UndoButtonHeight;
        ApplyButtonColors(_undoButton, PrimaryButtonColor, Color.white);
        ApplyText(_undoButton, 15, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        _safeRoot.Add(_undoButton);

        _buildLabel = new Label();
        _buildLabel.style.position = Position.Absolute;
        _buildLabel.style.right = HudHorizontalPadding + 12f;
        _buildLabel.style.top = HudTopPadding + HudBarHeight + 6f;
        _buildLabel.style.width = 64f;
        _buildLabel.style.height = 20f;
        ApplyText(_buildLabel, 12, FontStyle.Bold, new Color(0.20f, 0.16f, 0.12f, 0.45f), TextAnchor.MiddleRight);
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
        ApplyPanelStyle(_stagePanel, new Color(1f, 1f, 1f, 0.94f), new Color(0.73f, 0.52f, 0.28f, 0.58f), 24f);
        _stageOverlay.Add(_stagePanel);

        var title = new Label("스테이지 선택");
        ApplyText(title, 24, FontStyle.Bold, TextColor, TextAnchor.MiddleCenter);
        title.style.height = 32f;
        _stagePanel.Add(title);

        var body = new Label("배송 루트");
        ApplyText(body, 14, FontStyle.Normal, SubTextColor, TextAnchor.MiddleCenter);
        body.style.height = 22f;
        _stagePanel.Add(body);

        _stageGrid = new VisualElement { pickingMode = PickingMode.Ignore };
        _stageGrid.style.marginTop = 16f;
        _stagePanel.Add(_stageGrid);

        _progressControls = new VisualElement { pickingMode = PickingMode.Ignore };
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

        var panel = new VisualElement();
        panel.style.width = new Length(84f, LengthUnit.Percent);
        panel.style.maxWidth = ResultPanelMaxWidth;
        panel.style.minHeight = 240f;
        panel.style.paddingLeft = 28f;
        panel.style.paddingRight = 28f;
        panel.style.paddingTop = 36f;
        panel.style.paddingBottom = 24f;
        ApplyPanelStyle(panel, new Color(1f, 1f, 1f, 0.94f), new Color(0.73f, 0.52f, 0.28f, 0.58f), 24f);
        _resultOverlay.Add(panel);

        _resultTitle = new Label();
        _resultTitle.style.height = 44f;
        ApplyText(_resultTitle, 30, FontStyle.Bold, TextColor, TextAnchor.MiddleCenter);
        panel.Add(_resultTitle);

        _resultBody = new Label();
        _resultBody.style.minHeight = 64f;
        _resultBody.style.whiteSpace = WhiteSpace.Normal;
        ApplyText(_resultBody, 18, FontStyle.Normal, SubTextColor, TextAnchor.MiddleCenter);
        panel.Add(_resultBody);

        _resultButton = CreatePanelButton(string.Empty, () => _handleResult?.Invoke(), 20);
        _resultButton.style.height = 54f;
        _resultButton.style.marginLeft = 26f;
        _resultButton.style.marginRight = 26f;
        _resultButton.style.marginTop = 22f;
        panel.Add(_resultButton);
    }

    private void RefreshStageButtons(StageButtonState[] stages)
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
                ApplyButtonColors(button, PrimaryButtonColor, Color.white);
            }
            else
            {
                ApplyButtonColors(button, stage.Unlocked ? HudBackgroundColor : DisabledButtonColor, stage.Unlocked ? new Color(0.18f, 0.14f, 0.10f) : new Color(1f, 1f, 1f, 0.62f));
            }
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
            var rowElement = new VisualElement { pickingMode = PickingMode.Ignore };
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
        ApplyText(button, fontSize, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
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

    private void ApplyButtonColors(Button button, Color background, Color text)
    {
        ApplyPanelStyle(button, background, new Color(1f, 1f, 1f, 0.18f), 18f);
        button.style.color = text;
    }

    private void ApplyText(TextElement element, int fontSize, FontStyle fontStyle, Color color, TextAnchor alignment)
    {
        if (_font != null)
        {
            element.style.unityFont = _font;
        }

        element.style.fontSize = fontSize;
        element.style.unityFontStyleAndWeight = fontStyle;
        element.style.color = color;
        element.style.unityTextAlign = alignment;
    }

    private static bool IsDisplayed(VisualElement element)
    {
        return element != null && element.resolvedStyle.opacity > 0.5f;
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

        overlay.style.visibility = Visibility.Visible;
        overlay.style.opacity = visible ? 1f : 0f;
        overlay.pickingMode = visible ? PickingMode.Position : PickingMode.Ignore;
        for (int i = 0; i < overlay.childCount; i++)
        {
            overlay.ElementAt(i).style.visibility = Visibility.Visible;
        }
    }
}
