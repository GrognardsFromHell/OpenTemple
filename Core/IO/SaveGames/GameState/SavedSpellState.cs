using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Spells;

namespace OpenTemple.Core.IO.SaveGames.GameState
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

        [TempleDllLocation(0x10079220)]
        public void Write(BinaryWriter writer)
        {
            writer.WriteInt32(SpellIdSerial);

            writer.WriteInt32(ActiveSpells.Count);

            foreach (var activeSpell in ActiveSpells.Values)
            {
                activeSpell.Write(writer);
            }
        }
    }

    public class SavedActiveSpell
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

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
        public HashSet<ObjectId> InitialTargets { get; set; } = new HashSet<ObjectId>();

        public List<ObjectId> Projectiles { get; set; } = new List<ObjectId>();

        public LocAndOffsets AoECenter { get; set; }

        public float AoECenterZ { get; set; } // TODO consolidate with aoeCenter

        public int Duration { get; set; }

        public int DurationRemaining { get; set; }

        public int SpellRange { get; set; }

        public bool SavingThrowResult { get; set; }

        public MetaMagicData MetaMagic { get; set; }

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
                if (!targetIds[i].IsNull)
                {
                    result.Targets.Add((targetIds[i], targetPartSysIds[i]));
                }
            }

            // NOTE: This is not 100% what vailla did, but it's close enough
            result.InitialTargets.EnsureCapacity(initialTargetCount);
            for (var i = 0; i < initialTargetCount; i++)
            {
                if (!targetIds[i].IsNull)
                {
                    result.InitialTargets.Add(targetIds[i]);
                }
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
            result.SavingThrowResult = reader.ReadInt32() != 0;
            var metaMagicData = reader.ReadUInt32();
            result.MetaMagic = MetaMagicData.Unpack(metaMagicData);
            var spellId = reader.ReadInt32();
            if (spellId != result.Id)
            {
                throw new CorruptSaveException($"Trailing spell id is {spellId} for spell {result.Id}");
            }

            return result;
        }

        [TempleDllLocation(0x100786b0)]
        public void Write(BinaryWriter writer)
        {
            writer.WriteInt32(Id);

            Logger.Debug("Saving spellId {0}", Id);

            writer.WriteInt32(Id);

            writer.WriteInt32(IsActive ? 1 : 0);

            writer.WriteInt32(SpellEnum);
            writer.WriteInt32(SpellEnumOriginal);

            writer.WriteInt32((int) AnimFlags);

            writer.WriteObjectId(CasterId);
            writer.WriteInt32(CasterPartSysHash);

            writer.WriteInt32(ClassCode);
            writer.WriteInt32(SpellLevel);
            writer.WriteInt32(CasterLevel);
            writer.WriteInt32(DC);

            Trace.Assert(SpellObjects.Count < 128);
            writer.WriteInt32(SpellObjects.Count);

            writer.WriteObjectId(AoEObjectId);

            // Spell objects (fixed length list)
            for (var i = 0; i < 128; i++)
            {
                if (i < SpellObjects.Count)
                {
                    writer.WriteObjectId(SpellObjects[i].Item1); // Object id
                    writer.WriteInt32(SpellObjects[i].Item2); // Part sys hash
                }
                else
                {
                    writer.WriteObjectId(ObjectId.CreateNull());
                    writer.WriteInt32(0);
                }
            }

            // targets
            Trace.Assert(InitialTargets.Count < 32);
            Trace.Assert(Targets.Count < 32);
            writer.WriteInt32(InitialTargets.Count);
            writer.WriteInt32(Targets.Count);

            Span<ObjectId> targetIds = stackalloc ObjectId[32];
            Span<int> targetPartSysIds = stackalloc int[32]; // ELF32 hashes

            // This is not 100% what vanilla did, but we're trying to replicate it here
            for (var i = 0; i < Targets.Count; i++)
            {
                if (i < targetIds.Length)
                {
                    targetIds[i] = Targets[i].Item1;
                    targetPartSysIds[i] = Targets[i].Item2;
                }
            }

            // Fill out the rest with the initial D20 action targets (which Vanilla did not save separately)
            var initialTargetIdx = Targets.Count;
            foreach (var initialTarget in InitialTargets)
            {
                if (initialTargetIdx < targetIds.Length)
                {
                    targetIds[initialTargetIdx] = initialTarget;
                }

                initialTargetIdx++;
            }

            for (var i = 0; i < targetIds.Length; i++)
            {
                writer.WriteObjectId(targetIds[i]);
            }
            for (var i = 0; i < targetPartSysIds.Length; i++)
            {
                 writer.WriteInt32(targetPartSysIds[i]);
            }

            // projectiles
            Trace.Assert(Projectiles.Count < 5);
            writer.WriteInt32(Projectiles.Count);
            for (var i = 0; i < 5; i++)
            {
                if (i < Projectiles.Count)
                {
                    writer.WriteObjectId(Projectiles[i]);
                }
                else
                {
                    writer.WriteObjectId(ObjectId.CreateNull());
                }
            }

            writer.WriteLocationAndOffsets(AoECenter);
            writer.WriteSingle(AoECenterZ);

            writer.WriteInt32(Duration);
            writer.WriteInt32(DurationRemaining);
            writer.WriteInt32(SpellRange);
            writer.WriteInt32(SavingThrowResult ? 1 : 0);
            writer.WriteUInt32(MetaMagic.Pack());

            writer.WriteInt32(Id);
        }
    }
}