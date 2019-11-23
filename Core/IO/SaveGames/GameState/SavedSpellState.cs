using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Spells;

namespace SpicyTemple.Core.IO.SaveGames.GameState
{
    public class SavedSpellState
    {
        public int SpellIdSerial { get; set; }

        public Dictionary<int, SavedActiveSpell> ActiveSpells { get; set; } = new Dictionary<int, SavedActiveSpell>();

        // This was previously embedded in MapLoad, for some reason...
        [TempleDllLocation(0x100792a0)]
        public static SavedSpellState Read(BinaryReader reader)
        {
            var result = new SavedSpellState();

            result.SpellIdSerial = reader.ReadInt32();

            var spellCount = reader.ReadInt32();
            var activeSpells = result.ActiveSpells;
            for (var i = 0; i < spellCount; i++)
            {
                var activeSpell = SavedActiveSpell.Read(reader);
                if (!activeSpells.TryAdd(activeSpell.Id, activeSpell))
                {
                    throw new CorruptSaveException($"Duplicate active spell id: {activeSpell.Id}");
                }
            }

            return result;
        }
    }

    public class SavedActiveSpell
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        public int Id { get; set; }

        public bool IsActive { get; set; }

        public int SpellEnum { get; set; }

        public int SpellEnumOriginal { get; set; }

        public SpellAnimationFlag AnimFlags { get; set; }

        public ObjectId CasterId { get; set; }

        // ELF32 hash of the particle system id
        public int CasterPartSysHash { get; set; }

        public int ClassCode { get; set; }

        public int SpellLevel { get; set; }

        public int CasterLevel { get; set; }

        public int DC { get; set; }

        public ObjectId AoEObjectId { get; set; }

        // ObjectID and associated particle system id hash (ELF32)
        public List<(ObjectId, int)> SpellObjects { get; set; } = new List<(ObjectId, int)>();

        // ObjectID and associated particle system id hash (ELF32)
        public List<(ObjectId, int)> Targets { get; set; } = new List<(ObjectId, int)>();

        // The initial targets when the spell was initially cast
        public List<ObjectId> InitialTargets { get; set; } = new List<ObjectId>();

        public List<ObjectId> Projectiles { get; set; } = new List<ObjectId>();

        public LocAndOffsets AoECenter { get; set; }

        public float AoECenterZ { get; set; } // TODO consolidate with aoeCenter

        public int Duration { get; set; }

        public int DurationRemaining { get; set; }

        public int SpellRange { get; set; }

        public int SavingThrowResult { get; set; }

        public MetaMagicData MetaMagic { get; set; }

        public int SpellId { get; set; }

        [TempleDllLocation(0x100786b0)]
        public static SavedActiveSpell Read(BinaryReader reader)
        {
            var result = new SavedActiveSpell();

            result.Id = reader.ReadInt32();

            Logger.Debug("Loading spellId {0}", result.Id);

            var key = reader.ReadInt32();
            if (key != result.Id)
            {
                throw new CorruptSaveException($"Spell ID {result.Id} doesn't match key {key}");
            }

            result.IsActive = reader.ReadInt32() != 0;
            if (!result.IsActive)
            {
                Logger.Debug("Spell was inactive!");
            }

            result.SpellEnum = reader.ReadInt32();
            result.SpellEnumOriginal = reader.ReadInt32();

            result.AnimFlags = (SpellAnimationFlag) reader.ReadInt32();

            result.CasterId = reader.ReadObjectId();
            result.CasterPartSysHash = reader.ReadInt32();

            result.ClassCode = reader.ReadInt32();
            result.SpellLevel = reader.ReadInt32();
            result.CasterLevel = reader.ReadInt32();
            result.DC = reader.ReadInt32();

            var spellObjectCount = reader.ReadInt32();

            result.AoEObjectId = reader.ReadObjectId();

            // Spell objects
            for (var i = 0; i < 128; i++)
            {
                var spellObjId = reader.ReadObjectId();
                var spellPartSysIdHash = reader.ReadInt32();

                if (i < spellObjectCount && !spellObjId.IsNull)
                {
                    result.SpellObjects.Add((spellObjId, spellPartSysIdHash));
                }
            }

            // targets
            var initialTargetCount = reader.ReadInt32();
            var targetCount = reader.ReadInt32();

            Span<ObjectId> targetIds = stackalloc ObjectId[32];
            for (var i = 0; i < targetIds.Length; i++)
            {
                targetIds[i] = reader.ReadObjectId();
            }

            Span<int> targetPartSysIds = stackalloc int[32]; // ELF32 hashes
            for (var i = 0; i < targetPartSysIds.Length; i++)
            {
                targetPartSysIds[i] = reader.ReadInt32();
            }

            if (targetCount < 0 || targetCount > targetIds.Length)
            {
                throw new CorruptSaveException($"Invalid target count: {targetCount}");
            }

            if (initialTargetCount < 0 || initialTargetCount > targetIds.Length)
            {
                throw new CorruptSaveException($"Invalid initial target count: {initialTargetCount}");
            }

            for (var i = 0; i < targetCount; i++)
            {
                result.Targets.Add((targetIds[i], targetPartSysIds[i]));
            }

            // NOTE: This is not 100% what vailla did, but it's close enough
            for (var i = 0; i < initialTargetCount; i++)
            {
                result.InitialTargets.Add(targetIds[i]);
            }

            // projectiles
            var projectileCount = reader.ReadInt32();
            for (var i = 0; i < 5; i++)
            {
                var projectileId = reader.ReadObjectId();

                if (i < projectileCount && !projectileId.IsNull)
                {
                    result.Projectiles.Add(projectileId);
                }
            }

            result.AoECenter = reader.ReadLocationAndOffsets();
            result.AoECenterZ = reader.ReadSingle();

            result.Duration = reader.ReadInt32();
            result.DurationRemaining = reader.ReadInt32();
            result.SpellRange = reader.ReadInt32();
            result.SavingThrowResult = reader.ReadInt32();
            var metaMagicData = reader.ReadUInt32();
            result.MetaMagic = MetaMagicData.Unpack(metaMagicData);
            var spellId = reader.ReadInt32();
            if (spellId != result.Id)
            {
                throw new CorruptSaveException($"Trailing spell id is {spellId} for spell {result.Id}");
            }

            return result;
        }
    }
}