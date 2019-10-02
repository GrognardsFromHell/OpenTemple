
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
    [DialogScript(153)]
    public class TurnkeyDialog : Turnkey, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 21:
                case 22:
                    Trace.Assert(originalScript == "game.story_state >= 5");
                    return StoryState >= 5;
                case 31:
                case 32:
                    Trace.Assert(originalScript == "game.quests[43].state >= qs_accepted");
                    return GetQuestState(43) >= QuestState.Accepted;
                case 65:
                case 66:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_gather_information) >= 10 or pc.stat_level_get(stat_level_bard) >= 1");
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 10 || pc.GetStat(Stat.level_bard) >= 1;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                case 23:
                case 24:
                case 41:
                case 42:
                case 61:
                case 62:
                case 101:
                case 102:
                case 114:
                case 115:
                    Trace.Assert(originalScript == "npc.attack( pc )");
                    npc.Attack(pc);
                    break;
                case 51:
                case 52:
                case 63:
                case 64:
                case 103:
                case 104:
                    Trace.Assert(originalScript == "game.global_flags[208] = 1");
                    SetGlobalFlag(208, true);
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
                case 65:
                case 66:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 10);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
