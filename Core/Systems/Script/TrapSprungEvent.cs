using OpenTemple.Core.GameObjects;

namespace OpenTemple.Core.Systems.Script;

/// <param name="Object">The trap game object.</param>
/// <param name="Type">Definition of the trap type.</param>
public readonly record struct TrapSprungEvent(
    GameObject Object,
    Trap Type
);
