using System;
using System.Diagnostics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Systems.Feats;

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

                // TODO initClass(obj);

                // TODO initRace(obj);

                // TODO initFeats(obj);
            }
            else
            {
                Logger.Info("Attempted D20Status Init for non-critter {0}", obj);
            }

            initItemConditions(obj);

            // TODO D20StatusInitFromInternalFields(obj, dispatcher);

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
                                // TODO dispatcher._ConditionAdd_NumArgs0(conds.ConditionDisabled);
                            }
                            else
                            {
                                // TODO dispatcher._ConditionAdd_NumArgs0(conds.ConditionUnconscious);
                            }
                        }

                        else
                        {
                            if (hpCur == 0)
                            {
                                // TODO dispatcher._ConditionAdd_NumArgs0(conds.ConditionDisabled);
                            }
                            else if ((int) subdualDam > hpCur)
                            {
                                // TODO dispatcher._ConditionAdd_NumArgs0(conds.ConditionUnconscious);
                            }
                        }
                    }
                }
            }
        }

        [TempleDllLocation(0x1004ca00)]
        public void initItemConditions(GameObjectBody obj)
        {

            var dispatcher = (Dispatcher) obj.GetDispatcher();
            if (dispatcher == null) {
                return;
            }

            if (obj.IsCritter()) {
                dispatcher.ClearItemConditions();
                if (GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_Polymorphed) == 0)
                {
                    foreach (var item in obj.EnumerateChildren())
                    {
                        var itemInvLocation = item.GetInt32(obj_f.item_inv_location);
                        if (AreItemConditionsActive(item, itemInvLocation)) {
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
            if ( item.type == ObjectType.weapon
                 || item.type == ObjectType.armor && item.GetArmorFlags().IsShield()
                 || item.GetItemWearFlags() != default )
            {
                return ItemSystem.IsInvIdxWorn(itemInvIdx);
            }
            else
            {
                return true;
            }
        }

        [TempleDllLocation(0x100ff500)]
        private void InitFromItemConditionFields(Dispatcher dispatcher, GameObjectBody item, int invIdx) {

            var itemConds = item.GetInt32Array(obj_f.item_pad_wielder_condition_array);
            var itemArgs = item.GetInt32Array(obj_f.item_pad_wielder_argument_array);

            var argIdx = 0;
            for (var i = 0; i < itemConds.Count; i++){
                var condId = itemConds[i];
                var condStruct = GameSystems.D20.Conditions.GetByHash(condId);
                if (condStruct == null){
                    Logger.Warn($"Item condition {condId} not found!");
                    continue;
                }

                Span<int> args = stackalloc int[condStruct.numArgs];
                for (var j = 0; j < condStruct.numArgs; j++)	{
                    args[j] = itemArgs[argIdx++];
                }
                if (args.Length >= 3)
                {
                    args[2] = invIdx;
                }

                dispatcher.AddItemCondition(condStruct, args);

            }


        }

    }
}