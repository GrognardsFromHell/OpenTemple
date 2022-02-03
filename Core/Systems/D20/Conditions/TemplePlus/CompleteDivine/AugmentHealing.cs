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
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    // Augment Healing, Complete Divine: p. 79
    public class AugmentHealing
    {
        private static void QueryHealingBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            // Note:  This implementation effects only damage healed by effected spells.  Damage done to undead is not increased.
            // Also empower will apply to this bonus.  This seems to be the best interpretation of the feat.
            var healingBonus = 0;
            // Argument is the spell id, 0 indicates non spell healing
            if (dispIo.data1 > 0)
            {
                var spPacket = GameSystems.Spell.GetActiveSpell(dispIo.data1);
                if (spPacket.spellEnum > 0)
                {
                    var spEntry = GameSystems.Spell.GetSpellEntry(spPacket.spellEnum);
                    // Is it a conjuration(healing) spell
                    if (spEntry.spellSchoolEnum == SchoolOfMagic.Conjuration &&
                        spEntry.spellSubSchoolEnum == SubschoolOfMagic.Healing)
                    {
                        // Bonus is twice the spell level
                        healingBonus = 2 * spPacket.spellKnownSlotLevel;
                    }
                }
            }

            // Return the bonus
            dispIo.return_val += healingBonus;
        }

        // Extra, Extra
        [AutoRegister, FeatCondition("Augment Healing")]
        public static readonly ConditionSpec Condition = ConditionSpec.Create("Augment Healing", 2)
            .SetUnique()
            .AddQueryHandler("Healing Bonus", QueryHealingBonus)
            .Build();
    }
}