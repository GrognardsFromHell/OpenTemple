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

        [TempleDllLocation(0x10AA36A4)]
        private bool reactionState;

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

        [TempleDllLocation(0x10054180)]
        [TempleDllLocation(0x10053e90)]
        public int GetReaction(GameObjectBody critter, GameObjectBody towards)
        {
            if (GameSystems.AI.NpcAiListFindEnemy(critter, towards))
            {
                return 0;
            }
            else if (towards.IsPC() && critter.IsNPC())
            {
                var baseReaction = NpcReactionLevelGet(critter);
                return baseReaction + GameSystems.Reputation.GetReactionModFromReputation(towards, critter);
            }
            else
            {
                return 50;
            }
        }


        [TempleDllLocation(0x10053D60)]
        public int NpcReactionLevelGet(GameObjectBody obj)
        {
            if (_reactionNpcObject == obj && _reactionPlayerObject != null && reactionState)
            {
                return obj.GetInt32(obj_f.npc_reaction_level_idx, 0);
            }
            else if (!obj.GetCritterFlags().HasFlag(CritterFlag.FATIGUE_LIMITING))
            {
                return obj.GetInt32(obj_f.npc_reaction_base);
            }
            else
            {
                return obj.GetInt32(obj_f.npc_reaction_level_idx, 1);
            }
        }
    }
}