
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
    [DialogScript(146)]
    public class KelleenDialog : Kelleen, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 25:
                case 26:
                    originalScript = "game.global_flags[110] == 1";
                    return GetGlobalFlag(110);
                case 27:
                case 28:
                    originalScript = "game.quests[52].state == qs_completed";
                    return GetQuestState(52) == QuestState.Completed;
                case 29:
                case 30:
                    originalScript = "game.quests[46].state == qs_completed";
                    return GetQuestState(46) == QuestState.Completed;
                case 31:
                case 32:
                    originalScript = "game.quests[52].state == qs_accepted";
                    return GetQuestState(52) == QuestState.Accepted;
                case 103:
                case 104:
                    originalScript = "pc.skill_level_get(npc, skill_bluff) >= 10";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 10;
                case 105:
                case 106:
                    originalScript = "game.quests[52].state >= qs_accepted";
                    return GetQuestState(52) >= QuestState.Accepted;
                case 107:
                case 108:
                    originalScript = "game.quests[46].state >= qs_accepted";
                    return GetQuestState(46) >= QuestState.Accepted;
                case 175:
                case 176:
                    originalScript = "game.quests[52].state != qs_completed";
                    return GetQuestState(52) != QuestState.Completed;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 43:
                case 44:
                case 91:
                case 101:
                case 102:
                case 111:
                case 161:
                case 162:
                case 173:
                case 174:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
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
                case 103:
                case 104:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 10);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
