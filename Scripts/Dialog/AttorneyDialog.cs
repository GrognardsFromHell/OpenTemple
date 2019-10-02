
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
    [DialogScript(359)]
    public class AttorneyDialog : Attorney, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                    Trace.Assert(originalScript == "game.global_vars[993] != 5");
                    return GetGlobalVar(993) != 5;
                case 3:
                    Trace.Assert(originalScript == "game.global_vars[993] == 5 and game.global_flags[870] == 0");
                    return GetGlobalVar(993) == 5 && !GetGlobalFlag(870);
                case 11:
                    Trace.Assert(originalScript == "game.global_vars[993] == 8");
                    return GetGlobalVar(993) == 8;
                case 21:
                    Trace.Assert(originalScript == "game.party[0].reputation_has(44) == 1");
                    return PartyLeader.HasReputation(44);
                case 22:
                    Trace.Assert(originalScript == "game.party[0].reputation_has(45) == 1 and game.party[0].reputation_has(44) == 0");
                    return PartyLeader.HasReputation(45) && !PartyLeader.HasReputation(44);
                case 23:
                    Trace.Assert(originalScript == "game.party[0].reputation_has(46) == 1 and game.party[0].reputation_has(45) == 0 and game.party[0].reputation_has(44) == 0");
                    return PartyLeader.HasReputation(46) && !PartyLeader.HasReputation(45) && !PartyLeader.HasReputation(44);
                case 24:
                    Trace.Assert(originalScript == "game.party[0].reputation_has(42) == 1 and game.party[0].reputation_has(44) == 0 and game.party[0].reputation_has(45) == 0 and game.party[0].reputation_has(46) == 0");
                    return PartyLeader.HasReputation(42) && !PartyLeader.HasReputation(44) && !PartyLeader.HasReputation(45) && !PartyLeader.HasReputation(46);
                case 25:
                    Trace.Assert(originalScript == "game.party[0].reputation_has(43) == 1 and game.party[0].reputation_has(44) == 0 and game.party[0].reputation_has(45) == 0 and game.party[0].reputation_has(42) == 0 and game.party[0].reputation_has(46) == 0");
                    return PartyLeader.HasReputation(43) && !PartyLeader.HasReputation(44) && !PartyLeader.HasReputation(45) && !PartyLeader.HasReputation(42) && !PartyLeader.HasReputation(46);
                case 26:
                    Trace.Assert(originalScript == "game.global_vars[993] == 8 and game.party[0].reputation_has(44) == 0 and game.party[0].reputation_has(45) == 0 and game.party[0].reputation_has(42) == 0 and game.party[0].reputation_has(43) == 0 and game.party[0].reputation_has(46) == 0");
                    return GetGlobalVar(993) == 8 && !PartyLeader.HasReputation(44) && !PartyLeader.HasReputation(45) && !PartyLeader.HasReputation(42) && !PartyLeader.HasReputation(43) && !PartyLeader.HasReputation(46);
                case 27:
                    Trace.Assert(originalScript == "game.party[0].reputation_has(35) == 1 and game.party[0].reputation_has(44) == 0 and game.party[0].reputation_has(45) == 0 and game.party[0].reputation_has(46) == 0 and game.party[0].reputation_has(42) == 0 and game.party[0].reputation_has(43) == 0 and game.global_vars[993] != 8");
                    return PartyLeader.HasReputation(35) && !PartyLeader.HasReputation(44) && !PartyLeader.HasReputation(45) && !PartyLeader.HasReputation(46) && !PartyLeader.HasReputation(42) && !PartyLeader.HasReputation(43) && GetGlobalVar(993) != 8;
                case 28:
                    Trace.Assert(originalScript == "game.party[0].reputation_has(34) == 1 and game.party[0].reputation_has(44) == 0 and game.party[0].reputation_has(45) == 0 and game.party[0].reputation_has(46) == 0 and game.party[0].reputation_has(42) == 0 and game.party[0].reputation_has(43) == 0 and game.global_vars[993] != 8 and game.party[0].reputation_has(35) == 0");
                    return PartyLeader.HasReputation(34) && !PartyLeader.HasReputation(44) && !PartyLeader.HasReputation(45) && !PartyLeader.HasReputation(46) && !PartyLeader.HasReputation(42) && !PartyLeader.HasReputation(43) && GetGlobalVar(993) != 8 && !PartyLeader.HasReputation(35);
                case 41:
                case 72:
                case 241:
                    Trace.Assert(originalScript == "game.global_vars[334] == 1");
                    return GetGlobalVar(334) == 1;
                case 42:
                case 73:
                case 242:
                    Trace.Assert(originalScript == "game.global_vars[334] == 2");
                    return GetGlobalVar(334) == 2;
                case 43:
                case 74:
                case 243:
                    Trace.Assert(originalScript == "game.global_vars[334] == 3");
                    return GetGlobalVar(334) == 3;
                case 44:
                case 75:
                case 244:
                    Trace.Assert(originalScript == "game.global_vars[334] == 4");
                    return GetGlobalVar(334) == 4;
                case 45:
                case 76:
                case 245:
                    Trace.Assert(originalScript == "game.global_vars[334] >= 5");
                    return GetGlobalVar(334) >= 5;
                case 46:
                case 77:
                case 246:
                    Trace.Assert(originalScript == "game.global_vars[333] == 1 and game.global_vars[334] == 0");
                    return GetGlobalVar(333) == 1 && GetGlobalVar(334) == 0;
                case 47:
                case 78:
                case 247:
                    Trace.Assert(originalScript == "game.global_vars[333] == 2 and game.global_vars[334] == 0");
                    return GetGlobalVar(333) == 2 && GetGlobalVar(334) == 0;
                case 48:
                case 79:
                case 248:
                    Trace.Assert(originalScript == "game.global_vars[333] == 3 and game.global_vars[334] == 0");
                    return GetGlobalVar(333) == 3 && GetGlobalVar(334) == 0;
                case 49:
                case 80:
                case 249:
                    Trace.Assert(originalScript == "game.global_vars[333] == 4 and game.global_vars[334] == 0");
                    return GetGlobalVar(333) == 4 && GetGlobalVar(334) == 0;
                case 50:
                case 81:
                case 250:
                    Trace.Assert(originalScript == "game.global_vars[333] >= 5 and game.global_vars[334] == 0");
                    return GetGlobalVar(333) >= 5 && GetGlobalVar(334) == 0;
                case 91:
                case 171:
                    Trace.Assert(originalScript == "pc.money_get() >= 2000000");
                    return pc.GetMoney() >= 2000000;
                case 92:
                case 172:
                    Trace.Assert(originalScript == "pc.money_get() <= 1999900");
                    return pc.GetMoney() <= 1999900;
                case 101:
                case 181:
                    Trace.Assert(originalScript == "pc.money_get() >= 4000000");
                    return pc.GetMoney() >= 4000000;
                case 102:
                case 182:
                    Trace.Assert(originalScript == "pc.money_get() <= 3999900");
                    return pc.GetMoney() <= 3999900;
                case 111:
                case 191:
                    Trace.Assert(originalScript == "pc.money_get() >= 6000000");
                    return pc.GetMoney() >= 6000000;
                case 112:
                case 192:
                    Trace.Assert(originalScript == "pc.money_get() <= 5999900");
                    return pc.GetMoney() <= 5999900;
                case 121:
                case 201:
                    Trace.Assert(originalScript == "pc.money_get() >= 8000000");
                    return pc.GetMoney() >= 8000000;
                case 122:
                case 202:
                    Trace.Assert(originalScript == "pc.money_get() <= 7999900");
                    return pc.GetMoney() <= 7999900;
                case 131:
                case 211:
                    Trace.Assert(originalScript == "pc.money_get() >= 10000000");
                    return pc.GetMoney() >= 10000000;
                case 132:
                case 212:
                    Trace.Assert(originalScript == "pc.money_get() <= 9999900");
                    return pc.GetMoney() <= 9999900;
                case 281:
                case 311:
                    Trace.Assert(originalScript == "pc.money_get() >= 14000000");
                    return pc.GetMoney() >= 14000000;
                case 282:
                case 312:
                    Trace.Assert(originalScript == "pc.money_get() <= 13999900");
                    return pc.GetMoney() <= 13999900;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 3:
                    Trace.Assert(originalScript == "game.global_vars[993] = 8");
                    SetGlobalVar(993, 8);
                    break;
                case 91:
                case 171:
                    Trace.Assert(originalScript == "pc.money_adj(-2000000); clear_reps(npc,pc)");
                    pc.AdjustMoney(-2000000);
                    clear_reps(npc, pc);
                    ;
                    break;
                case 101:
                case 181:
                    Trace.Assert(originalScript == "pc.money_adj(-4000000); clear_reps(npc,pc)");
                    pc.AdjustMoney(-4000000);
                    clear_reps(npc, pc);
                    ;
                    break;
                case 111:
                case 191:
                    Trace.Assert(originalScript == "pc.money_adj(-6000000); clear_reps(npc,pc)");
                    pc.AdjustMoney(-6000000);
                    clear_reps(npc, pc);
                    ;
                    break;
                case 121:
                case 201:
                    Trace.Assert(originalScript == "pc.money_adj(-8000000); clear_reps(npc,pc)");
                    pc.AdjustMoney(-8000000);
                    clear_reps(npc, pc);
                    ;
                    break;
                case 131:
                case 211:
                    Trace.Assert(originalScript == "pc.money_adj(-10000000); clear_reps(npc,pc)");
                    pc.AdjustMoney(-10000000);
                    clear_reps(npc, pc);
                    ;
                    break;
                case 141:
                case 161:
                case 221:
                case 231:
                case 291:
                case 301:
                case 321:
                    Trace.Assert(originalScript == "game.global_flags[260] = 1; game.fade_and_teleport(0,0,0,5121,252,506)");
                    SetGlobalFlag(260, true);
                    FadeAndTeleport(0, 0, 0, 5121, 252, 506);
                    ;
                    break;
                case 151:
                    Trace.Assert(originalScript == "pc.barter(npc); game.global_vars[969] = 4");
                    throw new NotSupportedException("Conversion failed.");
                case 160:
                case 220:
                case 230:
                case 300:
                case 320:
                    Trace.Assert(originalScript == "game.global_flags[955] = 1");
                    SetGlobalFlag(955, true);
                    break;
                case 281:
                case 311:
                    Trace.Assert(originalScript == "pc.money_adj(-14000000); clear_reps(npc,pc)");
                    pc.AdjustMoney(-14000000);
                    clear_reps(npc, pc);
                    ;
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
