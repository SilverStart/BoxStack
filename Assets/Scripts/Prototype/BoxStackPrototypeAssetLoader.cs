// 프로토타입 에셋 로더 - Resources/Editor 로딩 방식을 제품용 에셋 파이프라인으로 교체하기 위한 경계.

using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

internal sealed class BoxStackPrototypeAssetLoader
{
    private const string ParcelAssetFolder = "Assets/Art/Prototype/Parcel";
    private const string ParcelResourceFolder = "Prototype/Parcel";
    private const string BackgroundAssetFolder = "Assets/Art/Prototype/Backgrounds";
    private const string BackgroundResourceFolder = "Prototype/Backgrounds";
    private const string DisplayFontResourcePath = "Prototype/Fonts/DNFBitBitv2";
    private const string BodyFontResourcePath = "Prototype/Fonts/GmarketSansBold";
    private const string KoreanFallbackFontResourcePath = "Prototype/Fonts/NotoSansKR-VF";
    private const string PrototypeConfigResourcePath = "Prototype/BoxStackPrototypeConfig";

    internal BoxStackPrototypeConfig LoadConfig()
    {
        return Resources.Load<BoxStackPrototypeConfig>(PrototypeConfigResourcePath);
    }

    internal Font LoadDisplayFont()
    {
        return Resources.Load<Font>(DisplayFontResourcePath);
    }

    internal Font LoadBodyFont()
    {
        return Resources.Load<Font>(BodyFontResourcePath);
    }

    internal Font LoadKoreanFallbackFont()
    {
        return Resources.Load<Font>(KoreanFallbackFontResourcePath);
    }

    internal Sprite LoadParcelSprite(string assetName)
    {
        return LoadPrototypeSprite(assetName, ParcelResourceFolder, ParcelAssetFolder);
    }

    internal Sprite LoadBackgroundSprite(string assetName)
    {
        return LoadPrototypeSprite(assetName, BackgroundResourceFolder, BackgroundAssetFolder);
    }

    internal Sprite CreateParcelBoxPlaceholder()
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

    internal Sprite CreateSolidSprite(Color color)
    {
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point
        };

        texture.SetPixel(0, 0, color);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }

    internal Rect GetVisibleTextureRect(Sprite sprite)
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
}
