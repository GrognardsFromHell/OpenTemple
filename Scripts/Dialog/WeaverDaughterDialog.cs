
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
    [DialogScript(84)]
    public class WeaverDaughterDialog : WeaverDaughter, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    Trace.Assert(originalScript == "not npc.has_met(pc)");
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                    Trace.Assert(originalScript == "npc.has_met(pc)");
                    return npc.HasMet(pc);
                case 11:
                case 12:
                    Trace.Assert(originalScript == "game.quests[98].state == qs_accepted and game.quests[99].state == qs_unknown");
                    return GetQuestState(98) == QuestState.Accepted && GetQuestState(99) == QuestState.Unknown;
                case 13:
                case 14:
                    Trace.Assert(originalScript == "game.quests[99].state == qs_mentioned");
                    return GetQuestState(99) == QuestState.Mentioned;
                case 15:
                case 16:
                    Trace.Assert(originalScript == "game.global_vars[698] == 3 and game.quests[99].state != qs_completed");
                    return GetGlobalVar(698) == 3 && GetQuestState(99) != QuestState.Completed;
                case 17:
                case 18:
                    Trace.Assert(originalScript == "game.global_vars[698] == 4");
                    return GetGlobalVar(698) == 4;
                case 101:
                case 102:
                case 121:
                case 122:
                case 151:
                case 152:
                case 181:
                case 182:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 6");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 6;
                case 103:
                case 104:
                case 123:
                case 124:
                case 183:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 6");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 6;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 21:
                    Trace.Assert(originalScript == "argue(npc,pc,20)");
                    argue(npc, pc, 20);
                    break;
                case 41:
                    Trace.Assert(originalScript == "argue(npc,pc,30)");
                    argue(npc, pc, 30);
                    break;
                case 50:
                    Trace.Assert(originalScript == "make_hate( npc, pc )");
                    make_hate(npc, pc);
                    break;
                case 51:
                case 52:
                    Trace.Assert(originalScript == "argue(npc,pc,40)");
                    argue(npc, pc, 40);
                    break;
                case 61:
                case 62:
                    Trace.Assert(originalScript == "argue(npc,pc,50)");
                    argue(npc, pc, 50);
                    break;
                case 100:
                    Trace.Assert(originalScript == "game.quests[99].state = qs_mentioned");
                    SetQuestState(99, QuestState.Mentioned);
                    break;
                case 110:
                    Trace.Assert(originalScript == "game.quests[99].state = qs_accepted");
                    SetQuestState(99, QuestState.Accepted);
                    break;
                case 140:
                    Trace.Assert(originalScript == "game.quests[99].state = qs_completed");
                    SetQuestState(99, QuestState.Completed);
                    break;
                case 141:
                case 142:
                case 143:
                case 201:
                case 202:
                    Trace.Assert(originalScript == "create_item_in_inventory(12893,pc)");
                    Utilities.create_item_in_inventory(12893, pc);
                    break;
                case 200:
                    Trace.Assert(originalScript == "game.particles( 'sp-Call Lightning', npc ); game.global_vars[698] = 6");
                    AttachParticles("sp-Call Lightning", npc);
                    SetGlobalVar(698, 6);
                    ;
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
                case 101:
                case 102:
                case 121:
                case 122:
                case 151:
                case 152:
                case 181:
                case 182:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 6);
                    return true;
                case 103:
                case 104:
                case 123:
                case 124:
                case 183:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 6);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
