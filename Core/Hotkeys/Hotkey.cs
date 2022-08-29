using System.Dynamic;
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
}

public class HotkeyBuilder
{
    private readonly string _id;
    private KeyReference _primaryKey;
    private KeyReference _secondaryKey;
    private HotkeyTrigger _trigger;
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

    public HotkeyBuilder OnKeyDown()
    {
        _trigger = HotkeyTrigger.KeyDown;
        return this;
    }

    public HotkeyBuilder Held()
    {
        _trigger = HotkeyTrigger.Held;
        return this;
    }

    public Hotkey Build()
    {
        return new Hotkey()
        {
            Id = _id,
            PrimaryKey = _primaryKey,
            SecondaryKey = _secondaryKey,
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
