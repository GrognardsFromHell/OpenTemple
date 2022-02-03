using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.RadialMenus;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class Wildshape
    {
        // mainly to replace the lack of D20StatusInit callback
        // In vanilla, all the callbacks for *being* wild shaped were part of this condition,
        // but we split it up into the feat for shaping, and the condition for being wild-shaped.
        [TempleDllLocation(0x102ee3a8)] [AutoRegister]
        public static readonly ConditionSpec FeatCondition = ConditionSpec.Create("Wild Shape", 3)
            .SetUnique()
            .AddHandler(DispatcherType.RadialMenuEntry, DruidWildShapeInit) // otherwise you go into init hell
            .Build();

        // because the wild shape args get overwritten on each init
        [AutoRegister]
        public static readonly ConditionSpec WildShaped = ConditionSpec.Create("Wild Shaped", 3)
            .SetUnique()
            .AddHandler(DispatcherType.ConditionAdd, DruidWildShapedInit)
            .AddHandler(DispatcherType.RadialMenuEntry, WildShapeRadialMenu)
            .AddHandler(DispatcherType.D20ActionCheck, (D20DispatcherKey) 119, WildShapeCheck)
            .AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey) 119, WildShapeMorph)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, WildShapeInit)
            .AddHandler(DispatcherType.BeginRound, WildShapeBeginRound)
            .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_STRENGTH,
                WildshapeReplaceStats)
            .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_DEXTERITY,
                WildshapeReplaceStats)
            .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_CONSTITUTION,
                WildshapeReplaceStats)
            .AddQueryHandler(D20DispatcherKey.QUE_Polymorphed, WildShapePolymorphedQuery)
            .AddHandler(DispatcherType.GetCritterNaturalAttacksNum, DruidWildShapeGetNumAttacks)
            .AddQueryHandler(D20DispatcherKey.QUE_CannotCast, WildShapeCannotCastQuery)
            .AddHandler(DispatcherType.ConditionAddFromD20StatusInit,
                DruidWildShapeD20StatusInit) // takes care of resetting the item conditions
            .AddHandler(DispatcherType.GetModelScale, DruidWildShapeScale) // NEW! scales the model too
            .AddSignalHandler("Deduct Wild Shape Charge", DeductWildshapeCharge)
            .AddQueryHandler("Wild Shape Charges", GetWildshapeCharges)
            .AddQueryHandler("Wild Shaped Condition Added", WildshapeConditionAdded)
            .Build();

        [TemplePlusLocation("ClassAbilityCallbacks::DruidWildShapedInit")]
        private static void DruidWildShapedInit(in DispatcherCallbackArgs evt)
        {
        }

        [TemplePlusLocation("ClassAbilityCallbacks::DruidWildShapeD20StatusInit")]
        private static void DruidWildShapeD20StatusInit(in DispatcherCallbackArgs evt)
        {
            if (evt.GetConditionArg3() != 0)
                GameSystems.D20.Status.UpdateItemConditions(evt.objHndCaller);
        }

        [TemplePlusLocation("ClassAbilityCallbacks::DruidWildShapeGetNumAttacks")]
        private static void DruidWildShapeGetNumAttacks(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();

            var protoId = GameSystems.D20.D20QueryInt(evt.objHndCaller, D20DispatcherKey.QUE_Polymorphed);
            if (protoId == 0)
            {
                return;
            }

            var protoHandle = GameSystems.Proto.GetProtoById(protoId);
            if (protoHandle == null)
            {
                return;
            }

            dispIo.returnVal = (ActionErrorCode) GameSystems.Critter.GetCritterNaturalAttackCount(protoHandle);
        }

        [TemplePlusLocation("ClassAbilityCallbacks::DruidWildShapeScale")]
        private static void DruidWildShapeScale(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoMoveSpeed();

            var protoId = GameSystems.D20.D20QueryInt(evt.objHndCaller, D20DispatcherKey.QUE_Polymorphed);
            if (protoId == 0)
                return;
            var protoHandle = GameSystems.Proto.GetProtoById(protoId);
            if (protoHandle == null)
                return;

            var protoScale = protoHandle.GetInt32(obj_f.model_scale);
            dispIo.bonlist.bonusEntries[0].bonValue = protoScale; // modifies the initial value
        }

        [TemplePlusLocation("ClassAbilityCallbacks::DruidWildShapeInit")]
        private static void DruidWildShapeInit(in DispatcherCallbackArgs evt)
        {
            var druidLvl = evt.objHndCaller.GetStat(Stat.level_druid);
            var numTimes = 1; // number of times can wild shape per day
            if (druidLvl >= 6)
            {
                switch (druidLvl)
                {
                    case 6:
                        numTimes = 2;
                        break;
                    case 7:
                    case 8:
                    case 9:
                        numTimes = 3;
                        break;
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                        numTimes = 4;
                        break;
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                        numTimes = 5;
                        break;
                    default: // 18 and above
                        numTimes = 6;
                        break;
                }

                // elemental num times (new)
                if (druidLvl >= 16)
                {
                    numTimes += (1 << 8);
                }

                if (druidLvl >= 18)
                    numTimes += (1 << 8);
                if (druidLvl >= 20)
                    numTimes += (1 << 8);
            }

            evt.SetConditionArg1(numTimes);

            // Add if the condition has not already been added.  The extender messes up things up if a query is not used and
            // the condition can get added many times.
            var res = GameSystems.D20.D20QueryPython(evt.objHndCaller, "Wild Shaped Condition Added");
            if (res == 0)
            {
                evt.objHndCaller.AddCondition(WildShaped, numTimes, 0, 0);
            }
        }

        [DispTypes(DispatcherType.ConditionAdd, DispatcherType.NewDay)]
        [TempleDllLocation(0x100fbdb0)]
        [TemplePlusLocation("condition.cpp:472")]
        public static void WildShapeInit(in DispatcherCallbackArgs evt)
        {
            var druidLvl = evt.objHndCaller.GetStat(Stat.level_druid);
            var numTimes = 1; // number of times can wild shape per day
            if (druidLvl >= 6)
            {
                switch (druidLvl)
                {
                    case 6:
                        numTimes = 2;
                        break;
                    case 7:
                    case 8:
                    case 9:
                        numTimes = 3;
                        break;
                    case 10:
                    case 11:
                    case 12:
                    case 13:
                        numTimes = 4;
                        break;
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                        numTimes = 5;
                        break;
                    default: // 18 and above
                        numTimes = 6;
                        break;
                }

                // elemental num times (new)
                if (druidLvl >= 16)
                {
                    numTimes += (1 << 8);
                }

                if (druidLvl >= 18)
                    numTimes += (1 << 8);
                if (druidLvl >= 20)
                    numTimes += (1 << 8);
            }

            //See if any bonus uses should be added
            var extraWildShape = GameSystems.D20.D20QueryPython(evt.objHndCaller, "Extra Wildshape Uses");
            var extraElementalWildShape =
                GameSystems.D20.D20QueryPython(evt.objHndCaller, "Extra Wildshape Elemental Uses");
            numTimes += extraWildShape;
            numTimes += (1 << 8) * extraElementalWildShape;

            evt.SetConditionArg1(numTimes);
            if (evt.GetConditionArg3() != 0)
            {
                evt.SetConditionArg3(0);
                evt.objHndCaller.FreeAnimHandle();

                GameSystems.ParticleSys.CreateAtObj("sp-animal shape", evt.objHndCaller);
                GameSystems.D20.Status.UpdateItemConditions(evt.objHndCaller);
            }
        }

        private const int WildShapeActionBitmask = 1 << 24;

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100fbb20)]
        [TemplePlusLocation("condition.cpp:473")]
        public static void WildShapeRadialMenu(in DispatcherCallbackArgs evt)
        {
            var wildshapeMain = RadialMenuEntry.CreateParent(5076);

            var druid = evt.objHndCaller;
            var wsId = GameSystems.D20.RadialMenu.AddToStandardNode(druid, ref wildshapeMain,
                RadialMenuStandardNode.Class);

            // if wild shape active - add "Deactivate" node
            if (evt.GetConditionArg3() != 0)
            {
                var wsDeactivate = RadialMenuEntry.CreateAction(5077, D20ActionType.CLASS_ABILITY_SA,
                    (int) WildShapeProtoIdx.Deactivate, "TAG_CLASS_FEATURES_DRUID_WILD_SHAPE");
                wsDeactivate.AddAsChild(druid, wsId);
                return;
            }

            // else add the WS options
            void AddOption(WildShapeProtoIdx optionIdx, int parentIdx)
            {
                var wsProto = DruidWildShapes.Options[optionIdx].protoId;
                var protoCode = (int) optionIdx | WildShapeActionBitmask;

                var wsOption = RadialMenuEntry.CreateAction(0, D20ActionType.CLASS_ABILITY_SA, protoCode,
                    "TAG_CLASS_FEATURES_DRUID_WILD_SHAPE");

                var protoHandle = GameSystems.Proto.GetProtoById((ushort) wsProto);
                wsOption.text = GameSystems.MapObject.GetDisplayName(protoHandle);
                GameSystems.D20.RadialMenu.AddChildNode(druid, ref wsOption, parentIdx);
            }

            var druidLvl = druid.GetStat(Stat.level_druid);
            foreach (var kvp in DruidWildShapes.Options)
            {
                if (druidLvl >= kvp.Value.minLvl && kvp.Value.monCat == MonsterCategory.animal)
                {
                    AddOption(kvp.Key, wsId);
                }
            }

            if (GameSystems.D20.D20Query(druid, D20DispatcherKey.QUE_Wearing_Ring_of_Change))
            {
                AddOption(WildShapeProtoIdx.Hill_Giant, wsId);
            }

            // elementals
            if (druidLvl >= 16)
            {
                var wsElem = RadialMenuEntry.CreateParent(5118);
                var elemId = GameSystems.D20.RadialMenu.AddChildNode(druid, ref wsElem, wsId);

                foreach (var kvp in DruidWildShapes.Options)
                {
                    if (druidLvl >= kvp.Value.minLvl && kvp.Value.monCat == MonsterCategory.elemental)
                    {
                        AddOption(kvp.Key, elemId);
                    }
                }
            }
        }


        [DispTypes(DispatcherType.D20ActionCheck)]
        [TempleDllLocation(0x100fbc60)]
        [TemplePlusLocation("condition.cpp:474")]
        public static void WildShapeCheck(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var druidLvl = evt.objHndCaller.GetStat(Stat.level_druid);

            var action = dispIo.action;
            if ((action.data1 & WildShapeActionBitmask) != 0)
            {
                if (evt.GetConditionArg3() != 0) // already polymorphed
                    return;

                var optionId = (WildShapeProtoIdx) (action.data1 & ~WildShapeActionBitmask);
                var spec = DruidWildShapes.Options[optionId];
                if (druidLvl < spec.minLvl && spec.minLvl != -1)
                {
                    dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                }

                var numTimes = evt.GetConditionArg1();
                if (spec.monCat == MonsterCategory.elemental)
                {
                    numTimes >>= 8;
                    if (numTimes <= 0)
                        dispIo.returnVal = ActionErrorCode.AEC_OUT_OF_CHARGES;
                }
                else
                {
                    // normal animal (or plant)
                    numTimes &= 0xFF;
                    if (numTimes <= 0)
                        dispIo.returnVal = ActionErrorCode.AEC_OUT_OF_CHARGES;
                }
            }
        }

        [DispTypes(DispatcherType.D20ActionPerform)]
        [TempleDllLocation(0x100fbce0)]
        [TemplePlusLocation("condition.cpp:475")]
        public static void WildShapeMorph(in DispatcherCallbackArgs evt)
        {
            var druid = evt.objHndCaller;
            var druidLvl = druid.GetStat(Stat.level_druid);

            var dispIo = evt.GetDispIoD20ActionTurnBased();

            var action = dispIo.action;
            if ((action.data1 & WildShapeActionBitmask) == 0)
            {
                return;
            }

            void initObj(int protoId)
            {
                druid.FreeAnimHandle();
                if (protoId != 0)
                {
                    var lvl = druid.GetStat(Stat.level);
                    GameSystems.Combat.Heal(druid, druid, Dice.Constant(lvl), D20ActionType.CLASS_ABILITY_SA);
                }

                GameSystems.ParticleSys.CreateAtObj("sp-animal shape", druid);
                GameSystems.D20.Status.UpdateItemConditions(druid);
            }

            var curWsProto = evt.GetConditionArg3();
            if (curWsProto != 0)
            {
                // deactivating
                evt.SetConditionArg3(0);
                initObj(0);
                return;
            }

            var numTimes = evt.GetConditionArg1();
            var idx = (WildShapeProtoIdx) (action.data1 & ~WildShapeActionBitmask);
            var protoSpec = DruidWildShapes.Options[idx];
            if (protoSpec.monCat == MonsterCategory.elemental)
            {
                if ((numTimes >> 8) <= 0)
                    return;
                evt.SetConditionArg1(numTimes - (1 << 8));
            }
            else
            {
                // normal animal or plant
                if ((numTimes & 0xFF) <= 0)
                    return;
                evt.SetConditionArg1(numTimes - 1);
            }

            evt.SetConditionArg2(600 * druidLvl);
            evt.SetConditionArg3(protoSpec.protoId);
            initObj(protoSpec.protoId);
        }


        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100fbe70)]
        public static void WildShapeBeginRound(in DispatcherCallbackArgs evt)
        {
            if ((evt.GetConditionArg3()) != 0)
            {
                var dispIo = evt.GetDispIoD20Signal();
                var v2 = evt.GetConditionArg2() - dispIo.data1;
                if (v2 < 0)
                {
                    evt.SetConditionArg3(0);
                    evt.objHndCaller.FreeAnimHandle();

                    GameSystems.ParticleSys.CreateAtObj("sp-animal shape", evt.objHndCaller);
                    GameSystems.D20.Status.UpdateItemConditions(evt.objHndCaller);
                }

                evt.SetConditionArg2(v2);
            }
        }

        [DispTypes(DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x100fbf30)]
        public static void WildshapeReplaceStats(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoBonusList();
            var protoId = evt.GetConditionArg3();
            if (protoId != 0)
            {
                var attribute = evt.GetAttributeFromDispatcherKey();
                var protoObj = GameSystems.Proto.GetProtoById((ushort) protoId);
                var baseValue = protoObj.GetInt32(obj_f.critter_abilities_idx, (int) attribute);
                var text = GameSystems.D20.Combat.GetCombatMesLine(118);
                dispIo.bonlist.ReplaceBonus(1, baseValue, 102, text);
            }
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100fbf90)]
        public static void WildShapePolymorphedQuery(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            if ((condArg3) != 0)
            {
                evt.GetDispIoD20Query().return_val = condArg3;
            }
        }

        [DispTypes(DispatcherType.GetCritterNaturalAttacksNum)]
        [TempleDllLocation(0x100fc010)]
        public static void WildShapeGetNumAttacks(in DispatcherCallbackArgs evt)
        {
            if ((evt.GetConditionArg3()) != 0)
            {
                var dispIo = evt.GetDispIoD20ActionTurnBased();
                dispIo.returnVal = (ActionErrorCode) GameSystems.D20.Actions.DispatchD20ActionCheck(dispIo.action,
                    dispIo.tbStatus,
                    DispatcherType.GetNumAttacksBase);
            }
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100fbfc0)]
        public static void WildShapeCannotCastQuery(in DispatcherCallbackArgs evt)
        {
            if ((evt.GetConditionArg3()) != 0)
            {
                if (!GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.NATURAL_SPELL))
                {
                    evt.GetDispIoD20Query().return_val = 1;
                }
            }
        }


        // Gets the number of wildshape charges
        public static void GetWildshapeCharges(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            // Wildshape charges are the lower bits, elemental charges are the higher bits
            var numTimes = evt.GetConditionArg1();
            numTimes = numTimes & 255;
            dispIo.resultData = (ulong) numTimes;
            dispIo.return_val = numTimes > 0 ? 1 : 0;
        }

        // Called to deduct a wild shape charge
        public static void DeductWildshapeCharge(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            // Wildshape charges are the lower bits, elemental charges are the higher bits
            var numTimes = evt.GetConditionArg1();
            numTimes = numTimes & 255;
            // Deduct one regular wild shape charge (make sure there is at least one or things could get really goofed up)
            if (numTimes > 0)
            {
                var wildshapeValue = evt.GetConditionArg1() - 1;
                evt.SetConditionArg1(wildshapeValue);
            }
        }

        public static void WildshapeConditionAdded(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            // Used by the C++ side to make sure the condition does not get added multiple times
            dispIo.return_val = 1;
        }

    }

    public static class WildshapeExtensions
    {
        public static int GetWildshapeCharges(this GameObject critter)
        {
            return (int) GameSystems.D20.D20QueryReturnData(critter, "Wild Shape Charges");
        }
    }

}