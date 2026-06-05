using UnityEngine;

internal sealed class BoxStackSceneComposition
{
    internal const float CameraInitialY = 2.5f;

    private const float CameraBottomPadding = 0.25f;
    private const float FloorY = -0.65f;
    private const float FloorHeight = 0.35f;
    private const float FloorWidth = 6.2f;
    private const float FloorVisualHeight = 0.35f;
    private static readonly Color DefaultBackgroundColor = new Color(0.12f, 0.18f, 0.35f);

    private readonly BoxStackPrototypeAssetLoader _assetLoader;
    private readonly PhysicsMaterial2D _floorPhysicsMaterial;
    private readonly bool _useStackLikeAbstractVisuals;

    private Sprite _floorSprite;
    private Sprite _backgroundSprite;
    private SpriteRenderer _floorRenderer;
    private SpriteRenderer _backgroundRenderer;
    private GameObject _background;

    internal BoxStackSceneComposition(
        BoxStackPrototypeAssetLoader assetLoader,
        PhysicsMaterial2D floorPhysicsMaterial,
        bool useStackLikeAbstractVisuals)
    {
        _assetLoader = assetLoader;
        _floorPhysicsMaterial = floorPhysicsMaterial;
        _useStackLikeAbstractVisuals = useStackLikeAbstractVisuals;
        MinimumCameraY = CameraInitialY;
    }

    internal Camera Camera { get; private set; }
    internal Collider2D FloorCollider { get; private set; }
    internal float MinimumCameraY { get; private set; }

    internal void Initialize(Sprite floorSprite, Sprite backgroundSprite)
    {
        _floorSprite = floorSprite;
        _backgroundSprite = backgroundSprite;

        Camera = EnsureCamera();
        if (_backgroundSprite != null)
        {
            CreateBackground();
        }

        CreateFloor();
    }

    internal void UpdateBackground()
    {
        if (Camera == null || _background == null || _backgroundSprite == null)
        {
            return;
        }

        float height = Camera.orthographicSize * 2.08f;
        float width = height * Camera.aspect;
        FitSpriteToCoverWorldSize(_background.transform, _backgroundSprite, new Vector2(width, height));
        _background.transform.localPosition = new Vector3(0f, 0f, 10f);
    }

    internal void ApplyPalette(BoxStackPrototypePalette palette)
    {
        if (Camera != null)
        {
            Camera.backgroundColor = palette.BackgroundBottom;
        }

        if (!_useStackLikeAbstractVisuals)
        {
            return;
        }

        _backgroundSprite = _assetLoader.CreateStackGradientBackgroundSprite(
            palette.BackgroundBottom,
            palette.BackgroundMiddle,
            palette.BackgroundTop);
        if (_backgroundRenderer != null)
        {
            _backgroundRenderer.sprite = _backgroundSprite;
            UpdateBackground();
        }

        _floorSprite = _assetLoader.CreateStackFloorSprite(
            palette.FloorTop,
            palette.FloorBottom,
            Color.white);
        if (_floorRenderer != null)
        {
            _floorRenderer.sprite = _floorSprite;
            FitSpriteToWorldSize(_floorRenderer.transform, _floorSprite, _assetLoader.GetVisibleTextureRect(_floorSprite), new Vector2(FloorWidth, FloorVisualHeight));
        }
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
        camera.backgroundColor = DefaultBackgroundColor;
        if (Object.FindFirstObjectByType<AudioListener>() == null)
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
        FitSpriteToWorldSize(visual.transform, _floorSprite, _assetLoader.GetVisibleTextureRect(_floorSprite), new Vector2(FloorWidth, FloorVisualHeight));

        var collider = floor.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(FloorWidth, FloorHeight);
        collider.sharedMaterial = _floorPhysicsMaterial;
        FloorCollider = collider;

        UpdateMinimumCameraY();
    }

    private void CreateBackground()
    {
        if (Camera == null || _backgroundSprite == null)
        {
            return;
        }

        _background = new GameObject("Prototype Stack-Like Background");
        _background.transform.SetParent(Camera.transform, false);

        var renderer = _background.AddComponent<SpriteRenderer>();
        _backgroundRenderer = renderer;
        renderer.sprite = _backgroundSprite;
        renderer.sortingOrder = -50;

        UpdateBackground();
    }

    private void UpdateMinimumCameraY()
    {
        if (Camera == null)
        {
            MinimumCameraY = CameraInitialY;
            return;
        }

        float floorBottomY = FloorY - (FloorHeight * 0.5f);
        MinimumCameraY = floorBottomY + Camera.orthographicSize - CameraBottomPadding;

        if (Camera.transform.position.y < MinimumCameraY)
        {
            Camera.transform.position = new Vector3(0f, MinimumCameraY, -10f);
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
}
