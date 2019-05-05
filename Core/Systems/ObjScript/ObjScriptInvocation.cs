using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Systems.ObjScript
{
    public struct ObjScriptInvocation
    {
        public ObjectScript script;
        public int field4;
        public GameObjectBody triggerer;
        public GameObjectBody attachee;
        public int spellId;
        public int arg4;
        public ObjScriptEvent eventId;
    }
}