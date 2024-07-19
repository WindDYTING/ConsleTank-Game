using System;

namespace Tank;

public static class Utils
{
    public static void UsingColor(ConsoleColor foregroundColor, ConsoleColor backgroundColor, Action action)
    {
        var originalForegroundColor = Console.ForegroundColor;
        var originalBackgroundColor = Console.BackgroundColor;

        Console.ForegroundColor = foregroundColor;
        Console.BackgroundColor = backgroundColor;
        
        action();

        Console.ForegroundColor = originalForegroundColor;
        Console.BackgroundColor = originalBackgroundColor;
    }

    public static void UsingColor(ConsoleColorPair colorPair, Action action)
    {
        UsingColor(colorPair.ForegroundColor, colorPair.BackgroundColor, action);
    }
}