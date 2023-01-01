using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Hotkeys;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.RadialMenus;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Utils;
using static SDL2.SDL;

namespace OpenTemple.Core.Systems.D20;

public class HotkeySystem : IDisposable
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    [TempleDllLocation(0x102E8B78)]
    private static readonly IImmutableSet<KeyReference> AssignableKeys = new HashSet<KeyReference>
    {
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_Q),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_W),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_E),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_R),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_T),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_Y),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_U),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_I),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_O),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_P),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_LEFTBRACKET),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_RIGHTBRACKET),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_A),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_S),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_D),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_F),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_G),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_H),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_J),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_K),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_L),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_SEMICOLON),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_APOSTROPHE),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_BACKSLASH),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_Z),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_X),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_C),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_V),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_B),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_N),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_M),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_COMMA),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_PERIOD),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_SLASH),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_F11),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_F12),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_F13),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_F14),
        KeyReference.Physical(SDL_Scancode.SDL_SCANCODE_F15),
    }.ToImmutableHashSet();

    [TempleDllLocation(0x10BD0248)]
    private readonly Dictionary<string, RadialMenuEntry> _hotkeyTable = new();

    [TempleDllLocation(0x10bd0d40)]
    private readonly Dictionary<int, string> _translations;

    [TempleDllLocation(0x100f3b80)]
    public HotkeySystem()
    {
        _translations = Tig.FS.ReadMesFile("mes/hotkeys.mes");
    }

    [TempleDllLocation(0x100f3bc0)]
    public void Dispose()
    {
    }

    [TempleDllLocation(0x100f3bd0)]
    public SavedHotkeys SaveHotkeys()
    {
        var result = new SavedHotkeys();

        foreach (var (id, radialMenuEntry) in _hotkeyTable)
        {
            result.Hotkeys.Add(new SavedHotkey
            {
                Id = id,
                ActionType = radialMenuEntry.d20ActionType,
                ActionData = radialMenuEntry.d20ActionData1,
                SpellData = radialMenuEntry.d20SpellData,
                TextHash = ElfHash.Hash(radialMenuEntry.text),
                Text = radialMenuEntry.text
            });
        }

        return result;
    }

    [TempleDllLocation(0x100f3c80)]
    public void LoadHotkeys(SavedHotkeys savedHotkeys)
    {
        _hotkeyTable.Clear();

        foreach (var savedHotkey in savedHotkeys.Hotkeys)
        {
            _hotkeyTable[savedHotkey.Id] = new RadialMenuEntry
            {
                text = savedHotkey.Text,
                d20ActionType = savedHotkey.ActionType,
                d20ActionData1 = savedHotkey.ActionData,
                d20SpellData = savedHotkey.SpellData
            };
            // TODO: Vanilla used only the ELF32 hash stored in textHash, which sometimes doesn't match the text!
        }
    }

    [TempleDllLocation(0x100F3ED0)]
    public bool IsReservedHotkey(KeyReference key)
    {
        return false;
    }

    [TempleDllLocation(0x100F3F20)]
    public int HotkeyReservedPopup(KeyReference key)
    {
        Stub.TODO();
        return 0;
    }

    [TempleDllLocation(0x100F3D20)]
    public bool IsNormalNonreservedHotkey(KeyReference key)
    {
        return AssignableKeys.Contains(key);
    }

    [TempleDllLocation(0x100F3D60)]
    public bool ActivateHotkeyEntry(GameObject obj, Hotkey key)
    {
        if (!_hotkeyTable.TryGetValue(key.Id, out var hotkeyEntry) ||
            hotkeyEntry.d20ActionType == D20ActionType.UNASSIGNED)
        {
            return false;
        }

        return GameSystems.D20.RadialMenu.ActivateEntry(obj, hotkeyEntry);
    }

    [TempleDllLocation(0x100f3df0)]
    private bool FindBoundHotkeyId(ref RadialMenuEntry radMenuEntry, [MaybeNullWhen(false)] out string hotkeyId)
    {
        foreach (var kvp in _hotkeyTable)
        {
            if (RadialMenuSystem.HotkeyCompare(kvp.Value, radMenuEntry))
            {
                hotkeyId = kvp.Key;
                return true;
            }
        }

        hotkeyId = null;
        return false;
    }

    [TempleDllLocation(0x100f3e80)]
    public string? GetHotkeyLetter(ref RadialMenuEntry radMenuEntryToBind)
    {
        if (FindBoundHotkeyId(ref radMenuEntryToBind, out var hotkeyId))
        {
            return InGameHotKey.UserAssignableHotkeys
                .FirstOrDefault(hk => hk.Id == hotkeyId)
                ?.PrimaryKey.Text;
        }

        return null;
    }

    [TempleDllLocation(0x100f40a0)]
    public bool HotkeyAssignCreatePopupUi(RadialMenuEntry radMenuEntry, Hotkey hotkey)
    {
        if (!InGameHotKey.IsUserAssignableHotkey(hotkey))
        {
            return false;
        }

        if (radMenuEntry.type == RadialMenuEntryType.Parent)
        {
            return false;
        }

        var middle = GameSystems.D20.Combat.GetCombatMesLine(180); // ' key to '
        var prefix = GameSystems.D20.Combat.GetCombatMesLine(179); // Assign '
        var suffix = GameSystems.D20.Combat.GetCombatMesLine(181); // '?
        var keyName = hotkey.PrimaryKey.Text;
        var questionText = $"{prefix}{keyName}{middle}{radMenuEntry.text}{suffix}";

        if (!_hotkeyTable.TryGetValue(hotkey.Id, out var currentEntry))
        {
            questionText += GameSystems.D20.Combat.GetCombatMesLine(182); // (Key currently unassigned)
        }
        else
        {
            var currentlyPrefix = GameSystems.D20.Combat.GetCombatMesLine(183); // (Key currently assigned to '
            var currentlySuffix = GameSystems.D20.Combat.GetCombatMesLine(184); // ')

            questionText += $"{currentlyPrefix}{currentEntry.text}{currentlySuffix}";
        }

        var dialogTitle = GameSystems.D20.Combat.GetCombatMesLine(185); // Assign Hotkey

        [TempleDllLocation(0x100f4030)]
        void Callback(int buttonIdx)
        {
            if (buttonIdx != 0) return;

            AssignHotkey(radMenuEntry, hotkey);
        }

        // TODO: These should be part of the delegate
        // radMenuEntryToBind/*0x115b1ed4*/ = radMenuEntry;
        // hotkeyKeyIdxToBind/*0x115b1ed0*/ = key;
        GameUiBridge.Confirm(questionText, dialogTitle, false, Callback);

        return true;
    }

    private void AssignHotkey(RadialMenuEntry radMenuEntry, Hotkey hotkey)
    {
        if (!InGameHotKey.IsUserAssignableHotkey(hotkey))
        {
            throw new InvalidOperationException("Cannot bind to non-user hotkey " + hotkey);
        }

        // Unbind an existing hotkey
        if (FindBoundHotkeyId(ref radMenuEntry, out var hotkeyId))
        {
            _hotkeyTable.Remove(hotkeyId);
        }

        _hotkeyTable[hotkey.Id] = radMenuEntry;
    }

    public void TriggerOutOfCombat(Hotkey hotkey)
    {
        var leader = GameSystems.Party.GetConsciousLeader();
        GameSystems.D20.Actions.TurnBasedStatusInit(leader);
        leader = GameSystems.Party.GetConsciousLeader(); // in case the leader changes somehow...
        Logger.Info("Intgame: Resetting sequence.");
        GameSystems.D20.Actions.CurSeqReset(leader);

        AddHotkeyActionToSequence(hotkey);
    }

    public bool AddHotkeyActionToSequence(Hotkey hotkey)
    {
        if (GameSystems.D20.Actions.CurrentSequence == null)
        {
            throw new InvalidOperationException("Cannot add a hotkey action if no action is active.");
        }

        var leader = GameSystems.Party.GetConsciousLeader();
        GameSystems.D20.Actions.GlobD20ActnInit();
        if (ActivateHotkeyEntry(GameSystems.Party.GetConsciousLeader(), hotkey))
        {
            GameSystems.D20.Actions.ActionAddToSeq();
            GameSystems.D20.Actions.sequencePerform();
            leader.PlayVoiceConfirmationSound();
            GameSystems.D20.RadialMenu.ClearActiveRadialMenu();
            return true;
        }

        return false;
    }

    public bool IsAssigned(Hotkey hotkey)
    {
        return _hotkeyTable.TryGetValue(hotkey.Id, out var hotkeyEntry)
               && hotkeyEntry.d20ActionType != D20ActionType.UNASSIGNED;
    }
}