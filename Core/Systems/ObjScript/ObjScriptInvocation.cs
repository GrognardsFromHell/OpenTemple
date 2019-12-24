using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;

namespace OpenTemple.Core.Systems.ObjScript
{
    public struct ObjScriptInvocation
    {
        public ObjectScript script;
        public GameObjectBody triggerer;
        public GameObjectBody attachee;
        public SpellPacketBody spell;
        public ObjScriptEvent eventId;
        public TrapSprungEvent trapEvent;
    }
}