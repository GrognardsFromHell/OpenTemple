
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
    [DialogScript(524)]
    public class MasterOfTheArenaDialog : MasterOfTheArena, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
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
                    Trace.Assert(originalScript == "game.global_vars[974] = 2");
                    SetGlobalVar(974, 2);
                    break;
                case 40:
                    Trace.Assert(originalScript == "game.quests[65].state = qs_mentioned");
                    SetQuestState(65, QuestState.Mentioned);
                    break;
                case 41:
                case 43:
                case 101:
                case 103:
                    Trace.Assert(originalScript == "game.quests[65].state = qs_accepted");
                    SetQuestState(65, QuestState.Accepted);
                    break;
                case 50:
                    Trace.Assert(originalScript == "game.global_vars[994] = 1");
                    SetGlobalVar(994, 1);
                    break;
                case 61:
                    Trace.Assert(originalScript == "disappear(npc,pc); spawn_owlbears(npc,pc)");
                    disappear(npc, pc);
                    spawn_owlbears(npc, pc);
                    ;
                    break;
                case 71:
                    Trace.Assert(originalScript == "game.fade_and_teleport(0,0,0,5001,708,508); game.global_flags[944] = 1");
                    FadeAndTeleport(0, 0, 0, 5001, 708, 508);
                    SetGlobalFlag(944, true);
                    ;
                    break;
                case 81:
                    Trace.Assert(originalScript == "party_transfer_to( npc, 11050 ); game.quests[65].state = qs_botched; game.fade_and_teleport(0,0,0,5001,708,508)");
                    Utilities.party_transfer_to(npc, 11050);
                    SetQuestState(65, QuestState.Botched);
                    FadeAndTeleport(0, 0, 0, 5001, 708, 508);
                    ;
                    break;
                case 91:
                case 131:
                    Trace.Assert(originalScript == "game.fade_and_teleport(0,0,0,5001,708,508)");
                    FadeAndTeleport(0, 0, 0, 5001, 708, 508);
                    break;
                case 121:
                    Trace.Assert(originalScript == "disappear(npc,pc); spawn_giants(npc,pc)");
                    disappear(npc, pc);
                    spawn_giants(npc, pc);
                    ;
                    break;
                case 130:
                    Trace.Assert(originalScript == "game.global_vars[994] = 3");
                    SetGlobalVar(994, 3);
                    break;
                case 151:
                    Trace.Assert(originalScript == "disappear(npc,pc); spawn_undead(npc,pc)");
                    disappear(npc, pc);
                    spawn_undead(npc, pc);
                    ;
                    break;
                case 160:
                    Trace.Assert(originalScript == "game.global_vars[994] = 5; game.quests[65].state = qs_completed");
                    SetGlobalVar(994, 5);
                    SetQuestState(65, QuestState.Completed);
                    ;
                    break;
                case 161:
                    Trace.Assert(originalScript == "reward_pc( 6347, pc ); game.fade_and_teleport(0,0,0,5001,708,508)");
                    Utilities.reward_pc(6347, pc, npc);
                    FadeAndTeleport(0, 0, 0, 5001, 708, 508);
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
