using System;
using System.Diagnostics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Systems.D20
{
    public class D20System : IGameSystem, IResetAwareSystem, ITimeAwareSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        public const bool IsEditor = false;

        public D20ActionSystem Actions { get; private set; }

        public D20ObjectRegistry ObjectRegistry { get; private set; }

        public BonusSystem BonusSystem { get; }

        public ConditionRegistry Conditions { get; }

        public D20StatusSystem StatusSystem { get; }

        public D20Initiative Initiative { get; private set; }

        public RadialMenuSystem RadialMenu { get; private set; }

        public HotkeySystem Hotkeys { get; private set; }

        public D20BuffDebuffSystem BuffDebuff { get; private set; }

        [TempleDllLocation(0x1004c8a0)]
        public D20System()
        {
            Conditions = new ConditionRegistry();
            BonusSystem = new BonusSystem();
            StatusSystem = new D20StatusSystem();
            // TODO

            ObjectRegistry = new D20ObjectRegistry();
            Actions = new D20ActionSystem();
            Initiative = new D20Initiative();

            RadialMenu = new RadialMenuSystem();
            Hotkeys = new HotkeySystem();

            BuffDebuff = new D20BuffDebuffSystem();
        }

        [TempleDllLocation(0x1004C950)]
        public void Dispose()
        {
            ObjectRegistry?.Dispose();
            ObjectRegistry = null;

            Actions?.Dispose();
            Actions = null;

            Initiative?.Dispose();
            Initiative = null;

            Hotkeys?.Dispose();
            Hotkeys = null;

            BuffDebuff?.Dispose();
            BuffDebuff = null;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x11E61530)]
        private TimePoint _lastAdvanceTime;

        [TempleDllLocation(0x1004fc40)]
        public void AdvanceTime(TimePoint time)
        {
            if (_lastAdvanceTime == default)
            {
                _lastAdvanceTime = time;
                return;
            }

            var elapsedSeconds = (float) (time - _lastAdvanceTime).TotalSeconds;
            _lastAdvanceTime = time;

            foreach (var partyMember in GameSystems.Party.PartyMembers)
            {
                var dispatcher = partyMember.GetDispatcher();
                if (dispatcher != null)
                {
                    var dispIo = new DispIoD20Signal();
                    dispIo.TimePoint = time;
                    dispatcher.Process(DispatcherType.D20AdvanceTime, D20DispatcherKey.NONE, dispIo);
                    ((Dispatcher) dispatcher).RemoveExpiredConditions();
                }
            }

            if (!GameSystems.Combat.IsCombatActive())
            {
                AdvanceOutOfCombatTurns(elapsedSeconds);
            }

            if (!GameUiBridge.IsPartyPoolVisible())
            {
                GameUiBridge.UpdatePartyUi();
            }
        }

        private int _currentOutOfCombatInitiative;

        private const int SecondsPerTurn = 6;

        // TODO: How this function advances through initiative outside of combat is very weird...
        private void AdvanceOutOfCombatTurns(float elapsedSeconds)
        {
            if (elapsedSeconds < 0)
            {
                _partialOutOfCombatTurnTime = 0.0f;
                _currentOutOfCombatInitiative = 0;
            }

            _partialOutOfCombatTurnTime += elapsedSeconds;

            if (_partialOutOfCombatTurnTime > SecondsPerTurn)
            {
                _partialOutOfCombatTurnTime = MathF.IEEERemainder(_partialOutOfCombatTurnTime, SecondsPerTurn);
                GameUiBridge.UpdateCombatUi();
            }

            var elapsedTurnsThisRound = (int) (_partialOutOfCombatTurnTime / SecondsPerTurn * 25);
            Trace.Assert(elapsedTurnsThisRound >= 0 && elapsedTurnsThisRound <= 25);

            // The initiative is counting down from initiative 25 as time elapses
            var currentIni = 25 - elapsedTurnsThisRound;
            if (_currentOutOfCombatInitiative != currentIni)
            {
                var currentInitiative = _currentOutOfCombatInitiative;
                _currentOutOfCombatInitiative = currentIni;
                GameSystems.D20.ObjectRegistry.OnInitiativeTransition(
                    currentInitiative,
                    _currentOutOfCombatInitiative
                );
            }
        }

        public int D20QueryPython(GameObjectBody obj, string type)
        {
            Stub.TODO();
            return 0;
        }

        public int D20QueryPython(GameObjectBody obj, string type, object arg)
        {
            Stub.TODO();
            return 0;
        }

        [TempleDllLocation(0x1004ceb0)]
        public int D20QueryItem(GameObjectBody item, D20DispatcherKey queryKey)
        {
            var dispIo = new DispIoD20Query();
            dispIo.obj = item;
            DispatchForItem(item, DispatcherType.D20Query, queryKey, dispIo);
            return dispIo.return_val;
        }

        public int D20Query(GameObjectBody obj, D20DispatcherKey queryKey)
        {
            var dispatcher = obj.GetDispatcher();
            if (dispatcher == null)
            {
                return 0;
            }

            var dispIO = DispIoD20Query.Default;
            dispatcher.Process(DispatcherType.D20Query, queryKey, dispIO);
            return dispIO.return_val;
        }

        [TempleDllLocation(0x1004e6b0)]
        public void D20SendSignal(GameObjectBody obj, D20DispatcherKey key, GameObjectBody arg)
        {
            if (obj == null)
            {
                Logger.Warn("D20SendSignal called with null handle! Key was {0}", key);
                return;
            }

            var dispatcher = obj.GetDispatcher();
            if (dispatcher == null)
            {
                Logger.Info("d20SendSignal(): Object {0} lacks a Dispatcher", obj);
                return;
            }

            DispIoD20Signal dispIO = DispIoD20Signal.Default;
            dispIO.obj = arg;
            dispatcher.Process(DispatcherType.D20Signal, key, dispIO);
        }

        [TempleDllLocation(0x1004e6b0)]
        public void D20SendSignal(GameObjectBody obj, D20DispatcherKey key, int arg1 = 0, int arg2 = 0)
        {
            var dispatcher = obj.GetDispatcher();
            if (dispatcher == null)
            {
                Logger.Info("d20SendSignal(): Object {0} lacks a Dispatcher", obj);
                return;
            }

            var dispIO = DispIoD20Signal.Default;
            dispIO.data1 = arg1;
            dispIO.data2 = arg2;
            dispatcher.Process(DispatcherType.D20Signal, key, dispIO);
        }

        [TempleDllLocation(0x1004fee0)]
        public void RemoveDispatcher(GameObjectBody obj)
        {
            var dispatcher = obj.GetDispatcher() as Dispatcher;

            dispatcher?.ClearAll();
        }

        public bool CritterHasCondition(GameObjectBody obj, string conditionSpec, out int spellIdx)
        {
            return CritterHasCondition(obj, Conditions[conditionSpec], out spellIdx);
        }

        public bool CritterHasCondition(GameObjectBody obj, ConditionSpec conditionSpec, out int spellIdx)
        {
            var dispatcher = obj.GetDispatcher();
            if (dispatcher == null)
            {
                spellIdx = -1;
                return false;
            }

            var dispIO = DispIoD20Query.Default;
            dispIO.condition = conditionSpec;
            dispatcher.Process(DispatcherType.D20Query, D20DispatcherKey.QUE_Critter_Has_Condition, dispIO);

            spellIdx = (int) dispIO.data2; // TODO: This is most likely wrong. check this again.
            return dispIO.return_val != 0;
        }

        [TempleDllLocation(0x1004e620)]
        public int GetWeaponGlowType(GameObjectBody wielder, GameObjectBody item)
        {
            var dispIo = DispIoD20Query.Default;
            dispIo.obj = item;

            if (wielder != null)
            {
                var dispatcher = wielder.GetDispatcher();
                if (dispatcher == null)
                {
                    return 0;
                }

                dispatcher.Process(DispatcherType.WeaponGlowType, 0, dispIo);
                return dispIo.return_val;
            }

            DispatchForItem(item, DispatcherType.WeaponGlowType, 0, dispIo);
            return dispIo.return_val;
        }

        [TempleDllLocation(0x1004cdb0)]
        private void DispatchForItem(GameObjectBody item, DispatcherType dispType, D20DispatcherKey dispKey,
            object dispIo)
        {
            var condArray = item.GetInt32Array(obj_f.item_pad_wielder_condition_array);
            var argArrayCount = 0; // there's only one argument list for all attached conditions

            for (var i = 0; i < condArray.Count; i++)
            {
                var condNameHash = condArray[i];

                var condition = Conditions.GetByHash(condNameHash);
                if (condition != null)
                {
                    Span<int> condArgsIn = stackalloc int[condition.numArgs];
                    foreach (ref var arg in condArgsIn)
                    {
                        arg = item.GetInt32(obj_f.item_pad_wielder_argument_array, argArrayCount++);
                    }

                    condArgsIn[2] = -1; // ... why?
                    ItemDispatcher.DispatcherProcessorForItems(condition, condArgsIn, dispType, dispKey, dispIo);
                }
            }
        }

        [TempleDllLocation(0x10092A50)]
        public void turnBasedReset()
        {
            // TODO
        }

        public void ResetRadialMenus()
        {
            // TODO
        }

        [TempleDllLocation(0x100e5080)]
        public void SetCritterStrategy(GameObjectBody obj, string strategy)
        {
            // TODO
        }

        [TempleDllLocation(0x1004f0d0)]
        public int GetArmorSkillCheckPenalty(GameObjectBody armor)
        {
            var penalty = armor.GetInt32(obj_f.armor_armor_check_penalty);
            var parent = GameSystems.Item.GetParent(armor);
            if (parent == null || !parent.IsCritter())
            {
                return penalty;
            }

            var dispIo = DispIoObjBonus.Default;
            var itemName = GameSystems.MapObject.GetDisplayName(armor);
            dispIo.bonlist.AddBonus(penalty, 1, 112, itemName);

            var dispatcher = parent.GetDispatcher();
            dispatcher?.Process(DispatcherType.ArmorCheckPenalty, D20DispatcherKey.NONE, dispIo);

            return dispIo.bonlist.OverallBonus;
        }

        [TempleDllLocation(0x1004cd40)]
        public GameObjectBody D20QueryReturnObject(GameObjectBody obj, D20DispatcherKey queryKey,
            int arg1 = 0, int arg2 = 0)
        {
            Trace.Assert(queryKey == D20DispatcherKey.QUE_Critter_Is_Charmed
                         || queryKey == D20DispatcherKey.QUE_Critter_Is_Afraid
                         || queryKey == D20DispatcherKey.QUE_Critter_Is_Held);

            var dispatcher = obj.GetDispatcher();
            if (dispatcher == null)
            {
                return null;
            }

            var dispIO = new DispIoD20Query();
            dispIO.return_val = 0;
            dispIO.data1 = arg1;
            dispIO.data2 = arg2;
            dispatcher.Process(DispatcherType.D20Query, queryKey, dispIO);

            return dispIO.obj;
        }

        // How many seconds of out of combat time are accumulated that are not enough
        // to cause a new turn.
        [TempleDllLocation(0x11E61538)]
        private float _partialOutOfCombatTurnTime;

        [TempleDllLocation(0x100decb0)]
        public void EndTurnBasedCombat()
        {
            Initiative.Reset();
            _partialOutOfCombatTurnTime =
                MathF.IEEERemainder((float) GameSystems.TimeEvent.GameTime.Seconds, SecondsPerTurn);

            if (GameUiBridge.IsTutorialActive())
            {
                if (GameSystems.Script.GetGlobalFlag(4))
                {
                    GameSystems.Script.SetGlobalFlag(4, false);
                    GameSystems.Script.SetGlobalFlag(2, true);
                    GameUiBridge.ShowTutorialTopic(18);
                }
            }
        }
    }
}