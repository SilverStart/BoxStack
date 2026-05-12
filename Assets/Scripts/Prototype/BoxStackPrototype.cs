// 프로토타입 - 제품 코드로 사용하지 않음
// 질문: 2D/2.5D 택배 박스 쌓기 루프가 Toss 앱인앱 콘셉트와 AI PNG 에셋 파이프라인에 맞는가?
// 날짜: 2026-05-02

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
    private const string KoreanFontResourcePath = "Prototype/Fonts/NotoSansKR-VF";
    private const string PrototypeConfigResourcePath = "Prototype/BoxStackPrototypeConfig";
    private const string HighestUnlockedStageKey = "BoxStackPrototype.HighestUnlockedStage";
    private const int PrototypeBuildNumber = 16;
    private const string StackBaseSpriteName = "parcel_stack_base_01";
    private const string BackgroundSpriteName = "logistics_center_bg_01";
    private static readonly bool UseLogisticsCenterBackground = false;
    private const float BoxSize = 1.0f;
    private const float CameraYOffset = 2.2f;
    private const float CameraInitialY = 2.5f;
    private const float CameraBottomPadding = 0.25f;
    private const float FloorY = -0.65f;
    private const float FloorHeight = 0.35f;
    private readonly List<GameObject> _placedBoxes = new List<GameObject>();
    private readonly List<BoxVisual> _boxVisuals = new List<BoxVisual>();
    private readonly List<BoxSnapshot> _rescueSnapshot = new List<BoxSnapshot>();

    private GameObject _activeBox;
    private GameObject _droppingBox;
    private Camera _camera;
    private Sprite _floorSprite;
    private Sprite _backgroundSprite;
    private BoxStackPrototypeConfig _prototypeConfig;
    private PhysicsMaterial2D _parcelPhysicsMaterial;
    private PhysicsMaterial2D _floorPhysicsMaterial;
    private GameObject _background;
    private BoxStackPrototypeUi _prototypeUi;
    private Font _prototypeFont;
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
    private int _highestUnlockedStageIndex;
    private int _freeRescuesRemaining;
    private int _adRescuesRemaining;
    private bool _hasRescueSnapshot;
    private bool _rescueAdInProgress;
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

    private struct BoxSnapshot
    {
        public BoxSnapshot(GameObject box, Vector3 position, Quaternion rotation, RigidbodyType2D bodyType, Vector2 linearVelocity, float angularVelocity, bool simulated)
        {
            Box = box;
            Position = position;
            Rotation = rotation;
            BodyType = bodyType;
            LinearVelocity = linearVelocity;
            AngularVelocity = angularVelocity;
            Simulated = simulated;
        }

        public GameObject Box;
        public Vector3 Position;
        public Quaternion Rotation;
        public RigidbodyType2D BodyType;
        public Vector2 LinearVelocity;
        public float AngularVelocity;
        public bool Simulated;
    }

    private static readonly BoxAssetDefinition[] BoxAssetDefinitions =
    {
        new BoxAssetDefinition("parcel_box_basic_01", new Vector2(1.0f, 1.0f)),
        new BoxAssetDefinition("parcel_box_wide_01", new Vector2(1.18f, 0.88f)),
        new BoxAssetDefinition("parcel_box_tall_01", new Vector2(0.88f, 1.18f))
    };

    private static readonly Color ParcelBrownBackgroundColor = new Color(0.70f, 0.56f, 0.38f);

    private BoxStackPrototypeConfig.TuningSettings Tuning
    {
        get
        {
            return _prototypeConfig != null
                ? _prototypeConfig.Tuning
                : BoxStackPrototypeConfig.GetDefaultTuning();
        }
    }

    private int StageCount
    {
        get
        {
            return _prototypeConfig != null
                ? _prototypeConfig.StageCount
                : BoxStackPrototypeConfig.DefaultStageCount;
        }
    }

    private BoxStackPrototypeConfig.StageSettings CurrentStage
    {
        get
        {
            return GetStage(_currentStageIndex);
        }
    }

    private BoxStackPrototypeConfig.StageSettings GetStage(int stageIndex)
    {
        if (_prototypeConfig != null)
        {
            return _prototypeConfig.GetStage(stageIndex);
        }

        return BoxStackPrototypeConfig.GetDefaultStage(stageIndex);
    }

    private int CurrentTargetBoxes
    {
        get { return CurrentStage.TargetBoxes; }
    }

    private float CurrentMoveRange
    {
        get
        {
            return Mathf.Min(CurrentStageMoveRange, GetScreenSafeMoveRange());
        }
    }

    private float CurrentStageMoveRange
    {
        get { return Tuning.BaseMoveRange * CurrentStage.RangeMultiplier; }
    }

    private float CurrentMoveSpeed
    {
        get { return Tuning.BaseMoveSpeed * CurrentStage.SpeedMultiplier; }
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
        LoadPrototypeConfig();
        LoadPrototypeSprites();
        CreatePhysicsMaterials();
        _camera = EnsureCamera();
        if (UseLogisticsCenterBackground)
        {
            CreateBackground();
        }

        CreateFloor();
        LoadStageProgress();
        EnsurePrototypeUi();
        RestartGame();
    }

    private void LoadPrototypeConfig()
    {
        _prototypeConfig = Resources.Load<BoxStackPrototypeConfig>(PrototypeConfigResourcePath);
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

        if (UndoPressed() && UndoLastPlacedBox())
        {
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
            else if (!StackIsSingleColumn())
            {
                EndRun(false, "STACK CROOKED");
            }
        }
        else if (_state == PrototypeState.ResolvingDrop)
        {
            if (AnyBoxLost())
            {
                EndRun(false, "STACK LOST");
            }
            else if (!StackIsSingleColumn())
            {
                EndRun(false, "STACK CROOKED");
            }
        }
        else if (_state == PrototypeState.ValidatingClear)
        {
            UpdateClearValidation();
        }

        UpdateCamera();
    }

    private void FixedUpdate()
    {
        if (_state == PrototypeState.ResolvingDrop)
        {
            ClampDroppingBoxFallSpeed();
        }
    }

    private void LateUpdate()
    {
        RefreshPrototypeUi();
    }

    private void EnsurePrototypeUi()
    {
        if (_prototypeUi != null)
        {
            return;
        }

        _prototypeFont = Resources.Load<Font>(KoreanFontResourcePath);

        var uiObject = new GameObject("BoxStack Prototype UI");
        uiObject.transform.SetParent(transform, false);
        _prototypeUi = uiObject.AddComponent<BoxStackPrototypeUi>();
        _prototypeUi.Initialize(
            _prototypeFont,
            OpenStageSelect,
            HandleUndoButton,
            SelectStage,
            CloseStageSelect,
            ResetStageProgress,
            UnlockAllStagesForPlaytest,
            HandleCurrentResultButton);
    }

    private void RefreshPrototypeUi()
    {
        if (_prototypeUi == null)
        {
            return;
        }

        bool won = _state == PrototypeState.Won;
        bool resultVisible = IsResultState();
        bool hasNextStage = won && HasNextStage();
        var stages = new BoxStackPrototypeUi.StageButtonState[StageCount];
        for (int i = 0; i < StageCount; i++)
        {
            BoxStackPrototypeConfig.StageSettings stage = GetStage(i);
            stages[i] = new BoxStackPrototypeUi.StageButtonState(
                stage.Number,
                stage.TargetBoxes,
                IsStageUnlocked(i),
                i == _currentStageIndex);
        }

        _prototypeUi.Refresh(new BoxStackPrototypeUi.UiState(
            PrototypeBuildNumber,
            GetStageLabel(),
            _placedBoxes.Count,
            CurrentTargetBoxes,
            _placedBoxes.Count / (float)CurrentTargetBoxes,
            GetHudStatusLabel(),
            GetHudFeedbackLabel(),
            _state == PrototypeState.StageSelect,
            CanUseUndoSkill(),
            GetTotalRescuesRemaining(),
            resultVisible,
            resultVisible ? won ? hasNextStage ? "배송 완료!" : "전체 배송 완료!" : "배송 실패" : string.Empty,
            resultVisible ? GetResultBody(won) : string.Empty,
            resultVisible ? GetResultButtonLabel(won, hasNextStage) : string.Empty,
            ShouldShowProgressTestControls(),
            stages));
    }

    private void HandleCurrentResultButton()
    {
        if (!IsResultState())
        {
            return;
        }

        bool won = _state == PrototypeState.Won;
        HandleResultButton(won, won && HasNextStage());
    }

    private void HandleUndoButton()
    {
        UndoLastPlacedBox();
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
        return $"{CurrentStage.Number}단계";
    }

    private string GetHudFeedbackLabel()
    {
        switch (_state)
        {
            case PrototypeState.ResolvingDrop:
                return "좋아요";
            case PrototypeState.ValidatingClear:
                return "조심!";
            case PrototypeState.Won:
                return "배송 완료";
            case PrototypeState.Failed:
                return "적재 실패";
        }

        return _statusText == "UNDO" ? "되돌렸어요" : string.Empty;
    }

    private static bool ShouldShowProgressTestControls()
    {
        return Application.isEditor || Debug.isDebugBuild;
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
                return $"스테이지 {CurrentStage.Number} 클리어\n다음 스테이지가 열렸어요";
            }

            return "20스테이지를 모두 클리어했어요";
        }

        if (_statusText == "STACK CROOKED")
        {
            if (CanUseFailureRescue())
            {
                return GetFailureRescueBody();
            }

            return "한 줄로 쌓이지 않았어요";
        }

        if (CanUseFailureRescue())
        {
            return GetFailureRescueBody();
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

    private string GetFailureRescueBody()
    {
        if (_rescueAdInProgress)
        {
            return "광고 확인 후 방금 전으로 돌아가요";
        }

        return CanUseFreeFailureRescue() ? "한 번 되돌릴 수 있어요" : "광고 보고 한 번 더 이어갈 수 있어요";
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
        collider.sharedMaterial = _floorPhysicsMaterial;

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
        _rescueSnapshot.Clear();
        _activeBox = null;
        _droppingBox = null;
        _cameraVelocityY = 0f;
        _clearValidationEndTime = 0f;
        BoxStackPrototypeConfig.TuningSettings tuning = Tuning;
        _freeRescuesRemaining = tuning.FreeRescuesPerStage;
        _adRescuesRemaining = tuning.AdRescuesPerStage;
        _hasRescueSnapshot = false;
        _rescueAdInProgress = false;
        _attempts++;
        _state = PrototypeState.Playing;
        _statusText = $"RUN {_attempts}";
        SpawnNextBox();
        RefreshPrototypeUi();
    }

    private void ChangeStage(int direction)
    {
        int nextStageIndex = Mathf.Clamp(_currentStageIndex + direction, 0, _highestUnlockedStageIndex);
        if (nextStageIndex == _currentStageIndex)
        {
            return;
        }

        _currentStageIndex = nextStageIndex;
        RestartGame();
    }

    private void SelectStage(int stageIndex)
    {
        stageIndex = Mathf.Clamp(stageIndex, 0, StageCount - 1);
        if (!IsStageUnlocked(stageIndex))
        {
            return;
        }

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
        RefreshPrototypeUi();
    }

    private void CloseStageSelect()
    {
        if (_state != PrototypeState.StageSelect)
        {
            return;
        }

        Time.timeScale = _timeScaleBeforeStageSelect <= 0f ? 1f : _timeScaleBeforeStageSelect;
        _state = _stateBeforeStageSelect;
        RefreshPrototypeUi();
    }

    private bool HasNextStage()
    {
        return _currentStageIndex < StageCount - 1;
    }

    private bool IsStageUnlocked(int stageIndex)
    {
        return stageIndex <= _highestUnlockedStageIndex;
    }

    private void LoadStageProgress()
    {
        int unlockedStageNumber = PlayerPrefs.GetInt(HighestUnlockedStageKey, 1);
        _highestUnlockedStageIndex = Mathf.Clamp(unlockedStageNumber - 1, 0, StageCount - 1);
        _currentStageIndex = Mathf.Clamp(_currentStageIndex, 0, _highestUnlockedStageIndex);
    }

    private void SaveStageProgress()
    {
        PlayerPrefs.SetInt(HighestUnlockedStageKey, GetStage(_highestUnlockedStageIndex).Number);
        PlayerPrefs.Save();
    }

    private void ResetStageProgress()
    {
        _highestUnlockedStageIndex = 0;
        _currentStageIndex = 0;
        SaveStageProgress();
        CloseStageSelect();
        RestartGame();
    }

    private void UnlockAllStagesForPlaytest()
    {
        _highestUnlockedStageIndex = StageCount - 1;
        SaveStageProgress();
        RefreshPrototypeUi();
    }

    private void UnlockNextStage()
    {
        if (!HasNextStage() || _highestUnlockedStageIndex > _currentStageIndex)
        {
            return;
        }

        _highestUnlockedStageIndex = Mathf.Clamp(_currentStageIndex + 1, 0, StageCount - 1);
        SaveStageProgress();
    }

    private bool CanUseUndoSkill()
    {
        return _hasRescueSnapshot
            && _freeRescuesRemaining > 0
            && !_rescueAdInProgress
            && (_state == PrototypeState.Playing
                || _state == PrototypeState.ResolvingDrop
                || _state == PrototypeState.ValidatingClear);
    }

    private bool UndoLastPlacedBox()
    {
        return UseFreeFailureRescue();
    }

    private bool CanUseFailureRescue()
    {
        return false;
    }

    private bool CanUseFreeFailureRescue()
    {
        return CanUseUndoSkill();
    }

    private bool CanUseAdFailureRescue()
    {
        return false;
    }

    private int GetTotalRescuesRemaining()
    {
        return _freeRescuesRemaining + _adRescuesRemaining;
    }

    private bool UseFreeFailureRescue()
    {
        if (!CanUseFreeFailureRescue())
        {
            return false;
        }

        _freeRescuesRemaining--;
        return RestoreFailureRescueSnapshot();
    }

    private bool UseAdFailureRescue()
    {
        if (!CanUseAdFailureRescue())
        {
            return false;
        }

        _adRescuesRemaining--;
        return RestoreFailureRescueSnapshot();
    }

    private bool RestoreFailureRescueSnapshot()
    {
        StopAllCoroutines();
        _rescueAdInProgress = false;

        if (_activeBox != null)
        {
            Destroy(_activeBox);
            _activeBox = null;
        }

        if (_droppingBox != null && !_placedBoxes.Contains(_droppingBox))
        {
            Destroy(_droppingBox);
        }

        _droppingBox = null;

        for (int i = _placedBoxes.Count - 1; i >= _rescueSnapshot.Count; i--)
        {
            GameObject extraBox = _placedBoxes[i];
            _placedBoxes.RemoveAt(i);
            if (extraBox != null)
            {
                Destroy(extraBox);
            }
        }

        for (int i = 0; i < _rescueSnapshot.Count; i++)
        {
            BoxSnapshot snapshot = _rescueSnapshot[i];
            if (snapshot.Box == null)
            {
                continue;
            }

            if (i < _placedBoxes.Count)
            {
                _placedBoxes[i] = snapshot.Box;
            }

            snapshot.Box.transform.position = snapshot.Position;
            snapshot.Box.transform.rotation = snapshot.Rotation;

            var body = snapshot.Box.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.bodyType = snapshot.BodyType;
                body.simulated = snapshot.Simulated;
                body.linearVelocity = snapshot.LinearVelocity;
                body.angularVelocity = snapshot.AngularVelocity;
            }
        }

        _rescueSnapshot.Clear();
        _hasRescueSnapshot = false;
        _clearValidationEndTime = 0f;
        _state = PrototypeState.Playing;
        _statusText = "UNDO";
        _cameraVelocityY = 0f;
        SpawnNextBox();
        RefreshPrototypeUi();
        return true;
    }

    private IEnumerator MockRewardAdAndRescue()
    {
        if (_rescueAdInProgress || !CanUseAdFailureRescue())
        {
            yield break;
        }

        _rescueAdInProgress = true;
        yield return new WaitForSecondsRealtime(0.65f);

        UseAdFailureRescue();
    }

    private void SpawnNextBox()
    {
        _spawnHeight = 0.45f + (_placedBoxes.Count * BoxSize) + 2.15f;
        _moveStartedAt = Time.time;

        _activeBox = CreatePrototypeBox($"Prototype Parcel Box {_placedBoxes.Count + 1}", _activeTint);
        _activeBox.transform.position = new Vector3(0f, _spawnHeight, 0f);
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
        collider.sharedMaterial = _parcelPhysicsMaterial;

        var body = box.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        BoxStackPrototypeConfig.TuningSettings tuning = Tuning;
        body.gravityScale = tuning.SettledGravityScale;
        body.mass = 1f;
        body.linearDamping = tuning.LinearDamping;
        body.angularDamping = tuning.AngularDamping;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        return box;
    }

    private void CreatePhysicsMaterials()
    {
        BoxStackPrototypeConfig.TuningSettings tuning = Tuning;
        _parcelPhysicsMaterial = new PhysicsMaterial2D("Prototype Parcel Friction")
        {
            friction = tuning.ParcelFriction,
            bounciness = tuning.ParcelBounciness
        };
        _floorPhysicsMaterial = new PhysicsMaterial2D("Prototype Floor Friction")
        {
            friction = tuning.FloorFriction,
            bounciness = tuning.ParcelBounciness
        };
    }

    private void MoveActiveBox()
    {
        if (_activeBox == null)
        {
            return;
        }

        float elapsed = Time.time - _moveStartedAt;
        float moveRange = CurrentMoveRange;
        if (moveRange <= 0f)
        {
            _activeBox.transform.position = new Vector3(0f, _spawnHeight, 0f);
            return;
        }

        float rangeSpeedCompensation = CurrentStageMoveRange / moveRange;
        float cyclePosition = Mathf.Repeat(
            (elapsed * CurrentMoveSpeed * rangeSpeedCompensation / (2f * Mathf.PI)) + 0.25f,
            1f);
        float normalizedX = cyclePosition < 0.5f
            ? -1f + (cyclePosition * 4f)
            : 3f - (cyclePosition * 4f);
        float x = normalizedX * moveRange;
        _activeBox.transform.position = new Vector3(x, _spawnHeight, 0f);
    }

    private float GetScreenSafeMoveRange()
    {
        if (_camera == null)
        {
            return Tuning.BaseMoveRange;
        }

        float cameraHalfWidth = _camera.orthographicSize * _camera.aspect;
        return Mathf.Max(0f, cameraHalfWidth - GetActiveBoxHalfWidth() - Tuning.MoveRangeScreenPadding);
    }

    private float GetActiveBoxHalfWidth()
    {
        if (_activeBox != null && _activeBox.TryGetComponent(out BoxCollider2D collider))
        {
            return collider.size.x * Mathf.Abs(_activeBox.transform.lossyScale.x) * 0.5f;
        }

        return BoxSize * 0.5f;
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

        CaptureRescueSnapshot();
        _state = PrototypeState.ResolvingDrop;
        _statusText = "DROP";

        var body = _activeBox.GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = Tuning.DroppingGravityScale;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;

        StartCoroutine(ResolveDrop(_activeBox));
        _droppingBox = _activeBox;
        _activeBox = null;
    }

    private void ClampDroppingBoxFallSpeed()
    {
        if (_droppingBox == null)
        {
            return;
        }

        var body = _droppingBox.GetComponent<Rigidbody2D>();
        if (body == null)
        {
            return;
        }

        Vector2 velocity = body.linearVelocity;
        float maxDroppingFallSpeed = Tuning.MaxDroppingFallSpeed;
        if (velocity.y < -maxDroppingFallSpeed)
        {
            body.linearVelocity = new Vector2(velocity.x, -maxDroppingFallSpeed);
        }
    }

    private IEnumerator ResolveDrop(GameObject droppedBox)
    {
        yield return new WaitForSeconds(Tuning.DropSettleSeconds);

        if (droppedBox == null || BoxIsLost(droppedBox))
        {
            EndRun(false, "MISSED");
            yield break;
        }

        var renderer = droppedBox.GetComponentInChildren<SpriteRenderer>();
        if (renderer != null && IsPlaceholderSprite(renderer.sprite))
        {
            renderer.color = _placedTint;
        }

        var body = droppedBox.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.gravityScale = Tuning.SettledGravityScale;
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
        _clearValidationEndTime = Time.time + Tuning.ClearValidationSeconds;
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

    private void CaptureRescueSnapshot()
    {
        _rescueSnapshot.Clear();

        for (int i = 0; i < _placedBoxes.Count; i++)
        {
            GameObject box = _placedBoxes[i];
            if (box == null)
            {
                continue;
            }

            var body = box.GetComponent<Rigidbody2D>();
            if (body == null)
            {
                _rescueSnapshot.Add(new BoxSnapshot(box, box.transform.position, box.transform.rotation, RigidbodyType2D.Dynamic, Vector2.zero, 0f, true));
                continue;
            }

            _rescueSnapshot.Add(new BoxSnapshot(
                box,
                box.transform.position,
                box.transform.rotation,
                body.bodyType,
                body.linearVelocity,
                body.angularVelocity,
                body.simulated));
        }

        _hasRescueSnapshot = true;
    }

    private void EndRun(bool won, string status)
    {
        if (_state == PrototypeState.Won || _state == PrototypeState.Failed)
        {
            return;
        }

        _state = won ? PrototypeState.Won : PrototypeState.Failed;
        _statusText = status;
        if (won)
        {
            UnlockNextStage();
        }

        if (_activeBox != null)
        {
            Destroy(_activeBox);
            _activeBox = null;
        }

        if (_droppingBox != null && !_placedBoxes.Contains(_droppingBox))
        {
            Destroy(_droppingBox);
        }

        _droppingBox = null;
        FreezePlacedBoxPhysics();
        RefreshPrototypeUi();
    }

    private void FreezePlacedBoxPhysics()
    {
        for (int i = 0; i < _placedBoxes.Count; i++)
        {
            GameObject box = _placedBoxes[i];
            if (box == null)
            {
                continue;
            }

            var body = box.GetComponent<Rigidbody2D>();
            if (body == null)
            {
                continue;
            }

            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = false;
        }
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
            if (box == null || Mathf.Abs(box.transform.position.x - referenceX) > Tuning.StackLineTolerance)
            {
                return false;
            }
        }

        return true;
    }

    private bool BoxIsLost(GameObject box)
    {
        if (box == null)
        {
            return true;
        }

        Vector3 position = box.transform.position;
        BoxStackPrototypeConfig.TuningSettings tuning = Tuning;
        return position.y < tuning.LostHeight || Mathf.Abs(position.x) > tuning.LostHorizontalDistance;
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

        Texture2D resourceTexture = Resources.Load<Texture2D>($"{resourceFolder}/{assetName}");
        if (resourceTexture != null)
        {
            return CreateRuntimeSprite(assetName, resourceTexture);
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
                return CreateRuntimeSprite(assetName, fileTexture);
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
            return CreateRuntimeSprite(assetName, texture);
        }

        return null;
#else
        return null;
#endif
    }

    private static Sprite CreateRuntimeSprite(string assetName, Texture2D texture)
    {
        texture.name = assetName;
        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width);
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

    private bool DropPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null
            && Mouse.current.leftButton.wasPressedThisFrame
            && IsPointerInsideBlockingUi(Mouse.current.position.ReadValue()))
        {
            return false;
        }

        if (Touchscreen.current != null
            && Touchscreen.current.primaryTouch.press.wasPressedThisFrame
            && IsPointerInsideBlockingUi(Touchscreen.current.primaryTouch.position.ReadValue()))
        {
            return false;
        }

        bool keyboard = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool mouse = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool touch = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;
        return keyboard || mouse || touch;
#else
        if (Input.GetMouseButtonDown(0) && IsPointerInsideBlockingUi(Input.mousePosition))
        {
            return false;
        }

        return Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
#endif
    }

    private bool IsPointerInsideBlockingUi(Vector2 screenPosition)
    {
        return _prototypeUi != null && _prototypeUi.ContainsBlockingScreenPoint(screenPosition);
    }

    private static bool RestartPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.R);
#endif
    }

    private static bool UndoPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.uKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.U);
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
