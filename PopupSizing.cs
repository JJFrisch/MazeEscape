namespace MazeEscape;

using Microsoft.Maui.Graphics;

internal static class PopupSizing
{
    public static Size Calculate(
        double widthFactor,
        double heightFactor,
        double minWidth,
        double maxWidth,
        double minHeight,
        double maxHeight)
    {
        double pageWidth = Application.Current?.MainPage?.Width ?? App.PlayerData.WindowWidth;
        double pageHeight = Application.Current?.MainPage?.Height ?? App.PlayerData.WindowHeight;

        if (pageWidth <= 0)
        {
            pageWidth = App.PlayerData.WindowWidth;
        }

        if (pageHeight <= 0)
        {
            pageHeight = App.PlayerData.WindowHeight;
        }

        return new Size(
            Math.Clamp(pageWidth * widthFactor, minWidth, maxWidth),
            Math.Clamp(pageHeight * heightFactor, minHeight, maxHeight));
    }
}
