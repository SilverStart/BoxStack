using UnityEngine;

internal sealed class BoxStackBoxFactory
{
    private readonly BoxStackPrototypeAssetLoader _assetLoader;
    private readonly BoxStackPrototypeBoxVisualCatalog _visualCatalog;

    internal BoxStackBoxFactory(
        BoxStackPrototypeAssetLoader assetLoader,
        BoxStackPrototypeBoxVisualCatalog visualCatalog)
    {
        _assetLoader = assetLoader;
        _visualCatalog = visualCatalog;
    }

    internal GameObject CreateBox(
        string boxName,
        string boxSequence,
        int boxIndex,
        int targetBoxes,
        bool useStackLikeAbstractVisuals,
        Color tint,
        BoxStackPrototypePalette palette,
        PhysicsMaterial2D parcelPhysicsMaterial,
        BoxStackPrototypeConfig.TuningSettings tuning)
    {
        BoxStackPrototypeBoxVisual boxVisual = _visualCatalog.GetVisual(boxSequence, boxIndex);
        var box = new GameObject(boxName);

        var visual = new GameObject("Visual");
        visual.transform.SetParent(box.transform, false);

        var renderer = visual.AddComponent<SpriteRenderer>();
        bool useGeneratedStackVisual = useStackLikeAbstractVisuals && boxVisual.IsPlaceholder;
        Sprite sprite = boxVisual.Sprite;
        Rect visibleTextureRect = boxVisual.VisibleTextureRect;
        Vector2 visualWorldSize = boxVisual.WorldSize;
        Vector2 colliderSize = boxVisual.WorldSize * 0.98f;
        Color rendererColor = boxVisual.IsPlaceholder ? tint : Color.white;
        if (useGeneratedStackVisual)
        {
            sprite = CreateStackBlockSprite(boxIndex, targetBoxes, palette);
            visibleTextureRect = _assetLoader.GetVisibleTextureRect(sprite);
            rendererColor = Color.white;
        }

        renderer.sprite = sprite;
        renderer.color = rendererColor;
        renderer.sortingOrder = 10 + boxIndex;
        FitSpriteToWorldSize(visual.transform, sprite, visibleTextureRect, visualWorldSize);

        var collider = box.AddComponent<BoxCollider2D>();
        collider.size = colliderSize;
        collider.sharedMaterial = parcelPhysicsMaterial;

        var body = box.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = tuning.SettledGravityScale;
        body.mass = 1f;
        body.linearDamping = tuning.LinearDamping;
        body.angularDamping = tuning.AngularDamping;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        return box;
    }

    internal bool TryApplyPlacedTint(GameObject box, Color tint)
    {
        if (box == null)
        {
            return false;
        }

        var renderer = box.GetComponentInChildren<SpriteRenderer>();
        if (renderer == null || !_visualCatalog.IsPlaceholderSprite(renderer.sprite))
        {
            return false;
        }

        renderer.color = tint;
        return true;
    }

    private Sprite CreateStackBlockSprite(
        int boxIndex,
        int targetBoxes,
        BoxStackPrototypePalette palette)
    {
        int safeTargetBoxes = Mathf.Max(1, targetBoxes);
        float bottomProgress = Mathf.Clamp01(boxIndex / (float)safeTargetBoxes);
        float topProgress = Mathf.Clamp01((boxIndex + 1) / (float)safeTargetBoxes);
        return _assetLoader.CreateStackBlockPlaceholder(
            palette.GetStackGradientColor(bottomProgress),
            palette.GetStackGradientColor(topProgress));
    }

    private static void FitSpriteToWorldSize(
        Transform target,
        Sprite sprite,
        Rect visibleTextureRect,
        Vector2 worldSize)
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
}
