using System;
using System.Collections.Generic;
using System.Linq;
using SpicyTemple.Core.GFX;

namespace SpicyTemple.Core.Systems.Anim
{
    internal struct AnimGoalBuilder
    {
        private readonly AnimGoal _goal;

        public AnimGoalBuilder(AnimGoalType type)
        {
            _goal = new AnimGoal(type);
        }

        public AnimGoalBuilder SetPriority(AnimGoalPriority priority)
        {
            _goal.priority = priority;
            return this;
        }

        public AnimGoalBuilder SetFieldC(bool enable)
        {
            _goal.field_C = enable ? 1 : 0;
            return this;
        }

        public AnimGoalBuilder SetField10(bool enable)
        {
            _goal.field_10 = enable ? 1 : 0;
            return this;
        }

        public AnimGoalBuilder SetInterruptAll(bool enable)
        {
            _goal.interruptAll = enable;
            return this;
        }

        public AnimGoalCleanupBuilder AddCleanup(AnimGoalStateCallback callback)
        {
            _goal.state_special = new AnimGoalState
            {
                callback = callback
            };
            return new AnimGoalCleanupBuilder(this);
        }

        public AnimGoalStateBuilder AddState(AnimGoalStateCallback callback)
        {
            var index = _goal.statecount++;
            _goal.states.Add(new AnimGoalState
            {
                callback = callback
            });
            return new AnimGoalStateBuilder(this, index);
        }

        public AnimGoalBuilder SetRelatedGoals(params AnimGoalType[] types)
        {
            _goal.relatedGoal = types.ToList();
            return this;
        }

        public AnimGoal Build() => _goal;

        public struct AnimGoalStateBuilder
        {
            private readonly AnimGoalBuilder _goalBuilder;

            private readonly int _stateIndex;

            private AnimGoalState State
            {
                get => _goalBuilder._goal.states[_stateIndex];
                set => _goalBuilder._goal.states[_stateIndex] = value;
            }

            public AnimGoalStateBuilder(AnimGoalBuilder goalBuilder, int stateIndex)
            {
                _goalBuilder = goalBuilder;
                _stateIndex = stateIndex;
            }

            public AnimGoalStateBuilder SetArgs(AnimGoalProperty param1, AnimGoalProperty? param2 = null)
            {
                var state = State;
                state.argInfo1 = (int) param1;
                if (param2.HasValue)
                {
                    state.argInfo2 = (int) param2.Value;
                }
                else
                {
                    state.argInfo2 = -1;
                }

                State = state;

                return this;
            }

            public AnimGoalStateBuilder SetFlagsData(int flagsData)
            {
                var state = State;
                state.flagsData = flagsData;
                State = state;
                return this;
            }

            public AnimGoalStateBuilder OnSuccess(uint transition, int delay = 0)
            {
                var state = State;
                state.afterSuccess.newState = transition;
                state.afterSuccess.delay = delay;
                State = state;
                return this;
            }

            public AnimGoalStateBuilder OnFailure(uint transition, int delay = 0)
            {
                var state = State;
                state.afterFailure.newState = transition;
                state.afterFailure.delay = delay;
                State = state;
                return this;
            }
        };

        public struct AnimGoalCleanupBuilder
        {
            private readonly AnimGoalBuilder _goalBuilder;

            private AnimGoalState State
            {
                get => _goalBuilder._goal.state_special.Value;
                set => _goalBuilder._goal.state_special = value;
            }

            public AnimGoalCleanupBuilder(AnimGoalBuilder goalBuilder)
            {
                _goalBuilder = goalBuilder;
            }

            public AnimGoalCleanupBuilder SetArgs(AnimGoalProperty param1, AnimGoalProperty? param2 = null)
            {
                var state = State;
                state.argInfo1 = (int) param1;
                if (param2.HasValue)
                {
                    state.argInfo2 = (int) param2.Value;
                }
                else
                {
                    state.argInfo2 = -1;
                }

                State = state;

                return this;
            }

            public AnimGoalCleanupBuilder SetFlagsData(int flagsData)
            {
                var state = State;
                state.flagsData = flagsData;
                State = state;
                return this;
            }
        }
    }

    public class AnimationGoals
    {
        private const uint T_INVALIDATE_PATH = (uint) AnimStateTransitionFlags.GOAL_INVALIDATE_PATH;
        private const uint T_POP_GOAL = (uint) AnimStateTransitionFlags.POP_GOAL;
        private const uint T_POP_GOAL_TWICE = (uint) AnimStateTransitionFlags.POP_GOAL_TWICE;
        private const uint T_POP_ALL = (uint) AnimStateTransitionFlags.POP_ALL;
        private const uint T_REWIND = (uint) AnimStateTransitionFlags.REWIND;
        private static uint T_GOTO_STATE(uint nextState) => nextState + 1;

        private static uint T_PUSH_GOAL(AnimGoalType goalType)
        {
            return ((uint) AnimStateTransitionFlags.PUSH_GOAL) | ((uint) goalType);
        }

        private const int DELAY_CUSTOM = AnimStateTransition.DelayCustom;
        private const int DELAY_SLOT = AnimStateTransition.DelaySlot;
        private const int DELAY_RANDOM = AnimStateTransition.DelayRandom;

        private Dictionary<AnimGoalType, AnimGoal> _goals = new Dictionary<AnimGoalType,AnimGoal>();

        private void Add(AnimGoal goal)
        {
            if (_goals.ContainsKey(goal.Type))
            {
                throw new ArgumentException("Duplicate animation goal type: " + goal.Type);
            }

            _goals[goal.Type] = goal;
        }

        private static int PackAnimId(NormalAnimType animType)
        {
            return (int) animType + 64;
        }
        private static int PackAnimId(WeaponAnim animType)
        {
            return (int) animType;
        }

        public AnimationGoals()
        {
            // AnimGoalType.animate
            var animate = new AnimGoalBuilder(AnimGoalType.animate)
                .SetPriority(AnimGoalPriority.AGP_3)
                .SetFieldC(true);
            animate.AddCleanup(AnimGoalActions.GoalAnimateCleanup)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            animate.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            animate.AddState(AnimGoalActions.GoalUnconcealAnimate) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_GOAL, DELAY_SLOT);
            animate.AddState(AnimGoalActions.GoalIsConcealed) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.unconceal))
                .OnFailure(T_GOTO_STATE(3));
            animate.AddState(AnimGoalActions.GoalThrowItemPlayAnim) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_REWIND)
                .OnFailure(T_GOTO_STATE(4));
            animate.AddState(AnimGoalActions.GoalActionPerform3) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            Add(animate.Build());

            // AnimGoalType.animate_loop
            var animate_loop = new AnimGoalBuilder(AnimGoalType.animate_loop)
                .SetPriority(AnimGoalPriority.AGP_7)
                .SetField10(true);
            animate_loop.AddCleanup(AnimGoalActions.GoalFreeSoundHandle)
                .SetFlagsData(1);
            animate_loop.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(3))
                .OnFailure(T_GOTO_STATE(1));
            animate_loop.AddState(AnimGoalActions.GoalLoopWhileCloseToParty) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_REWIND, 800);
            animate_loop.AddState(AnimGoalActions.GoalAnimateFireDmgContinueAnim) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_REWIND, DELAY_SLOT);
            animate_loop.AddState(AnimGoalActions.GoalAnimateForever) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_ALL);
            Add(animate_loop.Build());

            // AnimGoalType.anim_idle
            var anim_idle = new AnimGoalBuilder(AnimGoalType.anim_idle)
                .SetPriority(AnimGoalPriority.AGP_7);
            anim_idle.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            anim_idle.AddState(AnimGoalActions.GoalContinueWithAnim) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_GOTO_STATE(3));
            anim_idle.AddState(AnimGoalActions.GoalStartIdleAnimIfCloseToParty) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_ALL);
            anim_idle.AddState(AlwaysSucceed) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_POP_ALL);
            Add(anim_idle.Build());

            // AnimGoalType.anim_fidget
            var anim_fidget = new AnimGoalBuilder(AnimGoalType.anim_fidget)
                .SetPriority(AnimGoalPriority.AGP_1);
            anim_fidget.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            anim_fidget.AddState(AnimGoalActions.GoalContinueWithAnim2) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_GOTO_STATE(4));
            anim_fidget.AddState(AnimGoalActions.GoalStartFidgetAnimIfCloseToParty) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_GOTO_STATE(3));
            anim_fidget.AddState(AnimGoalActions.GoalActionPerform3) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            anim_fidget.AddState(AnimGoalActions.GoalCritterShouldNotAutoAnimate) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_POP_ALL);
            Add(anim_fidget.Build());

            // AnimGoalType.move_to_tile
            var move_to_tile = new AnimGoalBuilder(AnimGoalType.move_to_tile)
                .SetPriority(AnimGoalPriority.AGP_2)
                .SetRelatedGoals(AnimGoalType.move_near_tile, AnimGoalType.run_to_tile);
            move_to_tile.AddCleanup(AnimGoalActions.GoalResetToIdleAnim)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            move_to_tile.AddState(AnimGoalActions.GoalParam1ObjCloseToParam2Loc) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_GOTO_STATE(1));
            move_to_tile.AddState(AnimGoalActions.GoalIsProne) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_GOTO_STATE(2));
            move_to_tile.AddState(AnimGoalActions.GoalIsConcealed) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.unconceal))
                .OnFailure(T_GOTO_STATE(3));
            move_to_tile.AddState(AnimGoalActions.GoalIsCurrentPathValid) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_GOTO_STATE(5));
            move_to_tile.AddState(AnimGoalActions.GoalIsRotatedTowardNextPathNode) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.attempt_move))
                .OnFailure(T_PUSH_GOAL(AnimGoalType.rotate));
            move_to_tile.AddState(AnimGoalActions.GoalCalcPathToTarget) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_REWIND)
                .OnFailure(T_GOTO_STATE(6));
            move_to_tile.AddState(AlwaysFail) // Index 6
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.move_to_pause) | T_REWIND, 1000)
                .OnFailure(T_POP_ALL);
            Add(move_to_tile.Build());

            // AnimGoalType.run_to_tile
            var run_to_tile = new AnimGoalBuilder(AnimGoalType.run_to_tile)
                .SetPriority(AnimGoalPriority.AGP_2)
                .SetRelatedGoals(AnimGoalType.move_near_tile, AnimGoalType.move_to_tile, AnimGoalType.run_near_tile);
            run_to_tile.AddCleanup(AnimGoalActions.GoalResetToIdleAnim)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            run_to_tile.AddState(AnimGoalActions.GoalSetRunningFlag) // Index 0
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_GOTO_STATE(1));
            run_to_tile.AddState(AnimGoalActions.GoalParam1ObjCloseToParam2Loc) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_GOTO_STATE(2));
            run_to_tile.AddState(AnimGoalActions.GoalIsProne) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_GOTO_STATE(3));
            run_to_tile.AddState(AnimGoalActions.GoalIsConcealed) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.unconceal))
                .OnFailure(T_GOTO_STATE(4));
            run_to_tile.AddState(AnimGoalActions.GoalIsCurrentPathValid) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(5))
                .OnFailure(T_GOTO_STATE(6));
            run_to_tile.AddState(AnimGoalActions.GoalIsRotatedTowardNextPathNode) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.attempt_move))
                .OnFailure(T_PUSH_GOAL(AnimGoalType.rotate));
            run_to_tile.AddState(AnimGoalActions.GoalCalcPathToTarget) // Index 6
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_REWIND)
                .OnFailure(T_GOTO_STATE(7));
            run_to_tile.AddState(AlwaysFail) // Index 7
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.move_to_pause) | T_REWIND, 1000)
                .OnFailure(T_POP_ALL);
            Add(run_to_tile.Build());

            // AnimGoalType.attempt_move
            var attempt_move = new AnimGoalBuilder(AnimGoalType.attempt_move)
                .SetPriority(AnimGoalPriority.AGP_2);
            attempt_move.AddCleanup(AnimGoalActions.GoalAttemptMoveCleanup)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            attempt_move.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_GOTO_STATE(1));
            attempt_move.AddState(AnimGoalActions.GoalHasDoorInPath) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(3));
            attempt_move.AddState(AnimGoalActions.GoalIsDoorFullyClosed) // Index 2
                .SetArgs(AnimGoalProperty.SCRATCH_OBJ)
                .OnSuccess(T_GOTO_STATE(7))
                .OnFailure(T_GOTO_STATE(3));
            attempt_move.AddState(AnimGoalActions.GoalMoveAlongPath) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_GOAL, DELAY_SLOT);
            attempt_move.AddState(AnimGoalActions.GoalIsCurrentPathValid) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(5))
                .OnFailure(T_GOTO_STATE(9));
            attempt_move.AddState(AnimGoalActions.GoalStateCallback7) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_GOTO_STATE(6));
            attempt_move.AddState(AnimGoalActions.GoalPlayMoveAnim) // Index 6
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND)
                .OnFailure(T_POP_ALL);
            attempt_move.AddState(AnimGoalActions.GoalPlayAnim) // Index 7
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(PackAnimId(WeaponAnim.Idle))
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.open_door) | T_REWIND)
                .OnFailure(T_POP_ALL);
            attempt_move.AddState(AlwaysSucceed) // Index 8
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(4294967295))
                .OnFailure(T_GOTO_STATE(4294967295));
            attempt_move.AddState(AlwaysFail) // Index 9
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.move_to_pause) | T_REWIND, 1000)
                .OnFailure(T_POP_ALL);
            attempt_move.AddState(null) // Index 10
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(0)
                .OnSuccess(T_GOTO_STATE(4294967295))
                .OnFailure(T_GOTO_STATE(4294967295));
            attempt_move.AddState(null) // Index 11
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(0)
                .OnSuccess(T_GOTO_STATE(4294967295))
                .OnFailure(T_GOTO_STATE(4294967295));
            Add(attempt_move.Build());

            // AnimGoalType.move_to_pause
            var move_to_pause = new AnimGoalBuilder(AnimGoalType.move_to_pause)
                .SetPriority(AnimGoalPriority.AGP_2)
                .SetFieldC(true);
            move_to_pause.AddState(AnimGoalActions.GoalParam1ObjCloseToParam2Loc) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_POP_GOAL, 1000)
                .OnFailure(T_POP_GOAL, 1000);
            move_to_pause.AddState(AlwaysFail) // Index 1
                .SetArgs(AnimGoalProperty.BLOCK_OBJ)
                .OnSuccess(T_POP_ALL, 1000)
                .OnFailure(T_POP_ALL);
            move_to_pause.AddState(AlwaysSucceed) // Index 2
                .SetArgs(AnimGoalProperty.BLOCK_OBJ)
                .OnSuccess(T_POP_ALL, 1000)
                .OnFailure(T_POP_ALL);
            Add(move_to_pause.Build());

            // AnimGoalType.move_near_tile
            var move_near_tile = new AnimGoalBuilder(AnimGoalType.move_near_tile)
                .SetPriority(AnimGoalPriority.AGP_2)
                .SetRelatedGoals(AnimGoalType.run_to_tile, AnimGoalType.move_to_tile, AnimGoalType.run_near_tile);
            move_near_tile.AddCleanup(AnimGoalActions.GoalResetToIdleAnim)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            move_near_tile.AddState(AnimGoalActions.GoalParam1ObjCloseToParam2Loc) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_GOTO_STATE(1));
            move_near_tile.AddState(AnimGoalActions.GoalIsProne) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_GOTO_STATE(2));
            move_near_tile.AddState(AnimGoalActions.GoalIsConcealed) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.unconceal))
                .OnFailure(T_GOTO_STATE(3));
            move_near_tile.AddState(AnimGoalActions.GoalIsCurrentPathValid) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_GOTO_STATE(5));
            move_near_tile.AddState(AnimGoalActions.GoalIsRotatedTowardNextPathNode) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.attempt_move))
                .OnFailure(T_PUSH_GOAL(AnimGoalType.rotate));
            move_near_tile.AddState(AnimGoalActions.GoalFindPathNear) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_REWIND)
                .OnFailure(T_GOTO_STATE(6));
            move_near_tile.AddState(AlwaysFail) // Index 6
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.move_to_pause) | T_REWIND, 1000)
                .OnFailure(T_POP_ALL);
            Add(move_near_tile.Build());

            // AnimGoalType.move_near_obj
            var move_near_obj = new AnimGoalBuilder(AnimGoalType.move_near_obj)
                .SetPriority(AnimGoalPriority.AGP_2);
            move_near_obj.AddCleanup(AnimGoalActions.GoalResetToIdleAnim)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            move_near_obj.AddState(AnimGoalActions.GoalIsTargetWithinRadius) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_GOTO_STATE(1));
            move_near_obj.AddState(AnimGoalActions.GoalIsProne) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_GOTO_STATE(2));
            move_near_obj.AddState(AnimGoalActions.GoalIsConcealed) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.unconceal))
                .OnFailure(T_GOTO_STATE(3));
            move_near_obj.AddState(AnimGoalActions.GoalIsCurrentPathValid) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.attempt_move_near))
                .OnFailure(T_GOTO_STATE(4));
            move_near_obj.AddState(AnimGoalActions.GoalFindPathNearObject) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.attempt_move_near))
                .OnFailure(T_GOTO_STATE(5));
            move_near_obj.AddState(AlwaysFail) // Index 5
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.move_to_pause) | T_REWIND, 1000)
                .OnFailure(T_POP_ALL);
            Add(move_near_obj.Build());

            // AnimGoalType.move_straight
            var move_straight = new AnimGoalBuilder(AnimGoalType.move_straight)
                .SetPriority(AnimGoalPriority.AGP_3);
            move_straight.AddState(AnimGoalActions.GoalParam1ObjCloseToParam2Loc) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_GOTO_STATE(1));
            move_straight.AddState(AnimGoalActions.GoalIsCurrentPathValid) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.attempt_move_straight))
                .OnFailure(T_GOTO_STATE(2));
            move_straight.AddState(AnimGoalActions.GoalCalcPathToTarget2) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.attempt_move_straight))
                .OnFailure(T_GOTO_STATE(3));
            move_straight.AddState(AlwaysFail) // Index 3
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.move_to_pause) | T_REWIND, 1000)
                .OnFailure(T_POP_ALL);
            Add(move_straight.Build());

            // AnimGoalType.attempt_move_straight
            var attempt_move_straight = new AnimGoalBuilder(AnimGoalType.attempt_move_straight)
                .SetPriority(AnimGoalPriority.AGP_3);
            attempt_move_straight.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            attempt_move_straight.AddState(AnimGoalActions.GoalContinueMoveStraight) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_CUSTOM)
                .OnFailure(T_POP_GOAL, DELAY_SLOT);
            attempt_move_straight.AddState(AnimGoalActions.GoalPlayWaterRipples) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND)
                .OnFailure(T_POP_ALL);
            Add(attempt_move_straight.Build());

            // AnimGoalType.open_door
            var open_door = new AnimGoalBuilder(AnimGoalType.open_door)
                .SetPriority(AnimGoalPriority.AGP_3)
                .SetInterruptAll(true)
                .SetFieldC(true);
            open_door.AddState(AnimGoalActions.GoalIsParam1Door) // Index 0
                .SetArgs(AnimGoalProperty.SCRATCH_OBJ)
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_POP_GOAL);
            open_door.AddState(AnimGoalActions.GoalIsDoorFullyClosed) // Index 1
                .SetArgs(AnimGoalProperty.SCRATCH_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_POP_GOAL);
            open_door.AddState(AnimGoalActions.GoalAttemptOpenDoor) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SCRATCH_OBJ)
                .OnSuccess(T_GOTO_STATE(3))
                .OnFailure(T_POP_ALL);
            open_door.AddState(AnimGoalActions.GoalIsDoorLocked) // Index 3
                .SetArgs(AnimGoalProperty.SCRATCH_OBJ, AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_GOTO_STATE(5));
            open_door.AddState(AnimGoalActions.GoalUnlockDoorReturnFalse) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SCRATCH_OBJ)
                .OnSuccess(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.unlock_door))
                .OnFailure(T_POP_ALL);
            open_door.AddState(AnimGoalActions.GoalUseObject) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SCRATCH_OBJ)
                .OnSuccess(T_GOTO_STATE(6))
                .OnFailure(T_POP_ALL);
            open_door.AddState(AnimGoalActions.GoalPlayDoorLockedSound) // Index 6
                .SetArgs(AnimGoalProperty.SCRATCH_OBJ, AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(7))
                .OnFailure(T_POP_ALL);
            open_door.AddState(AnimGoalActions.GoalStateCallback1) // Index 7
                .SetArgs(AnimGoalProperty.SCRATCH_OBJ, AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(66)
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_POP_ALL);
            Add(open_door.Build());

            // AnimGoalType.attempt_open_door
            var attempt_open_door = new AnimGoalBuilder(AnimGoalType.attempt_open_door)
                .SetPriority(AnimGoalPriority.AGP_3)
                .SetInterruptAll(true)
                .SetFieldC(true);
            attempt_open_door.AddState(AnimGoalActions.GoalIsParam1Door) // Index 0
                .SetArgs(AnimGoalProperty.SCRATCH_OBJ)
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_POP_GOAL);
            attempt_open_door.AddState(AnimGoalActions.GoalIsDoorFullyClosed) // Index 1
                .SetArgs(AnimGoalProperty.SCRATCH_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_POP_ALL);
            attempt_open_door.AddState(AnimGoalActions.GoalAttemptOpenDoor) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SCRATCH_OBJ)
                .OnSuccess(T_GOTO_STATE(3))
                .OnFailure(T_POP_ALL);
            attempt_open_door.AddState(AnimGoalActions.GoalIsDoorLocked) // Index 3
                .SetArgs(AnimGoalProperty.SCRATCH_OBJ, AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_GOTO_STATE(5));
            attempt_open_door.AddState(AnimGoalActions.GoalUnlockDoorReturnFalse) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SCRATCH_OBJ)
                .OnSuccess(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.unlock_door))
                .OnFailure(T_POP_ALL);
            attempt_open_door.AddState(AnimGoalActions.GoalUseObject) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SCRATCH_OBJ)
                .OnSuccess(T_GOTO_STATE(6))
                .OnFailure(T_POP_ALL);
            attempt_open_door.AddState(AnimGoalActions.GoalPlayDoorLockedSound) // Index 6
                .SetArgs(AnimGoalProperty.SCRATCH_OBJ, AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(7))
                .OnFailure(T_POP_ALL);
            attempt_open_door.AddState(AnimGoalActions.GoalStateCallback1) // Index 7
                .SetArgs(AnimGoalProperty.SCRATCH_OBJ, AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(66)
                .OnSuccess(T_POP_GOAL | 0x1000000)
                .OnFailure(T_POP_ALL);
            Add(attempt_open_door.Build());


            // AnimGoalType.unlock_door was undefined


            // AnimGoalType.jump_window was undefined


            // AnimGoalType.pickup_item was undefined


            // AnimGoalType.attempt_pickup was undefined

            // AnimGoalType.pickpocket
            var pickpocket = new AnimGoalBuilder(AnimGoalType.pickpocket)
                .SetPriority(AnimGoalPriority.AGP_3);
            pickpocket.AddState(AnimGoalActions.GoalSaveStateDataInSkillData) // Index 0
                .SetFlagsData(12)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.use_skill_on))
                .OnFailure(T_POP_ALL);
            Add(pickpocket.Build());

            // AnimGoalType.attack
            var attack = new AnimGoalBuilder(AnimGoalType.attack)
                .SetPriority(AnimGoalPriority.AGP_3)
                .SetRelatedGoals(AnimGoalType.attempt_attack);
            attack.AddCleanup(AnimGoalActions.GoalResetToIdleAnim)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            attack.AddState(AnimGoalActions.GoalIsAlive) // Index 0
                .SetArgs(AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_POP_GOAL);
            attack.AddState(AnimGoalActions.GoalAttackEndTurnIfUnreachable) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_POP_ALL);
            attack.AddState(AnimGoalActions.GoalIsProne) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_GOTO_STATE(3));
            attack.AddState(AnimGoalActions.GoalIsConcealed) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.unconceal))
                .OnFailure(T_GOTO_STATE(4));
            attack.AddState(AlwaysSucceed) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.attempt_attack) | 0x4000000)
                .OnFailure(T_GOTO_STATE(5));
            attack.AddState(AnimGoalActions.GoalSaveStateDataOrSpellRangeInRadius) // Index 5
                .SetFlagsData(1)
                .OnSuccess(T_GOTO_STATE(6))
                .OnFailure(T_POP_ALL);
            attack.AddState(AnimGoalActions.GoalIsTargetWithinRadius) // Index 6
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(7))
                .OnFailure(T_PUSH_GOAL(AnimGoalType.move_near_obj_combat));
            attack.AddState(AlwaysFail) // Index 7
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.move_to_pause) | T_REWIND, 1000)
                .OnFailure(T_POP_ALL);
            Add(attack.Build());

            // AnimGoalType.attempt_attack
            var attempt_attack = new AnimGoalBuilder(AnimGoalType.attempt_attack)
                .SetPriority(AnimGoalPriority.AGP_3)
                .SetRelatedGoals(AnimGoalType.attack);
            attempt_attack.AddCleanup(AnimGoalActions.GoalResetToIdleAnim)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            attempt_attack.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            attempt_attack.AddState(AnimGoalActions.GoalAttackContinueWithAnim) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(6))
                .OnFailure(T_GOTO_STATE(8));
            attempt_attack.AddState(AnimGoalActions.GoalSetRotationToFaceTargetObj) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(3))
                .OnFailure(T_GOTO_STATE(9));
            attempt_attack.AddState(AlwaysSucceed) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_GOTO_STATE(11));
            attempt_attack.AddState(AnimGoalActions.GoalEnterCombat) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(5))
                .OnFailure(T_GOTO_STATE(9));
            attempt_attack.AddState(AnimGoalActions.GoalAttackPlayWeaponHitEffect) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_REWIND)
                .OnFailure(T_GOTO_STATE(9));
            attempt_attack.AddState(AnimGoalActions.GoalSlotFlagSet8If4AndNotSetYet) // Index 6
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_GOTO_STATE(12))
                .OnFailure(T_REWIND, DELAY_SLOT);
            attempt_attack.AddState(AnimGoalActions.GoalActionPerform2) // Index 7
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_GOTO_STATE(9));
            attempt_attack.AddState(AnimGoalActions.GoalHasReachWithMainWeapon) // Index 8
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(9))
                .OnFailure(T_GOTO_STATE(9));
            attempt_attack.AddState(AnimGoalActions.GoalPlayAnim) // Index 9
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(PackAnimId(WeaponAnim.Idle))
                .OnSuccess(T_GOTO_STATE(10))
                .OnFailure(T_GOTO_STATE(10));
            attempt_attack.AddState(AnimGoalActions.GoalAttackPlayIdleAnim) // Index 10
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(11))
                .OnFailure(T_GOTO_STATE(11));
            attempt_attack.AddState(AnimGoalActions.GoalAttemptAttackCheck) // Index 11
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.attack) | 0x4000000, 5)
                .OnFailure(T_POP_GOAL);
            attempt_attack.AddState(AlwaysSucceed) // Index 12
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(7))
                .OnFailure(T_GOTO_STATE(11));
            Add(attempt_attack.Build());

            // AnimGoalType.talk
            var talk = new AnimGoalBuilder(AnimGoalType.talk)
                .SetPriority(AnimGoalPriority.AGP_3);
            talk.AddState(AnimGoalActions.GoalNotPreventedFromTalking) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_POP_GOAL);
            talk.AddState(AnimGoalActions.GoalIsProne) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_GOTO_STATE(2));
            talk.AddState(AnimGoalActions.GoalIsConcealed) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.unconceal))
                .OnFailure(T_GOTO_STATE(3));
            talk.AddState(AnimGoalActions.GoalIsWithinTalkingDistance) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_GOTO_STATE(5));
            talk.AddState(AnimGoalActions.GoalInitiateDialog) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_POP_ALL);
            talk.AddState(AnimGoalActions.GoalSaveStateDataOrSpellRangeInRadius) // Index 5
                .SetFlagsData(5)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.move_near_obj))
                .OnFailure(T_POP_ALL);
            Add(talk.Build());


            // AnimGoalType.pick_weapon was undefined

            // AnimGoalType.chase
            var chase = new AnimGoalBuilder(AnimGoalType.chase)
                .SetPriority(AnimGoalPriority.AGP_3);
            chase.AddState(AnimGoalActions.GoalSetRunningFlag) // Index 0
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_GOTO_STATE(1));
            chase.AddState(AnimGoalActions.GoalSetRadiusToAiSpread) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_POP_ALL);
            chase.AddState(AnimGoalActions.GoalIsTargetWithinRadius) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(3))
                .OnFailure(T_PUSH_GOAL(AnimGoalType.move_near_obj));
            chase.AddState(AnimGoalActions.GoalActionPerform3) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1)
                .OnSuccess(T_REWIND, 100)
                .OnFailure(T_POP_ALL);
            Add(chase.Build());

            // AnimGoalType.follow
            var follow = new AnimGoalBuilder(AnimGoalType.follow)
                .SetPriority(AnimGoalPriority.AGP_2)
                .SetRelatedGoals(AnimGoalType.run_near_obj, AnimGoalType.move_near_obj);
            follow.AddState(AnimGoalActions.GoalSetRadiusToAiSpread) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_POP_ALL);
            follow.AddState(AnimGoalActions.GoalIsCloserThanDesiredSpread) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(2));
            follow.AddState(AnimGoalActions.GoalIsTargetWithinRadius) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_GOTO_STATE(3));
            follow.AddState(AlwaysFail) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.move_near_obj))
                .OnFailure(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.run_near_obj));
            follow.AddState(AnimGoalActions.GoalActionPerform3) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1)
                .OnSuccess(T_REWIND, 100)
                .OnFailure(T_POP_ALL);
            Add(follow.Build());

            // AnimGoalType.follow_to_location
            var follow_to_location = new AnimGoalBuilder(AnimGoalType.follow_to_location)
                .SetPriority(AnimGoalPriority.AGP_2)
                .SetRelatedGoals(AnimGoalType.run_near_obj, AnimGoalType.move_near_obj);
            follow_to_location.AddState(AnimGoalActions.GoalSetRadiusTo4) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_POP_ALL);
            follow_to_location.AddState(AnimGoalActions.GoalTargetLocWithinRadius) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.run_to_tile))
                .OnFailure(T_PUSH_GOAL(AnimGoalType.follow));
            Add(follow_to_location.Build());

            // AnimGoalType.flee
            var flee = new AnimGoalBuilder(AnimGoalType.flee)
                .SetPriority(AnimGoalPriority.AGP_3);
            flee.AddState(AnimGoalActions.GoalSetRunningFlag) // Index 0
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_GOTO_STATE(1));
            flee.AddState(AnimGoalActions.GoalSaveStateDataOrSpellRangeInRadius) // Index 1
                .SetFlagsData(9)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_POP_ALL);
            flee.AddState(AnimGoalActions.GoalIsTargetWithinRadius) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.move_away_from_obj))
                .OnFailure(T_GOTO_STATE(3));
            flee.AddState(AnimGoalActions.GoalActionPerform3) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1)
                .OnSuccess(T_POP_GOAL, DELAY_SLOT)
                .OnFailure(T_POP_ALL);
            Add(flee.Build());

            // AnimGoalType.throw_spell
            var throw_spell = new AnimGoalBuilder(AnimGoalType.throw_spell)
                .SetPriority(AnimGoalPriority.AGP_3)
                .SetRelatedGoals(AnimGoalType.attempt_spell);
            throw_spell.AddCleanup(AnimGoalActions.GoalResetToIdleAnim)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            throw_spell.AddState(AlwaysSucceed) // Index 0
                .SetArgs(AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_GOTO_STATE(9));
            throw_spell.AddState(AnimGoalActions.GoalIsProne) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_GOTO_STATE(2));
            throw_spell.AddState(AnimGoalActions.GoalIsConcealed) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.unconceal))
                .OnFailure(T_GOTO_STATE(3));
            throw_spell.AddState(AnimGoalActions.GoalReturnTrue) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_GOTO_STATE(7));
            throw_spell.AddState(AlwaysSucceed) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(5))
                .OnFailure(T_PUSH_GOAL(AnimGoalType.pick_weapon));
            throw_spell.AddState(AlwaysSucceed) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(8))
                .OnFailure(T_GOTO_STATE(6));
            throw_spell.AddState(AnimGoalActions.GoalSaveStateDataOrSpellRangeInRadius) // Index 6
                .SetFlagsData(8)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.move_near_obj))
                .OnFailure(T_GOTO_STATE(9));
            throw_spell.AddState(AlwaysSucceed) // Index 7
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(9))
                .OnFailure(T_GOTO_STATE(9));
            throw_spell.AddState(AnimGoalActions.GoalSetRotationToFaceTargetObj) // Index 8
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.attempt_spell))
                .OnFailure(T_GOTO_STATE(9));
            throw_spell.AddState(AnimGoalActions.GoalCastConjureEnd) // Index 9
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SKILL_DATA)
                .SetFlagsData(1)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            Add(throw_spell.Build());

            // AnimGoalType.attempt_spell
            var attempt_spell = new AnimGoalBuilder(AnimGoalType.attempt_spell)
                .SetPriority(AnimGoalPriority.AGP_4)
                .SetRelatedGoals(AnimGoalType.throw_spell);
            attempt_spell.AddCleanup(AnimGoalActions.GoalCastConjureEnd)
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SKILL_DATA)
                .SetFlagsData(1);
            attempt_spell.AddState(AnimGoalActions.GoalIsNotStackFlagsData40) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(10))
                .OnFailure(T_GOTO_STATE(1));
            attempt_spell.AddState(AnimGoalActions.GoalSetSlotFlags4) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(3))
                .OnFailure(T_GOTO_STATE(7), DELAY_SLOT);
            attempt_spell.AddState(AlwaysSucceed) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND)
                .OnFailure(T_GOTO_STATE(11));
            attempt_spell.AddState(AnimGoalActions.GoalSlotFlagSet8If4AndNotSetYet) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_REWIND, DELAY_SLOT);
            attempt_spell.AddState(AnimGoalActions.GoalReturnFalse) // Index 4
                .SetArgs(AnimGoalProperty.SKILL_DATA)
                .OnSuccess(T_GOTO_STATE(5))
                .OnFailure(T_GOTO_STATE(8));
            attempt_spell.AddState(AnimGoalActions.GoalSpawnFireball) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SELF_OBJ_PRECISE_LOC)
                .SetFlagsData(5)
                .OnSuccess(T_GOTO_STATE(6))
                .OnFailure(T_GOTO_STATE(7));
            attempt_spell.AddState(AnimGoalActions.GoalStateCallback1) // Index 6
                .SetArgs(AnimGoalProperty.SCRATCH_OBJ, AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(29)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_ALL, DELAY_SLOT);
            attempt_spell.AddState(AlwaysSucceed) // Index 7
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_POP_ALL);
            attempt_spell.AddState(AnimGoalActions.GoalAttemptSpell) // Index 8
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SKILL_DATA)
                .OnSuccess(T_REWIND)
                .OnFailure(T_GOTO_STATE(7));
            attempt_spell.AddState(AnimGoalActions.GoalActionPerform3) // Index 9
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            attempt_spell.AddState(AlwaysSucceed) // Index 10
                .SetArgs(AnimGoalProperty.SKILL_DATA)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(11));
            attempt_spell.AddState(AnimGoalActions.GoalAttemptSpell) // Index 11
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SKILL_DATA)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            Add(attempt_spell.Build());

            // AnimGoalType.shoot_spell
            var shoot_spell = new AnimGoalBuilder(AnimGoalType.shoot_spell)
                .SetPriority(AnimGoalPriority.AGP_3);
            shoot_spell.AddCleanup(AnimGoalActions.GoalDestroyParam1)
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SKILL_DATA)
                .SetFlagsData(1);
            shoot_spell.AddState(AnimGoalActions.GoalParam1ObjCloseToParam2Loc) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_LOC_PRECISE)
                .OnSuccess(T_GOTO_STATE(3))
                .OnFailure(T_GOTO_STATE(1));
            shoot_spell.AddState(AnimGoalActions.GoalIsCurrentPathValid) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(7))
                .OnFailure(T_GOTO_STATE(2));
            shoot_spell.AddState(AnimGoalActions.GoalCalcPathToTarget2) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_LOC_PRECISE)
                .OnSuccess(T_GOTO_STATE(7))
                .OnFailure(T_POP_ALL);
            shoot_spell.AddState(AnimGoalActions.GoalAreOnSameTile) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_GOTO_STATE(8));
            shoot_spell.AddState(AnimGoalActions.GoalAttemptSpell) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SKILL_DATA)
                .OnSuccess(T_GOTO_STATE(5))
                .OnFailure(T_POP_ALL);
            shoot_spell.AddState(AnimGoalActions.GoalSetOffAndDestroyParam1) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_POP_ALL);
            shoot_spell.AddState(AlwaysFail) // Index 6
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SKILL_DATA)
                .OnSuccess(T_GOTO_STATE(5))
                .OnFailure(T_GOTO_STATE(5));
            shoot_spell.AddState(AlwaysFail) // Index 7
                .SetArgs(AnimGoalProperty.SKILL_DATA)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.attempt_move_straight_spell))
                .OnFailure(T_POP_ALL);
            shoot_spell.AddState(AnimGoalActions.GoalSetTargetLocFromObj) // Index 8
                .SetArgs(AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(0))
                .OnFailure(T_POP_GOAL);
            Add(shoot_spell.Build());

            // AnimGoalType.hit_by_spell
            var hit_by_spell = new AnimGoalBuilder(AnimGoalType.hit_by_spell)
                .SetPriority(AnimGoalPriority.AGP_3)
                .SetFieldC(true);
            hit_by_spell.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            hit_by_spell.AddState(AlwaysFail) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_GOAL, DELAY_SLOT);
            hit_by_spell.AddState(AlwaysFail) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SPELL_DATA)
                .OnSuccess(T_REWIND)
                .OnFailure(T_GOTO_STATE(3));
            hit_by_spell.AddState(AnimGoalActions.GoalActionPerform3) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            Add(hit_by_spell.Build());

            // AnimGoalType.hit_by_weapon
            var hit_by_weapon = new AnimGoalBuilder(AnimGoalType.hit_by_weapon)
                .SetPriority(AnimGoalPriority.AGP_3)
                .SetFieldC(true);
            hit_by_weapon.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            hit_by_weapon.AddState(AnimGoalActions.GoalUnconcealAnimate) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_ALL, DELAY_SLOT);
            hit_by_weapon.AddState(AnimGoalActions.GoalPlayGetHitAnim) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(3))
                .OnFailure(T_POP_ALL);
            hit_by_weapon.AddState(AnimGoalActions.GoalThrowItemPlayAnim) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_REWIND)
                .OnFailure(T_POP_ALL);
            Add(hit_by_weapon.Build());

            // AnimGoalType.dodge
            var dodge = new AnimGoalBuilder(AnimGoalType.dodge)
                .SetPriority(AnimGoalPriority.AGP_3)
                .SetFieldC(true);
            dodge.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            dodge.AddState(AnimGoalActions.GoalUnconcealAnimate) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_ALL, DELAY_SLOT);
            dodge.AddState(AnimGoalActions.GoalPlayDodgeAnim) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(3))
                .OnFailure(T_POP_ALL);
            dodge.AddState(AnimGoalActions.GoalThrowItemPlayAnim) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_REWIND)
                .OnFailure(T_POP_ALL);
            Add(dodge.Build());

            // AnimGoalType.dying
            var dying = new AnimGoalBuilder(AnimGoalType.dying)
                .SetPriority(AnimGoalPriority.AGP_5)
                .SetFieldC(true);
            dying.AddCleanup(AnimGoalActions.GoalDyingCleanup)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            dying.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            dying.AddState(AnimGoalActions.GoalDyingContinueAnim) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_GOTO_STATE(6));
            dying.AddState(AnimGoalActions.GoalIsProne) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(6))
                .OnFailure(T_GOTO_STATE(3));
            dying.AddState(AnimGoalActions.GoalIsConcealed) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_GOTO_STATE(4));
            dying.AddState(AnimGoalActions.GoalStartDeathAnim) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(5))
                .OnFailure(T_GOTO_STATE(6));
            dying.AddState(AnimGoalActions.GoalDyingPlaySoundAndRipples) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_REWIND)
                .OnFailure(T_GOTO_STATE(6));
            dying.AddState(AnimGoalActions.GoalDyingReturnTrue) // Index 6
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(8))
                .OnFailure(T_GOTO_STATE(7));
            dying.AddState(AnimGoalActions.GoalActionPerform3) // Index 7
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            dying.AddState(AnimGoalActions.GoalSetNoBlockIfNotInParty) // Index 8
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            Add(dying.Build());

            // AnimGoalType.destroy_obj
            var destroy_obj = new AnimGoalBuilder(AnimGoalType.destroy_obj)
                .SetPriority(AnimGoalPriority.AGP_5);
            destroy_obj.AddState(AnimGoalActions.GoalSetOffAndDestroyParam1) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            Add(destroy_obj.Build());

            // AnimGoalType.use_skill_on
            var use_skill_on = new AnimGoalBuilder(AnimGoalType.use_skill_on)
                .SetPriority(AnimGoalPriority.AGP_3);
            use_skill_on.AddCleanup(AnimGoalActions.GoalResetToIdleAnim)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            use_skill_on.AddState(AnimGoalActions.GoalSaveStateDataOrSpellRangeInRadius) // Index 0
                .SetFlagsData(5)
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_POP_ALL);
            use_skill_on.AddState(AnimGoalActions.GoalIsTargetWithinRadius) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .SetFlagsData(1)
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_GOTO_STATE(2));
            use_skill_on.AddState(AnimGoalActions.GoalIsCurrentPathValid) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.attempt_move))
                .OnFailure(T_GOTO_STATE(3));
            use_skill_on.AddState(AnimGoalActions.GoalFindPathNearObject) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.attempt_move))
                .OnFailure(T_GOTO_STATE(6));
            use_skill_on.AddState(AnimGoalActions.GoalIsRotatedTowardTarget) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(5))
                .OnFailure(T_PUSH_GOAL(AnimGoalType.rotate));
            use_skill_on.AddState(AnimGoalActions.GoalCheckParam2AgainstStateFlagData) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.FLAGS_DATA)
                .SetFlagsData(11)
                .OnSuccess(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.use_picklock_skill_on))
                .OnFailure(T_GOTO_STATE(7));
            use_skill_on.AddState(AlwaysFail) // Index 6
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.move_to_pause) | T_REWIND, 1000)
                .OnFailure(T_POP_ALL);
            use_skill_on.AddState(AnimGoalActions.GoalCheckParam2AgainstStateFlagData) // Index 7
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.FLAGS_DATA)
                .SetFlagsData(12)
                .OnSuccess(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.attempt_use_pickpocket_skill_on))
                .OnFailure(T_GOTO_STATE(8));
            use_skill_on.AddState(AnimGoalActions.GoalCheckParam2AgainstStateFlagData) // Index 8
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.FLAGS_DATA)
                .SetFlagsData(4)
                .OnSuccess(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.use_disable_device_skill_on_data))
                .OnFailure(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.attempt_use_skill_on));
            Add(use_skill_on.Build());

            // AnimGoalType.attempt_use_skill_on
            var attempt_use_skill_on = new AnimGoalBuilder(AnimGoalType.attempt_use_skill_on)
                .SetPriority(AnimGoalPriority.AGP_3);
            attempt_use_skill_on.AddCleanup(AnimGoalActions.GoalResetToIdleAnim)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            attempt_use_skill_on.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            attempt_use_skill_on.AddState(AnimGoalActions.GoalUnconcealAnimate) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(5))
                .OnFailure(T_GOTO_STATE(7), DELAY_SLOT);
            attempt_use_skill_on.AddState(AnimGoalActions.GoalSetRotationToFaceTargetObj) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(3))
                .OnFailure(T_POP_ALL);
            attempt_use_skill_on.AddState(AnimGoalActions.GoalPlayAnim) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(PackAnimId(NormalAnimType.SkillPickpocket))
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_POP_ALL);
            attempt_use_skill_on.AddState(AnimGoalActions.GoalThrowItemPlayAnim) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_REWIND)
                .OnFailure(T_GOTO_STATE(8));
            attempt_use_skill_on.AddState(AnimGoalActions.GoalSlotFlagSet8If4AndNotSetYet) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_GOTO_STATE(6))
                .OnFailure(T_REWIND, DELAY_SLOT);
            attempt_use_skill_on.AddState(AnimGoalActions.GoalActionPerform) // Index 6
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_ALL);
            attempt_use_skill_on.AddState(AnimGoalActions.GoalPlayAnim) // Index 7
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(PackAnimId(WeaponAnim.Idle))
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_POP_ALL);
            attempt_use_skill_on.AddState(AnimGoalActions.GoalActionPerform3) // Index 8
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            Add(attempt_use_skill_on.Build());


            // AnimGoalType.skill_conceal was undefined

            // AnimGoalType.projectile
            var projectile = new AnimGoalBuilder(AnimGoalType.projectile)
                .SetPriority(AnimGoalPriority.AGP_5);
            projectile.AddCleanup(AnimGoalActions.GoalProjectileCleanup)
                .SetFlagsData(1);
            projectile.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            projectile.AddState(AnimGoalActions.GoalUpdateMoveStraight) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_REWIND, DELAY_CUSTOM)
                .OnFailure(T_POP_GOAL);
            projectile.AddState(AnimGoalActions.GoalParam1ObjCloseToParam2Loc) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_GOTO_STATE(3));
            projectile.AddState(AnimGoalActions.GoalStateCallback3) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_POP_GOAL);
            projectile.AddState(AnimGoalActions.GoalIsCurrentPathValid) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(6))
                .OnFailure(T_GOTO_STATE(5));
            projectile.AddState(AlwaysSucceed) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_GOTO_STATE(6))
                .OnFailure(T_POP_GOAL);
            projectile.AddState(AnimGoalActions.GoalBeginMoveStraight) // Index 6
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_REWIND)
                .OnFailure(T_POP_GOAL);
            Add(projectile.Build());

            // AnimGoalType.throw_item
            var throw_item = new AnimGoalBuilder(AnimGoalType.throw_item)
                .SetPriority(AnimGoalPriority.AGP_3);
            throw_item.AddCleanup(AnimGoalActions.GoalThrowItemCleanup)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            throw_item.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            throw_item.AddState(AnimGoalActions.GoalUnconcealAnimate) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(5))
                .OnFailure(T_GOTO_STATE(7), DELAY_SLOT);
            throw_item.AddState(AnimGoalActions.GoalPlayAnim) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(PackAnimId(WeaponAnim.Idle))
                .OnSuccess(T_GOTO_STATE(3))
                .OnFailure(T_GOTO_STATE(7));
            throw_item.AddState(AnimGoalActions.GoalSetRotationToFaceTargetLoc) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_GOTO_STATE(7));
            throw_item.AddState(AnimGoalActions.GoalThrowItemPlayAnim) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_REWIND)
                .OnFailure(T_GOTO_STATE(7));
            throw_item.AddState(AnimGoalActions.GoalSlotFlagSet8If4AndNotSetYet) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_GOTO_STATE(6))
                .OnFailure(T_REWIND, DELAY_SLOT);
            throw_item.AddState(AnimGoalActions.GoalThrowItem) // Index 6
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SCRATCH_OBJ)
                .SetFlagsData(5)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_GOTO_STATE(7));
            throw_item.AddState(AnimGoalActions.GoalPlayAnim) // Index 7
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(PackAnimId(WeaponAnim.Idle))
                .OnSuccess(T_GOTO_STATE(8))
                .OnFailure(T_GOTO_STATE(8));
            throw_item.AddState(AnimGoalActions.GoalActionPerform3) // Index 8
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            Add(throw_item.Build());

            // AnimGoalType.use_object
            var use_object = new AnimGoalBuilder(AnimGoalType.use_object)
                .SetPriority(AnimGoalPriority.AGP_3);
            use_object.AddState(AnimGoalActions.GoalSetRadiusTo2) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_POP_ALL);
            use_object.AddState(AnimGoalActions.GoalIsTargetWithinRadius) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_PUSH_GOAL(AnimGoalType.move_near_obj));
            use_object.AddState(AnimGoalActions.GoalIsRotatedTowardTarget) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(3))
                .OnFailure(T_PUSH_GOAL(AnimGoalType.rotate));
            use_object.AddState(AnimGoalActions.GoalIsParam1Door) // Index 3
                .SetArgs(AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(5))
                .OnFailure(T_GOTO_STATE(4));
            use_object.AddState(AnimGoalActions.GoalUseObject) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            use_object.AddState(AnimGoalActions.GoalSaveParam1InScratch) // Index 5
                .SetArgs(AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(6))
                .OnFailure(T_POP_ALL);
            use_object.AddState(AnimGoalActions.GoalIsDoorFullyClosed) // Index 6
                .SetArgs(AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.open_door))
                .OnFailure(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.close_door));
            Add(use_object.Build());

            // AnimGoalType.use_item_on_object
            var use_item_on_object = new AnimGoalBuilder(AnimGoalType.use_item_on_object)
                .SetPriority(AnimGoalPriority.AGP_3);
            use_item_on_object.AddState(AnimGoalActions.GoalSaveStateDataOrSpellRangeInRadius) // Index 0
                .SetFlagsData(1)
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_POP_ALL);
            use_item_on_object.AddState(AnimGoalActions.GoalIsTargetWithinRadius) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_PUSH_GOAL(AnimGoalType.move_near_obj));
            use_item_on_object.AddState(AnimGoalActions.GoalUseItemOnObj) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            Add(use_item_on_object.Build());

            // AnimGoalType.use_item_on_object_with_skill
            var use_item_on_object_with_skill = new AnimGoalBuilder(AnimGoalType.use_item_on_object_with_skill)
                .SetPriority(AnimGoalPriority.AGP_3);
            use_item_on_object_with_skill.AddState(AnimGoalActions.GoalSaveStateDataOrSpellRangeInRadius) // Index 0
                .SetFlagsData(1)
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_POP_ALL);
            use_item_on_object_with_skill.AddState(AnimGoalActions.GoalIsTargetWithinRadius) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_PUSH_GOAL(AnimGoalType.move_near_obj));
            use_item_on_object_with_skill.AddState(AnimGoalActions.GoalUseItemOnObjWithSkillDummy) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            Add(use_item_on_object_with_skill.Build());

            // AnimGoalType.use_item_on_tile
            var use_item_on_tile = new AnimGoalBuilder(AnimGoalType.use_item_on_tile)
                .SetPriority(AnimGoalPriority.AGP_3);
            use_item_on_tile.AddState(AnimGoalActions.GoalSaveStateDataOrSpellRangeInRadius) // Index 0
                .SetFlagsData(1)
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_POP_ALL);
            use_item_on_tile.AddState(AnimGoalActions.GoalTargetLocWithinRadius) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_PUSH_GOAL(AnimGoalType.move_near_tile));
            use_item_on_tile.AddState(AnimGoalActions.GoalUseItemOnLoc) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            Add(use_item_on_tile.Build());

            // AnimGoalType.use_item_on_tile_with_skill
            var use_item_on_tile_with_skill = new AnimGoalBuilder(AnimGoalType.use_item_on_tile_with_skill)
                .SetPriority(AnimGoalPriority.AGP_3);
            use_item_on_tile_with_skill.AddState(AnimGoalActions.GoalSaveStateDataOrSpellRangeInRadius) // Index 0
                .SetFlagsData(1)
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_POP_ALL);
            use_item_on_tile_with_skill.AddState(AnimGoalActions.GoalTargetLocWithinRadius) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_PUSH_GOAL(AnimGoalType.move_near_tile));
            use_item_on_tile_with_skill.AddState(AnimGoalActions.GoalUseItemOnLocWithSkillDummy) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            Add(use_item_on_tile_with_skill.Build());

            // AnimGoalType.knockback
            var knockback = new AnimGoalBuilder(AnimGoalType.knockback)
                .SetPriority(AnimGoalPriority.AGP_5);
            knockback.AddState(AnimGoalActions.GoalParam1ObjCloseToParam2Loc) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_GOTO_STATE(1));
            knockback.AddState(AnimGoalActions.GoalIsCurrentPathValid) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.attempt_move_straight_knockback))
                .OnFailure(T_GOTO_STATE(2));
            knockback.AddState(AnimGoalActions.GoalKnockbackFunc) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.attempt_move_straight_knockback))
                .OnFailure(T_GOTO_STATE(3));
            knockback.AddState(AlwaysSucceed) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            Add(knockback.Build());

            // AnimGoalType.floating
            var floating = new AnimGoalBuilder(AnimGoalType.floating)
                .SetPriority(AnimGoalPriority.AGP_5)
                .SetInterruptAll(true)
                .SetFieldC(true)
                .SetField10(true);
            floating.AddState(AnimGoalActions.GoalIsNotStackFlagsData20) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            floating.AddState(AnimGoalActions.GoalJiggleAlongYAxis) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, 100)
                .OnFailure(T_GOTO_STATE(3), 100);
            floating.AddState(AnimGoalActions.GoalStartJigglingAlongYAxis) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND)
                .OnFailure(T_POP_ALL);
            floating.AddState(AnimGoalActions.GoalEndJigglingAlongYAxis) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            Add(floating.Build());

            // AnimGoalType.close_door
            var close_door = new AnimGoalBuilder(AnimGoalType.close_door)
                .SetPriority(AnimGoalPriority.AGP_3)
                .SetFieldC(true);
            close_door.AddState(AnimGoalActions.GoalIsParam1Door) // Index 0
                .SetArgs(AnimGoalProperty.SCRATCH_OBJ)
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_POP_GOAL);
            close_door.AddState(AnimGoalActions.GoalIsDoorFullyClosed) // Index 1
                .SetArgs(AnimGoalProperty.SCRATCH_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.attempt_close_door));
            Add(close_door.Build());

            // AnimGoalType.attempt_close_door
            var attempt_close_door = new AnimGoalBuilder(AnimGoalType.attempt_close_door)
                .SetPriority(AnimGoalPriority.AGP_3)
                .SetInterruptAll(true);
            attempt_close_door.AddState(AnimGoalActions.GoalIsParam1Door) // Index 0
                .SetArgs(AnimGoalProperty.SCRATCH_OBJ)
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_POP_GOAL);
            attempt_close_door.AddState(AnimGoalActions.GoalIsDoorFullyClosed) // Index 1
                .SetArgs(AnimGoalProperty.SCRATCH_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_GOTO_STATE(2));
            attempt_close_door.AddState(AnimGoalActions.GoalIsDoorMagicallyHeld) // Index 2
                .SetArgs(AnimGoalProperty.SCRATCH_OBJ, AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(3))
                .OnFailure(T_POP_ALL);
            attempt_close_door.AddState(AnimGoalActions.GoalStateCallback1) // Index 3
                .SetArgs(AnimGoalProperty.SCRATCH_OBJ, AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(67)
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_POP_ALL);
            Add(attempt_close_door.Build());

            // AnimGoalType.animate_reverse
            var animate_reverse = new AnimGoalBuilder(AnimGoalType.animate_reverse)
                .SetPriority(AnimGoalPriority.AGP_3)
                .SetFieldC(true);
            animate_reverse.AddCleanup(AnimGoalActions.GoalResetToIdleAnim)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            animate_reverse.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            animate_reverse.AddState(AlwaysFail) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_GOAL, DELAY_SLOT);
            animate_reverse.AddState(AlwaysFail) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_REWIND)
                .OnFailure(T_GOTO_STATE(3));
            animate_reverse.AddState(AnimGoalActions.GoalActionPerform3) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            Add(animate_reverse.Build());

            // AnimGoalType.move_away_from_obj
            var move_away_from_obj = new AnimGoalBuilder(AnimGoalType.move_away_from_obj)
                .SetPriority(AnimGoalPriority.AGP_2);
            move_away_from_obj.AddCleanup(AnimGoalActions.GoalResetToIdleAnim)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            move_away_from_obj.AddState(AnimGoalActions.GoalIsProne) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_GOTO_STATE(1));
            move_away_from_obj.AddState(AnimGoalActions.GoalIsConcealed) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.unconceal))
                .OnFailure(T_GOTO_STATE(2));
            move_away_from_obj.AddState(AnimGoalActions.GoalIsTargetWithinRadius) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_POP_GOAL);
            move_away_from_obj.AddState(AnimGoalActions.GoalIsCurrentPathValid) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.attempt_move))
                .OnFailure(T_GOTO_STATE(4));
            move_away_from_obj.AddState(AnimGoalActions.GoalMoveAwayFromObj) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.attempt_move))
                .OnFailure(T_GOTO_STATE(5));
            move_away_from_obj.AddState(AnimGoalActions.GoalTurnTowardsOrAway) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(6))
                .OnFailure(T_POP_ALL);
            move_away_from_obj.AddState(AnimGoalActions.GoalSetNoFlee) // Index 6
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            Add(move_away_from_obj.Build());

            // AnimGoalType.rotate
            var rotate = new AnimGoalBuilder(AnimGoalType.rotate)
                .SetPriority(AnimGoalPriority.AGP_2);
            rotate.AddCleanup(AnimGoalActions.GoalSetRotationToParam2)
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SCRATCH_VAL2);
            rotate.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_GOTO_STATE(2));
            rotate.AddState(AnimGoalActions.GoalPlayRotationAnim) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SCRATCH_VAL2)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_POP_GOAL);
            rotate.AddState(AnimGoalActions.GoalRotate) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SCRATCH_VAL2)
                .OnSuccess(T_REWIND, 15)
                .OnFailure(T_POP_GOAL);
            Add(rotate.Build());

            // AnimGoalType.unconceal
            var unconceal = new AnimGoalBuilder(AnimGoalType.unconceal)
                .SetPriority(AnimGoalPriority.AGP_4);
            unconceal.AddCleanup(AnimGoalActions.GoalUnconcealCleanup)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            unconceal.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            unconceal.AddState(AnimGoalActions.GoalUnconcealAnimate) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_GOAL, DELAY_SLOT);
            unconceal.AddState(AnimGoalActions.GoalPlayUnconcealAnim) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND)
                .OnFailure(T_POP_ALL);
            unconceal.AddState(null) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(0)
                .OnSuccess(T_GOTO_STATE(4294967295))
                .OnFailure(T_GOTO_STATE(4294967295));
            unconceal.AddState(null) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(0)
                .OnSuccess(T_GOTO_STATE(4294967295))
                .OnFailure(T_GOTO_STATE(4294967295));
            Add(unconceal.Build());

            // AnimGoalType.run_near_tile
            var run_near_tile = new AnimGoalBuilder(AnimGoalType.run_near_tile)
                .SetPriority(AnimGoalPriority.AGP_2)
                .SetRelatedGoals(AnimGoalType.move_near_tile, AnimGoalType.run_to_tile);
            run_near_tile.AddCleanup(AnimGoalActions.GoalResetToIdleAnim)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            run_near_tile.AddState(AnimGoalActions.GoalSetRunningFlag) // Index 0
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_GOTO_STATE(1));
            run_near_tile.AddState(AnimGoalActions.GoalParam1ObjCloseToParam2Loc) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_GOTO_STATE(2));
            run_near_tile.AddState(AnimGoalActions.GoalIsProne) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_GOTO_STATE(3));
            run_near_tile.AddState(AnimGoalActions.GoalIsConcealed) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.unconceal))
                .OnFailure(T_GOTO_STATE(4));
            run_near_tile.AddState(AnimGoalActions.GoalIsCurrentPathValid) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(5))
                .OnFailure(T_GOTO_STATE(6));
            run_near_tile.AddState(AnimGoalActions.GoalIsRotatedTowardNextPathNode) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.attempt_move))
                .OnFailure(T_PUSH_GOAL(AnimGoalType.rotate));
            run_near_tile.AddState(AnimGoalActions.GoalFindPathNear) // Index 6
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_REWIND)
                .OnFailure(T_GOTO_STATE(7));
            run_near_tile.AddState(AlwaysFail) // Index 7
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.move_to_pause) | T_REWIND, 1000)
                .OnFailure(T_POP_ALL);
            Add(run_near_tile.Build());

            // AnimGoalType.run_near_obj
            var run_near_obj = new AnimGoalBuilder(AnimGoalType.run_near_obj)
                .SetPriority(AnimGoalPriority.AGP_2);
            run_near_obj.AddCleanup(AnimGoalActions.GoalResetToIdleAnim)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            run_near_obj.AddState(AnimGoalActions.GoalSetRunningFlag) // Index 0
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_GOTO_STATE(1));
            run_near_obj.AddState(AnimGoalActions.GoalIsTargetWithinRadius) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_GOTO_STATE(2));
            run_near_obj.AddState(AnimGoalActions.GoalIsProne) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_GOTO_STATE(3));
            run_near_obj.AddState(AnimGoalActions.GoalIsConcealed) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.unconceal))
                .OnFailure(T_GOTO_STATE(4));
            run_near_obj.AddState(AnimGoalActions.GoalIsCurrentPathValid) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.attempt_move))
                .OnFailure(T_GOTO_STATE(5));
            run_near_obj.AddState(AnimGoalActions.GoalFindPathNearObject) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.attempt_move))
                .OnFailure(T_GOTO_STATE(6));
            run_near_obj.AddState(AlwaysFail) // Index 6
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.move_to_pause) | T_REWIND, 1000)
                .OnFailure(T_POP_ALL);
            Add(run_near_obj.Build());

            // AnimGoalType.animate_stunned
            var animate_stunned = new AnimGoalBuilder(AnimGoalType.animate_stunned)
                .SetPriority(AnimGoalPriority.AGP_1)
                .SetFieldC(true);
            animate_stunned.AddCleanup(AnimGoalActions.GoalResetToIdleAnimUnstun)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            animate_stunned.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            animate_stunned.AddState(AnimGoalActions.GoalStunnedContinueAnim) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_GOTO_STATE(6));
            animate_stunned.AddState(AnimGoalActions.GoalIsProne) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_GOTO_STATE(3));
            animate_stunned.AddState(AnimGoalActions.GoalIsConcealed) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.unconceal))
                .OnFailure(T_GOTO_STATE(4));
            animate_stunned.AddState(AnimGoalActions.GoalStunnedPlayAnim) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_REWIND)
                .OnFailure(T_GOTO_STATE(5));
            animate_stunned.AddState(AnimGoalActions.GoalActionPerform3) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            animate_stunned.AddState(AnimGoalActions.GoalStunnedExpire) // Index 6
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_GOTO_STATE(4));
            Add(animate_stunned.Build());

            // AnimGoalType.animate_kneel_magic_hands
            var animate_kneel_magic_hands = new AnimGoalBuilder(AnimGoalType.animate_kneel_magic_hands)
                .SetPriority(AnimGoalPriority.AGP_3);
            animate_kneel_magic_hands.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            animate_kneel_magic_hands.AddState(AlwaysFail) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.animate_reverse));
            animate_kneel_magic_hands.AddState(AnimGoalActions.GoalIsProne) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_GOTO_STATE(3));
            animate_kneel_magic_hands.AddState(AnimGoalActions.GoalIsConcealed) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.unconceal))
                .OnFailure(T_GOTO_STATE(4));
            animate_kneel_magic_hands.AddState(AlwaysFail) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND)
                .OnFailure(T_GOTO_STATE(5));
            animate_kneel_magic_hands.AddState(AnimGoalActions.GoalActionPerform3) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            Add(animate_kneel_magic_hands.Build());

            // AnimGoalType.attempt_move_near
            var attempt_move_near = new AnimGoalBuilder(AnimGoalType.attempt_move_near)
                .SetPriority(AnimGoalPriority.AGP_2);
            attempt_move_near.AddCleanup(AnimGoalActions.GoalAttemptMoveCleanup)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            attempt_move_near.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(3))
                .OnFailure(T_GOTO_STATE(1));
            attempt_move_near.AddState(AlwaysFail) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(9), DELAY_RANDOM)
                .OnFailure(T_GOTO_STATE(2));
            attempt_move_near.AddState(AnimGoalActions.GoalMoveAlongPath) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_GOAL, DELAY_SLOT);
            attempt_move_near.AddState(AnimGoalActions.GoalIsCurrentPathValid) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_GOTO_STATE(11));
            attempt_move_near.AddState(AnimGoalActions.GoalStateCallback8) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_GOTO_STATE(5));
            attempt_move_near.AddState(AlwaysFail) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(10))
                .OnFailure(T_GOTO_STATE(6));
            attempt_move_near.AddState(AnimGoalActions.GoalHasDoorInPath) // Index 6
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(7))
                .OnFailure(T_GOTO_STATE(8));
            attempt_move_near.AddState(AnimGoalActions.GoalIsDoorFullyClosed) // Index 7
                .SetArgs(AnimGoalProperty.SCRATCH_OBJ)
                .OnSuccess(T_INVALIDATE_PATH | T_PUSH_GOAL(AnimGoalType.open_door) | T_REWIND, 50)
                .OnFailure(T_GOTO_STATE(8));
            attempt_move_near.AddState(AnimGoalActions.GoalPlayMoveAnim) // Index 8
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND)
                .OnFailure(T_GOTO_STATE(9));
            attempt_move_near.AddState(AnimGoalActions.GoalActionPerform3) // Index 9
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1)
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_POP_GOAL);
            attempt_move_near.AddState(AnimGoalActions.GoalIsDoorUnlockedAlwaysReturnFalse) // Index 10
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SCRATCH_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.jump_window) | T_REWIND)
                .OnFailure(T_POP_ALL);
            attempt_move_near.AddState(AlwaysFail) // Index 11
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.move_to_pause) | T_REWIND, 1000)
                .OnFailure(T_POP_ALL);
            Add(attempt_move_near.Build());

            // AnimGoalType.knock_down
            var knock_down = new AnimGoalBuilder(AnimGoalType.knock_down)
                .SetPriority(AnimGoalPriority.AGP_3)
                .SetFieldC(true);
            knock_down.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            knock_down.AddState(AnimGoalActions.GoalUnconcealAnimate) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_GOTO_STATE(3));
            knock_down.AddState(AlwaysFail) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND)
                .OnFailure(T_POP_ALL);
            knock_down.AddState(AnimGoalActions.GoalIsAliveAndConscious) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.anim_get_up), 200)
                .OnFailure(T_POP_GOAL);
            Add(knock_down.Build());

            // AnimGoalType.anim_get_up
            var anim_get_up = new AnimGoalBuilder(AnimGoalType.anim_get_up)
                .SetPriority(AnimGoalPriority.AGP_3);
            anim_get_up.AddCleanup(AnimGoalActions.GoalResetToIdleAnim)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            anim_get_up.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            anim_get_up.AddState(AnimGoalActions.GoalUnconcealAnimate) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_GOAL);
            anim_get_up.AddState(AnimGoalActions.GoalPlayGetUpAnim) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND)
                .OnFailure(T_GOTO_STATE(3));
            anim_get_up.AddState(AnimGoalActions.GoalActionPerform3) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            Add(anim_get_up.Build());

            // AnimGoalType.attempt_move_straight_knockback
            var attempt_move_straight_knockback = new AnimGoalBuilder(AnimGoalType.attempt_move_straight_knockback)
                .SetPriority(AnimGoalPriority.AGP_3);
            attempt_move_straight_knockback.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            attempt_move_straight_knockback.AddState(AnimGoalActions.GoalApplyKnockback) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_CUSTOM)
                .OnFailure(T_POP_ALL);
            attempt_move_straight_knockback.AddState(AnimGoalActions.GoalPlayWaterRipples) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND)
                .OnFailure(T_POP_ALL);
            attempt_move_straight_knockback.AddState(null) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(0)
                .OnSuccess(T_GOTO_STATE(4294967295))
                .OnFailure(T_GOTO_STATE(4294967295));
            Add(attempt_move_straight_knockback.Build());

            // AnimGoalType.wander
            var wander = new AnimGoalBuilder(AnimGoalType.wander)
                .SetPriority(AnimGoalPriority.AGP_3)
                .SetRelatedGoals(AnimGoalType.move_near_tile);
            wander.AddState(AnimGoalActions.GoalWander) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.move_near_tile), 300)
                .OnFailure(T_REWIND, 300);
            Add(wander.Build());

            // AnimGoalType.wander_seek_darkness
            var wander_seek_darkness = new AnimGoalBuilder(AnimGoalType.wander_seek_darkness)
                .SetPriority(AnimGoalPriority.AGP_3)
                .SetRelatedGoals(AnimGoalType.move_near_tile);
            wander_seek_darkness.AddState(AnimGoalActions.GoalWanderSeekDarkness) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.move_near_tile), 300)
                .OnFailure(T_REWIND, 300);
            Add(wander_seek_darkness.Build());

            // AnimGoalType.use_picklock_skill_on
            var use_picklock_skill_on = new AnimGoalBuilder(AnimGoalType.use_picklock_skill_on)
                .SetPriority(AnimGoalPriority.AGP_3);
            use_picklock_skill_on.AddCleanup(AnimGoalActions.GoalResetToIdleAnim)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            use_picklock_skill_on.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            use_picklock_skill_on.AddState(AnimGoalActions.GoalPickLockContinueWithAnim) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_GOTO_STATE(5), DELAY_SLOT);
            use_picklock_skill_on.AddState(AnimGoalActions.GoalSetRotationToFaceTargetObj) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(3))
                .OnFailure(T_POP_ALL);
            use_picklock_skill_on.AddState(AnimGoalActions.GoalPlayAnim) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(PackAnimId(NormalAnimType.PicklockConcentrated))
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_POP_ALL);
            use_picklock_skill_on.AddState(AnimGoalActions.GoalPickLockPlayPushDoorOpenAnim) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_GOTO_STATE(6));
            use_picklock_skill_on.AddState(AnimGoalActions.GoalPickLock) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_ALL);
            use_picklock_skill_on.AddState(AnimGoalActions.GoalPlayAnim) // Index 6
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(PackAnimId(WeaponAnim.Idle))
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_POP_ALL);
            Add(use_picklock_skill_on.Build());

            // AnimGoalType.please_move
            var please_move = new AnimGoalBuilder(AnimGoalType.please_move)
                .SetPriority(AnimGoalPriority.AGP_2)
                .SetRelatedGoals(AnimGoalType.move_near_tile, AnimGoalType.run_to_tile, AnimGoalType.move_to_tile);
            please_move.AddCleanup(AnimGoalActions.GoalResetToIdleAnim)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            please_move.AddState(AnimGoalActions.GoalPleaseMove) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_GOTO_STATE(1), DELAY_CUSTOM)
                .OnFailure(T_POP_GOAL);
            please_move.AddState(AnimGoalActions.GoalParam1ObjCloseToParam2Loc) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_GOTO_STATE(2));
            please_move.AddState(AnimGoalActions.GoalIsProne) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_GOTO_STATE(3));
            please_move.AddState(AnimGoalActions.GoalIsConcealed) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.unconceal))
                .OnFailure(T_GOTO_STATE(4));
            please_move.AddState(AnimGoalActions.GoalIsCurrentPathValid) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(5))
                .OnFailure(T_GOTO_STATE(6));
            please_move.AddState(AnimGoalActions.GoalIsRotatedTowardNextPathNode) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.attempt_move))
                .OnFailure(T_PUSH_GOAL(AnimGoalType.rotate));
            please_move.AddState(AnimGoalActions.GoalCalcPathToTarget) // Index 6
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_REWIND)
                .OnFailure(T_GOTO_STATE(7));
            please_move.AddState(AlwaysFail) // Index 7
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.move_to_pause) | T_REWIND, 1000)
                .OnFailure(T_POP_ALL);
            Add(please_move.Build());

            // AnimGoalType.attempt_spread_out
            var attempt_spread_out = new AnimGoalBuilder(AnimGoalType.attempt_spread_out)
                .SetPriority(AnimGoalPriority.AGP_2)
                .SetInterruptAll(true)
                .SetRelatedGoals(AnimGoalType.run_near_obj, AnimGoalType.move_near_obj);
            attempt_spread_out.AddState(AnimGoalActions.GoalSetRadiusToAiSpread) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_POP_ALL);
            attempt_spread_out.AddState(AnimGoalActions.GoalIsCloserThanDesiredSpread) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.move_away_from_obj))
                .OnFailure(T_GOTO_STATE(2));
            attempt_spread_out.AddState(AnimGoalActions.GoalIsTargetWithinRadius) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_GOTO_STATE(3));
            attempt_spread_out.AddState(AlwaysFail) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.move_near_obj))
                .OnFailure(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.run_near_obj));
            attempt_spread_out.AddState(AnimGoalActions.GoalActionPerform3) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1)
                .OnSuccess(T_REWIND, 100)
                .OnFailure(T_POP_ALL);
            Add(attempt_spread_out.Build());

            // AnimGoalType.animate_door_open
            var animate_door_open = new AnimGoalBuilder(AnimGoalType.animate_door_open)
                .SetPriority(AnimGoalPriority.AGP_3)
                .SetFieldC(true);
            animate_door_open.AddCleanup(AnimGoalActions.GoalOpenDoorCleanup)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            animate_door_open.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            animate_door_open.AddState(AnimGoalActions.GoalContinueWithDoorOpenAnim) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_GOTO_STATE(3));
            animate_door_open.AddState(AnimGoalActions.GoalPlayDoorOpenAnim) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_GOAL);
            animate_door_open.AddState(AnimGoalActions.GoalIsDoorSticky) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.pend_closing_door), 1500)
                .OnFailure(T_POP_GOAL);
            Add(animate_door_open.Build());

            // AnimGoalType.animate_door_closed
            var animate_door_closed = new AnimGoalBuilder(AnimGoalType.animate_door_closed)
                .SetPriority(AnimGoalPriority.AGP_3)
                .SetFieldC(true);
            animate_door_closed.AddCleanup(AnimGoalActions.GoalCloseDoorCleanup)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            animate_door_closed.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            animate_door_closed.AddState(AnimGoalActions.GoalContinueWithDoorCloseAnim) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_GOAL, DELAY_SLOT);
            animate_door_closed.AddState(AnimGoalActions.GoalPlayDoorCloseAnim) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_GOAL);
            Add(animate_door_closed.Build());

            // AnimGoalType.pend_closing_door
            var pend_closing_door = new AnimGoalBuilder(AnimGoalType.pend_closing_door)
                .SetPriority(AnimGoalPriority.AGP_3)
                .SetFieldC(true);
            pend_closing_door.AddState(AnimGoalActions.GoalIsLiveCritterNear) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.animate_door_closed), 1500)
                .OnFailure(T_REWIND, 1500);
            Add(pend_closing_door.Build());

            // AnimGoalType.throw_spell_friendly
            var throw_spell_friendly = new AnimGoalBuilder(AnimGoalType.throw_spell_friendly)
                .SetPriority(AnimGoalPriority.AGP_3)
                .SetInterruptAll(true)
                .SetRelatedGoals(AnimGoalType.attempt_spell);
            throw_spell_friendly.AddState(AlwaysSucceed) // Index 0
                .SetArgs(AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_POP_GOAL);
            throw_spell_friendly.AddState(AnimGoalActions.GoalIsProne) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_GOTO_STATE(2));
            throw_spell_friendly.AddState(AnimGoalActions.GoalIsConcealed) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(3))
                .OnFailure(T_GOTO_STATE(3));
            throw_spell_friendly.AddState(AnimGoalActions.GoalReturnTrue) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_GOTO_STATE(7));
            throw_spell_friendly.AddState(AlwaysSucceed) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(5))
                .OnFailure(T_PUSH_GOAL(AnimGoalType.pick_weapon));
            throw_spell_friendly.AddState(AlwaysSucceed) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(8))
                .OnFailure(T_POP_ALL);
            throw_spell_friendly.AddState(AnimGoalActions.GoalSaveStateDataOrSpellRangeInRadius) // Index 6
                .SetFlagsData(8)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.move_near_obj))
                .OnFailure(T_POP_ALL);
            throw_spell_friendly.AddState(AlwaysSucceed) // Index 7
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            throw_spell_friendly.AddState(AlwaysSucceed) // Index 8
                .OnSuccess(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.attempt_spell))
                .OnFailure(T_POP_ALL);
            Add(throw_spell_friendly.Build());

            // AnimGoalType.attempt_spell_friendly
            var attempt_spell_friendly = new AnimGoalBuilder(AnimGoalType.attempt_spell_friendly)
                .SetPriority(AnimGoalPriority.AGP_4)
                .SetInterruptAll(true)
                .SetRelatedGoals(AnimGoalType.throw_spell);
            attempt_spell_friendly.AddCleanup(AnimGoalActions.GoalCastConjureEnd)
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SKILL_DATA)
                .SetFlagsData(1);
            attempt_spell_friendly.AddState(AnimGoalActions.GoalIsNotStackFlagsData40) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(10))
                .OnFailure(T_GOTO_STATE(1));
            attempt_spell_friendly.AddState(AnimGoalActions.GoalSetSlotFlags4) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(3))
                .OnFailure(T_GOTO_STATE(7), DELAY_SLOT);
            attempt_spell_friendly.AddState(AlwaysSucceed) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND)
                .OnFailure(T_GOTO_STATE(11));
            attempt_spell_friendly.AddState(AnimGoalActions.GoalSlotFlagSet8If4AndNotSetYet) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_REWIND, DELAY_SLOT);
            attempt_spell_friendly.AddState(AnimGoalActions.GoalReturnFalse) // Index 4
                .SetArgs(AnimGoalProperty.SKILL_DATA)
                .OnSuccess(T_GOTO_STATE(5))
                .OnFailure(T_GOTO_STATE(8));
            attempt_spell_friendly.AddState(AnimGoalActions.GoalSpawnFireball) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SELF_OBJ_PRECISE_LOC)
                .SetFlagsData(5)
                .OnSuccess(T_GOTO_STATE(6))
                .OnFailure(T_GOTO_STATE(7));
            attempt_spell_friendly.AddState(AnimGoalActions.GoalStateCallback1) // Index 6
                .SetArgs(AnimGoalProperty.SCRATCH_OBJ, AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(29)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_ALL, DELAY_SLOT);
            attempt_spell_friendly.AddState(AlwaysSucceed) // Index 7
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_POP_ALL);
            attempt_spell_friendly.AddState(AnimGoalActions.GoalAttemptSpell) // Index 8
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SKILL_DATA)
                .OnSuccess(T_REWIND)
                .OnFailure(T_GOTO_STATE(7));
            attempt_spell_friendly.AddState(AlwaysSucceed) // Index 9
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            attempt_spell_friendly.AddState(AlwaysSucceed) // Index 10
                .SetArgs(AnimGoalProperty.SKILL_DATA)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(11));
            attempt_spell_friendly.AddState(AnimGoalActions.GoalAttemptSpell) // Index 11
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SKILL_DATA)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            Add(attempt_spell_friendly.Build());

            // AnimGoalType.animate_loop_fire_dmg
            var animate_loop_fire_dmg = new AnimGoalBuilder(AnimGoalType.animate_loop_fire_dmg)
                .SetPriority(AnimGoalPriority.AGP_1)
                .SetFieldC(true)
                .SetField10(true);
            animate_loop_fire_dmg.AddCleanup(AnimGoalActions.GoalFreeSoundHandle)
                .SetFlagsData(1);
            animate_loop_fire_dmg.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(3))
                .OnFailure(T_GOTO_STATE(1));
            animate_loop_fire_dmg.AddState(AnimGoalActions.GoalLoopWhileCloseToParty) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_REWIND, 800);
            animate_loop_fire_dmg.AddState(AnimGoalActions.GoalAnimateFireDmgContinueAnim) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_GOTO_STATE(4));
            animate_loop_fire_dmg.AddState(AnimGoalActions.GoalAnimateForever) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_POP_ALL);
            animate_loop_fire_dmg.AddState(AlwaysSucceed) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.PARENT_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_ALL);
            Add(animate_loop_fire_dmg.Build());

            // AnimGoalType.attempt_move_straight_spell
            var attempt_move_straight_spell = new AnimGoalBuilder(AnimGoalType.attempt_move_straight_spell)
                .SetPriority(AnimGoalPriority.AGP_3);
            attempt_move_straight_spell.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            attempt_move_straight_spell.AddState(AnimGoalActions.GoalContinueMoveStraight) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_CUSTOM)
                .OnFailure(T_POP_GOAL, DELAY_SLOT);
            attempt_move_straight_spell.AddState(AnimGoalActions.GoalPlayWaterRipples) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND)
                .OnFailure(T_POP_ALL);
            Add(attempt_move_straight_spell.Build());

            // AnimGoalType.move_near_obj_combat
            var move_near_obj_combat = new AnimGoalBuilder(AnimGoalType.move_near_obj_combat)
                .SetPriority(AnimGoalPriority.AGP_2);
            move_near_obj_combat.AddCleanup(AnimGoalActions.GoalResetToIdleAnim)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            move_near_obj_combat.AddState(AnimGoalActions.GoalIsTargetWithinRadius) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_GOTO_STATE(1));
            move_near_obj_combat.AddState(AnimGoalActions.GoalIsProne) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_GOTO_STATE(2));
            move_near_obj_combat.AddState(AnimGoalActions.GoalIsConcealed) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.unconceal))
                .OnFailure(T_GOTO_STATE(3));
            move_near_obj_combat.AddState(AnimGoalActions.GoalIsCurrentPathValid) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(6))
                .OnFailure(T_GOTO_STATE(4));
            move_near_obj_combat.AddState(AnimGoalActions.GoalFindPathNearObjectCombat) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(6))
                .OnFailure(T_GOTO_STATE(5));
            move_near_obj_combat.AddState(AlwaysFail) // Index 5
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.move_to_pause) | T_REWIND, 1000)
                .OnFailure(T_POP_ALL);
            move_near_obj_combat.AddState(AnimGoalActions.GoalPlaySoundScratch6) // Index 6
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(7))
                .OnFailure(T_GOTO_STATE(7));
            move_near_obj_combat.AddState(AnimGoalActions.GoalAttackerHasRangedWeapon) // Index 7
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.attempt_move_near_combat))
                .OnFailure(T_PUSH_GOAL(AnimGoalType.attempt_move_near));
            Add(move_near_obj_combat.Build());

            // AnimGoalType.attempt_move_near_combat
            var attempt_move_near_combat = new AnimGoalBuilder(AnimGoalType.attempt_move_near_combat)
                .SetPriority(AnimGoalPriority.AGP_2);
            attempt_move_near_combat.AddCleanup(AnimGoalActions.GoalAttemptMoveCleanup)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            attempt_move_near_combat.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(3))
                .OnFailure(T_GOTO_STATE(1));
            attempt_move_near_combat.AddState(AlwaysFail) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(9), DELAY_RANDOM)
                .OnFailure(T_GOTO_STATE(2));
            attempt_move_near_combat.AddState(AnimGoalActions.GoalMoveAlongPath) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_GOAL, DELAY_SLOT);
            attempt_move_near_combat.AddState(AnimGoalActions.GoalIsCurrentPathValid) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_GOTO_STATE(11));
            attempt_move_near_combat.AddState(AnimGoalActions.GoalMoveNearUpdateRadiusToReach) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_POP_GOAL | 0x8000000)
                .OnFailure(T_GOTO_STATE(5));
            attempt_move_near_combat.AddState(AlwaysFail) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(10))
                .OnFailure(T_GOTO_STATE(6));
            attempt_move_near_combat.AddState(AnimGoalActions.GoalHasDoorInPath) // Index 6
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(7))
                .OnFailure(T_GOTO_STATE(8));
            attempt_move_near_combat.AddState(AnimGoalActions.GoalIsDoorFullyClosed) // Index 7
                .SetArgs(AnimGoalProperty.SCRATCH_OBJ)
                .OnSuccess(T_INVALIDATE_PATH | T_PUSH_GOAL(AnimGoalType.open_door) | T_REWIND, 50)
                .OnFailure(T_GOTO_STATE(8));
            attempt_move_near_combat.AddState(AnimGoalActions.GoalPlayMoveAnim) // Index 8
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND)
                .OnFailure(T_GOTO_STATE(9));
            attempt_move_near_combat.AddState(AnimGoalActions.GoalActionPerform3) // Index 9
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            attempt_move_near_combat.AddState(AnimGoalActions.GoalIsDoorUnlockedAlwaysReturnFalse) // Index 10
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SCRATCH_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.jump_window) | T_REWIND)
                .OnFailure(T_POP_ALL);
            attempt_move_near_combat.AddState(AlwaysFail) // Index 11
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.move_to_pause) | T_REWIND, 1000)
                .OnFailure(T_POP_ALL);
            Add(attempt_move_near_combat.Build());

            // AnimGoalType.use_container
            var use_container = new AnimGoalBuilder(AnimGoalType.use_container)
                .SetPriority(AnimGoalPriority.AGP_3);
            use_container.AddState(AnimGoalActions.GoalSetRadiusTo2) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_POP_ALL);
            use_container.AddState(AnimGoalActions.GoalIsTargetWithinRadius) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_PUSH_GOAL(AnimGoalType.move_near_obj));
            use_container.AddState(AnimGoalActions.GoalIsRotatedTowardTarget) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(3))
                .OnFailure(T_PUSH_GOAL(AnimGoalType.rotate));
            use_container.AddState(AnimGoalActions.GoalIsParam1Door) // Index 3
                .SetArgs(AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(5))
                .OnFailure(T_GOTO_STATE(4));
            use_container.AddState(AnimGoalActions.GoalUseObject) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            use_container.AddState(AnimGoalActions.GoalSaveParam1InScratch) // Index 5
                .SetArgs(AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(6))
                .OnFailure(T_POP_ALL);
            use_container.AddState(AnimGoalActions.GoalIsDoorFullyClosed) // Index 6
                .SetArgs(AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.open_door))
                .OnFailure(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.close_door));
            Add(use_container.Build());

            // AnimGoalType.throw_spell_w_cast_anim
            var throw_spell_w_cast_anim = new AnimGoalBuilder(AnimGoalType.throw_spell_w_cast_anim)
                .SetPriority(AnimGoalPriority.AGP_3)
                .SetRelatedGoals(AnimGoalType.attempt_spell);
            throw_spell_w_cast_anim.AddCleanup(AnimGoalActions.GoalResetToIdleAnim)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            throw_spell_w_cast_anim.AddState(AlwaysSucceed) // Index 0
                .SetArgs(AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_GOTO_STATE(9));
            throw_spell_w_cast_anim.AddState(AnimGoalActions.GoalIsProne) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_GOTO_STATE(2));
            throw_spell_w_cast_anim.AddState(AnimGoalActions.GoalIsConcealed) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.unconceal))
                .OnFailure(T_GOTO_STATE(3));
            throw_spell_w_cast_anim.AddState(AnimGoalActions.GoalReturnTrue) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_GOTO_STATE(7));
            throw_spell_w_cast_anim.AddState(AlwaysSucceed) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(5))
                .OnFailure(T_PUSH_GOAL(AnimGoalType.pick_weapon));
            throw_spell_w_cast_anim.AddState(AlwaysSucceed) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(8))
                .OnFailure(T_GOTO_STATE(6));
            throw_spell_w_cast_anim.AddState(AnimGoalActions.GoalSaveStateDataOrSpellRangeInRadius) // Index 6
                .SetArgs(AnimGoalProperty.SKILL_DATA)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.move_near_obj))
                .OnFailure(T_GOTO_STATE(9));
            throw_spell_w_cast_anim.AddState(AlwaysSucceed) // Index 7
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(9))
                .OnFailure(T_GOTO_STATE(9));
            throw_spell_w_cast_anim.AddState(AnimGoalActions.GoalSetRotationToFaceTargetLoc) // Index 8
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_TILE)
                .OnSuccess(T_POP_GOAL | T_PUSH_GOAL(AnimGoalType.attempt_spell_w_cast_anim))
                .OnFailure(T_GOTO_STATE(9));
            throw_spell_w_cast_anim.AddState(AnimGoalActions.GoalCastConjureEnd) // Index 9
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SKILL_DATA)
                .SetFlagsData(1)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            Add(throw_spell_w_cast_anim.Build());

            // AnimGoalType.attempt_spell_w_cast_anim
            var attempt_spell_w_cast_anim = new AnimGoalBuilder(AnimGoalType.attempt_spell_w_cast_anim)
                .SetPriority(AnimGoalPriority.AGP_4)
                .SetRelatedGoals(AnimGoalType.throw_spell);
            attempt_spell_w_cast_anim.AddCleanup(AnimGoalActions.GoalCastConjureEnd)
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SKILL_DATA)
                .SetFlagsData(1);
            attempt_spell_w_cast_anim.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(10))
                .OnFailure(T_GOTO_STATE(1));
            attempt_spell_w_cast_anim.AddState(AnimGoalActions.GoalUnconcealAnimate) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(3))
                .OnFailure(T_GOTO_STATE(7), DELAY_SLOT);
            attempt_spell_w_cast_anim.AddState(AlwaysSucceed) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND)
                .OnFailure(T_GOTO_STATE(12));
            attempt_spell_w_cast_anim.AddState(AnimGoalActions.GoalSlotFlagSet8If4AndNotSetYet) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_GOTO_STATE(13))
                .OnFailure(T_REWIND, DELAY_SLOT);
            attempt_spell_w_cast_anim.AddState(AnimGoalActions.GoalReturnFalse) // Index 4
                .SetArgs(AnimGoalProperty.SKILL_DATA)
                .OnSuccess(T_GOTO_STATE(5))
                .OnFailure(T_GOTO_STATE(8));
            attempt_spell_w_cast_anim.AddState(AnimGoalActions.GoalSpawnFireball) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SELF_OBJ_PRECISE_LOC)
                .SetFlagsData(5)
                .OnSuccess(T_GOTO_STATE(6))
                .OnFailure(T_GOTO_STATE(7));
            attempt_spell_w_cast_anim.AddState(AnimGoalActions.GoalStateCallback1) // Index 6
                .SetArgs(AnimGoalProperty.SCRATCH_OBJ, AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(38)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_ALL, DELAY_SLOT);
            attempt_spell_w_cast_anim.AddState(AlwaysSucceed) // Index 7
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_POP_ALL);
            attempt_spell_w_cast_anim.AddState(AnimGoalActions.GoalAttemptSpell) // Index 8
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SKILL_DATA)
                .OnSuccess(T_REWIND)
                .OnFailure(T_GOTO_STATE(7));
            attempt_spell_w_cast_anim.AddState(AnimGoalActions.GoalActionPerform3) // Index 9
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            attempt_spell_w_cast_anim.AddState(AlwaysSucceed) // Index 10
                .SetArgs(AnimGoalProperty.SKILL_DATA)
                .OnSuccess(T_GOTO_STATE(11))
                .OnFailure(T_GOTO_STATE(12));
            attempt_spell_w_cast_anim.AddState(AnimGoalActions.GoalTriggerSpell) // Index 11
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SKILL_DATA)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(12));
            attempt_spell_w_cast_anim.AddState(AnimGoalActions.GoalAttemptSpell) // Index 12
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.SKILL_DATA)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            attempt_spell_w_cast_anim.AddState(AnimGoalActions.GoalWasInterrupted) // Index 13
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(10))
                .OnFailure(T_GOTO_STATE(14));
            attempt_spell_w_cast_anim.AddState(AnimGoalActions.GoalIsAnimatingConjuration) // Index 14
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_GOTO_STATE(15))
                .OnFailure(T_GOTO_STATE(4));
            attempt_spell_w_cast_anim.AddState(AnimGoalActions.GoalStartConjurationAnim) // Index 15
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_ALL);
            Add(attempt_spell_w_cast_anim.Build());

            // AnimGoalType.throw_spell_w_cast_anim_2ndary
            var throw_spell_w_cast_anim_2ndary = new AnimGoalBuilder(AnimGoalType.throw_spell_w_cast_anim_2ndary)
                .SetPriority(AnimGoalPriority.AGP_5)
                .SetInterruptAll(true)
                .SetFieldC(true);
            throw_spell_w_cast_anim_2ndary.AddCleanup(AlwaysSucceed)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            throw_spell_w_cast_anim_2ndary.AddState(AnimGoalActions.GoalIsNotStackFlagsData40) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            throw_spell_w_cast_anim_2ndary.AddState(AnimGoalActions.GoalSetSlotFlags4) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_GOTO_STATE(4));
            throw_spell_w_cast_anim_2ndary.AddState(AlwaysSucceed) // Index 2
                .SetArgs(AnimGoalProperty.SKILL_DATA)
                .OnSuccess(T_GOTO_STATE(3), DELAY_SLOT)
                .OnFailure(T_POP_ALL);
            throw_spell_w_cast_anim_2ndary.AddState(AlwaysSucceed) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_ALL);
            throw_spell_w_cast_anim_2ndary.AddState(AlwaysSucceed) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_GOTO_STATE(2));
            Add(throw_spell_w_cast_anim_2ndary.Build());

            // AnimGoalType.back_off_from
            var back_off_from = new AnimGoalBuilder(AnimGoalType.back_off_from)
                .SetPriority(AnimGoalPriority.AGP_3);
            back_off_from.AddState(AnimGoalActions.GoalSetRunningFlag) // Index 0
                .OnSuccess(T_GOTO_STATE(1))
                .OnFailure(T_GOTO_STATE(1));
            back_off_from.AddState(AnimGoalActions.GoalSaveStateDataOrSpellRangeInRadius) // Index 1
                .SetFlagsData(9)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_POP_ALL);
            back_off_from.AddState(AnimGoalActions.GoalIsTargetWithinRadius) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_PUSH_GOAL(AnimGoalType.move_away_from_obj))
                .OnFailure(T_GOTO_STATE(3));
            back_off_from.AddState(AnimGoalActions.GoalActionPerform3) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1)
                .OnSuccess(T_POP_GOAL, DELAY_SLOT)
                .OnFailure(T_POP_ALL);
            Add(back_off_from.Build());

            // AnimGoalType.attempt_use_pickpocket_skill_on
            var attempt_use_pickpocket_skill_on = new AnimGoalBuilder(AnimGoalType.attempt_use_pickpocket_skill_on)
                .SetPriority(AnimGoalPriority.AGP_3);
            attempt_use_pickpocket_skill_on.AddCleanup(AnimGoalActions.GoalResetToIdleAnim)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            attempt_use_pickpocket_skill_on.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            attempt_use_pickpocket_skill_on.AddState(AnimGoalActions.GoalUnconcealAnimate) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_GOTO_STATE(7), DELAY_SLOT);
            attempt_use_pickpocket_skill_on.AddState(AnimGoalActions.GoalSetRotationToFaceTargetObj) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(3))
                .OnFailure(T_POP_ALL);
            attempt_use_pickpocket_skill_on.AddState(AnimGoalActions.GoalPickpocketPerform) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_POP_ALL);
            attempt_use_pickpocket_skill_on.AddState(AnimGoalActions.GoalCheckSlotFlag40000) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(7))
                .OnFailure(T_GOTO_STATE(5));
            attempt_use_pickpocket_skill_on.AddState(AnimGoalActions.GoalPlayAnim) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(PackAnimId(WeaponAnim.Idle))
                .OnSuccess(T_GOTO_STATE(6))
                .OnFailure(T_POP_ALL);
            attempt_use_pickpocket_skill_on.AddState(AnimGoalActions.GoalThrowItemPlayAnim) // Index 6
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_REWIND)
                .OnFailure(T_GOTO_STATE(8));
            attempt_use_pickpocket_skill_on.AddState(AnimGoalActions.GoalPlayAnim) // Index 7
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(PackAnimId(WeaponAnim.Idle))
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_POP_ALL);
            attempt_use_pickpocket_skill_on.AddState(AnimGoalActions.GoalActionPerform3) // Index 8
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1)
                .OnSuccess(T_POP_ALL)
                .OnFailure(T_POP_ALL);
            Add(attempt_use_pickpocket_skill_on.Build());

            // AnimGoalType.use_disable_device_skill_on_data
            var use_disable_device_skill_on_data = new AnimGoalBuilder(AnimGoalType.use_disable_device_skill_on_data)
                .SetPriority(AnimGoalPriority.AGP_3);
            use_disable_device_skill_on_data.AddCleanup(AnimGoalActions.GoalResetToIdleAnim)
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(1);
            use_disable_device_skill_on_data.AddState(AnimGoalActions.GoalIsSlotFlag10NotSet) // Index 0
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_GOTO_STATE(2))
                .OnFailure(T_GOTO_STATE(1));
            use_disable_device_skill_on_data.AddState(AnimGoalActions.GoalUnconcealAnimate) // Index 1
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_GOTO_STATE(5), DELAY_SLOT);
            use_disable_device_skill_on_data.AddState(AnimGoalActions.GoalSetRotationToFaceTargetObj) // Index 2
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_GOTO_STATE(3))
                .OnFailure(T_POP_ALL);
            use_disable_device_skill_on_data.AddState(AnimGoalActions.GoalPlayAnim) // Index 3
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(PackAnimId(NormalAnimType.SkillDisableDevice))
                .OnSuccess(T_GOTO_STATE(4))
                .OnFailure(T_POP_ALL);
            use_disable_device_skill_on_data.AddState(AnimGoalActions.GoalThrowItemPlayAnim) // Index 4
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.ANIM_ID_PREV)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_GOTO_STATE(6));
            use_disable_device_skill_on_data.AddState(AnimGoalActions.GoalAttemptTrapDisarm) // Index 5
                .SetArgs(AnimGoalProperty.SELF_OBJ, AnimGoalProperty.TARGET_OBJ)
                .OnSuccess(T_REWIND, DELAY_SLOT)
                .OnFailure(T_POP_ALL);
            use_disable_device_skill_on_data.AddState(AnimGoalActions.GoalPlayAnim) // Index 6
                .SetArgs(AnimGoalProperty.SELF_OBJ)
                .SetFlagsData(PackAnimId(WeaponAnim.Idle))
                .OnSuccess(T_POP_GOAL)
                .OnFailure(T_POP_ALL);
            Add(use_disable_device_skill_on_data.Build());
        }

        [TempleDllLocation(0x10262530)]
        private static bool AlwaysFail(AnimSlot slot) => false;

        [TempleDllLocation(0x101f5850)]
        private static bool AlwaysSucceed(AnimSlot slot) => true;

        public AnimGoal GetByType(AnimGoalType type)
        {
            return _goals[type];
        }

        public bool IsValidType(AnimGoalType type)
        {
            return _goals.ContainsKey(type);
        }
    }
}