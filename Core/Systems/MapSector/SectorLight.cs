using System.Numerics;
using OpenTemple.Core.GFX;

namespace OpenTemple.Core.Systems.MapSector;

public enum LegacyLightType : uint
{
    LLT_POINT = 1,
    LLT_SPOT = 2,
    LLT_DIRECTIONAL = 3
};

public struct LegacyLight
{
    public LegacyLightType type;
    public LinearColor Color;
    public Vector3 pos;
    public Vector3 dir;
    public float range;
    public float phi;
}