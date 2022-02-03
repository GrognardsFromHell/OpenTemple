
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
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{

    public class RapidMetamagic
    {
        public static void RapidMMActionCostMod(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjActionCost();
            if (dispIo.d20a.d20ActType != D20ActionType.CAST_SPELL)
            {
                return;
            }

            if (dispIo.acpOrig.hourglassCost <= ActionCostType.Standard) // original is already less than full round
            {
                return;
            }

            if (dispIo.acpCur.hourglassCost <= 0) // adjusted amount is already free action
            {
                return;
            }

            // check if the original spell is standard action or less - if so reduce action cost to standard action
            var spData = dispIo.d20a.d20SpellData;
            var spEntry = GameSystems.Spell.GetSpellEntry(spData.SpellEnum);
            if (spEntry.spellEnum == 0)
            {
                return;
            }

            var castingTimeType = spEntry.castingTimeType;
            var mmData = spData.metaMagicData;
            if (mmData.IsQuicken && (dispIo.tbStat.tbsFlags & TurnBasedStatusFlags.FreeActionSpellPerformed) == 0)
            {
                dispIo.acpCur.hourglassCost = 0;
                dispIo.tbStat.tbsFlags |= TurnBasedStatusFlags.FreeActionSpellPerformed;
                // print "reducing cost to 0"
                return;
            }
        }

        // args are just-in-case placeholders
        [FeatCondition("Rapid Metamagic")]
        [AutoRegister] public static readonly ConditionSpec Condition = ConditionSpec.Create("Rapid Metamagic Feat", 2)
            .SetUnique()
            .AddHandler(DispatcherType.ActionCostMod, RapidMMActionCostMod)
            .Build();
    }
}
