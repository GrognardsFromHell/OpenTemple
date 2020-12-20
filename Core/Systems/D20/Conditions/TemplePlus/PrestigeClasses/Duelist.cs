using System.Collections.Generic;
using System.Collections.Immutable;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    [AutoRegister]
    public class Duelist
    {
        public static readonly Stat ClassId = Stat.level_duelist;

         public static readonly D20ClassSpec ClassSpec = new D20ClassSpec("duelist")
        {
            classEnum = Stat.level_duelist,
            helpTopic = "TAG_DUELISTS",
            conditionName = "Duelist",
            flags = ClassDefinitionFlag.CDF_CoreClass,
            BaseAttackBonusProgression = BaseAttackProgressionType.Martial,
            hitDice = 10,
            FortitudeSaveProgression = SavingThrowProgressionType.LOW,
            ReflexSaveProgression = SavingThrowProgressionType.HIGH,
            WillSaveProgression = SavingThrowProgressionType.LOW,
            skillPts = 4,
            hasArmoredArcaneCasterFeature = false,
            classSkills = new HashSet<SkillId>
            {
                SkillId.bluff,
                SkillId.listen,
                SkillId.sense_motive,
                SkillId.spot,
                SkillId.tumble,
                SkillId.perform,
                SkillId.alchemy,
                SkillId.balance,
                SkillId.escape_artist,
                SkillId.jump,
            }.ToImmutableHashSet(),
            classFeats = new Dictionary<FeatId, int>
            {
                {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1},
                {FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL, 1},
            }.ToImmutableDictionary(),
        };

        private static readonly D20DispatcherKey preciseStrikeEnum = (D20DispatcherKey) 2400;

        public static bool IsArmorless(GameObjectBody obj)
        {
            var armor = obj.ItemWornAt(EquipSlot.Armor);
            if (armor != null)
            {
                var armorFlags = armor.GetArmorFlags();
                if (armorFlags != ArmorFlag.TYPE_NONE)
                {
                    return false;
                }
            }

            var shield = obj.ItemWornAt(EquipSlot.Shield);
            if (shield != null)
            {
                return false;
            }

            return true;
        }

        public static void CannyDefenseAcBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            if (!IsArmorless(evt.objHndCaller))
            {
                return;
            }

            var weap = evt.objHndCaller.ItemWornAt(EquipSlot.WeaponPrimary);
            if (weap == null || GameSystems.Item.IsRangedWeapon(weap))
            {
                weap = evt.objHndCaller.ItemWornAt(EquipSlot.WeaponSecondary);
                if (weap == null || GameSystems.Item.IsRangedWeapon(weap))
                {
                    return;
                }

            }

            var duelistLvl = evt.objHndCaller.GetStat(ClassId);
            var intScore = evt.objHndCaller.GetStat(Stat.intelligence);
            var intBonus = (intScore - 10) / 2;
            if (intBonus <= 0)
            {
                return;
            }

            if (duelistLvl < intBonus)
            {
                intBonus = duelistLvl;
            }

            dispIo.bonlist.ModifyBonus(intBonus, 3, 104); // Dexterity bonus,  ~Class~[TAG_LEVEL_BONUSES]
        }

        public static void ImprovedReactionInitBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoObjBonus();
            var duelistLvl = evt.objHndCaller.GetStat(ClassId);
            if (duelistLvl < 2)
            {
                return;
            }

            var bonVal = 2;
            if (duelistLvl >= 8)
            {
                bonVal = 4;
            }

            dispIo.bonlist.AddBonus(bonVal, 0, 137); // adds untyped bonus to initiative
            return;
        }
        public static void EnhancedMobility(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var duelistLvl = evt.objHndCaller.GetStat(ClassId);
            if (duelistLvl < 3)
            {
                return;
            }

            if (!IsArmorless(evt.objHndCaller))
            {
                return;
            }

            if ((dispIo.attackPacket.flags & D20CAF.AOO_MOVEMENT) != D20CAF.NONE)
            {
                dispIo.bonlist.AddBonus(4, 8, 137); // adds +4 dodge bonus
            }

            return;
        }
        public static void GraceReflexBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            var duelistLvl = evt.objHndCaller.GetStat(ClassId);
            if (duelistLvl < 4)
            {
                return;
            }

            if (!IsArmorless(evt.objHndCaller))
            {
                return;
            }

            dispIo.bonlist.AddBonus(2, 34, 137); // Competence bonus
            return;
        }
        // def PreciseStrikeRadial(attachee, args, dispIo):
        // duelistLvl = attachee.stat_level_get(classEnum)
        // if (duelistLvl < 5):
        // return 0
        // add radial menu action Precise Strike
        // radialAction = tpdp.RadialMenuEntryPythonAction(-1, D20A_PYTHON_ACTION, preciseStrikeEnum, 0,  "TAG_INTERFACE_HELP")
        // radialParentId = radialAction.add_child_to_standard(attachee, tpdp.RadialMenuStandardNode.Class)
        // return 0
        // def OnPreciseStrikeCheck(attachee, args, dispIo):
        // if (not IsUsingLightOrOneHandedPiercing(attachee)):
        // dispIo.return_val = AEC_WRONG_WEAPON_TYPE
        // return 0
        // tgt = dispIo.d20a.target
        // stdChk = ActionCheckTargetStdAtk(attachee, tgt)
        // if (stdChk != AEC_OK):
        // dispIo.return_val = stdChk
        // return 0
        // def OnPreciseStrikePerform(attachee, args, dispIo):
        // print "I performed!"
        // return 0

        private static readonly string preciseStrikeString = "Precise Strike";
        public static void PreciseStrikeDamageBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            var duelistLvl = evt.objHndCaller.GetStat(ClassId);
            if (duelistLvl < 5)
            {
                return;
            }

            // check if attacking with one weapon and without a shield
            if ((evt.objHndCaller.ItemWornAt(EquipSlot.WeaponSecondary) != null && evt.objHndCaller.ItemWornAt(EquipSlot.WeaponPrimary) != null) || evt.objHndCaller.ItemWornAt(EquipSlot.Shield) != null)
            {
                return;
            }

            // check if light or one handed piercing
            if (!IsUsingLightOrOneHandedPiercing(evt.objHndCaller))
            {
                return;
            }

            var tgt = dispIo.attackPacket.victim;
            if (tgt == null) // shouldn't happen but better be safe
            {
                return;
            }

            if (tgt.D20Query(D20DispatcherKey.QUE_Critter_Is_Immune_Critical_Hits))
            {
                return;
            }

            var damage_dice = Dice.D6;
            if (duelistLvl >= 10)
            {
                damage_dice = damage_dice.WithCount(2);
            }

            dispIo.damage.AddDamageDice(damage_dice, DamageType.Unspecified, 127);
        }

        public static void ElaborateParry(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var duelistLvl = evt.objHndCaller.GetStat(ClassId);
            if (duelistLvl < 7)
            {
                return;
            }

            if (!evt.objHndCaller.D20Query(D20DispatcherKey.QUE_FightingDefensively)) // this also covers Total Defense
            {
                return;
            }

            dispIo.bonlist.AddBonus(duelistLvl, 8, 137); // Dodge bonus,  ~Class~[TAG_LEVEL_BONUSES]
            return;
        }

        public static bool IsUsingLightOrOneHandedPiercing(GameObjectBody obj)
        {
            var weap = obj.ItemWornAt(EquipSlot.WeaponPrimary);
            var offhand = obj.ItemWornAt(EquipSlot.WeaponSecondary);
            if (weap == null && offhand == null)
            {
                return false;
            }

            if (weap == null)
            {
                weap = offhand;
                offhand = null;
            }

            if (IsWeaponLightOrOneHandedPiercing(obj, weap))
            {
                return true;
            }

            // check the offhand
            if (offhand != null)
            {
                if (IsWeaponLightOrOneHandedPiercing(obj, offhand))
                {
                    return true;
                }

            }

            return false;
        }

        public static bool IsWeaponLightOrOneHandedPiercing(GameObjectBody obj, GameObjectBody weap)
        {
            // truth table
            // nor. | enlarged |  return
            // 0		x			1 			assume un-enlarged state
            // 1		0			1			shouldn't be possible... unless it's actually reduce person (I don't really care about that)
            // 1		1		is_piercing
            // 1 		2		is_piercing
            // 2		x 			0
            // 3		x 			0
            var normalWieldType = GameSystems.Item.GetWieldType(obj, weap, true); // "normal" means weapon is not enlarged
            if (normalWieldType >= 2) // two handed or unwieldable
            {
                return false;
            }

            if (normalWieldType == 0)
            {
                return true;
            }

            // otherwise if the weapon is also enlarged;
            var wieldType = GameSystems.Item.GetWieldType(obj, weap, false);
            if (wieldType == 0)
            {
                return true;
            }

            // weapon is not light, but is one handed - check if piercing
            var attackType = (DamageType) weap.GetInt32(obj_f.weapon_attacktype);
            if (attackType == DamageType.Piercing) // should be strictly piercing from what I understand (supposed to be rapier-like)
            {
                return true;
            }

            return false;
        }

        public static void DuelistDeflectArrows(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var duelistLvl = evt.objHndCaller.GetStat(ClassId);
            if (duelistLvl < 9)
            {
                return;
            }

            var offendingWeapon = dispIo.attackPacket.GetWeaponUsed();
            if (offendingWeapon == null)
            {
                return;
            }

            if ((dispIo.attackPacket.flags & D20CAF.RANGED) == D20CAF.NONE)
            {
                return;
            }

            // check if attacker visible
            var attacker = dispIo.attackPacket.attacker/*AttackPacket*/;
            if (attacker == null)
            {
                return;
            }

            if (attacker.D20Query(D20DispatcherKey.QUE_Critter_Is_Invisible)
                && !evt.objHndCaller.D20Query(D20DispatcherKey.QUE_Critter_Can_See_Invisible))
            {
                return;
            }

            if (evt.objHndCaller.D20Query(D20DispatcherKey.QUE_Critter_Is_Blinded))
            {
                return;
            }

            // check flatfooted
            if (evt.objHndCaller.D20Query(D20DispatcherKey.QUE_Flatfooted))
            {
                return;
            }

            // check light weapon or one handed piercing
            if (!IsUsingLightOrOneHandedPiercing(evt.objHndCaller))
            {
                return;
            }

            var atkflags = dispIo.attackPacket.flags;
            atkflags |= D20CAF.DEFLECT_ARROWS;
            atkflags &= ~(D20CAF.HIT | D20CAF.CRITICAL);
            dispIo.attackPacket.flags = atkflags;
        }

        public static readonly ConditionSpec ClassCondition = TemplePlusClassConditions.Create(ClassSpec)
            .AddHandler(DispatcherType.GetAC, CannyDefenseAcBonus)
            .AddHandler(DispatcherType.GetAC, EnhancedMobility)
            .AddHandler(DispatcherType.GetAC, ElaborateParry)
            .AddHandler(DispatcherType.InitiativeMod, ImprovedReactionInitBonus)
            .AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_REFLEX, GraceReflexBonus)
            .AddHandler(DispatcherType.DealingDamage, PreciseStrikeDamageBonus)
            .AddHandler(DispatcherType.DeflectArrows, DuelistDeflectArrows)
            .Build();
    }
}
