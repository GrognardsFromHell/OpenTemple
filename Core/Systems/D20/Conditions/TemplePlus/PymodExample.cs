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
using SpicyTemple.Core.Systems.RadialMenus;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class PymodExample
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        // Crash course!
        // ToEE uses an Event system to handle modifiers.
        //
        // Example: When ToEE calculates a critter's AC, it runs a GetAC event.
        //  To modify the result, we'll use an OnGetAC event handler.
        //
        //  The event handler takes an Event Object as input,
        //  and modifies it to add bonuses to AC.
        //
        //  This event object gets passed around to all the various event handlers,
        //  each in turn adding its own pluses (which can be negative also).
        //
        //  The internal C function will then sum up all the bonuses and get the value.
        //
        // Each event type generally has its own type of Event Object -
        //  each with different properties and methods.
        //  Consult the Temple+ Wiki for full info.
        //
        // Or , just learn by example from here :)
        //
        public static void OnInit(in DispatcherCallbackArgs evt, int arg)
        {
            Logger.Info("{0}", "I've been inited! My attachee: " + evt.objHndCaller);
            // print str(args)
        }

        public static void CreateRadialMenuEntry(in DispatcherCallbackArgs evt)
        {
            // print "My radial menu is being built! Attachee: " + str(attachee)
            // add a parent node labeled "Attempt Succeeds!" (combat.mes line 143) to the Class standard node
            var radialParent = RadialMenuEntry.CreateParent(143); // combat.mes line
            var radialParentId = radialParent.AddAsChild(evt.objHndCaller, RadialMenuStandardNode.Class);
            // add a child action node to the above node, labeled "Guidance" (combat.mes line 5055), that triggers a Cast Spell action
            var radialAction = RadialMenuEntry.CreateAction(5055, D20ActionType.CAST_SPELL, 0, "TAG_INTERFACE_HELP");
            radialAction.AddAsChild(evt.objHndCaller, radialParentId);
        }

        public static void BeginRound(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            // print "Callback for Character's start of round! Note: this gets called in realtime too, every 6 seconds. Useful for Spell tickdowns."
            // print "Num ticked: " + str(dispIo.data1) # dispIo.data1 will contain the number of rounds "ticked"
        }

        public static void EffectTooltip(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoEffectTooltip();
            var spellEnum = 2;
            dispIo.bdb.AddEntry(54, "\nExtra Effect Text", spellEnum); // use this for spells
            dispIo.bdb.AddEntry(54, "Effect Text", -2); // use -2 in the second arg for non-spell effects
            // first arg: indicator type
            // range up to 90 is for buffs (indicators above portrait)
            // then up to 167 are debuffs (below portrait)
            // above are effects "inside" the portrait
            // second arg: spell enum
        }
        // TurnBasedStatusInit is used for initializing the "hourglass" state (Full Round Action, Partial Charge, Standard Action, Move Action, 5' step only) and "surplus" move distance (from when you used a move action and have remaining move), as well as other flags (TBSF_ flags in constants.py)

        public static void TurnBasedStatusInit(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIOTurnBasedStatus();
            dispIo.tbStatus.hourglassState = HourglassState.STD; // sets to Standard Action Only
            dispIo.tbStatus.surplusMoveDistance = 16f;
        }

        public static void GetArmorClass(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            dispIo.bonlist.AddBonus(-10, 0, 180); // value, bonus type, bonus.mes line
        }
        // creates and registers a condition with 0 args; prevents duplicates!

        [AutoRegister] public static readonly ConditionSpec pmEx = ConditionSpec.Create("PyMod Example", 0)
            .AddHandler(DispatcherType.ConditionAdd, OnInit, 1)
            .AddHandler(DispatcherType.RadialMenuEntry, CreateRadialMenuEntry)
            .AddHandler(DispatcherType.BeginRound, BeginRound)
            .AddHandler(DispatcherType.EffectTooltip, EffectTooltip)
            .AddHandler(DispatcherType.TurnBasedStatusInit, TurnBasedStatusInit)
            .AddHandler(DispatcherType.GetAC, GetArmorClass)
            .Build();
    }
}