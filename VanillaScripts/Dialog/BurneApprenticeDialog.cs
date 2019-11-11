
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
    [DialogScript(262)]
    public class BurneApprenticeDialog : BurneApprentice, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "not npc.has_met( pc )";
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                case 12:
                    originalScript = "npc.has_met( pc )";
                    return npc.HasMet(pc);
                case 6:
                case 7:
                    originalScript = "game.global_flags[358] == 1 and not npc.has_met( pc ) and game.global_flags[359] == 0";
                    return GetGlobalFlag(358) && !npc.HasMet(pc) && !GetGlobalFlag(359);
                case 8:
                case 9:
                    originalScript = "game.global_flags[358] == 1 and npc.has_met( pc ) and game.global_flags[359] == 0";
                    return GetGlobalFlag(358) && npc.HasMet(pc) && !GetGlobalFlag(359);
                case 10:
                case 11:
                    originalScript = "game.global_flags[359] == 1";
                    return GetGlobalFlag(359);
                case 47:
                case 48:
                case 55:
                case 56:
                case 73:
                case 74:
                case 85:
                case 86:
                case 105:
                case 106:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 2208 ) and anyone( pc.group_list(), \"has_item\", 4003 ) and anyone( pc.group_list(), \"has_item\", 4004 ) and anyone( pc.group_list(), \"has_item\", 3603 ) and anyone( pc.group_list(), \"has_item\", 2203 )";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(2208)) && pc.GetPartyMembers().Any(o => o.HasItemByName(4003)) && pc.GetPartyMembers().Any(o => o.HasItemByName(4004)) && pc.GetPartyMembers().Any(o => o.HasItemByName(3603)) && pc.GetPartyMembers().Any(o => o.HasItemByName(2203));
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 47:
                case 48:
                case 55:
                case 56:
                case 73:
                case 74:
                case 85:
                case 86:
                case 105:
                case 106:
                    originalScript = "party_transfer_to( npc, 2208 ); party_transfer_to( npc, 4003 ); party_transfer_to( npc, 4004 ); party_transfer_to( npc, 3603 ); party_transfer_to( npc, 2203 )";
                    Utilities.party_transfer_to(npc, 2208);
                    Utilities.party_transfer_to(npc, 4003);
                    Utilities.party_transfer_to(npc, 4004);
                    Utilities.party_transfer_to(npc, 3603);
                    Utilities.party_transfer_to(npc, 2203);
                    ;
                    break;
                case 90:
                    originalScript = "destroy_orb( npc, pc ); game.global_flags[359] = 1";
                    destroy_orb(npc, pc);
                    SetGlobalFlag(359, true);
                    ;
                    break;
                case 121:
                    originalScript = "play_effect( npc, pc )";
                    play_effect(npc, pc);
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
