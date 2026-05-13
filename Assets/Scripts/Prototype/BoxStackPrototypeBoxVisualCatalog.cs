// 프로토타입 박스 비주얼 카탈로그 - 스테이지 박스 코드와 실제 스프라이트 선택 규칙을 한곳에 모은다.

using System.Collections.Generic;
using UnityEngine;

internal readonly struct BoxStackPrototypeBoxVisual
{
    internal BoxStackPrototypeBoxVisual(Sprite sprite, Vector2 worldSize, Rect visibleTextureRect, bool isPlaceholder)
    {
        Sprite = sprite;
        WorldSize = worldSize;
        VisibleTextureRect = visibleTextureRect;
        IsPlaceholder = isPlaceholder;
    }

    internal Sprite Sprite { get; }
    internal Vector2 WorldSize { get; }
    internal Rect VisibleTextureRect { get; }
    internal bool IsPlaceholder { get; }
}

internal sealed class BoxStackPrototypeBoxVisualCatalog
{
    private const string StackBaseSpriteName = "parcel_stack_base_01";
    private const string BackgroundSpriteName = "logistics_center_bg_01";

    private static readonly BoxAssetDefinition[] BoxAssetDefinitions =
    {
        new BoxAssetDefinition("parcel_box_basic_01", new Vector2(1.0f, 1.0f)),
        new BoxAssetDefinition("parcel_box_wide_01", new Vector2(1.18f, 0.88f)),
        new BoxAssetDefinition("parcel_box_tall_01", new Vector2(0.88f, 1.18f))
    };

    private readonly List<BoxStackPrototypeBoxVisual> _boxVisuals = new List<BoxStackPrototypeBoxVisual>();
    private readonly BoxStackPrototypeAssetLoader _assetLoader;
    private readonly float _fallbackBoxSize;

    internal BoxStackPrototypeBoxVisualCatalog(BoxStackPrototypeAssetLoader assetLoader, float fallbackBoxSize)
    {
        _assetLoader = assetLoader;
        _fallbackBoxSize = fallbackBoxSize;
    }

    internal Sprite FloorSprite { get; private set; }
    internal Sprite BackgroundSprite { get; private set; }

    internal void Load(bool useLogisticsCenterBackground)
    {
        _boxVisuals.Clear();

        for (int i = 0; i < BoxAssetDefinitions.Length; i++)
        {
            BoxAssetDefinition definition = BoxAssetDefinitions[i];
            Sprite sprite = _assetLoader.LoadParcelSprite(definition.Name);
            if (sprite != null)
            {
                _boxVisuals.Add(new BoxStackPrototypeBoxVisual(sprite, definition.WorldSize, _assetLoader.GetVisibleTextureRect(sprite), false));
                continue;
            }

            _boxVisuals.Add(CreatePlaceholderVisual(definition.WorldSize));
        }

        if (_boxVisuals.Count == 0)
        {
            _boxVisuals.Add(CreatePlaceholderVisual(Vector2.one * _fallbackBoxSize));
        }

        FloorSprite = _assetLoader.LoadParcelSprite(StackBaseSpriteName) ?? _assetLoader.CreateSolidSprite(new Color(0.16f, 0.18f, 0.22f));
        BackgroundSprite = useLogisticsCenterBackground
            ? _assetLoader.LoadBackgroundSprite(BackgroundSpriteName)
            : null;
    }

    internal BoxStackPrototypeBoxVisual GetVisual(string boxSequence, int boxIndex)
    {
        if (_boxVisuals.Count == 0)
        {
            return CreatePlaceholderVisual(Vector2.one * _fallbackBoxSize);
        }

        int visualIndex = GetBoxVisualIndex(GetStageBoxCode(boxSequence, boxIndex));
        visualIndex = Mathf.Clamp(visualIndex, 0, _boxVisuals.Count - 1);
        return _boxVisuals[visualIndex];
    }

    internal bool IsPlaceholderSprite(Sprite sprite)
    {
        for (int i = 0; i < _boxVisuals.Count; i++)
        {
            BoxStackPrototypeBoxVisual visual = _boxVisuals[i];
            if (visual.IsPlaceholder && visual.Sprite == sprite)
            {
                return true;
            }
        }

        return false;
    }

    private BoxStackPrototypeBoxVisual CreatePlaceholderVisual(Vector2 worldSize)
    {
        Sprite placeholder = _assetLoader.CreateParcelBoxPlaceholder();
        return new BoxStackPrototypeBoxVisual(placeholder, worldSize, _assetLoader.GetVisibleTextureRect(placeholder), true);
    }

    private static char GetStageBoxCode(string boxSequence, int boxIndex)
    {
        if (string.IsNullOrEmpty(boxSequence))
        {
            return 'B';
        }

        return boxSequence[Mathf.Clamp(boxIndex, 0, boxSequence.Length - 1)];
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

    private readonly struct BoxAssetDefinition
    {
        internal BoxAssetDefinition(string name, Vector2 worldSize)
        {
            Name = name;
            WorldSize = worldSize;
        }

        internal string Name { get; }
        internal Vector2 WorldSize { get; }
    }
}
