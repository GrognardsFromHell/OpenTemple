using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ImGuiNET;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.D20
{
    public class D20Initiative : IDisposable, IReadOnlyList<GameObjectBody>
    {
        [TempleDllLocation(0x10BCAD90)]
        private bool _surpriseRound;

        [TempleDllLocation(0x10BCAC78)]
        private CritterGroup _initiativeOrder;

        [TempleDllLocation(0x10BCAD88)]
        private GameObjectBody _currentActor;

        [TempleDllLocation(0x10BCAD80)]
        private int _currentActorInitiative;

        [TempleDllLocation(0x100dec60)]
        public void OnExitCombat()
        {
            // The list can change while we iterate...
            var copiedList = new List<GameObjectBody>(_initiativeOrder.Members);
            foreach (var member in copiedList)
            {
                GameSystems.Script.ExecuteObjectScript(member, member, ObjScriptEvent.ExitCombat);
            }
        }

        [TempleDllLocation(0x100ded90)]
        public void Sort()
        {
            _initiativeOrder.Sort();
        }

        public void Dispose()
        {
        }

        public void Reset()
        {
            _initiativeOrder.Clear();
        }

        [TempleDllLocation(0x100dedb0)]
        public int GetInitiative(GameObjectBody obj)
        {
            return obj.GetInt32(obj_f.initiative);
        }

        [TempleDllLocation(0x100dedd0)]
        public bool Contains(GameObjectBody obj)
        {
            return _initiativeOrder.Contains(obj);
        }

        public GameObjectBody CurrentActor
        {
            [TempleDllLocation(0x100dee40)]
            get => _currentActor;

            [TempleDllLocation(0x100dee10)]
            set
            {
                _currentActor = value;
                _currentActorInitiative = GetInitiative(_currentActor);
            }
        }

        [TempleDllLocation(0x100dee50)]
        public int CurrentActorIndex => _initiativeOrder.IndexOf(_currentActor);

        private class InitiativeComparer : IComparer<GameObjectBody>
        {
            [TempleDllLocation(0x100def20)]
            public int Compare(GameObjectBody x, GameObjectBody y)
            {
                var xInit = x.GetInt32(obj_f.initiative);
                var yInit = y.GetInt32(obj_f.initiative);

                if (xInit != yInit)
                {
                    return xInit.CompareTo(yInit);
                }

                var xSubinit = x.GetInt32(obj_f.subinitiative);
                var ySubinit = y.GetInt32(obj_f.subinitiative);

                return xSubinit.CompareTo(ySubinit);
            }
        }

        [TempleDllLocation(0x100defa0)]
        private void ArbitrateConflicts()
        {
            for (int i = 0; i < _initiativeOrder.Count - 1; i++)
            {
                var combatant = _initiativeOrder[i];
                var initiative = combatant.GetInt32(obj_f.initiative);
                var subinitiative = combatant.GetInt32(obj_f.subinitiative);

                for (int j = i + 1; j < _initiativeOrder.Count; j++)
                {
                    var combatant2 = _initiativeOrder[j];
                    var initiative2 = combatant2.GetInt32(obj_f.initiative);
                    var subinitiative2 = combatant2.GetInt32(obj_f.subinitiative);
                    if (initiative != initiative2)
                        break;
                    if (subinitiative != subinitiative2)
                        break;

                    combatant2.SetInt32(obj_f.subinitiative, subinitiative2 - 1);
                }
            }
        }

        [TempleDllLocation(0x100df080)]
        private void MakeUnique(int initiative, int subinitiative)
        {
            foreach (var actor in _initiativeOrder)
            {
                var memberInitiative = actor.GetInt32(obj_f.initiative);
                var memberSubinitiative = actor.GetInt32(obj_f.subinitiative);
                if (initiative == memberInitiative && subinitiative == memberSubinitiative)
                {
                    actor.SetInt32(obj_f.subinitiative, memberSubinitiative - 1);
                }
            }
        }

        [TempleDllLocation(0x100ded20)]
        public void Save(BinaryWriter writer)
        {
            _initiativeOrder.WriteTo(writer);

            var currentActorIdx = _initiativeOrder.IndexOf(_currentActor);
            writer.Write(currentActorIdx);
        }

        [TempleDllLocation(0x100df100)]
        public void Load(BinaryReader reader)
        {
            _initiativeOrder.LoadFrom(reader);
            _initiativeOrder.Comparer = new InitiativeComparer();

            var actorIdx = reader.ReadInt32();
            if (actorIdx == -1)
            {
                _currentActor = null;
                _currentActorInitiative = 0;
            }
            else
            {
                _currentActor = _initiativeOrder[actorIdx];
                _currentActorInitiative = _currentActor.GetInt32(obj_f.initiative);
            }
        }

        [TempleDllLocation(0x100df190)]
        public void SurpriseRound()
        {
            _surpriseRound = true;

            foreach (var member in _initiativeOrder)
            {
                Stub.TODO();
            }
        }

        [TempleDllLocation(0x100df1e0)]
        public void AddToInitiative(GameObjectBody obj)
        {
            if (Contains(obj))
            {
                return;
            }

            if (obj.HasFlag(ObjectFlag.DESTROYED) || obj.HasFlag(ObjectFlag.OFF))
            {
                return;
            }

            if (GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_EnterCombat) == 0)
            {
                return;
            }

            GameSystems.AI.OnAddToInitiative(obj);

            var initiativeRoll = Dice.Roll(1, 20);
            var initiativeBonus = GetInitiativeBonus(obj, out _);
            obj.SetInt32(obj_f.initiative, initiativeRoll + initiativeBonus);

            _initiativeOrder.Add(obj);

            // Dexterity is used as the tie-breaker
            var dexterity = GameSystems.Stat.StatLevelGet(obj, Stat.dexterity);
            obj.SetInt32(obj_f.subinitiative, 100 * dexterity);
            ArbitrateConflicts();

            // Add the flatfooted condition as long as the critter has not acted
            // TODO ConditionAdd___byObjHnd_0_args(handle, &pCondition_Flatfooted);
            Stub.TODO();
            // Add the surprise round condition
            if (_surpriseRound)
            {
                // ConditionAdd___byObjHnd_0_args(handle, &Condition_Surprised);
                Stub.TODO();
            }
        }

        [TempleDllLocation(0x100df2b0)]
        public void RewindCurrentActor()
        {
            CurrentActor = _initiativeOrder[0];
        }

        [TempleDllLocation(0x100df310)]
        public void NextActor()
        {
            var currentInitiative = GetInitiative(_currentActor);
            var v7 = CurrentActorIndex;
            var nextActorIdx = v7 + 1;

            if (nextActorIdx >= _initiativeOrder.Count)
            {
                // Initiate the next round
                _surpriseRound = false;

                foreach (var member in _initiativeOrder)
                {
                    DispatchInitiative(member);
                }

                nextActorIdx = 0;
                GameSystems.D20.ObjectRegistry.OnInitiativeTransition(currentInitiative, 0);
                currentInitiative = 0;
            }

            _currentActor = _initiativeOrder[nextActorIdx];

            if (_currentActor != null)
            {
                var nextInitiative = _currentActor.GetInt32(obj_f.initiative);
                if (currentInitiative != nextInitiative)
                {
                    GameSystems.D20.ObjectRegistry.OnInitiativeTransition(currentInitiative, nextInitiative);
                }
            }
        }

        [TempleDllLocation(0x1004cef0)]
        public void DispatchInitiative(GameObjectBody obj)
        {
            var dispatcher = obj.GetDispatcher();
            dispatcher?.Process(DispatcherType.Initiative, D20DispatcherKey.NONE, null);
        }

        [TempleDllLocation(0x100df2e0)]
        public void SetInitiative(GameObjectBody obj, int initiative)
        {
            obj.SetInt32(obj_f.initiative, initiative);
            ArbitrateConflicts();
            _initiativeOrder.Sort();
        }

        /// <summary>
        /// Sets the initiative of the given object to be just before another.
        /// </summary>
        [TempleDllLocation(0x100df350)]
        public void SetInitiativeBefore(GameObjectBody obj, GameObjectBody objBefore)
        {
            var targetInitiative = GetInitiative(objBefore);
            obj.SetInt32(obj_f.initiative, targetInitiative);
            ArbitrateConflicts();
            _initiativeOrder.Sort();
            var targetSubinitiative = objBefore.GetInt32(obj_f.subinitiative) - 1;
            MakeUnique(targetInitiative, targetSubinitiative);
            obj.SetInt32(obj_f.subinitiative, targetSubinitiative);
            ArbitrateConflicts();
            _initiativeOrder.Sort();
        }

        /// <summary>
        /// Sets the initiative of the given object to be just after another.
        /// </summary>
        [TempleDllLocation(0x100df3d0)]
        [TempleDllLocation(0x100df570)]
        public void SetInitiativeTo(GameObjectBody obj, GameObjectBody targetObj)
        {
            var targetInitiative = GetInitiative(targetObj);
            obj.SetInt32(obj_f.initiative, targetInitiative);
            ArbitrateConflicts();
            _initiativeOrder.Sort();
            var targetSubinitiative = targetObj.GetInt32(obj_f.subinitiative);
            MakeUnique(targetInitiative, targetSubinitiative);
            obj.SetInt32(obj_f.subinitiative, targetSubinitiative);
            ArbitrateConflicts();
            _initiativeOrder.Sort();
        }

        [TempleDllLocation(0x100df450)]
        public void CreateForParty()
        {
            _initiativeOrder.Clear();
            _initiativeOrder.Comparer = new InitiativeComparer();

            if (GameUiBridge.IsTutorialActive() && GameSystems.Script.GetGlobalFlag(4))
            {
                GameUiBridge.ShowTutorialTopic(11);
            }

            foreach (var member in GameSystems.Party.PartyMembers)
            {
                if (GameSystems.D20.D20Query(member, D20DispatcherKey.QUE_EnterCombat) != 0)
                {
                    AddToInitiative(member);
                }
            }

            RewindCurrentActor();
        }

        [TempleDllLocation(0x100df500)]
        public void AddSurprisedCondition(GameObjectBody obj)
        {
            Stub.TODO();
            AddToInitiative(obj);
            // ConditionAdd___byObjHnd_0_args(ObjHnd, &Condition_SurpriseRound);
        }

        [TempleDllLocation(0x100df530)]
        public void RemoveFromInitiative(GameObjectBody obj)
        {
            if (obj == _currentActor && _initiativeOrder.Count > 0)
            {
                NextActor();
            }

            _initiativeOrder.Remove(obj);
        }

        public int GetInitiativeBonus(GameObjectBody obj, out BonusList bonusList)
        {
            var dispatcher = obj.GetDispatcher();
            if (dispatcher == null)
            {
                bonusList = default;
                return 0;
            }

            var bonusIo = DispIoObjBonus.Default;
            dispatcher.Process(DispatcherType.InitiativeMod, D20DispatcherKey.NONE, bonusIo);
            bonusList = bonusIo.bonlist;

            return bonusIo.bonlist.OverallBonus;
        }

        [TempleDllLocation(0x100df5a0)]
        public void Move(GameObjectBody obj, int toIndex)
        {
            var currentIndex = _initiativeOrder.IndexOf(obj);

            if (toIndex < currentIndex)
            {
                SetInitiativeTo(obj, _initiativeOrder[toIndex]);
            }
            else if (toIndex > currentIndex)
            {
                SetInitiativeBefore(obj, _initiativeOrder[toIndex]);
            }
            else if (toIndex == currentIndex)
            {
                return;
            }

            NextActor();
        }

        [TempleDllLocation(0x100dedf0)]
        public IEnumerator<GameObjectBody> GetEnumerator()
        {
            return _initiativeOrder.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _initiativeOrder).GetEnumerator();
        }

        [TempleDllLocation(0x100deda0)]
        public int Count => _initiativeOrder.Count;

        public GameObjectBody this[int index] => _initiativeOrder[index];
    }
}