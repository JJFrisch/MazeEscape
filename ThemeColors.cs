namespace MazeEscape;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

public static class ThemeColors
{
    public static Color OverlayText => GetColor("OverlayTextColor", Colors.White);
    public static Color OverlayMutedText => GetColor("OverlayMutedTextColor", Colors.LightGray);
    public static Color OverlayGoldText => GetColor("OverlayGoldTextColor", Colors.Gold);
    public static Color OverlayGemText => GetColor("OverlayGemTextColor", Colors.MediumPurple);
    public static Color OverlaySuccessText => GetColor("OverlaySuccessTextColor", Colors.LightGreen);

    private static Color GetColor(string resourceKey, Color fallback)
    {
        if (Application.Current?.Resources.TryGetValue(resourceKey, out var resource) == true && resource is Color color)
        {
            return color;
        }

        return fallback;
    }
}