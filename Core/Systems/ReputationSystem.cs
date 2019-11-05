using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Systems
{
    public class ReputationSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem
    {
        [TempleDllLocation(0x10aa46b4)]
        private readonly Dictionary<int, Reputation> _reputations = new Dictionary<int, Reputation>();

        [TempleDllLocation(0x10aa36d8)]
        private readonly Dictionary<int, EarnedReputation> _earnedReputations = new Dictionary<int, EarnedReputation>();

        [TempleDllLocation(0x10aa3704)]
        private readonly Dictionary<int, string> _translations;

        private readonly Dictionary<int, string> _greetingsMaleNpcMalePc;
        private readonly Dictionary<int, string> _greetingsMaleNpcFemalePc;
        private readonly Dictionary<int, string> _greetingsFemaleNpcMalePc;
        private readonly Dictionary<int, string> _greetingsFemaleNpcFemalePc;

        [TempleDllLocation(0x10054b00)]
        public ReputationSystem()
        {
            _translations = Tig.FS.ReadMesFile("mes/gamereplog.mes");
            _greetingsMaleNpcMalePc = Tig.FS.ReadMesFile("mes/game_rp_npc_m2m.mes");
            _greetingsMaleNpcFemalePc = Tig.FS.ReadMesFile("mes/game_rp_npc_m2f.mes");
            _greetingsFemaleNpcMalePc = Tig.FS.ReadMesFile("mes/game_rp_npc_f2m.mes");
            _greetingsFemaleNpcFemalePc = Tig.FS.ReadMesFile("mes/game_rp_npc_f2f.mes");

            Stub.TODO();
        }

        public void Dispose()
        {
        }

        [TempleDllLocation(0x100542a0)]
        public void Reset()
        {
            _earnedReputations.Clear();
        }

        [TempleDllLocation(0x100542d0)]
        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100542f0)]
        public bool LoadGame()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10054d70)]
        public bool HasFactionFromReputation(GameObjectBody pc, int faction)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10054BD0)]
        public int GetReactionModFromReputation(GameObjectBody pc, GameObjectBody npc)
        {
            Stub.TODO();
            return 0;
        }

        [TempleDllLocation(0x100546e0)]
        public bool HasReputation(GameObjectBody pc, int reputation)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10054740)]
        public void AddReputation(GameObjectBody pc, int reputation)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10054820)]
        public void RemoveReputation(GameObjectBody pc, int reputation)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns a random reputation that is affecting the NPC's reaction towards the player.
        /// </summary>
        [TempleDllLocation(0x10054cb0)]
        public bool TryGetReputationAffectingReaction(GameObjectBody pc, GameObjectBody npc, out int reputationId)
        {
            reputationId = 0;
            if (!pc.IsPC() || !npc.IsNPC())
            {
                return false;
            }

            if (_earnedReputations.Count == 0)
            {
                return false;
            }

            // TODO: I think this is busted because it does not disregard current reputation influences and then will drop reputations because of the reputation itself further down below
            var currentReaction = GameSystems.Reaction.GetReaction(npc, pc);
            var currentReactionLevel = GameSystems.Reaction.GetReactionLevel(currentReaction);

            var foundReputations = new List<int>();

            foreach (var earnedReputation in _earnedReputations.Values)
            {
                if (!earnedReputation.IsEarned)
                {
                    continue;
                }

                var reputation = _reputations[earnedReputation.Id];

                foreach (var reactionModifier in reputation.ReactionModifiers)
                {
                    if (GameSystems.Critter.HasFaction(npc, reactionModifier.FactionId))
                    {
                        // Only record the reputation if it's modifier could bring us over the neutral mark
                        if (reactionModifier.Modifier < 0 && currentReactionLevel > 3)
                        {
                            foundReputations.Add(earnedReputation.Id);
                        }
                        else if (reactionModifier.Modifier > 0 && currentReactionLevel < 3)
                        {
                            foundReputations.Add(earnedReputation.Id);
                        }
                    }
                }
            }

            if (foundReputations.Count > 0)
            {
                reputationId = GameSystems.Random.PickRandom(foundReputations);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a greeting message referencing an earned reputation (either positive or negative).
        /// </summary>
        [TempleDllLocation(0x10054850)]
        public bool TryGetGreeting(GameObjectBody pc, GameObjectBody npc, int reputationId, out string greeting)
        {
            if (!pc.IsPC() || !npc.IsNPC())
            {
                greeting = null;
                return false;
            }

            Dictionary<int, string> greetings;
            if (npc.GetGender() == Gender.Male)
            {
                greetings = pc.GetGender() == Gender.Male ? _greetingsMaleNpcMalePc : _greetingsMaleNpcFemalePc;
            }
            else
            {
                greetings = pc.GetGender() == Gender.Male ? _greetingsFemaleNpcMalePc : _greetingsFemaleNpcFemalePc;
            }

            return greetings.TryGetValue(reputationId, out greeting);
        }

        private struct EarnedReputation
        {
            public bool IsEarned;
            public int Id;
            public TimePoint TimeEarned;
        }

        private class Reputation
        {
            public List<ReactionModifier> ReactionModifiers = new List<ReactionModifier>();
        }

        private struct ReactionModifier
        {
            public int FactionId;
            public int Modifier;
        }
    }
}