namespace OpenTemple.Core.Systems.Anim;

public struct AnimGoalState
{
    public AnimGoalStateCallback callback;
    public int argInfo1;
    public int argInfo2;
    public int flagsData;
    public AnimStateTransition afterFailure;
    public AnimStateTransition afterSuccess;
}