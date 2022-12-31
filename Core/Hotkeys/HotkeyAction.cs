using System;

namespace OpenTemple.Core.Hotkeys;

/// <summary>
/// An action associated with the hotkey that can trigger it, given that the given condition returns true.
/// </summary>
public record HotkeyAction(Hotkey Hotkey, Action Callback, Func<bool> Condition)
{
    public HotkeyAction(Hotkey hotkey, Action callback) : this(hotkey, callback, () => true)
    {
    }
}
