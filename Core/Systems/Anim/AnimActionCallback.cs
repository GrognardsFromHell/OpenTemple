using System.Diagnostics;
using OpenTemple.Core.GameObjects;

namespace OpenTemple.Core.Systems.Anim;

public readonly struct AnimActionCallback
{
    public readonly GameObject obj;
    public readonly int uniqueId;

    public AnimActionCallback(GameObject obj, int uniqueId)
    {
        this.obj = obj;
        this.uniqueId = uniqueId;
    }
}