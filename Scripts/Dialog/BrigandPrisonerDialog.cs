
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
    [DialogScript(133)]
    public class BrigandPrisonerDialog : BrigandPrisoner, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 31:
                case 32:
                    originalScript = "game.quests[52].state >= qs_accepted";
                    return GetQuestState(52) >= QuestState.Accepted;
                case 51:
                case 133:
                case 134:
                    originalScript = "pc.follower_atmax() == 0";
                    return !pc.HasMaxFollowers();
                case 52:
                case 53:
                    originalScript = "pc.follower_atmax() == 1";
                    return pc.HasMaxFollowers();
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                    originalScript = "game.global_vars[134] = 1";
                    SetGlobalVar(134, 1);
                    break;
                case 2:
                case 3:
                    originalScript = "game.global_flags[130] = 1; get_rep( npc, pc )";
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
                    originalScript = "run_off( npc, pc )";
                    run_off(npc, pc);
                    break;
                case 81:
                case 82:
                case 141:
                    originalScript = "pc.follower_add( npc )";
                    pc.AddFollower(npc);
                    break;
                case 91:
                    originalScript = "pc.follower_remove( npc ); move_wicked( npc, pc )";
                    pc.RemoveFollower(npc);
                    move_wicked(npc, pc);
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
