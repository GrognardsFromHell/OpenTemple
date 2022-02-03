
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
    [DialogScript(590)]
    public class AngraMainyuDialog : AngraMainyu, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                    originalScript = "game.party_size() == 1 and pc.stat_level_get(stat_level_paladin) == 0";
                    return GameSystems.Party.PartySize == 1 && pc.GetStat(Stat.level_paladin) == 0;
                case 12:
                    originalScript = "game.party_size() >= 2 and pc.stat_level_get(stat_level_paladin) == 0";
                    return GameSystems.Party.PartySize >= 2 && pc.GetStat(Stat.level_paladin) == 0;
                case 13:
                case 71:
                case 91:
                case 111:
                case 131:
                case 151:
                    originalScript = "game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD";
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 14:
                case 72:
                case 92:
                case 112:
                case 132:
                case 152:
                    originalScript = "game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL";
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 15:
                case 73:
                case 93:
                case 113:
                case 133:
                case 153:
                    originalScript = "game.party_alignment == LAWFUL_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL";
                    return PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 32:
                    originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 20";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 20;
                case 41:
                    originalScript = "pc.money_get() >= 10000000 and pc.skill_level_get(npc,skill_diplomacy) >= 25";
                    return pc.GetMoney() >= 10000000 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 25;
                case 42:
                    originalScript = "pc.money_get() >= 10000000 and pc.skill_level_get(npc,skill_diplomacy) <= 24";
                    return pc.GetMoney() >= 10000000 && pc.GetSkillLevel(npc, SkillId.diplomacy) <= 24;
                case 43:
                    originalScript = "pc.money_get() >= 20000000 and pc.skill_level_get(npc,skill_diplomacy) >= 24";
                    return pc.GetMoney() >= 20000000 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 24;
                case 44:
                    originalScript = "pc.money_get() >= 20000000 and pc.skill_level_get(npc,skill_diplomacy) <= 23";
                    return pc.GetMoney() >= 20000000 && pc.GetSkillLevel(npc, SkillId.diplomacy) <= 23;
                case 45:
                    originalScript = "pc.money_get() >= 30000000 and pc.skill_level_get(npc,skill_diplomacy) >= 23";
                    return pc.GetMoney() >= 30000000 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 23;
                case 46:
                    originalScript = "pc.money_get() >= 30000000 and pc.skill_level_get(npc,skill_diplomacy) <= 22";
                    return pc.GetMoney() >= 30000000 && pc.GetSkillLevel(npc, SkillId.diplomacy) <= 22;
                case 47:
                    originalScript = "pc.money_get() >= 40000000 and pc.skill_level_get(npc,skill_diplomacy) >= 22";
                    return pc.GetMoney() >= 40000000 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 22;
                case 48:
                    originalScript = "pc.money_get() >= 40000000 and pc.skill_level_get(npc,skill_diplomacy) <= 21";
                    return pc.GetMoney() >= 40000000 && pc.GetSkillLevel(npc, SkillId.diplomacy) <= 21;
                case 49:
                    originalScript = "pc.money_get() >= 50000000 and pc.skill_level_get(npc,skill_diplomacy) >= 21";
                    return pc.GetMoney() >= 50000000 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 21;
                case 50:
                    originalScript = "pc.money_get() >= 50000000 and pc.skill_level_get(npc,skill_diplomacy) <= 20";
                    return pc.GetMoney() >= 50000000 && pc.GetSkillLevel(npc, SkillId.diplomacy) <= 20;
                case 51:
                    originalScript = "pc.money_get() <= 9900000 or pc.skill_level_get(npc,skill_diplomacy) <= 19";
                    return pc.GetMoney() <= 9900000 || pc.GetSkillLevel(npc, SkillId.diplomacy) <= 19;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                    originalScript = "game.global_vars[802] = game.global_vars[802] + 1; increment_rep(npc,pc)";
                    SetGlobalVar(802, GetGlobalVar(802) + 1);
                    increment_rep(npc, pc);
                    ;
                    break;
                case 13:
                case 14:
                case 15:
                case 31:
                case 51:
                case 71:
                case 72:
                case 73:
                case 91:
                case 92:
                case 93:
                case 111:
                case 112:
                case 113:
                case 131:
                case 132:
                case 133:
                case 151:
                case 152:
                case 153:
                    originalScript = "npc.attack(pc)";
                    npc.Attack(pc);
                    break;
                case 60:
                case 80:
                case 100:
                case 120:
                case 140:
                    originalScript = "game.global_flags[563] = 1";
                    SetGlobalFlag(563, true);
                    break;
                case 61:
                    originalScript = "pc.money_adj(-10000000)";
                    pc.AdjustMoney(-10000000);
                    break;
                case 81:
                    originalScript = "pc.money_adj(-20000000)";
                    pc.AdjustMoney(-20000000);
                    break;
                case 101:
                    originalScript = "pc.money_adj(-30000000)";
                    pc.AdjustMoney(-30000000);
                    break;
                case 121:
                    originalScript = "pc.money_adj(-40000000)";
                    pc.AdjustMoney(-40000000);
                    break;
                case 141:
                    originalScript = "pc.money_adj(-50000000)";
                    pc.AdjustMoney(-50000000);
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
                case 32:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 20);
                    return true;
                case 41:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 25);
                    return true;
                case 43:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 24);
                    return true;
                case 45:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 23);
                    return true;
                case 47:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 22);
                    return true;
                case 49:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 21);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
