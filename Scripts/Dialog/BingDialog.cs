
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
    [DialogScript(57)]
    public class BingDialog : Bing, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "not npc.has_met(pc)";
                    return !npc.HasMet(pc);
                case 61:
                case 62:
                    originalScript = "game.quests[11].state != qs_unknown";
                    return GetQuestState(11) != QuestState.Unknown;
                case 223:
                case 224:
                case 283:
                case 284:
                    originalScript = "pc.stat_level_get(stat_charisma) >= 16 and pc.stat_level_get( stat_gender ) == gender_female and pc.skill_level_get(npc,skill_diplomacy) >= 5";
                    return pc.GetStat(Stat.charisma) >= 16 && pc.GetGender() == Gender.Female && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 5;
                case 285:
                case 286:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 8040 )";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(8040));
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 60:
                case 70:
                    originalScript = "game.global_vars[996] = 1";
                    SetGlobalVar(996, 1);
                    break;
                case 260:
                    originalScript = "game.global_flags[978] = 1";
                    SetGlobalFlag(978, true);
                    break;
                case 285:
                case 286:
                    originalScript = "party_transfer_to( npc, 8040 )";
                    Utilities.party_transfer_to(npc, 8040);
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
                case 223:
                case 224:
                case 283:
                case 284:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 5);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
