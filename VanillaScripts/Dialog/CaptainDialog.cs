
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

namespace VanillaScripts.Dialog
{
    [DialogScript(6)]
    public class CaptainDialog : Captain, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                case 163:
                case 164:
                case 173:
                case 174:
                    originalScript = "game.quests[1].state == qs_accepted and npc.has_met( pc )";
                    return GetQuestState(1) == QuestState.Accepted && npc.HasMet(pc);
                case 4:
                case 5:
                case 161:
                case 162:
                case 171:
                case 172:
                    originalScript = "not npc.has_met( pc )";
                    return !npc.HasMet(pc);
                case 9:
                    originalScript = "npc.has_met( pc )";
                    return npc.HasMet(pc);
                case 21:
                case 28:
                    originalScript = "anyone( pc.group_list(), \"has_follower\", 8000 )";
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8000));
                case 22:
                case 26:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) >= 7";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 7;
                case 23:
                case 25:
                    originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 9";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 9;
                case 24:
                case 27:
                    originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 4";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 4;
                case 101:
                case 102:
                    originalScript = "game.global_flags[67] == 0";
                    return !GetGlobalFlag(67);
                case 103:
                case 104:
                    originalScript = "game.quests[1].state == qs_accepted";
                    return GetQuestState(1) == QuestState.Accepted;
                case 105:
                case 106:
                    originalScript = "game.global_flags[70] == 0";
                    return !GetGlobalFlag(70);
                case 111:
                case 113:
                case 151:
                case 152:
                    originalScript = "not anyone( pc.group_list(), \"has_follower\", 8000 )";
                    return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8000));
                case 112:
                case 114:
                    originalScript = "not anyone( pc.group_list(), \"has_follower\", 8000 ) and game.global_flags[70] == 0";
                    return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8000)) && !GetGlobalFlag(70);
                case 501:
                case 510:
                    originalScript = "game.party_alignment == LAWFUL_GOOD";
                    return PartyAlignment == Alignment.LAWFUL_GOOD;
                case 502:
                case 511:
                    originalScript = "game.party_alignment == CHAOTIC_GOOD";
                    return PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 503:
                case 512:
                    originalScript = "game.party_alignment == LAWFUL_EVIL";
                    return PartyAlignment == Alignment.LAWFUL_EVIL;
                case 504:
                case 513:
                    originalScript = "game.party_alignment == CHAOTIC_EVIL";
                    return PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 505:
                case 514:
                    originalScript = "game.party_alignment == TRUE_NEUTRAL";
                    return PartyAlignment == Alignment.NEUTRAL;
                case 506:
                case 515:
                    originalScript = "game.party_alignment == NEUTRAL_GOOD";
                    return PartyAlignment == Alignment.NEUTRAL_GOOD;
                case 507:
                case 516:
                    originalScript = "game.party_alignment == NEUTRAL_EVIL";
                    return PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 508:
                case 517:
                    originalScript = "game.party_alignment == LAWFUL_NEUTRAL";
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL;
                case 509:
                case 518:
                    originalScript = "game.party_alignment == CHAOTIC_NEUTRAL";
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 30:
                case 60:
                    originalScript = "game.quests[1].state = qs_completed";
                    SetQuestState(1, QuestState.Completed);
                    break;
                case 80:
                    originalScript = "game.quests[1].state = qs_completed; npc.reaction_adj( pc,-10)";
                    SetQuestState(1, QuestState.Completed);
                    npc.AdjustReaction(pc, -10);
                    ;
                    break;
                case 90:
                    originalScript = "game.quests[1].state = qs_completed; game.global_vars[1] = 1";
                    SetQuestState(1, QuestState.Completed);
                    SetGlobalVar(1, 1);
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
                case 22:
                case 26:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 7);
                    return true;
                case 23:
                case 25:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 9);
                    return true;
                case 24:
                case 27:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 4);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
