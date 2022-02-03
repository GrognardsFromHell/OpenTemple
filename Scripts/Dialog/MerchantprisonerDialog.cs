
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

namespace Scripts.Dialog
{
    [DialogScript(235)]
    public class MerchantprisonerDialog : Merchantprisoner, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 6:
                case 7:
                    originalScript = "pc.stat_level_get(stat_level_paladin) == 0";
                    return pc.GetStat(Stat.level_paladin) == 0;
                case 11:
                case 25:
                case 26:
                case 31:
                    originalScript = "game.global_flags[352] == 0";
                    return !GetGlobalFlag(352);
                case 12:
                case 13:
                    originalScript = "pc.stat_level_get( stat_gender ) == gender_male and game.global_flags[352] == 0";
                    return pc.GetGender() == Gender.Male && !GetGlobalFlag(352);
                case 15:
                case 23:
                case 24:
                case 32:
                    originalScript = "game.global_flags[352] == 1";
                    return GetGlobalFlag(352);
                case 16:
                case 17:
                    originalScript = "pc.stat_level_get( stat_gender ) == gender_male and game.global_flags[352] == 1";
                    return pc.GetGender() == Gender.Male && GetGlobalFlag(352);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 6:
                case 7:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
                    break;
                case 11:
                case 12:
                case 13:
                case 25:
                case 26:
                case 31:
                case 43:
                case 44:
                    originalScript = "run_off(npc, pc)";
                    run_off(npc, pc);
                    break;
                case 41:
                case 42:
                    originalScript = "pc.money_adj(+103000); run_off(npc, pc)";
                    pc.AdjustMoney(+103000);
                    run_off(npc, pc);
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
