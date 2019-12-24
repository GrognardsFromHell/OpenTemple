
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
    [DialogScript(35)]
    public class BrotherSmythReplacementDialog : BrotherSmythReplacement, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                    originalScript = "not npc.has_met( pc ) and game.global_flags[979] != 1";
                    return !npc.HasMet(pc) && !GetGlobalFlag(979);
                case 4:
                    originalScript = "game.quests[100].state == qs_mentioned";
                    return GetQuestState(100) == QuestState.Mentioned;
                case 5:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 12602 ) and game.quests[100].state == qs_accepted";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(12602)) && GetQuestState(100) == QuestState.Accepted;
                case 6:
                    originalScript = "npc.has_met( pc ) or game.global_flags[979] == 1";
                    return npc.HasMet(pc) || GetGlobalFlag(979);
                case 7:
                    originalScript = "npc.has_met( pc )";
                    return npc.HasMet(pc);
                case 21:
                    originalScript = "game.global_vars[36] == 0";
                    return GetGlobalVar(36) == 0;
                case 22:
                    originalScript = "game.global_vars[36] == 1";
                    return GetGlobalVar(36) == 1;
                case 23:
                    originalScript = "game.quests[100].state == qs_unknown";
                    return GetQuestState(100) == QuestState.Unknown;
                case 24:
                    originalScript = "game.areas[3] == 0 and game.story_state >= 2";
                    return !IsAreaKnown(3) && StoryState >= 2;
                case 26:
                    originalScript = "game.quests[73].state != qs_completed and game.quests[65].state != qs_completed and game.global_flags[500] == 1";
                    return GetQuestState(73) != QuestState.Completed && GetQuestState(65) != QuestState.Completed && GetGlobalFlag(500);
                case 71:
                case 75:
                case 81:
                case 101:
                case 401:
                case 441:
                case 471:
                    originalScript = "game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD";
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 72:
                case 76:
                case 102:
                case 402:
                case 442:
                case 472:
                    originalScript = "game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL";
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 73:
                case 77:
                case 83:
                case 103:
                case 403:
                case 443:
                case 473:
                    originalScript = "game.party_alignment == LAWFUL_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL";
                    return PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 79:
                case 478:
                    originalScript = "pc.item_find(12602) != OBJ_HANDLE_NULL";
                    return pc.FindItemByName(12602) != null;
                case 82:
                    originalScript = "game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment ==  CHAOTIC_NEUTRAL";
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 231:
                case 233:
                    originalScript = "pc.money_get() >= 10000";
                    return pc.GetMoney() >= 10000;
                case 232:
                case 234:
                    originalScript = "pc.money_get() < 10000";
                    return pc.GetMoney() < 10000;
                case 261:
                case 262:
                    originalScript = "pc.stat_level_get(stat_level_rogue) >= 1";
                    return pc.GetStat(Stat.level_rogue) >= 1;
                case 263:
                case 264:
                    originalScript = "pc.stat_level_get(stat_level_rogue) < 1";
                    return pc.GetStat(Stat.level_rogue) < 1;
                case 313:
                case 333:
                    originalScript = "(pc.skill_level_get(npc,skill_intimidate) >= 8) and (game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL)";
                    return (pc.GetSkillLevel(npc, SkillId.intimidate) >= 8) && (PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL);
                case 314:
                case 332:
                    originalScript = "(pc.skill_level_get(npc,skill_diplomacy) >= 8) and (game.party_alignment == CHAOTIC_GOOD or game.party_alignment == CHAOTIC_NEUTRAL)";
                    return (pc.GetSkillLevel(npc, SkillId.diplomacy) >= 8) && (PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.CHAOTIC_NEUTRAL);
                case 341:
                case 342:
                case 343:
                    originalScript = "game.global_flags[500] == 0";
                    return !GetGlobalFlag(500);
                case 344:
                case 345:
                case 346:
                    originalScript = "game.global_flags[500] == 1";
                    return GetGlobalFlag(500);
                case 372:
                case 492:
                    originalScript = "game.party_alignment == CHAOTIC_GOOD or game.party_alignment == CHAOTIC_NEUTRAL";
                    return PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 373:
                case 375:
                case 376:
                case 493:
                case 495:
                case 496:
                    originalScript = "game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL";
                    return PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 411:
                    originalScript = "game.party_alignment == LAWFUL_GOOD";
                    return PartyAlignment == Alignment.LAWFUL_GOOD;
                case 412:
                    originalScript = "game.party_alignment == NEUTRAL_GOOD";
                    return PartyAlignment == Alignment.NEUTRAL_GOOD;
                case 413:
                    originalScript = "game.party_alignment == CHAOTIC_GOOD";
                    return PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 414:
                    originalScript = "game.party_alignment == LAWFUL_NEUTRAL";
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL;
                case 415:
                    originalScript = "game.party_alignment == TRUE_NEUTRAL";
                    return PartyAlignment == Alignment.NEUTRAL;
                case 416:
                    originalScript = "game.party_alignment == CHAOTIC_NEUTRAL";
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 417:
                    originalScript = "game.party_alignment == LAWFUL_EVIL";
                    return PartyAlignment == Alignment.LAWFUL_EVIL;
                case 418:
                    originalScript = "game.party_alignment == NEUTRAL_EVIL";
                    return PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 419:
                    originalScript = "game.party_alignment == CHAOTIC_EVIL";
                    return PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 475:
                    originalScript = "(pc.item_find(12602) != OBJ_HANDLE_NULL) and (game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD)";
                    return (pc.FindItemByName(12602) != null) && (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD);
                case 476:
                    originalScript = "(pc.item_find(12602) != OBJ_HANDLE_NULL) and (game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL)";
                    return (pc.FindItemByName(12602) != null) && (PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL);
                case 477:
                    originalScript = "(pc.item_find(12602) != OBJ_HANDLE_NULL) and (game.party_alignment == LAWFUL_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL)";
                    return (pc.FindItemByName(12602) != null) && (PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL);
                case 481:
                    originalScript = "game.global_vars[970] == 0";
                    return GetGlobalVar(970) == 0;
                case 482:
                    originalScript = "game.global_vars[705] != 2 and game.global_vars[974] == 1";
                    return GetGlobalVar(705) != 2 && GetGlobalVar(974) == 1;
                case 483:
                    originalScript = "game.global_vars[705] == 2 and game.global_vars[974] == 1";
                    return GetGlobalVar(705) == 2 && GetGlobalVar(974) == 1;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 70:
                    originalScript = "game.quests[100].state = qs_mentioned";
                    SetQuestState(100, QuestState.Mentioned);
                    break;
                case 71:
                case 72:
                case 73:
                case 74:
                case 79:
                case 471:
                case 472:
                case 473:
                case 474:
                case 475:
                case 476:
                case 477:
                case 478:
                    originalScript = "game.quests[100].state = qs_accepted";
                    SetQuestState(100, QuestState.Accepted);
                    break;
                case 81:
                case 82:
                case 83:
                case 84:
                    originalScript = "game.quests[100].state = qs_completed";
                    SetQuestState(100, QuestState.Completed);
                    break;
                case 91:
                case 92:
                    originalScript = "party_transfer_to( npc, 12602 )";
                    Utilities.party_transfer_to(npc, 12602);
                    break;
                case 100:
                    originalScript = "game.global_flags[979] = 1";
                    SetGlobalFlag(979, true);
                    break;
                case 110:
                    originalScript = "game.areas[3] = 1; game.story_state = 3";
                    MakeAreaKnown(3);
                    StoryState = 3;
                    ;
                    break;
                case 112:
                case 113:
                    originalScript = "game.worldmap_travel_by_dialog(3)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 141:
                    originalScript = "npc.reaction_adj( pc,-5)";
                    npc.AdjustReaction(pc, -5);
                    break;
                case 170:
                case 180:
                    originalScript = "game.global_vars[36] = 1";
                    SetGlobalVar(36, 1);
                    break;
                case 210:
                    originalScript = "game.picker( npc, spell_cure_light_wounds, should_heal_hp_on, [ 220, 0, 230 ] )";
                    // FIXME: picker;
                    break;
                case 231:
                case 233:
                    originalScript = "pc.money_adj(-10000); npc.cast_spell( spell_cure_light_wounds, picker_obj )";
                    pc.AdjustMoney(-10000);
                    npc.CastSpell(WellKnownSpells.CureLightWounds, PickedObject);
                    ;
                    break;
                case 240:
                    originalScript = "npc.spells_pending_to_memorized()";
                    npc.PendingSpellsToMemorized();
                    break;
                case 250:
                    originalScript = "game.global_vars[705] = 1";
                    SetGlobalVar(705, 1);
                    break;
                case 251:
                case 252:
                    originalScript = "game.global_vars[974] = 1";
                    SetGlobalVar(974, 1);
                    break;
                case 330:
                    originalScript = "game.global_vars[705] = 2";
                    SetGlobalVar(705, 2);
                    break;
                case 352:
                case 362:
                case 375:
                case 376:
                case 495:
                case 496:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
                    break;
                case 370:
                    originalScript = "pc.money_adj(13072); create_item_in_inventory(12589,pc)";
                    pc.AdjustMoney(13072);
                    Utilities.create_item_in_inventory(12589, pc);
                    ;
                    break;
                case 390:
                    originalScript = "game.global_vars[970] = 1";
                    SetGlobalVar(970, 1);
                    break;
                case 450:
                    originalScript = "game.quests[104].state = qs_mentioned";
                    SetQuestState(104, QuestState.Mentioned);
                    break;
                case 460:
                    originalScript = "game.quests[73].state = qs_mentioned";
                    SetQuestState(73, QuestState.Mentioned);
                    break;
                case 490:
                    originalScript = "pc.money_adj(13072); create_item_in_inventory(12589,pc); create_item_in_inventory(11050,pc)";
                    pc.AdjustMoney(13072);
                    Utilities.create_item_in_inventory(12589, pc);
                    Utilities.create_item_in_inventory(11050, pc);
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
                case 313:
                case 333:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 8);
                    return true;
                case 314:
                case 332:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 8);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
