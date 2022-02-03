
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
    [DialogScript(22)]
    public class TeamsterDialog : Teamster, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "not npc.has_met(pc)";
                    return !npc.HasMet(pc);
                case 4:
                    originalScript = "npc.has_met(pc)";
                    return npc.HasMet(pc);
                case 11:
                case 12:
                    originalScript = "pc.skill_level_get(npc,skill_gather_information) >= 4";
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 4;
                case 13:
                case 14:
                    originalScript = "game.global_flags[6] == 1 and game.global_flags[4] == 0";
                    return GetGlobalFlag(6) && !GetGlobalFlag(4);
                case 15:
                case 16:
                case 141:
                case 142:
                    originalScript = "game.global_flags[4] == 1";
                    return GetGlobalFlag(4);
                case 51:
                case 52:
                    originalScript = "game.quests[3].state == qs_mentioned or game.quests[3].state == qs_accepted";
                    return GetQuestState(3) == QuestState.Mentioned || GetQuestState(3) == QuestState.Accepted;
                case 131:
                case 132:
                    originalScript = "game.global_flags[5] == 1";
                    return GetGlobalFlag(5);
                case 133:
                case 134:
                    originalScript = "game.global_flags[7] == 1";
                    return GetGlobalFlag(7);
                case 143:
                case 144:
                    originalScript = "game.global_flags[4] == 0 and game.quests[3].state >= qs_mentioned";
                    return !GetGlobalFlag(4) && GetQuestState(3) >= QuestState.Mentioned;
                case 145:
                case 146:
                    originalScript = "game.global_flags[4] == 0 and game.quests[3].state == qs_unknown";
                    return !GetGlobalFlag(4) && GetQuestState(3) == QuestState.Unknown;
                case 151:
                case 152:
                    originalScript = "game.party_alignment == LAWFUL_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == NEUTRAL_GOOD";
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL_GOOD;
                case 153:
                case 154:
                    originalScript = "game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_GOOD";
                    return PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 155:
                case 156:
                    originalScript = "game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == LAWFUL_EVIL or game.party_alignment == CHAOTIC_NEUTRAL";
                    return PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 20:
                    originalScript = "game.global_flags[40] = 1";
                    SetGlobalFlag(40, true);
                    break;
                case 153:
                case 154:
                    originalScript = "make_worry( npc, pc )";
                    make_worry(npc, pc);
                    break;
                case 155:
                case 156:
                case 170:
                    originalScript = "make_hate( npc, pc )";
                    make_hate(npc, pc);
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
                case 11:
                case 12:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 4);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
