using System;
using System.Diagnostics;
using System.Linq;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Conditions;

namespace SpicyTemple.Core.GameObject
{
    public class Dispatcher : IDispatcher
    {
        private readonly ILogger Logger = new ConsoleLogger();

        private const int
            DISPATCHER_MAX =
                250; // max num of simultaneous Dispatches going on (static int counter inside _DispatcherProcessor)

        private static int _dispCounter = 0;

        private readonly SubDispatcherAttachment[][] subDispNodes_ =
            new SubDispatcherAttachment[(int) DispatcherType.Count][];

        private readonly GameObjectBody _owner;

        private ConditionAttachment[] permanentMods;

        private ConditionAttachment[] itemConds;

        private ConditionAttachment[] conditions;

        public Dispatcher(GameObjectBody owner)
        {
            _owner = owner;
        }

        public void Process(DispatcherType type, D20DispatcherKey key, object args)
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
                        Process(type, D20DispatcherKey.NONE, args);
                        if (dispIoImmunity.interrupt == 0)
                        {
                            subDispNode.subDispDef.callback(subDispNode, _owner, type, key, args);
                        }
                    }
                    else
                    {
                        subDispNode.subDispDef.callback(subDispNode, _owner, type, key, args);
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
                attachment.args[i] = args[0];
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
                        subDispatcher.subDispDef.callback(subDispatcher, _owner,
                            DispatcherType.ConditionAddFromD20StatusInit, 0, null);
                    }
                }
            }
        }

        private int _ConditionAddDispatch(ref ConditionAttachment[] ppCondNode, ConditionSpec condStruct, int arg1,
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

        private int _ConditionAddDispatchArgs(ref ConditionAttachment[] ppCondNode,
            ConditionSpec condStruct, ReadOnlySpan<int> args)
        {
            Trace.Assert(condStruct.numArgs >= args.Length);

            // pre-add section (may abort adding condition, or cause another condition to be deleted first)
            var dispIO14h = DispIoCondStruct.Default;
            dispIO14h.condStruct = condStruct;
            dispIO14h.outputFlag = 1;
            dispIO14h.arg1 = 0;
            dispIO14h.arg2 = 0;
            if (args.Length > 0)
            {
                dispIO14h.arg1 = args[0];
            }

            if (args.Length > 1)
            {
                dispIO14h.arg2 = args[1];
            }

            Process(DispatcherType.ConditionAddPre, 0, dispIO14h);

            if (dispIO14h.outputFlag == 0)
            {
                return 0;
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
                    subDispatcher.subDispDef.callback(subDispatcher, _owner,
                        DispatcherType.ConditionAdd, 0, null);
                }
            }

            return 1;
        }

        public int _ConditionAddToAttribs_NumArgs0(ConditionSpec condStruct)
        {
            return _ConditionAddDispatch(ref permanentMods, condStruct, 0, 0, 0, 0);
        }

        public int _ConditionAddToAttribs_NumArgs2(ConditionSpec condStruct, int arg1, int arg2)
        {
            return _ConditionAddDispatch(ref permanentMods, condStruct, arg1, arg2, 0, 0);
        }

        public int _ConditionAdd_NumArgs0(ConditionSpec condStruct)
        {
            return _ConditionAddDispatch(ref conditions, condStruct, 0, 0, 0, 0);
        }

        public int _ConditionAdd_NumArgs1(ConditionSpec condStruct, int arg1)
        {
            return _ConditionAddDispatch(ref conditions, condStruct, arg1, 0, 0, 0);
        }

        public int _ConditionAdd_NumArgs2(ConditionSpec condStruct, int arg1, int arg2)
        {
            return _ConditionAddDispatch(ref conditions, condStruct, arg1, arg2, 0, 0);
        }

        public int _ConditionAdd_NumArgs3(ConditionSpec condStruct, int arg1, int arg2, int arg3)
        {
            return _ConditionAddDispatch(ref conditions, condStruct, arg1, arg2, arg3, 0);
        }

        public int _ConditionAdd_NumArgs4(ConditionSpec condStruct, int arg1, int arg2, int arg3, int arg4)
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
            _DispatcherClearField(ref itemConds);
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
            if (dispCondList == null || dispCondList.Length == 0)
            {
                dispCondList = null;
                return;
            }

            var attachmentsToFree = dispCondList;
            dispCondList = null;

            foreach (var attachment in attachmentsToFree)
            {
                foreach (var subDispatcher in GetSubDispatcher(DispatcherType.ConditionRemove))
                {
                    var sdd = subDispatcher.subDispDef;
                    if (sdd.dispKey == 0 && (subDispatcher.condNode.flags & 1) == 0
                                         && subDispatcher.condNode == attachment)
                    {
                        sdd.callback(subDispatcher, _owner, DispatcherType.ConditionRemove, 0, null);
                    }
                }

                Detach(attachment);
            }
        }

        [TempleDllLocation(0x100e1e30)]
        public void Detach(ConditionAttachment attachment)
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
    }
}