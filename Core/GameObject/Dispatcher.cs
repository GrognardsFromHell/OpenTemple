using System;
using System.Diagnostics;
using System.Linq;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Conditions;

namespace OpenTemple.Core.GameObject
{
    public class Dispatcher : IDispatcher
    {
        private readonly ILogger Logger = LoggingSystem.CreateLogger();

        // max num of simultaneous Dispatches going on (static int counter inside _DispatcherProcessor)
        private const int DISPATCHER_MAX = 250;

        private static int _dispCounter = 0;

        private readonly SubDispatcherAttachment[][] subDispNodes_ =
            new SubDispatcherAttachment[(int) DispatcherType.Count][];

        private readonly GameObjectBody _owner;

        private ConditionAttachment[] permanentMods = Array.Empty<ConditionAttachment>();

        public ConditionAttachment[] itemConds = Array.Empty<ConditionAttachment>();

        private ConditionAttachment[] conditions = Array.Empty<ConditionAttachment>();

        public Dispatcher(GameObjectBody owner)
        {
            _owner = owner;
        }

        public void Process(DispatcherType type, D20DispatcherKey key, object dispIo)
        {
            if (_dispCounter > DISPATCHER_MAX)
            {
                Logger.Error("Dispatcher maximum recursion reached!");
                return;
            }

            _dispCounter++;

            foreach (var subDispNode in GetSubDispatcher(type))
            {
                if ((subDispNode.subDispDef.dispKey == key ||
                     subDispNode.subDispDef.dispKey == D20DispatcherKey.NONE) && !subDispNode.condNode.IsExpired)
                {
                    DispIoTypeImmunityTrigger dispIoImmunity = DispIoTypeImmunityTrigger.Default;
                    dispIoImmunity.condNode = subDispNode.condNode;

                    if (type != DispatcherType.ImmunityTrigger || key != D20DispatcherKey.IMMUNITY_SPELL)
                    {
                        Process(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPELL, dispIoImmunity);
                    }

                    if (dispIoImmunity.interrupt == 1 && type != DispatcherType.Unused63)
                    {
                        // dispType63 is essentially <. Minor globe of invulnerability
                        dispIoImmunity.interrupt = 0;
                        dispIoImmunity.val2 = 10;
                        Process(DispatcherType.Unused63, D20DispatcherKey.NONE, dispIo);
                        if (dispIoImmunity.interrupt == 0)
                        {
                            var args = new DispatcherCallbackArgs(subDispNode, _owner, type, key, dispIo);
                            subDispNode.subDispDef.callback(in args);
                        }
                    }
                    else
                    {
                        var args = new DispatcherCallbackArgs(subDispNode, _owner, type, key, dispIo);

                        subDispNode.subDispDef.callback(in args);
                    }
                }
            }

            _dispCounter--;
        }


        [TempleDllLocation(0x100e1dd0)]
        public void Attach(ConditionAttachment attachment)
        {
            foreach (var subDispDef in attachment.condStruct.subDispDefs)
            {
                var type = subDispDef.dispType;
                ref var currentList = ref subDispNodes_[(int) type];
                if (currentList == null)
                {
                    currentList = new SubDispatcherAttachment[1];
                }
                else
                {
                    Array.Resize(ref currentList, currentList.Length + 1);
                }

                currentList[^1] = new SubDispatcherAttachment(subDispDef, attachment);
            }
        }

        [TempleDllLocation(0x100e2670)]
        public void AddItemCondition(ConditionSpec condStruct, ReadOnlySpan<int> args)
        {
            var attachment = new ConditionAttachment(condStruct);
            for (int i = 0; i < condStruct.numArgs; i++)
            {
                attachment.args[i] = args[i];
            }

            // Save the attachment
            if (itemConds == null)
            {
                itemConds = new[] {attachment};
            }
            else
            {
                Array.Resize(ref itemConds, itemConds.Length + 1);
                itemConds[^1] = attachment;
            }

            Attach(attachment);

            // This is a bit weird, we're just calling ourselves here, really
            foreach (var subDispatcher in GetSubDispatcher(DispatcherType.ConditionAddFromD20StatusInit))
            {
                if (subDispatcher.subDispDef.dispKey == 0)
                {
                    var condNode = subDispatcher.condNode;
                    if (!condNode.IsExpired && condNode == attachment)
                    {
                        var callbackArgs = new DispatcherCallbackArgs(subDispatcher, _owner,
                            DispatcherType.ConditionAddFromD20StatusInit, 0, null);
                        subDispatcher.subDispDef.callback(in callbackArgs);
                    }
                }
            }
        }

        [TempleDllLocation(0x100e22d0)]
        private bool _ConditionAddDispatch(ref ConditionAttachment[] ppCondNode, ConditionSpec condStruct, int arg1,
            int arg2, int arg3, int arg4)
        {
            Trace.Assert(condStruct.numArgs >= 0 && condStruct.numArgs <= 8);

            int index = 0;
            Span<int> args = stackalloc int[4];
            if (condStruct.numArgs > 0)
            {
                args[index++] = arg1;
            }

            if (condStruct.numArgs > 1)
            {
                args[index++] = arg2;
            }

            if (condStruct.numArgs > 2)
            {
                args[index++] = arg3;
            }

            if (condStruct.numArgs > 3)
            {
                args[index++] = arg4;
            }


            return _ConditionAddDispatchArgs(ref ppCondNode, condStruct, args);
        }


        private bool _ConditionAddDispatchArgs(ref ConditionAttachment[] ppCondNode,
            ConditionSpec condStruct, ReadOnlySpan<int> args)
        {
            // pre-add section (may abort adding condition, or cause another condition to be deleted first)
            var dispIo = DispIoCondStruct.Default;
            dispIo.condStruct = condStruct;
            dispIo.outputFlag = true;
            dispIo.arg1 = 0;
            dispIo.arg2 = 0;
            if (args.Length > 0)
            {
                dispIo.arg1 = args[0];
            }

            if (args.Length > 1)
            {
                dispIo.arg2 = args[1];
            }

            Process(DispatcherType.ConditionAddPre, 0, dispIo);

            if (!dispIo.outputFlag)
            {
                return false;
            }

            // adding condition
            var condNodeNew = new ConditionAttachment(condStruct);
            for (int i = 0; i < condStruct.numArgs; ++i)
            {
                if (i < args.Length)
                {
                    condNodeNew.args[i] = args[i];
                }
                else
                {
                    // Fill the rest with zeros
                    condNodeNew.args[i] = 0;
                }
            }

            // Append to the condition attachment list
            var currentCount = ppCondNode?.Length ?? 0;
            Array.Resize(ref ppCondNode, currentCount + 1);
            ppCondNode[^1] = condNodeNew;

            Attach(condNodeNew);

            foreach (var subDispatcher in GetSubDispatcher(DispatcherType.ConditionAdd))
            {
                if (subDispatcher.subDispDef.dispKey == 0
                    && (subDispatcher.condNode.flags & 1) == 0
                    && condNodeNew == subDispatcher.condNode)
                {
                    var callbackArgs = new DispatcherCallbackArgs(subDispatcher, _owner,
                        DispatcherType.ConditionAdd, 0, null);
                    subDispatcher.subDispDef.callback(in callbackArgs);
                }
            }

            return true;
        }

        [TempleDllLocation(0x100e24c0)]
        public bool _ConditionAddToAttribs_NumArgs0(ConditionSpec condStruct)
        {
            return _ConditionAddDispatch(ref permanentMods, condStruct, 0, 0, 0, 0);
        }

        [TempleDllLocation(0x100e2500)]
        public bool _ConditionAddToAttribs_NumArgs2(ConditionSpec condStruct, int arg1, int arg2)
        {
            return _ConditionAddDispatch(ref permanentMods, condStruct, arg1, arg2, 0, 0);
        }

        [TempleDllLocation(0x100e24e0)]
        public bool _ConditionAdd_NumArgs0(ConditionSpec condStruct)
        {
            return _ConditionAddDispatch(ref conditions, condStruct, 0, 0, 0, 0);
        }

        public bool _ConditionAdd_NumArgs1(ConditionSpec condStruct, int arg1)
        {
            return _ConditionAddDispatch(ref conditions, condStruct, arg1, 0, 0, 0);
        }

        [TempleDllLocation(0x100e2530)]
        public bool _ConditionAdd_NumArgs2(ConditionSpec condStruct, int arg1, int arg2)
        {
            return _ConditionAddDispatch(ref conditions, condStruct, arg1, arg2, 0, 0);
        }

        [TempleDllLocation(0x100e2560)]
        public bool _ConditionAdd_NumArgs3(ConditionSpec condStruct, int arg1, int arg2, int arg3)
        {
            return _ConditionAddDispatch(ref conditions, condStruct, arg1, arg2, arg3, 0);
        }

        [TempleDllLocation(0x100e2590)]
        public bool _ConditionAdd_NumArgs4(ConditionSpec condStruct, int arg1, int arg2, int arg3, int arg4)
        {
            return _ConditionAddDispatch(ref conditions, condStruct, arg1, arg2, arg3, arg4);
        }

        private SubDispatcherAttachment[] GetSubDispatcher(DispatcherType type)
        {
            return subDispNodes_[(int) type] ?? Array.Empty<SubDispatcherAttachment>();
        }

        [TempleDllLocation(0x100e2720)]
        public void ClearPermanentMods()
        {
            _DispatcherClearField(ref permanentMods);
        }

        [TempleDllLocation(0x100e2740)]
        public void ClearItemConditions()
        {
            _DispatcherClearField(ref itemConds);
        }

        [TempleDllLocation(0x100e2760)]
        public void ClearConditions()
        {
            _DispatcherClearField(ref conditions);
        }

        [TempleDllLocation(0x100e2780)]
        public void ClearAll()
        {
            // Detach the global conditions first
            foreach (var globalAttachment in GameSystems.D20.Conditions.GlobalAttachments)
            {
                Detach(globalAttachment);
            }

            ClearPermanentMods();
            ClearItemConditions();
            ClearConditions();
        }

        [TempleDllLocation(0x100e2400)]
        private void _DispatcherClearField(ref ConditionAttachment[] dispCondList)
        {
            if (dispCondList.Length == 0)
            {
                dispCondList = Array.Empty<ConditionAttachment>();
                return;
            }

            var attachmentsToFree = dispCondList;
            dispCondList = Array.Empty<ConditionAttachment>();

            foreach (var attachment in attachmentsToFree)
            {
                foreach (var subDispatcher in GetSubDispatcher(DispatcherType.ConditionRemove))
                {
                    var sdd = subDispatcher.subDispDef;
                    if (sdd.dispKey == 0 && (subDispatcher.condNode.flags & 1) == 0
                                         && subDispatcher.condNode == attachment)
                    {
                        var callbackArgs = new DispatcherCallbackArgs(subDispatcher, _owner,
                            DispatcherType.ConditionRemove, 0, null);
                        sdd.callback(in callbackArgs);
                    }
                }

                Detach(attachment);
            }
        }

        [TempleDllLocation(0x100e2240)]
        public void DispatchRemoveCondition(ConditionAttachment attachment)
        {
            // Call first pass remove
            var removeSubdispatcher = GetSubDispatcher(DispatcherType.ConditionRemove);
            for (var i = 0; i < removeSubdispatcher.Length; i++)
            {
                ref var subdispatcher = ref removeSubdispatcher[i];
                if (subdispatcher.subDispDef.dispKey == 0)
                {
                    var condNode = subdispatcher.condNode;
                    if (!condNode.IsExpired && condNode == attachment)
                    {
                        var callbackArgs = new DispatcherCallbackArgs(
                            subdispatcher,
                            _owner,
                            DispatcherType.ConditionRemove,
                            0,
                            null
                        );
                        subdispatcher.subDispDef.callback(in callbackArgs);
                    }
                }
            }

            // Call second pass remove
            var removeSubdispatcher2 = GetSubDispatcher(DispatcherType.ConditionRemove2);
            for (var i = 0; i < removeSubdispatcher2.Length; i++)
            {
                ref var subdispatcher = ref removeSubdispatcher2[i];
                if (subdispatcher.subDispDef.dispKey == 0)
                {
                    var condNode = subdispatcher.condNode;
                    if (!condNode.IsExpired && condNode == attachment)
                    {
                        var callbackArgs = new DispatcherCallbackArgs(
                            subdispatcher,
                            _owner,
                            DispatcherType.ConditionRemove2,
                            0,
                            null
                        );
                        subdispatcher.subDispDef.callback(in callbackArgs);
                    }
                }
            }

            attachment.IsExpired = true;
        }

        [TempleDllLocation(0x100e1e30)]
        private void Detach(ConditionAttachment attachment)
        {
            for (var i = 0; i < subDispNodes_.Length; i++)
            {
                ref var subAttachment = ref subDispNodes_[i];
                if (subAttachment == null)
                {
                    continue; // Nothing to remove
                }

                // Count how many we're going to remove
                var remaining = subAttachment.Count(n => n.condNode != attachment);
                if (remaining == 0)
                {
                    subAttachment = null;
                    continue;
                }

                var idx = 0;
                var newList = new SubDispatcherAttachment[remaining];
                foreach (var subDispNode in subAttachment)
                {
                    if (subDispNode.condNode != attachment)
                    {
                        newList[idx++] = subDispNode;
                    }
                }

                subAttachment = newList;
            }
        }

        [TempleDllLocation(0x100e1e80)]
        public void RemoveExpiredConditions()
        {
            if (conditions == null || conditions.Length == 0)
            {
                return;
            }

            var conditionCount = conditions.Length;
            for (var i = 0; i < conditionCount; i++)
            {
                if (conditions[i].IsExpired)
                {
                    Detach(conditions[i]);

                    // Move everything else forward
                    for (int j = i + 1; j < conditionCount; j++)
                    {
                        conditions[j - 1] = conditions[j];
                    }

                    i--;
                    conditionCount--;
                }
            }

            if (conditionCount != conditions.Length)
            {
                Array.Resize(ref conditions, conditionCount);
            }
        }

        [TempleDllLocation(0x100e25c0)]
        public void AddCondFromInternalFields(ConditionSpec condition, ReadOnlySpan<int> args)
        {
            var attachment = new ConditionAttachment(condition);
            attachment.args = new object[condition.numArgs];
            for (var i = 0; i < attachment.args.Length; i++)
            {
                attachment.args[i] = args[i];
            }
            Attach(attachment);

            // Call the init for the newly added condition (we could also just check the cond struct to be honest)
            var currentList = subDispNodes_[(int) DispatcherType.ConditionAddFromD20StatusInit];
            if (currentList != null)
            {
                foreach (var subdispatcher in currentList)
                {
                    if (!subdispatcher.condNode.IsExpired && subdispatcher.condNode.condStruct == condition)
                    {
                        var callbackArgs = new DispatcherCallbackArgs(
                            subdispatcher,
                            _owner,
                            DispatcherType.ConditionRemove2,
                            0,
                            null
                        );
                        subdispatcher.subDispDef.callback(callbackArgs);
                    }
                }
            }
        }

        [TempleDllLocation(0x100e1b90)]
        public void SetPermanentModArgsFromDataFields(ConditionSpec condStructIn, ReadOnlySpan<int> condArgs)
        {
            // Find and set the arguments
            foreach (var attachment in permanentMods)
            {
                if (attachment.condStruct == condStructIn && !attachment.ArgsFromField)
                {
                    var condStruct = attachment.condStruct;
                    if (condStruct.Uniqueness == UniquenessType.UniqueArg1
                        // TODO: These two should just be defined as being unique with data1
                        || condStruct == FeatConditions.SpellFocus
                        || condStruct == FeatConditions.GreaterSpellFocus)
                    {
                        if (condStruct.numArgs != 0 && (int) attachment.args[0] != condArgs[0])
                        {
                            continue;
                        }
                    }

                    for (int i = 0; i < condStruct.numArgs; i++)
                    {
                        if (condArgs[i] != 0xDEADBEEF)
                        {
                            attachment.args[i] = condArgs[i];
                        }
                    }

                    attachment.ArgsFromField = true;
                    return;
                }
            }

            foreach (var attachment in itemConds)
            {
                var condStruct = attachment.condStruct;
                if (condStruct == condStructIn && !attachment.ArgsFromField)
                {
                    var arg3 = condStruct.numArgs > 2 ? attachment.args[2] : 0;
                    if ((int) arg3 == condArgs[2])
                    {
                        for (int i = 0; i < condStruct.numArgs; i++)
                        {
                            if (condArgs[i] != 0xDEADBEEF)
                            {
                                attachment.args[i] = condArgs[i];
                            }
                        }

                        return;
                    }
                }
            }

            Logger.Info("modifier changed for {0} ({1}) (proto change?)", condStructIn.condName, _owner);
        }

        [TempleDllLocation(0x100e1cc0)]
        public void DispatcherCondsResetFlag2()
        {
            foreach (var attachment in permanentMods)
            {
                attachment.ArgsFromField = false;
            }
            foreach (var attachment in itemConds)
            {
                attachment.ArgsFromField = false;
            }
        }

    }
}