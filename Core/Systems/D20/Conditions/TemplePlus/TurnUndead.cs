using OpenTemple.Core.GameObject;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.Script.Extensions;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public static class TurnUndead
    {

        // Called when a turn undead is performed:  The data is the turn undead type
        public static void TurnUndeadPerform(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            if (dispIo.data1 == 7)
            {
                // Greater Turning:  Also deduct a turn undead charge
                var NewCharges = evt.GetConditionArg2() - 1;
                if (NewCharges > 0)
                {
                    evt.SetConditionArg2(NewCharges);
                }
                else
                {
                    evt.SetConditionArg2(0);
                }
            }
            else if (dispIo.data1 == 0)
            {
                // Standard Turn Undead
                var charges = evt.GetConditionArg2();
                if (charges == 0)
                {
                    evt.objHndCaller.D20SendSignal("Deduct Greater Turn Undead Charge"); // Zero out greater turning
                }
            }
        }

        // Gets the number of turn undead charges
        public static void GetTurnUndeadCharges(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.resultData = (ulong) evt.GetConditionArg2();
        }

        // Called to deduct a turn undead charge
        public static void DeductTurnUndeadCharge(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            // Deduct one turn undead charge
            var NewCharges = evt.GetConditionArg2() - 1;
            if (NewCharges > 0)
            {
                evt.SetConditionArg2(NewCharges);
            }
            else
            {
                evt.SetConditionArg2(0);
                evt.objHndCaller.D20SendSignal("Deduct Greater Turn Undead Charge"); // Zero out greater turning
            }
        }

        // Note:  Arg 0 = turn undead type, Arg 1 =  is the number of chrages
        [AutoRegister]
        public static readonly ConditionSpec TurnUndeadExtension = ConditionSpec.Extend(DomainConditions.TurnUndead)
            .AddSignalHandler("Turn Undead Perform", TurnUndeadPerform)
            .AddSignalHandler("Deduct Turn Undead Charge", DeductTurnUndeadCharge)
            .AddQueryHandler("Turn Undead Charges", GetTurnUndeadCharges)
            .Build();

        public static int GetTurnUndeadCharges(this GameObjectBody critter)
        {
            return (int) GameSystems.D20.D20QueryReturnData(critter, "Turn Undead Charges");
        }
    }
}