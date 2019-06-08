using System.Collections.Generic;

namespace SpicyTemple.Core.Systems.Anim
{
    public class AnimGoal
    {
        public AnimGoal(AnimGoalType type)
        {
            Type = type;
        }

        public AnimGoalType Type { get; }

        public int statecount = 0;
        public AnimGoalPriority priority = AnimGoalPriority.AGP_NONE;
        public bool interruptAll = false;
        public int field_C = 0; // Indicates that it should be saved
        public int field_10 = 0;
        public List<AnimGoalType> relatedGoal = new List<AnimGoalType>();
        public List<AnimGoalState> states = new List<AnimGoalState>();
        public AnimGoalState? state_special;
    }
}