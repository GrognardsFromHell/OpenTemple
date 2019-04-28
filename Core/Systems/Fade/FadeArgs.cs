using SpicyTemple.Core.GFX;

namespace SpicyTemple.Core.Systems.Fade
{
    public struct FadeArgs
    {
        public int flags;
        public PackedLinearColorA color;
        public int countSthgUsually48;
        public float transitionTime;
        public int field10;
        public int field14;
        public int hoursToPass;

        public static FadeArgs Default => new FadeArgs();

    }
}