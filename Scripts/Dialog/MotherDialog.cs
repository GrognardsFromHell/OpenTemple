
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
    [DialogScript(547)]
    public class MotherDialog : Mother, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 103:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == LAWFUL_EVIL");
                    return PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL;
                case 122:
                case 201:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_item\", 12900)");
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(12900));
                case 124:
                    Trace.Assert(originalScript == "game.quests[99].state == qs_completed");
                    return GetQuestState(99) == QuestState.Completed;
                case 155:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_sense_motive) >= 6");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 6;
                case 156:
                case 202:
                    Trace.Assert(originalScript == "game.global_vars[958] == 6");
                    return GetGlobalVar(958) == 6;
                case 183:
                    Trace.Assert(originalScript == "game.quests[99].state == qs_accepted");
                    return GetQuestState(99) == QuestState.Accepted;
                case 231:
                    Trace.Assert(originalScript == "pc.money_get() >= 500000");
                    return pc.GetMoney() >= 500000;
                case 232:
                    Trace.Assert(originalScript == "pc.money_get() <= 499900");
                    return pc.GetMoney() <= 499900;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 104:
                case 132:
                case 142:
                case 182:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 121:
                case 183:
                    Trace.Assert(originalScript == "npc.reaction_adj( pc, -10 )");
                    npc.AdjustReaction(pc, -10);
                    break;
                case 181:
                    Trace.Assert(originalScript == "party_transfer_to( npc, 12900 )");
                    Utilities.party_transfer_to(npc, 12900);
                    break;
                case 190:
                    Trace.Assert(originalScript == "create_item_in_inventory(8120,pc); create_item_in_inventory(8037,pc)");
                    Utilities.create_item_in_inventory(8120, pc);
                    Utilities.create_item_in_inventory(8037, pc);
                    ;
                    break;
                case 191:
                    Trace.Assert(originalScript == "npc.reaction_adj( pc, 20 )");
                    npc.AdjustReaction(pc, 20);
                    break;
                case 192:
                    Trace.Assert(originalScript == "pc.barter(npc); npc.reaction_adj( pc, 15 )");
                    throw new NotSupportedException("Conversion failed.");
                case 231:
                    Trace.Assert(originalScript == "pc.money_adj(-500000)");
                    pc.AdjustMoney(-500000);
                    break;
                case 240:
                    Trace.Assert(originalScript == "bling(npc,pc); game.global_vars[958] = 8");
                    bling(npc, pc);
                    SetGlobalVar(958, 8);
                    ;
                    break;
                default:
                    Trace.Assert(originalScript == null);
                    return;
            }
        }
        public bool TryGetSkillCheck(int lineNumber, out DialogSkillChecks skillChecks)
        {
            switch (lineNumber)
            {
                case 155:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 6);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
