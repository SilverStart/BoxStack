using UnityEngine;
using UnityEngine.UIElements;

internal sealed class BoxStackUiStyling
{
    private Font _displayFont;
    private Font _bodyFont;
    private Font _fallbackFont;

    internal void SetFonts(Font displayFont, Font bodyFont, Font fallbackFont)
    {
        _displayFont = displayFont;
        _bodyFont = bodyFont;
        _fallbackFont = fallbackFont;
    }

    internal void ApplyPanelStyle(VisualElement element, Color background, Color border, float radius)
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

    internal void ApplyGlassPanelStyle(VisualElement element, Color tint, float backgroundAlpha, float radius)
    {
        Color background = Color.Lerp(tint, Color.white, 0.14f);
        background.a = backgroundAlpha;

        ApplyPanelStyle(element, background, Color.clear, radius);
    }

    internal void ApplyButtonColors(Button button, Color background, Color text)
    {
        float alpha = background.a > 0f ? Mathf.Clamp(background.a, 0.48f, 0.82f) : 0.52f;
        ApplyGlassPanelStyle(button, background, alpha, 18f);
        button.style.color = text;
    }

    internal void ApplyDisplayText(TextElement element, int fontSize, FontStyle fontStyle, Color color, TextAnchor alignment)
    {
        ApplyText(element, _displayFont != null ? _displayFont : _fallbackFont, fontSize, fontStyle, color, alignment);
    }

    internal void ApplyBodyText(TextElement element, int fontSize, FontStyle fontStyle, Color color, TextAnchor alignment)
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
}
