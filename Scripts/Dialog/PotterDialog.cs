
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
    [DialogScript(88)]
    public class PotterDialog : Potter, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    Trace.Assert(originalScript == "not npc.has_met(pc)");
                    return !npc.HasMet(pc);
                case 4:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.global_flags[63] == 0");
                    return npc.HasMet(pc) && !GetGlobalFlag(63);
                case 5:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.global_flags[63] == 1");
                    return npc.HasMet(pc) && GetGlobalFlag(63);
                case 12:
                case 15:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL");
                    return PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 101:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 2");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 2;
                case 104:
                    Trace.Assert(originalScript == "pc.money_get() >= 10000");
                    return pc.GetMoney() >= 10000;
                case 121:
                case 124:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_race) == race_gnome");
                    return pc.GetRace() == RaceId.svirfneblin;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 50:
                    Trace.Assert(originalScript == "make_hate( npc, pc ); game.global_flags[64] = 1");
                    make_hate(npc, pc);
                    SetGlobalFlag(64, true);
                    ;
                    break;
                case 104:
                    Trace.Assert(originalScript == "pc.money_adj(-10000); game.global_flags[63] = 0");
                    pc.AdjustMoney(-10000);
                    SetGlobalFlag(63, false);
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
                case 101:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 2);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
