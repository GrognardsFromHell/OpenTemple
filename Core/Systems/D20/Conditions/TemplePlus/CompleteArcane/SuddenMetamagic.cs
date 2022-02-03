using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems.RadialMenus;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    /// <summary>
    /// Shared methods for the sudden metamagic feats.
    /// </summary>
    internal static class SuddenMetamagic
    {

        private static void RadialEntryToggle(in DispatcherCallbackArgs evt, string name)
        {
            // Add a checkbox to turn on and off the feat if there is a charge available, otherwise just don't show it
            if (evt.GetConditionArg1() > 0)
            {
                var checkbox = evt.CreateToggleForArg(1);
                checkbox.text = name;
                checkbox.helpSystemHashkey = "TAG_INTERFACE_HELP";
                checkbox.AddAsChild(evt.objHndCaller, RadialMenuStandardNode.Feats);
            }
        }

        internal delegate void ModifyMetamagicCallback(ref MetaMagicData metaMagicData);

        private static void ModifyMetamagic(in DispatcherCallbackArgs evt, ModifyMetamagicCallback callback)
        {
            var dispIo = evt.GetEvtObjMetaMagic();
            // Check for a charge
            var charges = evt.GetConditionArg1();
            if (charges < 1)
            {
                return;
            }

            // Check if feat is turned on
            if (evt.GetConditionArg2() != 0)
            {
                var metaMagicData = dispIo.mmData;
                callback(ref metaMagicData);
                dispIo.mmData = metaMagicData;
            }
        }

        private static void NewDay(in DispatcherCallbackArgs evt)
        {
            // One charge per day
            evt.SetConditionArg1(1);
            // Set the checkbox to off at the begining of the day
            evt.SetConditionArg2(0);
        }

        private static void DeductCharge(in DispatcherCallbackArgs evt)
        {
            // Check for a charge and the enable flag
            var charges = evt.GetConditionArg1();
            if (charges < 1 || evt.GetConditionArg2() == 0)
            {
                return;
            }

            // Decrement the charges
            charges = charges - 1;
            evt.SetConditionArg1(charges);
        }

        public static ConditionSpec.Builder Create(string conditionId, string displayName, ModifyMetamagicCallback callback)
        {
            // Charges, Toggeled On, Spare, Spare
            return ConditionSpec.Create(conditionId, 4)
                .SetUnique()
                .AddHandler(DispatcherType.RadialMenuEntry, RadialEntryToggle, displayName)
                .AddHandler(DispatcherType.ConditionAdd, NewDay)
                .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, NewDay)
                .AddHandler(DispatcherType.MetaMagicMod, ModifyMetamagic, callback)
                .AddSignalHandler("Sudden Metamagic Deduct Charge", DeductCharge);

        }

    }
}