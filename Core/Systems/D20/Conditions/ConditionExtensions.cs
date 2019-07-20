using System;
using System.Diagnostics;
using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Systems.D20.Conditions
{
    /// <summary>
    /// Convenience extensions for working with conditions on objects.
    /// </summary>
    public static class ConditionExtensions
    {
        public static bool AddCondition(this GameObjectBody obj, string conditionName, params int[] args)
        {
            var conditionSpec = GameSystems.D20.Conditions[conditionName];
            if (conditionSpec == null)
            {
                throw new ArgumentException("Unknown condition: '" + conditionName + "'");
            }

            return AddCondition(obj, conditionSpec, args);
        }

        [TempleDllLocation(0x1004ca60)]
        [TempleDllLocation(0x1004caa0)]
        [TempleDllLocation(0x1004cae0)]
        [TempleDllLocation(0x1004cb30)]
        public static bool AddCondition(this GameObjectBody obj, ConditionSpec conditionSpec, params int[] args)
        {
            Trace.Assert(obj.IsCritter());
            Trace.Assert(args.Length == conditionSpec.numArgs);

            if (!(obj.GetDispatcher() is Dispatcher dispatcher))
            {
                return false;
            }

            switch (args.Length)
            {
                case 0:
                    return dispatcher._ConditionAdd_NumArgs0(conditionSpec);
                case 1:
                    return dispatcher._ConditionAdd_NumArgs1(conditionSpec, args[0]);
                case 2:
                    return dispatcher._ConditionAdd_NumArgs2(conditionSpec, args[0], args[1]);
                case 3:
                    return dispatcher._ConditionAdd_NumArgs3(conditionSpec, args[0], args[1], args[2]);
                case 4:
                    return dispatcher._ConditionAdd_NumArgs4(conditionSpec, args[0], args[1], args[2], args[3]);
                default:
                    throw new ArgumentOutOfRangeException(nameof(args));
            }
        }

        public static bool HasCondition(this GameObjectBody obj, string conditionName)
        {
            var conditionSpec = GameSystems.D20.Conditions[conditionName];
            if (conditionSpec == null)
            {
                throw new ArgumentException("Unknown condition: '" + conditionName + "'");
            }

            return HasCondition(obj, conditionSpec);
        }

        public static bool HasCondition(this GameObjectBody obj, ConditionSpec condition)
        {
            var dispatcher = obj.GetDispatcher();
            if (dispatcher == null)
            {
                return false; // Can't have conditions without a dispatcher
            }

            var dispIo = DispIoD20Query.Default;
            dispIo.obj = condition;
            dispatcher.Process(DispatcherType.D20Query, D20DispatcherKey.QUE_Critter_Has_Condition, dispIo);
            return dispIo.return_val != 0;
        }

    }
}