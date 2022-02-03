
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
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts;

public static class Stench
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private static readonly int OBJ_SPELL_STENCH = 6400;
    private static readonly int STENCH_DURATION = 100;
    private static readonly obj_f EFFECT_VAR = obj_f.item_pad_i_1;
    private static readonly obj_f COUNT_VAR = obj_f.item_pad_i_2;
    private static readonly obj_f STATE_VAR = obj_f.critter_pad_i_5;
    private static readonly int STATE_UNPROCESSED = 0;
    private static readonly int STATE_UNAFFECTED = 1;
    private static readonly int STATE_NOTINSTENCH = 2;
    private static readonly int STATE_NAUSEA = 3;
    private static readonly int STATE_NAUSEA_HANGOVER = 4;
    // Unused. (I felt that it was only fair to get further saving throws after nausea expires)

    private static readonly int STATE_NAUSEA_EXPIRED = 5;
    private static readonly int STATE_SICKNESS = 6;
    private static readonly int STATE_NOPROCESS = 7;
    private static readonly int STATE_NEUTRALISED = 8;
    private static readonly int CID = 0;
    private static readonly int COUNTER = 0;
    private static readonly int BASECASTERID = 1;
    private static int casterIdNext = BASECASTERID;
    // Main stench processing routine.

    public static void processStench(GameObject caster, int spell_id)
    {
        // Get caster ID, initialising if necessary.
        var casterId = Co8.getObjVarNibble(caster, STATE_VAR, CID);
        if ((casterId == 0))
        {
            casterId = casterIdNext;
            casterIdNext = casterIdNext + 1;
            if ((casterIdNext > 7))
            {
                casterIdNext = BASECASTERID;
            }

            Co8.setObjVarNibble(caster, STATE_VAR, CID, casterId);
        }

        // While caster is alive, concious and not destroyed or off.
        // print "checking caster... flags= ", caster.object_flags_get() & 0xff, " anded=", (caster.object_flags_get() & (OF_DESTROYED | OF_OFF))
        if ((caster.GetStat(Stat.hp_current) > -10 && (caster.GetObjectFlags() & (ObjectFlag.DESTROYED | ObjectFlag.OFF)) == 0))
        {
            Logger.Info("Starting stench processing for {0} id={1} spell_id={2}", caster, casterId, spell_id);
            foreach (var critter in ObjList.ListVicinity(caster.GetLocation(), ObjectListFilter.OLC_CRITTERS))
            {
                if ((critter.GetStat(Stat.hp_current) <= -10))
                {
                    Co8.setObjVarNibble(critter, STATE_VAR, casterId, STATE_NOPROCESS);
                }

                // Get critter status and process appropriately.
                var status = Co8.getObjVarNibble(critter, STATE_VAR, casterId);
                if ((status == STATE_UNPROCESSED))
                {
                    // Unprocessed - process critter init, and assign state.
                    if ((critter.IsMonsterSubtype(MonsterSubtype.demon) || critter.IsMonsterCategory(MonsterCategory.elemental) || critter.IsMonsterCategory(MonsterCategory.undead) || critter.GetStat(Stat.level_monk) > 10 || critter.GetStat(Stat.level_druid) > 8))
                    {
                        // Unaffected critters. (demons, elementals, undead, classes immune to poison)
                        Logger.Info("{0} unaffected", critter);
                        status = STATE_UNAFFECTED;
                    }
                    else if (has_necklace(critter))
                    {
                        Logger.Info("{0} unaffected", critter);
                        status = STATE_UNAFFECTED;
                    }
                    else if ((inStenchArea(caster, critter)))
                    {
                        // In stench area - attempt save, apply the stench and take ownership.
                        status = attemptSave(caster, spell_id, critter, status);
                        applyStenchEffect(casterId, spell_id, critter, status);
                    }
                    else
                    {
                        // Not yet in stench area.
                        Logger.Info("{0} not yet in stench area", critter);
                        status = STATE_NOTINSTENCH;
                    }

                }
                else if ((status == STATE_NOTINSTENCH))
                {
                    // Check cured count.
                    var curedCount = Co8.getObjVarNibble(critter, STATE_VAR, COUNTER);
                    if ((curedCount > 0))
                    {
                        Logger.Info("{0} has been cured, skipping round", critter);
                        Co8.setObjVarNibble(critter, STATE_VAR, COUNTER, curedCount - 1);
                    }
                    // Check if now in stink area.
                    else if ((inStenchArea(caster, critter)))
                    {
                        // In stink area - apply the stench effect.
                        Logger.Info("{0} now in stench area, attempting save", critter);
                        status = attemptSave(caster, spell_id, critter, status);
                        applyStenchEffect(casterId, spell_id, critter, status);
                    }

                }
                else if ((status == STATE_SICKNESS))
                {
                    // Check if now out of stink area.
                    if ((!inStenchArea(caster, critter)))
                    {
                        Logger.Info("{0} now out of stench area, removing sickness", critter);
                        status = STATE_NOPROCESS;
                        removeStenchEffect(casterId, spell_id, critter, status, false);
                    }

                }
                else if ((status == STATE_NAUSEA))
                {
                    // Check if now out of stink area.
                    if ((!inStenchArea(caster, critter)))
                    {
                        status = STATE_NAUSEA_HANGOVER;
                        var hangoverCount = RandomRange(0, 3);
                        var stench_obj = getStenchObj(critter);
                        if ((stench_obj != null))
                        {
                            Logger.Info("{0} now out of stench area, changing to nausea hangover, count= {1}", critter, hangoverCount);
                            Co8.setObjVarNibble(stench_obj, EFFECT_VAR, casterId, status);
                            Co8.setObjVarNibble(stench_obj, COUNT_VAR, casterId, hangoverCount);
                        }
                        else
                        {
                            Logger.Info("Error: Can't find spell object for nausea.");
                            status = STATE_NOTINSTENCH; // In case of error.
                        }

                    }

                }
                else if ((status == STATE_NAUSEA_HANGOVER))
                {
                    var stench_obj = getStenchObj(critter);
                    if ((stench_obj != null))
                    {
                        // Check if now back in stink area.
                        if ((inStenchArea(caster, critter)))
                        {
                            Logger.Info("{0} now back in stench area, changing back to nausea", critter);
                            status = STATE_NAUSEA;
                            Co8.setObjVarNibble(stench_obj, EFFECT_VAR, casterId, status);
                        }
                        else
                        {
                            // Maintain hangover.
                            var hangoverCount = Co8.getObjVarNibble(stench_obj, COUNT_VAR, casterId);
                            Logger.Info("{0} has nausea hangover, count= {1}", critter, hangoverCount);
                            if ((hangoverCount < 1))
                            {
                                status = STATE_NOTINSTENCH;
                                removeStenchEffect(casterId, spell_id, critter, status, true);
                            }
                            else
                            {
                                Co8.setObjVarNibble(stench_obj, COUNT_VAR, casterId, hangoverCount - 1);
                            }

                        }

                    }
                    else
                    {
                        Logger.Info("Error: Can't find spell object for hangover.");
                        status = STATE_NOTINSTENCH; // In case of error.
                    }

                }
                else
                {
                    continue;

                }

                // Store critter status.
                Co8.setObjVarNibble(critter, STATE_VAR, casterId, status);
            }

            // Setup processing for next round.
            StartTimer(500, () => processStench(caster, spell_id));
        }
        else
        {
            Logger.Info("Caster died or expired, ending the stench.");
            endStench(caster, spell_id);
        }

        return;
    }
    public static bool inStenchArea(GameObject caster, GameObject critter)
    {
        return (critter.DistanceTo(caster) < 10);
    }
    public static int attemptSave(GameObject caster, int spell_id, GameObject critter, int status)
    {
        // In stink area - attempt saving throw.
        if ((critter.SavingThrowSpell(24, SavingThrowType.Fortitude, D20SavingThrowFlag.POISON, caster, spell_id)))
        {
            // Passed saving throw, apply sickness.
            status = STATE_SICKNESS;
            Logger.Info("{0} is in stench, passed saving throw - applying sickness", critter);
            critter.FloatMesFileLine("mes/spell.mes", 30001);
        }
        else
        {
            // Failed saving throw - apply nausea.
            status = STATE_NAUSEA;
            Logger.Info("{0} is in stench, failed saving throw - applying nausea", critter);
            critter.FloatMesFileLine("mes/spell.mes", 30002);
        }

        return status;
    }
    // Give the critter an effect object with the appropriate effect.

    public static void applyStenchEffect(int casterId, int spell_id, GameObject critter, int status)
    {
        var stench_obj = getStenchObj(critter);
        if ((stench_obj != null))
        {
            if ((status == STATE_NAUSEA))
            {
                // Check current stench effect.
                var effectOwnerId = Co8.getObjVarNibble(stench_obj, EFFECT_VAR, CID);
                var effectStatus = Co8.getObjVarNibble(stench_obj, EFFECT_VAR, effectOwnerId);
                if ((effectStatus == STATE_SICKNESS))
                {
                    // Take ownership of effect since Nausea trumps Sickness.
                    Logger.Info("caster {0} taking ownership of effect from caster {1} on {2}", casterId, effectOwnerId, critter);
                    var effectVar = Co8.getObjVarDWord(stench_obj, EFFECT_VAR);
                    stench_obj.Destroy();
                    stench_obj = createStenchObject(spell_id, critter, status);
                    Co8.setObjVarDWord(stench_obj, EFFECT_VAR, effectVar);
                }

            }

        }
        else
        {
            stench_obj = createStenchObject(spell_id, critter, status);
        }

        // Set the status & id into the effect var.
        Co8.setObjVarNibble(stench_obj, EFFECT_VAR, CID, casterId);
        Co8.setObjVarNibble(stench_obj, EFFECT_VAR, casterId, status);
        return;
    }
    public static void removeStenchEffect(int casterId, int spell_id, GameObject critter, int status, bool unNullify)
    {
        var stench_obj = getStenchObj(critter);
        if ((stench_obj != null))
        {
            Logger.Info("removing effect {0} for {1} by caster {2}", stench_obj, critter, casterId);
            Co8.setObjVarNibble(stench_obj, EFFECT_VAR, casterId, status);
            // Check current stench effect.
            var effectOwnerId = Co8.getObjVarNibble(stench_obj, EFFECT_VAR, CID);
            if ((casterId == effectOwnerId))
            {
                // Pass ownership of effect to another stench.
                int s = 0;
                int otherId;
                for (otherId = BASECASTERID; otherId < 7; otherId++)
                {
                    s = Co8.getObjVarNibble(stench_obj, EFFECT_VAR, otherId);
                    if (s == STATE_NAUSEA || s == STATE_NAUSEA_HANGOVER)
                    {
                        break;
                        // Nausea trumps Sickness.
                    }

                }

                var effectVar = Co8.getObjVarDWord(stench_obj, EFFECT_VAR);
                stench_obj.Destroy();
                if (s == STATE_NAUSEA || s == STATE_NAUSEA_HANGOVER || s == STATE_SICKNESS)
                {
                    Logger.Info("caster {0} taking ownership of effect from caster {1} on {2}", otherId, casterId, critter);
                    stench_obj = createStenchObject(spell_id, critter, s);
                    Co8.setObjVarDWord(stench_obj, EFFECT_VAR, effectVar);
                    Co8.setObjVarNibble(stench_obj, EFFECT_VAR, CID, otherId);
                }

                if (s != STATE_NAUSEA && s != STATE_NAUSEA_HANGOVER && unNullify)
                {
                    reenableWeapons(critter);
                }

            }

        }
        else
        {
            reenableWeapons(critter);
        }

        return;
    }
    public static GameObject createStenchObject(int spell_id, GameObject critter, int status)
    {
        var stench_obj = GameSystems.MapObject.CreateObject(OBJ_SPELL_STENCH, critter.GetLocation());
        stench_obj.SetItemFlag(ItemFlag.NO_DROP);
        stench_obj.SetItemFlag(ItemFlag.NO_LOOT);
        Co8.set_spell_flag(stench_obj, Co8SpellFlag.HezrouStench);
        if ((status == STATE_SICKNESS))
        {
            Logger.Info("status: {0} applying sickness effect to {1} for {2}", status, stench_obj, critter);
            stench_obj.AddConditionToItem("sp-Unholy Blight", spell_id, STENCH_DURATION, 0); // Doesn't penalise saves - does flag char info.
            stench_obj.AddConditionToItem("Saving Throw Resistance Bonus", 0, -2);
            stench_obj.AddConditionToItem("Saving Throw Resistance Bonus", 1, -2);
            stench_obj.AddConditionToItem("Saving Throw Resistance Bonus", 2, -2);
        }
        else if ((status == STATE_NAUSEA))
        {
            Logger.Info("status: {0} applying nausea effect to {1} for {2}", status, stench_obj, critter);
            // Reduce weapon dmg since not supposed to be able to attack when nauseated.
            nullifyWeapons(critter);
            // Apply simulated nausea effects to stench obj.
            var strength = critter.GetStat(Stat.strength); // No strength bonus.
            stench_obj.AddConditionToItem("Attribute Enhancement Bonus", 0, 11 - strength);
            stench_obj.AddConditionToItem("sp-Chaos Hammer", spell_id, STENCH_DURATION, 0); // Only get move action. (+2 AC, -2 attack & damage rolls, +2 saves [shoddy implementation])
            stench_obj.AddConditionToItem("sp-Feeblemind", 0, 0, 0); // No casting. (-4 to saves for arcane spellcasters)
            if ((critter.GetStat(Stat.level_sorcerer) > 0 || critter.GetStat(Stat.level_bard) > 0 || critter.GetStat(Stat.level_wizard) > 0))
            {
                stench_obj.AddConditionToItem("Saving Throw Resistance Bonus", 0, 2);
                stench_obj.AddConditionToItem("Saving Throw Resistance Bonus", 1, 2);
                stench_obj.AddConditionToItem("Saving Throw Resistance Bonus", 2, 2); // Counteract feeblemind penalties for arcane casters.
            }
            else
            {
                stench_obj.AddConditionToItem("Saving Throw Resistance Bonus", 0, -2);
                stench_obj.AddConditionToItem("Saving Throw Resistance Bonus", 1, -2);
                stench_obj.AddConditionToItem("Saving Throw Resistance Bonus", 2, -2); // Counteract hammer bonuses.
            }

        }

        critter.GetItem(stench_obj);
        AttachParticles("sp-Stinking Cloud Hit", critter);
        return stench_obj;
    }
    public static GameObject getStenchObj(GameObject critter)
    {
        var it = 0;
        var stench_obj = critter.FindItemByName(OBJ_SPELL_STENCH);
        while ((stench_obj != null && !Co8.is_spell_flag_set(stench_obj, Co8SpellFlag.HezrouStench) && it < 217))
        {
            stench_obj = critter.GetInventoryItem(it);
            it += 1;
        }

        if ((it >= 217))
        {
            Logger.Info("Search for Stench Object a lot of times!");
        }

        return stench_obj;
    }
    public static void neutraliseStench(GameObject critter, int duration)
    {
        Logger.Info("Neutralising stench on: {0}", critter);
        var stench_obj = getStenchObj(critter);
        if ((stench_obj != null))
        {
            stench_obj.Destroy();
            reenableWeapons(critter);
        }

        // Add to cured count and set all stench id's to neutralised.
        var curedCount = Co8.getObjVarNibble(critter, STATE_VAR, COUNTER);
        Co8.setObjVarNibble(critter, STATE_VAR, COUNTER, curedCount + 1);
        for (var casterId = BASECASTERID; casterId < 7; casterId++)
        {
            Co8.setObjVarNibble(critter, STATE_VAR, casterId, STATE_NEUTRALISED);
        }

        // Setup processing for un-neutralisation.
        StartTimer(1000 * duration, () => unNeutraliseStench(critter));
        return;
    }
    public static void unNeutraliseStench(GameObject critter)
    {
        var curedCount = Co8.getObjVarNibble(critter, STATE_VAR, COUNTER);
        Logger.Info("Un-neutralising stench, curedCount={0}", curedCount);
        Co8.setObjVarNibble(critter, STATE_VAR, COUNTER, curedCount - 1);
        if ((curedCount <= 1))
        {
            Logger.Info("Un-neutralising stench on: {0}", critter);
            for (var casterId = BASECASTERID; casterId < 7; casterId++)
            {
                Co8.setObjVarNibble(critter, STATE_VAR, casterId, STATE_UNPROCESSED);
            }

        }

        return;
    }
    public static void nullifyWeapons(GameObject critter)
    {
        for (var num = 4000; num < 5000; num++)
        {
            var weapon = critter.FindItemByProto(num);
            if ((weapon != null && !Co8.is_spell_flag_set(weapon, Co8SpellFlag.HezrouStench)))
            {
                Logger.Info("Nullifying weapon {0} for {1}", weapon, critter);
                Co8.set_spell_flag(weapon, Co8SpellFlag.HezrouStench);
                weapon.SetInt(obj_f.weapon_pad_i_2, weapon.GetInt(obj_f.weapon_damage_dice));
                weapon.SetInt(obj_f.weapon_damage_dice, 0);
                weapon.SetItemFlag(ItemFlag.NO_DROP);
            }

        }

        return;
    }
    public static void reenableWeapons(GameObject critter)
    {
        for (var num = 4000; num < 5000; num++)
        {
            var weapon = critter.FindItemByProto(num);
            if ((weapon != null && Co8.is_spell_flag_set(weapon, Co8SpellFlag.HezrouStench)))
            {
                Logger.Info("Enabling weapon {0} for {1}", weapon, critter);
                weapon.SetInt(obj_f.weapon_damage_dice, weapon.GetInt(obj_f.weapon_pad_i_2));
                Co8.unset_spell_flag(weapon, Co8SpellFlag.HezrouStench);
                weapon.ClearItemFlag(ItemFlag.NO_DROP);
            }

        }

        return;
    }
    public static void endStench(GameObject caster, int spell_id)
    {
        var casterId = 1;
        if ((caster != null))
        {
            casterId = Co8.getObjVarNibble(caster, STATE_VAR, CID);
            Logger.Info("Cleaning up and ending stench for {0} id={1}", caster, casterId);
        }
        else
        {
            Logger.Info("Caster is null now, trying default");
            caster = SelectedPartyLeader; // just to get a location
        }

        foreach (var critter in ObjList.ListVicinity(caster.GetLocation(), ObjectListFilter.OLC_CRITTERS))
        {
            if ((Co8.getObjVarNibble(critter, STATE_VAR, casterId) != STATE_UNPROCESSED))
            {
                removeStenchEffect(casterId, spell_id, critter, STATE_UNPROCESSED, true);
            }

            if ((Co8.getObjVarNibble(critter, STATE_VAR, casterId) != STATE_NEUTRALISED))
            {
                Co8.setObjVarNibble(critter, STATE_VAR, casterId, STATE_UNPROCESSED);
            }

        }

        return;
    }
    public static bool has_necklace(GameObject critter)
    {
        var full = critter.ItemWornAt(EquipSlot.Necklace);
        if (full != null && full.GetNameId() == 6107)
        {
            return true;
        }

        return false;
    }

}