using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.GameObjects;

namespace SpicyTemple.Core.Systems
{
    public class PartySystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem
    {
        [TempleDllLocation(0x11E721E0)]
        private CritterGroup _party;

        [TempleDllLocation(0x11E71D00)]
        private CritterGroup _selected;

        [TempleDllLocation(0x11E71F40)]
        private CritterGroup _pcs;

        [TempleDllLocation(0x11E71BE0)]
        private CritterGroup _npcs;

        [TempleDllLocation(0x11E71E20)]
        private CritterGroup _aiFollowers;

        [TempleDllLocation(0x1080ABA4)]
        public Alignment PartyAlignment { get; set; }

        [TempleDllLocation(0x1080ABA0)]
        [TempleDllLocation(0x1002b8d0)]
        [TempleDllLocation(0x1002b8c0)]
        public bool IsPartyBanterVoiceEnabled { get; set; }

        // TODO: This field actually seems unused
        [TempleDllLocation(0x1080AB9C)]
        [TempleDllLocation(0x1002b8e0)]
        [TempleDllLocation(0x1002b8f0)]
        public bool IsPartyBanterTextEnabled { get; set; }

        [TempleDllLocation(0x1002b900)]
        [TempleDllLocation(0x1080AB70)]
        [TempleDllLocation(0x1002b910)]
        public bool ShowHitPoints { get; set; } = true;

        [TempleDllLocation(0x11E72380)]
        private SavedPartyState _savedState;

        [TempleDllLocation(0x1002b9d0)]
        public PartySystem()
        {
            _party = new CritterGroup();
            _selected = new CritterGroup();
            _pcs = new CritterGroup();
            _npcs = new CritterGroup();
            _aiFollowers = new CritterGroup();

            _selected.Comparer = new SelectedMemberComparer(_party);
            _party.Comparer = new PartyMemberComparer(_aiFollowers, _party);

            Reset();
        }

        /// <summary>
        /// Maintains the same order as the party group.
        /// </summary>
        [TempleDllLocation(0x1002abb0)]
        private class SelectedMemberComparer : IComparer<GameObjectBody>
        {
            private CritterGroup _party;

            public SelectedMemberComparer(CritterGroup party)
            {
                _party = party;
            }

            public int Compare(GameObjectBody x, GameObjectBody y)
            {
                var xIdx = _party.IndexOf(x);
                var yIdx = _party.IndexOf(y);
                return xIdx.CompareTo(yIdx);
            }
        }

        /// <summary>
        /// Sorts AI followers to the end of the list.
        /// </summary>
        [TempleDllLocation(0x1002b920)]
        private class PartyMemberComparer : IComparer<GameObjectBody>
        {
            private readonly CritterGroup _aiFollowers;
            private readonly CritterGroup _party;

            public PartyMemberComparer(CritterGroup aiFollowers, CritterGroup party)
            {
                _aiFollowers = aiFollowers;
                _party = party;
            }

            public int Compare(GameObjectBody x, GameObjectBody y)
            {
                if (x == y)
                {
                    return 0;
                }

                var xIsFollower = _aiFollowers.Contains(x);
                var yIsFollower = _aiFollowers.Contains(y);
                if (!xIsFollower && yIsFollower)
                {
                    return -1;
                }

                if (xIsFollower && !yIsFollower)
                {
                    return 1;
                }

                var xIdx = _party.IndexOf(x);
                var yIdx = _party.IndexOf(y);
                return xIdx.CompareTo(yIdx);
            }
        }

        public void Dispose()
        {
        }

        [TempleDllLocation(0x1002ac00)]
        public void Reset()
        {
            _party.Clear();
            _selected.Clear();
            _pcs.Clear();
            _npcs.Clear();
            _aiFollowers.Clear();

            // Clear party money
            _partyMoney[0] = 0;
            _partyMoney[1] = 0;
            _partyMoney[2] = 0;
            _partyMoney[3] = 0;

            PartyAlignment = Alignment.NEUTRAL;

            IsPartyBanterVoiceEnabled = true;
            IsPartyBanterTextEnabled = false;
        }

        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        public bool LoadGame()
        {
            throw new NotImplementedException();
        }

        private const int PARTY_SIZE_MAX = 8;

        [TempleDllLocation(0x1002BBE0)]
        public void AddToPCGroup(GameObjectBody obj)
        {
            var npcFollowers = _npcs.Count;
            var pcs = _pcs.Count;

            if (pcs < Globals.Config.MaxPCs
                || Globals.Config.MaxPCsFlexible && (npcFollowers + pcs < PARTY_SIZE_MAX))
            {
                _pcs.Add(obj);
                _party.Add(obj);
                AddToSelection(obj);
            }
        }

        [TempleDllLocation(0x1002BC40)]
        public void AddToNPCGroup(GameObjectBody obj)
        {
            var npcFollowers = _npcs.Count;
            if (npcFollowers >= 5)
                return;

            var pcs = _pcs.Count;

            if (npcFollowers < PARTY_SIZE_MAX - Globals.Config.MaxPCs
                || Globals.Config.MaxPCsFlexible && (npcFollowers + pcs < PARTY_SIZE_MAX))
            {
                _npcs.Add(obj);
                _party.Add(obj);
                AddToSelection(obj);
            }
        }

        [TempleDllLocation(0x1002B560)]
        public bool AddToSelection(GameObjectBody obj)
        {
            if (!_party.Contains(obj))
                return false;

            _selected.Add(obj);
            return true;
        }

        [TempleDllLocation(0x1002b5a0)]
        public void RemoveFromSelection(GameObjectBody obj)
        {
            _selected.Remove(obj);
        }

        [TempleDllLocation(0x1002b550)]
        public void ClearSelection()
        {
            _selected.Clear();
        }

        [TempleDllLocation(0x1002b5f0)]
        public bool IsSelected(GameObjectBody obj) => _selected.Contains(obj);

        public IEnumerable<GameObjectBody> PartyMembers => _party;

        public IEnumerable<GameObjectBody> PlayerCharacters => _pcs;

        [TempleDllLocation(0x1002b360)]
        [TempleDllLocation(0x1002b190)]
        public IEnumerable<GameObjectBody> NPCFollowers => _npcs;

        public IReadOnlyList<GameObjectBody> Selected => _selected;

        [TempleDllLocation(0x1002b1b0)]
        public bool IsInParty(GameObjectBody obj) => _party.Contains(obj);

        [TempleDllLocation(0x1002b2b0)]
        public int PartySize => _party.Count;

        public GameObjectBody GetLeader()
        {
            if (_pcs.Count == 0)
            {
                return null;
            }

            return GetPCGroupMemberN(0);
        }

        [TempleDllLocation(0x1002BE60)]
        public GameObjectBody GetConsciousLeader()
        {
            foreach (var selected in _selected)
            {
                if (selected != null)
                {
                    return selected;
                }
            }

            /* added fix in case the leader is not currently selected and is in combat
                This fixes issue with Fear'ed characters fucking up things in TB combat, because while running
                away they weren't selected.
                Symptoms included causing an incorrect radial menu to appear for the fear'd character.
            */
            if (GameSystems.Combat.IsCombatActive())
            {
                var curActor = GameSystems.D20.Initiative.CurrentActor;
                if (IsInParty(curActor) && !GameSystems.Critter.IsDeadOrUnconscious(curActor))
                {
                    return curActor;
                }
            }

            foreach (var member in _party)
            {
                if (member != null && !GameSystems.Critter.IsDeadOrUnconscious(member))
                {
                    return member;
                }
            }

            return null;
        }

        [TempleDllLocation(0x1002b150)]
        public GameObjectBody GetPartyGroupMemberN(int index) => _party[index];

        [TempleDllLocation(0x1002B170)]
        public GameObjectBody GetPCGroupMemberN(int index) => _pcs[index];

        // One entry per coin type
        private int[] _partyMoney = new int[4];

        [TempleDllLocation(0x1002B750)]
        public int GetPartyMoney()
        {
            return GetWorthInCopper(MoneyType.Copper) * GetPartyMoneyOfType(MoneyType.Copper)
                   + GetWorthInCopper(MoneyType.Silver) * GetPartyMoneyOfType(MoneyType.Silver)
                   + GetWorthInCopper(MoneyType.Gold) * GetPartyMoneyOfType(MoneyType.Gold)
                   + GetWorthInCopper(MoneyType.Platinum) * GetPartyMoneyOfType(MoneyType.Platinum);
        }

        [TempleDllLocation(0x1002b7d0)]
        public void AddPartyMoney(int pp, int gp, int slvp, int cp)
        {
            _partyMoney[0] += cp;
            _partyMoney[1] += slvp;
            _partyMoney[2] += gp;
            _partyMoney[3] += pp;
        }

        [TempleDllLocation(0x1002c020)]
        public void RemovePartyMoney(int pp, int gp, int slvp, int cp)
        {
            var totalCpAmount = GetCoinWorth(pp, gp, slvp, cp);
            _partyMoney[0] -= totalCpAmount;

            // Try to equalize a negative coin balance by filling it up with coins from the
            // higher types
            for (var type = MoneyType.Copper; type < MoneyType.Platinum; type++)
            {
                var needed = _partyMoney[(int) type];
                if (needed < 0)
                {
                    var worthInCopper = GetWorthInCopper(type);
                    var deficitInCopper = worthInCopper * needed;
                    var nextUpWorthInCopper = GetWorthInCopper(type + 1);

                    var nextTierCoinsConsumed = -(deficitInCopper / nextUpWorthInCopper);
                    if (Math.Abs(deficitInCopper) % nextUpWorthInCopper != 0)
                    {
                        ++nextTierCoinsConsumed;
                    }

                    _partyMoney[(int) type + 1] -= nextTierCoinsConsumed;
                    _partyMoney[(int) type] += nextTierCoinsConsumed * nextUpWorthInCopper / worthInCopper;
                }
            }
        }

        private ref int GetPartyMoneyOfType(MoneyType type) => ref _partyMoney[(int) type];

        [TempleDllLocation(0x1002B780)]
        public void GetPartyMoneyCoins(out int platinCoins, out int goldCoins, out int silverCoins, out int copperCoins)
        {
            platinCoins = GetPartyMoneyOfType(MoneyType.Platinum);
            goldCoins = GetPartyMoneyOfType(MoneyType.Gold);
            silverCoins = GetPartyMoneyOfType(MoneyType.Silver);
            copperCoins = GetPartyMoneyOfType(MoneyType.Copper);
        }

        /// <summary>
        /// Calculates the overall worth of a bunch of coins.
        /// </summary>
        [TempleDllLocation(0x1002B880)]
        public int GetCoinWorth(int platinCoins = 0, int goldCoins = 0, int silverCoins = 0, int copperCoins = 0)
        {
            return GetWorthInCopper(MoneyType.Platinum) * platinCoins
                   + GetWorthInCopper(MoneyType.Gold) * goldCoins
                   + GetWorthInCopper(MoneyType.Silver) * silverCoins
                   + copperCoins;
        }

        /// <summary>
        /// Returns the worth of a piece of currency in copper coins based on its type.
        /// </summary>
        [TempleDllLocation(0x10064D10)]
        private static int GetWorthInCopper(MoneyType moneyType)
        {
            switch (moneyType)
            {
                case MoneyType.Copper:
                    return 1;
                case MoneyType.Silver:
                    return 10;
                case MoneyType.Gold:
                    return 100;
                case MoneyType.Platinum:
                    return 1000;
                default:
                    throw new ArgumentOutOfRangeException(nameof(moneyType), moneyType, null);
            }
        }

        [TempleDllLocation(0x1002bd00)]
        public void RemoveFromAllGroups(GameObjectBody obj)
        {
            // Obj_Remove_From_Group_Array(&groupCurrentlySelected, a1);
            // Obj_Remove_From_Group_Array(&groupAiFollowers, a1);
            // Obj_Remove_From_Group_Array(&groupPCs, a1);
            // Obj_Remove_From_Group_Array(&groupNpcFollowers, a1);
            // Obj_Remove_From_Group_Array(&groupList, a1);
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1002bca0)]
        public bool AddAIFollower(GameObjectBody obj)
        {
            if (_aiFollowers.Count >= 10)
            {
                return false;
            }

            _aiFollowers.Add(obj);
            _party.Add(obj);
            GameSystems.Party.AddToSelection(obj);
            return true;
        }

        [TempleDllLocation(0x1002b220)]
        public bool IsAiFollower(GameObjectBody obj)
        {
            return _aiFollowers.Contains(obj);
        }

        [TempleDllLocation(0x1002b390)]
        public bool IsPlayerControlled(GameObjectBody obj)
        {
            if (!IsInParty(obj))
            {
                return false;
            }

            var partyCount = GetLivingPartyMemberCount();

            if (obj.IsPC())
            {
                if (partyCount <= 1)
                {
                    // ha! vanilla only checked this
                    if (IsInParty(obj)) // vanilla didn't check this
                        return true;
                }
            }

            if (IsAiFollower(obj))
                return false;

            // check if charmed by someone
            GameObjectBody leader;
            if (GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Critter_Is_Charmed) )
            {
                leader = GameSystems.D20.D20QueryReturnObject(obj, D20DispatcherKey.QUE_Critter_Is_Charmed);
                if (leader != null && !IsInParty(leader))
                {
                    return false;
                }
            }

            // checked if afraid of someone & can see them
            if (GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Critter_Is_Afraid) )
            {
                GameObjectBody fearer;
                fearer = GameSystems.D20.D20QueryReturnObject(obj, D20DispatcherKey.QUE_Critter_Is_Afraid);
                if (fearer != null && obj.DistanceToObjInFeet(fearer) < 40.0f
                                   && GameSystems.Combat.HasLineOfAttack(fearer, obj))
                {
                    return false;
                }
            }

            if (GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Critter_Is_AIControlled)                 || GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Critter_Is_Confused) )
            {
                return false;
            }

            return true;
        }

        [TempleDllLocation(0x1002b2c0)]
        public int GetLivingPartyMemberCount()
        {
            return _party.Count(obj => GameSystems.Critter.IsDeadNullDestroyed(obj));
        }

        /// <summary>
        /// Save the current party members for restoring them after a map change.
        /// </summary>
        [TempleDllLocation(0x1002BA40)]
        public void SaveCurrent()
        {
            var savedIds = _party.Select(p => p.id).ToArray();

            var pcsIndices = _pcs.Select(p => _party.IndexOf(p)).ToArray();
            var npcsIndices = _npcs.Select(p => _party.IndexOf(p)).ToArray();
            var aiFollowerIndices = _aiFollowers.Select(p => _party.IndexOf(p)).ToArray();
            var selectedIndices = _selected.Select(p => _party.IndexOf(p)).ToArray();

            _savedState = new SavedPartyState(savedIds,
                pcsIndices, npcsIndices, aiFollowerIndices, selectedIndices);

            _party.Clear();
            _pcs.Clear();
            _npcs.Clear();
            _aiFollowers.Clear();
            _selected.Clear();
        }

        /// <summary>
        /// Restore the party members from a set of ids previously saved with <see cref="SaveCurrent"/>.
        /// </summary>
        [TempleDllLocation(0x1002AEA0)]
        public void RestoreCurrent()
        {
            if (_savedState == null)
            {
                throw new InvalidOperationException();
            }

            foreach (var id in _savedState.Ids)
            {
                var obj = GameSystems.Object.GetObject(id);
                if (obj != null)
                {
                    _party.Add(obj);

                    if (!GameSystems.D20.ObjectRegistry.Contains(obj))
                    {
                        GameSystems.D20.Status.D20StatusInit(obj);
                    }
                }
            }

            foreach (var idx in _savedState.PCIndices)
            {
                var obj = GameSystems.Object.GetObject(_savedState.Ids[idx]);
                if (obj != null)
                {
                    _pcs.Add(obj);
                }
            }

            foreach (var idx in _savedState.NPCIndices)
            {
                var obj = GameSystems.Object.GetObject(_savedState.Ids[idx]);
                if (obj != null)
                {
                    _npcs.Add(obj);
                }
            }

            foreach (var idx in _savedState.AiFollowerIndices)
            {
                var obj = GameSystems.Object.GetObject(_savedState.Ids[idx]);
                if (obj != null)
                {
                    _aiFollowers.Add(obj);
                }
            }

            foreach (var idx in _savedState.SelectedIndices)
            {
                var obj = GameSystems.Object.GetObject(_savedState.Ids[idx]);
                if (obj != null)
                {
                    _selected.Add(obj);
                }
            }

            GameSystems.RollHistory.Clear();
            GameSystems.D20Rolls.Reset();

            foreach (var playerObj in _pcs)
            {
                GameSystems.Secretdoor.QueueSearchTimer(playerObj);
            }

            _savedState = null;
        }

        public void ClearPartyMoney()
        {
            _partyMoney[0] = 0;
            _partyMoney[1] = 0;
            _partyMoney[2] = 0;
            _partyMoney[3] = 0;
        }

        [TempleDllLocation(0x1002B820)]
        public void GiveMoneyFromItem(GameObjectBody item)
        {
            var moneyAmt = item.GetInt32(obj_f.money_quantity);
            var moneyType = item.GetInt32(obj_f.money_type);
            if (moneyType < 4 && moneyType >= 0)
            {
                _partyMoney[moneyType] += moneyAmt;
            }
        }

        [TempleDllLocation(0x1002b4d0)]
        public void Swap(int index1, int index2)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1002bd50)]
        public GameObjectBody GetMemberWithHighestSkill(SkillId skill)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1002b240)]
        public int DistanceToParty(GameObjectBody obj)
        {
            var objLocation = obj.GetLocation();

            var result = int.MaxValue;
            foreach (var partyMember in PartyMembers)
            {
                var distance = partyMember.GetLocation().EstimateDistance(objLocation);
                if (distance < result)
                {
                    result = distance;
                }
            }

            return result;
        }

        private class SavedPartyState
        {
            public ObjectId[] Ids { get; }

            public int[] PCIndices { get; }

            public int[] NPCIndices { get; }

            public int[] AiFollowerIndices { get; }

            public int[] SelectedIndices { get; }

            public SavedPartyState(ObjectId[] ids,
                int[] pcIndices,
                int[] npcIndices,
                int[] aiFollowerIndices,
                int[] selectedIndices)
            {
                Ids = ids;
                PCIndices = pcIndices;
                NPCIndices = npcIndices;
                AiFollowerIndices = aiFollowerIndices;
                SelectedIndices = selectedIndices;
            }
        }

        public int IndexOf(GameObjectBody critter) => _party.IndexOf(critter);

        [TempleDllLocation(0x1002b610)]
        public void SaveSelection(int groupIndex)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1002b690)]
        public void LoadSelection(int groupIndex)
        {
            throw new NotImplementedException();
        }
    }
}