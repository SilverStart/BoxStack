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
    private const int PrototypeBuildNumber = 99;
    private static readonly bool UseStackLikeAbstractVisuals = true;
    private static readonly bool UseLogisticsCenterBackground = false;
    private const float BoxSize = 1.0f;
    private const float CameraYOffset = 2.2f;
    private const float CameraInitialY = 2.5f;
    private const float CameraBottomPadding = 0.25f;
    private const float FloorY = -0.65f;
    private const float FloorHeight = 0.35f;
    private readonly List<GameObject> _placedBoxes = new List<GameObject>();
    private readonly BoxStackPrototypeAssetLoader _assetLoader = new BoxStackPrototypeAssetLoader();
    private readonly BoxStackStageProgress _stageProgress = new BoxStackStageProgress(new BoxStackStageProgressStore());
    private readonly BoxStackRunRules _runRules = new BoxStackRunRules();
    private readonly BoxStackPrototypeUiStateFactory _uiStateFactory = new BoxStackPrototypeUiStateFactory();

    private BoxStackPrototypeBoxVisualCatalog _boxVisualCatalog;
    private GameObject _activeBox;
    private GameObject _droppingBox;
    private Camera _camera;
    private Sprite _floorSprite;
    private Sprite _backgroundSprite;
    private SpriteRenderer _floorRenderer;
    private SpriteRenderer _backgroundRenderer;
    private Collider2D _floorCollider;
    private BoxStackPrototypeConfig _prototypeConfig;
    private PhysicsMaterial2D _parcelPhysicsMaterial;
    private PhysicsMaterial2D _floorPhysicsMaterial;
    private bool _dropPreContactVelocityResetUsed;
    private GameObject _background;
    private BoxStackPrototypeUi _prototypeUi;
    private BoxStackPrototypeAudio _prototypeAudio;
    private Font _displayFont;
    private Font _bodyFont;
    private Font _fallbackFont;
    private Color _activeTint = new Color(0.36f, 0.96f, 1.00f);
    private Color _placedTint = new Color(0.26f, 0.74f, 1.00f);
    private BoxStackPrototypePalette _currentPalette = BoxStackPrototypePalette.FromStageIndex(0);
    private BoxStackPrototypeState _state;
    private BoxStackPrototypeState _stateBeforeStageSelect;
    private float _spawnHeight;
    private float _moveStartedAt;
    private float _minimumCameraY = CameraInitialY;
    private float _cameraVelocityY;
    private float _clearValidationEndTime;
    private float _timeScaleBeforeStageSelect = 1f;
    private int _attempts;
    private bool _lastClearWasNewBest;
    private string _statusText = "READY";

    private static readonly Color StackLikeBackgroundColor = new Color(0.12f, 0.18f, 0.35f);

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
        _boxVisualCatalog = new BoxStackPrototypeBoxVisualCatalog(_assetLoader, BoxSize);
        LoadPrototypeConfig();
        LoadPrototypeSprites();
        CreatePhysicsMaterials();
        _camera = EnsureCamera();
        if (_backgroundSprite != null)
        {
            CreateBackground();
        }

        CreateFloor();
        LoadStageProgress();
        ApplyCurrentStagePalette();
        EnsurePrototypeUi();
        EnsurePrototypeAudio();
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
            ResetDroppingBoxVelocityBeforeStackContact();
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

        _displayFont = _assetLoader.LoadDisplayFont();
        _bodyFont = _assetLoader.LoadBodyFont();
        _fallbackFont = _assetLoader.LoadKoreanFallbackFont();

        var uiObject = new GameObject("BoxStack Prototype UI");
        uiObject.transform.SetParent(transform, false);
        uiObject.SetActive(false);
        _prototypeUi = uiObject.AddComponent<BoxStackPrototypeUi>();
        _prototypeUi.Initialize(
            _displayFont,
            _bodyFont,
            _fallbackFont,
            OpenStageSelect,
            SelectStage,
            CloseStageSelect,
            ResetStageProgress,
            UnlockAllStagesForPlaytest,
            HandleCurrentResultButton);
    }

    private void EnsurePrototypeAudio()
    {
        if (_prototypeAudio != null)
        {
            return;
        }

        var audioObject = new GameObject("BoxStack Prototype Audio");
        audioObject.transform.SetParent(transform, false);
        _prototypeAudio = audioObject.AddComponent<BoxStackPrototypeAudio>();
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
        if (!IsResultState())
        {
            return;
        }

        bool won = _state == BoxStackPrototypeState.Won;
        HandleResultButton(won, won && HasNextStage());
    }

    private static bool ShouldShowProgressTestControls()
    {
        return Application.isEditor || Debug.isDebugBuild;
    }

    private bool IsResultState()
    {
        return _state == BoxStackPrototypeState.Won || _state == BoxStackPrototypeState.Failed;
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
            _stageProgress.SelectFirstStage();
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
        camera.backgroundColor = StackLikeBackgroundColor;
        if (FindFirstObjectByType<AudioListener>() == null)
        {
            camera.gameObject.AddComponent<AudioListener>();
        }

        return camera;
    }

    private void CreateFloor()
    {
        var floor = new GameObject("Prototype 2D Floor");
        floor.transform.position = new Vector3(0f, FloorY, 0f);

        var visual = new GameObject("Visual");
        visual.transform.SetParent(floor.transform, false);

        var renderer = visual.AddComponent<SpriteRenderer>();
        _floorRenderer = renderer;
        renderer.sprite = _floorSprite;
        renderer.sortingOrder = -5;
        FitSpriteToWorldSize(visual.transform, _floorSprite, _assetLoader.GetVisibleTextureRect(_floorSprite), new Vector2(6.2f, 0.35f));

        var collider = floor.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(6.2f, FloorHeight);
        collider.sharedMaterial = _floorPhysicsMaterial;
        _floorCollider = collider;

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
        _lastClearWasNewBest = false;
        _state = BoxStackPrototypeState.Playing;
        _statusText = $"RUN {_attempts}";
        SpawnNextBox();
        RefreshPrototypeUi();
    }

    private void ChangeStage(int direction)
    {
        if (!_stageProgress.TryMove(direction))
        {
            return;
        }

        ApplyCurrentStagePalette();
        RestartGame();
    }

    private void SelectStage(int stageIndex)
    {
        if (!_stageProgress.TrySelect(stageIndex, StageCount, out bool stageChanged))
        {
            return;
        }

        CloseStageSelect();

        if (stageChanged)
        {
            ApplyCurrentStagePalette();
        }

        RestartGame();
    }

    private void OpenStageSelect()
    {
        if (_state == BoxStackPrototypeState.StageSelect)
        {
            return;
        }

        _stateBeforeStageSelect = _state;
        _timeScaleBeforeStageSelect = Time.timeScale;
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

        Time.timeScale = _timeScaleBeforeStageSelect <= 0f ? 1f : _timeScaleBeforeStageSelect;
        _state = _stateBeforeStageSelect;
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
        _stageProgress.Reset(GetStageNumber);
        CloseStageSelect();
        ApplyCurrentStagePalette();
        RestartGame();
    }

    private void UnlockAllStagesForPlaytest()
    {
        _stageProgress.UnlockAll(StageCount, GetStageNumber);
        RefreshPrototypeUi();
    }

    private void UnlockNextStage()
    {
        _stageProgress.UnlockNextStage(StageCount, GetStageNumber);
    }

    private void SpawnNextBox()
    {
        _spawnHeight = 0.45f + (_placedBoxes.Count * BoxSize) + 2.15f;
        _moveStartedAt = Time.time;

        _activeTint = GetStackBlockTint(_placedBoxes.Count, true);
        _activeBox = CreatePrototypeBox($"Prototype Stack Block {_placedBoxes.Count + 1}", _activeTint);
        _activeBox.transform.position = new Vector3(0f, _spawnHeight, 0f);
    }

    private GameObject CreatePrototypeBox(string boxName, Color tint)
    {
        BoxStackPrototypeBoxVisual boxVisual = _boxVisualCatalog.GetVisual(CurrentStage.BoxSequence, _placedBoxes.Count);
        var box = new GameObject(boxName);

        var visual = new GameObject("Visual");
        visual.transform.SetParent(box.transform, false);

        var renderer = visual.AddComponent<SpriteRenderer>();
        bool useGeneratedStackVisual = UseStackLikeAbstractVisuals && boxVisual.IsPlaceholder;
        Sprite sprite = boxVisual.Sprite;
        Rect visibleTextureRect = boxVisual.VisibleTextureRect;
        Vector2 visualWorldSize = boxVisual.WorldSize;
        Vector2 colliderSize = boxVisual.WorldSize * 0.98f;
        Color rendererColor = boxVisual.IsPlaceholder ? tint : Color.white;
        if (useGeneratedStackVisual)
        {
            sprite = CreateStackBlockSprite(_placedBoxes.Count);
            visibleTextureRect = _assetLoader.GetVisibleTextureRect(sprite);
            rendererColor = Color.white;
        }

        renderer.sprite = sprite;
        renderer.color = rendererColor;
        renderer.sortingOrder = 10 + _placedBoxes.Count;
        FitSpriteToWorldSize(visual.transform, sprite, visibleTextureRect, visualWorldSize);

        var collider = box.AddComponent<BoxCollider2D>();
        collider.size = colliderSize;
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

    private Sprite CreateStackBlockSprite(int boxIndex)
    {
        int targetBoxes = Mathf.Max(1, CurrentTargetBoxes);
        float bottomProgress = Mathf.Clamp01(boxIndex / (float)targetBoxes);
        float topProgress = Mathf.Clamp01((boxIndex + 1) / (float)targetBoxes);
        return _assetLoader.CreateStackBlockPlaceholder(
            _currentPalette.GetStackGradientColor(bottomProgress),
            _currentPalette.GetStackGradientColor(topProgress));
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

        _state = BoxStackPrototypeState.ResolvingDrop;
        _statusText = "DROP";

        var body = _activeBox.GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = Tuning.DroppingGravityScale;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;

        _droppingBox = _activeBox;
        _dropPreContactVelocityResetUsed = false;
        StartCoroutine(ResolveDrop(_droppingBox));
        _activeBox = null;
    }

    private void ResetDroppingBoxVelocityBeforeStackContact()
    {
        if (_dropPreContactVelocityResetUsed || _droppingBox == null || _placedBoxes.Count == 0)
        {
            return;
        }

        float resetDistance = Tuning.DropPreContactVelocityResetDistance;
        if (resetDistance <= 0f)
        {
            return;
        }

        var body = _droppingBox.GetComponent<Rigidbody2D>();
        if (body == null || body.linearVelocity.y >= 0f)
        {
            return;
        }

        if (!_droppingBox.TryGetComponent(out BoxCollider2D droppingCollider))
        {
            return;
        }

        Bounds droppingBounds = droppingCollider.bounds;
        for (int i = 0; i < _placedBoxes.Count; i++)
        {
            GameObject placedBox = _placedBoxes[i];
            if (placedBox == null || !placedBox.TryGetComponent(out BoxCollider2D placedCollider))
            {
                continue;
            }

            Bounds placedBounds = placedCollider.bounds;
            bool horizontallyOverlaps = droppingBounds.min.x < placedBounds.max.x
                && droppingBounds.max.x > placedBounds.min.x;
            if (!horizontallyOverlaps)
            {
                continue;
            }

            float verticalGap = droppingBounds.min.y - placedBounds.max.y;
            if (verticalGap >= 0f && verticalGap <= resetDistance)
            {
                Vector2 velocity = body.linearVelocity;
                body.linearVelocity = new Vector2(velocity.x, 0f);
                _dropPreContactVelocityResetUsed = true;
                return;
            }
        }
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
            if (elapsed >= tuning.DropMinimumResolveSeconds && StackMotionIsStable(droppedBox))
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

        var renderer = droppedBox.GetComponentInChildren<SpriteRenderer>();
        if (renderer != null && _boxVisualCatalog.IsPlaceholderSprite(renderer.sprite))
        {
            _placedTint = GetStackBlockTint(_placedBoxes.Count, false);
            renderer.color = _placedTint;
        }

        var body = droppedBox.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.gravityScale = Tuning.SettledGravityScale;
        }

        if (!_placedBoxes.Contains(droppedBox))
        {
            _placedBoxes.Add(droppedBox);
        }

        _droppingBox = null;

        if (StackHasMultipleFloorContacts(null))
        {
            EndRun(false, BoxStackRunRules.StackSpreadStatus);
            yield break;
        }

        PlayPlacementFeedback(_placedBoxes.Count - 1);

        if (_placedBoxes.Count >= CurrentTargetBoxes)
        {
            BeginClearValidation();
            yield break;
        }

        if (_state != BoxStackPrototypeState.ResolvingDrop)
        {
            yield break;
        }

        _state = BoxStackPrototypeState.Playing;
        _statusText = $"RUN {_attempts}";
        SpawnNextBox();
    }

    private bool StackMotionIsStable(GameObject droppedBox)
    {
        if (!BoxMotionIsStable(droppedBox))
        {
            return false;
        }

        for (int i = 0; i < _placedBoxes.Count; i++)
        {
            if (!BoxMotionIsStable(_placedBoxes[i]))
            {
                return false;
            }
        }

        return true;
    }

    private bool BoxMotionIsStable(GameObject box)
    {
        if (box == null)
        {
            return false;
        }

        var body = box.GetComponent<Rigidbody2D>();
        if (body == null || body.IsSleeping())
        {
            return true;
        }

        BoxStackPrototypeConfig.TuningSettings tuning = Tuning;
        float linearThreshold = tuning.StackStopLinearVelocity;
        return body.linearVelocity.sqrMagnitude <= linearThreshold * linearThreshold
            && Mathf.Abs(body.angularVelocity) <= tuning.StackStopAngularVelocity;
    }

    private void BeginClearValidation()
    {
        _state = BoxStackPrototypeState.ValidatingClear;
        _statusText = "VERIFYING";
        _clearValidationEndTime = Time.time + Tuning.ClearValidationSeconds;
    }

    private void UpdateClearValidation()
    {
        if (TryEvaluateStackFailure(null, out string status))
        {
            EndRun(false, status);
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
        if (_state == BoxStackPrototypeState.Won || _state == BoxStackPrototypeState.Failed)
        {
            return;
        }

        _state = won ? BoxStackPrototypeState.Won : BoxStackPrototypeState.Failed;
        _statusText = status;
        _lastClearWasNewBest = won && _stageProgress.CurrentStageIndex >= _stageProgress.HighestUnlockedStageIndex;
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
        FreezePlacedBoxPhysics();
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
        _floorSprite = _boxVisualCatalog.FloorSprite;
        _backgroundSprite = _boxVisualCatalog.BackgroundSprite;
    }

    private void CreateBackground()
    {
        if (_camera == null || _backgroundSprite == null)
        {
            return;
        }

        _background = new GameObject("Prototype Stack-Like Background");
        _background.transform.SetParent(_camera.transform, false);

        var renderer = _background.AddComponent<SpriteRenderer>();
        _backgroundRenderer = renderer;
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

    private Color GetStackBlockTint(int boxIndex, bool active)
    {
        return _currentPalette.GetBlockTint(boxIndex, CurrentTargetBoxes, active);
    }

    private void ApplyCurrentStagePalette()
    {
        _currentPalette = BoxStackPrototypePalette.FromStageIndex(_stageProgress.CurrentStageIndex);

        if (_camera != null)
        {
            _camera.backgroundColor = _currentPalette.BackgroundBottom;
        }

        if (!UseStackLikeAbstractVisuals)
        {
            return;
        }

        _backgroundSprite = _assetLoader.CreateStackGradientBackgroundSprite(
            _currentPalette.BackgroundBottom,
            _currentPalette.BackgroundMiddle,
            _currentPalette.BackgroundTop);
        if (_backgroundRenderer != null)
        {
            _backgroundRenderer.sprite = _backgroundSprite;
            UpdateBackground();
        }

        _floorSprite = _assetLoader.CreateStackFloorSprite(
            _currentPalette.FloorTop,
            _currentPalette.FloorBottom,
            Color.white);
        if (_floorRenderer != null)
        {
            _floorRenderer.sprite = _floorSprite;
            FitSpriteToWorldSize(_floorRenderer.transform, _floorSprite, _assetLoader.GetVisibleTextureRect(_floorSprite), new Vector2(6.2f, 0.35f));
        }
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

        target.localScale = new Vector3(
            worldSize.x / spriteSize.x,
            worldSize.y / spriteSize.y,
            1f);
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
