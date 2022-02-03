using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;

namespace OpenTemple.Core.Systems.ObjScript;

public struct ObjScriptInvocation
{
    public ObjectScript script;
    public GameObject triggerer;
    public GameObject attachee;
    public SpellPacketBody spell;
    public ObjScriptEvent eventId;
    public TrapSprungEvent trapEvent;
}