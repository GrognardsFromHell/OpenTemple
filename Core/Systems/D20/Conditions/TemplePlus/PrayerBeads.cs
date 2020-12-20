using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.RadialMenus;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class PrayerBeads
    {
        public static void PrBeadsNewday(in DispatcherCallbackArgs evt)
        {
            evt.SetConditionArg1(0);
            evt.SetConditionArg2(0);
            evt.SetConditionArg4(0);
        }

        public static void PrBeadsRadial(in DispatcherCallbackArgs evt)
        {
            var invIdx = evt.GetConditionArg3();
            var radialAction = RadialMenuEntry.CreateAction("Prayer Beads (Karma)", D20ActionType.ACTIVATE_DEVICE_FREE,
                invIdx, "TAG_INTERFACE_HELP");
            radialAction.AddAsChild(evt.objHndCaller, RadialMenuStandardNode.Items);
        }

        public static void PrBeadsPerform(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var invIdx = evt.GetConditionArg3();
            if (invIdx == dispIo.action.data1 /*D20Action*/)
            {
                var numUsedToday = evt.GetConditionArg4();
                evt.SetConditionArg4(numUsedToday + 1);
                evt.objHndCaller.AddCondition("Prayer Beads Karma Effect", 0, 0);
            }
        }

        public static void PrBeadsCheck(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var invIdx = evt.GetConditionArg3();
            if (invIdx == dispIo.action.data1 /*D20Action*/)
            {
                var numUsedToday = evt.GetConditionArg4();
                if (numUsedToday > 0)
                {
                    dispIo.returnVal = ActionErrorCode.AEC_OUT_OF_CHARGES;
                }
            }
        }

        // arg2 is the inventory index (automatically set by the game), arg3 is times used this day, arg4 is reserved
        [AutoRegister] public static readonly ConditionSpec prbd = ConditionSpec.Create("Prayer Beads", 5)
            .SetUnique()
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, PrBeadsNewday)
            .AddHandler(DispatcherType.RadialMenuEntry, PrBeadsRadial)
            .AddHandler(DispatcherType.D20ActionCheck, D20DispatcherKey.D20A_ACTIVATE_DEVICE_FREE, PrBeadsCheck)
            .AddHandler(DispatcherType.D20ActionPerform, D20DispatcherKey.D20A_ACTIVATE_DEVICE_FREE, PrBeadsPerform)
            .AddItemForceRemoveHandler()
            .Build();

        public static void PrBeadsAdded(in DispatcherCallbackArgs evt)
        {
            evt.SetConditionArg3(100);
        }

        public static void PrBeadsCasterLevelBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            var spellPkt = (SpellPacketBody) dispIo.obj;
            if (spellPkt.IsDivine())
            {
                dispIo.return_val += 4;
            }
        }

        public static void PrBeadsTickdown(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            var numRounds = evt.GetConditionArg3();
            var roundsToReduce = dispIo.data1;
            if (numRounds - roundsToReduce >= 0)
            {
                evt.SetConditionArg3(numRounds - roundsToReduce);
            }
            else
            {
                evt.RemoveThisCondition();
            }
        }

        public static void PrayerEffectTooltip(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoEffectTooltip();
            dispIo.bdb.AddEntry(52, "Karmic Prayer (" + evt.GetConditionArg3().ToString() + " rounds)", -2);
        }

        [AutoRegister] public static readonly ConditionSpec prayerKarma = ConditionSpec.Create("Prayer Beads Karma Effect", 3)
            .SetUnique()
            .AddHandler(DispatcherType.BaseCasterLevelMod, PrBeadsCasterLevelBonus)
            .AddHandler(DispatcherType.BeginRound, PrBeadsTickdown)
            .AddHandler(DispatcherType.ConditionAdd, PrBeadsAdded)
            .AddHandler(DispatcherType.EffectTooltip, PrayerEffectTooltip)
            .Build();
    }
}