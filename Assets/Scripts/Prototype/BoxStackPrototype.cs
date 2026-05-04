// PROTOTYPE - NOT FOR PRODUCTION
// Question: Does a 2D/2.5D parcel-box stacking loop fit the Toss app-in-app concept and AI PNG asset pipeline?
// Date: 2026-05-02

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public sealed class BoxStackPrototype : MonoBehaviour
{
    private const string ParcelAssetFolder = "Assets/Art/Prototype/Parcel";
    private const string ParcelResourceFolder = "Prototype/Parcel";
    private const string BackgroundAssetFolder = "Assets/Art/Prototype/Backgrounds";
    private const string BackgroundResourceFolder = "Prototype/Backgrounds";
    private const string StackBaseSpriteName = "parcel_stack_base_01";
    private const string BackgroundSpriteName = "logistics_center_bg_01";
    private static readonly bool UseLogisticsCenterBackground = false;
    private const float BaseMoveRange = 2.45f;
    private const float BaseMoveSpeed = 1.85f;
    private const float BoxSize = 1.0f;
    private const float DropSettleSeconds = 1.0f;
    private const float ClearValidationSeconds = 5.0f;
    private const float LostHeight = -4.0f;
    private const float LostHorizontalDistance = 4.0f;
    private const float StackLineTolerance = 0.75f;
    private const float CameraYOffset = 2.2f;
    private const float CameraInitialY = 2.5f;
    private const float CameraBottomPadding = 0.25f;
    private const float FloorY = -0.65f;
    private const float FloorHeight = 0.35f;
    private const int HudMaxFontSize = 28;
    private const int HudMinFontSize = 18;
    private const float HudHorizontalPadding = 24f;
    private const float HudTopY = 18f;
    private const float HudBarHeight = 68f;
    private const float HudProgressHeight = 14f;
    private const float ResultPanelMaxWidth = 360f;
    private const float ResultPanelHeight = 260f;

    private readonly List<GameObject> _placedBoxes = new List<GameObject>();
    private readonly List<BoxVisual> _boxVisuals = new List<BoxVisual>();

    private GameObject _activeBox;
    private GameObject _droppingBox;
    private Camera _camera;
    private Sprite _floorSprite;
    private Sprite _backgroundSprite;
    private GameObject _background;
    private GUIStyle _hudPillStyle;
    private GUIStyle _hudPillShadowStyle;
    private GUIStyle _hudProgressTrackStyle;
    private GUIStyle _hudProgressFillStyle;
    private GUIStyle _resultPanelStyle;
    private GUIStyle _resultButtonStyle;
    private Texture2D _hudPillTexture;
    private Texture2D _hudPillShadowTexture;
    private Texture2D _hudProgressTrackTexture;
    private Texture2D _hudProgressFillTexture;
    private Texture2D _hudProgressCapTexture;
    private Texture2D _resultPanelTexture;
    private Texture2D _resultButtonTexture;
    private Color _activeTint = new Color(1.0f, 0.82f, 0.45f);
    private Color _placedTint = new Color(0.86f, 0.62f, 0.34f);
    private PrototypeState _state;
    private PrototypeState _stateBeforeStageSelect;
    private float _spawnHeight;
    private float _moveStartedAt;
    private float _minimumCameraY = CameraInitialY;
    private float _cameraVelocityY;
    private float _clearValidationEndTime;
    private float _timeScaleBeforeStageSelect = 1f;
    private int _attempts;
    private int _currentStageIndex;
    private string _statusText = "READY";

    private enum PrototypeState
    {
        Playing,
        ResolvingDrop,
        ValidatingClear,
        StageSelect,
        Won,
        Failed
    }

    private struct BoxVisual
    {
        public BoxVisual(Sprite sprite, Vector2 worldSize, Rect visibleTextureRect, bool isPlaceholder)
        {
            Sprite = sprite;
            WorldSize = worldSize;
            VisibleTextureRect = visibleTextureRect;
            IsPlaceholder = isPlaceholder;
        }

        public Sprite Sprite;
        public Vector2 WorldSize;
        public Rect VisibleTextureRect;
        public bool IsPlaceholder;
    }

    private struct BoxAssetDefinition
    {
        public BoxAssetDefinition(string name, Vector2 worldSize)
        {
            Name = name;
            WorldSize = worldSize;
        }

        public string Name;
        public Vector2 WorldSize;
    }

    private struct StageConfig
    {
        public StageConfig(int number, int targetBoxes, string boxSequence, float speedMultiplier, float rangeMultiplier)
        {
            Number = number;
            TargetBoxes = targetBoxes;
            BoxSequence = boxSequence;
            SpeedMultiplier = speedMultiplier;
            RangeMultiplier = rangeMultiplier;
        }

        public int Number;
        public int TargetBoxes;
        public string BoxSequence;
        public float SpeedMultiplier;
        public float RangeMultiplier;
    }

    private static readonly BoxAssetDefinition[] BoxAssetDefinitions =
    {
        new BoxAssetDefinition("parcel_box_basic_01", new Vector2(1.0f, 1.0f)),
        new BoxAssetDefinition("parcel_box_wide_01", new Vector2(1.18f, 0.88f)),
        new BoxAssetDefinition("parcel_box_tall_01", new Vector2(0.88f, 1.18f))
    };

    private static readonly StageConfig[] StageConfigs =
    {
        new StageConfig(1, 4, "BBBB", 0.75f, 0.75f),
        new StageConfig(2, 5, "BBBBB", 0.80f, 0.80f),
        new StageConfig(3, 6, "BBWBBB", 0.85f, 0.85f),
        new StageConfig(4, 6, "BTBBWB", 0.90f, 0.90f),
        new StageConfig(5, 7, "BBWBTBB", 0.95f, 0.95f),
        new StageConfig(6, 7, "WBBTBBW", 1.00f, 1.00f),
        new StageConfig(7, 8, "BBTBWBBB", 1.00f, 1.00f),
        new StageConfig(8, 8, "WBTBBWBT", 1.05f, 1.00f),
        new StageConfig(9, 8, "BTTBWBBW", 1.05f, 1.05f),
        new StageConfig(10, 9, "BBWTBBWBB", 1.10f, 1.05f),
        new StageConfig(11, 9, "WTBBTBWBB", 1.10f, 1.10f),
        new StageConfig(12, 9, "BWBTWBTBB", 1.15f, 1.10f),
        new StageConfig(13, 10, "BBTWBTBWBB", 1.15f, 1.15f),
        new StageConfig(14, 10, "WBTBTWBBTB", 1.20f, 1.15f),
        new StageConfig(15, 10, "BTWBBTWTBB", 1.20f, 1.20f),
        new StageConfig(16, 11, "WBBTWBTBWBB", 1.25f, 1.20f),
        new StageConfig(17, 11, "BTBWTBBWTBB", 1.25f, 1.25f),
        new StageConfig(18, 12, "BWTBBWTBTWBB", 1.30f, 1.25f),
        new StageConfig(19, 12, "WTBTWBBTWBTB", 1.35f, 1.30f),
        new StageConfig(20, 12, "BTWTBWTBWTBB", 1.40f, 1.30f)
    };

    private static readonly Color ParcelBrownBackgroundColor = new Color(0.70f, 0.56f, 0.38f);

    private StageConfig CurrentStage
    {
        get
        {
            return StageConfigs[Mathf.Clamp(_currentStageIndex, 0, StageConfigs.Length - 1)];
        }
    }

    private int CurrentTargetBoxes
    {
        get { return CurrentStage.TargetBoxes; }
    }

    private float CurrentMoveRange
    {
        get { return BaseMoveRange * CurrentStage.RangeMultiplier; }
    }

    private float CurrentMoveSpeed
    {
        get { return BaseMoveSpeed * CurrentStage.SpeedMultiplier; }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<BoxStackPrototype>() != null)
        {
            return;
        }

        new GameObject("BoxStack 2D Prototype").AddComponent<BoxStackPrototype>();
    }

    private void Start()
    {
        LoadPrototypeSprites();
        _camera = EnsureCamera();
        if (UseLogisticsCenterBackground)
        {
            CreateBackground();
        }

        CreateFloor();
        RestartGame();
    }

    private void Update()
    {
        if (_state == PrototypeState.StageSelect)
        {
            if (CloseStageSelectPressed())
            {
                CloseStageSelect();
            }

            UpdateCamera();
            return;
        }

        if (PreviousStagePressed())
        {
            ChangeStage(-1);
            return;
        }

        if (NextStagePressed())
        {
            ChangeStage(1);
            return;
        }

        if (RestartPressed())
        {
            RestartGame();
            return;
        }

        if (_state == PrototypeState.Playing)
        {
            MoveActiveBox();

            if (DropPressed())
            {
                DropActiveBox();
            }

            if (AnyBoxLost())
            {
                EndRun(false, "STACK LOST");
            }
        }
        else if (_state == PrototypeState.ValidatingClear)
        {
            UpdateClearValidation();
        }

        UpdateCamera();
    }

    private void OnGUI()
    {
        EnsureHudStyles();

        float topY = GetHudTopY();
        float barWidth = Screen.width - (HudHorizontalPadding * 2f);
        var barRect = new Rect(HudHorizontalPadding, topY, barWidth, HudBarHeight);
        GUI.Box(new Rect(barRect.x, barRect.y + 2f, barRect.width, barRect.height), GUIContent.none, _hudPillShadowStyle);
        GUI.Box(barRect, GUIContent.none, _hudPillStyle);

        float leftWidth = Mathf.Clamp(Screen.width * 0.27f, 126f, 176f);
        float rightWidth = Mathf.Clamp(Screen.width * 0.18f, 72f, 104f);
        var labelRect = new Rect(barRect.x + 28f, barRect.y + 10f, leftWidth, 22f);
        var countRect = new Rect(barRect.x + 76f, barRect.y + 14f, leftWidth - 44f, 38f);
        var statusRect = new Rect(barRect.xMax - rightWidth - 22f, barRect.y + 16f, rightWidth, 36f);
        var progressRect = new Rect(
            barRect.x + leftWidth + 62f,
            barRect.y + ((HudBarHeight - HudProgressHeight) * 0.5f),
            Mathf.Max(44f, barWidth - leftWidth - rightWidth - 112f),
            HudProgressHeight);

        Color labelColor = new Color(0.37f, 0.42f, 0.49f, 0.92f);
        var labelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip,
            fontSize = 18,
            fontStyle = FontStyle.Normal,
            wordWrap = false,
            normal = { textColor = labelColor }
        };
        SetTextColorStates(labelStyle, labelColor);

        int countFontSize = GetHudFontSize("88 / 88", countRect.width);
        Color countColor = new Color(0.11f, 0.13f, 0.16f);
        var countStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            clipping = TextClipping.Clip,
            fontSize = countFontSize,
            fontStyle = FontStyle.Bold,
            wordWrap = false,
            normal = { textColor = countColor }
        };
        SetTextColorStates(countStyle, countColor);

        var statusStyle = new GUIStyle(labelStyle)
        {
            alignment = TextAnchor.MiddleCenter
        };
        SetTextColorStates(statusStyle, labelColor);

        GUI.Label(labelRect, GetStageLabel(), labelStyle);
        if (GUI.Button(labelRect, GUIContent.none, labelStyle))
        {
            OpenStageSelect();
        }

        GUI.Label(countRect, $"{_placedBoxes.Count} / {CurrentTargetBoxes}", countStyle);
        DrawHudProgress(progressRect, _placedBoxes.Count / (float)CurrentTargetBoxes);
        GUI.Label(statusRect, GetHudStatusLabel(), statusStyle);

        if (_state == PrototypeState.StageSelect)
        {
            DrawStageSelectOverlay();
            return;
        }

        DrawResultPopup();
    }

    private void DrawHudProgress(Rect rect, float progress)
    {
        DrawHudCapsule(rect, new Color(0.90f, 0.78f, 0.58f, 0.75f));

        float fillWidth = rect.width * Mathf.Clamp01(progress);
        if (fillWidth <= 0.5f)
        {
            return;
        }

        fillWidth = Mathf.Max(rect.height, fillWidth);
        var fillRect = new Rect(rect.x, rect.y, fillWidth, rect.height);
        DrawHudCapsule(fillRect, new Color(0.58f, 0.38f, 0.18f, 0.95f));
    }

    private string GetHudStatusLabel()
    {
        switch (_state)
        {
            case PrototypeState.ResolvingDrop:
                return "낙하";
            case PrototypeState.ValidatingClear:
                return "검수";
            case PrototypeState.StageSelect:
                return "선택";
            case PrototypeState.Won:
                return "완료";
            case PrototypeState.Failed:
                return "실패";
            default:
                return "진행";
        }
    }

    private string GetStageLabel()
    {
        return $"ST {CurrentStage.Number:00}";
    }

    private static int GetHudFontSize(string text, float maxWidth)
    {
        var content = new GUIContent(text);
        var measuringStyle = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            wordWrap = false
        };

        for (int fontSize = HudMaxFontSize; fontSize > HudMinFontSize; fontSize -= 2)
        {
            measuringStyle.fontSize = fontSize;
            if (measuringStyle.CalcSize(content).x <= maxWidth)
            {
                return fontSize;
            }
        }

        return HudMinFontSize;
    }

    private static void SetTextColorStates(GUIStyle style, Color color)
    {
        style.normal.textColor = color;
        style.hover.textColor = color;
        style.active.textColor = color;
        style.focused.textColor = color;
        style.onNormal.textColor = color;
        style.onHover.textColor = color;
        style.onActive.textColor = color;
        style.onFocused.textColor = color;
    }

    private void EnsureHudStyles()
    {
        if (_hudPillStyle != null)
        {
            return;
        }

        _hudPillTexture = CreateRoundedRectTexture(new Color(1f, 1f, 1f, 0.86f), new Color(0.73f, 0.52f, 0.28f, 0.72f), 4);
        _hudPillShadowTexture = CreateRoundedRectTexture(new Color(0.04f, 0.06f, 0.08f, 0.22f), new Color(0f, 0f, 0f, 0f));
        _hudProgressTrackTexture = CreateRoundedRectTexture(new Color(1f, 1f, 1f, 0.58f), new Color(0.12f, 0.18f, 0.22f, 0.18f));
        _hudProgressFillTexture = CreateRoundedRectTexture(new Color(0.58f, 0.38f, 0.18f, 0.92f), new Color(1f, 1f, 1f, 0.2f));
        _hudProgressCapTexture = CreateCircleTexture(Color.white);
        _resultPanelTexture = CreateRoundedRectTexture(new Color(1f, 1f, 1f, 0.94f), new Color(0.73f, 0.52f, 0.28f, 0.58f), 4);
        _resultButtonTexture = CreateRoundedRectTexture(new Color(0.58f, 0.38f, 0.18f, 0.96f), new Color(1f, 1f, 1f, 0.2f));

        _hudPillStyle = CreateHudBoxStyle(_hudPillTexture);
        _hudPillShadowStyle = CreateHudBoxStyle(_hudPillShadowTexture);
        _hudProgressTrackStyle = CreateHudBoxStyle(_hudProgressTrackTexture);
        _hudProgressFillStyle = CreateHudBoxStyle(_hudProgressFillTexture);
        _resultPanelStyle = CreateHudBoxStyle(_resultPanelTexture);
        _resultButtonStyle = CreateHudBoxStyle(_resultButtonTexture, 24);
        _resultButtonStyle.alignment = TextAnchor.MiddleCenter;
        _resultButtonStyle.fontStyle = FontStyle.Bold;
        _resultButtonStyle.fontSize = 20;
        _resultButtonStyle.normal.textColor = Color.white;
        _resultButtonStyle.hover.textColor = Color.white;
        _resultButtonStyle.active.textColor = Color.white;
    }

    private void DrawStageSelectOverlay()
    {
        Color previousColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.28f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previousColor;

        float panelWidth = Mathf.Max(300f, Mathf.Min(420f, Screen.width - 36f));
        float panelHeight = Mathf.Max(380f, Mathf.Min(520f, Screen.height - GetHudTopY() - HudBarHeight - 48f));
        float panelY = Mathf.Clamp(
            GetHudTopY() + HudBarHeight + 20f,
            18f,
            Screen.height - panelHeight - 18f);
        var panelRect = new Rect(
            (Screen.width - panelWidth) * 0.5f,
            panelY,
            panelWidth,
            panelHeight);

        GUI.Box(panelRect, GUIContent.none, _resultPanelStyle);

        Color titleColor = new Color(0.09f, 0.11f, 0.14f);
        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            clipping = TextClipping.Clip,
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            normal = { textColor = titleColor }
        };
        SetTextColorStates(titleStyle, titleColor);

        Color bodyColor = new Color(0.38f, 0.43f, 0.50f);
        var bodyStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            clipping = TextClipping.Clip,
            fontSize = 14,
            fontStyle = FontStyle.Normal,
            normal = { textColor = bodyColor }
        };
        SetTextColorStates(bodyStyle, bodyColor);

        var normalStageButtonStyle = new GUIStyle(_hudPillStyle)
        {
            alignment = TextAnchor.MiddleCenter,
            clipping = TextClipping.Clip,
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };
        SetTextColorStates(normalStageButtonStyle, new Color(0.18f, 0.14f, 0.10f));

        var selectedStageButtonStyle = new GUIStyle(_resultButtonStyle)
        {
            alignment = TextAnchor.MiddleCenter,
            clipping = TextClipping.Clip,
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };
        SetTextColorStates(selectedStageButtonStyle, Color.white);

        var titleRect = new Rect(panelRect.x + 24f, panelRect.y + 20f, panelRect.width - 48f, 32f);
        var bodyRect = new Rect(panelRect.x + 24f, panelRect.y + 54f, panelRect.width - 48f, 22f);
        GUI.Label(titleRect, "스테이지 선택", titleStyle);
        GUI.Label(bodyRect, "배송 루트", bodyStyle);

        const int columns = 4;
        const float gap = 8f;
        float gridX = panelRect.x + 24f;
        float gridY = panelRect.y + 92f;
        float gridWidth = panelRect.width - 48f;
        float cellWidth = (gridWidth - (gap * (columns - 1))) / columns;
        float cellHeight = Mathf.Clamp((panelRect.height - 174f - (gap * 4f)) / 5f, 44f, 58f);

        for (int i = 0; i < StageConfigs.Length; i++)
        {
            StageConfig stage = StageConfigs[i];
            int column = i % columns;
            int row = i / columns;
            var cellRect = new Rect(
                gridX + (column * (cellWidth + gap)),
                gridY + (row * (cellHeight + gap)),
                cellWidth,
                cellHeight);

            GUIStyle cellStyle = i == _currentStageIndex ? selectedStageButtonStyle : normalStageButtonStyle;
            string label = $"ST {stage.Number:00}\n{stage.TargetBoxes}개";
            if (GUI.Button(cellRect, label, cellStyle))
            {
                SelectStage(i);
            }
        }

        var closeRect = new Rect(panelRect.x + 72f, panelRect.yMax - 66f, panelRect.width - 144f, 46f);
        if (GUI.Button(closeRect, "닫기", _resultButtonStyle))
        {
            CloseStageSelect();
        }
    }

    private void DrawResultPopup()
    {
        if (!IsResultState())
        {
            return;
        }

        bool won = _state == PrototypeState.Won;
        bool hasNextStage = won && HasNextStage();
        string title = won ? hasNextStage ? "배송 완료!" : "전체 배송 완료!" : "배송 실패";
        string body = GetResultBody(won);
        string hint = GetResultButtonLabel(won, hasNextStage);

        Color previousColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.28f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previousColor;

        float panelWidth = Mathf.Max(240f, Mathf.Min(ResultPanelMaxWidth, Screen.width - 48f));
        float panelHeight = Mathf.Max(220f, Mathf.Min(ResultPanelHeight, Screen.height - 160f));
        float panelY = Mathf.Clamp(
            Mathf.Max(GetHudTopY() + HudBarHeight + 28f, (Screen.height - panelHeight) * 0.5f),
            24f,
            Screen.height - panelHeight - 24f);
        var panelRect = new Rect(
            (Screen.width - panelWidth) * 0.5f,
            panelY,
            panelWidth,
            panelHeight);

        GUI.Box(panelRect, GUIContent.none, _resultPanelStyle);

        Color titleColor = new Color(0.09f, 0.11f, 0.14f);
        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            clipping = TextClipping.Clip,
            fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.width * 0.065f), 26, 34),
            fontStyle = FontStyle.Bold,
            normal = { textColor = titleColor }
        };
        SetTextColorStates(titleStyle, titleColor);

        Color bodyColor = new Color(0.38f, 0.43f, 0.50f);
        var bodyStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            clipping = TextClipping.Clip,
            fontSize = 18,
            fontStyle = FontStyle.Normal,
            wordWrap = true,
            normal = { textColor = bodyColor }
        };
        SetTextColorStates(bodyStyle, bodyColor);

        var titleRect = new Rect(panelRect.x + 24f, panelRect.y + 38f, panelRect.width - 48f, 42f);
        var bodyRect = new Rect(panelRect.x + 32f, panelRect.y + 96f, panelRect.width - 64f, 52f);
        var buttonRect = new Rect(panelRect.x + 54f, panelRect.yMax - 78f, panelRect.width - 108f, 54f);

        GUI.Label(titleRect, title, titleStyle);
        GUI.Label(bodyRect, body, bodyStyle);

        if (GUI.Button(buttonRect, hint, _resultButtonStyle))
        {
            HandleResultButton(won, hasNextStage);
        }
    }

    private bool IsResultState()
    {
        return _state == PrototypeState.Won || _state == PrototypeState.Failed;
    }

    private string GetResultBody(bool won)
    {
        if (won)
        {
            if (HasNextStage())
            {
                return $"스테이지 {CurrentStage.Number} 클리어";
            }

            return "20스테이지를 모두 클리어했어요";
        }

        if (_statusText == "STACK CROOKED")
        {
            return "한 줄로 쌓이지 않았어요";
        }

        return "박스가 떨어졌어요";
    }

    private string GetResultButtonLabel(bool won, bool hasNextStage)
    {
        if (!won)
        {
            return "다시 도전";
        }

        return hasNextStage ? "다음 스테이지" : "처음부터";
    }

    private void HandleResultButton(bool won, bool hasNextStage)
    {
        if (won && hasNextStage)
        {
            ChangeStage(1);
            return;
        }

        if (won)
        {
            _currentStageIndex = 0;
        }

        RestartGame();
    }

    private void DrawHudCapsule(Rect rect, Color color)
    {
        if (rect.width <= 0f || rect.height <= 0f)
        {
            return;
        }

        Color previousColor = GUI.color;
        GUI.color = color;

        float capSize = rect.height;
        float centerWidth = Mathf.Max(0f, rect.width - capSize);
        GUI.DrawTexture(new Rect(rect.x + (capSize * 0.5f), rect.y, centerWidth, rect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.y, capSize, capSize), _hudProgressCapTexture);
        GUI.DrawTexture(new Rect(rect.xMax - capSize, rect.y, capSize, capSize), _hudProgressCapTexture);

        GUI.color = previousColor;
    }

    private static GUIStyle CreateHudBoxStyle(Texture2D texture, int sliceBorder = 24)
    {
        return new GUIStyle
        {
            normal = { background = texture },
            border = new RectOffset(sliceBorder, sliceBorder, sliceBorder, sliceBorder)
        };
    }

    private static float GetHudTopY()
    {
        Rect safeArea = Screen.safeArea;
        float topInset = Screen.height - safeArea.yMax;
        return Mathf.Max(HudTopY, topInset + 12f);
    }

    private static Texture2D CreateRoundedRectTexture(Color fill, Color border, int borderSize = 2)
    {
        const int size = 64;
        const int radius = 28;
        borderSize = Mathf.Clamp(borderSize, 1, radius - 1);
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool insideOuter = PixelInsideRoundedRect(x, y, size, size, radius);
                bool insideInner = PixelInsideRoundedRect(x - borderSize, y - borderSize, size - (borderSize * 2), size - (borderSize * 2), radius - borderSize);
                texture.SetPixel(x, y, insideOuter ? insideInner ? fill : border : Color.clear);
            }
        }

        texture.Apply();
        return texture;
    }

    private static Texture2D CreateCircleTexture(Color color)
    {
        const int size = 32;
        const float radius = size * 0.5f;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) - radius;
                float dy = (y + 0.5f) - radius;
                texture.SetPixel(x, y, (dx * dx) + (dy * dy) <= radius * radius ? color : Color.clear);
            }
        }

        texture.Apply();
        return texture;
    }

    private static bool PixelInsideRoundedRect(int x, int y, int width, int height, int radius)
    {
        if (width <= 0 || height <= 0)
        {
            return false;
        }

        float px = x + 0.5f;
        float py = y + 0.5f;
        float nearestX = Mathf.Clamp(px, radius, width - radius);
        float nearestY = Mathf.Clamp(py, radius, height - radius);
        float dx = px - nearestX;
        float dy = py - nearestY;
        return (dx * dx) + (dy * dy) <= radius * radius;
    }

    private static Camera EnsureCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            var cameraObject = new GameObject("Prototype Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
        }

        camera.orthographic = true;
        camera.orthographicSize = 4.7f;
        camera.transform.position = new Vector3(0f, CameraInitialY, -10f);
        camera.transform.rotation = Quaternion.identity;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = ParcelBrownBackgroundColor;
        return camera;
    }

    private void CreateFloor()
    {
        var floor = new GameObject("Prototype 2D Floor");
        floor.transform.position = new Vector3(0f, FloorY, 0f);

        var visual = new GameObject("Visual");
        visual.transform.SetParent(floor.transform, false);

        var renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = _floorSprite;
        renderer.sortingOrder = -5;
        FitSpriteToWorldSize(visual.transform, _floorSprite, new Vector2(6.2f, 0.35f));

        var collider = floor.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(6.2f, FloorHeight);

        UpdateMinimumCameraY();
    }

    private void RestartGame()
    {
        StopAllCoroutines();

        if (_activeBox != null)
        {
            Destroy(_activeBox);
        }

        if (_droppingBox != null)
        {
            Destroy(_droppingBox);
        }

        foreach (GameObject box in _placedBoxes)
        {
            if (box != null)
            {
                Destroy(box);
            }
        }

        _placedBoxes.Clear();
        _activeBox = null;
        _droppingBox = null;
        _cameraVelocityY = 0f;
        _clearValidationEndTime = 0f;
        _attempts++;
        _state = PrototypeState.Playing;
        _statusText = $"RUN {_attempts}";
        SpawnNextBox();
    }

    private void ChangeStage(int direction)
    {
        int nextStageIndex = Mathf.Clamp(_currentStageIndex + direction, 0, StageConfigs.Length - 1);
        if (nextStageIndex == _currentStageIndex)
        {
            return;
        }

        _currentStageIndex = nextStageIndex;
        RestartGame();
    }

    private void SelectStage(int stageIndex)
    {
        stageIndex = Mathf.Clamp(stageIndex, 0, StageConfigs.Length - 1);
        CloseStageSelect();

        if (stageIndex != _currentStageIndex)
        {
            _currentStageIndex = stageIndex;
        }

        RestartGame();
    }

    private void OpenStageSelect()
    {
        if (_state == PrototypeState.StageSelect)
        {
            return;
        }

        _stateBeforeStageSelect = _state;
        _timeScaleBeforeStageSelect = Time.timeScale;
        Time.timeScale = 0f;
        _state = PrototypeState.StageSelect;
    }

    private void CloseStageSelect()
    {
        if (_state != PrototypeState.StageSelect)
        {
            return;
        }

        Time.timeScale = _timeScaleBeforeStageSelect <= 0f ? 1f : _timeScaleBeforeStageSelect;
        _state = _stateBeforeStageSelect;
    }

    private bool HasNextStage()
    {
        return _currentStageIndex < StageConfigs.Length - 1;
    }

    private void SpawnNextBox()
    {
        _spawnHeight = 0.45f + (_placedBoxes.Count * BoxSize) + 2.15f;
        _moveStartedAt = Time.time;

        _activeBox = CreatePrototypeBox($"Prototype Parcel Box {_placedBoxes.Count + 1}", _activeTint);
        _activeBox.transform.position = new Vector3(-CurrentMoveRange, _spawnHeight, 0f);
    }

    private GameObject CreatePrototypeBox(string boxName, Color tint)
    {
        BoxVisual boxVisual = GetBoxVisual(GetStageBoxCode(_placedBoxes.Count));
        var box = new GameObject(boxName);

        var visual = new GameObject("Visual");
        visual.transform.SetParent(box.transform, false);

        var renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = boxVisual.Sprite;
        renderer.color = boxVisual.IsPlaceholder ? tint : Color.white;
        renderer.sortingOrder = 10 + _placedBoxes.Count;
        FitSpriteToWorldSize(visual.transform, boxVisual.Sprite, boxVisual.VisibleTextureRect, boxVisual.WorldSize);

        var collider = box.AddComponent<BoxCollider2D>();
        collider.size = boxVisual.WorldSize * 0.96f;

        var body = box.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 1.6f;
        body.mass = 1f;
        body.linearDamping = 0.25f;
        body.angularDamping = 0.4f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        return box;
    }

    private void MoveActiveBox()
    {
        if (_activeBox == null)
        {
            return;
        }

        float elapsed = Time.time - _moveStartedAt;
        float x = Mathf.Sin(elapsed * CurrentMoveSpeed) * CurrentMoveRange;
        _activeBox.transform.position = new Vector3(x, _spawnHeight, 0f);
    }

    private void UpdateCamera()
    {
        if (_camera == null)
        {
            return;
        }

        float highestBoxY = GetHighestBoxY();
        GameObject focusBox = _activeBox != null ? _activeBox : _droppingBox;
        float activeY = focusBox != null ? focusBox.transform.position.y : highestBoxY + 1f;
        float targetY = Mathf.Max(_minimumCameraY, Mathf.Lerp(highestBoxY + CameraYOffset, activeY + 1.15f, 0.55f));
        float nextY = Mathf.SmoothDamp(_camera.transform.position.y, targetY, ref _cameraVelocityY, 0.18f);

        _camera.transform.position = new Vector3(0f, nextY, -10f);
        _camera.transform.rotation = Quaternion.identity;
        UpdateBackground();
    }

    private void UpdateMinimumCameraY()
    {
        if (_camera == null)
        {
            _minimumCameraY = CameraInitialY;
            return;
        }

        float floorBottomY = FloorY - (FloorHeight * 0.5f);
        _minimumCameraY = floorBottomY + _camera.orthographicSize - CameraBottomPadding;

        if (_camera.transform.position.y < _minimumCameraY)
        {
            _camera.transform.position = new Vector3(0f, _minimumCameraY, -10f);
        }
    }

    private float GetHighestBoxY()
    {
        float highest = 0.5f;

        for (int i = 0; i < _placedBoxes.Count; i++)
        {
            GameObject box = _placedBoxes[i];
            if (box != null)
            {
                highest = Mathf.Max(highest, box.transform.position.y);
            }
        }

        return highest;
    }

    private void DropActiveBox()
    {
        if (_activeBox == null)
        {
            return;
        }

        _state = PrototypeState.ResolvingDrop;
        _statusText = "DROP";

        var body = _activeBox.GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;

        StartCoroutine(ResolveDrop(_activeBox));
        _droppingBox = _activeBox;
        _activeBox = null;
    }

    private IEnumerator ResolveDrop(GameObject droppedBox)
    {
        yield return new WaitForSeconds(DropSettleSeconds);

        if (droppedBox == null || BoxIsLost(droppedBox))
        {
            _droppingBox = null;
            EndRun(false, "MISSED");
            yield break;
        }

        var renderer = droppedBox.GetComponentInChildren<SpriteRenderer>();
        if (renderer != null && IsPlaceholderSprite(renderer.sprite))
        {
            renderer.color = _placedTint;
        }

        _placedBoxes.Add(droppedBox);
        _droppingBox = null;

        if (!StackIsSingleColumn())
        {
            EndRun(false, "STACK CROOKED");
            yield break;
        }

        if (_placedBoxes.Count >= CurrentTargetBoxes)
        {
            BeginClearValidation();
            yield break;
        }

        _state = PrototypeState.Playing;
        _statusText = $"RUN {_attempts}";
        SpawnNextBox();
    }

    private void BeginClearValidation()
    {
        _state = PrototypeState.ValidatingClear;
        _statusText = "VERIFYING";
        _clearValidationEndTime = Time.time + ClearValidationSeconds;
    }

    private void UpdateClearValidation()
    {
        if (AnyBoxLost())
        {
            EndRun(false, "STACK LOST");
            return;
        }

        if (!StackIsSingleColumn())
        {
            EndRun(false, "STACK CROOKED");
            return;
        }

        if (Time.time < _clearValidationEndTime)
        {
            return;
        }

        EndRun(true, "STACK COMPLETE");
    }

    private void EndRun(bool won, string status)
    {
        if (_state == PrototypeState.Won || _state == PrototypeState.Failed)
        {
            return;
        }

        _state = won ? PrototypeState.Won : PrototypeState.Failed;
        _statusText = status;

        if (_activeBox != null)
        {
            Destroy(_activeBox);
            _activeBox = null;
        }

        _droppingBox = null;
    }

    private bool AnyBoxLost()
    {
        for (int i = 0; i < _placedBoxes.Count; i++)
        {
            if (BoxIsLost(_placedBoxes[i]))
            {
                return true;
            }
        }

        return false;
    }

    private bool StackIsSingleColumn()
    {
        if (_placedBoxes.Count == 0)
        {
            return true;
        }

        if (_placedBoxes[0] == null)
        {
            return false;
        }

        float referenceX = _placedBoxes[0].transform.position.x;
        for (int i = 1; i < _placedBoxes.Count; i++)
        {
            GameObject box = _placedBoxes[i];
            if (box == null || Mathf.Abs(box.transform.position.x - referenceX) > StackLineTolerance)
            {
                return false;
            }
        }

        return true;
    }

    private static bool BoxIsLost(GameObject box)
    {
        if (box == null)
        {
            return true;
        }

        Vector3 position = box.transform.position;
        return position.y < LostHeight || Mathf.Abs(position.x) > LostHorizontalDistance;
    }

    private void LoadPrototypeSprites()
    {
        _boxVisuals.Clear();

        for (int i = 0; i < BoxAssetDefinitions.Length; i++)
        {
            BoxAssetDefinition definition = BoxAssetDefinitions[i];
            Sprite sprite = LoadPrototypeSprite(definition.Name);
            if (sprite != null)
            {
                _boxVisuals.Add(new BoxVisual(sprite, definition.WorldSize, GetVisibleTextureRect(sprite), false));
                continue;
            }

            Sprite placeholder = CreateParcelBoxSprite();
            _boxVisuals.Add(new BoxVisual(placeholder, definition.WorldSize, GetVisibleTextureRect(placeholder), true));
        }

        if (_boxVisuals.Count == 0)
        {
            Sprite placeholder = CreateParcelBoxSprite();
            _boxVisuals.Add(new BoxVisual(placeholder, Vector2.one * BoxSize, GetVisibleTextureRect(placeholder), true));
        }

        _floorSprite = LoadPrototypeSprite(StackBaseSpriteName) ?? CreateSolidSprite(new Color(0.16f, 0.18f, 0.22f));
        _backgroundSprite = UseLogisticsCenterBackground
            ? LoadPrototypeSprite(BackgroundSpriteName, BackgroundResourceFolder, BackgroundAssetFolder)
            : null;
    }

    private char GetStageBoxCode(int boxIndex)
    {
        string sequence = CurrentStage.BoxSequence;
        if (string.IsNullOrEmpty(sequence))
        {
            return 'B';
        }

        return sequence[Mathf.Clamp(boxIndex, 0, sequence.Length - 1)];
    }

    private BoxVisual GetBoxVisual(char boxCode)
    {
        if (_boxVisuals.Count == 0)
        {
            Sprite placeholder = CreateParcelBoxSprite();
            return new BoxVisual(placeholder, Vector2.one * BoxSize, GetVisibleTextureRect(placeholder), true);
        }

        int visualIndex = GetBoxVisualIndex(boxCode);
        visualIndex = Mathf.Clamp(visualIndex, 0, _boxVisuals.Count - 1);
        return _boxVisuals[visualIndex];
    }

    private static int GetBoxVisualIndex(char boxCode)
    {
        switch (boxCode)
        {
            case 'W':
                return 1;
            case 'T':
                return 2;
            default:
                return 0;
        }
    }

    private bool IsPlaceholderSprite(Sprite sprite)
    {
        for (int i = 0; i < _boxVisuals.Count; i++)
        {
            BoxVisual visual = _boxVisuals[i];
            if (visual.IsPlaceholder && visual.Sprite == sprite)
            {
                return true;
            }
        }

        return false;
    }

    private static Sprite LoadPrototypeSprite(string assetName)
    {
        return LoadPrototypeSprite(assetName, ParcelResourceFolder, ParcelAssetFolder);
    }

    private static Sprite LoadPrototypeSprite(string assetName, string resourceFolder, string assetFolder)
    {
        Sprite sprite = Resources.Load<Sprite>($"{resourceFolder}/{assetName}");
        if (sprite != null)
        {
            return sprite;
        }

#if UNITY_EDITOR
        string assetPath = $"{assetFolder}/{assetName}.png";
        string absolutePath = Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length));
        if (File.Exists(absolutePath))
        {
            byte[] bytes = File.ReadAllBytes(absolutePath);
            var fileTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                name = assetName
            };

            if (ImageConversion.LoadImage(fileTexture, bytes))
            {
                return Sprite.Create(fileTexture, new Rect(0f, 0f, fileTexture.width, fileTexture.height), new Vector2(0.5f, 0.5f), fileTexture.width);
            }
        }

        sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite != null)
        {
            return sprite;
        }

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        if (texture != null)
        {
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width);
        }

        return null;
#else
        return null;
#endif
    }

    private void CreateBackground()
    {
        if (_camera == null || _backgroundSprite == null)
        {
            return;
        }

        _background = new GameObject("Prototype Logistics Center Background");
        _background.transform.SetParent(_camera.transform, false);

        var renderer = _background.AddComponent<SpriteRenderer>();
        renderer.sprite = _backgroundSprite;
        renderer.sortingOrder = -50;

        UpdateBackground();
    }

    private void UpdateBackground()
    {
        if (_camera == null || _background == null || _backgroundSprite == null)
        {
            return;
        }

        float height = _camera.orthographicSize * 2.08f;
        float width = height * _camera.aspect;
        FitSpriteToCoverWorldSize(_background.transform, _backgroundSprite, new Vector2(width, height));
        _background.transform.localPosition = new Vector3(0f, 0f, 10f);
    }

    private static Rect GetVisibleTextureRect(Sprite sprite)
    {
        if (sprite == null || sprite.texture == null)
        {
            return Rect.zero;
        }

        Rect spriteRect = sprite.rect;
        Texture2D texture = sprite.texture;

        try
        {
            Color32[] pixels = texture.GetPixels32();
            int textureWidth = texture.width;
            int minX = Mathf.CeilToInt(spriteRect.xMax);
            int minY = Mathf.CeilToInt(spriteRect.yMax);
            int maxX = Mathf.FloorToInt(spriteRect.xMin);
            int maxY = Mathf.FloorToInt(spriteRect.yMin);
            int startX = Mathf.FloorToInt(spriteRect.xMin);
            int startY = Mathf.FloorToInt(spriteRect.yMin);
            int endX = Mathf.CeilToInt(spriteRect.xMax);
            int endY = Mathf.CeilToInt(spriteRect.yMax);

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    if (pixels[(y * textureWidth) + x].a <= 12)
                    {
                        continue;
                    }

                    minX = Mathf.Min(minX, x);
                    minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x + 1);
                    maxY = Mathf.Max(maxY, y + 1);
                }
            }

            if (maxX > minX && maxY > minY)
            {
                return Rect.MinMaxRect(minX, minY, maxX, maxY);
            }
        }
        catch (UnityException)
        {
            return spriteRect;
        }

        return spriteRect;
    }

    private static void FitSpriteToWorldSize(Transform target, Sprite sprite, Vector2 worldSize)
    {
        FitSpriteToWorldSize(target, sprite, GetVisibleTextureRect(sprite), worldSize);
    }

    private static void FitSpriteToWorldSize(Transform target, Sprite sprite, Rect visibleTextureRect, Vector2 worldSize)
    {
        if (sprite == null)
        {
            target.localScale = Vector3.one;
            target.localPosition = Vector3.zero;
            return;
        }

        float pixelsPerUnit = sprite.pixelsPerUnit;
        Vector2 visibleSize = visibleTextureRect.size / pixelsPerUnit;
        if (visibleSize.x <= 0f || visibleSize.y <= 0f)
        {
            target.localScale = Vector3.one;
            target.localPosition = Vector3.zero;
            return;
        }

        Vector3 scale = new Vector3(worldSize.x / visibleSize.x, worldSize.y / visibleSize.y, 1f);
        Vector2 spriteCenter = sprite.rect.center;
        Vector2 visibleCenter = visibleTextureRect.center;
        Vector2 localVisibleOffset = (visibleCenter - spriteCenter) / pixelsPerUnit;

        target.localScale = scale;
        target.localPosition = new Vector3(-localVisibleOffset.x * scale.x, -localVisibleOffset.y * scale.y, 0f);
    }

    private static void FitSpriteToCoverWorldSize(Transform target, Sprite sprite, Vector2 worldSize)
    {
        if (sprite == null)
        {
            target.localScale = Vector3.one;
            return;
        }

        Vector2 spriteSize = sprite.rect.size / sprite.pixelsPerUnit;
        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            target.localScale = Vector3.one;
            return;
        }

        float scale = Mathf.Max(worldSize.x / spriteSize.x, worldSize.y / spriteSize.y);
        target.localScale = new Vector3(scale, scale, 1f);
    }

    private static Sprite CreateParcelBoxSprite()
    {
        const int size = 96;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point
        };

        Color cardboard = new Color(0.76f, 0.49f, 0.23f, 1f);
        Color side = new Color(0.68f, 0.39f, 0.18f, 1f);
        Color top = new Color(0.88f, 0.63f, 0.34f, 1f);
        Color tape = new Color(0.38f, 0.24f, 0.12f, 1f);
        Color edge = new Color(0.12f, 0.08f, 0.05f, 1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool outside = x < 3 || x > size - 4 || y < 3 || y > size - 4;
                bool topBand = y > size - 28;
                bool sideBand = x > size - 24;
                bool verticalTape = x >= 43 && x <= 52;
                bool horizontalTape = y >= 46 && y <= 53;
                bool diagonalTop = topBand && Mathf.Abs((x - 48) - ((y - 68) * 2)) < 3;

                Color pixel = cardboard;
                if (topBand)
                {
                    pixel = top;
                }
                else if (sideBand)
                {
                    pixel = side;
                }

                if (verticalTape || horizontalTape || diagonalTop)
                {
                    pixel = tape;
                }

                if (outside)
                {
                    pixel = edge;
                }

                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static Sprite CreateSolidSprite(Color color)
    {
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point
        };

        texture.SetPixel(0, 0, color);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }

    private static bool DropPressed()
    {
#if ENABLE_INPUT_SYSTEM
        bool keyboard = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool mouse = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool touch = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
        return keyboard || mouse || touch;
#else
        return Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
#endif
    }

    private static bool RestartPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.R);
#endif
    }

    private static bool PreviousStagePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null
            && (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.P);
#endif
    }

    private static bool NextStagePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null
            && (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.nKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.N);
#endif
    }

    private static bool CloseStageSelectPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}
