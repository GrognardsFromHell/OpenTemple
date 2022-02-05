using System.Collections.Generic;

namespace OpenTemple.Core.Systems.Anim;

public class AnimGoal
{
    public AnimGoal(AnimGoalType type)
    {
        Type = type;
    }

    public AnimGoalType Type { get; }

    public int statecount = 0;
    public AnimGoalPriority priority = AnimGoalPriority.AGP_NONE;
    // When this is true, this goal cannot be interrupted
    public bool interruptAll = false;
    public bool PersistOnAreaTransition = false; // Indicates that it should be saved
    public int field_10 = 0;
    public List<AnimGoalType> relatedGoal = new();
    public List<AnimGoalState> states = new();
    public AnimGoalState? state_special;
}