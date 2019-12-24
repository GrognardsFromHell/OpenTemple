
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
    [DialogScript(12)]
    public class JarooDialog : Jaroo, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "not npc.has_met(pc)";
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                case 12:
                case 13:
                    originalScript = "npc.has_met(pc)";
                    return npc.HasMet(pc);
                case 6:
                case 7:
                    originalScript = "npc.has_met(pc) and game.party_alignment == TRUE_NEUTRAL and game.global_flags[18] == 0";
                    return npc.HasMet(pc) && PartyAlignment == Alignment.NEUTRAL && !GetGlobalFlag(18);
                case 8:
                case 9:
                    originalScript = "game.story_state >= 2 and game.party_alignment == TRUE_NEUTRAL and game.global_flags[19] == 0";
                    return StoryState >= 2 && PartyAlignment == Alignment.NEUTRAL && !GetGlobalFlag(19);
                case 10:
                case 11:
                case 89:
                case 90:
                    originalScript = "game.global_flags[17] == 1 and game.quests[5].state != qs_completed";
                    return GetGlobalFlag(17) && GetQuestState(5) != QuestState.Completed;
                case 28:
                case 29:
                case 93:
                case 94:
                    originalScript = "game.party_alignment == TRUE_NEUTRAL and game.global_flags[18] == 0";
                    return PartyAlignment == Alignment.NEUTRAL && !GetGlobalFlag(18);
                case 41:
                case 517:
                case 519:
                    originalScript = "pc.money_get() >= 10000";
                    return pc.GetMoney() >= 10000;
                case 42:
                    originalScript = "pc.money_get() >= 2000";
                    return pc.GetMoney() >= 2000;
                case 43:
                    originalScript = "pc.money_get() >= 500";
                    return pc.GetMoney() >= 500;
                case 44:
                    originalScript = "pc.money_get() >= 100";
                    return pc.GetMoney() >= 100;
                case 81:
                case 82:
                    originalScript = "pc.item_find( 3000 ) != OBJ_HANDLE_NULL or pc.item_find(5800) != OBJ_HANDLE_NULL";
                    return pc.FindItemByName(3000) != null || pc.FindItemByName(5800) != null;
                case 87:
                case 88:
                    originalScript = "game.global_flags[12] == 1 and game.quests[8].state == qs_accepted and game.global_flags[13] == 0";
                    return GetGlobalFlag(12) && GetQuestState(8) == QuestState.Accepted && !GetGlobalFlag(13);
                case 161:
                case 162:
                    originalScript = "pc.item_find( 3000 ) != OBJ_HANDLE_NULL";
                    return pc.FindItemByName(3000) != null;
                case 163:
                case 164:
                    originalScript = "pc.item_find(5800) != OBJ_HANDLE_NULL and game.global_flags[3] == 0";
                    return pc.FindItemByName(5800) != null && !GetGlobalFlag(3);
                case 191:
                case 193:
                    originalScript = "game.quests[3].state == qs_accepted";
                    return GetQuestState(3) == QuestState.Accepted;
                case 192:
                case 194:
                    originalScript = "game.quests[3].state <= qs_mentioned";
                    return GetQuestState(3) <= QuestState.Mentioned;
                case 204:
                case 205:
                    originalScript = "( game.quests[6].state == qs_mentioned or game.quests[6].state == qs_accepted ) and game.global_flags[10] == 1";
                    return (GetQuestState(6) == QuestState.Mentioned || GetQuestState(6) == QuestState.Accepted) && GetGlobalFlag(10);
                case 206:
                case 207:
                    originalScript = "game.quests[5].state == qs_accepted";
                    return GetQuestState(5) == QuestState.Accepted;
                case 261:
                    originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 4";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 4;
                case 353:
                case 354:
                    originalScript = "game.areas[3] == 1";
                    return IsAreaKnown(3);
                case 355:
                case 356:
                    originalScript = "game.areas[3] == 0";
                    return !IsAreaKnown(3);
                case 372:
                case 373:
                case 374:
                case 376:
                case 377:
                case 378:
                case 551:
                case 552:
                    originalScript = "game.global_vars[2] >= 100";
                    return GetGlobalVar(2) >= 100;
                case 379:
                case 380:
                case 381:
                case 382:
                case 383:
                case 384:
                case 553:
                case 558:
                    originalScript = "game.global_vars[2] <= 99";
                    return GetGlobalVar(2) <= 99;
                case 401:
                case 403:
                case 421:
                case 423:
                    originalScript = "pc.money_get() >= 21000";
                    return pc.GetMoney() >= 21000;
                case 437:
                case 439:
                    originalScript = "pc.money_get() >= 7000";
                    return pc.GetMoney() >= 7000;
                case 463:
                case 465:
                    originalScript = "pc.money_get() >= 30000";
                    return pc.GetMoney() >= 30000;
                case 481:
                case 483:
                case 501:
                case 503:
                    originalScript = "pc.money_get() >= 25000";
                    return pc.GetMoney() >= 25000;
                case 543:
                case 545:
                    originalScript = "pc.money_get() >= 35000";
                    return pc.GetMoney() >= 35000;
                case 563:
                case 565:
                    originalScript = "pc.money_get() >= 22500";
                    return pc.GetMoney() >= 22500;
                case 573:
                case 575:
                    originalScript = "pc.money_get() >= 27500";
                    return pc.GetMoney() >= 27500;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                    originalScript = "game.global_flags[1] = 1";
                    SetGlobalFlag(1, true);
                    break;
                case 6:
                case 7:
                case 93:
                case 94:
                    originalScript = "game.global_flags[18] = 1; game.global_flags[67] = 1";
                    SetGlobalFlag(18, true);
                    SetGlobalFlag(67, true);
                    ;
                    break;
                case 8:
                case 9:
                    originalScript = "game.global_flags[19] = 1";
                    SetGlobalFlag(19, true);
                    break;
                case 28:
                case 29:
                    originalScript = "game.global_flags[18] = 1";
                    SetGlobalFlag(18, true);
                    break;
                case 41:
                    originalScript = "game.global_vars[2] = game.global_vars[2] + 100;  pc.money_adj(-10000)";
                    SetGlobalVar(2, GetGlobalVar(2) + 100);
                    pc.AdjustMoney(-10000);
                    ;
                    break;
                case 42:
                    originalScript = "game.global_vars[2] = game.global_vars[2] + 20; pc.money_adj(-2000)";
                    SetGlobalVar(2, GetGlobalVar(2) + 20);
                    pc.AdjustMoney(-2000);
                    ;
                    break;
                case 43:
                    originalScript = "game.global_vars[2] = game.global_vars[2] + 5; pc.money_adj(-500)";
                    SetGlobalVar(2, GetGlobalVar(2) + 5);
                    pc.AdjustMoney(-500);
                    ;
                    break;
                case 44:
                    originalScript = "game.global_vars[2] = game.global_vars[2] + 1; pc.money_adj(-100)";
                    SetGlobalVar(2, GetGlobalVar(2) + 1);
                    pc.AdjustMoney(-100);
                    ;
                    break;
                case 100:
                    originalScript = "game.quests[4].state = qs_mentioned";
                    SetQuestState(4, QuestState.Mentioned);
                    break;
                case 140:
                case 150:
                    originalScript = "game.global_flags[23] = 1";
                    SetGlobalFlag(23, true);
                    break;
                case 170:
                    originalScript = "game.global_flags[3] = 1";
                    SetGlobalFlag(3, true);
                    break;
                case 230:
                    originalScript = "game.global_flags[8] = 1";
                    SetGlobalFlag(8, true);
                    break;
                case 270:
                case 310:
                    originalScript = "game.global_flags[13] = 1";
                    SetGlobalFlag(13, true);
                    break;
                case 323:
                    originalScript = "game.areas[2] = 1; game.story_state = 1";
                    MakeAreaKnown(2);
                    StoryState = 1;
                    ;
                    break;
                case 324:
                case 325:
                    originalScript = "game.worldmap_travel_by_dialog(2)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 336:
                    originalScript = "game.quests[5].state = qs_completed";
                    SetQuestState(5, QuestState.Completed);
                    break;
                case 346:
                    originalScript = "game.quests[26].state = qs_completed";
                    SetQuestState(26, QuestState.Completed);
                    break;
                case 358:
                    originalScript = "game.areas[3] = 1; game.story_state = 3";
                    MakeAreaKnown(3);
                    StoryState = 3;
                    ;
                    break;
                case 359:
                case 360:
                    originalScript = "game.worldmap_travel_by_dialog(3)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 390:
                    originalScript = "game.picker( npc, spell_remove_disease, should_heal_disease_on, [ 450, 370, 400 ] )";
                    // FIXME: picker;
                    break;
                case 401:
                case 403:
                    originalScript = "pc.money_adj(-21000); npc.cast_spell( spell_remove_disease, picker_obj )";
                    pc.AdjustMoney(-21000);
                    npc.CastSpell(WellKnownSpells.RemoveDisease, PickedObject);
                    ;
                    break;
                case 405:
                case 485:
                    originalScript = "npc.spells_pending_to_memorized()";
                    npc.PendingSpellsToMemorized();
                    break;
                case 410:
                    originalScript = "game.picker( npc, spell_neutralize_poison, should_heal_poison_on, [ 450, 370, 420 ] )";
                    // FIXME: picker;
                    break;
                case 421:
                case 423:
                    originalScript = "pc.money_adj(-21000); npc.cast_spell( spell_cure_neutralize_poison, picker_obj )";
                    pc.AdjustMoney(-21000);
                    npc.CastSpell(WellKnownSpells.NeutralizePoison, PickedObject);
                    break;
                case 434:
                    originalScript = "game.picker( npc, spell_cure_light_wounds, should_heal_hp_on, [ 450, 370, 436 ] )";
                    // FIXME: picker;
                    break;
                case 437:
                case 439:
                    originalScript = "pc.money_adj(-7000); npc.cast_spell( spell_cure_light_wounds, picker_obj )";
                    pc.AdjustMoney(-7000);
                    npc.CastSpell(WellKnownSpells.CureLightWounds, PickedObject);
                    ;
                    break;
                case 460:
                    originalScript = "game.picker( npc, spell_cure_serious_wounds, should_heal_hp_on, [ 450, 370, 462 ] )";
                    // FIXME: picker;
                    break;
                case 463:
                case 465:
                    originalScript = "pc.money_adj(-30000); npc.cast_spell( spell_cure_serious_wounds, picker_obj )";
                    pc.AdjustMoney(-30000);
                    npc.CastSpell(WellKnownSpells.CureSeriousWounds, PickedObject);
                    ;
                    break;
                case 470:
                    originalScript = "game.picker( npc, spell_remove_disease, should_heal_disease_on, [ 450, 370, 480 ] )";
                    // FIXME: picker;
                    break;
                case 481:
                case 483:
                    originalScript = "pc.money_adj(-25000); npc.cast_spell( spell_remove_disease, picker_obj )";
                    pc.AdjustMoney(-25000);
                    npc.CastSpell(WellKnownSpells.RemoveDisease, PickedObject);
                    ;
                    break;
                case 490:
                    originalScript = "game.picker( npc, spell_neutralize_poison, should_heal_poison_on, [ 450, 370, 500 ] )";
                    // FIXME: picker;
                    break;
                case 501:
                case 503:
                    originalScript = "pc.money_adj(-25000); npc.cast_spell( spell_cure_neutralize_poison, picker_obj )";
                    pc.AdjustMoney(-25000);
                    npc.CastSpell(WellKnownSpells.NeutralizePoison, PickedObject);
                    ;
                    break;
                case 514:
                    originalScript = "game.picker( npc, spell_cure_light_wounds, should_heal_hp_on, [ 450, 370, 516 ] )";
                    // FIXME: picker;
                    break;
                case 517:
                case 519:
                    originalScript = "pc.money_adj(-10000); npc.cast_spell( spell_cure_light_wounds, picker_obj )";
                    pc.AdjustMoney(-10000);
                    npc.CastSpell(WellKnownSpells.CureLightWounds, PickedObject);
                    ;
                    break;
                case 540:
                    originalScript = "game.picker( npc, spell_cure_serious_wounds, should_heal_hp_on, [ 450, 370, 542 ] )";
                    // FIXME: picker;
                    break;
                case 543:
                case 545:
                    originalScript = "pc.money_adj(-35000); npc.cast_spell( spell_cure_serious_wounds, picker_obj )";
                    pc.AdjustMoney(-35000);
                    npc.CastSpell(WellKnownSpells.CureSeriousWounds, PickedObject);
                    ;
                    break;
                case 560:
                case 570:
                    originalScript = "game.picker( npc, spell_reincarnation, should_resurrect_on, [ 450, 370, 562 ] )";
                    // FIXME: picker;
                    break;
                case 563:
                case 565:
                    originalScript = "pc.money_adj(-22500); npc.cast_spell( spell_reincarnation, picker_obj )";
                    pc.AdjustMoney(-22500);
                    npc.CastSpell(WellKnownSpells.Reincarnation, PickedObject);
                    ;
                    break;
                case 573:
                case 575:
                    originalScript = "pc.money_adj(-27500); npc.cast_spell( spell_reincarnation, picker_obj )";
                    pc.AdjustMoney(-27500);
                    npc.CastSpell(WellKnownSpells.Reincarnation, PickedObject);
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
                case 261:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 4);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
