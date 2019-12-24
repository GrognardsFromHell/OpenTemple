
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

namespace VanillaScripts.Dialog
{
    [DialogScript(118)]
    public class SkoleDialog : Skole, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                    originalScript = "not npc.has_met( pc )";
                    return !npc.HasMet(pc);
                case 3:
                case 4:
                case 327:
                case 328:
                    originalScript = "game.quests[42].state == qs_mentioned";
                    return GetQuestState(42) == QuestState.Mentioned;
                case 5:
                case 6:
                case 329:
                case 330:
                    originalScript = "game.quests[42].state == qs_accepted";
                    return GetQuestState(42) == QuestState.Accepted;
                case 7:
                case 8:
                    originalScript = "game.quests[42].state == qs_completed and game.global_flags[102] == 1";
                    return GetQuestState(42) == QuestState.Completed && GetGlobalFlag(102);
                case 9:
                case 10:
                    originalScript = "game.quests[42].state == qs_completed and game.global_flags[102] == 0 and game.global_flags[103] == 0";
                    return GetQuestState(42) == QuestState.Completed && !GetGlobalFlag(102) && !GetGlobalFlag(103);
                case 11:
                case 12:
                    originalScript = "npc.has_met( pc )";
                    return npc.HasMet(pc);
                case 22:
                case 23:
                    originalScript = "game.global_flags[369] == 0";
                    return !GetGlobalFlag(369);
                case 41:
                    originalScript = "game.global_flags[97] == 1";
                    return GetGlobalFlag(97);
                case 42:
                case 43:
                    originalScript = "pc.skill_level_get(npc, skill_sense_motive) >= 5";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 5;
                case 51:
                case 52:
                    originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 7";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 7;
                case 61:
                case 62:
                case 105:
                case 106:
                case 321:
                case 322:
                    originalScript = "game.quests[42].state == qs_unknown and game.global_flags[369] == 0";
                    return GetQuestState(42) == QuestState.Unknown && !GetGlobalFlag(369);
                case 63:
                case 64:
                    originalScript = "pc.money_get() <= 50000";
                    return pc.GetMoney() <= 50000;
                case 65:
                case 66:
                    originalScript = "pc.money_get() >= 50001";
                    return pc.GetMoney() >= 50001;
                case 81:
                case 83:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD) or (game.party_alignment == NEUTRAL_GOOD) or (game.party_alignment == CHAOTIC_GOOD)";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD) || (PartyAlignment == Alignment.NEUTRAL_GOOD) || (PartyAlignment == Alignment.CHAOTIC_GOOD);
                case 82:
                case 84:
                    originalScript = "(game.party_alignment == LAWFUL_NEUTRAL) or (game.party_alignment == TRUE_NEUTRAL) or (game.party_alignment == CHAOTIC_NEUTRAL) or (game.party_alignment == LAWFUL_EVIL) or (game.party_alignment == NEUTRAL_EVIL) or (game.party_alignment == CHAOTIC_EVIL)";
                    return (PartyAlignment == Alignment.LAWFUL_NEUTRAL) || (PartyAlignment == Alignment.NEUTRAL) || (PartyAlignment == Alignment.CHAOTIC_NEUTRAL) || (PartyAlignment == Alignment.LAWFUL_EVIL) || (PartyAlignment == Alignment.NEUTRAL_EVIL) || (PartyAlignment == Alignment.CHAOTIC_EVIL);
                case 101:
                case 102:
                case 103:
                case 104:
                case 325:
                case 326:
                    originalScript = "game.quests[42].state == qs_completed and game.global_flags[102] == 0";
                    return GetQuestState(42) == QuestState.Completed && !GetGlobalFlag(102);
                case 183:
                case 184:
                    originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 8";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 8;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 9:
                case 10:
                    originalScript = "game.global_flags[103] = 1";
                    SetGlobalFlag(103, true);
                    break;
                case 120:
                    originalScript = "npc.reaction_adj( pc,+10)";
                    npc.AdjustReaction(pc, +10);
                    break;
                case 150:
                    originalScript = "game.quests[42].state = qs_mentioned";
                    SetQuestState(42, QuestState.Mentioned);
                    break;
                case 160:
                    originalScript = "npc.reaction_adj( pc,-5)";
                    npc.AdjustReaction(pc, -5);
                    break;
                case 181:
                case 182:
                    originalScript = "npc.item_transfer_to(pc,5804)";
                    npc.TransferItemByNameTo(pc, 5804);
                    break;
                case 191:
                case 192:
                    originalScript = "npc.item_transfer_to(pc,5804); npc.item_transfer_to_by_proto(pc,6051)";
                    npc.TransferItemByNameTo(pc, 5804);
                    npc.TransferItemByProtoTo(pc, 6051);
                    ;
                    break;
                case 200:
                    originalScript = "pc.condition_add_with_args(\"Fallen_Paladin\",0,0)";
                    pc.AddCondition("Fallen_Paladin", 0, 0);
                    break;
                case 210:
                    originalScript = "game.quests[42].state = qs_accepted; prepare_goons( npc )";
                    SetQuestState(42, QuestState.Accepted);
                    prepare_goons(npc);
                    ;
                    break;
                case 291:
                case 301:
                case 302:
                case 361:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
                    break;
                case 310:
                    originalScript = "npc.item_transfer_to(pc,5805)";
                    npc.TransferItemByNameTo(pc, 5805);
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
                case 42:
                case 43:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 5);
                    return true;
                case 51:
                case 52:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 7);
                    return true;
                case 183:
                case 184:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 8);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
