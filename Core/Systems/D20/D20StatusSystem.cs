using System;
using System.Collections.Generic;
using System.Diagnostics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO.SaveGames;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20.Classes;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.D20
{
    public class D20StatusSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        // TODO This does not belong in this location
        [TempleDllLocation(0x100e1f10)]
        public Dispatcher InitDispatcher(GameObjectBody obj)
        {
            var dispatcherNew = new Dispatcher(obj);
            foreach (var globalAttachment in GameSystems.D20.Conditions.GlobalAttachments)
            {
                dispatcherNew.Attach(globalAttachment);
            }

            return dispatcherNew;
        }

        [TempleDllLocation(0x1004fdb0)]
        public void D20StatusInit(GameObjectBody obj)
        {
            if (obj.GetDispatcher() != null)
            {
                return;
            }

            var dispatcher = InitDispatcher(obj);
            obj.SetDispatcher(dispatcher);

            dispatcher.ClearPermanentMods();

            if (obj.IsCritter())
            {
                var psiptsCondStruct = GameSystems.D20.Conditions["Psi Points"];
                if (psiptsCondStruct != null)
                {
                    // args will be set from D20StatusInitFromInternalFields if this condition has already been previously applied
                    dispatcher._ConditionAddToAttribs_NumArgs0(psiptsCondStruct);
                }

                InitClass(dispatcher, obj);

                initRace(dispatcher, obj);

                initFeats(dispatcher, obj);
            }
            else
            {
                Logger.Info("Attempted D20Status Init for non-critter {0}", obj);
            }

            initItemConditions(obj);

            D20StatusInitFromInternalFields(obj, dispatcher);

            GameSystems.D20.ObjectRegistry.Add(obj);

            if (D20System.IsEditor)
            {
                return;
            }

            if (obj.IsCritter())
            {
                if (!GameSystems.Critter.IsDeadNullDestroyed(obj))
                {
                    int hpCur = GameSystems.Stat.StatLevelGet(obj, Stat.hp_current);
                    uint subdualDam = obj.GetUInt32(obj_f.critter_subdual_damage);

                    if (hpCur != D20StatSystem.UninitializedHitPoints)
                    {
                        if (hpCur < 0)
                        {
                            if (GameSystems.Feat.HasFeat(obj, FeatId.DIEHARD))
                            {
                                dispatcher._ConditionAdd_NumArgs0(StatusEffects.Disabled);
                            }
                            else
                            {
                                dispatcher._ConditionAdd_NumArgs0(StatusEffects.Unconscious);
                            }
                        }

                        else
                        {
                            if (hpCur == 0)
                            {
                                dispatcher._ConditionAdd_NumArgs0(StatusEffects.Disabled);
                            }
                            else if ((int) subdualDam > hpCur)
                            {
                                dispatcher._ConditionAdd_NumArgs0(StatusEffects.Unconscious);
                            }
                        }
                    }
                }
            }
        }

        [TempleDllLocation(0x100fee60)]
        private void InitClass(Dispatcher dispatcher, GameObjectBody objHnd)
        {
            if (!objHnd.IsCritter())
            {
                return;
            }

            foreach (var kvp in D20ClassSystem.Classes)
            {
                var classCode = kvp.Key;
                var classSpec = kvp.Value;

                if (objHnd.GetStat(classCode) <= 0)
                {
                    continue;
                }

                var condStructClass = GameSystems.D20.Conditions[classSpec.conditionName];
                if (condStructClass == null)
                {
                    Logger.Warn("Failed to find condition '{0}' for class '{1}'", classSpec.conditionName, classCode);
                    continue;
                }

                dispatcher._ConditionAddToAttribs_NumArgs0(condStructClass);
            }

            if (objHnd.GetStat(Stat.level_cleric) >= 1)
            {
                Stub.TODO();
                // TODO _D20StatusInitDomains(objHnd);
            }

            if (GameSystems.Feat.HasFeat(objHnd, FeatId.REBUKE_UNDEAD))
            {
                dispatcher._ConditionAddToAttribs_NumArgs2(DomainConditions.TurnUndead, 1, 0);
            }
            else if (GameSystems.Feat.HasFeat(objHnd, FeatId.TURN_UNDEAD))
            {
                dispatcher._ConditionAddToAttribs_NumArgs2(DomainConditions.TurnUndead, 0, 0);
            }

            if (objHnd.GetStat(Stat.level_bard) >= 1)
            {
                dispatcher._ConditionAddToAttribs_NumArgs0(ClassConditions.BardicMusic);
            }

            if ((objHnd.GetInt32(obj_f.critter_school_specialization) & 0xFF) != 0)
            {
                dispatcher._ConditionAddToAttribs_NumArgs0(ClassConditions.SchoolSpecialization);
            }
        }

        [TempleDllLocation(0x100fd790)]
        private void initRace(Dispatcher dispatcher, GameObjectBody critter)
        {
            if (!critter.IsCritter())
                return;

            if (GameSystems.Critter.IsUndead(critter))
            {
                dispatcher._ConditionAddToAttribs_NumArgs0(RaceConditions.MonsterUndead);
            }

            var race = GameSystems.Critter.GetRace(critter, false);
            var raceCond = D20RaceSystem.GetConditionName(race);
            dispatcher._ConditionAddToAttribs_NumArgs0(GameSystems.D20.Conditions[raceCond]);

            var racialSpells = D20RaceSystem.GetSpellLikeAbilities(race);
            var spellsMemorized = critter.GetSpellArray(obj_f.critter_spells_memorized_idx);
            foreach (var kvp in racialSpells)
            {
                var spell = kvp.Key;
                var count = 0;
                var specifiedCount = kvp.Value;

                for (var i = 0; i < spellsMemorized.Count; i++)
                {
                    var memSpell = spellsMemorized[i];
                    if (memSpell.classCode == spell.classCode && memSpell.spellLevel == spell.spellLevel &&
                        memSpell.spellEnum == spell.spellEnum && memSpell.padSpellStore == (int) race)
                    {
                        count++;
                        if (count >= specifiedCount)
                        {
                            break;
                        }
                    }
                }

                var spData = spell;
                spData.padSpellStore = (ushort) race;
                var spellStoreData = spData.spellStoreState;
                for (var i = 0; i < specifiedCount - count; i++)
                {
                    GameSystems.Spell.SpellMemorizedAdd(critter, spell.spellEnum, spell.classCode, spell.spellLevel,
                        spellStoreData);
                }
            }

            if (GameSystems.Critter.IsFireSubtype(critter))
            {
                dispatcher._ConditionAddToAttribs_NumArgs0(RaceConditions.MonsterSubtypeFire);
            }

            if (GameSystems.Critter.IsOoze(critter))
            {
                dispatcher._ConditionAddToAttribs_NumArgs0(RaceConditions.MonsterOoze);
            }
        }

        [TempleDllLocation(0x1004ca00)]
        public void initItemConditions(GameObjectBody obj)
        {
            var dispatcher = (Dispatcher) obj.GetDispatcher();
            if (dispatcher == null)
            {
                return;
            }

            if (obj.IsCritter())
            {
                dispatcher.ClearItemConditions();
                if (!GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Polymorphed))
                {
                    foreach (var item in obj.EnumerateChildren())
                    {
                        var itemInvLocation = item.GetInt32(obj_f.item_inv_location);
                        if (AreItemConditionsActive(item, itemInvLocation))
                        {
                            // sets args[2] equal to the itemInvLocation
                            InitFromItemConditionFields(dispatcher, item, itemInvLocation);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks whether an item's conditions apply depending on where it is located on a critter.
        /// </summary>
        [TempleDllLocation(0x100FEFA0)]
        private bool AreItemConditionsActive(GameObjectBody item, int itemInvIdx)
        {
            if (item.type == ObjectType.weapon
                || item.type == ObjectType.armor && item.GetArmorFlags().IsShield()
                || item.GetItemWearFlags() != default)
            {
                return ItemSystem.IsInvIdxWorn(itemInvIdx);
            }
            else
            {
                return true;
            }
        }

        [TempleDllLocation(0x100ff500)]
        private void InitFromItemConditionFields(Dispatcher dispatcher, GameObjectBody item, int invIdx)
        {
            var itemConds = item.GetInt32Array(obj_f.item_pad_wielder_condition_array);
            var itemArgs = item.GetInt32Array(obj_f.item_pad_wielder_argument_array);

            var argIdx = 0;
            for (var i = 0; i < itemConds.Count; i++)
            {
                var condId = itemConds[i];
                var condStruct = GameSystems.D20.Conditions.GetByHash(condId);
                if (condStruct == null)
                {
                    Logger.Warn($"Item condition {condId} not found!");
                    continue;
                }

                Span<int> args = stackalloc int[condStruct.numArgs];
                for (var j = 0; j < condStruct.numArgs; j++)
                {
                    args[j] = itemArgs[argIdx++];
                }

                if (args.Length >= 3)
                {
                    args[2] = invIdx;
                }

                dispatcher.AddItemCondition(condStruct, args);
            }
        }

        [TempleDllLocation(0x1004ff30)]
        [TemplePlusLocation("d20.cpp:171")]
        public void D20StatusRefresh(GameObjectBody critter)
        {
            Logger.Info("Refreshing D20 Status for {0}", critter);
            if (critter.GetDispatcher() is Dispatcher dispatcher)
            {
                // TODO dispatcher.PackDispatcherIntoObjFields(critter);
                Stub.TODO();
                dispatcher.ClearPermanentMods();
                InitClass(dispatcher, critter);
                initRace(dispatcher, critter);
                initFeats(dispatcher, critter);
                D20StatusInitFromInternalFields(critter, dispatcher);
            }
        }

        [TempleDllLocation(0x100fd2d0)]
        [TemplePlusLocation("d20.cpp:196")]
        private void initFeats(Dispatcher dispatcher, GameObjectBody critter)
        {
            foreach (var featId in GameSystems.Feat.FeatListElective(critter))
            {
                if (_GetCondStructFromFeat(featId, out var cond, out var arg))
                {
                    dispatcher._ConditionAddToAttribs_NumArgs2(cond, (int) featId, arg);
                }
            }

            dispatcher._ConditionAddToAttribs_NumArgs0(FeatConditions.AOO);
            dispatcher._ConditionAddToAttribs_NumArgs0(FeatConditions.CastDefensively);
            dispatcher._ConditionAddToAttribs_NumArgs0(FeatConditions.DealSubdualDamage);
            dispatcher._ConditionAddToAttribs_NumArgs0(FeatConditions.DealNormalDamage);
            dispatcher._ConditionAddToAttribs_NumArgs0(FeatConditions.FightingDefensively);
            dispatcher._ConditionAddToAttribs_NumArgs0(TemplePlusFeatConditions.DisableAoo);
            dispatcher._ConditionAddToAttribs_NumArgs0(TemplePlusFeatConditions.Disarm);
            dispatcher._ConditionAddToAttribs_NumArgs0(TemplePlusFeatConditions.AidAnother);
            dispatcher._ConditionAddToAttribs_NumArgs0(TemplePlusFeatConditions.PreferOneHandedWield);
            // "Trip Attack Of Opportunity" -> decided to incorporate this in Improved Trip to prevent AoOs on AoOs
        }

        [TempleDllLocation(0x100f7be0)]
        [TemplePlusLocation("condition.cpp:416")]
        private bool _GetCondStructFromFeat(FeatId featEnum, out ConditionSpec condStructOut, out int argout)
        {
            switch (featEnum)
            {
                case FeatId.GREATER_TWO_WEAPON_FIGHTING:
                    condStructOut = TemplePlusFeatConditions.GreaterTwoWeaponFighting;
                    argout = 0;
                    return true;
                case FeatId.GREATER_TWO_WEAPON_FIGHTING_RANGER:
                    condStructOut = TemplePlusFeatConditions.GreaterTwoWeaponFightingRanger;
                    argout = 0;
                    return true;
                case FeatId.DIVINE_MIGHT:
                    condStructOut = TemplePlusFeatConditions.DivineMight;
                    argout = 0;
                    return true;
                case FeatId.RECKLESS_OFFENSE:
                    condStructOut = TemplePlusFeatConditions.RecklessOffense;
                    argout = 0;
                    return true;
                case FeatId.KNOCK_DOWN:
                    condStructOut = TemplePlusFeatConditions.KnockDown;
                    argout = 0;
                    return true;
                case FeatId.SUPERIOR_EXPERTISE:
                    condStructOut = default;
                    argout = default;
                    return false; // willl just be patched inside Combat Expertise
                case FeatId.DEADLY_PRECISION:
                    condStructOut = TemplePlusFeatConditions.DeadlyPrecision;
                    argout = 0;
                    return true;
                case FeatId.PERSISTENT_SPELL:
                    condStructOut = TemplePlusFeatConditions.PersistentSpell;
                    argout = 0;
                    return true;
            }

            if (featEnum >= FeatId.GREATER_WEAPON_SPECIALIZATION_GAUNTLET
                && featEnum <= FeatId.GREATER_WEAPON_SPECIALIZATION_GRAPPLE)
            {
                condStructOut = TemplePlusFeatConditions.GreaterWeaponSpecialization;
                argout = featEnum - FeatId.GREATER_WEAPON_SPECIALIZATION_GAUNTLET;
                return true;
            }

            // Search the new Feat-CondStruct dictionary
            // TODO: Custom feat->cond mapping

            if (FeatConditionMapping.Mapping.TryGetValue(featEnum, out var featCond))
            {
                condStructOut = featCond.Condition;
                argout = featCond.Arg;
                return true;
            }

            condStructOut = default;
            argout = default;
            return false;
        }

        [TempleDllLocation(0x1004f910)]
        [TemplePlusLocation("d20.cpp:169")]
        private void D20StatusInitFromInternalFields(GameObjectBody objHnd, Dispatcher dispatcher)
        {
            Span<int> condArgs = stackalloc int[100];

            dispatcher.ClearConditions();

            var conditions = objHnd.GetInt32Array(obj_f.conditions);
            for (int i = 0, j = 0; i < conditions.Count; i++)
            {
                var condId = objHnd.GetInt32(obj_f.conditions, i);
                var condStruct = GameSystems.D20.Conditions.GetByHash(condId);
                if (condStruct != null)
                {
                    for (int k = 0; k < condStruct.numArgs; k++)
                    {
                        condArgs[k] = objHnd.GetInt32(obj_f.condition_arg0, j++);
                    }

                    if (condStruct.condName != "Unconscious")
                    {
                        dispatcher.AddCondFromInternalFields(condStruct, condArgs);
                    }
                }
                else
                {
                    Logger.Warn("Object {0} referenced unknown condition {1} in obj_f.conditions", objHnd, condId);
                }
            }

            int numPermMods = objHnd.GetArrayLength(obj_f.permanent_mods);
            int troubledIdx = -1;
            int actualArgCount = 0;
            for (int i = 0, j = 0; i < numPermMods; ++i)
            {
                var condId = objHnd.GetInt32(obj_f.permanent_mods, i);

                ConditionSpec condStruct;
                if (condId != 0)
                {
                    condStruct = GameSystems.D20.Conditions.GetByHash(condId);
                    if (condStruct == null && VanillaElfHashes.TryGetVanillaString(condId, out var originalName))
                    {
                        condStruct = GameSystems.D20.Conditions[originalName];
                        // TODO: remove this hack for currently unsupported condition
                        if (originalName == "Psi Points")
                        {
                            j += 4;
                            actualArgCount += 4;
                            continue;
                        }
                        if (condStruct == null)
                        {
                            throw new CorruptSaveException($"{objHnd} references unknown condition {originalName}");
                        }
                    }

                    if (condStruct == null)
                    {
                        troubledIdx = i;
                        Logger.Debug(
                            "Missing condition {0} for {1}, permanent mod idx: {2}/{3}, arg idx {4}; attempting to recover",
                            condId,
                            objHnd, i, numPermMods, j);
                        break;
                    }
                }
                else
                {
                    Logger.Debug("Found a null condition on the permanent mods of {0}", objHnd);
                    continue;
                }

                if (condStruct.numArgs > 10)
                {
                    troubledIdx = i;
                    Logger.Warn("Found condition with unusual number of args!");
                    break;
                }

                actualArgCount += condStruct.numArgs;
                for (int k = 0; k < condStruct.numArgs; k++)
                {
                    condArgs[k] = objHnd.GetInt32(obj_f.permanent_mod_data, j++);
                }

                dispatcher.SetPermanentModArgsFromDataFields(condStruct, condArgs);
            }

            // attempt recovery if necessary
            if (troubledIdx != -1)
            {
                actualArgCount = 0;
                var condRecover = new List<ConditionSpec>();
                for (int i = 0; i < numPermMods; i++)
                {
                    var condStruct = GameSystems.D20.Conditions.GetByHash(objHnd.GetInt32(obj_f.permanent_mods, i));
                    condRecover.Add(condStruct);

                    if (i == troubledIdx)
                    {
                        continue;
                    }

                    if (condStruct == null)
                    {
                        Logger.Debug("Missing ANOTHER condStruct for {0}, permanent mod idx: {1}/{2}; shit",
                            GameSystems.MapObject.GetDisplayName(objHnd), i, numPermMods);
                        break;
                    }

                    if (condStruct.numArgs > 10)
                    {
                        Logger.Warn("Found condition with unusual number of args!");
                        break;
                    }

                    actualArgCount += condStruct.numArgs;
                }

                var numOfArgs = objHnd.GetArrayLength(obj_f.permanent_mod_data);
                var diff = numOfArgs - actualArgCount;

                for (int i = 0, j = 0; i < numPermMods; i++)
                {
                    if (i == troubledIdx)
                    {
                        j += diff;
                        continue;
                    }

                    var condStruct = GameSystems.D20.Conditions.GetByHash(objHnd.GetInt32(obj_f.permanent_mods, i));
                    if (condStruct == null)
                    {
                        Logger.Debug(
                            "Missing ANOTHER condStruct for {0}, permanent mod idx: {1}/{2}, arg idx {3}; Too bad",
                            GameSystems.MapObject.GetDisplayName(objHnd), i, numPermMods, j);
                        break;
                    }

                    if (condStruct.numArgs > 10)
                    {
                        Logger.Warn("Found condition with unusual number of args!");
                        break;
                    }

                    if (i < troubledIdx)
                    {
                        j += condStruct.numArgs;
                        continue;
                    }

                    for (int k = 0; k < condStruct.numArgs; k++)
                    {
                        condArgs[k] = objHnd.GetInt32(obj_f.permanent_mod_data, j++);
                    }

                    dispatcher.SetPermanentModArgsFromDataFields(condStruct, condArgs);
                }
            }

            dispatcher.DispatcherCondsResetFlag2();

            var dispIo = new DispIoD20Signal();
            dispatcher.Process(DispatcherType.D20Signal, D20DispatcherKey.SIG_Unpack, dispIo);
        }
    }
}