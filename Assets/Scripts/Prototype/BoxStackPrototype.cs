// 프로토타입 - 제품 코드로 사용하지 않음
// 질문: 2D 물리 스택 루프가 Stack-like 추상 블록 게임 콘셉트와 모바일 플레이에 맞는가?
// 날짜: 2026-05-02

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public sealed class BoxStackPrototype : MonoBehaviour
{
    private const int PrototypeBuildNumber = 112;
    private static readonly bool UseStackLikeAbstractVisuals = true;
    private static readonly bool UseLogisticsCenterBackground = false;
    private const float BoxSize = 1.0f;
    private const float CameraYOffset = 2.2f;
    private readonly List<GameObject> _placedBoxes = new List<GameObject>();
    private readonly BoxStackPrototypeAssetLoader _assetLoader = new BoxStackPrototypeAssetLoader();
    private readonly BoxStackStageProgress _stageProgress = new BoxStackStageProgress(new BoxStackStageProgressStore());
    private readonly BoxStackRunRules _runRules = new BoxStackRunRules();
    private readonly BoxStackDropPhysics _dropPhysics = new BoxStackDropPhysics();
    private readonly BoxStackPrototypeUiStateFactory _uiStateFactory = new BoxStackPrototypeUiStateFactory();
    private readonly BoxStackActiveBoxMotion _activeBoxMotion = new BoxStackActiveBoxMotion();
    private readonly BoxStackClearValidation _clearValidation = new BoxStackClearValidation();
    private readonly BoxStackStageSelectSession _stageSelectSession = new BoxStackStageSelectSession();
    private readonly BoxStackResultFlow _resultFlow = new BoxStackResultFlow();
    private readonly BoxStackStageFlow _stageFlow = new BoxStackStageFlow();
    private readonly BoxStackRunCleanup _runCleanup = new BoxStackRunCleanup();
    private readonly BoxStackDroppedBoxSettlement _droppedBoxSettlement = new BoxStackDroppedBoxSettlement();
    private readonly BoxStackPostDropFlow _postDropFlow = new BoxStackPostDropFlow();
    private readonly BoxStackRunEndFlow _runEndFlow = new BoxStackRunEndFlow();

    private BoxStackPrototypeBoxVisualCatalog _boxVisualCatalog;
    private BoxStackBoxFactory _boxFactory;
    private BoxStackSceneComposition _sceneComposition;
    private BoxStackRuntimeHosts _runtimeHosts;
    private GameObject _activeBox;
    private GameObject _droppingBox;
    private Camera _camera;
    private Collider2D _floorCollider;
    private BoxStackPrototypeConfig _prototypeConfig;
    private PhysicsMaterial2D _parcelPhysicsMaterial;
    private PhysicsMaterial2D _floorPhysicsMaterial;
    private BoxStackPrototypeUi _prototypeUi;
    private BoxStackPrototypeAudio _prototypeAudio;
    private Color _activeTint = new Color(0.36f, 0.96f, 1.00f);
    private Color _placedTint = new Color(0.26f, 0.74f, 1.00f);
    private BoxStackPrototypePalette _currentPalette = BoxStackPrototypePalette.FromStageIndex(0);
    private BoxStackPrototypeState _state;
    private float _spawnHeight;
    private float _moveStartedAt;
    private float _minimumCameraY = BoxStackSceneComposition.CameraInitialY;
    private float _cameraVelocityY;
    private int _attempts;
    private bool _lastClearWasNewBest;
    private string _statusText = "READY";

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
            return GetStage(_stageProgress.CurrentStageIndex);
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
        _boxVisualCatalog = new BoxStackPrototypeBoxVisualCatalog(_assetLoader, BoxSize);
        _boxFactory = new BoxStackBoxFactory(_assetLoader, _boxVisualCatalog);
        LoadPrototypeConfig();
        LoadPrototypeSprites();
        CreatePhysicsMaterials();
        CreateSceneComposition();
        CreateRuntimeHosts();
        LoadStageProgress();
        ApplyCurrentStagePalette();
        EnsureRuntimeUi();
        EnsureRuntimeAudio();
        RestartGame();
    }

    private void LoadPrototypeConfig()
    {
        _prototypeConfig = _assetLoader.LoadConfig();
    }

    private void Update()
    {
        if (_state == BoxStackPrototypeState.StageSelect)
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

        if (_state == BoxStackPrototypeState.Playing)
        {
            MoveActiveBox();

            if (DropPressed())
            {
                DropActiveBox();
            }

            if (TryEvaluateStackFailure(null, out string status))
            {
                EndRun(false, status);
            }
        }
        else if (_state == BoxStackPrototypeState.ResolvingDrop)
        {
            if (TryEvaluateStackFailure(_droppingBox, out string status))
            {
                EndRun(false, status);
            }
        }
        else if (_state == BoxStackPrototypeState.ValidatingClear)
        {
            UpdateClearValidation();
        }

        UpdateCamera();
    }

    private void FixedUpdate()
    {
        if (_state == BoxStackPrototypeState.ResolvingDrop)
        {
            _dropPhysics.ResetVelocityBeforeStackContact(_droppingBox, _placedBoxes, Tuning);
            _dropPhysics.ClampFallSpeed(_droppingBox, Tuning);
        }
    }

    private void LateUpdate()
    {
        RefreshPrototypeUi();
    }

    private void EnsureRuntimeUi()
    {
        if (_prototypeUi != null)
        {
            return;
        }

        _runtimeHosts.EnsureUi(new BoxStackRuntimeHosts.UiCallbacks(
            OpenStageSelect,
            SelectStage,
            CloseStageSelect,
            ResetStageProgress,
            UnlockAllStagesForPlaytest,
            HandleCurrentResultButton));
        _prototypeUi = _runtimeHosts.Ui;
    }

    private void EnsureRuntimeAudio()
    {
        if (_prototypeAudio != null)
        {
            return;
        }

        _runtimeHosts.EnsureAudio();
        _prototypeAudio = _runtimeHosts.Audio;
    }

    private void RefreshPrototypeUi()
    {
        if (_prototypeUi == null)
        {
            return;
        }

        _prototypeUi.Refresh(_uiStateFactory.Create(new BoxStackPrototypeUiStateFactory.UiContext(
            PrototypeBuildNumber,
            StageCount,
            GetStage,
            _stageProgress.CurrentStageIndex,
            _stageProgress.HighestUnlockedStageIndex,
            _placedBoxes.Count,
            CurrentTargetBoxes,
            _state,
            _statusText,
            _lastClearWasNewBest,
            ShouldShowProgressTestControls(),
            _currentPalette)));
    }

    private void HandleCurrentResultButton()
    {
        BoxStackResultAction action = _resultFlow.GetResultButtonAction(_state, HasNextStage());
        if (action == BoxStackResultAction.None)
        {
            return;
        }

        if (action == BoxStackResultAction.AdvanceToNextStage)
        {
            ApplyStageFlowResult(_stageFlow.Move(_stageProgress, 1));
            return;
        }

        if (action == BoxStackResultAction.RestartFromFirstStage)
        {
            _stageProgress.SelectFirstStage();
        }

        RestartGame();
    }

    private static bool ShouldShowProgressTestControls()
    {
        return Application.isEditor || Debug.isDebugBuild;
    }

    private void RestartGame()
    {
        StopAllCoroutines();

        _runCleanup.ClearRunObjects(ref _activeBox, ref _droppingBox, _placedBoxes);
        _cameraVelocityY = 0f;
        _clearValidation.Reset();
        _attempts++;
        _lastClearWasNewBest = false;
        _state = BoxStackPrototypeState.Playing;
        _statusText = $"RUN {_attempts}";
        SpawnNextBox();
        RefreshPrototypeUi();
    }

    private void ChangeStage(int direction)
    {
        ApplyStageFlowResult(_stageFlow.Move(_stageProgress, direction));
    }

    private void SelectStage(int stageIndex)
    {
        BoxStackStageFlowResult result = _stageFlow.Select(_stageProgress, stageIndex, StageCount);
        if (!result.HasAction)
        {
            return;
        }

        CloseStageSelect();
        ApplyStageFlowResult(result);
    }

    private void OpenStageSelect()
    {
        if (_state == BoxStackPrototypeState.StageSelect)
        {
            return;
        }

        _stageSelectSession.Begin(_state, Time.timeScale);
        Time.timeScale = 0f;
        _state = BoxStackPrototypeState.StageSelect;
        RefreshPrototypeUi();
    }

    private void CloseStageSelect()
    {
        if (_state != BoxStackPrototypeState.StageSelect)
        {
            return;
        }

        _state = _stageSelectSession.End(out float restoredTimeScale);
        Time.timeScale = restoredTimeScale;
        RefreshPrototypeUi();
    }

    private bool HasNextStage()
    {
        return _stageProgress.HasNextStage(StageCount);
    }

    private void LoadStageProgress()
    {
        _stageProgress.Load(StageCount);
    }

    private int GetStageNumber(int stageIndex)
    {
        return GetStage(stageIndex).Number;
    }

    private void ResetStageProgress()
    {
        BoxStackStageFlowResult result = _stageFlow.Reset(_stageProgress, GetStageNumber);
        CloseStageSelect();
        ApplyStageFlowResult(result);
    }

    private void UnlockAllStagesForPlaytest()
    {
        ApplyStageFlowResult(_stageFlow.UnlockAll(_stageProgress, StageCount, GetStageNumber));
    }

    private void UnlockNextStage()
    {
        ApplyStageFlowResult(_stageFlow.UnlockNext(_stageProgress, StageCount, GetStageNumber));
    }

    private void ApplyStageFlowResult(BoxStackStageFlowResult result)
    {
        if (result.ShouldApplyPalette)
        {
            ApplyCurrentStagePalette();
        }

        if (result.ShouldRestartRun)
        {
            RestartGame();
            return;
        }

        if (result.ShouldRefreshUi)
        {
            RefreshPrototypeUi();
        }
    }

    private void SpawnNextBox()
    {
        _spawnHeight = 0.45f + (_placedBoxes.Count * BoxSize) + 2.15f;
        _moveStartedAt = Time.time;

        _activeTint = GetStackBlockTint(_placedBoxes.Count, true);
        _activeBox = _boxFactory.CreateBox(
            $"Prototype Stack Block {_placedBoxes.Count + 1}",
            CurrentStage.BoxSequence,
            _placedBoxes.Count,
            CurrentTargetBoxes,
            UseStackLikeAbstractVisuals,
            _activeTint,
            _currentPalette,
            _parcelPhysicsMaterial,
            Tuning);
        _activeBox.transform.position = new Vector3(0f, _spawnHeight, 0f);
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
        _activeBoxMotion.Move(
            _activeBox,
            _camera,
            _spawnHeight,
            _moveStartedAt,
            Time.time,
            CurrentStage,
            Tuning,
            BoxSize);
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
        _sceneComposition.UpdateBackground();
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

        _state = BoxStackPrototypeState.ResolvingDrop;
        _statusText = "DROP";

        _dropPhysics.BeginDrop(_activeBox, Tuning);

        _droppingBox = _activeBox;
        StartCoroutine(ResolveDrop(_droppingBox));
        _activeBox = null;
    }

    private IEnumerator ResolveDrop(GameObject droppedBox)
    {
        float resolveStartedAt = Time.time;
        float stableStartedAt = -1f;

        while (true)
        {
            if (_state != BoxStackPrototypeState.ResolvingDrop)
            {
                yield break;
            }

            if (TryEvaluateDroppedBoxFailure(droppedBox, out string status))
            {
                EndRun(false, status);
                yield break;
            }

            BoxStackPrototypeConfig.TuningSettings tuning = Tuning;
            float elapsed = Time.time - resolveStartedAt;
            if (elapsed >= tuning.DropMinimumResolveSeconds && _dropPhysics.StackMotionIsStable(droppedBox, _placedBoxes, tuning))
            {
                if (stableStartedAt < 0f)
                {
                    stableStartedAt = Time.time;
                }

                if (Time.time - stableStartedAt >= tuning.DropStableSeconds)
                {
                    break;
                }
            }
            else
            {
                stableStartedAt = -1f;
            }

            yield return null;
        }

        if (_state != BoxStackPrototypeState.ResolvingDrop)
        {
            yield break;
        }

        if (_runRules.BoxIsLost(droppedBox, Tuning))
        {
            EndRun(false, BoxStackRunRules.MissedStatus);
            yield break;
        }

        if (_droppedBoxSettlement.Settle(
            droppedBox,
            GetStackBlockTint(_placedBoxes.Count, false),
            _boxFactory,
            _placedBoxes,
            Tuning,
            out Color appliedPlacedTint))
        {
            _placedTint = appliedPlacedTint;
        }

        _droppingBox = null;

        if (StackHasMultipleFloorContacts(null))
        {
            EndRun(false, BoxStackRunRules.StackSpreadStatus);
            yield break;
        }

        PlayPlacementFeedback(_placedBoxes.Count - 1);

        ApplyPostDropFlowResult(_postDropFlow.GetNextAction(_placedBoxes.Count, CurrentTargetBoxes));
    }

    private void ApplyPostDropFlowResult(BoxStackPostDropAction action)
    {
        if (action == BoxStackPostDropAction.BeginClearValidation)
        {
            BeginClearValidation();
            return;
        }

        if (action == BoxStackPostDropAction.ContinuePlaying && _state == BoxStackPrototypeState.ResolvingDrop)
        {
            _state = BoxStackPrototypeState.Playing;
            _statusText = $"RUN {_attempts}";
            SpawnNextBox();
        }
    }

    private void BeginClearValidation()
    {
        _state = BoxStackPrototypeState.ValidatingClear;
        _statusText = "VERIFYING";
        _clearValidation.Begin(Time.time, Tuning.ClearValidationSeconds);
    }

    private void UpdateClearValidation()
    {
        if (TryEvaluateStackFailure(null, out string status))
        {
            EndRun(false, status);
            return;
        }

        if (!_clearValidation.IsComplete(Time.time))
        {
            return;
        }

        EndRun(true, "STACK COMPLETE");
    }

    private void EndRun(bool won, string status)
    {
        BoxStackRunEndResult result = _runEndFlow.GetEndResult(
            _state,
            won,
            status,
            _stageProgress.CurrentStageIndex,
            _stageProgress.HighestUnlockedStageIndex);
        if (!result.ShouldEnd)
        {
            return;
        }

        _state = result.State;
        _statusText = result.StatusText;
        _lastClearWasNewBest = result.LastClearWasNewBest;
        if (result.ShouldUnlockNextStage)
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
            if (status == BoxStackRunRules.StackSpreadStatus)
            {
                _placedBoxes.Add(_droppingBox);
            }
            else
            {
                Destroy(_droppingBox);
            }
        }

        _droppingBox = null;
        _dropPhysics.FreezePlacedBoxPhysics(_placedBoxes);
        PlayResultFeedback(won);
        RefreshPrototypeUi();
    }

    private void PlayPlacementFeedback(int placedBoxIndex)
    {
        if (_prototypeAudio != null)
        {
            _prototypeAudio.PlayPlacement(placedBoxIndex);
        }
    }

    private void PlayResultFeedback(bool won)
    {
        if (_prototypeAudio == null)
        {
            return;
        }

        if (won)
        {
            _prototypeAudio.PlayClear();
        }
        else
        {
            _prototypeAudio.PlayFailure();
        }
    }

    private bool TryEvaluateDroppedBoxFailure(GameObject droppedBox, out string status)
    {
        return _runRules.TryEvaluateDroppedBoxFailure(
            droppedBox,
            _placedBoxes,
            _floorCollider,
            Tuning,
            out status);
    }

    private bool TryEvaluateStackFailure(GameObject extraBox, out string status)
    {
        return _runRules.TryEvaluateStackFailure(
            _placedBoxes,
            extraBox,
            _floorCollider,
            Tuning,
            out status);
    }

    private bool StackHasMultipleFloorContacts(GameObject extraBox)
    {
        return _runRules.StackHasMultipleFloorContacts(_placedBoxes, extraBox, _floorCollider);
    }

    private void LoadPrototypeSprites()
    {
        _boxVisualCatalog.Load(UseLogisticsCenterBackground, UseStackLikeAbstractVisuals);
    }

    private void CreateSceneComposition()
    {
        _sceneComposition = new BoxStackSceneComposition(
            _assetLoader,
            _floorPhysicsMaterial,
            UseStackLikeAbstractVisuals);
        _sceneComposition.Initialize(_boxVisualCatalog.FloorSprite, _boxVisualCatalog.BackgroundSprite);
        _camera = _sceneComposition.Camera;
        _floorCollider = _sceneComposition.FloorCollider;
        _minimumCameraY = _sceneComposition.MinimumCameraY;
    }

    private void CreateRuntimeHosts()
    {
        _runtimeHosts = new BoxStackRuntimeHosts(_assetLoader, transform);
    }

    private Color GetStackBlockTint(int boxIndex, bool active)
    {
        return _currentPalette.GetBlockTint(boxIndex, CurrentTargetBoxes, active);
    }

    private void ApplyCurrentStagePalette()
    {
        _currentPalette = BoxStackPrototypePalette.FromStageIndex(_stageProgress.CurrentStageIndex);
        _sceneComposition.ApplyPalette(_currentPalette);
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

internal readonly struct BoxStackPrototypePalette
{
    internal BoxStackPrototypePalette(
        Color backgroundBottom,
        Color backgroundMiddle,
        Color backgroundTop,
        Color blockBase,
        Color blockAccent,
        Color floorBottom,
        Color floorTop,
        Color accent,
        Color secondaryAccent,
        Color panelBackground)
    {
        BackgroundBottom = backgroundBottom;
        BackgroundMiddle = backgroundMiddle;
        BackgroundTop = backgroundTop;
        BlockBase = blockBase;
        BlockAccent = blockAccent;
        FloorBottom = floorBottom;
        FloorTop = floorTop;
        Accent = accent;
        SecondaryAccent = secondaryAccent;
        PanelBackground = panelBackground;
    }

    internal Color BackgroundBottom { get; }
    internal Color BackgroundMiddle { get; }
    internal Color BackgroundTop { get; }
    internal Color BlockBase { get; }
    internal Color BlockAccent { get; }
    internal Color FloorBottom { get; }
    internal Color FloorTop { get; }
    internal Color Accent { get; }
    internal Color SecondaryAccent { get; }
    internal Color PanelBackground { get; }

    internal Color GetBlockTint(int boxIndex, int targetBoxes, bool active)
    {
        int maxIndex = Mathf.Max(1, targetBoxes - 1);
        float t = Mathf.Clamp01(boxIndex / (float)maxIndex);
        return GetStackGradientColor(t);
    }

    internal Color GetStackGradientColor(float progress)
    {
        float t = Mathf.Clamp01(progress);
        Color lowerTint = Color.Lerp(BackgroundTop, BlockAccent, 0.18f);
        lowerTint = Color.Lerp(lowerTint, Color.black, 0.10f);
        Color upperTint = Color.Lerp(BlockBase, Color.white, 0.42f);
        Color color = Color.Lerp(lowerTint, upperTint, t);
        color.a = 1f;
        return color;
    }

    internal static BoxStackPrototypePalette FromStageIndex(int stageIndex)
    {
        switch (Mathf.Abs(stageIndex) % 6)
        {
            case 1:
                return CreatePurplePink();
            case 2:
                return CreateSunsetCoral();
            case 3:
                return CreateMintTeal();
            case 4:
                return CreateIndigoCyan();
            case 5:
                return CreateLavenderBlue();
            default:
                return CreateAquaBlue();
        }
    }

    private static BoxStackPrototypePalette CreateAquaBlue()
    {
        return new BoxStackPrototypePalette(
            Hsv(0.47f, 0.30f, 0.98f),
            Hsv(0.52f, 0.58f, 0.72f),
            Hsv(0.61f, 0.72f, 0.42f),
            Hsv(0.54f, 0.78f, 0.82f),
            Hsv(0.46f, 0.72f, 1.00f),
            Hsv(0.58f, 0.60f, 0.26f, 0.82f),
            Hsv(0.49f, 0.44f, 1.00f, 0.96f),
            Hsv(0.49f, 0.70f, 1.00f, 0.90f),
            Hsv(0.16f, 0.70f, 1.00f, 0.96f),
            new Color(0.04f, 0.08f, 0.18f, 0.36f));
    }

    private static BoxStackPrototypePalette CreatePurplePink()
    {
        return new BoxStackPrototypePalette(
            Hsv(0.92f, 0.32f, 0.98f),
            Hsv(0.86f, 0.58f, 0.72f),
            Hsv(0.76f, 0.70f, 0.43f),
            Hsv(0.82f, 0.70f, 0.82f),
            Hsv(0.91f, 0.68f, 1.00f),
            Hsv(0.78f, 0.52f, 0.26f, 0.82f),
            Hsv(0.88f, 0.36f, 1.00f, 0.96f),
            Hsv(0.88f, 0.56f, 1.00f, 0.90f),
            Hsv(0.77f, 0.48f, 1.00f, 0.96f),
            new Color(0.10f, 0.06f, 0.18f, 0.38f));
    }

    private static BoxStackPrototypePalette CreateSunsetCoral()
    {
        return new BoxStackPrototypePalette(
            Hsv(0.12f, 0.28f, 0.98f),
            Hsv(0.04f, 0.58f, 0.74f),
            Hsv(0.00f, 0.72f, 0.43f),
            Hsv(0.04f, 0.76f, 0.84f),
            Hsv(0.11f, 0.76f, 1.00f),
            Hsv(0.04f, 0.58f, 0.28f, 0.82f),
            Hsv(0.11f, 0.42f, 1.00f, 0.96f),
            Hsv(0.08f, 0.70f, 1.00f, 0.90f),
            Hsv(0.14f, 0.66f, 1.00f, 0.96f),
            new Color(0.18f, 0.08f, 0.06f, 0.36f));
    }

    private static BoxStackPrototypePalette CreateMintTeal()
    {
        return new BoxStackPrototypePalette(
            Hsv(0.36f, 0.28f, 0.98f),
            Hsv(0.42f, 0.56f, 0.70f),
            Hsv(0.49f, 0.70f, 0.40f),
            Hsv(0.42f, 0.72f, 0.82f),
            Hsv(0.36f, 0.68f, 1.00f),
            Hsv(0.46f, 0.54f, 0.26f, 0.82f),
            Hsv(0.39f, 0.38f, 1.00f, 0.96f),
            Hsv(0.41f, 0.64f, 1.00f, 0.90f),
            Hsv(0.55f, 0.64f, 1.00f, 0.96f),
            new Color(0.04f, 0.14f, 0.13f, 0.36f));
    }

    private static BoxStackPrototypePalette CreateIndigoCyan()
    {
        return new BoxStackPrototypePalette(
            Hsv(0.52f, 0.32f, 0.98f),
            Hsv(0.57f, 0.60f, 0.70f),
            Hsv(0.65f, 0.74f, 0.42f),
            Hsv(0.60f, 0.74f, 0.82f),
            Hsv(0.52f, 0.72f, 1.00f),
            Hsv(0.63f, 0.58f, 0.25f, 0.82f),
            Hsv(0.53f, 0.42f, 1.00f, 0.96f),
            Hsv(0.53f, 0.76f, 1.00f, 0.90f),
            Hsv(0.69f, 0.48f, 1.00f, 0.96f),
            new Color(0.05f, 0.07f, 0.19f, 0.38f));
    }

    private static BoxStackPrototypePalette CreateLavenderBlue()
    {
        return new BoxStackPrototypePalette(
            Hsv(0.76f, 0.28f, 0.98f),
            Hsv(0.70f, 0.54f, 0.72f),
            Hsv(0.66f, 0.70f, 0.43f),
            Hsv(0.70f, 0.62f, 0.84f),
            Hsv(0.77f, 0.58f, 1.00f),
            Hsv(0.69f, 0.50f, 0.26f, 0.82f),
            Hsv(0.75f, 0.34f, 1.00f, 0.96f),
            Hsv(0.72f, 0.50f, 1.00f, 0.90f),
            Hsv(0.57f, 0.50f, 1.00f, 0.96f),
            new Color(0.08f, 0.07f, 0.20f, 0.38f));
    }

    private static Color Hsv(float h, float s, float v, float a = 1f)
    {
        Color color = Color.HSVToRGB(h, s, v);
        color.a = a;
        return color;
    }
}
