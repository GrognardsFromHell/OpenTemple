using System;
using System.Collections.Generic;
using System.IO;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.IO.SaveGames.GameState
{
    public class SavedD20State
    {
        public ObjectId[] TurnOrder { get; set; }

        // Index into TurnOrder
        public int CurrentTurnIndex { get; set; }

        // I wonder if this is actually used since it's constantly being reset and mostly used to "build" an incomplete action
        public SavedD20Action GlobalAction { get; set; }

        public int CurrentSequenceIndex { get; set; }

        public SavedD20ActionSequence[] ActionSequences { get; set; }

        public SavedProjectile[] Projectiles { get; set; }

        public SavedReadiedAction[] ReadiedActions { get; set; }

        public SavedHotkeys Hotkeys { get; set; }

        /// <summary>
        /// Contains kills for which no experience has been awarded yet (because of ongoing combat, for example).
        /// The key is the challenge rating, but keep in mind the order is: CR 1/4, 1/3, 1/2, 1, 2, etc.
        /// </summary>
        public Dictionary<int, int> PendingDefeatedEncounters { get; set; } = new Dictionary<int, int>(23);

        public SavedBrawlState BrawlState { get; set; }

        [TempleDllLocation(0x1004fbd0)]
        public static SavedD20State Read(BinaryReader reader, byte[] spellPacketsData)
        {
            var result = new SavedD20State();
            ReadInitiative(reader, result);

            result.GlobalAction = SavedD20Action.Load(reader);

            result.CurrentSequenceIndex = reader.ReadInt32();

            var actionSequences = new SavedD20ActionSequence[32];
            for (var i = 0; i < actionSequences.Length; i++)
            {
                actionSequences[i] = SavedD20ActionSequence.Load(reader);
            }

            // ToEE writes additional info after the block of action sequences
            // to fixup transient references
            for (var index = 0; index < actionSequences.Length; index++)
            {
                var sequence = actionSequences[index];
                if (!sequence.IsPerforming)
                {
                    // We reset the action sequence to null here because the save does not contain enough info
                    // to recover it (nor do we care since our action array only contains active sequences)
                    actionSequences[index] = null;
                    continue;
                }

                // Read values to fix up transient values from the structure we just read
                sequence.Performer = reader.ReadObjectId();
                sequence.PreviousSequenceIndex = reader.ReadInt32();
                sequence.InterruptedSequenceIndex = reader.ReadInt32();

                for (var j = 0; j < sequence.Actions.Length; j++)
                {
                    sequence.Actions[j] = SavedD20Action.Load(reader);
                }
            }

            result.ActionSequences = actionSequences;

            result.Projectiles = SavedProjectile.LoadProjectiles(reader);

            // Read the saved readied actions
            result.ReadiedActions = new SavedReadiedAction[32];
            for (var i = 0; i < 32; i++)
            {
                result.ReadiedActions[i] = SavedReadiedAction.Load(reader);
            }

            // D20 Actions use a separate file to store their spell packets. Why? That's beyond me.
            // This might have been added later in development.
            LoadSpellPackets(spellPacketsData, result);

            // The D20 system also contained the hotkeys :|
            result.Hotkeys = SavedHotkeys.Load(reader);

            // Load encounters for which no XP has been awarded yet.
            for (var i = 0; i < 23; i++)
            {
                var count = reader.ReadInt32();
                if (count > 0)
                {
                    result.PendingDefeatedEncounters[i] = count;
                }
            }

            result.BrawlState = SavedBrawlState.Load(reader);

            return result;
        }

        private static void LoadSpellPackets(byte[] spellPacketsData, SavedD20State result)
        {
            using var spellPacketsReader = new BinaryReader(new MemoryStream(spellPacketsData));
            // Probably a version number, but it was ignored on load
            var spellPacketsHeader = spellPacketsReader.ReadInt32();
            if (spellPacketsHeader != 1)
            {
                throw new CorruptSaveException("Expected a header of 1 in the spell packets file, but got: "
                                               + spellPacketsHeader);
            }

            foreach (var sequence in result.ActionSequences)
            {
                // We already filtered out any non-performing sequence at this point
                if (sequence != null)
                {
                    sequence.Spell = SavedActiveSpell.Read(spellPacketsReader);
                }
            }
        }

        [TempleDllLocation(0x100df100)]
        private static void ReadInitiative(BinaryReader reader, SavedD20State result)
        {
            var count = reader.ReadInt32();
            result.TurnOrder = new ObjectId[count];
            for (var i = 0; i < count; i++)
            {
                result.TurnOrder[i] = reader.ReadObjectId();
            }

            result.CurrentTurnIndex = reader.ReadInt32();
        }
    }

    /// <summary>
    /// Used for the tavern brawl in Nulb.
    /// </summary>
    public class SavedBrawlState
    {

        public bool InProgress { get; set; }

        public int Status { get; set; }

        public ObjectId PlayerId { get; set; }

        public ObjectId OpponentId { get; set; }

        public static SavedBrawlState Load(BinaryReader reader)
        {
            var result = new SavedBrawlState();
            result.InProgress = reader.ReadInt32() != 0;
            result.Status = reader.ReadInt32();
            result.PlayerId = reader.ReadObjectId();
            result.OpponentId = reader.ReadObjectId();
            return result;
        }

    }

    public class SavedReadiedAction
    {
        public bool IsActive { get; set; }

        public ObjectId Interrupter { get; set; }

        public ReadyVsTypeEnum Type { get; set; }

        public static SavedReadiedAction Load(BinaryReader reader)
        {
            var result = new SavedReadiedAction();
            result.IsActive = reader.ReadInt32() != 0;
            result.Interrupter = reader.ReadObjectId();
            var readyVsType = reader.ReadInt32();
            result.Type = readyVsType switch
            {
                0 => ReadyVsTypeEnum.RV_Spell,
                1 => ReadyVsTypeEnum.RV_Counterspell,
                2 => ReadyVsTypeEnum.RV_Approach,
                3 => ReadyVsTypeEnum.RV_Withdrawal,
                _ => throw new CorruptSaveException("Unknown ready action type: " + readyVsType)
            };
            return result;
        }
    }

    public class SavedProjectile
    {
        public ObjectId ProjectileId { get; set; }

        // Index of the action sequence to which the action belongs
        public int SequenceIndex { get; set; }

        // Index within the referenced sequence
        public int ActionIndex { get; set; }

        public static SavedProjectile[] LoadProjectiles(BinaryReader reader)
        {
            // This one is REALLY stupid.
            // ToEE fwrite's a SpellProjectile struct to the save that consists ENTIRELY of transient handles/pointers
            // But it'll still use the fact whether a pointer was NULL or not to inform how many object ids to read
            // after the struct
            var projectileCount = 0;
            for (var i = 0; i < 20; i++)
            {
                reader.ReadInt32(); // This was the pointer to the D20 action
                reader.ReadInt32(); // Padding
                var projHandle = reader.ReadInt64(); // The object handle of the projectile item
                reader.ReadInt64(); // The object handle of the ammo item
                if (projHandle != 0)
                {
                    projectileCount++;
                }
            }

            var result = new SavedProjectile[projectileCount];
            for (var i = 0; i < projectileCount; i++)
            {
                result[i].ProjectileId = reader.ReadObjectId();
                result[i].SequenceIndex = reader.ReadInt32();
                result[i].ActionIndex = reader.ReadInt32();
            }

            return result;
        }
    }

    public class SavedD20Action
    {
        public D20ActionType Type { get; set; }

        public int Data { get; set; }

        public D20CAF Flags { get; set; }

        public ObjectId Performer { get; set; }

        public ObjectId Target { get; set; }

        public LocAndOffsets TargetLocation { get; set; }

        public float DistanceTraveled { get; set; }

        public int RadialMenuArg { get; set; }

        public int RollHistoryId0 { get; set; }
        public int RollHistoryId1 { get; set; }
        public int RollHistoryId2 { get; set; }

        public D20SpellData SpellData { get; set; }

        public int SpellId { get; set; }

        public int AnimActionId { get; set; }

        public static SavedD20Action Load(BinaryReader reader)
        {
            var result = new SavedD20Action();
            result.Type = (D20ActionType) reader.ReadInt32(); // TODO: Think about action type
            result.Data = reader.ReadInt32();
            result.Flags = (D20CAF) reader.ReadInt32();
            reader.ReadInt32(); // Padding
            reader.ReadInt64(); // Performer handle is transient
            reader.ReadInt64(); // Target handle is transient
            result.TargetLocation = reader.ReadLocationAndOffsets();
            result.DistanceTraveled = reader.ReadSingle();
            result.RadialMenuArg = reader.ReadInt32();
            result.RollHistoryId0 = reader.ReadInt32();
            result.RollHistoryId1 = reader.ReadInt32();
            result.RollHistoryId2 = reader.ReadInt32();
            result.SpellData = ReadSpellData(reader);
            result.SpellId = reader.ReadInt32();
            result.AnimActionId = reader.ReadInt32();
            reader.ReadInt32(); // Path pointer is transient

            // ToEE saves the actual object id's after the struct it just dumped (which contains transient data)
            result.Performer = reader.ReadObjectId();
            result.Target = reader.ReadObjectId();
            return result;
        }

        internal static D20SpellData ReadSpellData(BinaryReader reader)
        {
            // The rest is mostly packed in 4-bit numbers
            Span<byte> packedSpellData = stackalloc byte[8];
            reader.Read(packedSpellData);

            var spellEnum = BitConverter.ToInt16(packedSpellData);
            // The upper 8-bit of the spell enum will bleed into the metamagic data,
            // but since the unpack function will only consider the lower 24-bit, this doesn't matter
            var metaMagicPacked = BitConverter.ToUInt32(packedSpellData.Slice(1));
            var classCode = packedSpellData[5];
            var inventoryIndex = -1;
            if (packedSpellData[6] != 0xFF)
            {
                inventoryIndex = packedSpellData[6];
            }

            var spontCastType = (packedSpellData[7] >> 4) & 0xF;
            var spellLevel = packedSpellData[7] & 0xF;

            return new D20SpellData(spellEnum, classCode, spellLevel, inventoryIndex,
                MetaMagicData.Unpack(metaMagicPacked), (SpontCastType) spontCastType);
        }
    }

    public class SavedD20TurnBasedStatus
    {
        public HourglassState HourglassState { get; set; }

        public TurnBasedStatusFlags Flags { get; set; }

        public int IndexSth { get; set; }

        public float SurplusMoveDistance { get; set; }

        public int BaseAttackNumCode { get; set; }

        public int AttackModeCount { get; set; }

        public int NumBonusAttacks { get; set; }

        public int NumAttacks { get; set; }

        public ActionErrorCode ErrorCode { get; set; }

        public static SavedD20TurnBasedStatus Load(BinaryReader reader)
        {
            var result = new SavedD20TurnBasedStatus();
            result.HourglassState = (HourglassState) reader.ReadInt32();
            result.Flags = (TurnBasedStatusFlags) reader.ReadInt32();
            result.IndexSth = reader.ReadInt32();
            result.SurplusMoveDistance = reader.ReadSingle();
            result.BaseAttackNumCode = reader.ReadInt32();
            result.AttackModeCount = reader.ReadInt32();
            result.NumBonusAttacks = reader.ReadInt32();
            result.NumAttacks = reader.ReadInt32();
            result.ErrorCode = (ActionErrorCode) reader.ReadInt32();
            return result;
        }
    }

    public class SavedD20ActionSequence
    {
        public SavedD20Action[] Actions { get; set; } = Array.Empty<SavedD20Action>();

        public bool IsPerforming { get; set; }

        public bool IsInterrupted { get; set; }

        public int CurrentActionIndex { get; set; } = -1;

        public int PreviousSequenceIndex { get; set; } = -1;

        public int InterruptedSequenceIndex { get; set; } = -1;

        public SavedD20TurnBasedStatus TurnStatus { get; set; }

        public ObjectId Performer { get; set; }

        public LocAndOffsets PerformerLocation { get; set; }

        public bool IgnoreLineOfSight { get; set; }

        // This is loaded from a separate file
        public SavedActiveSpell Spell { get; set; }

        public static SavedD20ActionSequence Load(BinaryReader reader)
        {
            var result = new SavedD20ActionSequence();

            // NOTE: It's stupid, but ToEE completely ignores the action structs that are embedded into the
            // action sequence struct, and will actually duplicate the actions after the action sequence array.
            reader.BaseStream.Seek(32 * 0x58, SeekOrigin.Current);

            var actionCount = reader.ReadInt32();
            result.Actions = new SavedD20Action[actionCount];

            result.CurrentActionIndex = reader.ReadInt32();
            reader.ReadInt32(); // Previous Sequence (pointer, transient data)
            reader.ReadInt32(); // Interrupt Sequence (pointer, transient data)
            var flags = reader.ReadInt32();
            result.IsPerforming = ((flags & 1) != 0);
            result.IsInterrupted = ((flags & 2) != 0);
            if ((flags & ~3) != 0)
            {
                throw new CorruptSaveException($"Invalid action sequence flags: 0x{flags:X}");
            }

            result.TurnStatus = SavedD20TurnBasedStatus.Load(reader);
            reader.ReadInt64(); // Skip transient performer object handle
            result.PerformerLocation = reader.ReadLocationAndOffsets();
            reader.ReadInt64(); // Skip transient target object handle
            // Skip the spell packet body, because ToEE was lazy and just saved the structure directly,
            // the packet only contains invalid object handles (since they are transient) and not the object ids
            // that are required to restore the handles. Instead of fixing this by saving information after the
            // sequences, ToEE saves the spell packets in action_sequencespellpackets.bin instead.
            reader.ReadBytes(0xAE8);
            reader.ReadInt32(); // Transient action pointer
            result.IgnoreLineOfSight = reader.ReadInt32() != 0;

            return result;
        }
    }

    public class SavedHotkey
    {
        public DIK Key { get; set; }

        // ELF32 hash of the radial menu entry's text that is triggered by this hotkey
        public int TextHash { get; set; }

        public D20ActionType ActionType { get; set; }

        public int ActionData { get; set; }

        public D20SpellData SpellData { get; set; }

        // The original text of the hotkey entry, but somehow this doesn't always match the ELF32 hash above
        public string Text { get; set; }
    }

    public class SavedHotkeys
    {
        public Dictionary<DIK, SavedHotkey> Hotkeys { get; set; } = new Dictionary<DIK, SavedHotkey>();

        public static SavedHotkeys Load(BinaryReader reader)
        {
            var hotkeys = new SavedHotkeys();

            for (var keyIndex = reader.ReadInt32(); keyIndex != -1; keyIndex = reader.ReadInt32())
            {
                var hotkey = new SavedHotkey();

                // Originally it just read the radial menu entry, but that contains so much
                // stale data it's not even funny.
                // The hotkey system will search the entire radial menu and compare each entry
                // against the following fields found in the hotkey system's copied radial menu entry:
                // - D20 action type
                // - D20 action data
                // - D20 spell data, spell enum original
                // - The upper 16 bit of the metamagic data
                // - Text Hash for certain action types
                reader.ReadInt32(); // Stale text pointer
                reader.ReadInt32(); // Stale text2 pointer
                hotkey.TextHash = reader.ReadInt32(); // Text elfhash
                reader.ReadInt32(); // Padding
                reader.ReadInt32(); // Radial menu entry type
                reader.ReadInt32(); // min arg
                reader.ReadInt32(); // max arg
                reader.ReadInt32(); // Stale actual arg pointer
                var hotkeyActionType = reader.ReadInt32();
                hotkey.ActionData = reader.ReadInt32();
                reader.ReadInt32(); // Action CAF
                hotkey.SpellData = SavedD20Action.ReadSpellData(reader);
                reader.ReadInt32(); // Dispatcher key
                reader.ReadInt32(); // Callback pointer
                reader.ReadInt32(); // Flags
                reader.ReadInt32(); // Help text hash
                reader.ReadInt32(); // Spell Id

                hotkey.Text = reader.ReadFixedString(128);

                if (keyIndex >= AssignableKeys.Length)
                {
                    throw new CorruptSaveException($"Hotkey table contains key which is outside range: {keyIndex}");
                }

                hotkey.Key = AssignableKeys[keyIndex];

                if (hotkeyActionType == -2)
                {
                    continue; // Unassigned
                }

                hotkey.ActionType = (D20ActionType) hotkeyActionType;

                if (!hotkeys.Hotkeys.TryAdd(hotkey.Key, hotkey))
                {
                    throw new CorruptSaveException($"Duplicate assignment to key {hotkey.Key}");
                }
            }

            return hotkeys;
        }

        /// <summary>
        /// ToEE maintains an internal list of the re-bindable keys. The index of a key in this table matches
        /// the index of the assigned action in the hotkey table.
        /// </summary>
        [TempleDllLocation(0x102E8B78)]
        private static readonly DIK[] AssignableKeys =
        {
            DIK.DIK_Q, DIK.DIK_W, DIK.DIK_E, DIK.DIK_R, DIK.DIK_T, DIK.DIK_Y, DIK.DIK_U, DIK.DIK_I, DIK.DIK_O,
            DIK.DIK_P, DIK.DIK_LBRACKET, DIK.DIK_RBRACKET, DIK.DIK_A, DIK.DIK_S, DIK.DIK_D, DIK.DIK_F, DIK.DIK_G,
            DIK.DIK_H,
            DIK.DIK_J, DIK.DIK_K, DIK.DIK_L, DIK.DIK_SEMICOLON, DIK.DIK_APOSTROPHE, DIK.DIK_BACKSLASH,
            DIK.DIK_Z, DIK.DIK_X, DIK.DIK_C, DIK.DIK_V, DIK.DIK_B, DIK.DIK_N, DIK.DIK_M, DIK.DIK_COMMA, DIK.DIK_PERIOD,
            DIK.DIK_SLASH, DIK.DIK_F11, DIK.DIK_F12, DIK.DIK_F13, DIK.DIK_F14, DIK.DIK_F15
        };
    }
}