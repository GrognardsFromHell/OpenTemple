
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
    [DialogScript(133)]
    public class BrigandPrisonerDialog : BrigandPrisoner, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 31:
                case 32:
                    Trace.Assert(originalScript == "game.quests[52].state >= qs_accepted");
                    return GetQuestState(52) >= QuestState.Accepted;
                case 51:
                case 133:
                case 134:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 0");
                    return !pc.HasMaxFollowers();
                case 52:
                case 53:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 1");
                    return pc.HasMaxFollowers();
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
                    Trace.Assert(originalScript == "game.global_vars[134] = 1");
                    SetGlobalVar(134, 1);
                    break;
                case 2:
                case 3:
                    Trace.Assert(originalScript == "game.global_flags[130] = 1; get_rep( npc, pc )");
                    SetGlobalFlag(130, true);
                    get_rep(npc, pc);
                    ;
                    break;
                case 35:
                case 36:
                case 41:
                case 42:
                case 54:
                case 55:
                case 61:
                    Trace.Assert(originalScript == "run_off( npc, pc )");
                    run_off(npc, pc);
                    break;
                case 81:
                case 82:
                case 141:
                    Trace.Assert(originalScript == "pc.follower_add( npc )");
                    pc.AddFollower(npc);
                    break;
                case 91:
                    Trace.Assert(originalScript == "pc.follower_remove( npc ); move_wicked( npc, pc )");
                    pc.RemoveFollower(npc);
                    move_wicked(npc, pc);
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
