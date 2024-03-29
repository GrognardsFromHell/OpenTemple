using System;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.Fade;

namespace OpenTemple.Core.Systems.Teleport;

public struct FadeAndTeleportArgs
{
    public FadeAndTeleportFlags flags; // FadeAndTeleportFlags
    public int field4;
    public GameObject? somehandle;
    public locXY destLoc;
    public int destMap;
    public int movieId;
    public int movieId2;
    public FadeArgs FadeOutArgs;
    public FadeArgs FadeInArgs;
    public int soundId;
    public int timeToAdvance; // In seconds
    public Action<int>? callback; // Activate via flag 0x400
    public int callbackArg; // Activate via flag 0x800
    public int field74;

    public static FadeAndTeleportArgs Default => new()
    {
        FadeOutArgs = new FadeArgs
        {
            color = PackedLinearColorA.Black
        }
    };
}