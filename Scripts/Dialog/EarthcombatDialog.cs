
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
    [DialogScript(446)]
    public class EarthcombatDialog : Earthcombat, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1021:
                    originalScript = "grate_strength() == 0";
                    return grate_strength() == 0;
                case 1022:
                    originalScript = "grate_strength() == 1";
                    return grate_strength() == 1;
                case 1023:
                    originalScript = "grate_strength() == 2";
                    return grate_strength() == 2;
                case 1501:
                case 1502:
                case 1556:
                    originalScript = "game.global_vars[454] & 2**8 == 0";
                    return (GetGlobalVar(454) & 0x100) == 0;
                case 1503:
                    originalScript = "game.global_vars[454] & 2**8 != 0 and game.global_flags[104] == 0";
                    return (GetGlobalVar(454) & 0x100) != 0 && !GetGlobalFlag(104);
                case 1504:
                    originalScript = "game.global_vars[454] & 2**9 != 0 and game.global_flags[104] == 0";
                    return (GetGlobalVar(454) & 0x200) != 0 && !GetGlobalFlag(104);
                case 1505:
                    originalScript = "npc_get(npc, 1) == 0 and 0==1";
                    return !ScriptDaemon.npc_get(npc, 1) && 0 == 1;
                case 1506:
                    originalScript = "game.global_flags[104] == 0";
                    return !GetGlobalFlag(104);
                case 1507:
                    originalScript = "game.global_flags[104] == 1";
                    return GetGlobalFlag(104);
                case 1521:
                case 1523:
                    originalScript = "pc.skill_level_get(skill_diplomacy) < 8";
                    return pc.GetSkillLevel(SkillId.diplomacy) < 8;
                case 1522:
                case 1524:
                    originalScript = "pc.skill_level_get(skill_diplomacy) >= 8";
                    return pc.GetSkillLevel(SkillId.diplomacy) >= 8;
                case 1557:
                    originalScript = "game.global_vars[454] & 2**8 != 0";
                    return (GetGlobalVar(454) & 0x100) != 0;
                case 1562:
                case 1575:
                case 1805:
                case 1902:
                case 1903:
                case 1905:
                    originalScript = "game.party_alignment &  ALIGNMENT_EVIL != 0";
                    return PartyAlignment.IsEvil();
                case 1563:
                case 1576:
                case 1806:
                case 1906:
                    originalScript = "game.party_alignment &  ALIGNMENT_GOOD != 0";
                    return PartyAlignment.IsGood();
                case 1564:
                case 1577:
                case 1807:
                case 1907:
                    originalScript = "game.party_alignment &  ALIGNMENT_EVIL == 0 and game.party_alignment &  ALIGNMENT_GOOD == 0";
                    return !PartyAlignment.IsEvil() && !PartyAlignment.IsGood();
                case 1574:
                case 1804:
                    originalScript = "tpsts(507, 1) == 0";
                    return !ScriptDaemon.tpsts(507, 1);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1031:
                case 1036:
                case 1041:
                    originalScript = "grate_away(npc)";
                    grate_away(npc);
                    break;
                case 1505:
                    originalScript = "npc_set(npc, 1)";
                    ScriptDaemon.npc_set(npc, 1);
                    break;
                case 1507:
                case 1700:
                case 1720:
                case 2101:
                    originalScript = "barrier_away(npc,1)";
                    barrier_away(npc, 1);
                    break;
                case 1536:
                case 1550:
                    originalScript = "unequip(3, pc, 1); unequip(4, pc, 1)";
                    Co8.unequip(EquipSlot.WeaponPrimary, pc, true);
                    Co8.unequip(EquipSlot.WeaponSecondary, pc, true);
                    break;
                case 1551:
                    originalScript = "earth_temple_haul_inside(pc, npc, 1555)";
                    earth_temple_haul_inside(pc, npc, 1555);
                    break;
                case 1555:
                case 2020:
                    originalScript = "game.sound( 4057, 1 )";
                    Sound(4057, 1);
                    break;
                case 1556:
                    originalScript = "game.global_vars[454] |= 2**8";
                    SetGlobalVar(454, GetGlobalVar(454) | 0x100);
                    break;
                case 1561:
                case 1572:
                case 1802:
                    originalScript = "switch_to_npc(pc, npc, 'romag',  failsafe_line = 2000)";
                    switch_to_npc(pc, npc, "romag", failsafe_line: 2000);
                    break;
                case 1562:
                case 1563:
                case 1564:
                case 1575:
                case 1576:
                case 1577:
                case 1805:
                case 1806:
                case 1807:
                case 1905:
                case 1906:
                case 1907:
                    originalScript = "earth_attack_party(pc, npc)";
                    earth_attack_party(pc, npc);
                    break;
                case 1571:
                case 1801:
                case 1901:
                case 1903:
                case 1921:
                case 2001:
                    originalScript = "move_party_inside(pc, npc, 2020, 454, 372)";
                    move_party_inside(pc, npc, 2020, 454, 372);
                    break;
                case 1573:
                case 1803:
                    originalScript = "switch_to_npc(pc, npc, 'hartsch',  failsafe_line = 2000)";
                    switch_to_npc(pc, npc, "hartsch", failsafe_line: 2000);
                    break;
                case 1574:
                case 1804:
                    originalScript = "switch_to_npc(pc, npc, 'earth troop commander',  failsafe_line = 2000)";
                    switch_to_npc(pc, npc, "earth troop commander", failsafe_line: 2000);
                    break;
                case 1701:
                case 1705:
                    originalScript = "barrier_away(npc,2)";
                    barrier_away(npc, 2);
                    break;
                case 1706:
                    originalScript = "barrier_away(npc,3)";
                    barrier_away(npc, 3);
                    break;
                case 1721:
                case 1725:
                    originalScript = "barrier_away(npc,4)";
                    barrier_away(npc, 4);
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
                case 1522:
                case 1524:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 8);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
