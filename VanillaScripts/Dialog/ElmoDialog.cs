
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

namespace VanillaScripts.Dialog
{
    [DialogScript(91)]
    public class ElmoDialog : Elmo, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 21:
                case 24:
                    originalScript = "pc.money_get() >= 100";
                    return pc.GetMoney() >= 100;
                case 61:
                case 62:
                    originalScript = "pc.money_get() >= 20000 and not pc.follower_atmax()";
                    return pc.GetMoney() >= 20000 && !pc.HasMaxFollowers();
                case 63:
                case 64:
                    originalScript = "pc.money_get() < 20000";
                    return pc.GetMoney() < 20000;
                case 65:
                case 66:
                    originalScript = "pc.follower_atmax()";
                    return pc.HasMaxFollowers();
                case 91:
                case 92:
                    originalScript = "game.global_flags[66] == 0";
                    return !GetGlobalFlag(66);
                case 93:
                case 94:
                    originalScript = "game.global_flags[66] == 1 and not pc.follower_atmax()";
                    return GetGlobalFlag(66) && !pc.HasMaxFollowers();
                case 95:
                case 96:
                    originalScript = "game.global_flags[66] == 1 and pc.follower_atmax()";
                    return GetGlobalFlag(66) && pc.HasMaxFollowers();
                case 102:
                case 105:
                    originalScript = "npc.area == 1";
                    return npc.GetArea() == 1;
                case 103:
                case 106:
                    originalScript = "npc.area == 3";
                    return npc.GetArea() == 3;
                case 107:
                case 108:
                    originalScript = "not npc.has_met( pc ) and game.global_flags[67] == 0";
                    return !npc.HasMet(pc) && !GetGlobalFlag(67);
                case 121:
                case 122:
                    originalScript = "game.global_flags[70] == 0 and game.global_flags[71] == 0";
                    return !GetGlobalFlag(70) && !GetGlobalFlag(71);
                case 123:
                case 124:
                    originalScript = "game.global_flags[70] == 1 and game.global_flags[71] == 0 and game.global_flags[72] == 0";
                    return GetGlobalFlag(70) && !GetGlobalFlag(71) && !GetGlobalFlag(72);
                case 131:
                case 132:
                    originalScript = "game.global_flags[71] == 0";
                    return !GetGlobalFlag(71);
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
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 21:
                case 24:
                    originalScript = "pc.money_adj(-100)";
                    pc.AdjustMoney(-100);
                    break;
                case 61:
                case 62:
                    originalScript = "elmo_joins_first_time(npc,pc)";
                    elmo_joins_first_time(npc, pc);
                    break;
                case 70:
                    originalScript = "pc.follower_add(npc); game.global_flags[66] = 1";
                    pc.AddFollower(npc);
                    SetGlobalFlag(66, true);
                    ;
                    break;
                case 123:
                case 124:
                    originalScript = "game.global_flags[71] = 1";
                    SetGlobalFlag(71, true);
                    break;
                case 210:
                    originalScript = "pc.follower_remove(npc)";
                    pc.RemoveFollower(npc);
                    break;
                case 331:
                    originalScript = "make_otis_talk(npc,pc,420)";
                    make_otis_talk(npc, pc, 420);
                    break;
                case 341:
                case 342:
                    originalScript = "make_otis_talk(npc,pc,430)";
                    make_otis_talk(npc, pc, 430);
                    break;
                case 351:
                    originalScript = "make_otis_talk(npc,pc,440)";
                    make_otis_talk(npc, pc, 440);
                    break;
                case 561:
                case 562:
                    originalScript = "switch_to_thrommel(npc,pc)";
                    switch_to_thrommel(npc, pc);
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
