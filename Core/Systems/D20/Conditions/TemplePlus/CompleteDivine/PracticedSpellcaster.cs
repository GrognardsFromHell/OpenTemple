using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Startup.Discovery;
using SpicyTemple.Core.Systems.D20.Classes;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class PracticedSpellcaster
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private static void PracticedSpellcasterLevelMod(in DispatcherCallbackArgs evt, bool divineSpells)
        {
            var dispIo = evt.GetDispIoD20Query();
            var spell_packet = (SpellPacketBody) dispIo.obj;
            if (spell_packet.IsDivine() != divineSpells)
            {
                return;
            }

            var bonVal = 4;
            var cur_caster_lvl = spell_packet.casterLevel;
            var cur_hd = GameSystems.Critter.GetHitDiceNum(evt.objHndCaller);
            bonVal = Math.Min(4, cur_hd - cur_caster_lvl);
            if (bonVal != 0)
            {
                Logger.Info("{0}", "Practiced Spellcaster: Adding to caster level " + bonVal);
                dispIo.return_val += bonVal;
            }
        }

        public static void OnAddSpellCastingArcane(in DispatcherCallbackArgs evt)
        {
            // arg0 holds the class
            if (evt.GetConditionArg1() == 0)
            {
                var highestArcane = D20ClassSystem.GetHighestArcaneCastingClass(evt.objHndCaller);
                evt.SetConditionArg1((int) highestArcane);
            }
        }

        public static void OnAddSpellCastingDivine(in DispatcherCallbackArgs evt)
        {
            // arg0 holds the class
            if (evt.GetConditionArg1() == 0)
            {
                var highestDivine = D20ClassSystem.GetHighestDivineCastingClass(evt.objHndCaller);
                evt.SetConditionArg1((int) highestDivine);
            }
        }

        // args are just-in-case placeholders
        [FeatCondition("Practiced Spellcaster - Arcane")]
        [AutoRegister] public static readonly ConditionSpec ConditionArcane = ConditionSpec
            .Create("Practiced Spellcaster Feat - Arcane", 2)
            .SetUnique()
            .AddHandler(DispatcherType.BaseCasterLevelMod, PracticedSpellcasterLevelMod, false)
            .AddHandler(DispatcherType.ConditionAdd, OnAddSpellCastingArcane)
            .Build();

        // args are just-in-case placeholders
        [FeatCondition("Practiced Spellcaster - Divine")]
        [AutoRegister] public static readonly ConditionSpec ConditionDivine = ConditionSpec
            .Create("Practiced Spellcaster Feat - Divine", 2)
            .SetUnique()
            .AddHandler(DispatcherType.BaseCasterLevelMod, PracticedSpellcasterLevelMod, true)
            .AddHandler(DispatcherType.ConditionAdd, OnAddSpellCastingDivine)
            .Build();
    }
}