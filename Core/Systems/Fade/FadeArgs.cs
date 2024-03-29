using OpenTemple.Core.GFX;

namespace OpenTemple.Core.Systems.Fade;

public struct FadeArgs
{
    public FadeFlag flags;
    public PackedLinearColorA color;
    public int fadeSteps;
    public float transitionTime;
    public int field10;

    public static FadeArgs Default => new();

}