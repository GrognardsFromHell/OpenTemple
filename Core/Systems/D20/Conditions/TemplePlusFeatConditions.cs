using System;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.RadialMenus;

namespace SpicyTemple.Core.Systems.D20.Conditions
{
    public class TemplePlusFeatConditions
    {
        public static readonly ConditionSpec DisableAoo = ConditionSpec.Create("Disable AoO", 1)
            .SetUnique()
            .AddQueryHandler(D20DispatcherKey.QUE_AOOPossible, (in DispatcherCallbackArgs evt) =>
            {
                var dispIo = evt.GetDispIoD20Query();
                if (dispIo.return_val != 0 && evt.GetConditionArg1() != 0)
                {
                    dispIo.return_val = 0;
                }
            })
            .AddHandler(DispatcherType.RadialMenuEntry, (in DispatcherCallbackArgs evt) =>
            {
                var entry = evt.CreateToggleForArg(0);
                entry.text = GameSystems.D20.Combat.GetCombatMesLine(5105);
                entry.helpSystemHashkey = "TAG_RADIAL_MENU_DISABLE_AOOS";
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref entry,
                    RadialMenuStandardNode.Options);
            })
            .Build();

        public static readonly ConditionSpec GreaterTwoWeaponFighting = ConditionSpec
            .Create("Greater Two Weapon Fighting", 2)
            .SetUnique()
            .Prevents(IsGreaterTwoWeaponFightingCondition)
            // same callback as Improved TWF (it just adds an extra attack... logic is inside the action sequence / d20 / GlobalToHit functions
            .AddHandler(DispatcherType.GetNumAttacksBase, GreaterTwoWeaponFightingGetNumAttacks)
            .Build();

        private static void GreaterTwoWeaponFightingGetNumAttacks(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var mainWeapon = GameSystems.Item.ItemWornAt(evt.objHndCaller, EquipSlot.WeaponPrimary);
            var offhand = GameSystems.Item.ItemWornAt(evt.objHndCaller, EquipSlot.WeaponSecondary);

            if (mainWeapon != offhand && mainWeapon != null && mainWeapon.type == ObjectType.weapon && offhand != null)
            {
                var weapFlags = mainWeapon.WeaponFlags;
                if ((weapFlags & WeaponFlag.RANGED_WEAPON) == 0 && offhand.type != ObjectType.armor)
                {
                    ++dispIo.returnVal;
                }
            }
        }

        public static readonly ConditionSpec GreaterTwoWeaponFightingRanger = ConditionSpec
            .Create("Greater Two Weapon Fighting Ranger", 2)
            .SetUnique()
            .Prevents(IsGreaterTwoWeaponFightingCondition)
            // same callback as Improved TWF (it just adds an extra attack... logic is inside the action sequence / d20 / GlobalToHit functions
            .AddHandler(DispatcherType.GetNumAttacksBase, (in DispatcherCallbackArgs evt) =>
            {
                if (evt.objHndCaller.IsWearingLightArmorOrLess())
                {
                    GreaterTwoWeaponFightingGetNumAttacks(in evt);
                }
            })
            .Build();

        private static bool IsGreaterTwoWeaponFightingCondition(ConditionSpec cond) =>
            cond == GreaterTwoWeaponFighting || cond == GreaterTwoWeaponFightingRanger;

        public static readonly ConditionSpec DivineMight = ConditionSpec
            .Create("Divine Might", 2)
            .SetUnique()
            .AddHandler(DispatcherType.ConditionAdd, (in DispatcherCallbackArgs evt) => evt.SetConditionArg1(1))
            .AddHandler(DispatcherType.RadialMenuEntry, (in DispatcherCallbackArgs evt) =>
            {
                if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_IsFallenPaladin))
                    return;

                var entry = RadialMenuEntry.CreateAction(5106, D20ActionType.DIVINE_MIGHT, 0, "TAG_DIVINE_MIGHT");
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref entry, RadialMenuStandardNode.Feats);
            })
            .Build();

        // Divine Might Bonus (gets activated when you choose the action from the Radial Menu)
        public static readonly ConditionSpec DivineMightBonus = ConditionSpec
            .Create("Divine Might Bonus", 2)
            .SetUnique()
            .AddHandler(DispatcherType.DealingDamage, (in DispatcherCallbackArgs evt) =>
            {
                var dispIo = evt.GetDispIoDamage();
                var featName = GameSystems.Feat.GetFeatName(FeatId.DIVINE_MIGHT);
                var damBonus = evt.GetConditionArg1();
                dispIo.damage.AddDamageBonus(damBonus, 0, 114, featName);
            })
            .AddHandler(DispatcherType.BeginRound, RemoveSelf)
            .AddSignalHandler(D20DispatcherKey.SIG_Killed, RemoveSelf)
            .AddHandler(DispatcherType.EffectTooltip, (in DispatcherCallbackArgs evt) =>
            {
                var dispIo = evt.GetDispIoEffectTooltip();
                dispIo.bdb.AddEntry(BuffDebuffType.Buff, 81, "Divine Might");
            })
            .Build();

        private static void RemoveSelf(in DispatcherCallbackArgs evt)
        {
            evt.RemoveThisCondition();
        }

        public static readonly ConditionSpec RecklessOffense = ConditionSpec
            .Create("Reckless Offense", 2)
            .SetUnique()
            .AddHandler(DispatcherType.RadialMenuEntry, (in DispatcherCallbackArgs evt) =>
            {
                var entry = evt.CreateToggleForArg(0);
                entry.text = GameSystems.D20.Combat.GetCombatMesLine(5107);
                entry.helpSystemHashkey = "TAG_FEAT_RECKLESS_OFFENSE";
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref entry, RadialMenuStandardNode.Feats);
            })
            .AddHandler(DispatcherType.GetAC, (in DispatcherCallbackArgs evt) =>
            {
                if (evt.GetConditionArg1() != 0)
                {
                    if (evt.GetConditionArg2() != 0)
                    {
                        var dispIo = evt.GetDispIoAttackBonus();
                        dispIo.bonlist.AddBonus(-4, 8, 337);
                    }
                }
            })
            .AddHandler(DispatcherType.ToHitBonus2, (in DispatcherCallbackArgs evt) =>
            {
                if (evt.GetConditionArg1() != 0)
                {
                    var dispIo = evt.GetDispIoAttackBonus();
                    if ((dispIo.attackPacket.flags & D20CAF.RANGED) == 0)
                    {
                        dispIo.bonlist.AddBonus(2, 0, 337);
                    }
                }
            })
            .AddSignalHandler(D20DispatcherKey.SIG_Attack_Made, FeatConditions.TacticalOptionAbusePrevention)
            .AddHandler(DispatcherType.BeginRound, (in DispatcherCallbackArgs evt) => evt.SetConditionArg2(0))
            .AddHandler(DispatcherType.ConditionAdd, (in DispatcherCallbackArgs evt) => evt.SetConditionArg1(0))
            .Build();

        public static readonly ConditionSpec KnockDown = ConditionSpec.Create("Knock-Down", 2).Build();

        public static readonly ConditionSpec GreaterWeaponSpecialization = ConditionSpec
            .Create("Greater Weapon Specialization", 2)
            .SetUniqueWithKeyArg1()
            .AddHandler(DispatcherType.DealingDamage, (in DispatcherCallbackArgs evt) =>
            {
                var feat = (FeatId) evt.GetConditionArg1();
                WeaponType wpnTypeFromCond = (WeaponType) evt.GetConditionArg2();
                var dispIo = evt.GetDispIoDamage();
                var weapon = dispIo.attackPacket.GetWeaponUsed();
                var usedWeaponType = weapon?.GetWeaponType() ?? WeaponType.unarmed_strike_medium_sized_being;

                if (usedWeaponType == wpnTypeFromCond)
                {
                    var featName = GameSystems.Feat.GetFeatName(feat);
                    dispIo.damage.AddDamageBonus(2, 0, 114, featName);
                }
            })
            .Build();

        public static readonly ConditionSpec DeadlyPrecision = ConditionSpec.Create("Deadly Precision", 2)
            .Build();

        public static readonly ConditionSpec PersistentSpell = ConditionSpec.Create("Persistent Spell", 2)
            .Build();

        // Disarm
        public static readonly ConditionSpec Disarm = ConditionSpec.Create("Disarm", 2)
            .SetUnique()
            .AddHandler(DispatcherType.RadialMenuEntry, (in DispatcherCallbackArgs evt) =>
            {
                var entry = RadialMenuEntry.CreateAction(5109, D20ActionType.DISARM, 0, "TAG_DISARM");
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref entry,
                    RadialMenuStandardNode.Offense);
            })
            .AddSignalHandler(D20DispatcherKey.SIG_HP_Changed, (in DispatcherCallbackArgs evt) =>
            {
                DispIoD20Signal dispIo = evt.GetDispIoD20Signal();
                int hpChange = dispIo.data1;
                if (hpChange < 0)
                {
                    if (evt.GetConditionArg2() != 0)
                    {
                        evt.SetConditionArg1(1);
                    }
                }
            })
            .AddQueryHandler(D20DispatcherKey.QUE_ActionTriggersAOO, (in DispatcherCallbackArgs evt) =>
            {
                DispIoD20Query dispIo = evt.GetDispIoD20Query();
                var action = (D20Action) dispIo.obj;
                if (action.d20ActType == D20ActionType.DISARM)
                {
                    if (!GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.IMPROVED_DISARM))
                    {
                        dispIo.return_val = 1;
                    }

                    evt.SetConditionArg2(1);
                }
            })
            .AddQueryHandler(D20DispatcherKey.QUE_Can_Perform_Disarm, (in DispatcherCallbackArgs evt) =>
            {
                var dispIo = evt.GetDispIoD20Query();
                evt.SetConditionArg2(0);
                if (evt.GetConditionArg1() == 0)
                {
                    dispIo.return_val = 1;
                }

                evt.SetConditionArg1(0);
            })
            .Build();


        // Disarm
        public static readonly ConditionSpec Disarmed = ConditionSpec.Create("Disarmed", 8)
            .SetUnique()
            .AddQueryHandler(D20DispatcherKey.QUE_Disarmed, (in DispatcherCallbackArgs evt) =>
            {
                DispIoD20Query dispIo = evt.GetDispIoD20Query();
                dispIo.return_val = 1;

                throw new NotImplementedException();
                // Retrieve ObjectID of disarmed weapon from args!
                // dispIo.obj = weapon;
            })
            .AddSignalHandler(D20DispatcherKey.SIG_Combat_End, (in DispatcherCallbackArgs evt) =>
            {
                var arg8 = evt.GetConditionArg(7);
                if (arg8 < 2
                    && GameSystems.Party.IsInParty(evt.objHndCaller)
                    && !GameSystems.Critter.IsDeadOrUnconscious(evt.objHndCaller))
                {
                    GameUiBridge.ShowTextBubble(evt.objHndCaller, "I was disarmed.");
                    evt.SetConditionArg(7, arg8 + 1);
                }
            })
            .AddSignalHandler(D20DispatcherKey.SIG_Disarmed_Weapon_Retrieve, (in DispatcherCallbackArgs evt) =>
            {
                GameObjectBody weapon;
                var dispIo = evt.GetDispIoD20Signal();
                var d20a = (D20Action) dispIo.obj;
                if (d20a.d20ATarget != null && d20a.d20ATarget.type == ObjectType.weapon)
                {
                    weapon = d20a.d20ATarget;
                }
                else
                {
//                     ObjectId objId;
//                    memcpy(&objId, evt.subDispNode.condNode.args, sizeof(ObjectId));
//                    weapon = GameSystems.Obj.GetHandleById(objId);
                    throw new NotImplementedException("Get the weapon from the obj id stored in the cond args");
                }

                if (weapon == null
                    || GameSystems.Item.GetParent(weapon) != null && GameSystems.Combat.IsCombatActive()
                    || weapon.type != ObjectType.weapon)
                {
                    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 195); //fail!
                    if (evt.GetConditionArg(6) < 2)
                        evt.SetConditionArg(6, evt.GetConditionArg(6) + 1);
                    else
                        evt.RemoveThisCondition();
                    return;
                }

                if (GameSystems.Item.ItemWornAt(evt.objHndCaller, EquipSlot.WeaponPrimary) == null)
                {
                    GameSystems.Item.SetParentAdvanced(weapon, evt.objHndCaller, 203, 0);
                }
                else
                {
                    GameSystems.Item.SetParentAdvanced(weapon, evt.objHndCaller, -1, 0);
                }

                GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 201);
                evt.RemoveThisCondition();
            })
            .AddHandler(DispatcherType.RadialMenuEntry, (in DispatcherCallbackArgs evt) =>
            {
                var entry = RadialMenuEntry.CreateAction(5111, D20ActionType.DISARMED_WEAPON_RETRIEVE, 0, "TAG_DISARM");
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref entry, RadialMenuStandardNode.Items);
            })
            .AddHandler(DispatcherType.ConditionAdd, (in DispatcherCallbackArgs evt) =>
            {
                // Instead of storing this obj id, pack/unpack should be used
                throw new NotImplementedException(
                    "Take the handle stored in arg0, convert it to an objid and store that in arg0");
            })
            .Build();

        public static readonly ConditionSpec PreferOneHandedWield = ConditionSpec.Create("Prefer One Handed Wield", 1)
            .AddQueryHandler(D20DispatcherKey.QUE_Is_Preferring_One_Handed_Wield, (in DispatcherCallbackArgs evt) =>
            {
                var dispIo = evt.GetDispIoD20Query();
                var isCurrentlyOn = evt.GetConditionArg1();
                dispIo.return_val = isCurrentlyOn;
            })
            .AddHandler(DispatcherType.RadialMenuEntry, (in DispatcherCallbackArgs evt) =>
            {
                var shield = GameSystems.Item.ItemWornAt(evt.objHndCaller, EquipSlot.Shield);
                if (shield == null || !GameSystems.Item.IsBuckler(shield))
                    return;

                var entry = evt.CreateToggleForArg(0);
                entry.text = GameSystems.D20.Combat.GetCombatMesLine(5124);
                entry.helpSystemHashkey = "TAG_RADIAL_MENU_PREFER_ONE_HANDED_WIELD";
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref entry,
                    RadialMenuStandardNode.Options);
            })
            .Build();

        public static readonly ConditionSpec AidAnother = ConditionSpec.Create("Aid Another")
            .SetUnique()
            .AddHandler(DispatcherType.RadialMenuEntry, (in DispatcherCallbackArgs evt) =>
            {
                var radMenuAidAnotherMain = RadialMenuEntry.CreateParent(5112);
                radMenuAidAnotherMain.helpSystemHashkey = "TAG_AID_ANOTHER";

                int newParent = GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller,
                    ref radMenuAidAnotherMain, RadialMenuStandardNode.Tactical);

                var wakeUp =
                    RadialMenuEntry.CreateAction(5113, D20ActionType.AID_ANOTHER_WAKE_UP, 0, "TAG_AID_ANOTHER");
                GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref wakeUp, newParent);
            })
            .Build();

        public static readonly ConditionSpec[] Conditions =
        {
            DisableAoo,
            Disarm,
            AidAnother,
            PreferOneHandedWield
        };

    }
}