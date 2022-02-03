using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus;

[AutoRegister]
public class Duergar
{
    public const RaceId Id = RaceId.dwarf + (3 << 5);

    public static readonly RaceSpec RaceSpec = new RaceSpec(Id, RaceBase.dwarf, Subrace.duergar)
    {
        effectiveLevel = 1,
        helpTopic = "TAG_DUERGAR",
        flags = 0,
        conditionName = "Duergar",
        heightMale = (45, 53),
        heightFemale = (43, 51),
        weightMale = (148, 178),
        weightFemale = (104, 134),
        statModifiers = {(Stat.constitution, 2), (Stat.charisma, -4)},
        ProtoId = 13020,
        materialOffset = 6, // offset into rules/material_ext.mes file
        spellLikeAbilities =
        {
            {
                new SpellStoreData(WellKnownSpells.Invisibility, 2,
                    SpellSystem.GetSpellClass(DomainId.Special)),
                1
            },
            {new SpellStoreData(WellKnownSpells.Enlarge, 1, SpellSystem.GetSpellClass(DomainId.Special)), 1},
        }
    };

    // note: dwarven move speed with heavy armor or when medium/heavy encumbered is already handled in Encumbered Medium, Encumbered Heavy condition callbacks
    public static readonly ConditionSpec Condition = ConditionSpec.Create(RaceSpec.conditionName)
        .AddAbilityModifierHooks(RaceSpec)
        .AddSaveBonusVsEffectType(D20SavingThrowFlag.SPELL_LIKE_EFFECT, 2)
        .AddSkillBonuses(
            (SkillId.listen, 1),
            (SkillId.spot, 1),
            (SkillId.move_silently, 4)
        )
        .AddBaseMoveSpeed(20)
        .AddPoisonImmunity()
        .AddFavoredClassHook(Stat.level_fighter)
        .AddHandler(DispatcherType.SkillLevel, D20DispatcherKey.SKILL_APPRAISE, OnGetAppraiseSkill)
        .AddHandler(DispatcherType.GetMoveSpeed,
            OnGetMoveSpeedSetLowerLimit) // paralysis immunity (affects normal Paralysis condition)
        .AddHandler(DispatcherType.ConditionAddPre,
            ConditionImmunityOnPreAdd) // paralysis immunity query - used in spell paralysis effects such as Hold Person/Monster
        .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Paralysis, true) // phantasm immunity
        .AddHandler(DispatcherType.SpellImmunityCheck, PhantasmImmunity)
        .AddHandler(DispatcherType.ToHitBonus2, OnGetToHitBonusVsOrcsAndGoblins)
        .AddHandler(DispatcherType.GetAC, OnGetArmorClassBonusVsGiants)
        .AddHandler(DispatcherType.AbilityCheckModifier, OnAbilityModCheckStabilityBonus)
        .AddHandler(DispatcherType.BaseCasterLevelMod, CasterLevelRacialSpell)
        .Build();

    private static readonly int BONUS_MES_RACIAL_BONUS = 139;
    private static readonly int BONUS_MES_STABILITY = 317;

    public static void ConditionImmunityOnPreAdd(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoCondStruct();
        if (dispIo.condStruct == StatusEffects.Paralyzed)
        {
            dispIo.outputFlag = false;
            evt.objHndCaller.FloatLine("Paralysis Immunity", TextFloaterColor.Red);
        }
    }

    public static void PhantasmImmunity(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoImmunity();
        if (IsPhantasmSubschool(dispIo.spellPkt))
        {
            dispIo.returnVal = 1;
        }
    }

    private static bool IsPhantasmSubschool(SpellPacketBody spellPacket)
    {
        // Check if phantasm subschool
        var spellEnum = spellPacket.spellEnum;
        if (spellEnum == 0)
        {
            return false;
        }

        var spellEntry = GameSystems.Spell.GetSpellEntry(spellEnum);
        return spellEntry.spellSchoolEnum == SchoolOfMagic.Illusion
               && spellEntry.spellSubSchoolEnum != SubschoolOfMagic.Phantasm;
    }

    public static void OnGetMoveSpeedSetLowerLimit(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoMoveSpeed();
        // this sets the lower limit for dwarf move speed at 20, unless someone has already set it (e.g. by web/entangle)
        if ((dispIo.bonlist.bonFlags & 2) != 0)
        {
            return;
        }

        var moveSpeedCapValue = 20;
        var capFlags = 2; // set lower limit
        var capType = 0; // operate on all bonus types
        var bonusMesline = BONUS_MES_RACIAL_BONUS; // racial ability
        dispIo.bonlist.SetOverallCap(capFlags, moveSpeedCapValue, capType, bonusMesline);
    }

    public static void OnGetAppraiseSkill(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoObjBonus();
        // adds appraise bonus to metal or rock items
        var item = dispIo.obj;
        if (item == null)
        {
            return;
        }

        var itemMaterial = item.GetMaterial();
        if ((itemMaterial == Material.stone || itemMaterial == Material.metal))
        {
            dispIo.bonlist.AddBonus(2, 0, BONUS_MES_RACIAL_BONUS);
        }
    }

    public static void OnGetToHitBonusVsOrcsAndGoblins(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoAttackBonus();
        var target = dispIo.attackPacket.victim;
        if (target == null)
        {
            return;
        }

        if (target.IsMonsterSubtype(MonsterSubtype.half_orc) || target.IsMonsterSubtype(MonsterSubtype.goblinoid) ||
            target.IsMonsterSubtype(MonsterSubtype.orc))
        {
            dispIo.bonlist.AddBonus(1, 0, BONUS_MES_RACIAL_BONUS);
        }
    }

    public static void OnGetArmorClassBonusVsGiants(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoAttackBonus();
        var attacker = dispIo.attackPacket.attacker /*AttackPacket*/;
        if (attacker == null)
        {
            return;
        }

        if (attacker.IsMonsterCategory(MonsterCategory.giant))
        {
            dispIo.bonlist.AddBonus(4, 8, BONUS_MES_RACIAL_BONUS);
        }
    }

    public static void OnAbilityModCheckStabilityBonus(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoObjBonus();
        var flags = dispIo.flags;
        if (((flags & SkillCheckFlags.UnderDuress)) != 0 && ((flags & SkillCheckFlags.Unk2)) != 0) // defender bonus
        {
            dispIo.bonlist.AddBonus(4, 22, BONUS_MES_STABILITY);
        }
    }

    public static void CasterLevelRacialSpell(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20Query();

        // Set caster level for racial spells to 2x character level (minimum 3)
        var spellPacket = (SpellPacketBody) dispIo.obj;
        if (spellPacket.spellClass != SpellSystem.GetSpellClass(DomainId.Special))
        {
            return;
        }

        var casterLevel = Math.Max(3, 2 * evt.objHndCaller.GetStat(Stat.level));
        if (dispIo.return_val < casterLevel)
        {
            dispIo.return_val = casterLevel;
        }
    }
}