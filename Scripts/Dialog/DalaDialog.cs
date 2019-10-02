
using System;
using System.Collections.Generic;
using System.Diagnostics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Dialog
{
    [DialogScript(109)]
    public class DalaDialog : Dala, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 62:
                case 64:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_charisma) >= 6");
                    return pc.GetStat(Stat.charisma) >= 6;
                case 65:
                case 66:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_charisma) <= 5");
                    return pc.GetStat(Stat.charisma) <= 5;
                case 71:
                case 73:
                    Trace.Assert(originalScript == "pc.money_get() >= 5");
                    return pc.GetMoney() >= 5;
                case 72:
                case 74:
                    Trace.Assert(originalScript == "pc.money_get() < 5");
                    return pc.GetMoney() < 5;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 71:
                case 73:
                    Trace.Assert(originalScript == "pc.money_adj(-5)");
                    pc.AdjustMoney(-5);
                    break;
                case 120:
                    Trace.Assert(originalScript == "game.quests[37].state = qs_completed");
                    SetQuestState(37, QuestState.Completed);
                    break;
                case 121:
                case 122:
                    Trace.Assert(originalScript == "make_dick_talk(npc,pc,1)");
                    make_dick_talk(npc, pc, 1);
                    break;
                case 133:
                case 136:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 150:
                    Trace.Assert(originalScript == "create_item_in_inventory( 8004, pc )");
                    Utilities.create_item_in_inventory(8004, pc);
                    break;
                case 200:
                    Trace.Assert(originalScript == "npc.damage(OBJ_HANDLE_NULL,D20DT_SUBDUAL,dice_new(\"20d2\"),D20DAP_NORMAL)");
                    npc.Damage(null, DamageType.Subdual, Dice.Parse("20d2"), D20AttackPower.NORMAL);
                    break;
                default:
                    Trace.Assert(originalScript == null);
                    return;
            }
        }
        public bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks)
        {
            switch (lineNumber)
            {
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
