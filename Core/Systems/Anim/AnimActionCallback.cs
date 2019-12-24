using OpenTemple.Core.GameObject;

namespace OpenTemple.Core.Systems.Anim
{
    public struct AnimActionCallback
    {
        public readonly GameObjectBody obj;
        public readonly int uniqueId;

        public AnimActionCallback(GameObjectBody obj, int uniqueId)
        {
            this.obj = obj;
            this.uniqueId = uniqueId;
        }
    }
}