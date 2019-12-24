
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
    [DialogScript(88)]
    public class PotterDialog : Potter, IDialogScript
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
                    originalScript = "npc.has_met(pc) and game.global_flags[63] == 0";
                    return npc.HasMet(pc) && !GetGlobalFlag(63);
                case 5:
                    originalScript = "npc.has_met(pc) and game.global_flags[63] == 1";
                    return npc.HasMet(pc) && GetGlobalFlag(63);
                case 12:
                case 15:
                    originalScript = "game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL";
                    return PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 101:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) >= 2";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 2;
                case 104:
                    originalScript = "pc.money_get() >= 10000";
                    return pc.GetMoney() >= 10000;
                case 121:
                case 124:
                    originalScript = "pc.stat_level_get(stat_race) == race_gnome";
                    return pc.GetRace() == RaceId.svirfneblin;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 50:
                    originalScript = "make_hate( npc, pc ); game.global_flags[64] = 1";
                    make_hate(npc, pc);
                    SetGlobalFlag(64, true);
                    ;
                    break;
                case 104:
                    originalScript = "pc.money_adj(-10000); game.global_flags[63] = 0";
                    pc.AdjustMoney(-10000);
                    SetGlobalFlag(63, false);
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
