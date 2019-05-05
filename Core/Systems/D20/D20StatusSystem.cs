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

            // TODO initItemConditions(obj);

            // TODO D20StatusInitFromInternalFields(obj, dispatcher);

            // TODO d20ObjRegistrySys.Append(obj);

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
    }
}