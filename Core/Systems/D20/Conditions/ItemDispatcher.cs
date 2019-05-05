using System;
using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Systems.D20.Conditions
{
    /// <summary>
    /// Allows dispatching directly to conditions for conditions that are attached to items and thus have no
    /// dispatcher.
    /// </summary>
    public static class ItemDispatcher
    {
        public static void DispatcherProcessorForItems(ConditionSpec condStruct, Span<int> condArgs,
            DispatcherType dispType, D20DispatcherKey key, object dispIo)
        {
            var condAttachment = new ConditionAttachment(condStruct);
            for (int i = 0; i < condStruct.numArgs; i++)
            {
                condAttachment.args[i] = condArgs[i];
            }

            for (int i = 0; i < condStruct.subDispDefs.Length; i++)
            {
                ref readonly var sdd = ref condStruct.subDispDefs[i];
                if (sdd.dispKey == key || sdd.dispKey == D20DispatcherKey.NONE)
                {
                    var attachment = new SubDispatcherAttachment();
                    attachment.subDispDef = sdd;
                    attachment.condNode = condAttachment;
                    sdd.callback(in attachment, null, dispType, key, dispIo);
                }
            }
        }
    }
}