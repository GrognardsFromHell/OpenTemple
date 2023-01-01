using System;

namespace OpenTemple.Core.Hotkeys;

/// <summary>
/// Registration for a hotkey that causes the widget to be notified when the hotkey is being held.
/// </summary>
public record HeldHotkeyState(Hotkey Hotkey, Action<bool> Callback, Func<bool> Condition)
{
    public HeldHotkeyState(Hotkey hotkey, Action<bool> callback) : this(hotkey, callback, () => true)
    {
    }

    public bool Held { get; set; }
}
