using System;
using System.Collections.Generic;
using System.Diagnostics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.RenderMaterials;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.RadialMenus;
using SpicyTemple.Core.Time;
using SpicyTemple.Core.Ui;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.D20
{
    public class D20System : IGameSystem, IResetAwareSystem, ITimeAwareSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        public const bool IsEditor = false;

        public D20ActionSystem Actions { get; private set; }

        public D20CombatSystem Combat { get; private set; }

        public D20DamageSystem Damage { get; private set; }

        public D20ObjectRegistry ObjectRegistry { get; private set; }

        public BonusSystem BonusSystem { get; }

        public ConditionRegistry Conditions { get; }

        public D20StatusSystem Status { get; }

        public D20Initiative Initiative { get; private set; }

        public RadialMenuSystem RadialMenu { get; private set; }

        public HotkeySystem Hotkeys { get; private set; }

        public D20BuffDebuffSystem BuffDebuff { get; private set; }

        [TempleDllLocation(0x1004c8a0)]
        public D20System()
        {
            Conditions = new ConditionRegistry();
            Conditions.Register(GlobalCondition.Conditions);
            foreach (var conditionSpec in GlobalCondition.Conditions)
            {
                Conditions.AttachGlobally(conditionSpec);
            }
            Conditions.Register(StatusEffects.Conditions);
            Conditions.Register(ClassConditions.Conditions);
            Conditions.Register(RaceConditions.Conditions);
            Conditions.Register(MonsterConditions.Conditions);
            Conditions.Register(SpellEffects.Conditions);
            Conditions.Register(ItemEffects.Conditions);
            Conditions.Register(FeatConditions.Conditions);
            Conditions.Register(DomainConditions.Conditions);
            Logger.Info("Registered {0} conditions.", Conditions.Count);

            BonusSystem = new BonusSystem();
            Status = new D20StatusSystem();
            // TODO

            ObjectRegistry = new D20ObjectRegistry();
            Actions = new D20ActionSystem();
            Initiative = new D20Initiative();

            RadialMenu = new RadialMenuSystem();
            Hotkeys = new HotkeySystem();

            BuffDebuff = new D20BuffDebuffSystem();
            Damage = new D20DamageSystem();
            Combat = new D20CombatSystem();
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

        [TempleDllLocation(0x1004c9b0)]
        public void Reset()
        {
            _lastAdvanceTime = GameSystems.TimeEvent.GameTime;
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
            else if (time < _lastAdvanceTime)
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
                _partialOutOfCombatTurnTime = _partialOutOfCombatTurnTime % SecondsPerTurn;
                GameUiBridge.UpdateInitiativeUi();
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

        public int D20QueryPython(GameObjectBody obj, string queryKey)
        {
            var dispatcher = obj.GetDispatcher();

            if (dispatcher == null)
            {
                return 0;
            }

            var dispIo = new DispIoD20Query();
            dispIo.return_val = 0;
            dispatcher.Process(DispatcherType.PythonQuery, (D20DispatcherKey) ElfHash.Hash(queryKey), dispIo);
            return dispIo.return_val;
        }

        public int D20QueryPython(GameObjectBody obj, string queryKey, object arg)
        {
            var dispatcher = obj.GetDispatcher();

            if (dispatcher == null)
            {
                return 0;
            }

            var dispIo = new DispIoD20Query();
            dispIo.return_val = 0;
            dispIo.obj = arg;
            dispatcher.Process(DispatcherType.PythonQuery, (D20DispatcherKey) ElfHash.Hash(queryKey), dispIo);
            return dispIo.return_val;
        }

        [TempleDllLocation(0x1004ceb0)]
        public int D20QueryItem(GameObjectBody item, D20DispatcherKey queryKey)
        {
            var dispIo = new DispIoD20Query();
            dispIo.obj = item;
            DispatchForItem(item, DispatcherType.D20Query, queryKey, dispIo);
            return dispIo.return_val;
        }

        [TempleDllLocation(0x1004cc00)]
        [TempleDllLocation(0x1004cc60)]
        public bool D20Query(GameObjectBody obj, D20DispatcherKey queryKey, int data1 = 0, int data2 = 0)
        {
            var dispatcher = obj.GetDispatcher();
            if (dispatcher == null)
            {
                return false;
            }

            var dispIO = DispIoD20Query.Default;
            dispIO.data1 = data1;
            dispIO.data2 = data2;
            dispatcher.Process(DispatcherType.D20Query, queryKey, dispIO);
            return dispIO.return_val != 0;
        }

        public int D20QueryInt(GameObjectBody obj, D20DispatcherKey queryKey, int data1 = 0, int data2 = 0)
        {
            var dispatcher = obj.GetDispatcher();
            if (dispatcher == null)
            {
                return 0;
            }

            var dispIO = DispIoD20Query.Default;
            dispIO.data1 = data1;
            dispIO.data2 = data2;
            dispatcher.Process(DispatcherType.D20Query, queryKey, dispIO);
            return dispIO.return_val;
        }

        public int D20QueryWithObject(GameObjectBody obj, D20DispatcherKey queryKey, object arg, int defaultResult = 0)
        {
            var dispatcher = obj.GetDispatcher();
            if (dispatcher == null)
            {
                return defaultResult;
            }

            var dispIO = DispIoD20Query.Default;
            dispIO.obj = arg;
            dispIO.return_val = defaultResult;
            dispatcher.Process(DispatcherType.D20Query, queryKey, dispIO);
            return dispIO.return_val;
        }

        [TempleDllLocation(0x1004e6b0)]
        public void D20SendSignal(GameObjectBody obj, D20DispatcherKey key, object arg)
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
            dispIO.obj = conditionSpec;
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

            return (GameObjectBody) dispIO.obj;
        }

        public ulong D20QueryReturnData(GameObjectBody obj, D20DispatcherKey queryKey, int arg1 = 0, int arg2 = 0)
        {
            Trace.Assert(queryKey != D20DispatcherKey.QUE_Critter_Is_Charmed
                         && queryKey != D20DispatcherKey.QUE_Critter_Is_Afraid
                         && queryKey != D20DispatcherKey.QUE_Critter_Is_Held);

            var dispatcher = obj.GetDispatcher();
            if (dispatcher == null)
            {
                return 0;
            }

            var dispIO = new DispIoD20Query();
            dispIO.return_val = 0;
            dispIO.data1 = arg1;
            dispIO.data2 = arg2;
            dispatcher.Process(DispatcherType.D20Query, queryKey, dispIO);

            return dispIO.resultData;
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
                (float) GameSystems.TimeEvent.GameTime.Seconds % SecondsPerTurn;

            if (GameUiBridge.IsTutorialActive())
            {
                if (GameSystems.Script.GetGlobalFlag(4))
                {
                    GameSystems.Script.SetGlobalFlag(4, false);
                    GameSystems.Script.SetGlobalFlag(2, true);
                    GameUiBridge.ShowTutorialTopic(TutorialTopic.Looting);
                }
            }
        }

        public void D20SignalPython(GameObjectBody handle, string queryKey, int arg1 = 0, int arg2 = 0)
        {
            D20SignalPython(handle, (D20DispatcherKey) ElfHash.Hash(queryKey), arg1, arg2);
        }

        public void D20SignalPython(GameObjectBody handle, D20DispatcherKey queryKey, int arg1, int arg2)
        {
            if (handle == null)
            {
                Logger.Warn("D20SignalPython called with null handle! Key was {0}, arg1 {1}, arg2 {2}", queryKey, arg1,
                    arg2);
                return;
            }

            var dispatcher = handle.GetDispatcher();
            if (dispatcher == null)
            {
                return;
            }

            var dispIo = DispIoD20Signal.Default;
            dispIo.return_val = 0;
            dispIo.data1 = arg1;
            dispIo.data2 = arg2;
            dispatcher.Process(DispatcherType.PythonSignal, queryKey, dispIo);
        }

        [TempleDllLocation(0x1004dfc0)]
        public GameObjectBody GetAttackWeapon(GameObjectBody obj, int attackCode, D20CAF flags)
        {
            if (flags.HasFlag(D20CAF.TOUCH_ATTACK) && !flags.HasFlag(D20CAF.THROWN_GRENADE))
            {
                return null;
            }

            if (flags.HasFlag(D20CAF.SECONDARY_WEAPON))
                return GameSystems.Item.ItemWornAt(obj, EquipSlot.WeaponSecondary);

            if (UsingSecondaryWeapon(obj, attackCode))
                return GameSystems.Item.ItemWornAt(obj, EquipSlot.WeaponSecondary);

            if (attackCode > AttackPacket.ATTACK_CODE_NATURAL_ATTACK)
                return null;

            return GameSystems.Item.ItemWornAt(obj, EquipSlot.WeaponPrimary);
        }


        public bool UsingSecondaryWeapon(D20Action action)
        {
            return UsingSecondaryWeapon(action.d20APerformer, action.data1);
        }

        public bool UsingSecondaryWeapon(GameObjectBody obj, int attackCode)
        {
            if (attackCode == AttackPacket.ATTACK_CODE_OFFHAND + 2 ||
                attackCode == AttackPacket.ATTACK_CODE_OFFHAND + 4 ||
                attackCode == AttackPacket.ATTACK_CODE_OFFHAND + 6)
            {
                if (attackCode == AttackPacket.ATTACK_CODE_OFFHAND + 2)
                {
                    return true;
                }

                if (attackCode == AttackPacket.ATTACK_CODE_OFFHAND + 4)
                {
                    if (GameSystems.Feat.HasFeatCount(obj, FeatId.IMPROVED_TWO_WEAPON_FIGHTING) != 0
                        || GameSystems.Feat.HasFeatCountByClass(obj, FeatId.IMPROVED_TWO_WEAPON_FIGHTING_RANGER) != 0)
                        return true;
                }
                else if (attackCode == AttackPacket.ATTACK_CODE_OFFHAND + 6)
                {
                    if (GameSystems.Feat.HasFeatCount(obj, FeatId.GREATER_TWO_WEAPON_FIGHTING) != 0
                        || GameSystems.Feat.HasFeatCountByClass(obj, FeatId.GREATER_TWO_WEAPON_FIGHTING_RANGER) != 0)
                        return true;
                }
            }

            return false;
        }

        public void ExtractAttackNumber(GameObjectBody obj, int attackCode, out int attackNumber, out bool dualWielding)
        {
            if (attackCode >= AttackPacket.ATTACK_CODE_NATURAL_ATTACK)
            {
                attackNumber = attackCode - AttackPacket.ATTACK_CODE_NATURAL_ATTACK;
                dualWielding = false;
            }
            else if (attackCode >= AttackPacket.ATTACK_CODE_OFFHAND)
            {
                dualWielding = true;
                int attackIdx = attackCode - (AttackPacket.ATTACK_CODE_OFFHAND + 1);
                int numOffhandExtraAttacks = GameSystems.Critter.NumOffhandExtraAttacks(obj);
                if (GameSystems.D20.UsingSecondaryWeapon(obj, attackCode))
                {
                    if (attackIdx % 2 != 0 && (attackIdx - 1) / 2 < numOffhandExtraAttacks)
                        attackNumber = 1 + (attackIdx - 1) / 2;
                    else
                        attackNumber = 0; // TODO This was added to guard the Trace.Assert below against uninitialized
                }
                else
                {
                    if ((attackIdx % 2) == 0 && (attackIdx / 2 < numOffhandExtraAttacks))
                        attackNumber = 1 + attackIdx / 2;
                    else
                        attackNumber = 1 + numOffhandExtraAttacks + (attackIdx - 2 * numOffhandExtraAttacks);
                }

                Trace.Assert(attackNumber > 0);
            }
            else // regular case (just primary hand)
            {
                attackNumber = attackCode - AttackPacket.ATTACK_CODE_PRIMARY;
                if (attackNumber <= 0) // seems to be the case for charge attack
                {
                    attackNumber = 1;
                }

                dualWielding = false;
            }
        }

    }
}