using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;

namespace SpicyTemple.Core.Systems.ObjScript
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