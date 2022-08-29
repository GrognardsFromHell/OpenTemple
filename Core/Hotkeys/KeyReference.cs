using System;
using System.Text;
using OpenTemple.Core.Platform;
using static SDL2.SDL;

namespace OpenTemple.Core.Hotkeys;

/// <summary>
/// References either a physical key on the keyboard regardless of keyboard map or a virtual key
/// that references a translated key.
/// Decides whether the hotkey is defined as a physical or virtual hot-key.
/// Physical hotkeys designate a physical key on the keyboard *regardless of layout*.
/// For example the key to toggle the console is always on the key right below the escape key,
/// regardless of what that key means in the users key map. On US keyboard this will be the GRAVE key,
/// while on German keyboards, it is the CARET key. But in either case, it should toggle the console.
/// In contrast to hotkeys like that, hotkeys that use letters or numbers should work regardless of where
/// those keys are placed. I.e. pressing "I" should open the inventory.
/// </summary>
public readonly struct KeyReference
{
    /// <summary>
    /// A key reference that references nothing and is equivalent to an unbound hotkey.
    /// </summary>
    public static readonly KeyReference None = default;

    /// <summary>
    /// Only valid if type is Physical.
    /// </summary>
    public SDL_Scancode PhysicalKey { get; init; }

    /// <summary>
    /// Only valid if type is Virtual.
    /// </summary>
    public SDL_Keycode VirtualKey { get; init; }

    /// <summary>
    /// Modifiers that must be present for the key to register.
    /// </summary>
    public KeyModifier Modifiers { get; init; }

    public string Text => ToString();

    public bool Equals(KeyReference other)
    {
        return PhysicalKey == other.PhysicalKey && VirtualKey == other.VirtualKey && Modifiers == other.Modifiers;
    }

    public override bool Equals(object? obj)
    {
        return obj is KeyReference other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int) PhysicalKey, (int) VirtualKey, (int) Modifiers);
    }

    public static bool operator ==(KeyReference left, KeyReference right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(KeyReference left, KeyReference right)
    {
        return !left.Equals(right);
    }

    public static KeyReference Physical(SDL_Scancode scancode, KeyModifier modifiers = default)
    {
        return new KeyReference
        {
            PhysicalKey = scancode,
            Modifiers = modifiers
        };
    }

    public static KeyReference Virtual(SDL_Keycode keycode, KeyModifier modifiers = default)
    {
        return new KeyReference
        {
            VirtualKey = keycode,
            Modifiers = modifiers
        };
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        if ((Modifiers & KeyModifier.Ctrl) != 0)
        {
            builder.Append("Ctrl+");
        }
        if ((Modifiers & KeyModifier.Shift) != 0)
        {
            builder.Append("Shift+");
        }
        if ((Modifiers & KeyModifier.Alt) != 0)
        {
            builder.Append("Alt+");
        }

        if (PhysicalKey != default)
        {
            builder.Append(SDL_GetScancodeName(PhysicalKey));
            builder.Append("+");
        }
        
        if (VirtualKey != default)
        {
            builder.Append(SDL_GetKeyName(VirtualKey));
            builder.Append("+");
        }

        // Remove trailing +
        if (builder.Length > 0)
        {
            builder.Remove(builder.Length - 1, 1);
        }

        return builder.ToString();
    }
}