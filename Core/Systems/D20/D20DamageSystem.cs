using System;
using System.Collections.Generic;
using System.Text;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems.D20
{
    public class D20DamageSystem
    {

        private static readonly D20AttackPower[] AttackPowers =
            (D20AttackPower[]) Enum.GetValues(typeof(D20AttackPower));

        private readonly Dictionary<int, string> _translations;

        [TempleDllLocation(0x100e0360)]
        public D20DamageSystem()
        {
            _translations = Tig.FS.ReadMesFile("mes/damage.mes");
        }

        public string GetTranslation(int id)
        {
            return _translations[id];
        }

        [TempleDllLocation(0x100e0ab0)]
        public string GetDamageTypeName(DamageType type)
        {
            return _translations[1001 + (int) type];
        }

        private string GetSingleAttackPowerName(D20AttackPower attackPower)
        {
            switch (attackPower)
            {
                case D20AttackPower.NORMAL:
                    return _translations[2000];
                case D20AttackPower.UNSPECIFIED:
                    return _translations[2001];
                case D20AttackPower.SILVER:
                    return _translations[2002];
                case D20AttackPower.MAGIC:
                    return _translations[2003];
                case D20AttackPower.HOLY:
                    return _translations[2004];
                case D20AttackPower.UNHOLY:
                    return _translations[2005];
                case D20AttackPower.CHAOS:
                    return _translations[2006];
                case D20AttackPower.LAW:
                    return _translations[2007];
                case D20AttackPower.ADAMANTIUM:
                    return _translations[2008];
                case D20AttackPower.BLUDGEONING:
                    return _translations[2009];
                case D20AttackPower.PIERCING:
                    return _translations[2010];
                case D20AttackPower.SLASHING:
                    return _translations[2011];
                case D20AttackPower.MITHRIL:
                    return _translations[2012];
                case D20AttackPower.COLD:
                    return _translations[2013];
                default:
                    throw new ArgumentOutOfRangeException(nameof(attackPower), attackPower, null);
            }
        }

        [TempleDllLocation(0x100e0b70)]
        public string GetAttackPowerName(D20AttackPower attackPowerMask)
        {
            string singleResult = null;
            StringBuilder combinedResult = null;
            foreach (var attackPower in AttackPowers)
            {
                if ((attackPowerMask & attackPower) == attackPower)
                {
                    if (singleResult == null)
                    {
                        // First matching attack power
                        singleResult = GetSingleAttackPowerName(attackPower);
                    }
                    else
                    {
                        if (combinedResult == null)
                        {
                            // This is the second matching attack power
                            combinedResult = new StringBuilder(singleResult);
                        }

                        combinedResult.Append(';');
                        combinedResult.Append(GetSingleAttackPowerName(attackPower));
                    }
                }
            }

            return combinedResult?.ToString() ?? singleResult;
        }

    }
}