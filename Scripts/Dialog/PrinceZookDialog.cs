
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
    [DialogScript(383)]
    public class PrinceZookDialog : PrinceZook, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                    originalScript = "not npc.has_met(pc) and (game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == LAWFUL_NEUTRAL)";
                    return !npc.HasMet(pc) && (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL);
                case 3:
                    originalScript = "not npc.has_met(pc) and (game.party_alignment == CHAOTIC_GOOD or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == LAWFUL_EVIL)";
                    return !npc.HasMet(pc) && (PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL);
                case 4:
                    originalScript = "not npc.has_met(pc) and (game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL)";
                    return !npc.HasMet(pc) && (PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL);
                case 5:
                    originalScript = "npc.has_met(pc) and (game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == LAWFUL_NEUTRAL)";
                    return npc.HasMet(pc) && (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL);
                case 6:
                    originalScript = "npc.has_met(pc) and (game.party_alignment == CHAOTIC_GOOD or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == LAWFUL_EVIL)";
                    return npc.HasMet(pc) && (PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL);
                case 7:
                    originalScript = "npc.has_met(pc) and (game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL)";
                    return npc.HasMet(pc) && (PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL);
                case 11:
                    originalScript = "game.global_flags[985] == 1 and game.quests[69].state == qs_unknown";
                    return GetGlobalFlag(985) && GetQuestState(69) == QuestState.Unknown;
                case 12:
                case 22:
                    originalScript = "game.global_vars[999] >= 1 and game.global_vars[999] <= 14 and game.quests[69].state == qs_accepted";
                    return GetGlobalVar(999) >= 1 && GetGlobalVar(999) <= 14 && GetQuestState(69) == QuestState.Accepted;
                case 13:
                case 23:
                    originalScript = "game.global_vars[999] >= 15 and game.quests[69].state != qs_completed";
                    return GetGlobalVar(999) >= 15 && GetQuestState(69) != QuestState.Completed;
                case 14:
                    originalScript = "game.global_flags[985] == 0 and game.quests[69].state == qs_unknown";
                    return !GetGlobalFlag(985) && GetQuestState(69) == QuestState.Unknown;
                case 15:
                    originalScript = "game.global_flags[985] == 1 and game.global_flags[991] == 1 and (game.quests[69].state == qs_mentioned or game.quests[69].state == qs_accepted)";
                    return GetGlobalFlag(985) && GetGlobalFlag(991) && (GetQuestState(69) == QuestState.Mentioned || GetQuestState(69) == QuestState.Accepted);
                case 16:
                    originalScript = "game.global_flags[985] == 1 and game.global_flags[991] == 0 and (game.quests[69].state == qs_mentioned or game.quests[69].state == qs_accepted)";
                    return GetGlobalFlag(985) && !GetGlobalFlag(991) && (GetQuestState(69) == QuestState.Mentioned || GetQuestState(69) == QuestState.Accepted);
                case 21:
                    originalScript = "game.quests[69].state == qs_mentioned";
                    return GetQuestState(69) == QuestState.Mentioned;
                case 24:
                    originalScript = "(game.quests[77].state == qs_accepted or game.quests[77].state == qs_mentioned) or (game.quests[84].state == qs_accepted or game.quests[84].state == qs_mentioned) or (game.quests[66].state == qs_accepted or game.quests[66].state == qs_mentioned) or (game.quests[67].state == qs_accepted or game.quests[67].state == qs_mentioned)";
                    return (GetQuestState(77) == QuestState.Accepted || GetQuestState(77) == QuestState.Mentioned) || (GetQuestState(84) == QuestState.Accepted || GetQuestState(84) == QuestState.Mentioned) || (GetQuestState(66) == QuestState.Accepted || GetQuestState(66) == QuestState.Mentioned) || (GetQuestState(67) == QuestState.Accepted || GetQuestState(67) == QuestState.Mentioned);
                case 25:
                    originalScript = "game.quests[69].state == qs_completed";
                    return GetQuestState(69) == QuestState.Completed;
                case 26:
                    originalScript = "game.quests[89].state == qs_mentioned";
                    return GetQuestState(89) == QuestState.Mentioned;
                case 27:
                    originalScript = "game.quests[89].state == qs_completed and not npc_get(npc,5)";
                    return GetQuestState(89) == QuestState.Completed && !ScriptDaemon.npc_get(npc, 5);
                case 51:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == LAWFUL_NEUTRAL)";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL);
                case 52:
                    originalScript = "(game.party_alignment == CHAOTIC_GOOD or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == LAWFUL_EVIL)";
                    return (PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL);
                case 53:
                    originalScript = "(game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL)";
                    return (PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL);
                case 61:
                    originalScript = "game.global_flags[985] == 0";
                    return !GetGlobalFlag(985);
                case 62:
                    originalScript = "game.global_flags[985] == 1";
                    return GetGlobalFlag(985);
                case 71:
                case 151:
                    originalScript = "game.global_flags[968] == 0";
                    return !GetGlobalFlag(968);
                case 72:
                    originalScript = "game.global_flags[991] == 1 and game.global_flags[968] == 1";
                    return GetGlobalFlag(991) && GetGlobalFlag(968);
                case 73:
                    originalScript = "game.global_flags[991] == 0 and game.global_flags[968] == 1";
                    return !GetGlobalFlag(991) && GetGlobalFlag(968);
                case 111:
                    originalScript = "game.quests[74].state == qs_accepted";
                    return GetQuestState(74) == QuestState.Accepted;
                case 112:
                    originalScript = "game.quests[74].state == qs_mentioned";
                    return GetQuestState(74) == QuestState.Mentioned;
                case 161:
                    originalScript = "game.global_flags[991] == 1";
                    return GetGlobalFlag(991);
                case 162:
                    originalScript = "game.global_flags[991] == 0";
                    return !GetGlobalFlag(991);
                case 172:
                    originalScript = "anyone( pc.group_list(), \"has_wielded\", 3007 )";
                    return pc.GetPartyMembers().Any(o => o.HasEquippedByName(3007));
                case 173:
                    originalScript = "pc.stat_level_get(stat_race) == race_gnome";
                    return pc.GetRace() == RaceId.svirfneblin;
                case 191:
                    originalScript = "(game.quests[77].state == qs_accepted or game.quests[77].state == qs_mentioned) and game.global_flags[989] == 0 and game.global_flags[946] == 0 and game.global_flags[863] == 0 and not npc_get(npc,1)";
                    return (GetQuestState(77) == QuestState.Accepted || GetQuestState(77) == QuestState.Mentioned) && !GetGlobalFlag(989) && !GetGlobalFlag(946) && !GetGlobalFlag(863) && !ScriptDaemon.npc_get(npc, 1);
                case 192:
                    originalScript = "(game.quests[84].state == qs_accepted or game.quests[84].state == qs_mentioned) and game.global_flags[973] == 0 and not npc_get(npc,2)";
                    return (GetQuestState(84) == QuestState.Accepted || GetQuestState(84) == QuestState.Mentioned) && !GetGlobalFlag(973) && !ScriptDaemon.npc_get(npc, 2);
                case 193:
                    originalScript = "(game.quests[67].state == qs_accepted or game.quests[67].state == qs_mentioned) and game.global_flags[989] == 0 and not npc_get(npc,3)";
                    return (GetQuestState(67) == QuestState.Accepted || GetQuestState(67) == QuestState.Mentioned) && !GetGlobalFlag(989) && !ScriptDaemon.npc_get(npc, 3);
                case 194:
                    originalScript = "(game.quests[66].state == qs_accepted or game.quests[66].state == qs_mentioned) and game.global_flags[989] == 0 and not npc_get(npc,4)";
                    return (GetQuestState(66) == QuestState.Accepted || GetQuestState(66) == QuestState.Mentioned) && !GetGlobalFlag(989) && !ScriptDaemon.npc_get(npc, 4);
                case 241:
                    originalScript = "game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == LAWFUL_NEUTRAL";
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL;
                case 242:
                    originalScript = "game.party_alignment == CHAOTIC_GOOD or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == LAWFUL_EVIL";
                    return PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL;
                case 243:
                    originalScript = "game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL";
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 10:
                    originalScript = "game.global_flags[969] = 1";
                    SetGlobalFlag(969, true);
                    break;
                case 27:
                    originalScript = "npc_set(npc,5)";
                    ScriptDaemon.npc_set(npc, 5);
                    break;
                case 50:
                    originalScript = "game.quests[69].state = qs_completed";
                    SetQuestState(69, QuestState.Completed);
                    break;
                case 60:
                case 120:
                    originalScript = "game.quests[69].state = qs_mentioned";
                    SetQuestState(69, QuestState.Mentioned);
                    break;
                case 70:
                case 150:
                    originalScript = "game.global_flags[981] = 1";
                    SetGlobalFlag(981, true);
                    break;
                case 80:
                    originalScript = "game.quests[69].state = qs_accepted";
                    SetQuestState(69, QuestState.Accepted);
                    break;
                case 130:
                    originalScript = "npc.item_transfer_to(pc,4407); game.quests[69].state = qs_accepted";
                    npc.TransferItemByNameTo(pc, 4407);
                    SetQuestState(69, QuestState.Accepted);
                    ;
                    break;
                case 160:
                case 180:
                    originalScript = "game.global_vars[977] = 1";
                    SetGlobalVar(977, 1);
                    break;
                case 191:
                    originalScript = "npc_set(npc,1)";
                    ScriptDaemon.npc_set(npc, 1);
                    break;
                case 192:
                    originalScript = "npc_set(npc,2)";
                    ScriptDaemon.npc_set(npc, 2);
                    break;
                case 193:
                    originalScript = "npc_set(npc,3)";
                    ScriptDaemon.npc_set(npc, 3);
                    break;
                case 194:
                    originalScript = "npc_set(npc,4)";
                    ScriptDaemon.npc_set(npc, 4);
                    break;
                case 240:
                    originalScript = "game.global_vars[939] = 2; game.quests[108].state = qs_completed";
                    SetGlobalVar(939, 2);
                    SetQuestState(108, QuestState.Completed);
                    ;
                    break;
                case 311:
                case 350:
                case 361:
                    originalScript = "game.quests[89].state = qs_accepted";
                    SetQuestState(89, QuestState.Accepted);
                    break;
                case 410:
                    originalScript = "game.quests[89].state = qs_mentioned";
                    SetQuestState(89, QuestState.Mentioned);
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
