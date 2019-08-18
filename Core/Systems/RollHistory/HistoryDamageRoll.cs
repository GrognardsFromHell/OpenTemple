using System.Text;
using SpicyTemple.Core.Systems.D20;

namespace SpicyTemple.Core.Systems.RollHistory
{
    // Damage dealt
    public class HistoryDamageRoll : HistoryEntry
    {
        public DamagePacket DamagePacket { get; }

        public HistoryDamageRoll(DamagePacket damagePacket)
        {
            DamagePacket = damagePacket;
        }

        [TempleDllLocation(0x10048470)]
        internal override void PrintToConsole(StringBuilder builder)
        {
            if (DamagePacket.GetOverallDamageByType() > 0)
            {
                for (var damageType = DamageType.Bludgeoning; damageType <= DamageType.Magic; damageType++)
                {
                    var damage = DamagePacket.GetOverallDamageByType(damageType);
                    if (damage > 0)
                    {
                        var damageTypeText = GameSystems.D20.Damage.GetDamageTypeName(damageType);
                        AppendDamage(builder, damage, damageTypeText);
                        builder.Append('\n');
                    }
                }
            }
            else
            {
                AppendTakesNoDamage(builder);
            }
        }

        [TempleDllLocation(0x100479c0)]
        private void AppendDamage(StringBuilder builder, int damage, string damageTypeName)
        {
            var victimName = GameSystems.MapObject.GetDisplayNameForParty(obj2);
            var damageText = GameSystems.RollHistory.GetTranslation(47); // damage
            var ofText = GameSystems.RollHistory.GetTranslation(40); // of
            var pointsText = GameSystems.RollHistory.GetTranslation(39); // points
            var takesText = GameSystems.RollHistory.GetTranslation(38); // takes

            builder.AppendFormat("{0} {1} ~{2} {3}~[ROLL_{4}] {5} {6} {7}", victimName, takesText, damage, pointsText,
                histId, ofText, damageTypeName, damageText);
        }

        [TempleDllLocation(0x10047930)]
        private void AppendTakesNoDamage(StringBuilder builder)
        {
            var victimName = GameSystems.MapObject.GetDisplayNameForParty(obj2);
            var noDamageText = GameSystems.RollHistory.GetTranslation(52); // no damage
            var takesText = GameSystems.RollHistory.GetTranslation(51); // takes

            builder.AppendFormat("{0} {1} ~{2}~[ROLL_{3}]!", victimName, takesText, noDamageText, histId);
        }
    }
}