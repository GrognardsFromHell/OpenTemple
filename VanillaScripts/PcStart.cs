using System;
using System.Collections.Generic;
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Systems.Script.Hooks;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    public class PcStart : IStartingEquipmentHook
    {
        private static readonly Dictionary<FeatId, int> proto_table = new Dictionary<FeatId, int>
        {
            {FeatId.EXOTIC_WEAPON_PROFICIENCY_BASTARD_SWORD, 4015},
            {FeatId.EXOTIC_WEAPON_PROFICIENCY_DWARVEN_WARAXE, 4063},
            {FeatId.EXOTIC_WEAPON_PROFICIENCY_GNOME_HOOKED_HAMMER, 4075},
            {FeatId.EXOTIC_WEAPON_PROFICIENCY_ORC_DOUBLE_AXE, 4062},
            {FeatId.EXOTIC_WEAPON_PROFICIENCY_SPIKE_CHAIN, 4209},
            {FeatId.EXOTIC_WEAPON_PROFICIENCY_SHURIKEN, 4211},
            {FeatId.IMPROVED_CRITICAL_DAGGER, 4060},
            {FeatId.IMPROVED_CRITICAL_LIGHT_MACE, 4071},
            {FeatId.IMPROVED_CRITICAL_CLUB, 4074},
            {FeatId.IMPROVED_CRITICAL_HALFSPEAR, 4116},
            {FeatId.IMPROVED_CRITICAL_HEAVY_MACE, 4068},
            {FeatId.IMPROVED_CRITICAL_MORNINGSTAR, 4070},
            {FeatId.IMPROVED_CRITICAL_QUARTERSTAFF, 4110},
            {FeatId.IMPROVED_CRITICAL_SHORTSPEAR, 4117},
            {FeatId.IMPROVED_CRITICAL_LIGHT_CROSSBOW, 4096},
            {FeatId.IMPROVED_CRITICAL_DART, 4127},
            {FeatId.IMPROVED_CRITICAL_SLING, 4115},
            {FeatId.IMPROVED_CRITICAL_HEAVY_CROSSBOW, 4097},
            {FeatId.IMPROVED_CRITICAL_JAVELIN, 4123},
            {FeatId.IMPROVED_CRITICAL_LIGHT_HAMMER, 4076},
            {FeatId.IMPROVED_CRITICAL_HANDAXE, 4067},
            {FeatId.IMPROVED_CRITICAL_SHORT_SWORD, 4049},
            {FeatId.IMPROVED_CRITICAL_BATTLEAXE, 4114},
            {FeatId.IMPROVED_CRITICAL_LONGSWORD, 4036},
            {FeatId.IMPROVED_CRITICAL_HEAVY_PICK, 4069},
            {FeatId.IMPROVED_CRITICAL_RAPIER, 4009},
            {FeatId.IMPROVED_CRITICAL_SCIMITAR, 4045},
            {FeatId.IMPROVED_CRITICAL_WARHAMMER, 4077},
            {FeatId.IMPROVED_CRITICAL_FALCHION, 4026},
            {FeatId.IMPROVED_CRITICAL_HEAVY_FLAIL, 4207},
            {FeatId.IMPROVED_CRITICAL_GLAIVE, 4118},
            {FeatId.IMPROVED_CRITICAL_GREATAXE, 4064},
            {FeatId.IMPROVED_CRITICAL_GREATCLUB, 4066},
            {FeatId.IMPROVED_CRITICAL_GREATSWORD, 4010},
            {FeatId.IMPROVED_CRITICAL_GUISARME, 4113},
            {FeatId.IMPROVED_CRITICAL_LONGSPEAR, 4194},
            {FeatId.IMPROVED_CRITICAL_RANSEUR, 4119},
            {FeatId.IMPROVED_CRITICAL_SCYTHE, 4072},
            {FeatId.IMPROVED_CRITICAL_SHORTBOW, 4201},
            {FeatId.IMPROVED_CRITICAL_LONGBOW, 4087},
            {FeatId.IMPROVED_CRITICAL_BASTARD_SWORD, 4015},
            {FeatId.IMPROVED_CRITICAL_DWARVEN_WARAXE, 4063},
            {FeatId.IMPROVED_CRITICAL_GNOME_HOOKED_HAMMER, 4075},
            {FeatId.IMPROVED_CRITICAL_ORC_DOUBLE_AXE, 4062},
            {FeatId.IMPROVED_CRITICAL_SPIKE_CHAIN, 4209},
            {FeatId.IMPROVED_CRITICAL_SHURIKEN, 4211},
            {FeatId.MARTIAL_WEAPON_PROFICIENCY_LIGHT_HAMMER, 4076},
            {FeatId.MARTIAL_WEAPON_PROFICIENCY_HANDAXE, 4067},
            {FeatId.MARTIAL_WEAPON_PROFICIENCY_SHORT_SWORD, 4049},
            {FeatId.MARTIAL_WEAPON_PROFICIENCY_BATTLEAXE, 4114},
            {FeatId.MARTIAL_WEAPON_PROFICIENCY_LONGSWORD, 4036},
            {FeatId.MARTIAL_WEAPON_PROFICIENCY_HEAVY_PICK, 4069},
            {FeatId.MARTIAL_WEAPON_PROFICIENCY_RAPIER, 4009},
            {FeatId.MARTIAL_WEAPON_PROFICIENCY_SCIMITAR, 4045},
            {FeatId.MARTIAL_WEAPON_PROFICIENCY_WARHAMMER, 4077},
            {FeatId.MARTIAL_WEAPON_PROFICIENCY_FALCHION, 4026},
            {FeatId.MARTIAL_WEAPON_PROFICIENCY_HEAVY_FLAIL, 4207},
            {FeatId.MARTIAL_WEAPON_PROFICIENCY_GLAIVE, 4118},
            {FeatId.MARTIAL_WEAPON_PROFICIENCY_GREATAXE, 4064},
            {FeatId.MARTIAL_WEAPON_PROFICIENCY_GREATCLUB, 4066},
            {FeatId.MARTIAL_WEAPON_PROFICIENCY_GREATSWORD, 4010},
            {FeatId.MARTIAL_WEAPON_PROFICIENCY_GUISARME, 4113},
            {FeatId.MARTIAL_WEAPON_PROFICIENCY_LONGSPEAR, 4194},
            {FeatId.MARTIAL_WEAPON_PROFICIENCY_RANSEUR, 4119},
            {FeatId.MARTIAL_WEAPON_PROFICIENCY_SCYTHE, 4072},
            {FeatId.MARTIAL_WEAPON_PROFICIENCY_SHORTBOW, 4201},
            {FeatId.MARTIAL_WEAPON_PROFICIENCY_LONGBOW, 4087},
            {FeatId.WEAPON_FINESSE_DAGGER, 4060},
            {FeatId.WEAPON_FINESSE_LIGHT_MACE, 4071},
            {FeatId.WEAPON_FINESSE_CLUB, 4074},
            {FeatId.WEAPON_FINESSE_HALFSPEAR, 4116},
            {FeatId.WEAPON_FINESSE_HEAVY_MACE, 4068},
            {FeatId.WEAPON_FINESSE_MORNINGSTAR, 4070},
            {FeatId.WEAPON_FINESSE_QUARTERSTAFF, 4110},
            {FeatId.WEAPON_FINESSE_SHORTSPEAR, 4117},
            {FeatId.WEAPON_FINESSE_LIGHT_CROSSBOW, 4096},
            {FeatId.WEAPON_FINESSE_DART, 4127},
            {FeatId.WEAPON_FINESSE_SLING, 4115},
            {FeatId.WEAPON_FINESSE_HEAVY_CROSSBOW, 4097},
            {FeatId.WEAPON_FINESSE_JAVELIN, 4123},
            {FeatId.WEAPON_FINESSE_LIGHT_HAMMER, 4076},
            {FeatId.WEAPON_FINESSE_HANDAXE, 4067},
            {FeatId.WEAPON_FINESSE_SHORT_SWORD, 4049},
            {FeatId.WEAPON_FINESSE_BATTLEAXE, 4114},
            {FeatId.WEAPON_FINESSE_LONGSWORD, 4036},
            {FeatId.WEAPON_FINESSE_HEAVY_PICK, 4069},
            {FeatId.WEAPON_FINESSE_RAPIER, 4009},
            {FeatId.WEAPON_FINESSE_SCIMITAR, 4045},
            {FeatId.WEAPON_FINESSE_WARHAMMER, 4077},
            {FeatId.WEAPON_FINESSE_FALCHION, 4026},
            {FeatId.WEAPON_FINESSE_HEAVY_FLAIL, 4207},
            {FeatId.WEAPON_FINESSE_GLAIVE, 4118},
            {FeatId.WEAPON_FINESSE_GREATAXE, 4064},
            {FeatId.WEAPON_FINESSE_GREATCLUB, 4066},
            {FeatId.WEAPON_FINESSE_GREATSWORD, 4010},
            {FeatId.WEAPON_FINESSE_GUISARME, 4113},
            {FeatId.WEAPON_FINESSE_LONGSPEAR, 4194},
            {FeatId.WEAPON_FINESSE_RANSEUR, 4119},
            {FeatId.WEAPON_FINESSE_SCYTHE, 4072},
            {FeatId.WEAPON_FINESSE_SHORTBOW, 4201},
            {FeatId.WEAPON_FINESSE_LONGBOW, 4087},
            {FeatId.WEAPON_FINESSE_BASTARD_SWORD, 4015},
            {FeatId.WEAPON_FINESSE_DWARVEN_WARAXE, 4063},
            {FeatId.WEAPON_FINESSE_GNOME_HOOKED_HAMMER, 4075},
            {FeatId.WEAPON_FINESSE_ORC_DOUBLE_AXE, 4062},
            {FeatId.WEAPON_FINESSE_SPIKE_CHAIN, 4209},
            {FeatId.WEAPON_FINESSE_SHURIKEN, 4211},
            {FeatId.WEAPON_FOCUS_DAGGER, 4060},
            {FeatId.WEAPON_FOCUS_LIGHT_MACE, 4071},
            {FeatId.WEAPON_FOCUS_CLUB, 4074},
            {FeatId.WEAPON_FOCUS_HALFSPEAR, 4116},
            {FeatId.WEAPON_FOCUS_HEAVY_MACE, 4068},
            {FeatId.WEAPON_FOCUS_MORNINGSTAR, 4070},
            {FeatId.WEAPON_FOCUS_QUARTERSTAFF, 4110},
            {FeatId.WEAPON_FOCUS_SHORTSPEAR, 4117},
            {FeatId.WEAPON_FOCUS_LIGHT_CROSSBOW, 4096},
            {FeatId.WEAPON_FOCUS_DART, 4127},
            {FeatId.WEAPON_FOCUS_SLING, 4115},
            {FeatId.WEAPON_FOCUS_HEAVY_CROSSBOW, 4097},
            {FeatId.WEAPON_FOCUS_JAVELIN, 4123},
            {FeatId.WEAPON_FOCUS_LIGHT_HAMMER, 4076},
            {FeatId.WEAPON_FOCUS_HANDAXE, 4067},
            {FeatId.WEAPON_FOCUS_SHORT_SWORD, 4049},
            {FeatId.WEAPON_FOCUS_BATTLEAXE, 4114},
            {FeatId.WEAPON_FOCUS_LONGSWORD, 4036},
            {FeatId.WEAPON_FOCUS_HEAVY_PICK, 4069},
            {FeatId.WEAPON_FOCUS_RAPIER, 4009},
            {FeatId.WEAPON_FOCUS_SCIMITAR, 4045},
            {FeatId.WEAPON_FOCUS_WARHAMMER, 4077},
            {FeatId.WEAPON_FOCUS_FALCHION, 4026},
            {FeatId.WEAPON_FOCUS_HEAVY_FLAIL, 4207},
            {FeatId.WEAPON_FOCUS_GLAIVE, 4118},
            {FeatId.WEAPON_FOCUS_GREATAXE, 4064},
            {FeatId.WEAPON_FOCUS_GREATCLUB, 4066},
            {FeatId.WEAPON_FOCUS_GREATSWORD, 4010},
            {FeatId.WEAPON_FOCUS_GUISARME, 4113},
            {FeatId.WEAPON_FOCUS_LONGSPEAR, 4194},
            {FeatId.WEAPON_FOCUS_RANSEUR, 4119},
            {FeatId.WEAPON_FOCUS_SCYTHE, 4072},
            {FeatId.WEAPON_FOCUS_SHORTBOW, 4201},
            {FeatId.WEAPON_FOCUS_LONGBOW, 4087},
            {FeatId.WEAPON_FOCUS_BASTARD_SWORD, 4015},
            {FeatId.WEAPON_FOCUS_DWARVEN_WARAXE, 4063},
            {FeatId.WEAPON_FOCUS_GNOME_HOOKED_HAMMER, 4075},
            {FeatId.WEAPON_FOCUS_ORC_DOUBLE_AXE, 4062},
            {FeatId.WEAPON_FOCUS_SPIKE_CHAIN, 4209},
            {FeatId.WEAPON_FOCUS_SHURIKEN, 4211},
        };

        public void GiveStartingEquipment(GameObject pc)
        {
            foreach (var (f, p) in proto_table)
            {
                if (pc.FindItemByProto(p) == null)
                {
                    if (pc.HasFeat(f))
                    {
                        var proto = proto_table[f];

                        Utilities.create_item_in_inventory(proto, pc);
                        if (((proto == 4201) || (proto == 4087)))
                        {
                            Utilities.create_item_in_inventory(5004, pc);
                        }
                        else if (((proto == 4096) || (proto == 4097)))
                        {
                            Utilities.create_item_in_inventory(5005, pc);
                        }
                        else if ((proto == 4115))
                        {
                            Utilities.create_item_in_inventory(5007, pc);
                        }
                    }
                }
            }
        }
    }
}