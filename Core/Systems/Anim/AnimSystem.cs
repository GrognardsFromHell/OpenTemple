using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Microsoft.VisualBasic.ApplicationServices;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.Pathfinding;
using SpicyTemple.Core.Systems.TimeEvents;

namespace SpicyTemple.Core.Systems.Anim
{
    public class AnimSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private const int ANIM_RUN_SLOT_CAP = 512;

        /*
            Set to true when ToEE cannot allocate an animation slot. This causes
            the anim system to try and interrupt as many animations as possible.
        */
        [TempleDllLocation(0x10AA4BB0)]
        private bool mAllSlotsUsed;

        // Fixed size array of 512 slots
        [TempleDllLocation(0x118CE520)]
        private List<AnimSlot> mSlots = new List<AnimSlot>();

        [TempleDllLocation(0x102AC880)]
        private AnimSlotId animIdGlobal;

        [TempleDllLocation(0x10AA4BB8)]
        private int animSysIsLoading;

        // The last slot that a goal was pushed to
        private AnimSlotId lastSlotPushedTo_;

        /*
            While processing the timer event for a slot, this will contain the slots index.
            Otherwise -1.
        */
        [TempleDllLocation(0x102B2654)]
        private int mCurrentlyProcessingSlotIdx;

        private List<AnimActionCallback> mActionCallbacks = new List<AnimActionCallback>();

        private Action mAllGoalsClearedCallback;

        [TempleDllLocation(0x10AA4BC0)]
        private int mActiveGoalCount;

        [TempleDllLocation(0x10AA4BBC)]
        private int slotsInUse;

        [TempleDllLocation(0x11E61520)]
        private int nextUniqueId;

        [TempleDllLocation(0x10307534)]
        public int customDelayInMs { get; set; }

        public AnimationGoals Goals { get; } = new AnimationGoals();

        [TempleDllLocation(0x10016bb0)]
        public AnimSystem()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1000c110)]
        public void Dispose()
        {
        }

        [TempleDllLocation(0x10054e10)]
        public bool IsProcessing
        {
            get
            {
                return false; // TODO
            }
        }

        public bool PushGoal(AnimSlotGoalStackEntry stackEntry, out AnimSlotId slotId)
        {
            return PushGoalInternal(stackEntry, out slotId, 0);
        }

        // TODO: Enumerable Generator
        [TempleDllLocation(0x10054E20)]
        private int GetFirstRunSlotIdxForObj(GameObjectBody handle)
        {
            for (int i = 0; i < mSlots.Count; i++)
            {
                var slot = mSlots[i];

                if (slot.IsActive
                    && !slot.IsStopProcessing
                    && slot.currentGoal > -1
                    && slot.id.slotIndex != -1
                    && slot.animObj == handle)
                {
                    return i;
                }
            }

            return -1;
        }

        // TODO: Enumerable Generator
        [TempleDllLocation(0x10054E70)]
        private int GetNextRunSlotIdxForObj(GameObjectBody handle, int startSlot)
        {
            for (int i = startSlot + 1; i < mSlots.Count; i++)
            {
                var slot = mSlots[i];

                if (slot.IsActive
                    && !slot.IsStopProcessing
                    && slot.currentGoal > -1
                    && slot.id.slotIndex != -1
                    && slot.animObj == handle)
                {
                    return i;
                }
            }

            return -1;
        }

        [TempleDllLocation(0x1000C430)]
        private AnimSlotId GetFirstRunSlotId(GameObjectBody handle)
        {
            for (var slotIdx = GetFirstRunSlotIdxForObj(handle);
                slotIdx != -1;
                slotIdx = GetNextRunSlotIdxForObj(handle, slotIdx))
            {
                var slot = GetRunSlot(slotIdx);

                var goal = Goals.GetByType(slot.goals[0].goalType);
                if (!goal.interruptAll)
                {
                    return slot.id;
                }
            }

            return AnimSlotId.Null;
        }

        public bool HasRunSlot(GameObjectBody obj) => !GetFirstRunSlotId(obj).IsNull;

        private AnimSlot GetRunSlot(int index)
        {
            return mSlots[index];
        }

        [TempleDllLocation(0x10016C40)]
        public AnimSlot GetSlot(AnimSlotId id)
        {
            if (id.IsNull)
            {
                return null;
            }

            foreach (var slot in mSlots)
            {
                if (slot.id.slotIndex == id.slotIndex && slot.id.uniqueId == id.uniqueId)
                {
                    return slot;
                }
            }

            return null;
        }


        [TempleDllLocation(0x10056600)]
        bool PushGoalInternal(AnimSlotGoalStackEntry stackEntry, out AnimSlotId slotIdOut, AnimSlotFlag flags)
        {
            Trace.Assert(Goals.IsValidType(stackEntry.goalType));

            slotIdOut = AnimSlotId.Null;

            // Don't push new goals while animations are loaded from the savegame
            if (animSysIsLoading != 0)
            {
                return false;
            }

            // Don't push goals for destroyed ObjectHandles.
            var handle = stackEntry.self.obj;
            if (handle.GetFlags().HasFlag(ObjectFlag.DESTROYED))
            {
                return false;
            }

            var previousSlotId = GetFirstRunSlotId(handle);
            if (!previousSlotId.IsNull)
            {
                var existingSlot = GetSlot(previousSlotId);
                if (existingSlot != null && existingSlot.pCurrentGoal.goalType == AnimGoalType.anim_idle)
                {
                    lastSlotPushedTo_ = previousSlotId;

                    if (existingSlot.IsStackFull)
                    {
                        Logger.Error(
                            "Anim: ERROR: Attempt to PushGoal: Goal Stack too LARGE!!! Killing the Animation Slot: AnimID: {0}",
                            lastSlotPushedTo_);
                        Logger.Error("Anim: Current SubGoal Stack is:");
                        for (var i = 0; i <= existingSlot.currentGoal; i++)
                        {
                            Logger.Error("[{0}]: Goal: {1}", i, existingSlot.goals[i].goalType);
                        }

                        existingSlot.flags |= AnimSlotFlag.STOP_PROCESSING;
                        existingSlot.currentState = 0;
                        return false;
                    }

                    existingSlot.currentState = 0;
                    existingSlot.pCurrentGoal = stackEntry; // TODO: We might want to copy here
                    existingSlot.goals.Add(existingSlot.pCurrentGoal);
                    existingSlot.currentGoal = existingSlot.goals.Count - 1;

                    IncreaseActiveGoalCount(existingSlot, Goals.GetByType(stackEntry.goalType));
                    slotIdOut = lastSlotPushedTo_;

                    existingSlot.pCurrentGoal.FreezeObjectRefs();

                    existingSlot.uniqueActionId = 0;
                    return true;
                }
            }

            var newSlotId = AllocSlot();
            if (newSlotId.IsNull)
            {
                lastSlotPushedTo_.Clear();
                return false;
            }

            lastSlotPushedTo_ = newSlotId;
            slotIdOut = lastSlotPushedTo_;

            var runInfo = mSlots[lastSlotPushedTo_.slotIndex];
            runInfo.currentState = 0;
            runInfo.field_14 = -1;
            runInfo.animObj = stackEntry.self.obj;
            runInfo.flags |= flags;
            runInfo.goals.Add(stackEntry); // TODO: We might want to copy here
            runInfo.currentGoal = runInfo.goals.Count - 1;
            runInfo.pCurrentGoal = runInfo.goals[runInfo.currentGoal];
            runInfo.pCurrentGoal.FreezeObjectRefs();

            IncreaseActiveGoalCount(runInfo, Goals.GetByType(runInfo.pCurrentGoal.goalType));

            runInfo.uniqueActionId = 0;

            // Schedule time events for this new slot
            var evt = new TimeEvent(TimeEventType.Anim);
            evt.arg1.int32 = lastSlotPushedTo_.slotIndex;
            evt.arg2.int32 = lastSlotPushedTo_.uniqueId;
            evt.arg3.int32 = 3333;

            return GameSystems.TimeEvent.Schedule(evt, 5, out _);
        }

        [TempleDllLocation(0x10015d70)]
        public void PushFidget(GameObjectBody obj)
        {
            throw new NotImplementedException();
        }

        public void PushDisableFidget()
        {
            Stub.TODO();
        }

        public void PopDisableFidget()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100146c0)]
        public void StartFidgetTimer()
        {
            if (!GameSystems.TimeEvent.IsScheduled(TimeEventType.FidgetAnim))
            {
                var partySize = GameSystems.Party.PartySize;
                if (partySize <= 0)
                {
                    partySize = 1;
                }

                var delay = TimeSpan.FromSeconds(10.0 / partySize);

                var timeEvent = new TimeEvent(TimeEventType.FidgetAnim);

                GameSystems.TimeEvent.Schedule(timeEvent, delay, out _);
            }
        }

        [TempleDllLocation(0x1001B830)]
        public bool ProcessAnimEvent(TimeEvent evt)
        {
            if (mAllSlotsUsed)
            {
                InterruptAllGoalsUpToPriority(AnimGoalPriority.AGP_3);
                mAllSlotsUsed = false;
            }

            // The animation slot id we're triggered for
            var triggerId = new AnimSlotId(evt.arg1.int32, evt.arg2.int32, evt.arg3.int32);

            Trace.Assert(triggerId.slotIndex >= 0 && triggerId.slotIndex < 512);

            var slot = mSlots[triggerId.slotIndex];

            // This seems like a pretty stupid check since slots cannot "move"
            // and the first part of their ID must be the slot index
            // Shouldn't this really check for the unique id of the animation instead?
            if (slot.id.slotIndex != triggerId.slotIndex)
            {
                Logger.Debug("{0} != {1}", slot.id, triggerId);
                return true;
            }

            // Slot did belong to "us", but it was deactivated earlier
            if (!slot.IsActive)
            {
                ProcessActionCallbacks();
                return true;
            }

            // Interesting how this reschedules in at least 100ms which seems steep for
            // animation processing
            // Have to check where and why this is set
            if (slot.flags.HasFlag(AnimSlotFlag.UNK1))
            {
                ProcessActionCallbacks();

                var rescheduleDelay = Math.Max(slot.path.someDelay, 100);
                return RescheduleEvent(rescheduleDelay, slot, evt);
            }

            if (slot.IsStackEmpty)
            {
                Logger.Warn("Found slot {0} with goal < 0", slot.id);
                slot.currentGoal = 0;
            }

            // This sets the current stack pointer, although it should already be set.
            // They used a lot of safeguard against themselves basically
            var currentGoal = slot.goals[slot.currentGoal];
            slot.pCurrentGoal = currentGoal;

            bool stopProcessing = false;
            AnimGoal goal = null;
            // var oldGoal = goal;

            // And another safeguard
            if (currentGoal.goalType < 0 || currentGoal.goalType >= AnimGoalType.count)
            {
                slot.flags |= AnimSlotFlag.STOP_PROCESSING;
                stopProcessing = true;
            }
            else
            {
                goal = Goals.GetByType(currentGoal.goalType);
            }

            // This validates object references found in the animation slot
            if (!ValidateSlot(slot))
            {
                ProcessActionCallbacks();
                return true;
            }

            // Validates that the object the animation runs for is not destroyed
            if (slot.animObj != null)
            {
                if (slot.animObj.GetFlags().HasFlag(ObjectFlag.DESTROYED))
                {
                    Logger.Warn("Processing animation slot {0} for destroyed object.", slot.id);
                }
            }
            else
            {
                // Animation is no longer associated with an object after validation
                slot.flags |= AnimSlotFlag.STOP_PROCESSING;
                stopProcessing = true;
            }

            int delay = 0;
            mCurrentlyProcessingSlotIdx = slot.id.slotIndex;
            // TODO: Clean up this terrible control flow
            if (!stopProcessing)
            {
                mCurrentlyProcessingSlotIdx = slot.id.slotIndex;

                // TODO: processing
                int loopNr = 0;

                while (!stopProcessing)
                {
                    ++loopNr;

                    // This only applies to in-development i think, since as of now there
                    // should be no infi-looping goals
                    if (loopNr >= 100)
                    {
                        Logger.Error("Goal {0} loops infinitely in animation {1}!",
                            slot.pCurrentGoal.goalType, slot.id);
                        GameSystems.Combat.AdvanceTurn(slot.animObj);
                        mCurrentlyProcessingSlotIdx = -1;
                        InterruptGoals(slot, AnimGoalPriority.AGP_HIGHEST);
                        ProcessActionCallbacks();
                        return true;
                    }

                    var currentState = goal.states[slot.currentState];

                    // Prepare for the current state
                    if (!PrepareSlotForGoalState(slot, currentState))
                    {
                        ProcessActionCallbacks();
                        return true;
                    }

                    /*
                    *******  PROCESSING *******
                    */

                    var stateResult = currentState.callback(slot);

                    // Check flags on the slot that may have been set by the callbacks.
                    if (slot.flags.HasFlag(AnimSlotFlag.UNK1))
                    {
                        stopProcessing = true;
                    }

                    if (!(slot.flags.HasFlag(AnimSlotFlag.ACTIVE)))
                    {
                        mCurrentlyProcessingSlotIdx = -1;
                        ProcessActionCallbacks();
                        return true;
                    }

                    if (slot.flags.HasFlag(AnimSlotFlag.STOP_PROCESSING))
                    {
                        break;
                    }

                    var transition = stateResult ? currentState.afterSuccess : currentState.afterFailure;
                    var nextState = transition.newState;
                    delay = transition.delay;

                    // Special transitions
                    if ((nextState & (uint) AnimStateTransitionFlags.MASK) != 0)
                    {
                        var nextStateFlags = ((AnimStateTransitionFlags) nextState & AnimStateTransitionFlags.MASK);


                        /*  if (currentGoal.goalType != AnimGoalType.anim_idle)
                            Logger.Debug("ProcessAnimEvent: Special transition; currentState: {:x}", slot.currentState);*/
                        if (nextStateFlags.HasFlag(AnimStateTransitionFlags.REWIND))
                        {
                            /*if (currentGoal.goalType != AnimGoalType.anim_idle)
                                Logger.Debug("Setting currentState to 0");*/
                            slot.currentState = 0;
                            stopProcessing = true;
                        }

                        if (nextStateFlags.HasFlag(AnimStateTransitionFlags.POP_GOAL_TWICE))
                        {
                            //	Logger.Debug("Popping 2 goals due to 0x38000000");
                            var newGoal = goal;
                            var popFlags = nextStateFlags;
                            PopGoal(slot, popFlags, ref newGoal, ref currentGoal, out stopProcessing);
                            PopGoal(slot, popFlags, ref newGoal, ref currentGoal, out stopProcessing);
                            //oldGoal = goal;
                        }
                        else if (nextStateFlags.HasFlag(AnimStateTransitionFlags.POP_GOAL))
                        {
                            //  Logger.Debug("Popping 1 goals due to 0x30000000");
                            var newGoal = goal;
                            var popFlags = nextStateFlags;
                            PopGoal(slot, popFlags, ref newGoal, ref currentGoal, out stopProcessing);
                            //oldGoal = goal;
                        }

                        if (nextStateFlags.HasFlag(AnimStateTransitionFlags.PUSH_GOAL))
                        {
                            if (slot.IsStackFull)
                            {
                                Logger.Error("Unable to push goal, because anim slot {0} has overrun!", slot.id);
                                Logger.Error("Current sub goal stack is:");

                                for (var i = 0; i < slot.currentGoal; i++)
                                {
                                    Logger.Info("\t[{0}]: Goal {1}", i, slot.goals[i].goalType);
                                }

                                //oldGoal = goal;
                                slot.flags |= AnimSlotFlag.STOP_PROCESSING;
                                slot.currentState = 0;
                                stopProcessing = true;
                            }
                            else
                            {
                                slot.currentState = 0;
                                slot.currentGoal++;

                                currentGoal = slot.goals[slot.currentGoal];
                                slot.pCurrentGoal = currentGoal;

                                // Apparently if 0x30 00 00 00 is also set, it copies the previous goal????
                                if (slot.currentGoal > 0 && !nextStateFlags.HasFlag(AnimStateTransitionFlags.POP_GOAL))
                                {
                                    //	Logger.Debug("Copying previous goal");
                                    slot.goals[slot.currentGoal] = slot.goals[slot.currentGoal - 1];
                                }

                                var newGoalType = (AnimGoalType) (nextState & 0xFFF);
                                goal = Goals.GetByType(newGoalType);
                                slot.goals[slot.currentGoal].goalType = newGoalType;

                                IncreaseActiveGoalCount(slot, goal);
                            }
                        }

                        if (nextStateFlags.HasFlag(AnimStateTransitionFlags.POP_ALL))
                        {
                            //  Logger.Debug("ProcessAnimEvent: 0x90 00 00 00");
                            currentGoal = slot.goals[0];
                            goal = Goals.GetByType(slot.goals[0].goalType);
                            var prio = goal.priority;
                            if (prio < AnimGoalPriority.AGP_7)
                            {
                                //    Logger.Debug("ProcessAnimEvent: root goal priority less than 7");
                                slot.flags |= AnimSlotFlag.STOP_PROCESSING;

                                for (var i = 1; i < slot.currentGoal; i++)
                                {
                                    var _goal = Goals.GetByType(slot.goals[i].goalType);

                                    if (_goal.state_special.HasValue)
                                    {
                                        if (PrepareSlotForGoalState(slot, _goal.state_special.Value))
                                        {
                                            _goal.state_special.Value.callback(slot);
                                        }
                                    }
                                }

                                var goal0 = Goals.GetByType(slot.goals[0].goalType);
                                if (goal0.state_special.HasValue)
                                {
                                    if (PrepareSlotForGoalState(slot, goal0.state_special.Value))
                                        goal0.state_special.Value.callback(slot);
                                }

                                slot.animPath.flags |= AnimPathFlag.UNK_1;
                                slot.currentState = 0;
                                slot.path.flags &= ~PathFlags.PF_COMPLETE;
                                GameSystems.Raycast.GoalDestinationsRemove(slot.path.mover);
                                //oldGoal = goal;
                                slot.field_14 = -1;
                                stopProcessing = true;
                            }
                            else
                            {
                                //	Logger.Debug("ProcessAnimEvent: root goal priority equal to 7");
                                currentGoal = slot.goals[slot.currentGoal];
                                goal = Goals.GetByType(currentGoal.goalType);
                                // oldGoal = goal;
                                while (goal.priority < AnimGoalPriority.AGP_7)
                                {
                                    PopGoal(slot, AnimStateTransitionFlags.POP_GOAL, ref goal, ref currentGoal,
                                        out stopProcessing);
                                    //  Logger.Debug("ProcessAnimEvent: Popped goal for {}.", description.getDisplayName(slot.animObj));
                                    currentGoal = slot.goals[slot.currentGoal];
                                    goal = Goals.GetByType(currentGoal.goalType);
                                    // oldGoal = goal;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Normal jump to another state without special flags
                        --nextState; // Jumps are 1-based, although the array is 0-based
                        if (slot.currentState == nextState)
                        {
                            Logger.Error("State {0} of goal {1} transitioned into itself.",
                                slot.currentState, currentGoal.goalType);
                        }

                        slot.currentState = (int) nextState;
                    }

                    if (delay > 0)
                    {
                        switch (delay)
                        {
                            case AnimStateTransition.DelaySlot:
                                // Use the delay specified in the slot. Reasoning currently unknown.
                                // NOTE: Could mean that it's waiting for pathing to complete
                                delay = slot.path.someDelay;
                                break;
                            case AnimStateTransition.DelayCustom:
                                // Used by some goal states to set their desired dynamic delay
                                delay = customDelayInMs;
                                break;
                            case AnimStateTransition.DelayRandom:
                                // Calculates the animation delay randomly in a range from 0 to 300
                                delay = GameSystems.Random.GetInt(0, 300);
                                break;
                            default:
                                // Keep predefined delay
                                break;
                        }

                        stopProcessing = true;
                    }

                    // If no delay has been set, the next state is immediately processed
                }
            }

            mCurrentlyProcessingSlotIdx = -1;

            // Does Flag 2 mean "COMPLETED" ?
            if (!(slot.flags.HasFlag(AnimSlotFlag.STOP_PROCESSING)))
            {
                if (slot.flags.HasFlag(AnimSlotFlag.ACTIVE))
                {
                    // This actually seems to be the "GOOD" case
                    RescheduleEvent(delay, slot, evt);
                }

                ProcessActionCallbacks();
                return true;
            }

            var result = true;
            if (slot.animObj != null)
            {
                // preserve the values i case the slot gets deallocated below
                var animObj = slot.animObj;
                var actionAnimId = slot.uniqueActionId;
                result = InterruptGoals(slot, AnimGoalPriority.AGP_HIGHEST);

                if (animObj != null && animObj.IsCritter())
                {
                    mActionCallbacks.Add(new AnimActionCallback(animObj, actionAnimId));
                }
            }

            ProcessActionCallbacks();
            return result;
        }


        /*
        When an event should be re-executed at a later time, but unmodified, this
        method is used. It also checks whether animations should "catch up" (by skipping
        frames essentially), or whether they should be run at whatever speed was
        intended,
        but visibly slowing down.
        */
        private bool RescheduleEvent(int delayMs, AnimSlot slot, TimeEvent oldEvt)
        {
            var evt = new TimeEvent(TimeEventType.Anim);
            evt.arg1.int32 = slot.id.slotIndex;
            evt.arg2.int32 = slot.id.uniqueId;
            evt.arg3.int32 = 1111; // Some way to identify these rescheduled events???

            if (Globals.Config.animCatchup)
            {
                return GameSystems.TimeEvent.ScheduleAbsolute(evt, oldEvt.time, delayMs, out slot.nextTriggerTime);
            }
            else
            {
                return GameSystems.TimeEvent.Schedule(evt, delayMs, out slot.nextTriggerTime);
            }
        }


        [TempleDllLocation(0x1000c760)]
        public void ClearForObject(GameObjectBody obj)
        {
            // TODO
        }

        [TempleDllLocation(0x1000c8d0)]
        public void InterruptAllGoalsUpToPriority(AnimGoalPriority priority)
        {
            Trace.Assert(priority >= AnimGoalPriority.AGP_NONE && priority <= AnimGoalPriority.AGP_HIGHEST);
            foreach (var slot in mSlots)
            {
                if (slot.IsActive)
                {
                    if (!InterruptGoals(slot, priority))
                    {
                        Logger.Error("Failed to interrupt anim goals of priority {0} on slot {1}",
                            priority, slot.id.slotIndex);
                    }
                }
            }
        }


        [TempleDllLocation(0x10056090)]
        public bool InterruptGoals(AnimSlot slot, AnimGoalPriority priority)
        {
            Trace.Assert(slot.id.slotIndex < mSlots.Count);

            if (!slot.IsActive)
            {
                return false;
            }

            if (!GameSystems.IsResetting() && slot.currentGoal != -1)
            {
                var stackTop = slot.goals[slot.currentGoal];
                var goal = Goals.GetByType(stackTop.goalType);

                if (priority < AnimGoalPriority.AGP_HIGHEST)
                {
                    if (goal.interruptAll)
                    {
                        return true;
                    }

                    if (goal.priority == AnimGoalPriority.AGP_5)
                    {
                        return false;
                    }
                }


                if (goal.priority == AnimGoalPriority.AGP_3)
                {
                    if (priority < AnimGoalPriority.AGP_3)
                    {
                        return false;
                    }
                }
                else if (goal.priority == AnimGoalPriority.AGP_2)
                {
                    if (priority < AnimGoalPriority.AGP_2)
                    {
                        return false;
                    }
                }
                else if (goal.priority >= priority)
                {
                    if (goal.priority != AnimGoalPriority.AGP_7)
                    {
                        return false;
                    }

                    slot.flags &= ~AnimSlotFlag.UNK5;
                }
            }

            var goalType = Goals.GetByType(slot.goals[0].goalType);
            if (goalType.priority >= AnimGoalPriority.AGP_7 &&
                priority < AnimGoalPriority.AGP_7)
            {
                var pNewStackTopOut = slot.goals[slot.currentGoal];
                for (goalType = Goals.GetByType(pNewStackTopOut.goalType);
                    goalType.priority < AnimGoalPriority.AGP_7;
                    goalType = Goals.GetByType(pNewStackTopOut.goalType))
                {
                    PopGoal(slot, AnimStateTransitionFlags.POP_GOAL, ref goalType, ref pNewStackTopOut, out _);
                    pNewStackTopOut = slot.goals[slot.currentGoal];
                }

                return true;
            }

            slot.flags |= AnimSlotFlag.STOP_PROCESSING;

            if (mCurrentlyProcessingSlotIdx == slot.id.slotIndex)
            {
                return true;
            }

            // Removes all time events for the slot
            GameSystems.TimeEvent.Remove(TimeEventType.Anim, evt => evt.arg1.int32 == slot.id.slotIndex);

            if (slot.currentGoal != -1)
            {
                if (slot.pCurrentGoal == null)
                {
                    slot.pCurrentGoal = slot.goals[slot.currentGoal];
                }

                for (var i = slot.currentGoal; i >= 0; i--)
                {
                    var goal = Goals.GetByType(slot.goals[i].goalType);
                    if (!GameSystems.IsResetting())
                    {
                        var state_special = goal.state_special;
                        if (state_special.HasValue && state_special.Value.callback != null)
                        {
                            if (PrepareSlotForGoalState(slot, state_special.Value))
                            {
                                state_special.Value.callback(slot);
                            }
                        }
                    }
                }
            }

            FreeSlot(slot);
            return false;
        }

        private AnimSlotId AllocSlot()
        {
            // Find a free slot
            int freeSlot = -1;
            for (int i = 0; i < ANIM_RUN_SLOT_CAP; i++)
            {
                if (!mSlots[i].IsActive)
                {
                    freeSlot = i;
                    break;
                }
            }

            if (freeSlot == -1)
            {
                Logger.Error("All animation slots are in use!");
                mAllSlotsUsed = true;
                return AnimSlotId.Null;
            }

            var slot = mSlots[freeSlot];
            slot.id.slotIndex = freeSlot;
            slot.id.uniqueId = nextUniqueId++;
            slot.id.field_8 = 0;
            slot.flags = AnimSlotFlag.ACTIVE;
            slot.animPath.pathLength = 0;
            slot.path.flags = 0;
            slot.pCurrentGoal = null;
            slot.animObj = null;
            slot.currentState = -1;
            slot.nextTriggerTime.timeInDays = 0;
            slot.nextTriggerTime.timeInMs = 0;

            slot.goals[0].self.obj = null;
            slot.goals[0].target.obj = null;
            slot.goals[0].block.obj = null;
            slot.goals[0].scratch.obj = null;
            slot.goals[0].parent.obj = null;
            slot.goals[0].targetTile.obj = null;
            slot.goals[0].selfTracking = default;
            slot.goals[0].targetTracking = default;
            slot.goals[0].blockTracking = default;
            slot.goals[0].scratchTracking = default;
            slot.goals[0].parentTracking = default;

            slotsInUse++;

            return slot.id;
        }

        [TempleDllLocation(0x10055D30)]
        [TempleDllLocation(0x10055ED0)]
        private void FreeSlot(AnimSlot slot)
        {
            if (!slot.IsActive)
            {
                slot.Clear();
                return;
            }

            for (int i = 0; i <= slot.currentGoal; i++)
            {
                var goalType = slot.goals[i].goalType;
                DecreaseActiveGoalCount(slot, Goals.GetByType(goalType));
            }

            slot.Clear();
            slotsInUse--;

            if (mActiveGoalCount == 0)
            {
                mAllGoalsClearedCallback?.Invoke();
            }
        }

        [TempleDllLocation(0x10016FC0)]
        void PopGoal(AnimSlot slot, AnimStateTransitionFlags popFlags,
            ref AnimGoal newGoal,
            ref AnimSlotGoalStackEntry newCurrentGoal,
            out bool stopProcessing)
        {
            stopProcessing = false;

            //Logger.Debug("Pop goal for {} with popFlags {:x}  (slot flags: {:x}, state {:x})", description.getDisplayName(slot.animObj), popFlags, static_cast<uint>(slot.flags), slot.currentState);
            if (slot.currentGoal == 0 && !popFlags.HasFlag(AnimStateTransitionFlags.PUSH_GOAL))
            {
                slot.flags |= AnimSlotFlag.STOP_PROCESSING;
            }

            if (newGoal.state_special.HasValue)
            {
                if (!popFlags.HasFlag(AnimStateTransitionFlags.UNK_1000000) ||
                    !popFlags.HasFlag(AnimStateTransitionFlags.UNK_4000000))
                {
                    if (PrepareSlotForGoalState(slot, newGoal.state_special.Value))
                    {
                        //  Logger.Debug("Pop goal for {}: doing state special callback.", description.getDisplayName(slot.animObj));
                        newGoal.state_special.Value.callback(slot);
                    }
                }
            }

            if (!popFlags.HasFlag(AnimStateTransitionFlags.UNK_1000000))
            {
                slot.flags &= ~(AnimSlotFlag.UNK10 | AnimSlotFlag.UNK7 | AnimSlotFlag.UNK5 |
                                AnimSlotFlag.UNK4 | AnimSlotFlag.UNK3);

                slot.animPath.pathLength = 0; // slot.anim_path.maxPathLength = 0;
            }

            if (popFlags.HasFlag(AnimStateTransitionFlags.GOAL_INVALIDATE_PATH))
            {
                GameObjectBody mover = slot.path.mover;
                slot.animPath.flags = AnimPathFlag.UNK_1;
                slot.path.flags = default;
                GameSystems.Raycast.GoalDestinationsRemove(mover);
            }

            DecreaseActiveGoalCount(slot, newGoal);
            slot.currentGoal--;
            slot.currentState = 0;
            if (slot.IsStackEmpty)
            {
                if (!popFlags.HasFlag(AnimStateTransitionFlags.PUSH_GOAL))
                {
                    slot.flags |= AnimSlotFlag.STOP_PROCESSING;
                    //  Logger.Debug("Pop goal for {}: stopping processing (last goal was {}).", description.getDisplayName(slot.animObj), animGoalTypeNames[slot.pCurrentGoal.goalType]);
                }
            }
            else
            {
                var prevGoal = slot.goals[slot.currentGoal];
                //Logger.Debug("Popped goal {}, new goal is {}", animGoalTypeNames[slot.pCurrentGoal.goalType], animGoalTypeNames[prevGoal.goalType]);
                slot.pCurrentGoal = newCurrentGoal = slot.goals[slot.currentGoal];
                newGoal = Goals.GetByType(newCurrentGoal.goalType);
                stopProcessing = false;
                if (prevGoal.goalType == AnimGoalType.anim_fidget)
                {
                    Debugger.Break();
                    int dummy = 1;
                    // FIX: prevents AnimGoalType.anim_fidget from queueing an AnimComplete call (which
                    // creates the phantom animId = 0 bullshit)
                }
                else if (newCurrentGoal.goalType == AnimGoalType.anim_idle &&
                         !popFlags.HasFlag(AnimStateTransitionFlags.PUSH_GOAL))
                {
                    PushActionCallback(slot);
                }
            }

            // Logger.Debug("PopGoal: Ending with slot flags {:x}, state {:x}", static_cast<uint>(slot.flags), slot.currentState);
        }

        [TempleDllLocation(0x10016a30)]
        private void ProcessActionCallbacks()
        {
            // changed to manual iteration because PerformOnAnimComplete can alter the vector
            var initSize = mActionCallbacks.Count;
            for (var i = 0; i < mActionCallbacks.Count; i++)
            {
                var callback = mActionCallbacks[i];
                GameSystems.D20.Actions.PerformOnAnimComplete(callback.obj, callback.uniqueId);
                if (initSize != mActionCallbacks.Count)
                {
                    Debugger.Break();
                }

                mActionCallbacks[i] = new AnimActionCallback(null, 0);
            }

            mActionCallbacks.Clear();
        }

        [TempleDllLocation(0x10016a00)]
        private void PushActionCallback(AnimSlot slot)
        {
            if (slot.uniqueActionId == 0)
            {
                return;
            }

            mActionCallbacks.Add(new AnimActionCallback(slot.animObj, slot.uniqueActionId));
        }

        // Replaces PrepareSlotForGoalState with null state
        [TempleDllLocation(0x10055700)]
        private bool ValidateSlot(AnimSlot slot)
        {
            if (!slot.pCurrentGoal.ValidateObjectRefs())
            {
                slot.animObj = null;
                return false;
            }

            // sets the animObj of the slot to the self-obj of the goal
            slot.animObj = slot.pCurrentGoal.self.obj;
            return true;
        }

        [TempleDllLocation(0x10055700)]
        private bool PrepareSlotForGoalState(AnimSlot slot, AnimGoalState state)
        {
            if (!slot.pCurrentGoal.ValidateObjectRefs())
            {
                slot.animObj = null;
                return false;
            }

            // sets the animObj of the slot to the self-obj of the goal
            slot.animObj = slot.pCurrentGoal.self.obj;

            slot.stateFlagData = state.flagsData;
            slot.param1 = GetAnimParam(slot, (AnimGoalProperty) state.argInfo1);
            slot.param2 = GetAnimParam(slot, (AnimGoalProperty) state.argInfo2);
            return true;
        }

        private AnimParam GetAnimParam(AnimSlot slot, AnimGoalProperty property)
        {
            if ((int) property == -1)
            {
                Debugger.Break();
                var result = new AnimParam();
                result.number = 0;
                return result;
            }

            var currentStack = slot.pCurrentGoal;

            return currentStack.GetAnimParam(property);
        }


        [TempleDllLocation(0x10055BF0)]
        private void IncreaseActiveGoalCount(AnimSlot slot, AnimGoal goal)
        {
            if (goal.priority >= AnimGoalPriority.AGP_2 && !goal.interruptAll && !CurrentGoalHasField10_1(slot))
            {
                Trace.Assert(mActiveGoalCount >= 0);
                ++mActiveGoalCount;
            }
        }

        [TempleDllLocation(0x10055ca0)]
        private void DecreaseActiveGoalCount(AnimSlot slot, AnimGoal goal)
        {
            if (goal.priority >= AnimGoalPriority.AGP_2 && !goal.interruptAll && !CurrentGoalHasField10_1(slot) &&
                mActiveGoalCount >= 1)
            {
                --mActiveGoalCount;
            }
        }

        [TempleDllLocation(0x100551a0)]
        private bool CurrentGoalHasField10_1(AnimSlot slot)
        {
            Trace.Assert(slot.IsActive);
            Trace.Assert(slot.pCurrentGoal != null);
            Trace.Assert(!slot.IsStackEmpty);

            var goal = Goals.GetByType(slot.pCurrentGoal.goalType);

            return (goal.field_10 & 1) != 0 || (slot.pCurrentGoal.flagsData.number & 0x80) != 0;
        }


        [TempleDllLocation(0x1000c890)]
        public void InterruptAll()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1000c7e0)]
        public bool Interrupt(GameObjectBody obj, AnimGoalPriority priority, bool all)
        {
            Stub.TODO();
            return false;
        }

        [TempleDllLocation(0x1001a1d0)]
        public bool PushIdleOrLoop(GameObjectBody obj)
        {
            Trace.Assert(obj != null);

            if (HasRunSlot(obj))
            {
                return false;
            }

            if (obj.IsCritter())
            {
                var newGoal = new AnimSlotGoalStackEntry(obj, AnimGoalType.anim_idle, true);
                PushGoal(newGoal, out _);
                return false;
            }
            else
            {
                var newGoal = new AnimSlotGoalStackEntry(obj, AnimGoalType.animate_loop, true);
                newGoal.animIdPrevious.number = obj.GetIdleAnim();
                if (!PushGoal(newGoal, out var slotId))
                    return false;

                GetSlot(slotId).goals[0].soundHandle.number = -1;
                return true;
            }
        }

        [TempleDllLocation(0x1000c750)]
        public void SetAllGoalsClearedCallback(Action callback)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1000c950)]
        public bool InterruptAllForTbCombat()
        {
            Stub.TODO();
            return false;
        }

        [TempleDllLocation(0x1001aaa0)]
        public void NotifySpeedRecalc(GameObjectBody obj)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1001cab0)]
        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1001d250)]
        public bool LoadGame()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1000c120)]
        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void Debug()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1000C5A0)]
        public bool IsIdleOrFidgeting(GameObjectBody actor)
        {
            // TODO: This function was broken in vanilla and likely never worked
            var slot = mSlots[GetFirstRunSlotIdxForObj(actor)];
            if (slot.currentGoal != 0)
            {
                return false;
            }

            if (slot.pCurrentGoal == null)
            {
                slot.pCurrentGoal = slot.goals[0];
            }

            if (slot.pCurrentGoal.goalType > AnimGoalType.anim_fidget)
            {
                return false;
            }

            var animHandle = actor.GetOrCreateAnimHandle();
            var animId = animHandle.GetAnimId();
            if (!animId.IsSpecialAnim() && animId.GetNormalAnimType() == NormalAnimType.ItemFidget
                || animId.IsWeaponAnim() && (animId.GetWeaponAnim() == WeaponAnim.Fidget ||
                                             animId.GetWeaponAnim() == WeaponAnim.Fidget2 ||
                                             animId.GetWeaponAnim() == WeaponAnim.Fidget3 ||
                                             animId.GetWeaponAnim() == WeaponAnim.CombatFidget))
            {
                return true;
            }

            return false;
        }

        [TempleDllLocation(0x10054f30)]
        public bool IsRunningGoal(GameObjectBody objId, AnimGoalType animGoal, out AnimSlotId runId)
        {
            runId = AnimSlotId.Null;

            if (objId == null)
            {
                return false;
            }

            var slotIdx = GetFirstRunSlotIdxForObj(objId);
            if (slotIdx == -1)
            {
                return false;
            }

            var goal = animGoal;
            var slot = mSlots[slotIdx];

            for (int i = 0; i < slot.currentGoal - 1; i++)
            {
                if (slot.goals[i].goalType == goal)
                {
                    runId = slot.id;
                    return true;
                }
            }

            return false;
        }

        [TempleDllLocation(0x1001d220)]
        public bool PushAttackOther(GameObjectBody attacker, GameObjectBody target)
        {
            var random = GameSystems.Random.GetInt(0, 2);
            return PushAttack(attacker, target, -1, random, false, false);
        }

        [TempleDllLocation(0x1001c370)]
        public bool PushAttack(GameObjectBody attacker, GameObjectBody target, int scratchVal5, int attackAnimIdx,
            bool flag1, bool flag2)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10015ad0)]
        public bool ReturnProjectile(GameObjectBody projectile, LocAndOffsets returnTo, GameObjectBody target)
        {
            var objId = projectile;
            if (objId != null && IsRunningGoal(objId, AnimGoalType.projectile, out var runId))
            {
                var v3 = mSlots[runId.slotIndex];
                v3.goals[0].targetTile.location = returnTo;
                v3.goals[0].target.obj = v3.goals[0].parent.obj;
                v3.goals[0].scratch.obj = target;
                v3.flags &= ~AnimSlotFlag.UNK5;
                v3.animPath.flags = AnimPathFlag.UNK_1;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}