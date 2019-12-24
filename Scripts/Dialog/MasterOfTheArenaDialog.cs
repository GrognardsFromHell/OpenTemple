
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObject;
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
    [DialogScript(524)]
    public class MasterOfTheArenaDialog : MasterOfTheArena, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                    originalScript = "game.global_vars[974] = 2";
                    SetGlobalVar(974, 2);
                    break;
                case 40:
                    originalScript = "game.quests[65].state = qs_mentioned";
                    SetQuestState(65, QuestState.Mentioned);
                    break;
                case 41:
                case 43:
                case 101:
                case 103:
                    originalScript = "game.quests[65].state = qs_accepted";
                    SetQuestState(65, QuestState.Accepted);
                    break;
                case 50:
                    originalScript = "game.global_vars[994] = 1";
                    SetGlobalVar(994, 1);
                    break;
                case 61:
                    originalScript = "disappear(npc,pc); spawn_owlbears(npc,pc)";
                    disappear(npc, pc);
                    spawn_owlbears(npc, pc);
                    ;
                    break;
                case 71:
                    originalScript = "game.fade_and_teleport(0,0,0,5001,708,508); game.global_flags[944] = 1";
                    FadeAndTeleport(0, 0, 0, 5001, 708, 508);
                    SetGlobalFlag(944, true);
                    ;
                    break;
                case 81:
                    originalScript = "party_transfer_to( npc, 11050 ); game.quests[65].state = qs_botched; game.fade_and_teleport(0,0,0,5001,708,508)";
                    Utilities.party_transfer_to(npc, 11050);
                    SetQuestState(65, QuestState.Botched);
                    FadeAndTeleport(0, 0, 0, 5001, 708, 508);
                    ;
                    break;
                case 91:
                case 131:
                    originalScript = "game.fade_and_teleport(0,0,0,5001,708,508)";
                    FadeAndTeleport(0, 0, 0, 5001, 708, 508);
                    break;
                case 121:
                    originalScript = "disappear(npc,pc); spawn_giants(npc,pc)";
                    disappear(npc, pc);
                    spawn_giants(npc, pc);
                    ;
                    break;
                case 130:
                    originalScript = "game.global_vars[994] = 3";
                    SetGlobalVar(994, 3);
                    break;
                case 151:
                    originalScript = "disappear(npc,pc); spawn_undead(npc,pc)";
                    disappear(npc, pc);
                    spawn_undead(npc, pc);
                    ;
                    break;
                case 160:
                    originalScript = "game.global_vars[994] = 5; game.quests[65].state = qs_completed";
                    SetGlobalVar(994, 5);
                    SetQuestState(65, QuestState.Completed);
                    ;
                    break;
                case 161:
                    originalScript = "reward_pc( 6347, pc ); game.fade_and_teleport(0,0,0,5001,708,508)";
                    Utilities.reward_pc(6347, pc, npc);
                    FadeAndTeleport(0, 0, 0, 5001, 708, 508);
                    ;
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
