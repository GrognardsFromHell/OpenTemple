using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Actions;
using System;
using IronPython.Modules;
using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Systems.RadialMenus
{
    public delegate bool RadialMenuEntryCallback(GameObjectBody critter, ref RadialMenuEntry entry);

    public struct RadialMenuEntry
    {
        public string text; // Text to display

        public int text2; // string for popup dialog title, so far

        // public int textHash; // ELF hash of "text"
        public int fieldc;
        public RadialMenuEntryType type; // May define how the children are ordered (seen 4 been used here)
        public int minArg;
        public int maxArg;
        public Func<int> ArgumentGetter;
        public Action<int> ArgumentSetter;
        public bool HasArgument => ArgumentGetter != null && ArgumentSetter != null;
        public D20ActionType d20ActionType;
        public int d20ActionData1;
        public D20CAF d20Caf;
        public D20SpellData d20SpellData;

        public D20DispatcherKey
            dispKey; // example: DestructionDomainRadialMenu (the only one I've encountered so far), using this for python actions too now

        public RadialMenuEntryCallback callback;

        public bool HasMinArg
        {
            get => (flags & RadialMenuEntryFlags.HasMinArg) != 0;
            set
            {
                if (value)
                {
                    flags |= RadialMenuEntryFlags.HasMinArg;
                }
                else
                {
                    flags &= ~RadialMenuEntryFlags.HasMinArg;
                }
            }
        }

        public bool HasMaxArg
        {
            get => (flags & RadialMenuEntryFlags.HasMaxArg) != 0;
            set
            {
                if (value)
                {
                    flags |= RadialMenuEntryFlags.HasMaxArg;
                }
                else
                {
                    flags &= ~RadialMenuEntryFlags.HasMaxArg;
                }
            }
        }

        // see RadialMenuEntryFlags
        public RadialMenuEntryFlags flags;
        // String hash for the help topic associated with this entry
        public string helpSystemHashkey;

        public int
            spellIdMaybe; // used for stuff like Break Free / Dismiss Spell, and it also puts the id in the d20ActionData1 field

        [TempleDllLocation(0x100f0af0)]
        public static RadialMenuEntry Create()
        {
            var result = new RadialMenuEntry();
            result.text = "";
            result.d20ActionType = D20ActionType.NONE;
            return result;
        }

        public static RadialMenuEntry CreateAction(int combatMesLine, D20ActionType actionType, int data1,
            string helpId)
        {
            var result = new RadialMenuEntry();
            result.type = RadialMenuEntryType.Action;
            if (combatMesLine > 0)
                result.text = GameSystems.D20.Combat.GetCombatMesLine(combatMesLine);
            else
                result.text = "NULL";
            result.helpSystemHashkey = helpId;
            result.d20ActionType = actionType;
            result.d20ActionData1 = data1;
            return result;
        }

        public void AddAsChild(GameObjectBody critter, int parentId)
        {
            GameSystems.D20.RadialMenu.AddChildNode(critter, ref this, parentId);
        }

        public static RadialMenuEntry CreateParent(int combatMesLine)
        {
            var result = Create();
            result.type = RadialMenuEntryType.Parent;
            result.text = GameSystems.D20.Combat.GetCombatMesLine(combatMesLine);
            return result;
        }
    }
}