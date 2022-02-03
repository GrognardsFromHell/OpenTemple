
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Dialog
{
    [DialogScript(109)]
    public class DalaDialog : Dala, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 62:
                case 64:
                    originalScript = "pc.stat_level_get(stat_charisma) >= 6";
                    return pc.GetStat(Stat.charisma) >= 6;
                case 65:
                case 66:
                    originalScript = "pc.stat_level_get(stat_charisma) <= 5";
                    return pc.GetStat(Stat.charisma) <= 5;
                case 71:
                case 73:
                    originalScript = "pc.money_get() >= 5";
                    return pc.GetMoney() >= 5;
                case 72:
                case 74:
                    originalScript = "pc.money_get() < 5";
                    return pc.GetMoney() < 5;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 71:
                case 73:
                    originalScript = "pc.money_adj(-5)";
                    pc.AdjustMoney(-5);
                    break;
                case 120:
                    originalScript = "game.quests[37].state = qs_completed";
                    SetQuestState(37, QuestState.Completed);
                    break;
                case 121:
                case 122:
                    originalScript = "make_dick_talk(npc,pc,1)";
                    make_dick_talk(npc, pc, 1);
                    break;
                case 133:
                case 136:
                    originalScript = "npc.attack(pc)";
                    npc.Attack(pc);
                    break;
                case 150:
                    originalScript = "create_item_in_inventory( 8004, pc )";
                    Utilities.create_item_in_inventory(8004, pc);
                    break;
                case 200:
                    originalScript = "npc.damage(OBJ_HANDLE_NULL,D20DT_SUBDUAL,dice_new(\"20d2\"),D20DAP_NORMAL)";
                    npc.Damage(null, DamageType.Subdual, Dice.Parse("20d2"), D20AttackPower.NORMAL);
                    break;
                default:
                    originalScript = null;
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
