using System;
using OpenTemple.Core.Ui.Events;

namespace OpenTemple.Core.Hotkeys;

/// <summary>
/// An action associated with the hotkey that can trigger it, given that the given condition returns true.
/// </summary>
public record HotkeyAction(Hotkey Hotkey, Action<KeyboardEvent> Callback, Func<bool> Condition)
{
    public HotkeyAction(Hotkey hotkey, Action<KeyboardEvent> callback) : this(hotkey, callback, () => true)
    {
    }

    public HotkeyAction(Hotkey hotkey, Action callback, Func<bool> condition) : this(hotkey, e => callback(), condition)
    {
    }

    public HotkeyAction(Hotkey hotkey, Action callback) : this(hotkey, callback, () => true)
    {
    }
}