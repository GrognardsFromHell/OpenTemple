using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Feats;

namespace OpenTemple.Core.Systems
{
    /// <summary>
    /// Stuff from this class was originall part of the item system.
    /// </summary>
    public class WeaponSystem
    {
        private class WeaponTypeProperties
        {
            public readonly DamageType damType;

            public WeaponTypeProperties(DamageType damType)
            {
                this.damType = damType;
            }
        }

        private Dictionary<WeaponType, WeaponTypeProperties> wpnProps;

        public WeaponSystem()
        {
            wpnProps = new Dictionary<WeaponType, WeaponTypeProperties>
            {
                {WeaponType.javelin, new WeaponTypeProperties(DamageType.Piercing)},
                {WeaponType.dagger, new WeaponTypeProperties(DamageType.PiercingAndSlashing)},
                {WeaponType.short_sword, new WeaponTypeProperties(DamageType.Piercing)},
                {WeaponType.longsword, new WeaponTypeProperties(DamageType.Slashing)},
                {WeaponType.dart, new WeaponTypeProperties(DamageType.Piercing)},
                {WeaponType.dwarven_waraxe, new WeaponTypeProperties(DamageType.Slashing)},
                {WeaponType.quarterstaff, new WeaponTypeProperties(DamageType.Bludgeoning)},
                {WeaponType.light_crossbow, new WeaponTypeProperties(DamageType.Piercing)},
                {WeaponType.morningstar, new WeaponTypeProperties(DamageType.BludgeoningAndPiercing)},
                {WeaponType.shuriken, new WeaponTypeProperties(DamageType.Piercing)},
            };
            // todo: wakizashi, cutlass, or just generalize the fucking thing
        }

        public string GetName(WeaponType wpnType)
        {
            //Use the weapon focus feat to the the name of the feat
            var weaponFocusFeat = (FeatId) ((int) FeatId.WEAPON_FOCUS_GAUNTLET + wpnType);
            string weaponFocusFeatName = GameSystems.Feat.GetFeatName(weaponFocusFeat);
            var nIdx1 = weaponFocusFeatName.IndexOf("(", StringComparison.Ordinal);
            var nIdx2 = weaponFocusFeatName.IndexOf(")", StringComparison.Ordinal);
            var strWeaponName = weaponFocusFeatName.Substring(nIdx1 + 1, nIdx2 - nIdx1 - 1);
            return strWeaponName;
        }

        [TempleDllLocation(0x10065cc0)]
        public bool IsSimple(WeaponType wpnType)
        {
            if (wpnType == WeaponType.longspear || (wpnType <= WeaponType.javelin && wpnType >= WeaponType.gauntlet))
            {
                return true;
            }

            return false;
        }

        [TempleDllLocation(0x10065ce0)]
        public bool IsMartial(WeaponType wpnType)
        {
            if (wpnType <= WeaponType.composite_longbow && wpnType != WeaponType.longspear &&
                wpnType >= WeaponType.throwing_axe)
            {
                return true;
            }

            if (wpnType == WeaponType.kukri)
            {
                return true;
            }

            return false;
        }

        [TempleDllLocation(0x10065d00)]
        public bool IsExotic(WeaponType wpnType)
        {
            if (wpnType >= WeaponType.halfling_kama && wpnType <= WeaponType.grenade && wpnType != WeaponType.kukri)
            {
                return true;
            }

            return false;
        }

        [TempleDllLocation(0x10065d80)]
        public bool IsHalflingWeapon(WeaponType type)
        {
            switch (type)
            {
                case WeaponType.quarterstaff:
                case WeaponType.halfling_kama:
                case WeaponType.halfling_nunchaku:
                case WeaponType.halfling_siangham:
                case WeaponType.kama:
                case WeaponType.nunchaku:
                case WeaponType.siangham:
                    return true;
                default:
                    return false;
            }
        }

        [TempleDllLocation(0x10065d20)]
        public bool IsDruidWeapon(WeaponType wpnType)
        {
            switch (wpnType)
            {
                case WeaponType.dagger:
                case WeaponType.sickle:
                case WeaponType.club:
                case WeaponType.shortspear:
                case WeaponType.quarterstaff:
                case WeaponType.spear:
                case WeaponType.dart:
                case WeaponType.sling:
                case WeaponType.scimitar:
                case WeaponType.longspear:
                    return true;
                default:
                    return false;
            }

            return false;
        }


        [TempleDllLocation(0x10065e40)]
        public bool IsMonkWeapon(WeaponType wpnType)
        {
            switch (wpnType)
            {
                case WeaponType.dagger:
                case WeaponType.club:
                case WeaponType.quarterstaff:
                case WeaponType.light_crossbow:
                case WeaponType.sling:
                case WeaponType.heavy_crossbow:
                case WeaponType.javelin:
                case WeaponType.handaxe:
                case WeaponType.kama:
                case WeaponType.nunchaku:
                case WeaponType.siangham:
                case WeaponType.shuriken:
                    return true;
                default:
                    return false;
            }
        }

        [TempleDllLocation(0x10065eb0)]
        public bool IsRogueWeapon(SizeCategory wielderSize, WeaponType wpnType)
        {
            // TODO: looks like Troika intended to differentiate by the Wielder's Size? Was not implemented
            if (IsSimple(wpnType))
            {
                return true;
            }

            switch (wpnType)
            {
                case WeaponType.hand_crossbow:
                case WeaponType.rapier:
                case WeaponType.short_sword:
                case WeaponType.sap:
                case WeaponType.shortbow:
                case WeaponType.composite_shortbow:
                    return true;
                default:
                    return false;
            }
        }

        [TempleDllLocation(0x10065f10)]
        public bool IsWizardWeapon(WeaponType wpnType)
        {
            switch (wpnType)
            {
                case WeaponType.dagger:
                case WeaponType.club:
                case WeaponType.quarterstaff:
                case WeaponType.light_crossbow:
                case WeaponType.heavy_crossbow:
                    return true;
                default:
                    return false;
            }
        }

        [TempleDllLocation(0x10065f50)]
        public bool IsElvenWeapon(WeaponType wpnType)
        {
            switch (wpnType)
            {
                case WeaponType.longsword:
                case WeaponType.rapier:
                case WeaponType.shortbow:
                case WeaponType.composite_shortbow:
                case WeaponType.longbow:
                case WeaponType.composite_longbow:
                    return true;
                default:
                    return false;
            }

            return false;
        }

        [TempleDllLocation(0x10065de0)]
        public bool IsBardWeapon(WeaponType wpnType)
        {
            if (IsSimple(wpnType))
            {
                return true;
            }

            switch (wpnType)
            {
                case WeaponType.sap:
                case WeaponType.short_sword:
                case WeaponType.longsword:
                case WeaponType.rapier:
                case WeaponType.shortbow:
                case WeaponType.composite_shortbow:
                case WeaponType.whip:
                    return true;
                default:
                    return false;
            }
        }

        public bool IsSlashingOrBludgeoning(GameObject weapon)
        {
            return IsSlashingOrBludgeoning(weapon.GetWeaponType());
        }

        bool IsSlashingOrBludgeoning(WeaponType wpnType)
        {
            switch (wpnType)
            {
                case WeaponType.gauntlet:
                case WeaponType.light_mace:
                case WeaponType.sickle:
                case WeaponType.club:
                case WeaponType.heavy_mace:
                case WeaponType.morningstar:
                case WeaponType.quarterstaff:
                case WeaponType.sling:
                case WeaponType.throwing_axe:
                case WeaponType.light_hammer:
                case WeaponType.handaxe:
                case WeaponType.kukri:
                case WeaponType.sap:
                case WeaponType.battleaxe:
                case WeaponType.light_flail:
                case WeaponType.heavy_flail:
                case WeaponType.longsword:
                case WeaponType.scimitar:
                case WeaponType.warhammer:
                case WeaponType.falchion:
                case WeaponType.glaive:
                case WeaponType.greataxe:
                case WeaponType.greatclub:
                case WeaponType.greatsword:
                case WeaponType.guisarme:
                case WeaponType.halberd:
                case WeaponType.scythe:
                case WeaponType.kama:
                case WeaponType.halfling_kama:
                case WeaponType.nunchaku:
                case WeaponType.bastard_sword:
                case WeaponType.dwarven_waraxe:
                case WeaponType.whip:
                case WeaponType.orc_double_axe:
                case WeaponType.dire_flail:
                case WeaponType.gnome_hooked_hammer:
                case WeaponType.two_bladed_sword:
                case WeaponType.dwarven_urgrosh:
                    return true;
                default:
                    return false;
            }

            /*if (IsSlashingWeapon(wpnType) || IsBludgeoningWeapon(wpnType))
                return true;*/
            return false;
        }

        public bool IsSlashingWeapon(WeaponType wpnType)
        {
            if (!wpnProps.TryGetValue(wpnType, out var weaponProps))
            {
                return false;
            }

            return weaponProps.damType == DamageType.Slashing
                   || weaponProps.damType == DamageType.SlashingAndBludgeoning
                   || weaponProps.damType == DamageType.PiercingAndSlashing
                   || weaponProps.damType == DamageType.SlashingAndBludgeoningAndPiercing;
        }

        public bool IsPiercingWeapon(WeaponType wpnType)
        {
            if (!wpnProps.TryGetValue(wpnType, out var weaponProps))
            {
                return false;
            }

            return weaponProps.damType == DamageType.Piercing
                   || weaponProps.damType == DamageType.PiercingAndSlashing
                   || weaponProps.damType == DamageType.BludgeoningAndPiercing
                   || weaponProps.damType == DamageType.SlashingAndBludgeoningAndPiercing;
        }

        public bool IsBludgeoningWeapon(WeaponType wpnType)
        {
            if (!wpnProps.TryGetValue(wpnType, out var weaponProps))
            {
                return false;
            }

            return weaponProps.damType == DamageType.Bludgeoning
                   || weaponProps.damType == DamageType.SlashingAndBludgeoning
                   || weaponProps.damType == DamageType.BludgeoningAndPiercing
                   || weaponProps.damType == DamageType.SlashingAndBludgeoningAndPiercing;
        }

        public int GetBaseHardness(GameObject item)
        {
            if (item.type == ObjectType.weapon)
            {
                var weaponType = item.GetWeaponType();
                return GetBaseHardness(weaponType);
            }

            if (item.type == ObjectType.armor)
            {
                return 100;
            }

            return 10;
        }

        public int GetBaseHardness(WeaponType weapon)
        {
            switch (weapon)
            {
                case WeaponType.light_mace:
                case WeaponType.club:
                case WeaponType.shortspear:
                case WeaponType.heavy_mace:
                case WeaponType.morningstar:
                case WeaponType.quarterstaff:
                case WeaponType.spear:
                case WeaponType.light_crossbow: // 14
                case WeaponType.dart:
                case WeaponType.sling:
                case WeaponType.heavy_crossbow: // 17
                case WeaponType.javelin:
                case WeaponType.throwing_axe:
                case WeaponType.light_hammer:
                case WeaponType.handaxe:
                case WeaponType.light_lance:
                case WeaponType.light_pick:

                case WeaponType.battleaxe:
                case WeaponType.light_flail:

                case WeaponType.heavy_pick:
                case WeaponType.rapier:

                case WeaponType.trident:
                case WeaponType.warhammer:

                case WeaponType.heavy_flail:
                case WeaponType.glaive:
                case WeaponType.greataxe:
                case WeaponType.greatclub:

                case WeaponType.guisarme:
                case WeaponType.halberd:
                case WeaponType.longspear: //43
                case WeaponType.ranseur:
                case WeaponType.scythe:
                case WeaponType.shortbow:
                case WeaponType.composite_shortbow:
                case WeaponType.longbow:
                case WeaponType.composite_longbow:
                case WeaponType.halfling_kama: // 50

                case WeaponType.kama: //54

                case WeaponType.dwarven_waraxe: // racial weapons - 58 and on
                case WeaponType.gnome_hooked_hammer:
                case WeaponType.orc_double_axe:

                case WeaponType.dwarven_urgrosh:
                case WeaponType.hand_crossbow: // 65

                case WeaponType.whip:
                case WeaponType.repeating_crossbow:
                case WeaponType.net:

                    return 5;
                default:
                    return 10;
            }
        }

        [TempleDllLocation(0x100b52d0)]
        public bool IsReachWeaponType(WeaponType weapType)
        {
            switch (weapType)
            {
                case WeaponType.glaive:
                case WeaponType.guisarme:
                case WeaponType.longspear:
                case WeaponType.ranseur:
                case WeaponType.spike_chain:
                    return true;
                default:
                    return false;
            }
        }

        public bool IsMeleeWeapon(WeaponType wpnType)
        {
            switch (wpnType)
            {
                case WeaponType.light_crossbow:
                case WeaponType.dart:
                case WeaponType.sling:
                case WeaponType.heavy_crossbow:
                case WeaponType.javelin:
                case WeaponType.shortbow:
                case WeaponType.composite_shortbow:
                case WeaponType.longbow:
                case WeaponType.composite_longbow:
                case WeaponType.hand_crossbow:
                case WeaponType.shuriken:
                case WeaponType.repeating_crossbow:
                case WeaponType.net:
                case WeaponType.ray:
                case WeaponType.grenade:
                    return false;

                default:
                    return true;
            }
        }

        public bool IsRangedWeapon(WeaponType wpnType)
        {
            switch (wpnType)
            {
                case WeaponType.dagger:
                case WeaponType.shortspear:
                case WeaponType.spear:
                case WeaponType.light_crossbow:
                case WeaponType.dart:
                case WeaponType.sling:
                case WeaponType.heavy_crossbow:
                case WeaponType.javelin:
                case WeaponType.throwing_axe:
                case WeaponType.light_hammer:
                case WeaponType.trident:
                case WeaponType.shortbow:
                case WeaponType.composite_shortbow:
                case WeaponType.longbow:
                case WeaponType.composite_longbow:
                case WeaponType.hand_crossbow:
                case WeaponType.shuriken:
                case WeaponType.repeating_crossbow:
                case WeaponType.net:
                case WeaponType.ray:
                case WeaponType.grenade:
                    return true;

                default:
                    return false;
            }
        }

        [TempleDllLocation(0x10065780, secondary: true)]
        public bool IsCrossbow(WeaponType type)
        {
            return type == WeaponType.light_crossbow || type == WeaponType.heavy_crossbow;
        }

        public bool IsBow(WeaponType type)
        {
            return type >= WeaponType.shortbow && type <= WeaponType.composite_longbow;
        }

        [TempleDllLocation(0x1008f330, secondary: true)]
        public bool IsThrowingWeapon(WeaponType type)
        {
            return type >= WeaponType.punching_dagger && type <= WeaponType.javelin;
        }

        public bool IsTripWeapon(WeaponType type)
        {
            switch (type)
            {
                case WeaponType.dire_flail:
                case WeaponType.heavy_flail:
                case WeaponType.light_flail:
                case WeaponType.gnome_hooked_hammer:
                case WeaponType.guisarme:
                case WeaponType.halberd:
                case WeaponType.kama:
                case WeaponType.scythe:
                case WeaponType.sickle:
                case WeaponType.whip:
                case WeaponType.spike_chain:
                    return true;
                default:
                    return false;
            }
        }
    }
}