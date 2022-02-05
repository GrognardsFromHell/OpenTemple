using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
using OpenTemple.Core.Logging;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.D20.Classes.Prereq;
using OpenTemple.Core.Systems.RadialMenus;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Ui.InGameSelect;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus;

[AutoRegister]
public class ArcaneArcher
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    public static readonly Stat ClassId = Stat.level_arcane_archer;

    public const string EnhanceArrowName = "Enhance Arrow";
    public static readonly FeatId EnhanceArrowId = (FeatId) ElfHash.Hash(EnhanceArrowName);
    public const string ImbueArrowName = "Imbue Arrow";
    public static readonly FeatId ImbueArrowId = (FeatId) ElfHash.Hash(ImbueArrowName);
    public const string SeekerArrowName = "Seeker Arrow";
    public static readonly FeatId SeekerArrowId = (FeatId) ElfHash.Hash(SeekerArrowName);
    public const string PhaseArrowName = "Phase Arrow";
    public static readonly FeatId PhaseArrowId = (FeatId) ElfHash.Hash(PhaseArrowName);
    public const string HailOfArrowsName = "Hail of Arrows";
    public static readonly FeatId HailOfArrowsId = (FeatId) ElfHash.Hash(HailOfArrowsName);
    public const string ArrowOfDeathName = "Arrow of Death";
    public static readonly FeatId ArrowOfDeathId = (FeatId) ElfHash.Hash(ArrowOfDeathName);

    public static readonly D20ClassSpec ClassSpec = new("arcane_archer")
    {
        classEnum = ClassId,
        helpTopic = "TAG_ARCANE_ARCHERS",
        conditionName = "Arcane Archer",
        category = "Core 3.5 Ed Prestige Classes",
        flags = ClassDefinitionFlag.CDF_CoreClass,
        BaseAttackBonusProgression = BaseAttackProgressionType.Martial,
        hitDice = 8,
        FortitudeSaveProgression = SavingThrowProgressionType.HIGH,
        ReflexSaveProgression = SavingThrowProgressionType.HIGH,
        WillSaveProgression = SavingThrowProgressionType.LOW,
        skillPts = 4,
        hasArmoredArcaneCasterFeature = false,
        classSkills = new HashSet<SkillId>
        {
            SkillId.hide,
            SkillId.listen,
            SkillId.move_silently,
            SkillId.spot,
            SkillId.wilderness_lore,
            SkillId.alchemy,
            SkillId.craft,
            SkillId.ride,
            SkillId.use_rope,
        }.ToImmutableHashSet(),
        classFeats = new Dictionary<FeatId, int>
        {
            {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
            {FeatId.ARMOR_PROFICIENCY_MEDIUM, 1},
            {FeatId.SHIELD_PROFICIENCY, 1},
            {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1},
            {FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL, 1},
            {EnhanceArrowId, 1},
            {ImbueArrowId, 2},
            {SeekerArrowId, 4},
            {PhaseArrowId, 6},
            {HailOfArrowsId, 8},
            {ArrowOfDeathId, 10}
        }.ToImmutableDictionary(),
        Requirements =
        {
            ClassPrereqs.Race(RaceId.elf, RaceId.half_elf),
            ClassPrereqs.BaseAttackBonus(6),
            ClassPrereqs.Feat(FeatId.POINT_BLANK_SHOT),
            ClassPrereqs.Feat(FeatId.PRECISE_SHOT),
            ClassPrereqs.Feat(FeatId.WEAPON_FOCUS_LONGBOW, FeatId.WEAPON_FOCUS_SHORTBOW),
            ClassPrereqs.ArcaneSpellCaster()
        }
    };

    public static readonly ConditionSpec ClassCondition = TemplePlusClassConditions.Create(ClassSpec)
        .AddHandler(DispatcherType.DealingDamage, EnhanceArrowDamage)
        .AddHandler(DispatcherType.ToHitBonus2, EnhanceArrowToHit)
        .Build();

    private static readonly D20DispatcherKey imbueArrowEnum = (D20DispatcherKey) 2000;
    private static readonly D20DispatcherKey seekerArrowEnum = (D20DispatcherKey) 2001;
    private static readonly D20DispatcherKey phaseArrowEnum = (D20DispatcherKey) 2002;
    private static readonly D20DispatcherKey hailOfArrowsEnum = (D20DispatcherKey) 2003;
    private static readonly D20DispatcherKey deathArrowEnum = (D20DispatcherKey) 2004;

    #region Enhance Arrow

    public static void EnhanceArrowDamage(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoDamage();
        var classLvl = evt.objHndCaller.GetStat(ClassId);
        var bon_lvl = 1 + (classLvl - 1) / 2;
        if (dispIo.attackPacket.IsRangedWeaponAttack())
        {
            dispIo.damage.bonuses.AddBonus(bon_lvl, 12, 147, "Enhance Arrow");
        }
    }

    public static void EnhanceArrowToHit(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoAttackBonus();
        var classLvl = evt.objHndCaller.GetStat(ClassId);
        var bon_lvl = 1 + (classLvl - 1) / 2;
        if (dispIo.attackPacket.IsRangedWeaponAttack())
        {
            dispIo.bonlist.AddBonus(bon_lvl, 12, 147, "Enhance Arrow");
        }
    }

    #endregion

    #region Imbue Arrow feat

    public static bool ImbueOk(GameObject attachee, SpellStoreData spData)
    {
        if (!GameSystems.Spell.spellCanCast(attachee, spData.spellEnum, spData.classCode, spData.spellLevel))
        {
            return false;
        }

        if (!GameSystems.Spell.TryGetSpellEntry(spData.spellEnum, out var entry))
        {
            return false;
        }

        return entry.IsBaseModeTarget(UiPickerType.Area) || entry.IsBaseModeTarget(UiPickerType.Cone);
    }

    private static bool IsNaturallyCast(SpellStoreData data)
    {
        if (GameSystems.Spell.IsDomainSpell(data.classCode))
        {
            return false;
        }

        var castingClass = GameSystems.Spell.GetCastingClass(data.classCode);
        return D20ClassSystem.IsNaturalCastingClass(castingClass);
    }

    public static void ImbueArrowRadial(in DispatcherCallbackArgs evt)
    {
        if (!GameSystems.Critter.IsWieldingRangedWeapon(evt.objHndCaller))
        {
            return;
        }

        var known_spells = evt.objHndCaller.GetSpellArray(obj_f.critter_spells_known_idx);
        var radial_parent = RadialMenuEntry.CreateParent("Imbue Arrow");

        var imb_arrow_id = radial_parent.AddAsChild(evt.objHndCaller, RadialMenuStandardNode.Class);
        // create the spell level nodes
        var spell_level_ids = new List<int>();
        for (var p = 0; p < 10; p++)
        {
            var spell_level_node = RadialMenuEntry.CreateParent(p.ToString());
            spell_level_ids.Add(spell_level_node.AddAsChild(evt.objHndCaller, imb_arrow_id));
        }

        foreach (var knSp in known_spells)
        {
            if (IsNaturallyCast(knSp) && ImbueOk(evt.objHndCaller, knSp))
            {
                var spell_node =
                    RadialMenuEntry.CreatePythonAction(knSp, D20ActionType.PYTHON_ACTION, imbueArrowEnum, 0);
                spell_node.AddAsChild(evt.objHndCaller, spell_level_ids[knSp.spellLevel]);
            }
        }

        var mem_spells = evt.objHndCaller.GetSpellArray(obj_f.critter_spells_memorized_idx);
        foreach (var memSp in mem_spells)
        {
            if (!memSp.spellStoreState.usedUp && ImbueOk(evt.objHndCaller, memSp))
            {
                var spell_node =
                    RadialMenuEntry.CreatePythonAction(memSp, D20ActionType.PYTHON_ACTION, imbueArrowEnum, 0);
                spell_node.AddAsChild(evt.objHndCaller, spell_level_ids[memSp.spellLevel]);
            }
        }
    }

    public static void ImbueArrowPerform(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20ActionTurnBased();

        var cur_seq = GameSystems.D20.Actions.CurrentSequence;

        var tgtLoc = cur_seq.spellPktBody.aoeCenter;
        var arrowTgt = dispIo.action.d20ATarget;
        var min_dist = 10000.0f;
        if (arrowTgt == evt.objHndCaller) // e.g. when player brings up radial menu by right clicking the character
        {
            arrowTgt = null;
        }

        if (arrowTgt == null)
        {
            foreach (var tgt in cur_seq.spellPktBody.Targets)
            {
                var dist_to_tgt = tgt.Object.DistanceTo(tgtLoc.location, tgtLoc.off_x, tgtLoc.off_y);
                // print "Distance to tgt " + str(tgt) + ": " + str(dist_to_tgt)
                if (arrowTgt == null || dist_to_tgt < min_dist)
                {
                    if (dist_to_tgt < 1 || !tgt.Object.IsFriendly(evt.objHndCaller))
                    {
                        min_dist = dist_to_tgt;
                        arrowTgt = tgt.Object;
                    }
                }
            }
        }

        dispIo.action.d20ATarget = arrowTgt;
        // print "Imbue arrow: target is " + str(arrowTgt)
        // roll to hit
        dispIo.action.d20Caf |= D20CAF.RANGED;
        GameSystems.D20.Combat.ToHitProcessing(dispIo.action);
        var isCritical = (dispIo.action.d20Caf & D20CAF.CRITICAL) != D20CAF.NONE;

        // print "Imbue arrow: setting new spell ID"
        dispIo.action.FilterSpellTargets(cur_seq.spellPktBody);
        var new_spell_id = GameSystems.Spell.GetNewSpellId();
        GameSystems.Spell.RegisterSpell(cur_seq.spellPktBody, new_spell_id);
        dispIo.action.spellId = new_spell_id;
        cur_seq.castSpellAction.spellId = new_spell_id;
        cur_seq.spellPktBody.Debit();
        // print "Imbue arrow: spell ID registered " + str(new_spell_id)
        // provoke hostility if applicable
        foreach (var tgt in cur_seq.spellPktBody.Targets)
        {
            if (GameSystems.Spell.IsSpellHarmful(cur_seq.spellPktBody.spellEnum, evt.objHndCaller, tgt.Object))
            {
                evt.objHndCaller.Attack(tgt.Object);
            }
        }

        evt.objHndCaller.D20SendSignal(D20DispatcherKey.SIG_Spell_Cast, new_spell_id);
        foreach (var tgt in cur_seq.spellPktBody.Targets)
        {
            GameSystems.D20.D20SendSignal(tgt.Object, D20DispatcherKey.SIG_Spell_Cast, new_spell_id);
        }

        if (GameSystems.Anim.PushAttack(evt.objHndCaller, arrowTgt, -1, RandomRange(0, 2), isCritical, false))
        {
            var new_anim_id = GameSystems.Anim.GetActionAnimId(evt.objHndCaller);
            dispIo.action.d20Caf |= D20CAF.NEED_ANIM_COMPLETED;
            dispIo.action.animID = new_anim_id;
        }
    }

    public static void ImbueArrowActionFrame(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20ActionTurnBased();
        // print "Imbue Arrow Action Frame"
        var cur_seq = GameSystems.D20.Actions.CurrentSequence;
        var tgt = dispIo.action.d20ATarget;
        if (tgt != null)
        {
            var projectile = FireArrowProjectile(dispIo.action, tgt.GetLocationFull());
            if (projectile != null)
            {
                cur_seq.spellPktBody.spellId = dispIo.action.spellId;
                cur_seq.spellPktBody.AddProjectile(projectile);
            }
        }
        else
        {
            GameSystems.Script.Spells.SpellTrigger(dispIo.action.spellId, SpellEvent.SpellEffect);
        }
    }

    [FeatCondition(ImbueArrowName)]
    public static readonly ConditionSpec ImbueArrowFeat = ConditionSpec.Create("Imbue Arrow Feat", 3)
        .SetUnique()
        .AddHandler(DispatcherType.RadialMenuEntry, ImbueArrowRadial)
        .AddHandler(DispatcherType.PythonActionPerform, imbueArrowEnum, ImbueArrowPerform)
        .AddHandler(DispatcherType.PythonActionFrame, imbueArrowEnum, ImbueArrowActionFrame) // Seeker Arrow feat
        .Build();

    #endregion

    #region Seeker Arrow

    public static void SeekerArrowRadial(in DispatcherCallbackArgs evt)
    {
        if (!GameSystems.Critter.IsWieldingRangedWeapon(evt.objHndCaller))
        {
            return;
        }

        if (evt.GetConditionArg1() != 0)
        {
            Logger.Info("{0}", evt.GetConditionArg1().ToString());
            return;
        }

        var radial_action = RadialMenuEntry.CreatePythonAction("Seeker Arrow", D20ActionType.PYTHON_ACTION,
            seekerArrowEnum, 0, "TAG_INTERFACE_HELP");
        radial_action.AddAsChild(evt.objHndCaller, RadialMenuStandardNode.Class);
    }

    public static void SeekerArrowPerform(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20ActionTurnBased();
        var cur_seq = GameSystems.D20.Actions.CurrentSequence;
        var arrowTgt = dispIo.action.d20ATarget;
        var min_dist = 10000;
        // print "Seeker arrow: target is " + str(arrowTgt)
        // roll to hit
        // cover bonus won't be applied because it doesn't appear in the ActionCheck callback
        // TODO: apply a negator condition for the concealment chances
        dispIo.action.d20Caf |= D20CAF.RANGED;
        GameSystems.D20.Combat.ToHitProcessing(dispIo.action);
        var isCritical = (dispIo.action.d20Caf & D20CAF.CRITICAL) != D20CAF.NONE;

        if (GameSystems.Anim.PushAttack(evt.objHndCaller, arrowTgt, -1, RandomRange(0, 2), isCritical, false))
        {
            var new_anim_id = GameSystems.Anim.GetActionAnimId(evt.objHndCaller);
            // print "new anim id: " + str(new_anim_id)
            dispIo.action.d20Caf |= D20CAF.NEED_ANIM_COMPLETED;
            dispIo.action.animID = new_anim_id;
        }
    }

    public static void SeekerArrowActionFrame(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20ActionTurnBased();
        // print "Seeker Arrow Action Frame"
        evt.SetConditionArg1(1); // mark as used this day
        var tgt = dispIo.action.d20ATarget;

        // print "Seeker Arrow Target: " + str(tgt)
        if (tgt != null)
        {
            FireArrowProjectile(dispIo.action, tgt.GetLocationFull());
        }
    }

    private static GameObject FireArrowProjectile(D20Action action, LocAndOffsets targetLoc)
    {
        var wpn = action.d20APerformer.ItemWornAt(EquipSlot.WeaponPrimary);
        if (wpn == null)
        {
            return null;
        }

        var projectileProto = GameSystems.Item.GetWeaponProjectileProto(wpn);

        // if missed, randomize the target location a bit
        if ((action.d20Caf & D20CAF.HIT) == 0)
        {
            targetLoc.off_x = GameSystems.Random.GetInt(-30, 30);
            targetLoc.off_y = GameSystems.Random.GetInt(-30, 30);
        }

        var startLoc = action.d20APerformer.GetLocationFull();

        var projectileHandle = GameSystems.D20.Combat.CreateProjectileAndThrow(startLoc, projectileProto, 0, 0,
            targetLoc, action.d20APerformer, action.d20ATarget);
        projectileHandle.OffsetZ = 60f;
        GameObject ammoItem = null;
        if (GameSystems.D20.Actions.ProjectileAppend(action, projectileHandle, ammoItem))
        {
            // print "Seeker Arrow Action Frame: Projectile Appended"
            action.d20APerformer.DispatchProjectileCreated(projectileHandle, action.d20Caf);
            action.d20Caf |= D20CAF.NEED_PROJECTILE_HIT;
        }

        return projectileHandle;
    }

    public static void SeekerArrowReset(in DispatcherCallbackArgs evt)
    {
        evt.SetConditionArg1(0);
    }

    // arg0 - used this day
    [FeatCondition(SeekerArrowName)]
    public static readonly ConditionSpec SeekerArrowFeat = ConditionSpec.Create("Seeker Arrow Feat", 3)
        .SetUnique()
        .AddHandler(DispatcherType.ConditionAdd, SeekerArrowReset)
        .AddHandler(DispatcherType.RadialMenuEntry, SeekerArrowRadial)
        .AddHandler(DispatcherType.PythonActionPerform, seekerArrowEnum, SeekerArrowPerform)
        .AddHandler(DispatcherType.PythonActionFrame, seekerArrowEnum, SeekerArrowActionFrame)
        .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, SeekerArrowReset) // Phase Arrow feat
        .Build();

    #endregion

    public static void PhaseArrowRadial(in DispatcherCallbackArgs evt)
    {
        if (!GameSystems.Critter.IsWieldingRangedWeapon(evt.objHndCaller))
        {
            return;
        }

        if (evt.GetConditionArg1() != 0)
        {
            return;
        }

        var radial_action = RadialMenuEntry.CreatePythonAction("Phase Arrow", D20ActionType.PYTHON_ACTION,
            phaseArrowEnum, 0, "TAG_INTERFACE_HELP");
        radial_action.AddAsChild(evt.objHndCaller, RadialMenuStandardNode.Class);
    }

    public static void PhaseArrowPerform(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20ActionTurnBased();
        var cur_seq = GameSystems.D20.Actions.CurrentSequence;
        var arrowTgt = dispIo.action.d20ATarget;
        var min_dist = 10000;
        // print "Phase arrow: target is " + str(arrowTgt)
        // roll to hit
        dispIo.action.d20Caf |= D20CAF.RANGED;
        GameSystems.D20.Combat.ToHitProcessing(dispIo.action);
        var isCritical = (dispIo.action.d20Caf & D20CAF.CRITICAL) != D20CAF.NONE;

        if (GameSystems.Anim.PushAttack(evt.objHndCaller, arrowTgt, -1, RandomRange(0, 2), isCritical, false))
        {
            var new_anim_id = GameSystems.Anim.GetActionAnimId(evt.objHndCaller);
            // print "new anim id: " + str(new_anim_id)
            dispIo.action.d20Caf |= D20CAF.NEED_ANIM_COMPLETED;
            dispIo.action.animID = new_anim_id;
        }
    }

    public static void PhaseArrowActionFrame(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20ActionTurnBased();
        // print "Phase Arrow Action Frame"
        evt.SetConditionArg1(1); // mark as used this day
        var tgt = dispIo.action.d20ATarget;
        // print "Phase Arrow Target: " + str(tgt)
        if (tgt != null)
        {
            FireArrowProjectile(dispIo.action, tgt.GetLocationFull());
        }
    }

    public static void PhaseArrowReset(in DispatcherCallbackArgs evt)
    {
        evt.SetConditionArg1(0);
    }

    public static void PhaseArrowArmorNullifier(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoAttackBonus();
        if (dispIo.attackPacket.d20ActnType != D20ActionType.PYTHON_ACTION)
        {
            return;
        }

        if (dispIo.attackPacket.dispKey != (int) phaseArrowEnum)
        {
            return;
        }

        var armor_ac_bonus_type = 28;
        var shield_ac_bonus_type = 29;
        var armor_enh_bonus_type = 12;
        var shield_enh_bonus_type = 33;
        dispIo.bonlist.AddCap(armor_ac_bonus_type, 0, 114, "Phase Arrow");
        dispIo.bonlist.AddCap(shield_ac_bonus_type, 0, 114, "Phase Arrow");
        dispIo.bonlist.AddCap(armor_enh_bonus_type, 0, 114, "Phase Arrow");
        dispIo.bonlist.AddCap(shield_enh_bonus_type, 0, 114, "Phase Arrow");
    }

    // arg0 - used this day
    [FeatCondition(PhaseArrowName)]
    public static readonly ConditionSpec PhaseArrowFeat = ConditionSpec.Create("Phase Arrow Feat", 3)
        .SetUnique()
        .AddHandler(DispatcherType.ConditionAdd, PhaseArrowReset)
        .AddHandler(DispatcherType.RadialMenuEntry, PhaseArrowRadial)
        .AddHandler(DispatcherType.PythonActionPerform, phaseArrowEnum, PhaseArrowPerform)
        .AddHandler(DispatcherType.PythonActionFrame, phaseArrowEnum, PhaseArrowActionFrame)
        .AddHandler(DispatcherType.AcModifyByAttacker, PhaseArrowArmorNullifier)
        .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, PhaseArrowReset)
        .Build(); // Hail of Arrows feat

    #region Hail of Arrows
    public static void HailOfArrowsRadial(in DispatcherCallbackArgs evt)
    {
        if (!GameSystems.Critter.IsWieldingRangedWeapon(evt.objHndCaller))
        {
            return;
        }

        if (evt.GetConditionArg1() != 0)
        {
            return;
        }

        var radial_action = RadialMenuEntry.CreatePythonAction("Hail of Arrows", D20ActionType.PYTHON_ACTION,
            hailOfArrowsEnum, 0, "TAG_INTERFACE_HELP");
        radial_action.d20SpellData = new D20SpellData(
            3180,
            SpellSystem.GetSpellClass(ClassId),
            evt.objHndCaller.GetStat(ClassId)
        );
        radial_action.AddAsChild(evt.objHndCaller, RadialMenuStandardNode.Class);
    }

    public static void HailOfArrowsReset(in DispatcherCallbackArgs evt)
    {
        evt.SetConditionArg1(0);
    }
    // arg0 - used this day
    [FeatCondition(HailOfArrowsName)]
    public static readonly ConditionSpec HailOfArrowsFeat = ConditionSpec.Create("Hail of Arrows Feat", 3)
        .SetUnique()
        .AddHandler(DispatcherType.ConditionAdd, HailOfArrowsReset)
        .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, HailOfArrowsReset)
        .AddHandler(DispatcherType.RadialMenuEntry, HailOfArrowsRadial) // Death Arrow feat
        .Build();
    #endregion

    #region Death Arrow
    public static void DeathArrowRadial(in DispatcherCallbackArgs evt)
    {
        if (!GameSystems.Critter.IsWieldingRangedWeapon(evt.objHndCaller))
        {
            return;
        }

        if (evt.GetConditionArg1() != 0)
        {
            return;
        }

        var radial_action = RadialMenuEntry.CreatePythonAction("Arrow of Death", D20ActionType.PYTHON_ACTION,
            deathArrowEnum, 0, "TAG_INTERFACE_HELP");
        radial_action.AddAsChild(evt.objHndCaller, RadialMenuStandardNode.Class);
    }

    public static void DeathArrowReset(in DispatcherCallbackArgs evt)
    {
        evt.SetConditionArg1(0);
        evt.SetConditionArg2(0);
        evt.SetConditionArg3(0);
    }

    public static void DeathArrowDamage(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoDamage();
        // print "Death Arrow Damage"
        if (evt.GetConditionArg3() == 0) // not anticipating death attack
        {
            return;
        }

        if ((dispIo.attackPacket.flags & D20CAF.RANGED) == D20CAF.NONE)
        {
            return;
        }

        var tgt = dispIo.attackPacket.victim;
        if (tgt == null)
        {
            return;
        }

        GameSystems.RollHistory.CreateFromFreeText(GameSystems.MapObject.GetDisplayNameForParty(tgt) + " hit by Arrow of Death...\n\n");
        if (tgt.SavingThrow(20, SavingThrowType.Fortitude, D20SavingThrowFlag.NONE, evt.objHndCaller))
        {
            tgt.FloatMesFileLine("mes/spell.mes", 30001);
            /**/
            GameSystems.RollHistory.CreateFromFreeText("Death effect failed.\n\n");
        }
        else
        {
            evt.objHndCaller.FloatLine("Arrow of Death!");
            tgt.KillWithDeathEffect(evt.objHndCaller);
            GameSystems.RollHistory.CreateFromFreeText("Killed by Arrow of Death!\n\n");
        }
    }

    public static bool IsActive(in DispatcherCallbackArgs args)
    {
        return args.GetConditionArg1() != 0;
    }

    public static void DeathAttackDisable(in DispatcherCallbackArgs evt)
    {
        evt.SetConditionArg3(0); // unset
    }

    public static void DeathArrowAttackRollMade(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoAttackBonus();
        if ((dispIo.attackPacket.flags & D20CAF.RANGED) == D20CAF.NONE)
        {
            return;
        }

        if (IsActive(in evt))
        {
            evt.SetConditionArg1(0);
            evt.SetConditionArg3(1);
        }
    }

    public static void DeathArrowCheck(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20ActionTurnBased();
        if (IsActive(in evt))
        {
            dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
            return;
        }

        // check if enough usages / day left
        var maxNumPerDay = 1;
        if (evt.GetConditionArg2() >= maxNumPerDay)
        {
            dispIo.returnVal = ActionErrorCode.AEC_OUT_OF_CHARGES;
        }
    }

    public static void ODeathArrowPerform(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20ActionTurnBased();
        if (IsActive(in evt))
        {
            Logger.Info("Not performing arrow of death ebcause it's already active");
            return;
        }

        evt.SetConditionArg1(1); // set to active
        evt.SetConditionArg2(evt.GetConditionArg2() + 1); // increment number used / day
        evt.SetConditionArg3(0); // reset expecting damage state
        evt.objHndCaller.FloatLine("Death Arrow equipped", TextFloaterColor.Red);
    }
    // arg0 - is active; arg1 - times spent; arg2 - anticipate death attack

    [FeatCondition(ArrowOfDeathName)]
    public static readonly ConditionSpec DeathArrowFeat = ConditionSpec.Create("Death Arrow Feat", 3)
        .SetUnique()
        .AddHandler(DispatcherType.RadialMenuEntry, DeathArrowRadial)
        .AddHandler(DispatcherType.ConditionAdd, DeathArrowReset)
        .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, DeathArrowReset)
        .AddHandler(DispatcherType.DealingDamage2, DeathArrowDamage) // signifies that a to hit roll was made
        .AddHandler(DispatcherType.AcModifyByAttacker, DeathArrowAttackRollMade)
        .AddHandler(DispatcherType.PythonActionCheck, deathArrowEnum, DeathArrowCheck)
        .AddHandler(DispatcherType.PythonActionPerform, deathArrowEnum,
            ODeathArrowPerform) // gets triggered at the end of the damage calculation
        .AddHandler(DispatcherType.D20Signal, D20DispatcherKey.SIG_Attack_Made, DeathAttackDisable)
        .Build();

    #endregion
}