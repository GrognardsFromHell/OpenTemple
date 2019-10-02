
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
    [DialogScript(5)]
    public class CalmertDialog : Calmert, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 3:
                case 6:
                    Trace.Assert(originalScript == "game.party_alignment ==  NEUTRAL_GOOD");
                    return PartyAlignment == Alignment.NEUTRAL_GOOD;
                case 4:
                case 7:
                    Trace.Assert(originalScript == "game.party_alignment ==  NEUTRAL_EVIL");
                    return PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 11:
                case 12:
                    Trace.Assert(originalScript == "game.quests[8].state == qs_mentioned or game.quests[8].state == qs_accepted");
                    return GetQuestState(8) == QuestState.Mentioned || GetQuestState(8) == QuestState.Accepted;
                case 13:
                case 14:
                    Trace.Assert(originalScript == "game.quests[14].state == qs_mentioned");
                    return GetQuestState(14) == QuestState.Mentioned;
                case 15:
                case 16:
                    Trace.Assert(originalScript == "game.global_flags[29] == 1 and game.quests[14].state == qs_accepted");
                    return GetGlobalFlag(29) && GetQuestState(14) == QuestState.Accepted;
                case 17:
                case 33:
                case 37:
                case 45:
                case 55:
                case 56:
                case 71:
                case 81:
                case 131:
                case 145:
                case 173:
                case 183:
                case 188:
                case 191:
                case 192:
                case 201:
                case 211:
                case 224:
                case 253:
                case 261:
                case 271:
                case 281:
                case 329:
                case 431:
                case 455:
                case 456:
                    Trace.Assert(originalScript == "game.global_flags[28] == 0");
                    return !GetGlobalFlag(28);
                case 18:
                case 34:
                case 38:
                case 46:
                case 57:
                case 58:
                case 72:
                case 82:
                case 132:
                case 146:
                case 174:
                case 184:
                case 189:
                case 193:
                case 194:
                case 202:
                case 212:
                case 225:
                case 254:
                case 262:
                case 272:
                case 282:
                case 330:
                case 432:
                case 457:
                case 458:
                    Trace.Assert(originalScript == "game.global_flags[28] == 1");
                    return GetGlobalFlag(28);
                case 19:
                    Trace.Assert(originalScript == "game.global_vars[501] == 2");
                    return GetGlobalVar(501) == 2;
                case 20:
                    Trace.Assert(originalScript == "game.quests[97].state == qs_completed");
                    return GetQuestState(97) == QuestState.Completed;
                case 32:
                case 36:
                case 51:
                case 52:
                case 121:
                case 123:
                case 385:
                case 386:
                    Trace.Assert(originalScript == "pc.money_get() >= 1000");
                    return pc.GetMoney() >= 1000;
                case 43:
                case 44:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 7 and (game.party_alignment ==  CHAOTIC_EVIL or game.party_alignment ==  CHAOTIC_NEUTRAL)");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 7 && (PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL);
                case 53:
                case 54:
                case 451:
                case 452:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 6");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 6;
                case 61:
                case 62:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 5");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 5;
                case 101:
                case 301:
                    Trace.Assert(originalScript == "game.global_flags[21] == 0 and game.party_alignment != NEUTRAL_GOOD");
                    return !GetGlobalFlag(21) && PartyAlignment != Alignment.NEUTRAL_GOOD;
                case 102:
                case 103:
                case 302:
                case 303:
                    Trace.Assert(originalScript == "game.global_flags[22] == 0 and game.party_alignment ==  NEUTRAL_GOOD");
                    return !GetGlobalFlag(22) && PartyAlignment == Alignment.NEUTRAL_GOOD;
                case 104:
                case 304:
                    Trace.Assert(originalScript == "game.global_flags[21] == 0 and game.party_alignment !=  NEUTRAL_GOOD");
                    return !GetGlobalFlag(21) && PartyAlignment != Alignment.NEUTRAL_GOOD;
                case 110:
                case 111:
                case 223:
                case 310:
                case 311:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_deity ) != 16");
                    return pc.GetStat(Stat.deity) != 16;
                case 112:
                case 113:
                case 312:
                case 313:
                    Trace.Assert(originalScript == "game.global_vars[6] >= 100 and game.global_flags[28] == 0");
                    return GetGlobalVar(6) >= 100 && !GetGlobalFlag(28);
                case 114:
                case 115:
                case 314:
                case 315:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_deity ) == 16 and game.quests[14].state == qs_unknown");
                    return pc.GetStat(Stat.deity) == 16 && GetQuestState(14) == QuestState.Unknown;
                case 116:
                case 117:
                case 316:
                case 317:
                    Trace.Assert(originalScript == "game.global_vars[5] >= 8 and game.global_flags[29] == 1 and game.quests[14].state == qs_accepted");
                    return GetGlobalVar(5) >= 8 && GetGlobalFlag(29) && GetQuestState(14) == QuestState.Accepted;
                case 122:
                case 124:
                    Trace.Assert(originalScript == "pc.money_get() >= 10000");
                    return pc.GetMoney() >= 10000;
                case 141:
                case 142:
                    Trace.Assert(originalScript == "game.quests[14].state == qs_unknown");
                    return GetQuestState(14) == QuestState.Unknown;
                case 171:
                case 182:
                case 187:
                    Trace.Assert(originalScript == "game.quests[8].state == qs_accepted");
                    return GetQuestState(8) == QuestState.Accepted;
                case 172:
                    Trace.Assert(originalScript == "game.quests[8].state == qs_mentioned");
                    return GetQuestState(8) == QuestState.Mentioned;
                case 221:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_deity ) == 16");
                    return pc.GetStat(Stat.deity) == 16;
                case 243:
                case 244:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 4 and pc.money_get() >= 200");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 4 && pc.GetMoney() >= 200;
                case 245:
                case 246:
                    Trace.Assert(originalScript == "game.global_flags[30] == 1");
                    return GetGlobalFlag(30);
                case 251:
                case 252:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 4");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 4;
                case 351:
                case 353:
                    Trace.Assert(originalScript == "pc.money_get() >= 8000");
                    return pc.GetMoney() >= 8000;
                case 371:
                case 373:
                    Trace.Assert(originalScript == "pc.money_get() >= 24000");
                    return pc.GetMoney() >= 24000;
                case 421:
                    Trace.Assert(originalScript == "game.quests[97].state == qs_accepted");
                    return GetQuestState(97) == QuestState.Accepted;
                case 422:
                    Trace.Assert(originalScript == "game.quests[97].state != qs_accepted");
                    return GetQuestState(97) != QuestState.Accepted;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 32:
                case 36:
                case 51:
                case 52:
                case 121:
                case 123:
                    Trace.Assert(originalScript == "pc.money_adj(-1000); game.global_vars[6] = game.global_vars[6] + 10");
                    pc.AdjustMoney(-1000);
                    SetGlobalVar(6, GetGlobalVar(6) + 10);
                    ;
                    break;
                case 70:
                case 80:
                case 90:
                    Trace.Assert(originalScript == "game.global_flags[21] = 1; game.map_flags( 5011, 0, 1 )");
                    SetGlobalFlag(21, true);
                    // FIXME: map_flags;
                    ;
                    break;
                case 112:
                case 113:
                case 312:
                case 313:
                    Trace.Assert(originalScript == "game.global_flags[28] = 1");
                    SetGlobalFlag(28, true);
                    break;
                case 122:
                case 124:
                    Trace.Assert(originalScript == "pc.money_adj(-10000); game.global_vars[6] = game.global_vars[6] + 100");
                    pc.AdjustMoney(-10000);
                    SetGlobalVar(6, GetGlobalVar(6) + 100);
                    ;
                    break;
                case 160:
                    Trace.Assert(originalScript == "game.quests[14].state = qs_mentioned");
                    SetQuestState(14, QuestState.Mentioned);
                    break;
                case 161:
                case 162:
                    Trace.Assert(originalScript == "game.quests[14].state = qs_accepted");
                    SetQuestState(14, QuestState.Accepted);
                    break;
                case 243:
                case 244:
                    Trace.Assert(originalScript == "pc.money_adj(-200)");
                    pc.AdjustMoney(-200);
                    break;
                case 260:
                case 410:
                    Trace.Assert(originalScript == "game.quests[14].state = qs_completed");
                    SetQuestState(14, QuestState.Completed);
                    break;
                case 270:
                    Trace.Assert(originalScript == "game.quests[14].state = qs_completed; beggar_cavanaugh( npc, pc )");
                    SetQuestState(14, QuestState.Completed);
                    beggar_cavanaugh(npc, pc);
                    ;
                    break;
                case 295:
                    Trace.Assert(originalScript == "npc.spells_pending_to_memorized()");
                    npc.PendingSpellsToMemorized();
                    break;
                case 340:
                    Trace.Assert(originalScript == "game.picker( npc, spell_cure_light_wounds, should_heal_hp_on, [ 290, 320, 350 ] )");
                    // FIXME: picker;
                    break;
                case 351:
                case 353:
                    Trace.Assert(originalScript == "pc.money_adj(-8000); npc.cast_spell( spell_cure_light_wounds, picker_obj )");
                    pc.AdjustMoney(-8000);
                    npc.CastSpell(WellKnownSpells.CureLightWounds, PickedObject);
                    ;
                    break;
                case 360:
                    Trace.Assert(originalScript == "game.picker( npc, spell_remove_disease, should_heal_disease_on, [ 290, 320, 370 ] )");
                    // FIXME: picker;
                    break;
                case 371:
                case 373:
                    Trace.Assert(originalScript == "pc.money_adj(-24000); npc.cast_spell( spell_remove_disease, picker_obj )");
                    pc.AdjustMoney(-24000);
                    npc.CastSpell(WellKnownSpells.RemoveDisease, PickedObject);
                    ;
                    break;
                case 385:
                case 386:
                    Trace.Assert(originalScript == "pc.money_adj(-1000)");
                    pc.AdjustMoney(-1000);
                    break;
                case 400:
                    Trace.Assert(originalScript == "game.global_flags[30] = 1; game.quests[14].state = qs_botched");
                    SetGlobalFlag(30, true);
                    SetQuestState(14, QuestState.Botched);
                    ;
                    break;
                case 401:
                    Trace.Assert(originalScript == "beggar_cavanaugh( npc, pc )");
                    beggar_cavanaugh(npc, pc);
                    break;
                case 2000:
                    Trace.Assert(originalScript == "create_terjon(npc, pc)");
                    create_terjon(npc, pc);
                    break;
                case 2001:
                    Trace.Assert(originalScript == "switch_to_terjon(npc, pc)");
                    switch_to_terjon(npc, pc);
                    break;
                case 2021:
                    Trace.Assert(originalScript == "look_spugnoir(npc, pc)");
                    look_spugnoir(npc, pc);
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
                case 43:
                case 44:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 7);
                    return true;
                case 53:
                case 54:
                case 451:
                case 452:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 6);
                    return true;
                case 61:
                case 62:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 5);
                    return true;
                case 243:
                case 244:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 4);
                    return true;
                case 251:
                case 252:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 4);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
