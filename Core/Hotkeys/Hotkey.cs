using System;
using System.Text;
using OpenTemple.Core.Platform;
using static SDL2.SDL;

namespace OpenTemple.Core.Hotkeys;

public class Hotkey
{
    public string Id { get; init; }
    public string? EnglishName { get; init; }
    public KeyReference PrimaryKey { get; init; }
    public KeyReference SecondaryKey { get; init; }
    public HotkeyTrigger Trigger { get; init; }

    public static HotkeyBuilder Build(string id) => new(id);

    public bool Matches(SDL_Keycode virtualKey, SDL_Scancode physicalKey, KeyModifier heldModifiers, out int extraModifiers)
    {
        var matched = false;
        extraModifiers = int.MaxValue;

        void MatchKey(KeyReference keyReference, ref int extraModifiers)
        {
            if (keyReference != default && keyReference.Matches(virtualKey, physicalKey, heldModifiers))
            {
                extraModifiers = Math.Min(extraModifiers, int.PopCount((int) (heldModifiers & ~PrimaryKey.Modifiers)));
                matched = true;
            }
        }

        MatchKey(PrimaryKey, ref extraModifiers);
        MatchKey(SecondaryKey, ref extraModifiers);

        return matched;
    }

    public override string ToString()
    {
        var result = new StringBuilder();

        result.Append(EnglishName ?? Id);

        if (PrimaryKey != KeyReference.None || SecondaryKey != KeyReference.None)
        {
            result.Append(" (");
            if (PrimaryKey != KeyReference.None)
            {
                result.Append(PrimaryKey.ToString());
            }

            if (SecondaryKey != KeyReference.None)
            {
                if (PrimaryKey != KeyReference.None)
                {
                    result.Append(", ");
                }

                result.Append(SecondaryKey.ToString());
            }

            result.Append(')');
        }

        return result.ToString();
    }
}

public class HotkeyBuilder
{
    private readonly string _id;
    private KeyReference _primaryKey;
    private KeyReference _secondaryKey;
    private HotkeyTrigger _trigger = HotkeyTrigger.KeyDown;
    private string? _englishName;

    public HotkeyBuilder(string id)
    {
        _id = id;
    }

    public HotkeyBuilder EnglishName(string name)
    {
        _englishName = name;
        return this;
    }

    public HotkeyBuilder Primary(SDL_Scancode scancode, KeyModifier modifiers = default)
    {
        _primaryKey = new KeyReference
        {
            PhysicalKey = scancode,
            Modifiers = modifiers
        };
        return this;
    }

    public HotkeyBuilder Primary(SDL_Keycode virtualKey, KeyModifier modifiers = default)
    {
        _primaryKey = new KeyReference
        {
            VirtualKey = virtualKey,
            Modifiers = modifiers
        };
        return this;
    }

    public HotkeyBuilder Primary(KeyModifier modifiers = default)
    {
        _primaryKey = new KeyReference
        {
            Modifiers = modifiers
        };
        return this;
    }

    public HotkeyBuilder Secondary(SDL_Scancode scancode, KeyModifier modifiers = default)
    {
        _secondaryKey = new KeyReference
        {
            PhysicalKey = scancode,
            Modifiers = modifiers
        };
        return this;
    }

    public HotkeyBuilder Secondary(SDL_Keycode virtualKey, KeyModifier modifiers = default)
    {
        _secondaryKey = new KeyReference
        {
            VirtualKey = virtualKey,
            Modifiers = modifiers
        };
        return this;
    }

    public HotkeyBuilder OnKeyDown()
    {
        _trigger = HotkeyTrigger.KeyDown;
        return this;
    }

    public HotkeyBuilder OnKeyDownAndRepeat()
    {
        _trigger = HotkeyTrigger.KeyDownAndRepeat;
        return this;
    }

    public HotkeyBuilder OnKeyUp()
    {
        _trigger = HotkeyTrigger.KeyUp;
        return this;
    }

    public HotkeyBuilder Held()
    {
        _trigger = HotkeyTrigger.Held;
        return this;
    }

    public Hotkey Build()
    {
        return new Hotkey
        {
            Id = _id,
            PrimaryKey = _primaryKey,
            SecondaryKey = _secondaryKey,
            Trigger = _trigger,
            EnglishName = _englishName
        };
    }
}

/// <summary>
/// Defines when an action associated with a hotkey is triggered.
/// </summary>
public enum HotkeyTrigger
{
    Held,
    KeyDown,
    KeyUp,
    KeyDownAndRepeat
}