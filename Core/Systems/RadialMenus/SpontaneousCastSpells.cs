using System;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems.Spells;

namespace OpenTemple.Core.Systems.RadialMenus
{
    public static class SpontaneousCastSpells
    {
        [TempleDllLocation(0x100f0e05)]
        private static readonly int[] GoodCleric =
        {
            WellKnownSpells.CureMinorWounds,
            WellKnownSpells.CureLightWounds,
            WellKnownSpells.CureModerateWounds,
            WellKnownSpells.CureSeriousWounds,
            WellKnownSpells.CureCriticalWounds,
            WellKnownSpells.HealingCircle,
            577,
            578,
            579,
            579,
            -1
        };

        [TempleDllLocation(0x100f0ed3)]
        private static readonly int[] EvilCleric =
        {
            WellKnownSpells.InflictMinorWounds,
            WellKnownSpells.InflictLightWounds,
            WellKnownSpells.InflictModerateWounds,
            WellKnownSpells.InflictSeriousWounds,
            WellKnownSpells.InflictCriticalWounds,
            WellKnownSpells.CircleOfDoom,
            581,
            582,
            583,
            583,
            -1
        };

        [TempleDllLocation(0x100f12e4)]
        private static readonly int[] Druid =
        {
            -1,
            WellKnownSpells.SummonNaturesAllyI,
            WellKnownSpells.SummonNaturesAllyIi,
            WellKnownSpells.SummonNaturesAllyIii,
            WellKnownSpells.SummonNaturesAllyIv,
            WellKnownSpells.SummonNaturesAllyV,
            WellKnownSpells.SummonNaturesAllyVi,
            WellKnownSpells.SummonNaturesAllyVii,
            WellKnownSpells.SummonNaturesAllyViii,
            WellKnownSpells.SummonNaturesAllyIx,
            4000
        };

        // Used for lookup into spell options
        [TempleDllLocation(0x100f0f82)]
        private static readonly int[] DruidMesKey =
        {
            -1, 2000, 2100, 2200, 2300, 2400, 2500, 2600, 2700, 2800, -1
        };

        public static bool TryGet(SpontCastType type, int spellLevel, out int spellEnum)
        {
            spellEnum = -1;
            switch (type)
            {
                case SpontCastType.None:
                    return false;
                case SpontCastType.GoodCleric:
                    if (spellLevel < GoodCleric.Length)
                    {
                        spellEnum = GoodCleric[spellLevel];
                    }

                    return spellEnum != -1;
                case SpontCastType.EvilCleric:
                    if (spellLevel < EvilCleric.Length)
                    {
                        spellEnum = EvilCleric[spellLevel];
                    }

                    return spellEnum != -1;
                case SpontCastType.Druid:
                    if (spellLevel < Druid.Length)
                    {
                        spellEnum = Druid[spellLevel];
                    }

                    return spellEnum != -1;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static bool TryGetDruidOptionsKey(int spellLevel, out int optionsKey)
        {
            if (spellLevel < DruidMesKey.Length)
            {
                optionsKey = DruidMesKey[spellLevel];
                return true;
            }

            optionsKey = -1;
            return false;
        }
    }
}