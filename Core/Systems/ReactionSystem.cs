using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Systems
{
    public class ReactionSystem : IGameSystem
    {
        public void Dispose()
        {
        }

        [TempleDllLocation(0x10AA36D0)]
        private GameObjectBody _reactionNpcObject;

        [TempleDllLocation(0x10AA36A8)]
        private GameObjectBody _reactionPlayerObject;

        [TempleDllLocation(0x10053ca0)]
        public GameObjectBody GetLastReactionPlayer(GameObjectBody npc)
        {
            if (_reactionNpcObject == npc)
            {
                return _reactionPlayerObject;
            }

            return null;
        }

        [TempleDllLocation(0x10053cd0)]
        public bool HasMet(GameObjectBody who, GameObjectBody whom)
        {
            if (who == whom)
            {
                return true;
            }

            if (!whom.IsPC() || !who.IsNPC())
            {
                return false;
            }

            if (who.GetCritterFlags().HasFlag(CritterFlag.FATIGUE_LIMITING))
            {
                return true;
            }

            return who.GetCritterFlags().HasFlag(CritterFlag.FATIGUE_LIMITING);
        }
    }
}