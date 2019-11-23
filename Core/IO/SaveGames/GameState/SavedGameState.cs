using System;
using System.IO;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20;

namespace SpicyTemple.Core.IO.SaveGames.GameState
{
    public class SavedGameState
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        public int SaveVersion { get; set; } = 0;

        public bool IsIronmanSave { get; set; }

        public int IronmanSlotNumber { get; set; }

        public string IronmanSaveName { get; set; }

        public SavedDescriptionState DescriptionState { get; set; }

        public SavedSectorState SectorState { get; set; }

        public SavedSkillState SkillState { get; set; }

        public SavedScriptState ScriptState { get; set; }

        public SavedMapState MapState { get; set; }

        public SavedMapFleeState MapFleeState { get; set; }

        public SavedSpellState SpellState { get; set; }

        public SavedLightSchemeState LightSchemeState { get; set; }

        public SavedAreaState AreaState { get; set; }

        public SavedSoundGameState SoundGameState { get; set; }

        public SavedCombatState CombatState { get; set; }

        public SavedTimeEventState TimeEventState { get; set; }

        public SavedQuestsState QuestsState { get; set; }

        public SavedAnimState AnimState { get; set; }

        public SavedReputationState ReputationState { get; set; }

        public SavedMonsterGenState MonsterGenState { get; set; }

        public SavedPartyState PartyState { get; set; }

        public SavedGroupSelections GroupSelections { get; set; }

        public SavedD20State D20State { get; set; }

        public SavedObjFadeState ObjFadeState { get; set; }

        public SavedD20RollsState D20RollsState { get; set; }

        public SavedSecretDoorState SecretDoorState { get; set; }

        public SavedRandomEncounterState RandomEncounterState { get; set; }

        public SavedObjectEventState ObjectEventState { get; set; }

        public SavedFormationState FormationState { get; set; }

        private static T LoadState<T>(BinaryReader reader, Func<BinaryReader, T> stateReader)
        {
            var posBeforeState = reader.BaseStream.Position;
            var state = stateReader(reader);
            var posAfterState = reader.BaseStream.Position;

            var sentinel = reader.ReadUInt32();
            if (sentinel != 0xBEEFCAFEu)
            {
                throw new CorruptSaveException($"Read sentinel 0x{sentinel:X} at pos {posAfterState} for " +
                                               $"state {typeof(T)} (which started at {posBeforeState})");
            }

            return state;
        }

        private static void SkipSentinel(BinaryReader reader)
        {
            var posAfterState = reader.BaseStream.Position;

            var sentinel = reader.ReadUInt32();
            if (sentinel != 0xBEEFCAFEu)
            {
                throw new CorruptSaveException($"Read sentinel 0x{sentinel:X} at pos {posAfterState}");
            }
        }

        public static SavedGameState Load(byte[] binaryData, byte[] spellPacketData,
            byte[] partyConfigData, byte[] mapFleeData)
        {
            using var reader = new BinaryReader(new MemoryStream(binaryData));

            var result = new SavedGameState();

            result.SaveVersion = reader.ReadInt32();
            if (result.SaveVersion != 0)
            {
                throw new CorruptSaveException("Save game version mismatch error. Expected 0, read " +
                                               result.SaveVersion);
            }

            result.IsIronmanSave = reader.ReadInt32() != 0;
            if (result.IsIronmanSave)
            {
                result.IronmanSlotNumber = reader.ReadInt32();
                result.IronmanSaveName = reader.ReadPrefixedString();
            }

            // Read the individual state blocks (which was done by the game systems themselves before)
            result.DescriptionState = LoadState(reader, SavedDescriptionState.Read);
            result.SectorState = LoadState(reader, SavedSectorState.Read);
            result.SkillState = LoadState(reader, SavedSkillState.Read);
            result.ScriptState = LoadState(reader, SavedScriptState.Read);
            // The map system would read the spells (Also note that none of the map subsystems had their own load functions)
            result.MapState = SavedMapState.Read(reader);
            result.SpellState = LoadState(reader, SavedSpellState.Read);
            result.LightSchemeState = LoadState(reader, SavedLightSchemeState.Read);
            // The player subsystem had an empty load hook (0x101f5850), but we still need to skip the sentinel
            SkipSentinel(reader);
            result.AreaState = SavedAreaState.Read(reader);
            result.SoundGameState = LoadState(reader, SavedSoundGameState.Read);
            result.CombatState = LoadState(reader, SavedCombatState.Read);
            result.TimeEventState = LoadState(reader, SavedTimeEventState.Read);
            // The rumor subsystem had an empty load hook (0x101f5850), but we still need to skip the sentinel
            SkipSentinel(reader);
            result.QuestsState = LoadState(reader, SavedQuestsState.Read);
            result.AnimState = LoadState(reader, SavedAnimState.Read);
            result.ReputationState = LoadState(reader, SavedReputationState.Read);
            result.MonsterGenState = LoadState(reader, SavedMonsterGenState.Read);
            result.PartyState = LoadState(reader, SavedPartyState.Read);
            result.D20State = LoadState(reader, r => SavedD20State.Read(r, spellPacketData));
            result.ObjFadeState = LoadState(reader, SavedObjFadeState.Read);
            result.D20RollsState = LoadState(reader, SavedD20RollsState.Read);
            result.SecretDoorState = LoadState(reader, SavedSecretDoorState.Read);
            result.RandomEncounterState = LoadState(reader, SavedRandomEncounterState.Read);
            result.ObjectEventState = LoadState(reader, SavedObjectEventState.Read);
            result.FormationState = LoadState(reader, SavedFormationState.Read);

            result.MapFleeState = SavedMapFleeState.Load(mapFleeData);
            result.GroupSelections = SavedGroupSelections.Load(partyConfigData);

            // Check that there's no trailing data remaining
            var remainingBytes = reader.BaseStream.Length - reader.BaseStream.Position;
            if (!reader.AtEnd())
            {
                throw new CorruptSaveException($"There are {remainingBytes} bytes at the end of the game " +
                                               $"state file remaining");
            }

            return result;
        }
    }
}