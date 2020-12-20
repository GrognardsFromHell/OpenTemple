using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems.RadialMenus;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.D20
{
    public class HotkeySystem : IDisposable
    {
        [TempleDllLocation(0x102E8B78)]
        private static readonly IImmutableSet<DIK> AssignableKeys = new HashSet<DIK>
        {
            DIK.DIK_Q, DIK.DIK_W, DIK.DIK_E, DIK.DIK_R, DIK.DIK_T, DIK.DIK_Y, DIK.DIK_U, DIK.DIK_I, DIK.DIK_O,
            DIK.DIK_P, DIK.DIK_LBRACKET, DIK.DIK_RBRACKET, DIK.DIK_A, DIK.DIK_S, DIK.DIK_D, DIK.DIK_F, DIK.DIK_G,
            DIK.DIK_H,
            DIK.DIK_J, DIK.DIK_K, DIK.DIK_L, DIK.DIK_SEMICOLON, DIK.DIK_APOSTROPHE, DIK.DIK_BACKSLASH,
            DIK.DIK_Z, DIK.DIK_X, DIK.DIK_C, DIK.DIK_V, DIK.DIK_B, DIK.DIK_N, DIK.DIK_M, DIK.DIK_COMMA, DIK.DIK_PERIOD,
            DIK.DIK_SLASH, DIK.DIK_F11, DIK.DIK_F12, DIK.DIK_F13, DIK.DIK_F14, DIK.DIK_F15
        }.ToImmutableHashSet();

        [TempleDllLocation(0x102E8C14)]
        private static readonly IImmutableSet<DIK> ReservedKeys = new HashSet<DIK>
        {
            DIK.DIK_H, DIK.DIK_I, DIK.DIK_L, DIK.DIK_M, DIK.DIK_F, DIK.DIK_R, DIK.DIK_O, DIK.DIK_C, DIK.DIK_SPACE
        }.ToImmutableHashSet();

        [TempleDllLocation(0x102E8C40)]
        private static readonly IImmutableSet<DIK> FunctionKeys = new HashSet<DIK>
        {
            DIK.DIK_F1, DIK.DIK_F2, DIK.DIK_F3, DIK.DIK_F4, DIK.DIK_F5, DIK.DIK_F6, DIK.DIK_F7, DIK.DIK_F8
        }.ToImmutableHashSet();

        [TempleDllLocation(0x10BD0248)]
        private readonly Dictionary<DIK, RadialMenuEntry> _hotkeyTable = new Dictionary<DIK, RadialMenuEntry>();

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

            foreach (var (key, radialMenuEntry) in _hotkeyTable)
            {
                result.Hotkeys[key] = new SavedHotkey
                {
                    Key = key,
                    ActionType = radialMenuEntry.d20ActionType,
                    ActionData = radialMenuEntry.d20ActionData1,
                    SpellData = radialMenuEntry.d20SpellData,
                    TextHash = ElfHash.Hash(radialMenuEntry.text),
                    Text = radialMenuEntry.text
                };
            }

            return result;
        }

        [TempleDllLocation(0x100f3c80)]
        public void LoadHotkeys(SavedHotkeys savedHotkeys)
        {
            _hotkeyTable.Clear();

            foreach (var savedHotkey in savedHotkeys.Hotkeys.Values)
            {
                _hotkeyTable[savedHotkey.Key] = new RadialMenuEntry
                {
                    text = savedHotkey.Text,
                    d20ActionType = savedHotkey.ActionType,
                    d20ActionData1 = savedHotkey.ActionData,
                    d20SpellData = savedHotkey.SpellData
                };
                // TODO: Vanilla used only the ELF32 hash stored in textHash, which sometimes doesn't match the text!
            }
        }

        [TempleDllLocation(0x100f4030)]
        public void HotkeyAssignCallback(int buttonIdx)
        {
            if (buttonIdx == 0)
            {
                Stub.TODO();
            }
        }

        [TempleDllLocation(0x100F3ED0)]
        public bool IsReservedHotkey(DIK dinputKey)
        {
            Stub.TODO();
            return false;
        }

        [TempleDllLocation(0x100F3F20)]
        public int HotkeyReservedPopup(DIK dinputKey)
        {
            Stub.TODO();
            return 0;
        }

        [TempleDllLocation(0x100F3D20)]
        public bool IsNormalNonreservedHotkey(DIK dinputKey)
        {
            Stub.TODO();
            return false;
        }

        [TempleDllLocation(0x100F3D60)]
        public bool RadmenuHotkeySthg(GameObjectBody obj, DIK key)
        {
            if (!_hotkeyTable.TryGetValue(key, out var hotkeyEntry) ||
                hotkeyEntry.d20ActionType == D20ActionType.UNASSIGNED)
            {
                return false;
            }

            return GameSystems.D20.RadialMenu.ActivateEntry(obj, hotkeyEntry);
        }

        [TempleDllLocation(0x100f3df0)]
        private DIK? HotkeyTableSearch(ref RadialMenuEntry radMenuEntry)
        {
            foreach (var kvp in _hotkeyTable)
            {
                if (RadialMenuSystem.HotkeyCompare(kvp.Value, radMenuEntry))
                {
                    return kvp.Key;
                }
            }

            return null;
        }

        [TempleDllLocation(0x100f3e80)]
        public string GetKeyDisplayName(DIK key)
        {
            // TODO: This is stupid and should instead use the platform's abstraction of getting a key name! (and it's also borked)
            return _translations[(int) key];
        }

        [TempleDllLocation(0x100f3e80)]
        public string GetHotkeyLetter(ref RadialMenuEntry radMenuEntryToBind)
        {
            var assignedKey = HotkeyTableSearch(ref radMenuEntryToBind);
            if (!assignedKey.HasValue)
            {
                return null;
            }

            return GetKeyDisplayName(assignedKey.Value);
        }

        [TempleDllLocation(0x100f40a0)]
        public bool HotkeyAssignCreatePopupUi(ref RadialMenuEntry radMenuEntry, DIK key)
        {
            if (!AssignableKeys.Contains(key))
            {
                return false;
            }

            if (GameSystems.D20.Hotkeys.IsReservedHotkey(key))
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
            var keyName = GetKeyDisplayName(key);
            var questionText = $"{prefix}{keyName}{middle}{radMenuEntry.text}{suffix}";

            if (!_hotkeyTable.TryGetValue(key, out var currentEntry))
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
            Stub.TODO(); // Show confirm box

            // TODO: These should be part of the delegate
            // radMenuEntryToBind/*0x115b1ed4*/ = radMenuEntry;
            // hotkeyKeyIdxToBind/*0x115b1ed0*/ = key;
            GameUiBridge.Confirm(questionText, dialogTitle, false, HotkeyAssignCallback);

            return true;
        }
    }
}