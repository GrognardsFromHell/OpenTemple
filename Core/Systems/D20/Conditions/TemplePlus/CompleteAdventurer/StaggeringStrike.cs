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

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus;

// Complete Adventurer: p. 112
public class StaggeringStrike
{
    public static void StaggeringStrikeFeatOnSneakAttack(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20Signal();
        evt.SetConditionArg1(1); // Set the flag to potentially apply the effect
    }

    public static void StaggeringStrikeFeatOnDamage(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoDamage();
        // Do nothing if sneak attack was not applied
        if (evt.GetConditionArg1() == 0)
        {
            return;
        }

        var tgt = dispIo.attackPacket.victim;
        if (tgt == null)
        {
            return;
        }

        var damage = dispIo.damage.finalDamage;

        var targetName = GameSystems.MapObject.GetDisplayNameForParty(tgt);
        GameSystems.RollHistory.CreateFromFreeText(targetName + " staggering strike...\n\n");
        // Fortitude save to avoid the effect
        if (tgt.SavingThrow(damage, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, evt.objHndCaller))
        {
            GameSystems.RollHistory.CreateFromFreeText("target resisted.\n\n");
        }
        else
        {
            evt.objHndCaller.FloatLine("Staggering strike!");
            GameSystems.RollHistory.CreateFromFreeText("target Staggered!\n\n");
            tgt.AddCondition("Staggering Strike Effect", 1);
        }

        return;
    }

    public static void StaggeringStrikeEffectBeginRound(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20Signal();
        var duration = evt.GetConditionArg1();
        // If zero rounds remaining remove the effect (to avoid it lasting forever outside of combat)
        if (duration < 1)
        {
            evt.RemoveThisCondition();
        }

        // Decrement the duration
        duration = duration - 1;
        evt.SetConditionArg1(duration);
        return;
    }

    public static void StaggeringStrikeEffectTurnBasedStatusInit(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIOTurnBasedStatus();
        // Remove the target's move action
        dispIo.tbStatus.hourglassState = HourglassState.STD; // sets to Standard Action Only
        // Duration is decremented in On Begin Round Just check it here
        var duration = evt.GetConditionArg1();
        if (duration < 1)
        {
            evt.RemoveThisCondition();
        }
    }

    public static void StaggeringStrikeEffectOnHealing(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoDamage();
        // Remove the effect if there is healing
        evt.RemoveThisCondition();
        return;
    }

    public static void StaggeringStrikeEffectGetTooltip(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoTooltip();
        // not active, do nothing
        if (evt.GetConditionArg1() == 0)
        {
            return;
        }

        // Set the tooltip
        dispIo.Append("Staggered");
        return;
    }

    public static void StaggeringStrikeEffectGetEffectTooltip(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoEffectTooltip();
        // not active, do nothing
        if (evt.GetConditionArg1() == 0)
        {
            return;
        }

        // Set the tooltip
        dispIo.bdb.AddEntry(ElfHash.Hash("STAGGERING_STRIKE"), "", -2);
    }
    // Setup the feat
    // Apply Effect, Extra

    [AutoRegister, FeatCondition("Staggering Strike")]
    public static readonly ConditionSpec StaggeringStrikeFeat = ConditionSpec.Create("Staggering Strike Feat", 2)
        .SetUnique()
        .AddSignalHandler("Sneak Attack Damage Applied", StaggeringStrikeFeatOnSneakAttack)
        .AddHandler(DispatcherType.DealingDamage2, StaggeringStrikeFeatOnDamage)
        .Build();

    // Rounds, Extra
    [AutoRegister]
    public static readonly ConditionSpec StaggeringStrikeEffect = ConditionSpec
        .Create("Staggering Strike Effect", 2)
        .SetUnique()
        .AddHandler(DispatcherType.TurnBasedStatusInit, StaggeringStrikeEffectTurnBasedStatusInit)
        .AddHandler(DispatcherType.BeginRound, StaggeringStrikeEffectBeginRound)
        .AddHandler(DispatcherType.ReceiveHealing, StaggeringStrikeEffectOnHealing)
        .AddHandler(DispatcherType.Tooltip, StaggeringStrikeEffectGetTooltip)
        .AddHandler(DispatcherType.EffectTooltip, StaggeringStrikeEffectGetEffectTooltip)
        .Build();
}