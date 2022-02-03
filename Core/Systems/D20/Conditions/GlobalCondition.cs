using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Utils;
using OpenTemple.Core.Systems.RadialMenus;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.Feats;

namespace OpenTemple.Core.Systems.D20.Conditions
{
    public static class GlobalCondition
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        [TempleDllLocation(0x102e8088)]
        public static readonly ConditionSpec Global = ConditionSpec.Create("Global", 0)
            .AddHandler(DispatcherType.AbilityScoreLevel, StatBaseGetBonuses)
            .AddHandler(DispatcherType.StatBaseGet, StatBaseGetBonuses)
            .AddHandler(DispatcherType.GetAC, GlobalGetArmorClass)
            .AddHandler(DispatcherType.ToHitBonusBase, GlobalMonsterToHit)
            .AddHandler(DispatcherType.ToHitBonus2, GlobalToHitBonus)
            .AddHandler(DispatcherType.ToHitBonus2, GlobalWeaponProficiencyToHitPenalty)
            .AddHandler(DispatcherType.ToHitBonus2, GlobalShieldNonProficiencyPenalty)
            .AddHandler(DispatcherType.ToHitBonus2, GlobalArmorNonProficiencyPenalty)
            .AddHandler(DispatcherType.DealingDamage, GlobalOnDamage)
            .AddHandler(DispatcherType.GetCriticalHitRange, GetCritRangeFromWeapon)
            .AddHandler(DispatcherType.GetCriticalHitExtraDice, CriticalHitGetDice)
            .AddHandler(DispatcherType.InitiativeMod, GetDexterityBonusToInitiative)
            .AddSignalHandler(D20DispatcherKey.SIG_HP_Changed, GlobalHpChanged)
            .AddHandler(DispatcherType.SkillLevel, SkillLevelWithBonusFromStat)
            .AddSkillLevelHandler(SkillId.pick_pocket, SynergySkillBonusFromBluff)
            .AddSkillLevelHandler(SkillId.diplomacy, SkillLevelDiplomacySynergy)
            .AddSkillLevelHandler(SkillId.intimidate, SynergySkillBonusFromBluff)
            .AddSkillLevelHandler(SkillId.disable_device, ThievesToolsSkillPenalty)
            .AddSkillLevelHandler(SkillId.perform, BardicInstrumentSkillPenalty)
            .AddSkillLevelHandler(SkillId.open_lock, ThievesToolsSkillPenalty)
            .AddHandler(DispatcherType.SaveThrowLevel, SavingThrow_BasicBonus_Callback)
            .AddHandler(DispatcherType.RadialMenuEntry, RadialMenuGlobal)
            .AddHandler(DispatcherType.Tooltip, GlobalTooltipCallback)
            .SetQueryResult(D20DispatcherKey.QUE_AOOIncurs, true)
            .SetQueryResult(D20DispatcherKey.QUE_ActionTriggersAOO, true)
            .AddQueryHandler(D20DispatcherKey.QUE_SpellInterrupted, ArmorArcaneSpellFailure)
            .SetQueryResult(D20DispatcherKey.QUE_CanBeFlanked, true)
            .AddQueryHandler(D20DispatcherKey.QUE_WieldedTwoHanded, GlobalWieldedTwoHandedQuery)
            .SetQueryResult(D20DispatcherKey.QUE_EnterCombat, true)
            .AddHandler(DispatcherType.GetNumAttacksBase, GlobalGetNumberOfAttacksBase)
            .AddHandler(DispatcherType.GetCritterNaturalAttacksNum, GetNumNaturalAttacks)
            .AddHandler(DispatcherType.CurrentHP, globalCurHPCalc)
            .AddHandler(DispatcherType.MaxHP, globalMaxHPCalc)
            .AddHandler(DispatcherType.GetLevel, GlobalGetCharacterLevel)
            .AddSignalHandler(D20DispatcherKey.SIG_Inventory_Update, UpdateEncumbranceWithLightLoad)
            .AddSignalHandler(D20DispatcherKey.SIG_Update_Encumbrance, UpdateEncumbranceWithLightLoad)
            .AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Encumbered_Light, QueryIsEncumberedLight)
            .AddHandler(DispatcherType.RadialMenuEntry, DismissSpellRadialEntry)
            .SetQueryResult(D20DispatcherKey.QUE_Is_BreakFree_Possible, false)
            .SetQueryResult(D20DispatcherKey.QUE_ActionTriggersAOO, true)
            .Build();


        public static IReadOnlyList<ConditionSpec> Conditions { get; } = new List<ConditionSpec>
        {
            Global,
        };

        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x100eef70)]
        public static void BardicInstrumentSkillPenalty(in DispatcherCallbackArgs evt)
        {
            DispIoObjBonus dispIo;

            dispIo = evt.GetDispIoObjBonus();
            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_BardicInstrument))
            {
                dispIo.bonOut.AddBonus(-2, 1, 314);
            }
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100ece80)]
        public static void QueryIsEncumberedLight(in DispatcherCallbackArgs evt)
        {
            StatusEffects.EncumbranceQuery(in evt, 0, 0);
        }


        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x100eed20)]
        public static void SkillLevelWithBonusFromStat(in DispatcherCallbackArgs evt)
        {
            var skillId = evt.GetSkillIdFromDispatcherKey();
            var dispIo = evt.GetDispIoObjBonus();

            var skillBase = GameSystems.Skill.GetSkillRanks(evt.objHndCaller, skillId);
            dispIo.bonOut.AddBonus(skillBase, 1, 102);

            var skillStat = GameSystems.Skill.GetDecidingStat(skillId);
            var skillStatLevel = evt.objHndCaller.GetStat(skillStat);
            var abilityScoreMod = D20StatSystem.GetModifierForAbilityScore(skillStatLevel);
            dispIo.bonOut.AddBonus(abilityScoreMod, 2 + (int) skillStat, 103 + (int) skillStat);
        }

        [DispTypes(DispatcherType.ToHitBonusBase)]
        [TempleDllLocation(0x100ee1b0)]
        public static void GlobalMonsterToHit(in DispatcherCallbackArgs args)
        {
            var dispIo = args.GetDispIoAttackBonus();
            if (dispIo.attackPacket.dispKey < 10 && args.objHndCaller.IsNPC())
            {
                var hitDice = args.objHndCaller.GetInt32(obj_f.npc_hitdice_idx, 0);
                var bonus = 0;
                var category = GameSystems.Critter.GetCategory(args.objHndCaller);

                switch (category)
                {
                    case MonsterCategory.aberration:
                    case MonsterCategory.animal:
                    case MonsterCategory.beast:
                    case MonsterCategory.construct:
                    case MonsterCategory.elemental:
                    case MonsterCategory.giant:
                    case MonsterCategory.humanoid:
                    case MonsterCategory.ooze:
                    case MonsterCategory.plant:
                    case MonsterCategory.shapechanger:
                    case MonsterCategory.vermin:
                        bonus = 3 * hitDice / 4;
                        break;
                    case MonsterCategory.dragon:
                    case MonsterCategory.magical_beast:
                    case MonsterCategory.monstrous_humanoid:
                    case MonsterCategory.outsider:
                        bonus = hitDice;
                        break;
                    case MonsterCategory.fey:
                    case MonsterCategory.undead:
                        bonus = hitDice / 2;
                        break;
                    default:
                        break;
                }

                dispIo.bonlist.AddBonus(bonus, 0, 118);
            }
        }

        [DispTypes(DispatcherType.InitiativeMod)]
        [TempleDllLocation(0x100eebb0)]
        public static void GetDexterityBonusToInitiative(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoObjBonus();
            var dexMod = evt.objHndCaller.GetStat(Stat.dex_mod);
            dispIo.bonOut.AddBonus(dexMod, 3, 104);
        }

        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x100ee050)]
        [TemplePlusLocation("condition.cpp:402")]
        public static void GlobalGetArmorClass(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            ref var bonusList = ref dispIo.bonlist;
            bonusList.AddBonus(10, 1, 102); // Base value for armor

            var attackFlags = dispIo.attackPacket.flags;

            // Handle inherent natural armor (i.e. from polymorph or NPC base)

            if ((attackFlags & D20CAF.TOUCH_ATTACK) == 0)
            {
                var obj = evt.objHndCaller;
                var polyProtoId = GameSystems.D20.D20QueryInt(evt.objHndCaller, D20DispatcherKey.QUE_Polymorphed);
                if (polyProtoId != 0)
                {
                    obj = GameSystems.Proto.GetProtoById((ushort) polyProtoId);
                }

                if (evt.objHndCaller.IsNPC() || polyProtoId != 0)
                {
                    var npcAcBonus = obj.GetInt32(obj_f.npc_ac_bonus);
                    bonusList.AddBonus(npcAcBonus, 9, 123);
                }
            }

            // Bonus from size category
            var sizeCategory = evt.objHndCaller.GetStat(Stat.size);
            var sizeBonus = GameSystems.Critter.GetBonusFromSizeCategory((SizeCategory) sizeCategory);
            bonusList.AddBonus(sizeBonus, 0, 115);

            // Dexterity bonus
            var dexModifier = evt.objHndCaller.GetStat(Stat.dex_mod);
            bonusList.AddBonus(dexModifier, 3, 104);

            if ((attackFlags & D20CAF.TRAP) != 0)
            {
                var attackVictim = dispIo.attackPacket.victim;
                if (attackVictim != null && GameSystems.Feat.HasFeat(attackVictim, FeatId.UNCANNY_DODGE))
                {
                    bonusList.zeroBonusSetMeslineNum(165); // "dex bonus retained"
                    return;
                }

                bonusList.AddCap(8, 0, 153);
                bonusList.AddCap(3, 0, 153);
            }

            if ((attackFlags & D20CAF.COVER) != 0)
            {
                bonusList.AddBonus(4, 0, 309);
            }
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100ee280)]
        [TemplePlusLocation("condition.cpp:403")]
        public static void GlobalToHitBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            evt.objHndCaller.DispatchToHitBonusBase(ref dispIo);

            if (dispIo.attackPacket.dispKey >= 10 &&
                !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Polymorphed))
            {
                var bonus = GameSystems.Critter.GetNaturalAttackBonus(evt.objHndCaller,
                    dispIo.attackPacket.dispKey - 10);
                dispIo.bonlist.AddBonus(bonus, 1, 118);
            }

            var attackFlags = dispIo.attackPacket.flags;
            if ((attackFlags & D20CAF.RANGED) != 0)
            {
                // ranged attacks are dex based
                var dexMod = evt.objHndCaller.GetStat(Stat.dex_mod);
                dispIo.bonlist.AddBonus(dexMod, 3, 104);
            }
            else
            {
                // melee attacks are dex based
                var strMod = evt.objHndCaller.GetStat(Stat.str_mod);
                dispIo.bonlist.AddBonus(strMod, 2, 103);
            }

            var attackCode = dispIo.attackPacket.dispKey;
            switch (attackCode)
            {
                case 2:
                case 7:
                case 8:
                    // Second attack
                    dispIo.bonlist.AddBonus(-5, 24, 119);
                    break;
                case 3:
                case 9:
                    // Third attack
                    dispIo.bonlist.AddBonus(-10, 25, 120);
                    break;
                case 4:
                    // Third attack
                    dispIo.bonlist.AddBonus(-15, 25, 120);
                    break;
                default:
                    break;
            }

            switch (attackCode)
            {
                case 5:
                case 7:
                case 9:
                    // Dual Wield
                    dispIo.bonlist.AddBonus(-6, 27, 122);
                    break;
                case 6:
                case 8:
                    // Off Hand
                    dispIo.bonlist.AddBonus(-10, 26, 121);
                    break;
                default:
                    break;
            }

            if (attackCode >= 5 && attackCode <= 9)
            {
                var offHandWeapon = GameSystems.Item.ItemWornAt(evt.objHndCaller, EquipSlot.WeaponSecondary);
                if (offHandWeapon != null)
                {
                    // Wielding off-hand light weapon while dual wielding
                    if (GameSystems.Item.GetWieldType(dispIo.attackPacket.attacker, offHandWeapon) == 0)
                    {
                        dispIo.bonlist.AddBonus(2, 0, 167);
                    }
                }
            }

            // Helpless target
            if (dispIo.attackPacket.victim != null && GameSystems.D20.D20Query(dispIo.attackPacket.victim,
                                                       D20DispatcherKey.QUE_Helpless)
                                                   && !GameSystems.D20.D20Query(dispIo.attackPacket.victim,
                                                       D20DispatcherKey.QUE_Critter_Is_Stunned))
            {
                dispIo.bonlist.AddBonus(4, 30, 136);
            }

            // Flanking
            if (GameSystems.D20.Combat.IsFlankedBy(dispIo.attackPacket.victim, dispIo.attackPacket.attacker))
            {
                dispIo.bonlist.AddBonus(2, 0, 201);
                attackFlags |= D20CAF.FLANKED;
            }

            // Size Bonus
            var sizeCategory = evt.objHndCaller.GetStat(Stat.size);
            var sizeBonus = GameSystems.Critter.GetBonusFromSizeCategory((SizeCategory) sizeCategory);
            dispIo.bonlist.AddBonus(sizeBonus, 0, 115);

            if ((attackFlags & D20CAF.RANGED) != 0)
            {
                // Handle shooting a ranged weapon into melee
                if (dispIo.attackPacket.victim != null)
                {
                    var enemies = GameSystems.Combat.GetEnemiesCanMelee(dispIo.attackPacket.victim);
                    if (enemies.Count > 0
                        && (enemies.Count != 1 || enemies[0] != evt.objHndCaller)
                        && !GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.PRECISE_SHOT))
                    {
                        dispIo.bonlist.AddBonus(-4, 0, 150);
                    }
                }

                // Consider weapon range increments and the associated penalty
                if (dispIo.attackPacket.victim != null)
                {
                    var weapon = dispIo.attackPacket.GetWeaponUsed();
                    if (weapon != null)
                    {
                        var distance = evt.objHndCaller.DistanceToObjInFeet(dispIo.attackPacket.victim);
                        var rangeIncrements = (int) (distance / weapon.GetInt32(obj_f.weapon_range));
                        if (rangeIncrements > 0)
                        {
                            dispIo.bonlist.AddBonus(-2 * rangeIncrements, 0, 303);
                        }
                    }
                }
            }
        }

        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100ee760)]
        [TemplePlusLocation("condition.cpp:404")]
        public static void GlobalOnDamage(in DispatcherCallbackArgs evt)
        {
            var damageMesLine = 100;
            var weaponName = "";
            var dispIo = evt.GetDispIoDamage();
            var weaponUsed = dispIo.attackPacket.GetWeaponUsed();

            var obj = evt.objHndCaller;
            var polyProtoId = GameSystems.D20.D20QueryInt(evt.objHndCaller, D20DispatcherKey.QUE_Polymorphed);
            if (polyProtoId != 0)
            {
                obj = GameSystems.Proto.GetProtoById((ushort) polyProtoId);
            }

            var attackCode = dispIo.attackPacket.dispKey;

            DamageType attackDamageType;
            Dice attackDice;
            if (weaponUsed != null)
            {
                var diceIo = new DispIoAttackDice();
                diceIo.flags = dispIo.attackPacket.flags;
                diceIo.wielder = evt.objHndCaller;
                diceIo.weapon = weaponUsed;
                attackDice = evt.objHndCaller.DispatchGetAttackDice(diceIo);
                attackDamageType = diceIo.attackDamageType;
                var partyLeader = GameSystems.Party.GetConsciousLeader();
                weaponName = GameSystems.MapObject.GetDisplayName(weaponUsed, partyLeader);
            }
            else
            {
                if (attackCode >= 10)
                {
                    var v8 = attackCode - 10;
                    attackDice = GameSystems.Critter.GetCritterDamageDice(obj, v8);
                    attackDamageType = GameSystems.Critter.GetCritterAttackDamageType(obj, v8);
                    damageMesLine = 114 + (int) GameSystems.Critter.GetCritterAttackType(obj, v8);
                    var diceIo = new DispIoAttackDice();
                    diceIo.flags = dispIo.attackPacket.flags;
                    diceIo.weapon = null;
                    diceIo.wielder = evt.objHndCaller;
                    diceIo.dicePacked = attackDice;
                    diceIo.attackDamageType = attackDamageType;
                    attackDice = evt.objHndCaller.DispatchGetAttackDice(diceIo);
                }
                else
                {
                    int monkLvl = evt.objHndCaller.GetStat(Stat.level_monk);

                    attackDamageType = DamageType.Subdual;
                    if (GameSystems.Feat.HasFeatCountByClass(evt.objHndCaller, FeatId.IMPROVED_UNARMED_STRIKE) > 0)
                    {
                        //Note:  Bludgeoning is zero so this will be default if nothing answers the query
                        int nDamageType = GameSystems.D20.D20QueryPython(evt.objHndCaller, "Unarmed Damage Type");
                        attackDamageType = (DamageType) nDamageType;
                    }

                    damageMesLine = 113; // Unarmed

                    // TODO: This should be a D20 query for monk level!
                    var beltItem = GameSystems.Item.ItemWornAt(evt.objHndCaller, EquipSlot.Lockpicks);
                    if (beltItem != null)
                    {
                        if (beltItem.ProtoId == 12420)
                        {
                            monkLvl += 5;
                        }
                    }

                    var dudeSize = (SizeCategory) evt.objHndCaller.GetStat(Stat.size);
                    attackDice = GetUnarmedAttackDice(monkLvl, dudeSize);
                }
            }

            var damagePacket = dispIo.damage;
            damagePacket.AddDamageDice(attackDice, attackDamageType, damageMesLine, weaponName);

            // Add strength modifier to damage
            var strMod = evt.objHndCaller.GetStat(Stat.str_mod);
            var attackFlags = dispIo.attackPacket.flags;
            if ((attackFlags & D20CAF.RANGED) != 0 && (attackFlags & D20CAF.THROWN) == 0)
            {
                // Handle ranged / non-thrown weapons (those are handled like melee weapons)
                if (weaponUsed == null)
                {
                    return;
                }

                var weaponType = weaponUsed.GetWeaponType();
                if (weaponType == WeaponType.sling)
                {
                    damagePacket.AddDamageBonus(strMod, 2, 103);
                    return;
                }

                // Consider strength, but only if it's a malus.
                if ((weaponType == WeaponType.shortbow || weaponType == WeaponType.longbow) && strMod < 0)
                {
                    damagePacket.AddDamageBonus(strMod, 2, 103);
                }

                return;
            }

            if (attackCode == 6 || attackCode == 8)
            {
                if (strMod > 0)
                {
                    strMod /= 2;
                }
            }
            else if (GameSystems.D20.D20QueryWithObject(evt.objHndCaller, D20DispatcherKey.QUE_WieldedTwoHanded,
                         dispIo) != 0
                     && strMod > 0
                     && GameSystems.Item.GetWieldType(evt.objHndCaller, weaponUsed) != 0)
            {
                strMod += strMod / 2;
            }

            if (attackCode >= 10 && strMod > 0 && GameSystems.Critter.GetDamageIdx(obj, attackCode - 10) > 0)
            {
                strMod /= 2;
            }

            damagePacket.AddDamageBonus(strMod, 2, 103);
            return;
        }

        private static Dice GetUnarmedAttackDice(int monkLvl, SizeCategory dudeSize)
        {
            if (dudeSize < SizeCategory.Medium) // small monk
            {
                if (monkLvl <= 0)
                {
                    return Dice.D2;
                }
                else if (monkLvl < 4)
                {
                    return Dice.D4;
                }
                else if (monkLvl < 8)
                {
                    return Dice.D6;
                }
                else if (monkLvl < 12)
                {
                    return Dice.D8;
                }
                else if (monkLvl < 16)
                {
                    return Dice.D10;
                }
                else if (monkLvl < 20)
                {
                    return new Dice(2, 6);
                }
                else // 20 and above
                {
                    return new Dice(2, 8);
                }
            }
            else if (dudeSize > SizeCategory.Medium) // Large Monk
            {
                if (monkLvl <= 0)
                {
                    return Dice.D4;
                }
                else if (monkLvl < 4)
                {
                    return Dice.D8;
                }
                else if (monkLvl < 8)
                {
                    return new Dice(2, 6);
                }
                else if (monkLvl < 12)
                {
                    return new Dice(2, 8);
                }
                else if (monkLvl < 16)
                {
                    return new Dice(3, 6);
                }
                else if (monkLvl < 20)
                {
                    return new Dice(3, 8);
                }
                else
                {
                    return new Dice(4, 8);
                }
            }
            else // normal monk
            {
                if (monkLvl <= 0)
                {
                    return new Dice(1, 3);
                }
                else if (monkLvl < 4)
                {
                    return Dice.D6;
                }
                else if (monkLvl < 8)
                {
                    return Dice.D8;
                }
                else if (monkLvl < 12)
                {
                    return Dice.D10;
                }
                else if (monkLvl < 16)
                {
                    return new Dice(2, 6);
                }
                else if (monkLvl < 20)
                {
                    return new Dice(2, 8);
                }
                else
                {
                    return new Dice(2, 10);
                }
            }
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100ef060)]
        [TemplePlusLocation("condition.cpp:444")]
        public static void ArmorArcaneSpellFailure(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();

            var d20SpellData = (D20SpellData) dispIo.obj;

            var retVal = dispIo.return_val;
            var v29 = dispIo;
            if (retVal == 1)
            {
                return;
            }

            var spellEnum = d20SpellData.SpellEnum;
            var spellClassCode = d20SpellData.spellClassCode;
            var invIdx = d20SpellData.itemSpellData;
            var mmData = d20SpellData.metaMagicData;

            if ((spellClassCode & 0x80) == 0)
            {
                return;
            }

            var castingClass = (Stat) (spellClassCode & 0x7F);
            if (!d20SpellData.HasItem)
            {
                if ((GameSystems.Spell.GetSpellComponentRegardMetamagic(spellEnum, mmData) & SpellComponent.Somatic) != 0
                    // TODO: Improve check for arcane casting
                    && (castingClass == Stat.level_sorcerer || castingClass == Stat.level_wizard ||
                        castingClass == Stat.level_bard))
                {
                    var failChance = 0;
                    foreach (var (slot, item) in evt.objHndCaller.EnumerateEquipment())
                    {
                        if (item.type == ObjectType.armor)
                        {
                            // This accounts for bards being allowed to wear light armor without incurring
                            // the spell failure chance. Shields are excluded though.
                            if (slot != EquipSlot.Armor || castingClass != Stat.level_bard ||
                                !item.GetArmorFlags().IsLightArmorOrLess())
                            {
                                failChance += item.GetInt32(obj_f.armor_arcane_spell_failure);
                            }
                        }
                    }

                    if (failChance > 0)
                    {
                        var rollRes = Dice.D100.Roll();
                        if (rollRes >= failChance)
                        {
                            GameSystems.RollHistory.AddPercentageCheck(evt.objHndCaller, null, failChance, 59, rollRes,
                                62, 192);
                        }
                        else
                        {
                            GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x1D, evt.objHndCaller, null);
                            GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 57);
                            GameSystems.RollHistory.AddPercentageCheck(evt.objHndCaller, null, failChance, 59, rollRes,
                                57, 192);
                            v29.return_val = 1;
                        }
                    }
                }
            }
        }

        [DispTypes(DispatcherType.StatBaseGet, DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x100ee020)]
        public static void StatBaseGetBonuses(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoBonusList();
            var stat = evt.GetAttributeFromDispatcherKey();
            var statValue = GameSystems.Stat.ObjStatBaseGet(evt.objHndCaller, stat);
            dispIo.bonlist.AddBonus(statValue, 1, BonusMessages.InitialValue);
        }


        [DispTypes(DispatcherType.GetCriticalHitRange)]
        [TempleDllLocation(0x100eeb10)]
        public static void GetCritRangeFromWeapon(in DispatcherCallbackArgs evt)
        {
            var weapCritRange = 1;
            var dispIo = evt.GetDispIoAttackBonus();
            var weapon = dispIo.attackPacket.GetWeaponUsed();
            if (weapon != null)
            {
                weapCritRange = weapon.GetInt32(obj_f.weapon_crit_range);
            }

            dispIo.bonlist.AddBonus(weapCritRange, 1, BonusMessages.InitialValue);
        }


        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100ef2a0)]
        public static void SavingThrow_BasicBonus_Callback(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            if (evt.dispKey == D20DispatcherKey.SAVE_FORTITUDE)
            {
                var conMod = evt.objHndCaller.GetStat(Stat.con_mod);
                dispIo.bonlist.AddBonus(conMod, 0, 105);
            }
            else if (evt.dispKey == D20DispatcherKey.SAVE_REFLEX)
            {
                var conMod = evt.objHndCaller.GetStat(Stat.dex_mod);
                dispIo.bonlist.AddBonus(conMod, 0, 104);
            }
            else if (evt.dispKey == D20DispatcherKey.SAVE_WILL)
            {
                var conMod = evt.objHndCaller.GetStat(Stat.wis_mod);
                dispIo.bonlist.AddBonus(conMod, 0, 107);
            }
            else
            {
                Logger.Info("d20_mods_global.c / _calc_base_save_bonus(): bad saving throw new type parameter");
            }
        }


        [DispTypes(DispatcherType.Tooltip)]
        [TempleDllLocation(0x100eefe0)]
        public static void GlobalTooltipCallback(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoTooltip();
            if (GameSystems.Critter.IsDeadNullDestroyed(evt.objHndCaller))
            {
                dispIo.Append(GameSystems.D20.Combat.GetCombatMesLine(30));
            }
        }

        [DispTypes(DispatcherType.CurrentHP)]
        [TempleDllLocation(0x100ef360)]
        public static void globalCurHPCalc(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoBonusList();
            var maxHp = evt.objHndCaller.GetStat(Stat.hp_max);
            var damageTaken = evt.objHndCaller.GetInt32(obj_f.hp_damage);
            dispIo.bonlist.AddBonus(maxHp - damageTaken, 1, BonusMessages.InitialValue);
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100ef4b0)]
        [TemplePlusLocation("condition.cpp:461")]
        public static void GlobalWieldedTwoHandedQuery(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            var dispIoDamage = (DispIoDamage) dispIo.obj;
            var attackPacket = dispIoDamage.attackPacket;
            var weapon = attackPacket.GetWeaponUsed();
            var offhandWeapon = GameSystems.Item.ItemWornAt(evt.objHndCaller, EquipSlot.WeaponSecondary);
            var shield = GameSystems.Item.ItemWornAt(evt.objHndCaller, EquipSlot.Shield);
            dispIo.return_val = (weapon != null && offhandWeapon == null && shield == null) ? 1 : 0;
            if (weapon != null)
            {
                if (weapon.GetWeaponType() == WeaponType.rapier)
                {
                    dispIo.return_val = 0;
                }
            }
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100ece10)]
        public static void UpdateEncumbranceWithLightLoad(in DispatcherCallbackArgs evt)
        {
            // Calls the encumbrance update if we don't have one of the actual encumrance conditions that
            // can take care of it.
            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_Encumbered_Medium)
                && !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_Encumbered_Heavy)
                && !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_Encumbered_Overburdened))
            {
                StatusEffects.UpdateEncumbrance(in evt);
            }
        }


        [DispTypes(DispatcherType.MaxHP)]
        [TempleDllLocation(0x100ef3a0)]
        public static void globalMaxHPCalc(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoBonusList();
            var hpMax = evt.objHndCaller.GetBaseStat(Stat.hp_max);
            if (evt.objHndCaller.IsPC() || evt.objHndCaller.IsNPC())
            {
                GameObject critter = evt.objHndCaller;
                if (!GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_Critter_Has_No_Con_Score))
                {
                    var conMod = critter.GetStat(Stat.con_mod);
                    hpMax += conMod * critter.GetStat(Stat.level);

                    if (critter.IsNPC())
                    {
                        hpMax += critter.GetInt32(obj_f.npc_hitdice_idx, 0) * conMod;
                    }
                }

                var level = critter.GetStat(Stat.level);
                if (critter.IsNPC())
                {
                    level += critter.GetInt32Array(obj_f.npc_hitdice_idx).Count;
                }

                if (hpMax < level)
                {
                    hpMax = level;
                }
            }

            dispIo.bonlist.AddBonus(hpMax, 1, 102);
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100ee610)]
        public static void GlobalArmorNonProficiencyPenalty(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var critter = evt.objHndCaller;
            var armor = GameSystems.Item.ItemWornAt(critter, EquipSlot.Armor);
            if (armor != null)
            {
                var bonValue = GameSystems.D20.GetArmorSkillCheckPenalty(armor);

                if (!GameSystems.Feat.IsProficientWithArmor(critter, armor) && !critter.IsNPC())
                {
                    dispIo.bonlist.AddBonus(bonValue, 0, 145);
                }
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100eebf0)]
        public static void GlobalHpChanged(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();

            var critter = evt.objHndCaller;
            var hpCur = critter.GetStat(Stat.hp_current);
            var subdualDam = critter.GetInt32(obj_f.critter_subdual_damage);

            if (hpCur <= -10)
            {
                GameSystems.D20.Combat.Kill(critter, null);
                return;
            }

            if (hpCur < 0)
            {
                if (!GameSystems.Feat.HasFeat(critter, FeatId.DIEHARD))
                {
                    if (!GameSystems.Critter.IsDeadOrUnconscious(critter))
                    {
                        GameSystems.Anim.PushDying(critter);
                    }

                    var hpChange = dispIo.data2;
                    if (hpChange <= 0)
                    {
                        if (hpChange < 0)
                        {
                            critter.AddCondition(StatusEffects.Dying);
                            return;
                        }
                    }

                    critter.AddCondition(StatusEffects.Unconscious);
                    return;
                }

                critter.AddCondition(StatusEffects.Disabled);
                return;
            }

            if (hpCur == 0)
            {
                critter.AddCondition(StatusEffects.Disabled);
                return;
            }

            if (subdualDam < hpCur)
            {
                return;
            }

            if (!GameSystems.Critter.IsDeadOrUnconscious(critter))
            {
                GameSystems.Anim.PushDying(critter);
            }

            critter.AddCondition(StatusEffects.Unconscious);
        }

        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x100eeee0)]
        public static void SynergySkillBonusFromBluff(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoObjBonus();
            if (GameSystems.Skill.GetSkillRanks(evt.objHndCaller, SkillId.bluff) >= 5)
            {
                dispIo.bonOut.AddBonus(2, 32, BonusMessages.BluffSynergyBonus);
            }
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100ee6a0)]
        public static void GlobalShieldNonProficiencyPenalty(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var shield = GameSystems.Item.ItemWornAt(evt.objHndCaller, EquipSlot.Shield);
            if (shield == null)
            {
                shield = GameSystems.Item.ItemWornAt(evt.objHndCaller, EquipSlot.WeaponSecondary);
            }

            if (shield != null && shield.type == ObjectType.armor)
            {
                var bonValue = GameSystems.D20.GetArmorSkillCheckPenalty(shield);
                if (!GameSystems.Feat.IsProficientWithArmor(evt.objHndCaller, shield) && !evt.objHndCaller.IsNPC())
                {
                    dispIo.bonlist.AddBonus(bonValue, 0, 146);
                }
            }
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100ee590)]
        public static void GlobalWeaponProficiencyToHitPenalty(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var weapon = dispIo.attackPacket.GetWeaponUsed();
            if (weapon != null)
            {
                var weapType = weapon.GetWeaponType();
                if (!GameSystems.Feat.IsProficientWithWeaponType(evt.objHndCaller, weapType) &&
                    !evt.objHndCaller.IsNPC())
                {
                    dispIo.bonlist.AddBonus(-4, 37, 138);
                }
            }
        }

        [DispTypes(DispatcherType.GetLevel)]
        [TempleDllLocation(0x100eed90)]
        public static void GlobalGetCharacterLevel(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoObjBonus();
            var stat = evt.GetClassFromDispatcherKey();
            var lvl = evt.objHndCaller.GetStat(stat);
            dispIo.bonOut.AddBonus(lvl, 1, BonusMessages.InitialValue);
        }

        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x100eee70)]
        [TemplePlusLocation("ability_fixes.cpp:79")]
        public static void SkillLevelDiplomacySynergy(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoObjBonus();
            if (GameSystems.Skill.GetSkillRanks(evt.objHndCaller, SkillId.bluff) >= 5)
            {
                dispIo.bonOut.AddBonus(2, 32, 140);
            }

            if (GameSystems.Skill.GetSkillRanks(evt.objHndCaller, SkillId.sense_motive) >= 5)
            {
                dispIo.bonOut.AddBonus(2, 32, 302);
            }
        }

        [DispTypes(DispatcherType.GetCriticalHitExtraDice)]
        [TempleDllLocation(0x100eeb60)]
        public static void CriticalHitGetDice(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();

            var diceCount = 1;
            var weapon = dispIo.attackPacket.GetWeaponUsed();
            if (weapon != null)
            {
                diceCount = weapon.GetInt32(obj_f.weapon_crit_hit_chart) - 1;
            }

            dispIo.bonlist.AddBonus(diceCount, 1, 102);
        }


        [DispTypes(DispatcherType.GetNumAttacksBase)]
        [TempleDllLocation(0x100efef0)]
        public static void GlobalGetNumberOfAttacksBase(in DispatcherCallbackArgs args)
        {
            var dispIo = args.GetDispIoD20ActionTurnBased();
            var babPastFive = args.objHndCaller.DispatchToHitBonusBase() - 5;
            if (babPastFive <= 0)
            {
                dispIo.returnVal = (ActionErrorCode) 1;
            }
            else
            {
                dispIo.returnVal = (ActionErrorCode) ((babPastFive - 1) / 5 + 2);
            }
        }


        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x100eef20)]
        public static void ThievesToolsSkillPenalty(in DispatcherCallbackArgs evt)
        {
            DispIoObjBonus dispIo;

            dispIo = evt.GetDispIoObjBonus();
            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Has_Thieves_Tools))
            {
                dispIo.bonOut.AddBonus(-2, 1, 314);
            }
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100eedd0)]
        [TemplePlusLocation("condition.cpp:3583")]
        public static void DismissSpellRadialEntry(in DispatcherCallbackArgs evt)
        {
            if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Can_Dismiss_Spells))
            {
                var spellId = (int) GameSystems.D20.D20QueryReturnData(evt.objHndCaller,
                    D20DispatcherKey.QUE_Critter_Can_Dismiss_Spells);
                var radMenuEntry = RadialMenuEntry.CreateAction(5101, D20ActionType.DISMISS_SPELLS,
                    spellId, "TAG_DISMISS_SPELL");
                GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry, RadialMenuStandardNode.Spells);
            }
        }

        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100eefc0)]
        public static void RadialMenuGlobal(in DispatcherCallbackArgs evt)
        {
            GameSystems.D20.RadialMenu.BuildStandardRadialMenu(evt.objHndCaller);
            AddThrownWeapons(evt.objHndCaller);
        }

        [TempleDllLocation(0x100ff020)]
        private static void AddThrownWeapons(GameObject critter)
        {
            var primaryWeaponProto = 0;

            void AddMenuEntry(GameObject thrownItem, D20ActionType actionType, bool isSecondaryWeapon = false)
            {
                var itemName = GameSystems.MapObject.GetDisplayName(thrownItem, critter);
                var meslineValue = GameSystems.D20.Combat.GetCombatMesLine(5088);

                var text = $"{meslineValue} {itemName}";
                var radMenuEntry = RadialMenuEntry.CreateAction(text, actionType, 0, "TAG_THROW_WEAPON");
                if (isSecondaryWeapon)
                {
                    radMenuEntry.d20Caf |= D20CAF.SECONDARY_WEAPON;
                }

                GameSystems.D20.RadialMenu.AddToStandardNode(critter, ref radMenuEntry, RadialMenuStandardNode.Offense);
            }

            var weapon = GameSystems.Item.ItemWornAt(critter, EquipSlot.WeaponPrimary);
            var secondaryWeapon = GameSystems.Item.ItemWornAt(critter, EquipSlot.WeaponSecondary);
            if (weapon != null)
            {
                primaryWeaponProto = weapon.ProtoId;
                var ammoType = weapon.GetWeaponAmmoType();
                if (ammoType.IsGrenade())
                {
                    AddMenuEntry(weapon, D20ActionType.THROW_GRENADE);
                }
                else if (ammoType.IsThrown())
                {
                    AddMenuEntry(weapon, D20ActionType.THROW);
                }
            }

            if (secondaryWeapon != null && secondaryWeapon.type != ObjectType.armor)
            {
                if (primaryWeaponProto != secondaryWeapon.ProtoId)
                {
                    var ammoType = secondaryWeapon.GetWeaponAmmoType();
                    if (ammoType.IsThrown())
                    {
                        // TODO: Why is it not checking again for grenades on the secondary weapon???
                        AddMenuEntry(secondaryWeapon, D20ActionType.THROW, true);
                    }
                }
            }
        }

        [DispTypes(DispatcherType.GetCritterNaturalAttacksNum)]
        [TempleDllLocation(0x100ef330)]
        public static void GetNumNaturalAttacks(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            dispIo.returnVal = (ActionErrorCode) GameSystems.Critter.GetCritterNaturalAttackCount(evt.objHndCaller);
        }
    }
}