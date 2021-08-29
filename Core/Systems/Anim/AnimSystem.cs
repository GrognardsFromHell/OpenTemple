using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.IO.SaveGames.MapState;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.Pathfinding;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.TimeEvents;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.InGameSelect;
using OpenTemple.Core.Utils;
using Path = System.IO.Path;

namespace OpenTemple.Core.Systems.Anim
{
    public class AnimSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public bool VerbosePartyLogging { get; set; }

        private AnimSlotId _verboseLoggingForSlot = AnimSlotId.Null;

        public void DebugLastPushedSlot()
        {
            _verboseLoggingForSlot = lastSlotPushedTo_;
        }

        public void DebugSlot(AnimSlotId slotId)
        {
            _verboseLoggingForSlot = slotId;
        }

        /*
            Set to true when ToEE cannot allocate an animation slot. This causes
            the anim system to try and interrupt as many animations as possible.
        */
        [TempleDllLocation(0x10AA4BB0)]
        private bool mAllSlotsUsed;

        [TempleDllLocation(0x118CE520)]
        private List<AnimSlot> mSlots = new List<AnimSlot>();

        [TempleDllLocation(0x102AC880)]
        private AnimSlotId animIdGlobal;

        [TempleDllLocation(0x10AA4BB8)]
        private bool animSysIsLoading;

        // The last slot that a goal was pushed to
        [TempleDllLocation(0x102B2648)]
        private AnimSlotId lastSlotPushedTo_;

        /*
            While processing the timer event for a slot, this will contain the slots index.
            Otherwise -1.
        */
        [TempleDllLocation(0x102B2654)]
        private int mCurrentlyProcessingSlotIdx = -1;

        /// <summary>
        /// Callbacks for animations that have reached their action trigger that should be dispatched
        /// to the action system after animation processing has completed.
        /// </summary>
        private readonly List<AnimActionCallback> _actionCallbacks = new();

        private Action mAllGoalsClearedCallback;

        [TempleDllLocation(0x10AA4BC0)]
        private int mActiveGoalCount;

        [TempleDllLocation(0x10AA4BBC)]
        private int slotsInUse;

        [TempleDllLocation(0x11E61520)]
        private int nextUniqueId;

        [TempleDllLocation(0x10307534)]
        public int customDelayInMs { get; set; }

        // The next id that'll be assigned to uniqueActionId if the action system requests one to be assigned
        [TempleDllLocation(0x10307540)]
        private int _nextUniqueActionId = 1;

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
        public bool IsProcessing => mCurrentlyProcessingSlotIdx != -1;

        [TempleDllLocation(0x10056d20)]
        public bool PushGoal(AnimSlotGoalStackEntry stackEntry, out AnimSlotId slotId)
        {
            return PushGoalInternal(stackEntry, out slotId, 0);
        }

        public IEnumerable<AnimSlot> EnumerateSlots(GameObjectBody obj)
        {
            for (var slotIdx = GameSystems.Anim.GetFirstRunSlotIdxForObj(obj);
                slotIdx != -1;
                slotIdx = GameSystems.Anim.GetNextRunSlotIdxForObj(obj, slotIdx))
            {
                yield return GameSystems.Anim.mSlots[slotIdx];
            }
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
        public AnimSlotId GetFirstRunSlotId(GameObjectBody handle)
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
            if (animSysIsLoading)
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
                    existingSlot.pCurrentGoal = stackEntry;
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

        [TempleDllLocation(0x10307550)]
        public bool fidgetDisabled;

        [TempleDllLocation(0x10015d70)]
        public bool PushFidget(GameObjectBody obj)
        {
            if (!obj.IsCritter())
            {
                return false;
            }

            if ((obj.GetCritterFlags2() & CritterFlag2.AUTO_ANIMATES) == 0)
            {
                return false;
            }

            if (GameSystems.Combat.IsCombatActive())
            {
                return false;
            }

            return PushFidgetInternal(obj);
        }

        [TempleDllLocation(0x10015BB0)]
        private bool PushFidgetInternal(GameObjectBody critter)
        {
            if (!critter.IsCritter())
            {
                return false;
            }

            if (!CritterCanAnimate(critter)
                || IsRunningGoal(critter, AnimGoalType.anim_fidget, out _)
                || !GetFirstRunSlotId(critter).IsNull)
            {
                return false;
            }

            var aasHandle = critter.GetOrCreateAnimHandle();
            var currentAnim = aasHandle.GetAnimId();

            // Only fidget if the current animation is an idle animation
            if (currentAnim.IsWeaponAnim() && currentAnim.GetWeaponAnim() == WeaponAnim.Idle
                || !currentAnim.IsSpecialAnim() && currentAnim.GetNormalAnimType() == NormalAnimType.ItemIdle)
            {
                var goalData = new AnimSlotGoalStackEntry(critter, AnimGoalType.anim_fidget, true);
                return PushGoal(goalData, out _);
            }

            return false;
        }

        [TempleDllLocation(0x100144b0)]
        public void FidgetEnable()
        {
            fidgetDisabled = false;
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

            Trace.Assert(triggerId.slotIndex >= 0 && triggerId.slotIndex < mSlots.Count);

            var slot = mSlots[triggerId.slotIndex];

            // This seems like a pretty stupid check since slots cannot "move"
            // and the first part of their ID must be the slot index
            // Shouldn't this really check for the unique id of the animation instead?
            if (slot.id.uniqueId != triggerId.uniqueId)
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

                var rescheduleDelay = Math.Max((int)slot.path.PauseTime.TotalMilliseconds, 100);
                return RescheduleEvent(rescheduleDelay, slot, evt);
            }

            if (slot.IsStackEmpty)
            {
                Logger.Warn("Found slot {0} with goal < 0", slot.id);
                slot.currentGoal = 0;
            }

            // This validates object references found in the animation slot
            if (!ValidateSlot(slot))
            {
                ProcessActionCallbacks();
                return true;
            }

            int delay = 0;
            mCurrentlyProcessingSlotIdx = slot.id.slotIndex;
            // TODO: Clean up this terrible control flow

            // TODO: processing
            int loopNr = 0;

            bool stopProcessing = false;
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

                // This sets the current stack pointer, although it should already be set.
                // They used a lot of safeguard against themselves basically
                var currentGoal = slot.goals[slot.currentGoal];
                slot.pCurrentGoal = currentGoal;

                // And another safeguard
                if (currentGoal.goalType < 0 || currentGoal.goalType >= AnimGoalType.count)
                {
                    slot.flags |= AnimSlotFlag.STOP_PROCESSING;
                    break;
                }

                var goal = Goals.GetByType(currentGoal.goalType);

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
                    break;
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

                if (VerbosePartyLogging && GameSystems.Party.IsInParty(slot.animObj) &&
                    slot.pCurrentGoal.goalType != AnimGoalType.anim_idle
                    || _verboseLoggingForSlot == slot.id)
                {
                    Logger.Debug("PC {0} {1} [Depth:{2}] [State:{3}] {4} = {5}",
                        GameSystems.MapObject.GetDisplayName(slot.animObj),
                        slot.pCurrentGoal.goalType,
                        slot.goals.Count,
                        slot.currentState,
                        currentState.callback.Method.Name,
                        stateResult
                    );
                }

                // Check flags on the slot that may have been set by the callbacks.
                if (slot.flags.HasFlag(AnimSlotFlag.UNK1))
                {
                    // TODO: Make sure this is *ever* used
                    stopProcessing = true;
                }

                if (!slot.flags.HasFlag(AnimSlotFlag.ACTIVE))
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
                if ((nextState & (uint)AnimStateTransitionFlags.MASK) != 0)
                {
                    var currentStack = new List<AnimSlotGoalStackEntry>(slot.goals);

                    var nextStateFlags = (AnimStateTransitionFlags)nextState & AnimStateTransitionFlags.MASK;
                    if (nextStateFlags.HasFlag(AnimStateTransitionFlags.REWIND))
                    {
                        slot.currentState = 0;
                        stopProcessing = true;
                    }

                    if (nextStateFlags.HasFlag(AnimStateTransitionFlags.POP_GOAL_TWICE))
                    {
                        // TODO: I believe this is never used by any type of goal (double check)
                        var newGoal = goal;
                        var popFlags = nextStateFlags;
                        PopGoal(slot, popFlags, ref newGoal, ref currentGoal);
                        PopGoal(slot, popFlags, ref newGoal, ref currentGoal);
                    }
                    else if (nextStateFlags.HasFlag(AnimStateTransitionFlags.POP_GOAL))
                    {
                        var newGoal = goal;
                        var popFlags = nextStateFlags;
                        PopGoal(slot, popFlags, ref newGoal, ref currentGoal);
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
                            var newGoalType = (AnimGoalType)(nextState & 0xFFF);
                            goal = Goals.GetByType(newGoalType);

                            AnimSlotGoalStackEntry stackEntry = new AnimSlotGoalStackEntry(null, newGoalType);

                            // TODO: I think TOEE Doesnt _Clear_ the old params, so it means if you POP/PUSH
                            // it'll inherit the popped goal's parameters...
                            if (slot.goals.Count < currentStack.Count)
                            {
                                currentStack[slot.goals.Count].CopyTo(stackEntry);
                            }

                            // Apparently if 0x30 00 00 00 is also set, it copies the previous goal????
                            if (slot.currentGoal >= 0 && !nextStateFlags.HasFlag(AnimStateTransitionFlags.POP_GOAL))
                            {
                                slot.goals[slot.currentGoal].CopyTo(stackEntry);
                            }

                            stackEntry.goalType = newGoalType;
                            slot.goals.Add(stackEntry);
                            slot.currentState = 0;
                            slot.currentGoal++;
                            currentGoal = slot.goals[slot.currentGoal];
                            slot.pCurrentGoal = currentGoal;

                            IncreaseActiveGoalCount(slot, goal);
                        }
                    }

                    if (nextStateFlags.HasFlag(AnimStateTransitionFlags.POP_ALL))
                    {
                        //  Logger.Debug("ProcessAnimEvent: 0x90 00 00 00");
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
                            slot.ClearPath();
                            //oldGoal = goal;
                            slot.field_14 = -1;
                            stopProcessing = true;
                        }
                        else
                        {
                            currentGoal = slot.goals[slot.currentGoal];
                            goal = Goals.GetByType(currentGoal.goalType);
                            while (goal.priority < AnimGoalPriority.AGP_7)
                            {
                                PopGoal(slot, AnimStateTransitionFlags.POP_GOAL, ref goal, ref currentGoal);
                                currentGoal = slot.goals[slot.currentGoal];
                                goal = Goals.GetByType(currentGoal.goalType);
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

                    slot.currentState = (int)nextState;
                }

                if (delay > 0)
                {
                    switch (delay)
                    {
                        case AnimStateTransition.DelaySlot:
                            // Use the delay specified in the slot. Reasoning currently unknown.
                            // NOTE: Could mean that it's waiting for pathing to complete
                            delay = (int)slot.path.PauseTime.TotalMilliseconds;
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
                    _actionCallbacks.Add(new AnimActionCallback(animObj, actionAnimId));
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
            if (GameSystems.LightScheme.IsUpdating)
            {
                return;
            }

            for (int slotIdx = GetFirstRunSlotIdxForObj(obj);
                slotIdx != -1;
                slotIdx = GetNextRunSlotIdxForObj(obj, slotIdx))
            {
                var slot = mSlots[slotIdx];

                if (!slot.IsActive)
                {
                    continue;
                }

                slot.flags |= AnimSlotFlag.UNK11 | AnimSlotFlag.STOP_PROCESSING;

                if (mCurrentlyProcessingSlotIdx == slotIdx)
                {
                    continue;
                }

                // Clear the time event for this slot
                GameSystems.TimeEvent.Remove(TimeEventType.Anim, evt => evt.arg1.int32 == slot.id.slotIndex);

                for (int i = slot.currentGoal; i >= 0; i--)
                {
                    var goalState = slot.goals[i];
                    var goal = Goals.GetByType(goalState.goalType);
                    if (goal.state_special.HasValue)
                    {
                        if (PrepareSlotForGoalState(slot, goal.state_special.Value))
                        {
                            goal.state_special.Value.callback(slot);
                        }
                    }
                }

                FreeSlot(slot);
            }

            // Remove any queued anim callbacks. Note that this may leave dangling actions for the object.
            if (_actionCallbacks.RemoveAll(c => c.obj == obj) > 0)
            {
                // We might have to call back the anim system to tell it the animation was interrupted
                Logger.Warn("Removed pending anim callbacks for obj {0} during removal", obj);
            }
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

            if (slot.goals.Count > 0)
            {
                var goalType = Goals.GetByType(slot.goals[0].goalType);
                if (goalType.priority >= AnimGoalPriority.AGP_7 &&
                    priority < AnimGoalPriority.AGP_7)
                {
                    var pNewStackTopOut = slot.goals[slot.currentGoal];
                    for (goalType = Goals.GetByType(pNewStackTopOut.goalType);
                        goalType.priority < AnimGoalPriority.AGP_7;
                        goalType = Goals.GetByType(pNewStackTopOut.goalType))
                    {
                        PopGoal(slot, AnimStateTransitionFlags.POP_GOAL, ref goalType, ref pNewStackTopOut);
                        pNewStackTopOut = slot.goals[slot.currentGoal];
                    }

                    return true;
                }
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
            for (int i = 0; i < mSlots.Count; i++)
            {
                if (!mSlots[i].IsActive)
                {
                    freeSlot = i;
                    break;
                }
            }

            if (freeSlot == -1)
            {
                freeSlot = mSlots.Count;
                mSlots.Add(new AnimSlot());
            }

            var slot = mSlots[freeSlot];
            slot.id.slotIndex = freeSlot;
            slot.id.uniqueId = nextUniqueId++;
            slot.id.field_8 = 0;
            slot.flags = AnimSlotFlag.ACTIVE;
            slot.animPath.maxPathLength = 0;
            slot.path.flags = 0;
            slot.pCurrentGoal = null;
            slot.animObj = null;
            slot.currentState = -1;
            slot.nextTriggerTime.timeInDays = 0;
            slot.nextTriggerTime.timeInMs = 0;

            /*slot.goals[0].self.obj = null;
            slot.goals[0].target.obj = null;
            slot.goals[0].block.obj = null;
            slot.goals[0].scratch.obj = null;
            slot.goals[0].parent.obj = null;
            slot.goals[0].targetTile.obj = null;
            slot.goals[0].selfTracking = default;
            slot.goals[0].targetTracking = default;
            slot.goals[0].blockTracking = default;
            slot.goals[0].scratchTracking = default;
            slot.goals[0].parentTracking = default;*/

            slotsInUse++;

            return slot.id;
        }

        [TempleDllLocation(0x10055D30)]
        [TempleDllLocation(0x10055ED0)]
        private void FreeSlot(AnimSlot slot)
        {
            if (_verboseLoggingForSlot == slot.id)
            {
                _verboseLoggingForSlot = AnimSlotId.Null;
            }

            if (lastSlotPushedTo_ == slot.id)
            {
                lastSlotPushedTo_ = AnimSlotId.Null;
            }

            if (animIdGlobal == slot.id)
            {
                animIdGlobal = AnimSlotId.Null;
            }

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

            // Clean up slots from the back
            while (mSlots.Count > 0 && !mSlots[^1].IsActive)
            {
                mSlots.RemoveAt(mSlots.Count - 1);
            }

            if (mActiveGoalCount == 0)
            {
                mAllGoalsClearedCallback?.Invoke();
            }
        }

        [TempleDllLocation(0x10016FC0)]
        private void PopGoal(AnimSlot slot, AnimStateTransitionFlags popFlags,
            ref AnimGoal newGoal,
            ref AnimSlotGoalStackEntry newCurrentGoal)
        {
            //Logger.Debug("Pop goal for {0} with popFlags {1:x}  (slot flags: {2:x}, state {3:x})", description.getDisplayName(slot.animObj), popFlags, static_cast<uint>(slot.flags), slot.currentState);
            if (slot.currentGoal == 0 && !popFlags.HasFlag(AnimStateTransitionFlags.PUSH_GOAL))
            {
                slot.flags |= AnimSlotFlag.STOP_PROCESSING;
            }

            if (newGoal.state_special.HasValue)
            {
                if ((popFlags & (AnimStateTransitionFlags.UNK_1000000 | AnimStateTransitionFlags.GOAL_INVALIDATE_PATH |
                                 AnimStateTransitionFlags.UNK_4000000)) == 0 ||
                    (popFlags & AnimStateTransitionFlags.UNK_4000000) == 0)
                {
                    if (PrepareSlotForGoalState(slot, newGoal.state_special.Value))
                    {
                        //  Logger.Debug("Pop goal for {0}: doing state special callback.", description.getDisplayName(slot.animObj));
                        newGoal.state_special.Value.callback(slot);
                    }
                }
            }

            if (!popFlags.HasFlag(AnimStateTransitionFlags.UNK_1000000))
            {
                slot.flags &= ~(AnimSlotFlag.UNK10 | AnimSlotFlag.UNK7 | AnimSlotFlag.UNK5 |
                                AnimSlotFlag.UNK4 | AnimSlotFlag.UNK3);

                slot.animPath.maxPathLength = 0;
            }

            if (popFlags.HasFlag(AnimStateTransitionFlags.GOAL_INVALIDATE_PATH))
            {
                GameObjectBody mover = slot.path.mover;
                slot.animPath.flags = AnimPathFlag.UNK_1;
                slot.path.flags = default;
                GameSystems.Raycast.GoalDestinationsRemove(mover);
            }

            DecreaseActiveGoalCount(slot, newGoal);
            slot.goals.RemoveAt(slot.currentGoal);
            slot.currentGoal--;
            slot.currentState = 0;
            if (slot.IsStackEmpty)
            {
                if (!popFlags.HasFlag(AnimStateTransitionFlags.PUSH_GOAL))
                {
                    slot.flags |= AnimSlotFlag.STOP_PROCESSING;
                    //  Logger.Debug("Pop goal for {0}: stopping processing (last goal was {1}).", description.getDisplayName(slot.animObj), animGoalTypeNames[slot.pCurrentGoal.goalType]);
                }
            }
            else
            {
                var prevGoal = slot.goals[slot.currentGoal];
                //Logger.Debug("Popped goal {0}, new goal is {1}", animGoalTypeNames[slot.pCurrentGoal.goalType], animGoalTypeNames[prevGoal.goalType]);
                slot.pCurrentGoal = newCurrentGoal = slot.goals[slot.currentGoal];
                newGoal = Goals.GetByType(newCurrentGoal.goalType);
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

        private bool _processingActionCallbacks;

        [TempleDllLocation(0x10016a30)]
        private void ProcessActionCallbacks()
        {
            if (_processingActionCallbacks)
            {
                // Any added callbacks will be processed at the end of the already active call to this function
                return;
            }

            _processingActionCallbacks = true;
            try
            {
                // changed to manual iteration because PerformOnAnimComplete can alter the vector
                for (var i = 0; i < _actionCallbacks.Count; i++)
                {
                    var callback = _actionCallbacks[i];
                    GameSystems.D20.Actions.PerformOnAnimComplete(callback.obj, callback.uniqueId);

                    _actionCallbacks[i] = new AnimActionCallback(null, 0);
                }

                _actionCallbacks.Clear();
            }
            finally
            {
                _processingActionCallbacks = false;
            }
        }

        [TempleDllLocation(0x10016a00)]
        private void PushActionCallback(AnimSlot slot)
        {
            if (slot.uniqueActionId == 0)
            {
                return;
            }

            _actionCallbacks.Add(new AnimActionCallback(slot.animObj, slot.uniqueActionId));
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
            slot.param1 = GetAnimParam(slot, (AnimGoalProperty)state.argInfo1);
            slot.param2 = GetAnimParam(slot, (AnimGoalProperty)state.argInfo2);
            return true;
        }

        private AnimParam GetAnimParam(AnimSlot slot, AnimGoalProperty property)
        {
            if ((int)property == -1)
            {
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

            return (goal.field_10 & 1) != 0 || (slot.pCurrentGoal.scratchVal1.number & 0x80) != 0;
        }


        [TempleDllLocation(0x1000c890)]
        public bool InterruptAll()
        {
            foreach (var slot in mSlots)
            {
                if (!InterruptGoals(slot, AnimGoalPriority.AGP_7))
                {
                    Logger.Warn("Failed to interrupt goals for slot {0}", slot.id);
                    return false;
                }
            }

            return true;
        }

        [TempleDllLocation(0x1000c7e0)]
        public bool Interrupt(GameObjectBody obj, AnimGoalPriority priority, bool all = false)
        {
            var lastSlot = -1;
            if (priority < AnimGoalPriority.AGP_NONE || priority >= AnimGoalPriority.AGP_MAX)
            {
                throw new ArgumentOutOfRangeException("Invalid anim priority: " + priority);
            }

            if (all)
            {
                priority = AnimGoalPriority.AGP_NONE;
            }

            var slotIdx = GetFirstRunSlotIdxForObj(obj);
            if (slotIdx == -1)
                return true;

            while (slotIdx != lastSlot)
            {
                lastSlot = slotIdx;
                if (slotIdx != -1 && !InterruptGoals(mSlots[slotIdx], priority))
                {
                    return false;
                }

                slotIdx = GetNextRunSlotIdxForObj(obj, slotIdx); // FindNextSlotIdx
                if (slotIdx == -1)
                    return true;
            }

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
                newGoal.animIdPrevious.number = obj.GetIdleAnimId();
                if (!PushGoal(newGoal, out var slotId))
                    return false;

                GetSlot(slotId).goals[0].soundStreamId = -1;
                return true;
            }
        }

        [TempleDllLocation(0x1000c750)]
        public void SetAllGoalsClearedCallback(Action callback)
        {
            mAllGoalsClearedCallback = callback;
        }

        [TempleDllLocation(0x1000c950)]
        public void InterruptAllForTbCombat()
        {
            for (var i = mSlots.Count - 1; i >= 0; i--)
            {
                var slot = mSlots[i];
                if (slot.IsActive
                    && slot.animObj.type != ObjectType.portal /* ... why???? */
                    && !CurrentGoalHasField10_1(slot) /* continue in combat?? */
                    && !InterruptGoals(slot, AnimGoalPriority.AGP_3))
                {
                    Logger.Warn("Failed to interrupt animation slot for {0}", slot.animObj);
                }
            }
        }

        [TempleDllLocation(0x1001aaa0)]
        [TempleDllLocation(0x1001aa40)]
        public void NotifySpeedRecalc(GameObjectBody obj)
        {
            var slotId = GetFirstRunSlotId(obj);
            if (!slotId.IsNull)
            {
                var slot = GetSlot(slotId);
                slot.flags |= AnimSlotFlag.SPEED_RECALC;
            }
        }

        [TempleDllLocation(0x1001cab0)]
        public void SaveGame(SavedGameState savedGameState)
        {
            savedGameState.AnimState = new SavedAnimState
            {
                NextUniqueId = _nextUniqueActionId,
                ActiveGoalCount = mActiveGoalCount,
// TODO: -animcatchup
                NextUniqueActionId = _nextUniqueActionId,
                Slots = SaveSlots()
            };
        }

        private Dictionary<int, SavedAnimSlot> SaveSlots()
        {
            var result = new Dictionary<int, SavedAnimSlot>(mSlots.Count);
            foreach (var animSlot in mSlots)
            {
                result[animSlot.id.slotIndex] = SaveSlot(animSlot);
            }

            return result;
        }

        [TempleDllLocation(0x1001ac10)]
        private SavedAnimSlot SaveSlot(AnimSlot animSlot)
        {
            return new SavedAnimSlot
            {
                Id = animSlot.id,
                CurrentState = animSlot.currentState,
                Flags = animSlot.flags,
                Field14 = animSlot.field_14,
                AnimatedObject = GameSystems.MapObject.CreateFrozenRef(animSlot.animObj),
                Goals = SaveGoals(animSlot.goals),
                AnimPath = SaveAnimPath(animSlot.animPath),
                PathPauseTime = animSlot.path.PauseTime,
                NextTriggerTime = animSlot.nextTriggerTime,
                GameTimeSth = animSlot.gametimeSth,
                CurrentPing = animSlot.currentPing
            };
        }

        private List<SavedAnimGoal> SaveGoals(List<AnimSlotGoalStackEntry> goals)
        {
            var result = new List<SavedAnimGoal>(goals.Count);
            foreach (var goal in goals)
            {
                result.Add(new SavedAnimGoal
                {
                    Type = goal.goalType,
                    // NOTE: This is not quite how Vanilla does it, they do prefer the already frozen refs
                    // My reasoning here is that if the handles were stale, the game would have already crashed
                    Self = GameSystems.MapObject.CreateFrozenRef(goal.self.obj),
                    Target = GameSystems.MapObject.CreateFrozenRef(goal.target.obj),
                    Block = GameSystems.MapObject.CreateFrozenRef(goal.block.obj),
                    Scratch = GameSystems.MapObject.CreateFrozenRef(goal.scratch.obj),
                    Parent = GameSystems.MapObject.CreateFrozenRef(goal.parent.obj),
                    TargetTile = goal.targetTile.location.location,
                    Range = goal.range.location.location,
                    AnimId = goal.animId.number,
                    AnimIdPrevious = goal.animIdPrevious.number,
                    AnimData = goal.animData.number,
                    SpellData = goal.spellData.number,
                    SkillData = goal.flagsData.number,
                    ScratchVal1 = goal.scratchVal1.number,
                    ScratchVal2 = goal.scratchVal2.number,
                    ScratchVal3 = goal.scratchVal3.number,
                    ScratchVal4 = goal.scratchVal4.number,
                    ScratchVal5 = goal.scratchVal5.number,
                    ScratchVal6 = goal.scratchVal6.number,
                    // Saving and restoring these seems incredibly bogus by the way... (stale handles)
                    SoundHandle = goal.soundHandle.number,
                    SoundStreamId = goal.soundStreamId,
                    SoundStreamId2 = goal.soundStreamId2
                });
            }

            return result;
        }

        private SavedAnimPath SaveAnimPath(AnimPath animPath)
        {
            return new SavedAnimPath
            {
                Flags = animPath.flags,
                Deltas = animPath.deltas.ToArray(),
                Range = animPath.range,
                FieldD0 = animPath.fieldD0,
                FieldD4 = animPath.fieldD4,
                DeltaIdxMax = animPath.deltaIdxMax,
                FieldDC = animPath.fieldDC,
                MaxPathLength = animPath.maxPathLength,
                FieldE4 = animPath.fieldE4,
                ObjectLoc = animPath.objLoc,
                TargetLoc = animPath.tgtLoc
            };
        }

        [TempleDllLocation(0x1001d250)]
        public void LoadGame(SavedGameState savedGameState)
        {
            animSysIsLoading = true;
            try
            {
                LoadGame(savedGameState.AnimState);
            }
            finally
            {
                animSysIsLoading = false;
            }
        }

        [TempleDllLocation(0x1001cda0)]
        private void LoadGame(SavedAnimState animState)
        {
            // We assume anims have been cleared using Reset()
            Trace.Assert(mSlots.Count == 0);

            nextUniqueId = animState.NextUniqueId;
            mActiveGoalCount = animState.ActiveGoalCount;
            // TODO: UseAbsoluteTime (functionality not implemented yet, see 0x10307538)
            _nextUniqueActionId = animState.NextUniqueActionId;

            foreach (var (slotIndex, savedSlot) in animState.Slots)
            {
                var slot = LoadSlot(savedSlot);
                if (slot != null)
                {
                    EnsureSlotIndexValid(slotIndex);
                    mSlots[slotIndex] = slot;
                }
            }
        }

        private void EnsureSlotIndexValid(int slotIndex)
        {
            // TODO: This sucks hard, it probably needs to be a dictionary...
            var slotsAlloced = new List<AnimSlotId>();
            while (mSlots.Count < slotIndex + 1)
            {
                slotsAlloced.Add(AllocSlot());
            }

            foreach (var slotId in slotsAlloced)
            {
                GetSlot(slotId).Clear();
            }
        }

        [TempleDllLocation(0x1001ada0)]
        private AnimSlot LoadSlot(SavedAnimSlot savedSlot)
        {
            var animSlot = new AnimSlot();
            animSlot.id = savedSlot.Id;
            animSlot.flags = savedSlot.Flags;
            animSlot.currentState = savedSlot.CurrentState;
            animSlot.field_14 = savedSlot.Field14;
            animSlot.animObj = GameSystems.Object.GetObject(savedSlot.AnimatedObject.guid);
            if (animSlot.animObj == null)
            {
                Logger.Error("Failed to load anim slot because animated object {0} could not be found.",
                    savedSlot.AnimatedObject);
                return null;
            }

            animSlot.currentGoal = savedSlot.Goals.Count - 1;

            // Restore the individual goals
            animSlot.goals.Capacity = savedSlot.Goals.Count;
            foreach (var savedGoal in savedSlot.Goals)
            {
                animSlot.goals.Add(LoadGoal(savedGoal));
            }

            animSlot.pCurrentGoal = animSlot.goals[^1];

            LoadAnimPath(savedSlot.AnimPath, ref animSlot.animPath);

            animSlot.path.PauseTime = savedSlot.PathPauseTime;

            animSlot.nextTriggerTime = savedSlot.NextTriggerTime;
            animSlot.gametimeSth = savedSlot.GameTimeSth;
            animSlot.currentPing = savedSlot.CurrentPing;
            return animSlot;
        }

        private void LoadAnimPath(SavedAnimPath savedAnimPath, ref AnimPath animPath)
        {
            animPath.flags = savedAnimPath.Flags;
            animPath.deltas = savedAnimPath.Deltas.ToArray();
            animPath.range = savedAnimPath.Range;
            animPath.fieldD0 = savedAnimPath.FieldD0;
            animPath.fieldD4 = savedAnimPath.FieldD4;
            animPath.deltaIdxMax = savedAnimPath.DeltaIdxMax;
            animPath.fieldDC = savedAnimPath.FieldDC;
            animPath.maxPathLength = savedAnimPath.MaxPathLength;
            animPath.fieldE4 = savedAnimPath.FieldE4;
            animPath.objLoc = savedAnimPath.ObjectLoc;
            animPath.tgtLoc = savedAnimPath.TargetLoc;
        }

        private AnimSlotGoalStackEntry LoadGoal(SavedAnimGoal savedGoal)
        {
            var result = new AnimSlotGoalStackEntry();
            result.goalType = savedGoal.Type;
            result.selfTracking = savedGoal.Self;
            result.targetTracking = savedGoal.Target;
            result.blockTracking = savedGoal.Block;
            result.scratchTracking = savedGoal.Scratch;
            result.parentTracking = savedGoal.Parent;
            result.ValidateObjectRefs();

            result.targetTile.location = new LocAndOffsets(savedGoal.TargetTile);
            result.range.location = new LocAndOffsets(savedGoal.Range); // TODO: Most likely wrong
            result.animId.number = savedGoal.AnimId;
            result.animIdPrevious.number = savedGoal.AnimIdPrevious;
            result.animData.number = savedGoal.AnimData;
            result.spellData.number = savedGoal.SpellData;
            result.skillData.number = savedGoal.SkillData;
            result.flagsData.number = savedGoal.FlagsData;
            result.scratchVal1.number = savedGoal.ScratchVal1;
            result.scratchVal2.number = savedGoal.ScratchVal2;
            result.scratchVal3.number = savedGoal.ScratchVal3;
            result.scratchVal4.number = savedGoal.ScratchVal4;
            result.scratchVal5.number = savedGoal.ScratchVal5;
            result.scratchVal6.number = savedGoal.ScratchVal6;
            result.soundHandle.number = savedGoal.SoundHandle;
            result.soundStreamId = savedGoal.SoundStreamId;
            result.soundStreamId2 = savedGoal.SoundStreamId2;
            return result;
        }

        [TempleDllLocation(0x10054dd0)]
        [TempleDllLocation(0x1000c120)]
        public void Reset()
        {
            mSlots.Clear();
            mActiveGoalCount = 0;
            slotsInUse = 0;
            _nextUniqueActionId = 1;
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
        public bool IsRunningGoal(GameObjectBody obj, AnimGoalType animGoalType, out AnimSlotId runId)
        {
            return IsRunningGoal(obj, animGoalType, out runId, out _);
        }

        public bool IsRunningGoal(GameObjectBody obj, AnimGoalType animGoalType, out AnimSlotId runId,
            out int goalIndex)
        {
            if (obj == null)
            {
                runId = AnimSlotId.Null;
                goalIndex = -1;
                return false;
            }

            var goal = Goals.GetByType(animGoalType);
            var searchForType = animGoalType;

            // Prefer direct matches, but also look for other types
            for (int i = 0; i <= goal.relatedGoal.Count; i++)
            {
                // Substitute the goal type
                if (i > 0)
                {
                    var related = goal.relatedGoal[i - 1];
                    searchForType = related;
                }

                foreach (var slot in EnumerateSlots(obj))
                {
                    for (var index = 0; index < slot.goals.Count; index++)
                    {
                        var runningGoal = slot.goals[index];
                        if (runningGoal.goalType == searchForType)
                        {
                            runId = slot.id;
                            goalIndex = index;
                            return true;
                        }
                    }
                }
            }

            runId = AnimSlotId.Null;
            goalIndex = -1;
            return false;
        }

        [TempleDllLocation(0x1001d220)]
        public bool PushAttackOther(GameObjectBody attacker, GameObjectBody target)
        {
            var random = GameSystems.Random.GetInt(0, 2);
            return PushAttack(attacker, target, -1, random, false, false);
        }

        [TempleDllLocation(0x100154a0)]
        private static bool CritterCanAnimate(GameObjectBody obj)
        {
            return obj != null && !GameSystems.Critter.IsDeadOrUnconscious(obj);
        }

        [TempleDllLocation(0x10015ee0)]
        public bool PushPleaseMove(GameObjectBody critter, GameObjectBody critter2)
        {
            var movingCritter = critter2;
            if (critter2 == null)
            {
                return false;
            }

            var movingForCritter = critter;
            if (critter2 == critter)
            {
                return false;
            }

            if (critter != null)
            {
                if (critter2.IsPC())
                {
                    // Position in party is tie-breaker for the PCs
                    // Previously the numeric value of the objhnd was compared here, which we can no longer do
                    if (critter.IsPC())
                    {
                        var critterIdx = GameSystems.Party.IndexOf(critter);
                        var critter2Idx = GameSystems.Party.IndexOf(critter2);
                        if (critter2Idx < critterIdx)
                        {
                            movingForCritter = critter2;
                            movingCritter = critter;
                        }
                    }
                    else
                    {
                        // critter is an NPC and should move for critter2 (the PC)
                        movingForCritter = critter2;
                        movingCritter = critter;
                    }
                }
            }

            if (!CritterCanAnimate(movingCritter))
            {
                return false;
            }

            if (anim_get_slot_with_fieldc_goal(movingCritter, out _))
            {
                return false;
            }

            var stackEntry = new AnimSlotGoalStackEntry(movingCritter, AnimGoalType.please_move, true);
            stackEntry.target.obj = movingForCritter;
            PushGoal(stackEntry, out _);
            return true;
        }

        [TempleDllLocation(0x10054fd0)]
        private bool anim_get_slot_with_fieldc_goal(GameObjectBody handle, out AnimSlotId a2)
        {
            if (handle == null)
            {
                a2 = AnimSlotId.Null;
                return false;
            }

            for (int idx = GetFirstRunSlotIdxForObj(handle); idx != -1; idx = GetNextRunSlotIdxForObj(handle, idx))
            {
                var slot = mSlots[idx];
                var goal = Goals.GetByType(slot.goals[0].goalType);
                if (!goal.PersistOnAreaTransition)
                {
                    a2 = slot.id;
                    return true;
                }
            }

            a2 = AnimSlotId.Null;
            return false;
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

        public AnimSlot GetSlot(GameObjectBody obj)
        {
            var idx = GetFirstRunSlotIdxForObj(obj);
            if (idx != -1)
            {
                return mSlots[idx];
            }

            return null;
        }

        [TempleDllLocation(0x1001c170)]
        [TempleDllLocation(0x1001a2e0)]
        public bool PushRunToTile(GameObjectBody obj, LocAndOffsets pos, PathQueryResult path = null)
        {
            if (obj == null || GameSystems.Critter.IsDeadOrUnconscious(obj) ||
                !obj.IsPC() && GameSystems.Reaction.GetLastReactionPlayer(obj) != null)
            {
                return false;
            }

            if (IsRunningGoal(obj, AnimGoalType.run_to_tile, out animIdGlobal))
            {
                var newgoal = new AnimSlotGoalStackEntry(obj, AnimGoalType.run_to_tile);
                newgoal.targetTile.location = pos;

                if (!Interrupt(obj, AnimGoalPriority.AGP_3, false) || !PushGoal(newgoal, out animIdGlobal))
                {
                    return true;
                }
            }
            else
            {
                var newgoal = new AnimSlotGoalStackEntry(obj, AnimGoalType.run_to_tile);
                newgoal.targetTile.location = pos;
                if (path != null)
                {
                    newgoal.targetTile.location = path.to;
                }
                else
                {
                    newgoal.targetTile.location = pos;
                }

                if (!Interrupt(obj, AnimGoalPriority.AGP_3, false) || !PushGoal(newgoal, out animIdGlobal))
                {
                    return false;
                }
            }

            var slot = GetSlot(animIdGlobal);
            if (slot != null)
            {
                if (path != null)
                {
                    slot.path = path;
                    GameSystems.Raycast.GoalDestinationsAdd(slot.path.mover, slot.path.to);
                    slot.field_14 = 0;
                }
                else
                {
                    slot.ClearPath();
                }
            }

            return true;
        }

        [TempleDllLocation(0x1001c1b0)]
        public bool PushRunNearTile(GameObjectBody actor, LocAndOffsets target, int radiusFeet)
        {
            if (actor == null
                || GameSystems.Critter.IsDeadOrUnconscious(actor)
                || actor.IsNPC() && GameSystems.Reaction.GetLastReactionPlayer(actor) != null)
            {
                return false;
            }

            if (!GameSystems.Anim.IsRunningGoal(actor, AnimGoalType.run_to_tile, out GameSystems.Anim.animIdGlobal))
            {
                var goalData = new AnimSlotGoalStackEntry(actor, AnimGoalType.run_near_tile, true);
                goalData.targetTile.location = target;
                goalData.animId.floatNum = radiusFeet;
                if (!GameSystems.Anim.PushGoal(goalData, out animIdGlobal))
                {
                    return false;
                }

                GameSystems.Anim.TurnOnRunning(animIdGlobal);
                return true;
            }

            // If the critter was already running near a tile, adjust the target
            var slot = GetSlot(GameSystems.Anim.animIdGlobal);
            slot.flags |= AnimSlotFlag.RUNNING;

            if (!slot.goals[0].targetTile.location.AlmostEquals(target))
            {
                slot.animPath.flags |= AnimPathFlag.UNK_4;
                slot.ClearPath();
                slot.goals[0].targetTile.location = target;
            }

            // Change vs. Vanilla, where this was not changed:
            slot.goals[0].animId.floatNum = radiusFeet;

            return true;
        }

        // should the game use the Running animation?
        [TempleDllLocation(0x10014750)]
        public bool ShouldRun(GameObjectBody obj)
        {
            // TODO: Checks for inputs should be moved out of the anim system

            var isAlwaysRun = Globals.Config.AlwaysRun;
            if (GameSystems.Party.IsInParty(obj))
            {
                var isCtrlPressed = Tig.Keyboard.IsKeyPressed(VirtualKey.VK_LCONTROL)
                                    || Tig.Keyboard.IsKeyPressed(VirtualKey.VK_RCONTROL);
                if (isAlwaysRun)
                {
                    if (isCtrlPressed)
                        return false;
                }
                else if (isCtrlPressed)
                {
                    return true;
                }
            }

            return isAlwaysRun;
        }

        [TempleDllLocation(0x1001d060)]
        public void PushMoveToTile(GameObjectBody obj, LocAndOffsets pos)
        {
            if (obj == null || GameSystems.Critter.IsDeadOrUnconscious(obj) ||
                !obj.IsPC() && GameSystems.Reaction.GetLastReactionPlayer(obj) != null)
            {
                return;
            }

            if (obj.IsPC() && ShouldRun(obj))
            {
                PushRunToTile(obj, pos);
                return;
            }

            var goalData = new AnimSlotGoalStackEntry(obj, AnimGoalType.move_to_tile);
            goalData.targetTile.location = pos;

            Interrupt(obj, AnimGoalPriority.AGP_3, false);
            PushGoal(goalData, out var animId);

            // Reset the path, because the goal doesn't do it
            // NOTE: Vanilla did not do this, effectively borking this function when trying to cancel ongoing
            // movement. But in Vanilla, the "always walk" option was always true and only changeable via
            // the config file.
            var slot = GetSlot(animId);
            slot?.ClearPath();
        }

        [TempleDllLocation(0x1001a930)]
        public void TurnOnRunning()
        {
            TurnOnRunning(animIdGlobal);
        }

        [TempleDllLocation(0x1001a8e0)]
        public void TurnOnRunning(AnimSlotId slotId)
        {
            var slot = GetSlot(slotId);
            if (slot != null)
            {
                slot.flags |= AnimSlotFlag.RUNNING;
            }
            else
            {
                Logger.Info("Failed to turn on running for slot: {0}", slotId);
            }
        }

        [TempleDllLocation(0x1001ca80)]
        public void TurnOn100(AnimSlotId slotId)
        {
            var slot = GetSlot(slotId);
            if (slot != null)
            {
                slot.flags |= AnimSlotFlag.UNK_100;
            }
            else
            {
                Logger.Info("Failed to turn on flag 0x100 for slot: {0}", slotId);
            }
        }

        [TempleDllLocation(0x1001aa10)]
        public void TurnOn4000(AnimSlotId slotId)
        {
            var slot = GetSlot(slotId);
            if (slot != null)
            {
                slot.flags |= AnimSlotFlag.UNK9;
            }
            else
            {
                Logger.Info("Failed to turn on flag 0x4000 for slot: {0}", slotId);
            }
        }

        [TempleDllLocation(0x10055060)]
        public bool HasAttackAnim(GameObjectBody handle, GameObjectBody target)
        {
            if (!IsRunningGoal(handle, AnimGoalType.attack, out var slotId))
            {
                return false;
            }

            // Check the target, if that was requested
            if (target != null)
            {
                var slot = GetRunSlot(slotId.slotIndex);

                var targetCheck = GameSystems.Critter.GetLeaderRecursive(target);
                if (targetCheck == null)
                {
                    targetCheck = target;
                }

                var actual = GameSystems.Critter.GetLeaderRecursive(slot.goals[0].target.obj);
                if (actual == null)
                {
                    actual = slot.goals[0].target.obj;
                }

                // We have an attack goal, but not on the right target
                if (targetCheck != actual)
                {
                    return false;
                }
            }

            return true;
        }

        [TempleDllLocation(0x1001aaf0)]
        public int GetGoalSubslotsInUse(AnimSlotId animSlotId)
        {
            return GetSlot(animSlotId).currentGoal;
        }

        [TempleDllLocation(0x1001ab40)]
        public bool IsAnimatingForever(AnimSlotId animSlotId)
        {
            var slot = GetSlot(animSlotId);
            var currentGoalType = slot.pCurrentGoal.goalType;

            return currentGoalType == AnimGoalType.anim_fidget || currentGoalType == AnimGoalType.animate_loop;
        }


        [TempleDllLocation(0x10056a50)]
        public bool AddSubGoal(AnimSlotId id, AnimSlotGoalStackEntry stackEntry)
        {
            Trace.Assert(stackEntry.goalType >= 0 && stackEntry.goalType < AnimGoalType.count);
            // Previously, ToEE had a failsafe here which might actually not have worked anyway (for null anim ids)
            // But checking the code, this function is never called with a null anim id anyway
            Trace.Assert(!id.IsNull);

            var slot = GetSlot(id);
            if (slot == null)
            {
                Logger.Error("Cannot add subgoal to invalid animation slot {0}", id);
                return false;
            }

            if (slot.IsStackFull)
            {
                return false;
            }

            Trace.Assert(!slot.IsStackEmpty);

            // Since this is "prepending" to the stack
            // We have to move all stack entries backwards
            if (++slot.currentGoal > 0)
            {
                for (int i = slot.currentGoal; i >= 1; i--)
                {
                    slot.goals[i] = slot.goals[i - 1];
                }
            }

            slot.pCurrentGoal = slot.goals[slot.currentGoal];

            slot.goals[0] = stackEntry;
            slot.goals[0].FreezeObjectRefs();

            if (slot.field_14 != -1)
            {
                ++slot.field_14;
            }

            IncreaseActiveGoalCount(slot, Goals.GetByType(stackEntry.goalType));

            return true;
        }

        [TempleDllLocation(0x10056460)]
        public AnimSlotId GetSlotForGoalAndObjs(GameObjectBody handle, AnimSlotGoalStackEntry goalData)
        {
            // Iterate over all slots belonging to the object
            foreach (var slot in EnumerateSlots(handle))
            {
                var firstGoalState = slot.goals[0];
                if (!IsEquivalentGoalType(goalData.goalType, firstGoalState.goalType))
                {
                    continue;
                }

                if (firstGoalState.self.obj == goalData.self.obj
                    && firstGoalState.target.obj == goalData.target.obj
                    && firstGoalState.block.obj == goalData.block.obj
                    && firstGoalState.scratch.obj == goalData.scratch.obj
                    && firstGoalState.parent.obj == goalData.parent.obj)
                {
                    return slot.id;
                }
            }

            return AnimSlotId.Null;
        }

        private bool IsEquivalentGoalType(AnimGoalType expected, AnimGoalType actual)
        {
            if (expected == actual)
            {
                return true;
            }

            var goal = Goals.GetByType(expected);
            return goal.relatedGoal.Contains(actual);
        }

        [TempleDllLocation(0x1001ABB0)]
        public int GetActionAnimId(GameObjectBody animObj)
        {
            if (lastSlotPushedTo_.IsNull)
            {
                return 0;
            }

            var slot = GetSlot(lastSlotPushedTo_);
            if (slot == null || slot.animObj != animObj)
            {
                return 0;
            }

            // We actually assign an id here so the function name is a bit of a misnomer
            slot.uniqueActionId = _nextUniqueActionId++;
            return slot.uniqueActionId;
        }

        // Push death animation with randomly chosen anim
        public void PushDying(GameObjectBody critter)
        {
            EncodedAnimId deathAnim;
            switch (GameSystems.Random.GetInt(0, 2))
            {
                case 0:
                default:
                    deathAnim = new EncodedAnimId(NormalAnimType.Death);
                    break;
                case 1:
                    deathAnim = new EncodedAnimId(NormalAnimType.Death2);
                    break;
                case 2:
                    deathAnim = new EncodedAnimId(NormalAnimType.Death3);
                    break;
            }

            PushDying(critter, deathAnim);
        }

        [TempleDllLocation(0x100157b0)]
        public void PushDying(GameObjectBody critter, EncodedAnimId deathAnim)
        {
            var goal = new AnimSlotGoalStackEntry(critter, AnimGoalType.dying, true);
            goal.scratchVal2.number = deathAnim;
            PushGoal(goal, out animIdGlobal);
        }

        [TempleDllLocation(0x100158e0)]
        public void PushDodge(GameObjectBody attacker, GameObjectBody target)
        {
            if (!GameSystems.Critter.IsDeadOrUnconscious(target) && !GameSystems.Critter.IsProne(target))
            {
                if (Interrupt(target, AnimGoalPriority.AGP_4))
                {
                    var goalData = new AnimSlotGoalStackEntry(target, AnimGoalType.dodge, true);
                    GameSystems.Combat.EnterCombat(target);
                    goalData.target.obj = attacker;
                    goalData.scratchVal6.number = 5;
                    PushGoal(goalData, out animIdGlobal);
                }
            }
        }

        private const string DamageDefaultPrefix = "hit-UNSPECIFIED-";

        private static readonly Dictionary<DamageType, string> DamageParticleEffectPrefixes =
            new Dictionary<DamageType, string>
            {
                { DamageType.Acid, "hit-ACID-" },
                { DamageType.Cold, "hit-COLD-" },
                { DamageType.Electricity, "hit-SHOCK-" },
                { DamageType.Fire, "hit-FIRE-" },
                { DamageType.Sonic, "hit-SONIC-" },
                { DamageType.NegativeEnergy, "hit-NEGATIVE_ENERGY-" },
                { DamageType.Subdual, "hit-SUBDUAL-" },
                { DamageType.Poison, "hit-POISON-" },
                { DamageType.PositiveEnergy, "hit-POSITIVE_ENERGY-" },
                { DamageType.Force, "hit-FORCE-" }
            };

        [TempleDllLocation(0x10016A90)]
        public void PlayDamageEffect(GameObjectBody target, DamageType damageType, int damageAmount)
        {
            if (damageType == DamageType.BloodLoss)
            {
                return;
            }

            var partSysNamePattern = DamageParticleEffectPrefixes.GetValueOrDefault(damageType, DamageDefaultPrefix);
            string partSysName = null;
            if (damageAmount > 12)
            {
                partSysName = partSysNamePattern + "heavy";
            }

            if (damageAmount < 4)
            {
                partSysName = partSysNamePattern + "light";
            }

            if (partSysName == null || !GameSystems.ParticleSys.DoesNameExist(partSysName))
            {
                partSysName = partSysNamePattern + "medium";
            }

            GameSystems.ParticleSys.CreateAtObj(partSysName, target);
        }

        [TempleDllLocation(0x10015680)]
        [TempleDllLocation(0x1001a540)]
        public bool PushAttemptAttack(GameObjectBody attacker, GameObjectBody target)
        {
            Trace.Assert(attacker != target);

            var goal = new AnimSlotGoalStackEntry(attacker, AnimGoalType.attempt_attack);
            goal.target.obj = target;

            if (GetSlotForGoalAndObjs(attacker, goal).IsNull)
            {
                if (Interrupt(attacker, AnimGoalPriority.AGP_3))
                {
                    goal.scratchVal6.number = -1;
                    goal.animIdPrevious.number = -1;
                    return PushGoal(goal, out animIdGlobal);
                }
            }

            return false;
        }

        [TempleDllLocation(0x1007d340)]
        public bool PushUseSkillOn(GameObjectBody critter, GameObjectBody target, SkillId skillId)
        {
            return PushUseSkillOn(critter, target, null, skillId);
        }

        [TempleDllLocation(0x1001c690)]
        public bool PushUseSkillOn(GameObjectBody critter, GameObjectBody target, GameObjectBody scratch,
            SkillId skillId)
        {
            if (GameSystems.Critter.IsDeadOrUnconscious(critter))
            {
                return false;
            }

            var newgoal = new AnimSlotGoalStackEntry(critter, AnimGoalType.use_skill_on, true);
            newgoal.target.obj = target;
            newgoal.scratch.obj = scratch;
            newgoal.flagsData.number = (int)skillId;

            if (!PushGoal(newgoal, out animIdGlobal))
            {
                return false;
            }

            TurnOn4000(animIdGlobal);

            if (critter.IsNPC())
            {
                TurnOnRunning(animIdGlobal);
            }
            else
            {
                if (ShouldRun(critter))
                {
                    TurnOnRunning(animIdGlobal);
                }
            }

            return true;
        }

        [TempleDllLocation(0x10015290)]
        public bool PushAnimate(GameObjectBody critter, NormalAnimType animType)
        {
            var goal = new AnimSlotGoalStackEntry(critter, AnimGoalType.animate, true);
            goal.animIdPrevious.number = new EncodedAnimId(animType);
            return PushGoal(goal, out _);
        }

        public bool PushAnimate(GameObjectBody critter, EncodedAnimId animId)
        {
            var goal = new AnimSlotGoalStackEntry(critter, AnimGoalType.animate, true);
            // If the desired animation is weapon-dependent, refresh the actual weapon types now
            if (animId.IsWeaponAnim())
            {
                animId = GameSystems.Critter.GetAnimId(critter, animId.GetWeaponAnim());
            }

            goal.animIdPrevious.number = animId;
            return PushGoal(goal, out _);
        }

        [TempleDllLocation(0x1008d590)]
        public bool PushSpellInterrupt(GameObjectBody caster, GameObjectBody item, AnimGoalType animGoalType,
            SchoolOfMagic spellSchool)
        {
            AnimSlotGoalStackEntry goalData = new AnimSlotGoalStackEntry(caster, animGoalType, true);
            // I would expect the caster to just be this:
            Trace.Assert(caster == GameSystems.D20.Actions.CurrentSequence.spellPktBody.caster);
            goalData.target.obj = GameSystems.D20.Actions.CurrentSequence.spellPktBody.caster;
            goalData.skillData.number = 0;

            if (!GameSystems.Item.UsesWandAnim(item))
            {
                goalData.animIdPrevious.number = GameSystems.Spell.GetSpellSchoolAnimId(spellSchool);
            }
            else
            {
                goalData.animIdPrevious.number = GameSystems.Spell.GetAnimIdWand(spellSchool);
            }

            return goalData.Push(out _);
        }

        [TempleDllLocation(0x100153e0)]
        public bool PushRotate(GameObjectBody obj, float rotation)
        {
            var shortestRotation = Angles.ShortestAngleBetween(obj.Rotation, rotation);
            if (MathF.Abs(shortestRotation) < Angles.ToRadians(0.1f))
            {
                return false;
            }

            if (GameSystems.Critter.IsDeadOrUnconscious(obj))
            {
                return false;
            }

            var newGoal = new AnimSlotGoalStackEntry(obj, AnimGoalType.rotate, true);
            newGoal.scratchVal2.floatNum = rotation;
            return PushGoal(newGoal, out animIdGlobal);
        }

        [TempleDllLocation(0x10079790)]
        public bool PushSpellCast(SpellPacketBody spellPkt, GameObjectBody item)
        {
            // note: the original included the spell ID generation & registration, this is separated here.
            var caster = spellPkt.caster;
            var goalData = new AnimSlotGoalStackEntry(caster, AnimGoalType.throw_spell_w_cast_anim, true);

            goalData.skillData.number = spellPkt.spellId;

            SpellEntry spEntry = GameSystems.Spell.GetSpellEntry(spellPkt.spellEnum);

            // if self-targeted spell
            if (spEntry.IsBaseModeTarget(UiPickerType.Single) && spellPkt.Targets.Length == 0)
            {
                goalData.target.obj = spellPkt.caster;

                if (spellPkt.aoeCenter.location == locXY.Zero)
                {
                    goalData.targetTile.location = caster.GetLocationFull();
                }
                else
                {
                    goalData.targetTile.location = spellPkt.aoeCenter;
                }
            }

            else
            {
                var tgt = spellPkt.Targets[0].Object;
                goalData.target.obj = tgt;
                if (tgt != null && spellPkt.aoeCenter.location == locXY.Zero)
                {
                    goalData.targetTile.location = tgt.GetLocationFull();
                }
                else
                {
                    goalData.targetTile.location = spellPkt.aoeCenter;
                }
            }

            if (GameSystems.Item.UsesWandAnim(item))
            {
                goalData.animIdPrevious.number = GameSystems.Spell.GetAnimIdWand(spEntry.spellSchoolEnum);
            }
            else
            {
                goalData.animIdPrevious.number = GameSystems.Spell.GetSpellSchoolAnimId(spEntry.spellSchoolEnum);
            }

            return goalData.Push(out _);
        }

        /// <param name="soundId">Not quite clear when this is played...</param>
        /// <returns></returns>
        [TempleDllLocation(0x1001c370)]
        public bool PushAttack(GameObjectBody attacker, GameObjectBody target, int soundId, int attackAnimIdx,
            bool playCrit, bool useSecondaryAnim)
        {
            if (attacker == target)
            {
                return false;
            }

            if (!CritterCanAnimate(attacker))
            {
                return false;
            }

            var goalStackEntry = new AnimSlotGoalStackEntry(attacker, AnimGoalType.attack);
            if (useSecondaryAnim)
            {
                goalStackEntry.scratchVal1.number |= 0x10000;
            }

            goalStackEntry.target.obj = target;


            if (GetSlotForGoalAndObjs(attacker, goalStackEntry).IsNull)
            {
                if (Interrupt(attacker, AnimGoalPriority.AGP_3))
                {
                    GameSystems.SoundGame.StartCombatMusic(attacker);
                    goalStackEntry.scratchVal6.number = soundId;

                    WeaponAnim weaponAnim;
                    if (!playCrit)
                    {
                        if (!useSecondaryAnim)
                        {
                            weaponAnim = (WeaponAnim)(attackAnimIdx + 1);
                        }
                        else
                        {
                            weaponAnim = (WeaponAnim)(attackAnimIdx + 4);
                        }
                    }
                    else
                    {
                        goalStackEntry.scratchVal1.number |= 0x8000;
                        weaponAnim = useSecondaryAnim ? WeaponAnim.LeftCriticalSwing : WeaponAnim.RightCriticalSwing;
                    }

                    goalStackEntry.animIdPrevious.number = (int)weaponAnim;
                    Logger.Info("Attack animation: {0}", weaponAnim);

                    if (PushGoal(goalStackEntry, out animIdGlobal))
                    {
                        if (!attacker.IsNPC())
                        {
                            if (!ShouldRun(attacker))
                            {
                                return true;
                            }
                        }

                        TurnOnRunning(animIdGlobal);
                        return true;
                    }
                }
            }

            GetOffMyLawn(attacker);
            return false;
        }

        [TempleDllLocation(0x1001c530)]
        public bool PushThrowWeapon(GameObjectBody attacker, GameObjectBody target, int scratchVal6, in bool secondary)
        {
            if (attacker == target)
            {
                return false;
            }

            if (!CritterCanAnimate(attacker))
            {
                return false;
            }

            var goal = new AnimSlotGoalStackEntry(attacker, AnimGoalType.attack);
            goal.target.obj = target;
            if (secondary)
            {
                goal.scratchVal1.number |= 0x10000;
            }

            if (!GetSlotForGoalAndObjs(attacker, goal).IsNull
                || !Interrupt(attacker, AnimGoalPriority.AGP_3))
            {
                GetOffMyLawn(attacker);
                return false;
            }

            GameSystems.SoundGame.StartCombatMusic(attacker);
            goal.scratchVal6.number = scratchVal6;
            goal.animIdPrevious.number = (int)(secondary ? WeaponAnim.LeftThrow : WeaponAnim.RightThrow);

            if (!PushGoal(goal, out animIdGlobal))
            {
                return false;
            }

            if (attacker.IsNPC() || ShouldRun(attacker))
            {
                TurnOnRunning(animIdGlobal);
            }

            return true;
        }

        [TempleDllLocation(0x100159b0)]
        public bool PushThrowProjectile(GameObjectBody attacker, GameObjectBody projectile, in int missX, in int missY,
            GameObjectBody target, LocAndOffsets targetLoc, int scratchVal6)
        {
            if (projectile == null)
            {
                return false;
            }

            if (!Interrupt(projectile, AnimGoalPriority.AGP_4))
            {
                return false;
            }

            var goal = new AnimSlotGoalStackEntry(projectile, AnimGoalType.projectile);

            goal.target.location = targetLoc;
            goal.target.obj = target;
            goal.parent.obj = attacker;
            goal.scratch.obj = target;
            goal.scratchVal6.number = scratchVal6;
            return PushGoal(goal, out animIdGlobal);
        }

        [TempleDllLocation(0x10015760)]
        public bool PushGetUp(GameObjectBody critter)
        {
            var goal = new AnimSlotGoalStackEntry(critter, AnimGoalType.anim_get_up, true);
            return PushGoal(goal, out animIdGlobal);
        }

        [TempleDllLocation(0x10010e80)]
        public bool PickUpItemWithSound(GameObjectBody sourceObj, GameObjectBody targetObj)
        {
            if (sourceObj == null || targetObj == null)
            {
                return false;
            }

            // The item really needs to be on the ground (this is not a good check, usually check for ITEM flag)
            var currentParent = GameSystems.Item.GetParent(targetObj);
            if (currentParent != null && currentParent.ProtoId != 1000)
            {
                return false;
            }

            if (GameSystems.Party.IsInParty(sourceObj))
            {
                var soundId = GameSystems.SoundMap.CombatFindWeaponSound(targetObj, sourceObj, null, 0);
                GameSystems.SoundGame.PositionalSound(soundId, 1, sourceObj);
            }

            return GameSystems.Item.SetItemParent(targetObj, sourceObj, ItemInsertFlag.Use_Bags);
        }

        [TempleDllLocation(0x1001c9a0)]
        public bool PushTalk(GameObjectBody critter, GameObjectBody target)
        {
            if (critter == null || GameSystems.Critter.IsDeadOrUnconscious(critter))
            {
                return false;
            }

            var goal = new AnimSlotGoalStackEntry(critter, AnimGoalType.talk, true);
            goal.target.obj = target;
            if (!PushGoal(goal, out animIdGlobal))
            {
                return false;
            }

            GameSystems.Anim.TurnOn4000(animIdGlobal);
            return true;
        }

        [TempleDllLocation(0x100154f0)]
        public bool PushMoveNearTile(GameObjectBody critter, LocAndOffsets target, int tileRadius)
        {
            if (critter == null
                || GameSystems.Critter.IsDeadOrUnconscious(critter)
                || critter.IsNPC() && GameSystems.Reaction.GetLastReactionPlayer(critter) != null)
            {
                return false;
            }

            if (GameSystems.Anim.IsRunningGoal(critter, AnimGoalType.run_to_tile, out animIdGlobal))
            {
                var slot = GetSlot(animIdGlobal);
                if (slot.goals[0].targetTile.location.DistanceTo(target) <= 0.000001f)
                {
                    // The existing goal already has the right target
                    return true;
                }

                slot.animPath.flags |= AnimPathFlag.UNK_4;
                slot.path.flags &= PathFlags.PF_COMPLETE;

                GameSystems.Raycast.GoalDestinationsRemove(slot.path.mover);
                slot.goals[0].targetTile.location = target;
                return true;
            }

            var goal = new AnimSlotGoalStackEntry(critter, AnimGoalType.move_near_tile, true);
            goal.targetTile.location = target;
            goal.animId.number = tileRadius;
            return PushGoal(goal, out animIdGlobal);
        }

        [TempleDllLocation(0x1001a560)]
        public bool PushWander(GameObjectBody critter, locXY tetherLoc, int radius)
        {
            Trace.Assert(tetherLoc != locXY.Zero);
            Trace.Assert(radius > 0);
            Trace.Assert(critter != null && critter.IsCritter());

            if (GameSystems.Critter.IsDeadOrUnconscious(critter) || !GameSystems.Anim.GetFirstRunSlotId(critter).IsNull)
            {
                return false;
            }

            var sourceCritter = critter;
            if (FindCritterStandingInTheWay(ref sourceCritter, out var blockingCritter))
            {
                GameSystems.Anim.PushPleaseMove(blockingCritter, sourceCritter);
                return false;
            }

            var goalData = new AnimSlotGoalStackEntry(critter, AnimGoalType.wander);
            goalData.animId.number = radius;
            goalData.scratchVal2.number = tetherLoc.locx;
            goalData.scratchVal3.number = tetherLoc.locy;
            return PushGoal(goalData, out animIdGlobal);
        }

        [TempleDllLocation(0x1001a720)]
        public bool PushWanderSeekDarkness(GameObjectBody critter, locXY tetherLoc, int radius)
        {
            Trace.Assert(critter != null);
            Trace.Assert(tetherLoc != locXY.Zero);
            Trace.Assert(radius > 0);
            Trace.Assert(critter.IsCritter());

            if (!GameSystems.Critter.IsDeadOrUnconscious(critter) && GameSystems.Anim.GetFirstRunSlotId(critter).IsNull)
            {
                var pSourceObj = critter;
                if (FindCritterStandingInTheWay(ref pSourceObj, out var pBlockObj))
                {
                    GameSystems.Anim.PushPleaseMove(pBlockObj, pSourceObj);
                }
                else
                {
                    var goalData = new AnimSlotGoalStackEntry(critter, AnimGoalType.wander_seek_darkness, true);
                    goalData.animId.number = radius;
                    goalData.scratchVal2.number = tetherLoc.locx;
                    goalData.scratchVal3.number = tetherLoc.locy;
                    return PushGoal(goalData, out animIdGlobal);
                }
            }

            return false;
        }

        [TempleDllLocation(0x10015fd0)]
        public void GetOffMyLawn(GameObjectBody critterThatMoved)
        {
            Trace.Assert(critterThatMoved != null);

            using var crittersOnSameTile =
                ObjList.ListTile(critterThatMoved.GetLocation(), ObjectListFilter.OLC_CRITTERS);

            var countSharingTile = 0;
            foreach (var critter in crittersOnSameTile)
            {
                if (!GameSystems.Critter.IsDeadNullDestroyed(critter) &&
                    !anim_get_slot_with_fieldc_goal(critter, out _))
                {
                    ++countSharingTile;
                }
            }

            if (countSharingTile <= 1)
            {
                return;
            }

            // We're selecting a PC using a tie-breaker (Object ID ordering) to make sure
            // we're not pushing each other out of the way in an infinite loop.
            GameObjectBody smallestNpc = null;
            GameObjectBody smallestPc = null;

            var othersNeedToMove = new List<GameObjectBody>(countSharingTile);

            foreach (var critter in crittersOnSameTile)
            {
                if (!GameSystems.Critter.IsDeadNullDestroyed(critter) &&
                    !anim_get_slot_with_fieldc_goal(critter, out _))
                {
                    if (critter.IsNPC())
                    {
                        othersNeedToMove.Add(critter);
                        // We only need to find a tie-breaker NPC if no PCs are present
                        if (smallestPc == null)
                        {
                            if (smallestNpc != null)
                            {
                                if (critter.id < smallestNpc.id)
                                {
                                    smallestNpc = critter;
                                }
                            }
                            else
                            {
                                smallestNpc = critter;
                            }
                        }
                    }
                    else
                    {
                        if (smallestPc != null)
                        {
                            if (critter.id < smallestPc.id)
                            {
                                smallestPc = critter;
                            }
                        }
                        else
                        {
                            smallestPc = critter;
                        }
                    }
                }
            }

            // Fall back to the tie-breaker NPC if no PCs are present
            if (smallestPc == null)
            {
                smallestPc = smallestNpc;
            }

            foreach (var critter in othersNeedToMove)
            {
                if (smallestPc != critter)
                {
                    PushPleaseMove(smallestPc, critter);
                }
            }
        }

        [TempleDllLocation(0x10016210)]
        public bool FindCritterStandingInTheWay(ref GameObjectBody pSourceObj, out GameObjectBody pBlockObj)
        {
            Trace.Assert(pSourceObj != null);

            if (GameSystems.Anim.anim_get_slot_with_fieldc_goal(pSourceObj, out _))
            {
                pBlockObj = null;
                return false;
            }

            using var objListResult = ObjList.ListTile(pSourceObj.GetLocation(), ObjectListFilter.OLC_CRITTERS);

            var foundBlockers = 0;
            foreach (var otherCritter in objListResult)
            {
                if (!GameSystems.Critter.IsDeadNullDestroyed(otherCritter) &&
                    !GameSystems.Anim.anim_get_slot_with_fieldc_goal(otherCritter, out _))
                {
                    ++foundBlockers;
                }
            }

            if (foundBlockers <= 1)
            {
                pBlockObj = null;
                return false;
            }

            foreach (var otherCritter in objListResult)
            {
                if (GameSystems.Critter.IsDeadNullDestroyed(otherCritter)
                    || GameSystems.Anim.anim_get_slot_with_fieldc_goal(otherCritter, out _)
                    || !otherCritter.IsNPC()
                    || otherCritter == pSourceObj)
                {
                    continue;
                }

                // To prevent critters from shoving each other out of the way indefinitely,
                // we're using the following sorting criteria:
                // PCs before NPCs
                // Critters with "higher" IDs have precedence
                pBlockObj = otherCritter;
                if (pSourceObj.IsNPC())
                {
                    if (pBlockObj.IsNPC())
                    {
                        if (pSourceObj.id < pBlockObj.id)
                        {
                            pBlockObj = pSourceObj;
                            pSourceObj = otherCritter;
                        }
                    }
                }
                else if (pBlockObj.IsPC())
                {
                    if (pSourceObj.id < pBlockObj.id)
                    {
                        pBlockObj = pSourceObj;
                        pSourceObj = otherCritter;
                    }
                }

                return true;
            }

            pBlockObj = null;
            return false;
        }

        [TempleDllLocation(0x1000c500)]
        public AnimGoalPriority GetCurrentPriority(GameObjectBody handle)
        {
            for (var slotIdx = GetFirstRunSlotIdxForObj(handle);
                slotIdx != -1;
                slotIdx = GetNextRunSlotIdxForObj(handle, slotIdx))
            {
                var slot = mSlots[slotIdx];
                var goal = Goals.GetByType(slot.goals[0].goalType);

                if (!goal.interruptAll)
                {
                    return goal.priority;
                }
            }

            return AnimGoalPriority.AGP_NONE;
        }

        [TempleDllLocation(0x10056350)]
        public void InterruptGoalsByType(GameObjectBody handle, AnimGoalType type,
            AnimGoalType keep = (AnimGoalType)(-1))
        {
            // type is always ag_flee, keep is always -1 where we call it
            AnimGoalPriority interruptPriority;

            if (keep == (AnimGoalType)(-1))
            {
                interruptPriority = AnimGoalPriority.AGP_HIGHEST;
            }
            else
            {
                var goal = Goals.GetByType(keep);
                interruptPriority = goal.priority;
                Trace.Assert(interruptPriority >= AnimGoalPriority.AGP_NONE &&
                             interruptPriority <= AnimGoalPriority.AGP_HIGHEST);
                if (goal.interruptAll)
                {
                    interruptPriority = AnimGoalPriority.AGP_NONE;
                }
            }

            for (int slotIdx = GetFirstRunSlotIdxForObj(handle);
                slotIdx != -1;
                slotIdx = GetNextRunSlotIdxForObj(handle, slotIdx))
            {
                var slot = GetRunSlot(slotIdx);
                if (slot.goals[0].goalType == type || slot.pCurrentGoal.goalType == type)
                {
                    InterruptGoals(slot, interruptPriority);
                }
            }
        }

        [TempleDllLocation(0x10015820)]
        public bool PushHitByWeapon(GameObjectBody victim, GameObjectBody attacker)
        {
            if (attacker == null || victim == null || GameSystems.Critter.IsDeadOrUnconscious(victim))
            {
                return false;
            }

            if (!GameSystems.Anim.Interrupt(victim, AnimGoalPriority.AGP_4))
            {
                return false;
            }

            var goalData = new AnimSlotGoalStackEntry(victim, AnimGoalType.hit_by_weapon, true);
            GameSystems.Combat.EnterCombat(victim);
            goalData.target.obj = attacker;
            goalData.scratchVal6.number = 5;
            return PushGoal(goalData, out animIdGlobal);
        }

        [TempleDllLocation(0x10015e00)]
        public bool PushUnconceal(GameObjectBody critter)
        {
            if (critter == null || !critter.IsCritter() || !critter.IsConscious())
            {
                return false;
            }

            var goalData = new AnimSlotGoalStackEntry(critter, AnimGoalType.unconceal, true);

            return PushGoal(goalData, out animIdGlobal);
        }

        [TempleDllLocation(0x10056c10)]
        public void StopOngoingAttackAnimOnGroup(GameObjectBody attacker, GameObjectBody target)
        {
            foreach (var slot in EnumerateSlots(attacker))
            {
                var goalType = slot.goals[0].goalType;

                if (goalType == AnimGoalType.attack || goalType == AnimGoalType.attempt_attack)
                {
                    var attackedTarget = slot.goals[0].target.obj;
                    if (target == attackedTarget || target == GameSystems.Critter.GetLeaderRecursive(attackedTarget))
                    {
                        InterruptGoals(slot, AnimGoalPriority.AGP_5);
                        return;
                    }
                }
            }
        }

        [TempleDllLocation(0x10056c90)]
        public void StopOngoingAttackAnim(GameObjectBody attacker, GameObjectBody target)
        {
            foreach (var slot in EnumerateSlots(attacker))
            {
                var goalType = slot.goals[0].goalType;
                if (goalType == AnimGoalType.attack || goalType == AnimGoalType.attempt_attack)
                {
                    var attackedTarget = slot.goals[0].target.obj;
                    if (target == attackedTarget)
                    {
                        InterruptGoals(slot, AnimGoalPriority.AGP_5);
                        return;
                    }
                }
            }
        }

        [TempleDllLocation(0x10016f40)]
        public void InterruptAllExceptFidgetOrIdle()
        {
            foreach (var animSlot in mSlots)
            {
                if (animSlot.IsActive)
                {
                    var obj = animSlot.animObj;
                    // TODO: All this special casing junk needs to go
                    if (obj.type != ObjectType.portal)
                    {
                        Interrupt(obj, AnimGoalPriority.AGP_HIGHEST);
                    }
                }
            }
        }

        private const string AreaTransitionSaveFile = "Anim.dat";

        // TODO: Animation slots that have been saved should be stopped in my opinion, they might be automatically stopped by the interrupt that comes,
        //       But explicitly clearing or removing the saved slots here makes it more explicit and easier to follow.
        [TempleDllLocation(0x1001af30)]
        [TempleDllLocation(0x1001b2b0)]
        public void SaveToMap(int mapId, bool forDestinationMap)
        {
            var saveDir = GameSystems.Map.GetSaveDir(mapId);
            Directory.CreateDirectory(saveDir);

            var path = Path.Join(saveDir, AreaTransitionSaveFile);

            // Load the current state and keep it.
            // This only works because the file for a map is deleted everytime the current map changes to that map
            var savedSlots = new List<SavedAnimSlot>();
            if (File.Exists(path))
            {
                var savedState = MapAnimState.Load(path);
                savedSlots.AddRange(savedState.Slots);
                Logger.Info("Loaded {0} existing saved animations for map {1}", savedSlots.Count, mapId);
            }

            // Interrupt goals WILL modify the list.
            var slotsCopy = mSlots.ToArray();
            foreach (var slot in slotsCopy)
            {
                if (slot == null || !slot.IsActive)
                {
                    continue;
                }

                if (forDestinationMap)
                {
                    var goal = Goals.GetByType(slot.goals[0].goalType);
                    if (GameSystems.Teleport.IsTeleporting(slot.animObj) && goal.PersistOnAreaTransition)
                    {
                        savedSlots.Add(SaveSlot(slot));
                        continue;
                    }

                    InterruptGoals(slot, AnimGoalPriority.AGP_MAX);
                }
                else
                {
                    //TODO I have the hunch that this is NEVER called since before it can be called, all goals
                    // had been interrupted (see above), when the goals for the destination map are saved
                    savedSlots.Add(SaveSlot(slot));
                }
            }

            Logger.Info("Saving {0} animations for map {1} (Transition destination: {2})",
                savedSlots.Count, mapId, forDestinationMap);

            MapAnimState.Save(path, new MapAnimState
            {
                Slots = savedSlots
            });
        }

        /// <summary>
        /// NOTE: This is only used in area transitions.
        /// </summary>
        [TempleDllLocation(0x1001b5f0)]
        public void LoadFromMap(int mapId)
        {
            var path = Path.Join(GameSystems.Map.GetSaveDir(mapId), AreaTransitionSaveFile);
            if (!File.Exists(path))
            {
                // This is normal if the map was never visited
                Logger.Debug("Not loading animations on area transition, because the file does not exist: {0}", path);
                return;
            }

            var savedAnims = MapAnimState.Load(path);

            // Delete the saved animations, because otherwise we'd duplicate them on the next area transition here,
            // since saving to the map's transition file is done in append mode!
            File.Delete(path);

            foreach (var savedSlot in savedAnims.Slots)
            {
                // We do not actually need to keep the slot id intact, while Vanilla tried to,
                // it had fallback code to simply allocate a new one.
                var slot = LoadSlot(savedSlot);
                if (slot == null)
                {
                    continue;
                }

                // See if the original slot location is still free
                var slotIndex = savedSlot.Id.slotIndex;
                EnsureSlotIndexValid(slotIndex);

                if (mSlots[slotIndex] != null && mSlots[slotIndex].IsActive)
                {
                    Logger.Info("Slot index {0} on area transition was already taken. Allocating new.", slotIndex);
                    slot.id = AllocSlot();
                    slotIndex = slot.id.slotIndex;
                }

                mSlots[slotIndex] = slot;
            }

            Logger.Info("Loaded {0} saved anims from {1}", savedAnims.Slots.Count, path);
        }
    }
}