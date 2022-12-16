using static SDL2.SDL;

namespace OpenTemple.Core.Platform;

/// <summary>
/// Easier access to the system clipboard.
/// </summary>
public static class Clipboard
{
    public static bool HasText => SDL_HasClipboardText() == SDL_bool.SDL_TRUE;

    public static void SetText(string text)
    {
        SDL_SetClipboardText(text);
    }

    public static string? GetText()
    {
        return SDL_GetClipboardText();
    }
}