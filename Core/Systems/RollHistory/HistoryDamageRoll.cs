using System.Text;
using OpenTemple.Core.Systems.D20;

namespace OpenTemple.Core.Systems.RollHistory
{
    // Damage dealt, former type 1
    public class HistoryDamageRoll : HistoryEntry
    {
        public DamagePacket DamagePacket { get; }

        public HistoryDamageRoll(DamagePacket damagePacket)
        {
            DamagePacket = damagePacket;
        }

        public override string Title => GameSystems.RollHistory.GetTranslation(58); // Damage Roll

        [TempleDllLocation(0x10048470)]
        public override void FormatShort(StringBuilder builder)
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

        [TempleDllLocation(0x1019bbb0)]
        public override void FormatLong(StringBuilder builder)
        {
            if (obj != null)
            {
                builder.Append(GameSystems.MapObject.GetDisplayNameForParty(obj));
                builder.Append(' ');
                builder.Append(GameSystems.RollHistory.GetTranslation(15)); // hits
                builder.Append(' ');
                builder.Append(GameSystems.MapObject.GetDisplayNameForParty(obj2));
                builder.Append("...");
            }
            else
            {
                builder.Append(GameSystems.MapObject.GetDisplayNameForParty(obj2));
                builder.Append(' ');
                builder.Append(GameSystems.RollHistory.GetTranslation(31)); // Damaged...
            }

            builder.Append("\n\n\n");

            if (DamagePacket.description != null)
            {
                builder.Append(DamagePacket.description);
                builder.Append(" x ");
                builder.Append(DamagePacket.critHitMultiplier);
            }
            else
            {
                builder.Append("\n\n");
            }

            builder.Append(GameSystems.RollHistory.GetTranslation(16)); // Damage
            builder.Append("\n");

            for (var i = 0; i < DamagePacket.FormattedLineCount; i++)
            {
                DamagePacket.FormatLine(i, out var dice, out var value, out var damageType, out var text,
                    out var suffix);
                if (text == null)
                {
                    continue;
                }

                if (dice.Count <= 0)
                {
                    if (value > 0)
                    {
                        builder.Append('+');
                        builder.Append(value);
                    }
                    else if (value < 0)
                    {
                        builder.Append(value);
                    }
                    else
                    {
                        builder.Append("--");
                    }
                }
                else
                {
                    if (value == 0)
                    {
                        builder.Append("--");
                    }
                    else
                    {
                        dice.Format(builder, false);
                        builder.Append('=');
                        builder.Append(value);
                    }
                }

                builder.Append("@t");
                builder.Append(text);

                if (damageType != DamageType.Unspecified)
                {
                    builder.Append(" (");
                    builder.Append(GameSystems.D20.Damage.GetDamageTypeName(damageType));
                    builder.Append(")");
                }

                if (suffix != null)
                {
                    builder.Append(suffix);
                }

                builder.Append("\n");
            }

            builder.Append("\n\n\n");
            builder.Append(DamagePacket.GetOverallDamageByType());
            builder.Append("   ");
            builder.Append(GameSystems.RollHistory.GetTranslation(2)); // Total
            builder.Append("\n");
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