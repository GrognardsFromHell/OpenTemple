using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.GameObjects;

namespace SpicyTemple.Core.Systems.Fade
{
    public struct FadeAndTeleportArgs
    {
        public int flags; // FadeAndTeleportFlags
        public int field4;
        public ObjHndl somehandle;
        public locXY destLoc;
        public int destMap;
        public int movieId;
        public int field20;
        public int field24;
        public int field28;
        public int field2c;
        public PackedLinearColorA color;
        public int field34;
        public float somefloat;
        public int field3c;
        public int field40;
        public int field44;
        public int field48;
        public PackedLinearColorA field4c;
        public int field50;
        public float somefloat2;
        public int field58;
        public int field5c;
        public int field60;
        public int soundId;
        public int timeToAdvance;
        public int field6c;
        public int field70;
        public int field74;

        public static FadeAndTeleportArgs Default => new FadeAndTeleportArgs
        {
            color = new PackedLinearColorA(0, 0, 0, 255)
        };

    }
}