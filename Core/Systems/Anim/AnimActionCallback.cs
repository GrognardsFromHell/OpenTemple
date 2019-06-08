using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Systems.Anim
{
    public struct AnimActionCallback
    {
        public GameObjectBody obj;
        public uint uniqueId;

        public AnimActionCallback(GameObjectBody obj, uint uniqueId)
        {
            this.obj = obj;
            this.uniqueId = uniqueId;
        }
    }
}