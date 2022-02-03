using System.Numerics;
using OpenTemple.Core.GameObjects;

#nullable enable

namespace OpenTemple.Core.Systems.Vfx;

public class ChainLightningTarget
{
    public GameObject? Object { get; }
    public Vector3 Location { get; }
    public bool IsEffectTriggered { get; private set; }

    public ChainLightningTarget(Vector3 location)
    {
        Object = null;
        Location = location;
    }

    public ChainLightningTarget(GameObject obj, Vector3 location)
    {
        Object = obj;
        Location = location;
    }

    public void TriggerEffect()
    {
        if (!IsEffectTriggered)
        {
            if (Object != null)
            {
                GameSystems.SoundGame.PositionalSound(7027, 1, Object);
                GameSystems.ParticleSys.CreateAtObj("sp-Chain Lightning-hit", Object);
            }

            IsEffectTriggered = true;
        }
    }
}