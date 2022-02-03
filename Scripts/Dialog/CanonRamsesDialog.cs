
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

namespace Scripts.Dialog;

[DialogScript(379)]
public class CanonRamsesDialog : CanonRamses, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 2:
            case 11:
                originalScript = "pc.d20_query(Q_IsFallenPaladin) == 1";
                return pc.D20Query(D20DispatcherKey.QUE_IsFallenPaladin);
            case 21:
                originalScript = "game.global_flags[938] == 1 and game.global_flags[973] == 0 and not npc_get(npc,1)";
                return GetGlobalFlag(938) && !GetGlobalFlag(973) && !ScriptDaemon.npc_get(npc, 1);
            case 22:
                originalScript = "game.global_flags[973] == 1 and not npc_get(npc,2)";
                return GetGlobalFlag(973) && !ScriptDaemon.npc_get(npc, 2);
            case 23:
                originalScript = "game.quests[85].state == qs_mentioned";
                return GetQuestState(85) == QuestState.Mentioned;
            case 24:
                originalScript = "game.global_vars[949] == 2";
                return GetGlobalVar(949) == 2;
            case 25:
                originalScript = "(game.quests[84].state == qs_mentioned or game.quests[84].state == qs_accepted) and game.global_flags[973] == 0 and not npc_get(npc,7)";
                return (GetQuestState(84) == QuestState.Mentioned || GetQuestState(84) == QuestState.Accepted) && !GetGlobalFlag(973) && !ScriptDaemon.npc_get(npc, 7);
            case 26:
                originalScript = "(game.quests[83].state == qs_mentioned or game.quests[83].state == qs_botched) and not npc_get(npc,3)";
                return (GetQuestState(83) == QuestState.Mentioned || GetQuestState(83) == QuestState.Botched) && !ScriptDaemon.npc_get(npc, 3);
            case 27:
                originalScript = "(game.quests[77].state == qs_accepted or game.quests[77].state == qs_mentioned) or (game.quests[66].state == qs_accepted or game.quests[66].state == qs_mentioned) or (game.quests[67].state == qs_accepted or game.quests[67].state == qs_mentioned)";
                return (GetQuestState(77) == QuestState.Accepted || GetQuestState(77) == QuestState.Mentioned) || (GetQuestState(66) == QuestState.Accepted || GetQuestState(66) == QuestState.Mentioned) || (GetQuestState(67) == QuestState.Accepted || GetQuestState(67) == QuestState.Mentioned);
            case 28:
                originalScript = "game.quests[78].state == qs_accepted";
                return GetQuestState(78) == QuestState.Accepted;
            case 51:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 1";
                return pc.GetStat(Stat.level_paladin) == 1;
            case 52:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 2";
                return pc.GetStat(Stat.level_paladin) == 2;
            case 53:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 3";
                return pc.GetStat(Stat.level_paladin) == 3;
            case 54:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 4";
                return pc.GetStat(Stat.level_paladin) == 4;
            case 55:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 5";
                return pc.GetStat(Stat.level_paladin) == 5;
            case 56:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 6";
                return pc.GetStat(Stat.level_paladin) == 6;
            case 57:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 7";
                return pc.GetStat(Stat.level_paladin) == 7;
            case 58:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 8";
                return pc.GetStat(Stat.level_paladin) == 8;
            case 59:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 9";
                return pc.GetStat(Stat.level_paladin) == 9;
            case 60:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 10";
                return pc.GetStat(Stat.level_paladin) == 10;
            case 61:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 11";
                return pc.GetStat(Stat.level_paladin) == 11;
            case 62:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 12";
                return pc.GetStat(Stat.level_paladin) == 12;
            case 63:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 13";
                return pc.GetStat(Stat.level_paladin) == 13;
            case 64:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 14";
                return pc.GetStat(Stat.level_paladin) == 14;
            case 65:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 15";
                return pc.GetStat(Stat.level_paladin) == 15;
            case 66:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 16";
                return pc.GetStat(Stat.level_paladin) == 16;
            case 67:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 17";
                return pc.GetStat(Stat.level_paladin) == 17;
            case 68:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 18";
                return pc.GetStat(Stat.level_paladin) == 18;
            case 69:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 19";
                return pc.GetStat(Stat.level_paladin) == 19;
            case 70:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 20";
                return pc.GetStat(Stat.level_paladin) == 20;
            case 81:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 1 and pc.money_get() >= 100000";
                return pc.GetStat(Stat.level_paladin) == 1 && pc.GetMoney() >= 100000;
            case 82:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 2 and pc.money_get() >= 200000";
                return pc.GetStat(Stat.level_paladin) == 2 && pc.GetMoney() >= 200000;
            case 83:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 3 and pc.money_get() >= 300000";
                return pc.GetStat(Stat.level_paladin) == 3 && pc.GetMoney() >= 300000;
            case 84:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 4 and pc.money_get() >= 400000";
                return pc.GetStat(Stat.level_paladin) == 4 && pc.GetMoney() >= 400000;
            case 85:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 5 and pc.money_get() >= 500000";
                return pc.GetStat(Stat.level_paladin) == 5 && pc.GetMoney() >= 500000;
            case 86:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 6 and pc.money_get() >= 600000";
                return pc.GetStat(Stat.level_paladin) == 6 && pc.GetMoney() >= 600000;
            case 87:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 7 and pc.money_get() >= 700000";
                return pc.GetStat(Stat.level_paladin) == 7 && pc.GetMoney() >= 700000;
            case 88:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 8 and pc.money_get() >= 800000";
                return pc.GetStat(Stat.level_paladin) == 8 && pc.GetMoney() >= 800000;
            case 89:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 9 and pc.money_get() >= 900000";
                return pc.GetStat(Stat.level_paladin) == 9 && pc.GetMoney() >= 900000;
            case 90:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 10 and pc.money_get() >= 1000000";
                return pc.GetStat(Stat.level_paladin) == 10 && pc.GetMoney() >= 1000000;
            case 91:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 11 and pc.money_get() >= 1100000";
                return pc.GetStat(Stat.level_paladin) == 11 && pc.GetMoney() >= 1100000;
            case 92:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 12 and pc.money_get() >= 1200000";
                return pc.GetStat(Stat.level_paladin) == 12 && pc.GetMoney() >= 1200000;
            case 93:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 13 and pc.money_get() >= 1300000";
                return pc.GetStat(Stat.level_paladin) == 13 && pc.GetMoney() >= 1300000;
            case 94:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 14 and pc.money_get() >= 1400000";
                return pc.GetStat(Stat.level_paladin) == 14 && pc.GetMoney() >= 1400000;
            case 95:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 15 and pc.money_get() >= 1500000";
                return pc.GetStat(Stat.level_paladin) == 15 && pc.GetMoney() >= 1500000;
            case 96:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 16 and pc.money_get() >= 1600000";
                return pc.GetStat(Stat.level_paladin) == 16 && pc.GetMoney() >= 1600000;
            case 97:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 17 and pc.money_get() >= 1700000";
                return pc.GetStat(Stat.level_paladin) == 17 && pc.GetMoney() >= 1700000;
            case 98:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 18 and pc.money_get() >= 1800000";
                return pc.GetStat(Stat.level_paladin) == 18 && pc.GetMoney() >= 1800000;
            case 99:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 19 and pc.money_get() >= 1900000";
                return pc.GetStat(Stat.level_paladin) == 19 && pc.GetMoney() >= 1900000;
            case 100:
                originalScript = "pc.stat_level_get(stat_level_paladin) == 20 and pc.money_get() >= 2000000";
                return pc.GetStat(Stat.level_paladin) == 20 && pc.GetMoney() >= 2000000;
            case 121:
                originalScript = "game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD";
                return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD;
            case 122:
                originalScript = "game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == LAWFUL_EVIL";
                return PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL;
            case 123:
                originalScript = "game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == CHAOTIC_NEUTRAL";
                return PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
            case 131:
                originalScript = "pc.money_get() >= 500000";
                return pc.GetMoney() >= 500000;
            case 151:
                originalScript = "not npc_get(npc,8)";
                return !ScriptDaemon.npc_get(npc, 8);
            case 231:
                originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == LAWFUL_EVIL) and game.quests[84].state == qs_mentioned";
                return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL) && GetQuestState(84) == QuestState.Mentioned;
            case 232:
                originalScript = "(game.party_alignment == CHAOTIC_GOOD or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == LAWFUL_EVIL or game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL) and game.quests[84].state == qs_mentioned";
                return (PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL) && GetQuestState(84) == QuestState.Mentioned;
            case 233:
                originalScript = "(game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL) and game.quests[84].state == qs_mentioned";
                return (PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL) && GetQuestState(84) == QuestState.Mentioned;
            case 234:
                originalScript = "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == LAWFUL_EVIL) and game.quests[84].state == qs_accepted";
                return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL) && GetQuestState(84) == QuestState.Accepted;
            case 235:
                originalScript = "(game.party_alignment == CHAOTIC_GOOD or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == LAWFUL_EVIL or game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL) and game.quests[84].state == qs_accepted";
                return (PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL) && GetQuestState(84) == QuestState.Accepted;
            case 236:
                originalScript = "(game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL) and game.quests[84].state == qs_accepted";
                return (PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL) && GetQuestState(84) == QuestState.Accepted;
            case 241:
                originalScript = "game.quests[83].state == qs_mentioned";
                return GetQuestState(83) == QuestState.Mentioned;
            case 242:
                originalScript = "game.quests[83].state == qs_botched";
                return GetQuestState(83) == QuestState.Botched;
            case 261:
                originalScript = "(game.quests[77].state == qs_accepted or game.quests[77].state == qs_mentioned) and game.global_flags[989] == 0 and game.global_flags[946] == 0 and game.global_flags[863] == 0 and not npc_get(npc,4)";
                return (GetQuestState(77) == QuestState.Accepted || GetQuestState(77) == QuestState.Mentioned) && !GetGlobalFlag(989) && !GetGlobalFlag(946) && !GetGlobalFlag(863) && !ScriptDaemon.npc_get(npc, 4);
            case 262:
                originalScript = "(game.quests[67].state == qs_accepted or game.quests[67].state == qs_mentioned) and game.global_flags[989] == 0 and not npc_get(npc,5)";
                return (GetQuestState(67) == QuestState.Accepted || GetQuestState(67) == QuestState.Mentioned) && !GetGlobalFlag(989) && !ScriptDaemon.npc_get(npc, 5);
            case 263:
                originalScript = "(game.quests[66].state == qs_accepted or game.quests[66].state == qs_mentioned) and game.global_flags[989] == 0 and not npc_get(npc,6)";
                return (GetQuestState(66) == QuestState.Accepted || GetQuestState(66) == QuestState.Mentioned) && !GetGlobalFlag(989) && !ScriptDaemon.npc_get(npc, 6);
            case 301:
                originalScript = "not npc_get(npc,9)";
                return !ScriptDaemon.npc_get(npc, 9);
            case 302:
                originalScript = "not npc_get(npc,10)";
                return !ScriptDaemon.npc_get(npc, 10);
            case 303:
                originalScript = "not npc_get(npc,11)";
                return !ScriptDaemon.npc_get(npc, 11);
            case 304:
                originalScript = "not npc_get(npc,12)";
                return !ScriptDaemon.npc_get(npc, 12);
            case 312:
                originalScript = "not npc_get(npc,13)";
                return !ScriptDaemon.npc_get(npc, 13);
            case 313:
                originalScript = "not npc_get(npc,14)";
                return !ScriptDaemon.npc_get(npc, 14);
            case 314:
                originalScript = "not npc_get(npc,15)";
                return !ScriptDaemon.npc_get(npc, 15);
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 21:
                originalScript = "npc_set(npc,1)";
                ScriptDaemon.npc_set(npc, 1);
                break;
            case 22:
                originalScript = "npc_set(npc,2)";
                ScriptDaemon.npc_set(npc, 2);
                break;
            case 24:
                originalScript = "game.global_vars[949] = 3";
                SetGlobalVar(949, 3);
                break;
            case 25:
                originalScript = "npc_set(npc,7)";
                ScriptDaemon.npc_set(npc, 7);
                break;
            case 26:
                originalScript = "npc_set(npc,3)";
                ScriptDaemon.npc_set(npc, 3);
                break;
            case 30:
                originalScript = "npc.spells_pending_to_memorized()";
                npc.PendingSpellsToMemorized();
                break;
            case 40:
                originalScript = "game.picker( npc, spell_raise_dead, should_resurrect_on, [ 140, 10, 130 ] )";
                // FIXME: picker;
                break;
            case 81:
                originalScript = "pc.money_adj(-100000)";
                pc.AdjustMoney(-100000);
                break;
            case 82:
                originalScript = "pc.money_adj(-200000)";
                pc.AdjustMoney(-200000);
                break;
            case 83:
                originalScript = "pc.money_adj(-300000)";
                pc.AdjustMoney(-300000);
                break;
            case 84:
                originalScript = "pc.money_adj(-400000)";
                pc.AdjustMoney(-400000);
                break;
            case 85:
                originalScript = "pc.money_adj(-500000)";
                pc.AdjustMoney(-500000);
                break;
            case 86:
                originalScript = "pc.money_adj(-600000)";
                pc.AdjustMoney(-600000);
                break;
            case 87:
                originalScript = "pc.money_adj(-700000)";
                pc.AdjustMoney(-700000);
                break;
            case 88:
                originalScript = "pc.money_adj(-800000)";
                pc.AdjustMoney(-800000);
                break;
            case 89:
                originalScript = "pc.money_adj(-900000)";
                pc.AdjustMoney(-900000);
                break;
            case 90:
                originalScript = "pc.money_adj(-1000000)";
                pc.AdjustMoney(-1000000);
                break;
            case 91:
                originalScript = "pc.money_adj(-1100000)";
                pc.AdjustMoney(-1100000);
                break;
            case 92:
                originalScript = "pc.money_adj(-1200000)";
                pc.AdjustMoney(-1200000);
                break;
            case 93:
                originalScript = "pc.money_adj(-1300000)";
                pc.AdjustMoney(-1300000);
                break;
            case 94:
                originalScript = "pc.money_adj(-1400000)";
                pc.AdjustMoney(-1400000);
                break;
            case 95:
                originalScript = "pc.money_adj(-1500000)";
                pc.AdjustMoney(-1500000);
                break;
            case 96:
                originalScript = "pc.money_adj(-1600000)";
                pc.AdjustMoney(-1600000);
                break;
            case 97:
                originalScript = "pc.money_adj(-1700000)";
                pc.AdjustMoney(-1700000);
                break;
            case 98:
                originalScript = "pc.money_adj(-1800000)";
                pc.AdjustMoney(-1800000);
                break;
            case 99:
                originalScript = "pc.money_adj(-1900000)";
                pc.AdjustMoney(-1900000);
                break;
            case 100:
                originalScript = "pc.money_adj(-2000000)";
                pc.AdjustMoney(-2000000);
                break;
            case 110:
                originalScript = "game.particles( 'cast-Transmutation-cast', npc ); pc.has_atoned()";
                AttachParticles("cast-Transmutation-cast", npc);
                pc.AtoneFallenPaladin();
                ;
                break;
            case 131:
                originalScript = "pc.money_adj(-500000); npc.cast_spell( spell_raise_dead, picker_obj )";
                pc.AdjustMoney(-500000);
                npc.CastSpell(WellKnownSpells.RaiseDead, PickedObject);
                ;
                break;
            case 160:
                originalScript = "npc_set(npc,8)";
                ScriptDaemon.npc_set(npc, 8);
                break;
            case 180:
                originalScript = "game.quests[85].state = qs_mentioned";
                SetQuestState(85, QuestState.Mentioned);
                break;
            case 210:
                originalScript = "game.quests[85].state = qs_accepted";
                SetQuestState(85, QuestState.Accepted);
                break;
            case 211:
                originalScript = "game.global_vars[949] = 1";
                SetGlobalVar(949, 1);
                break;
            case 220:
                originalScript = "game.quests[85].state = qs_completed";
                SetQuestState(85, QuestState.Completed);
                break;
            case 230:
                originalScript = "game.quests[86].state = qs_completed; game.party[0].reputation_add(47)";
                SetQuestState(86, QuestState.Completed);
                PartyLeader.AddReputation(47);
                ;
                break;
            case 231:
            case 232:
            case 233:
                originalScript = "game.global_vars[960] = 6";
                SetGlobalVar(960, 6);
                break;
            case 234:
            case 235:
            case 236:
                originalScript = "game.global_vars[960] = 6; game.quests[84].state = qs_botched";
                SetGlobalVar(960, 6);
                SetQuestState(84, QuestState.Botched);
                ;
                break;
            case 261:
                originalScript = "npc_set(npc,4)";
                ScriptDaemon.npc_set(npc, 4);
                break;
            case 262:
                originalScript = "npc_set(npc,5)";
                ScriptDaemon.npc_set(npc, 5);
                break;
            case 263:
                originalScript = "npc_set(npc,6)";
                ScriptDaemon.npc_set(npc, 6);
                break;
            case 301:
                originalScript = "npc_set(npc,9)";
                ScriptDaemon.npc_set(npc, 9);
                break;
            case 302:
                originalScript = "npc_set(npc,10)";
                ScriptDaemon.npc_set(npc, 10);
                break;
            case 303:
                originalScript = "npc_set(npc,11)";
                ScriptDaemon.npc_set(npc, 11);
                break;
            case 304:
                originalScript = "npc_set(npc,12)";
                ScriptDaemon.npc_set(npc, 12);
                break;
            case 312:
                originalScript = "npc_set(npc,13)";
                ScriptDaemon.npc_set(npc, 13);
                break;
            case 313:
                originalScript = "npc_set(npc,14)";
                ScriptDaemon.npc_set(npc, 14);
                break;
            case 314:
                originalScript = "npc_set(npc,15)";
                ScriptDaemon.npc_set(npc, 15);
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