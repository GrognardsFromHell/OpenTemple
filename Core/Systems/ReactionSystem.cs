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

        [TempleDllLocation(0x10053f20)]
        public void AdjustReaction(GameObjectBody npc, GameObjectBody towards, int adjustment)
        {
            if (adjustment == 0)
            {
                // Nothing to adjust.
                return;
            }

            if (!towards.IsPC() || !npc.IsNPC())
            {
                // Reaction is only tracked from NPCs towards PCs
                return;
            }

            if (!GameSystems.Party.IsInParty(npc) || !GameSystems.Party.IsInParty(towards) || adjustment >= 0)
            {
                var currentReaction = GameSystems.Reaction.NpcReactionLevelGet(npc);
                SetReaction(npc, currentReaction + adjustment);

                // When reaction towards our leader is adjusted, make the AI recheck if we actually
                // want to follow them anymore...
                var leader = GameSystems.Critter.GetLeader(npc);
                if (leader == towards)
                {
                    var npcFlags = npc.GetNPCFlags() | NpcFlag.CHECK_LEADER;
                    npc.SetNPCFlags(npcFlags);
                }
            }
        }

        [TempleDllLocation(0x10053df0)]
        private void SetReaction(GameObjectBody npc, int reactionLvl)
        {
            if (GameSystems.Reaction._reactionNpcObject == npc && GameSystems.Reaction._reactionPlayerObject != null)
            {
                npc.SetInt32(obj_f.npc_reaction_level_idx, 0, reactionLvl);
                // TODO: I still think this is a bug because it just uses a 0-59 time value...
                var time = GameSystems.TimeEvent.SecondOfMinute;
                npc.SetInt32(obj_f.npc_reaction_time_idx, 0, time);
                reactionState = true;
            }
            else
            {
                npc.SetInt32(obj_f.npc_reaction_level_idx, 1, reactionLvl);
                // TODO: I still think this is a bug because it just uses a 0-59 time value...
                var time = GameSystems.TimeEvent.SecondOfMinute;
                npc.SetInt32(obj_f.npc_reaction_time_idx, 1, time);
            }
        }
    }
}