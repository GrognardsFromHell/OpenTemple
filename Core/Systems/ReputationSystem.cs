using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.Systems
{
    public class ReputationSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        [TempleDllLocation(0x10aa46b4)]
        private readonly Dictionary<int, Reputation> _reputations;

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

            _reputations = LoadReputations();

            Stub.TODO();
        }

        private static Dictionary<int, Reputation> LoadReputations()
        {
            var result = new Dictionary<int, Reputation>();
            var lines = Tig.FS.ReadMesFile("rules/gamerep.mes");
            foreach (var (repId, line) in lines)
            {
                // Used to cause Paladins to fall
                bool isEvil;
                switch (repId)
                {
                    case 1:
                    case 4:
                    case 6:
                    case 7:
                    case 8:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                        isEvil = true;
                        break;
                    default:
                        isEvil = false;
                        break;
                }

                var factionRepSplit = line.IndexOf(',');
                if (factionRepSplit == -1)
                {
                    throw new InvalidOperationException("Malformed reputation line: " + line);
                }

                var grantedFactions =
                    line.Substring(0, factionRepSplit)
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .Where(f => f != 0)
                        .ToImmutableList();

                var effectStrings = line.Substring(factionRepSplit + 1)
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);
                // Go in and remove trailing commas, as the Co8 file is actually broken
                for (var i = 0; i < effectStrings.Length; i++)
                {
                    if (effectStrings[i].EndsWith(","))
                    {
                        effectStrings[i] = effectStrings[i].Substring(0, effectStrings[i].Length - 1);
                    }
                }

                var reactionMods = ImmutableList.CreateBuilder<ReactionModifier>();
                for (var i = 0; i + 1 < effectStrings.Length; i += 2)
                {
                    var value = int.Parse(effectStrings[i], CultureInfo.InvariantCulture);
                    var faction = int.Parse(effectStrings[i + 1], CultureInfo.InvariantCulture);
                    reactionMods.Add(new ReactionModifier(faction, value));
                }

                var reputation = new Reputation(repId, isEvil, grantedFactions, reactionMods.ToImmutable());
                result.Add(repId, reputation);
            }

            return result;
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
        public void SaveGame(SavedGameState savedGameState)
        {
            var savedReputations = _earnedReputations.Values
                .Select(earnedRep => new SavedReputation(
                    earnedRep.IsEarned,
                    earnedRep.Id,
                    earnedRep.TimeEarned
                ))
                .ToList();

            savedGameState.ReputationState = new SavedReputationState
            {
                SavedReputations = savedReputations
            };
        }

        [TempleDllLocation(0x100542f0)]
        public void LoadGame(SavedGameState savedGameState)
        {
            _earnedReputations.Clear();
            foreach (var savedReputation in savedGameState.ReputationState.SavedReputations)
            {
                _earnedReputations[savedReputation.Id] = new EarnedReputation
                {
                    Id = savedReputation.Id,
                    IsEarned = savedReputation.IsEarned,
                    TimeEarned = savedReputation.TimeEarned
                };
            }
        }

        [TempleDllLocation(0x10054d70)]
        public bool HasFactionFromReputation(GameObjectBody pc, int faction)
        {
            if (faction == 0 || !pc.IsPC())
            {
                return false;
            }

            foreach (var earnedRep in _earnedReputations.Values)
            {
                if (!earnedRep.IsEarned)
                {
                    continue;
                }

                var rep = _reputations[earnedRep.Id];
                if (rep.GrantedFactions.Contains(faction))
                {
                    return true;
                }
            }

            return false;
        }

        [TempleDllLocation(0x10054BD0)]
        public int GetReactionModFromReputation(GameObjectBody pc, GameObjectBody npc)
        {
            if (!pc.IsPC() || !npc.IsNPC())
            {
                return 0;
            }

            var adjSum = 0;
            foreach (var earnedRep in _earnedReputations.Values)
            {
                var rep = _reputations[earnedRep.Id];
                foreach (var modifier in rep.ReactionModifiers)
                {
                    if (earnedRep.IsEarned && GameSystems.Critter.HasFaction(npc, modifier.FactionId))
                    {
                        adjSum += modifier.Modifier;
                    }
                }
            }

            return adjSum;
        }

        [TempleDllLocation(0x100546e0)]
        public bool HasReputation(GameObjectBody pc, int reputationId)
        {
            if (_earnedReputations.TryGetValue(reputationId, out var earnedRep))
            {
                return earnedRep.IsEarned && earnedRep.Id == reputationId;
            }

            return false;
        }

        [TempleDllLocation(0x10054740)]
        public void AddReputation(GameObjectBody pc, int reputationId)
        {
            if (!pc.IsPC())
            {
                return;
            }

            var reputation = _reputations[reputationId];

            if (reputation.IsEvil)
            {
                Logger.Info("Earned evil reputation {0}, causing paladins to fall.", reputation.Id);
                foreach (var partyMember in GameSystems.Party.PartyMembers)
                {
                    partyMember.AddCondition(StatusEffects.FallenPaladin);
                }
            }

            if (!HasReputation(pc, reputationId))
            {
                var earnedReputation = new EarnedReputation();
                earnedReputation.Id = reputationId;
                earnedReputation.IsEarned = true;
                earnedReputation.TimeEarned = GameSystems.TimeEvent.GameTime;
                _earnedReputations[reputationId] = earnedReputation;
                GameUiBridge.PulseLogbookButton();
            }
        }

        [TempleDllLocation(0x10054820)]
        public void RemoveReputation(GameObjectBody pc, int reputationId)
        {
            if (pc.IsPC())
            {
                _earnedReputations.Remove(reputationId);
            }
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
            public int Id { get; }

            // This was previously hardcoded in AddReputation
            public bool IsEvil { get; }

            public IImmutableList<int> GrantedFactions { get; }

            public IImmutableList<ReactionModifier> ReactionModifiers { get; }

            public Reputation(int id, bool isEvil,
                IImmutableList<int> grantedFactions,
                IImmutableList<ReactionModifier> reactionModifiers)
            {
                Id = id;
                IsEvil = isEvil;
                GrantedFactions = grantedFactions;
                ReactionModifiers = reactionModifiers;
            }
        }

        private readonly struct ReactionModifier
        {
            public readonly int FactionId;
            public readonly int Modifier;

            public ReactionModifier(int factionId, int modifier)
            {
                FactionId = factionId;
                Modifier = modifier;
            }
        }
    }
}