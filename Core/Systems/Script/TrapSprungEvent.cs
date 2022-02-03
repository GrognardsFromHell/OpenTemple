using OpenTemple.Core.GameObjects;

namespace OpenTemple.Core.Systems.Script;

public class TrapSprungEvent
{

    /// <summary>
    /// The trap game object.
    /// </summary>
    public GameObject Object { get; }

    /// <summary>
    /// Definition of the trap type.
    /// </summary>
    public Trap Type { get; }

}