
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
    [DialogScript(74)]
    public class LubashDialog : Lubash, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 6:
                case 11:
                case 13:
                case 101:
                case 104:
                    originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 7";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 7;
                case 3:
                case 7:
                case 12:
                case 14:
                case 102:
                case 105:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) >= 3";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 3;
                case 4:
                case 8:
                case 103:
                case 106:
                    originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 12";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 12;
                case 9:
                    originalScript = "pc != game.party[0] and game.party[0].distance_to(pc) <= 40 and not critter_is_unconscious(game.party[0]) and anyone(game.party[0].group_list(), \"has_wielded\", 3005)";
                    return pc != PartyLeader && PartyLeader.DistanceTo(pc) <= 40 && !Utilities.critter_is_unconscious(PartyLeader) && PartyLeader.GetPartyMembers().Any(o => o.HasEquippedByName(3005));
                case 51:
                    originalScript = "pc.money_get() >= 5000";
                    return pc.GetMoney() >= 5000;
                case 61:
                    originalScript = "pc.money_get() >= 10000";
                    return pc.GetMoney() >= 10000;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 5:
                case 53:
                case 55:
                case 71:
                case 72:
                case 73:
                case 74:
                    originalScript = "npc.attack(pc)";
                    npc.Attack(pc);
                    break;
                case 9:
                    originalScript = "call_leader(npc, pc)";
                    call_leader(npc, pc);
                    break;
                case 51:
                    originalScript = "pc.money_adj(-5000)";
                    pc.AdjustMoney(-5000);
                    break;
                case 61:
                    originalScript = "pc.money_adj(-10000)";
                    pc.AdjustMoney(-10000);
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
                case 2:
                case 6:
                case 11:
                case 13:
                case 101:
                case 104:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 7);
                    return true;
                case 3:
                case 7:
                case 12:
                case 14:
                case 102:
                case 105:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 3);
                    return true;
                case 4:
                case 8:
                case 103:
                case 106:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 12);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
