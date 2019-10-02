
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
    [DialogScript(373)]
    public class JennaDialog : Jenna, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                    Trace.Assert(originalScript == "not npc.has_met(pc) and (game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD)");
                    return !npc.HasMet(pc) && (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD);
                case 3:
                    Trace.Assert(originalScript == "not npc.has_met(pc) and (game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL)");
                    return !npc.HasMet(pc) && (PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL);
                case 4:
                    Trace.Assert(originalScript == "not npc.has_met(pc) and (game.party_alignment == LAWFUL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL)");
                    return !npc.HasMet(pc) && (PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL);
                case 5:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.global_vars[998] == 0");
                    return npc.HasMet(pc) && GetGlobalVar(998) == 0;
                case 6:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.global_vars[998] == 4 and pc.stat_level_get( stat_gender ) == gender_male");
                    return npc.HasMet(pc) && GetGlobalVar(998) == 4 && pc.GetGender() == Gender.Male;
                case 7:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.global_vars[998] == 4 and pc.stat_level_get( stat_gender ) == gender_female");
                    return npc.HasMet(pc) && GetGlobalVar(998) == 4 && pc.GetGender() == Gender.Female;
                case 8:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.global_vars[998] == 5 and pc.stat_level_get( stat_gender ) == gender_male");
                    return npc.HasMet(pc) && GetGlobalVar(998) == 5 && pc.GetGender() == Gender.Male;
                case 9:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.global_vars[998] == 5 and pc.stat_level_get( stat_gender ) == gender_female");
                    return npc.HasMet(pc) && GetGlobalVar(998) == 5 && pc.GetGender() == Gender.Female;
                case 10:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.global_vars[998] == 7 and pc.stat_level_get( stat_gender ) == gender_male");
                    return npc.HasMet(pc) && GetGlobalVar(998) == 7 && pc.GetGender() == Gender.Male;
                case 11:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.global_vars[998] == 7 and pc.stat_level_get( stat_gender ) == gender_female");
                    return npc.HasMet(pc) && GetGlobalVar(998) == 7 && pc.GetGender() == Gender.Female;
                case 12:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.global_vars[998] == 8 and pc.stat_level_get( stat_gender ) == gender_male");
                    return npc.HasMet(pc) && GetGlobalVar(998) == 8 && pc.GetGender() == Gender.Male;
                case 13:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.global_vars[998] == 8 and pc.stat_level_get( stat_gender ) == gender_female");
                    return npc.HasMet(pc) && GetGlobalVar(998) == 8 && pc.GetGender() == Gender.Female;
                case 14:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.global_vars[998] == 10");
                    return npc.HasMet(pc) && GetGlobalVar(998) == 10;
                case 15:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.quests[75].state == qs_unknown and not get_1(npc)");
                    return npc.HasMet(pc) && GetQuestState(75) == QuestState.Unknown && !Scripts.get_1(npc);
                case 16:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.quests[75].state == qs_accepted and (game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL) and not get_2(npc)");
                    return npc.HasMet(pc) && GetQuestState(75) == QuestState.Accepted && (PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL) && !Scripts.get_2(npc);
                case 17:
                    Trace.Assert(originalScript == "game.global_vars[997] == 1 and not get_3(npc)");
                    return GetGlobalVar(997) == 1 && !Scripts.get_3(npc);
                case 18:
                    Trace.Assert(originalScript == "game.global_vars[997] == 3 and not get_3(npc)");
                    return GetGlobalVar(997) == 3 && !Scripts.get_3(npc);
                case 19:
                    Trace.Assert(originalScript == "game.global_vars[997] == 2 and not get_3(npc)");
                    return GetGlobalVar(997) == 2 && !Scripts.get_3(npc);
                case 31:
                    Trace.Assert(originalScript == "game.global_vars[998] == 1 and pc.stat_level_get( stat_gender ) == gender_male");
                    return GetGlobalVar(998) == 1 && pc.GetGender() == Gender.Male;
                case 32:
                    Trace.Assert(originalScript == "game.global_vars[998] == 1 and pc.stat_level_get( stat_gender ) == gender_female");
                    return GetGlobalVar(998) == 1 && pc.GetGender() == Gender.Female;
                case 33:
                    Trace.Assert(originalScript == "game.global_vars[998] == 2 and pc.stat_level_get( stat_gender ) == gender_male");
                    return GetGlobalVar(998) == 2 && pc.GetGender() == Gender.Male;
                case 34:
                    Trace.Assert(originalScript == "game.global_vars[998] == 2 and pc.stat_level_get( stat_gender ) == gender_female");
                    return GetGlobalVar(998) == 2 && pc.GetGender() == Gender.Female;
                case 35:
                    Trace.Assert(originalScript == "game.global_vars[998] == 3");
                    return GetGlobalVar(998) == 3;
                case 36:
                    Trace.Assert(originalScript == "(game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL)");
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL);
                case 37:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == LAWFUL_EVIL");
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL;
                case 38:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL");
                    return PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 121:
                case 122:
                case 331:
                case 351:
                    Trace.Assert(originalScript == "pc.money_get() >= 100000");
                    return pc.GetMoney() >= 100000;
                case 123:
                case 283:
                case 332:
                case 353:
                    Trace.Assert(originalScript == "pc.money_get() <= 99900");
                    return pc.GetMoney() <= 99900;
                case 151:
                case 201:
                case 231:
                    Trace.Assert(originalScript == "pc.money_get() >= 10000");
                    return pc.GetMoney() >= 10000;
                case 153:
                case 203:
                case 233:
                    Trace.Assert(originalScript == "pc.money_get() <= 9900");
                    return pc.GetMoney() <= 9900;
                case 291:
                    Trace.Assert(originalScript == "pc.money_get() >= 50000");
                    return pc.GetMoney() >= 50000;
                case 292:
                    Trace.Assert(originalScript == "pc.money_get() <= 49900");
                    return pc.GetMoney() <= 49900;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 15:
                    Trace.Assert(originalScript == "npc_1(npc)");
                    Scripts.npc_1(npc);
                    break;
                case 16:
                    Trace.Assert(originalScript == "npc_2(npc)");
                    Scripts.npc_2(npc);
                    break;
                case 17:
                case 18:
                case 19:
                    Trace.Assert(originalScript == "npc_3(npc)");
                    Scripts.npc_3(npc);
                    break;
                case 52:
                case 71:
                case 91:
                case 111:
                case 124:
                case 142:
                case 152:
                case 153:
                case 192:
                case 202:
                case 203:
                case 232:
                case 233:
                case 242:
                case 282:
                case 283:
                case 292:
                case 333:
                case 352:
                case 353:
                case 381:
                case 382:
                case 391:
                case 392:
                    Trace.Assert(originalScript == "game.global_vars[998] = 10");
                    SetGlobalVar(998, 10);
                    break;
                case 81:
                    Trace.Assert(originalScript == "game.global_vars[998] = 0");
                    SetGlobalVar(998, 0);
                    break;
                case 101:
                    Trace.Assert(originalScript == "run_off( npc, pc )");
                    run_off(npc, pc);
                    break;
                case 121:
                case 281:
                    Trace.Assert(originalScript == "pc.money_adj(-100000); game.global_vars[998] = 4");
                    pc.AdjustMoney(-100000);
                    SetGlobalVar(998, 4);
                    ;
                    break;
                case 151:
                    Trace.Assert(originalScript == "pc.money_adj(-10000); game.global_vars[998] = 5");
                    pc.AdjustMoney(-10000);
                    SetGlobalVar(998, 5);
                    ;
                    break;
                case 201:
                    Trace.Assert(originalScript == "pc.money_adj(-10000); game.global_vars[998] = 8");
                    pc.AdjustMoney(-10000);
                    SetGlobalVar(998, 8);
                    ;
                    break;
                case 231:
                    Trace.Assert(originalScript == "pc.money_adj(-10000); game.global_vars[998] = 10");
                    pc.AdjustMoney(-10000);
                    SetGlobalVar(998, 10);
                    ;
                    break;
                case 291:
                    Trace.Assert(originalScript == "pc.money_adj(-50000); game.global_vars[998] = 10");
                    pc.AdjustMoney(-50000);
                    SetGlobalVar(998, 10);
                    ;
                    break;
                case 331:
                    Trace.Assert(originalScript == "pc.money_adj(-100000); game.global_vars[998] = 7");
                    pc.AdjustMoney(-100000);
                    SetGlobalVar(998, 7);
                    ;
                    break;
                case 351:
                    Trace.Assert(originalScript == "pc.money_adj(-100000); game.global_vars[998] = 10");
                    pc.AdjustMoney(-100000);
                    SetGlobalVar(998, 10);
                    ;
                    break;
                default:
                    Trace.Assert(originalScript == null);
                    return;
            }
        }
        public bool TryGetSkillCheck(int lineNumber, out DialogSkillChecks skillChecks)
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
