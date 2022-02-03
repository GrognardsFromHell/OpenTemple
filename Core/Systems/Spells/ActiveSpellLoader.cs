using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using OpenTemple.Core.IO.SaveGames;
using OpenTemple.Core.IO.SaveGames.GameState;

namespace OpenTemple.Core.Systems.Spells;

internal static class ActiveSpellLoader
{
    public static SpellPacketBody LoadActiveSpell(SavedActiveSpell savedSpell)
    {
        var spellPacket = new SpellPacketBody
        {
            spellEnum = savedSpell.SpellEnum,
            spellEnumOriginal = savedSpell.SpellEnumOriginal,
            animFlags = savedSpell.AnimFlags,
            spellClass = savedSpell.ClassCode,
            spellKnownSlotLevel = savedSpell.SpellLevel,
            casterLevel = savedSpell.CasterLevel,
            dc = savedSpell.DC,
            aoeCenter = savedSpell.AoECenter,
            aoeCenterZ = savedSpell.AoECenterZ,
            duration = savedSpell.Duration,
            durationRemaining = savedSpell.DurationRemaining,
            spellRange = savedSpell.SpellRange,
            savingThrowResult = savedSpell.SavingThrowResult,
            metaMagicData = savedSpell.MetaMagic
        };

        // Restore referenced objects
        if (!savedSpell.CasterId.IsNull)
        {
            spellPacket.caster = GameSystems.Object.GetObject(savedSpell.CasterId);
            if (spellPacket.caster == null)
            {
                throw new CorruptSaveException(
                    $"Caster {savedSpell.CasterId} of active spell {savedSpell.Id} could not be found.");
            }
        }

        if (!savedSpell.AoEObjectId.IsNull)
        {
            spellPacket.aoeObj = GameSystems.Object.GetObject(savedSpell.AoEObjectId);
            if (spellPacket.aoeObj == null)
            {
                throw new CorruptSaveException(
                    $"AoE object {savedSpell.AoEObjectId} of active spell {savedSpell.Id} could not be found.");
            }
        }

        LoadSpellObjects(savedSpell, spellPacket);
        LoadTargets(savedSpell, spellPacket);
        LoadInitialTargets(savedSpell, spellPacket);
        LoadProjectiles(savedSpell, spellPacket);

        // Restore particle systems
        if (savedSpell.CasterPartSysHash != 0 && spellPacket.caster != null)
        {
            spellPacket.casterPartSys =
                GameSystems.ParticleSys.CreateAtObj(savedSpell.CasterPartSysHash, spellPacket.caster);
        }

        return spellPacket;
    }

    private static void LoadSpellObjects(SavedActiveSpell savedSpell, SpellPacketBody spellPacket)
    {
        var index = 0;
        spellPacket.spellObjs = new SpellObj[savedSpell.SpellObjects.Count];
        foreach (var (objectId, partSysHash) in savedSpell.SpellObjects)
        {
            var spellObj = GameSystems.Object.GetObject(objectId);
            if (spellObj == null)
            {
                throw new CorruptSaveException(
                    $"Spell object {objectId} of active spell {savedSpell.Id} could not be found.");
            }

            object partSys = null;
            if (partSysHash != 0)
            {
                partSys = GameSystems.ParticleSys.CreateAtObj(partSysHash, spellObj);
            }

            spellPacket.spellObjs[index++] = new SpellObj
            {
                obj = spellObj,
                partSys = partSys
            };
        }

        Trace.Assert(index == spellPacket.spellObjs.Length);
    }

    private static void LoadTargets(SavedActiveSpell savedSpell, SpellPacketBody spellPacket)
    {
        var index = 0;
        spellPacket.Targets = new SpellTarget[savedSpell.Targets.Count];
        foreach (var (objectId, partSysHash) in savedSpell.Targets)
        {
            var targetObj = GameSystems.Object.GetObject(objectId);
            if (targetObj == null)
            {
                throw new CorruptSaveException(
                    $"Spell target {objectId} of active spell {savedSpell.Id} could not be found.");
            }

            object partSys = null;
            if (partSysHash != 0)
            {
                partSys = GameSystems.ParticleSys.CreateAtObj(partSysHash, targetObj);
            }

            spellPacket.Targets[index++] = new SpellTarget(targetObj, partSys);
        }

        Trace.Assert(index == spellPacket.Targets.Length);
    }

    private static void LoadInitialTargets(SavedActiveSpell savedSpell, SpellPacketBody spellPacket)
    {
        spellPacket.InitialTargets = savedSpell.InitialTargets.Select(objectId =>
        {
            var obj = GameSystems.Object.GetObject(objectId);
            if (obj == null)
            {
                throw new CorruptSaveException(
                    $"Initial target {objectId} of spell {savedSpell.Id} couldn't be found.");
            }

            return obj;
        }).ToImmutableHashSet();
    }

    private static void LoadProjectiles(SavedActiveSpell savedSpell, SpellPacketBody spellPacket)
    {
        spellPacket.projectiles = savedSpell.Projectiles.Select(objectId =>
        {
            var obj = GameSystems.Object.GetObject(objectId);
            if (obj == null)
            {
                throw new CorruptSaveException(
                    $"Projectile {objectId} of spell {savedSpell.Id} couldn't be found.");
            }

            return obj;
        }).ToArray();
    }
}