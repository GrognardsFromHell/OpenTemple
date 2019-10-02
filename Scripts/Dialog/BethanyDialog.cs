
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
    [DialogScript(584)]
    public class BethanyDialog : Bethany, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 241:
                    Trace.Assert(originalScript == "game.global_vars[802] == 1");
                    return GetGlobalVar(802) == 1;
                case 242:
                    Trace.Assert(originalScript == "game.global_vars[802] == 2");
                    return GetGlobalVar(802) == 2;
                case 243:
                    Trace.Assert(originalScript == "game.global_vars[802] == 3");
                    return GetGlobalVar(802) == 3;
                case 244:
                    Trace.Assert(originalScript == "game.global_vars[802] == 4");
                    return GetGlobalVar(802) == 4;
                case 245:
                    Trace.Assert(originalScript == "game.global_vars[802] == 5");
                    return GetGlobalVar(802) == 5;
                case 246:
                    Trace.Assert(originalScript == "game.global_vars[802] == 6");
                    return GetGlobalVar(802) == 6;
                case 247:
                    Trace.Assert(originalScript == "game.global_vars[802] == 7");
                    return GetGlobalVar(802) == 7;
                case 248:
                    Trace.Assert(originalScript == "game.global_vars[802] == 8");
                    return GetGlobalVar(802) == 8;
                case 249:
                    Trace.Assert(originalScript == "game.global_vars[802] == 9");
                    return GetGlobalVar(802) == 9;
                case 250:
                    Trace.Assert(originalScript == "game.global_vars[802] == 0");
                    return GetGlobalVar(802) == 0;
                case 261:
                case 271:
                case 281:
                case 286:
                    Trace.Assert(originalScript == "game.global_flags[962] == 0");
                    return !GetGlobalFlag(962);
                case 262:
                case 272:
                case 282:
                case 287:
                    Trace.Assert(originalScript == "game.global_flags[962] == 1");
                    return GetGlobalFlag(962);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                case 6:
                    Trace.Assert(originalScript == "game.quests[105].state = qs_completed");
                    SetQuestState(105, QuestState.Completed);
                    break;
                case 31:
                case 41:
                    Trace.Assert(originalScript == "heal_beth(npc,pc)");
                    heal_beth(npc, pc);
                    break;
                case 50:
                case 60:
                    Trace.Assert(originalScript == "game.areas[9] = 1");
                    MakeAreaKnown(9);
                    break;
                case 190:
                case 200:
                case 210:
                case 220:
                    Trace.Assert(originalScript == "game.quests[62].state = qs_accepted");
                    SetQuestState(62, QuestState.Accepted);
                    break;
                case 192:
                case 202:
                case 212:
                case 222:
                    Trace.Assert(originalScript == "game.worldmap_travel_by_dialog(9)");
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 240:
                    Trace.Assert(originalScript == "game.party[0].reputation_add(79); game.quests[62].state = qs_completed");
                    PartyLeader.AddReputation(79);
                    SetQuestState(62, QuestState.Completed);
                    ;
                    break;
                case 380:
                case 390:
                case 490:
                case 500:
                    Trace.Assert(originalScript == "pc.reputation_add(80)");
                    pc.AddReputation(80);
                    break;
                case 381:
                case 391:
                    Trace.Assert(originalScript == "game.global_flags[549] = 1; face_holly(npc,pc)");
                    SetGlobalFlag(549, true);
                    face_holly(npc, pc);
                    ;
                    break;
                case 491:
                case 501:
                    Trace.Assert(originalScript == "game.global_flags[549] = 1");
                    SetGlobalFlag(549, true);
                    break;
                case 511:
                case 551:
                    Trace.Assert(originalScript == "face_holly(npc,pc)");
                    face_holly(npc, pc);
                    break;
                case 540:
                    Trace.Assert(originalScript == "game.quests[62].state = qs_completed");
                    SetQuestState(62, QuestState.Completed);
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
