using System;

namespace OpenTemple.Core.GameObjects
{
    [Flags]
    public enum ArmorTypeFlag
    {
        ArmorType1 = 0x01,
        ArmorType2 = 0x02,
        HelmType1 = 0x04,
        HelmType2 = 0x08,
        ArmorNone = 0x10,
    }

    [Flags]
    public enum ArmorFlag
    {
        TYPE_LIGHT = 0,
        TYPE_MEDIUM = 1,
        TYPE_HEAVY = 2,
        TYPE_SHIELD = 3,
        TYPE_BITMASK = TYPE_LIGHT|TYPE_MEDIUM|TYPE_HEAVY,
        HELM_TYPE_SMALL = 0,
        HELM_TYPE_MEDIUM = 4,
        HELM_TYPE_LARGE = 8,
        HELM_BITMASK = HELM_TYPE_SMALL|HELM_TYPE_MEDIUM|HELM_TYPE_LARGE,
        TYPE_NONE = 0x10
    }

    public static class ArmorFlagExtensions {

        public static ArmorFlag GetArmorType(this ArmorFlag armorFlag)
        {
            return armorFlag & (ArmorFlag.TYPE_BITMASK | ArmorFlag.TYPE_NONE);
        }

        public static bool IsLightArmorOrLess(this ArmorFlag armorFlag) { 
            if ((armorFlag & ArmorFlag.TYPE_NONE) != 0)
            {
                return true; // Marked as not being armor
            }

            return armorFlag.GetArmorType() == ArmorFlag.TYPE_LIGHT;
        }

        public static bool IsShield(this ArmorFlag armorFlag)
        {
            if (armorFlag.HasFlag(ArmorFlag.TYPE_NONE))
            {
                return false;
            }

            return (armorFlag & ArmorFlag.TYPE_BITMASK) == ArmorFlag.TYPE_SHIELD;
        }

    }

}