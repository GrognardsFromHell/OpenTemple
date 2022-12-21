using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using OpenTemple.Core.Logging;
using SharpDX.Direct3D11;

namespace OpenTemple.Core.IO.SaveGames.GameState;

public class SavedGameState
{
    private const uint Sentinel = 0xBEEFCAFEu;

    public int SaveVersion { get; set; }

    public bool IsIronmanSave { get; set; }

    public int IronmanSlotNumber { get; set; }

    public string? IronmanSaveName { get; set; }

    public SavedDescriptionState DescriptionState { get; set; }

    // Initializing here so the game can be saved without setting it (it's unused)
    public SavedSectorState SectorState { get; set; } = new();

    // Initializing here so the game can be saved without setting it (it's unused)
    public SavedSkillState SkillState { get; set; } = new();

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

    public SavedGroupSelections SavedGroupsState { get; set; }

    public SavedD20State D20State { get; set; }

    public SavedObjFadeState ObjFadeState { get; set; }

    public SavedD20RollsState D20RollsState { get; set; }

    public SavedSecretDoorState SecretDoorState { get; set; }

    public SavedObjectEventState ObjectEventState { get; set; }

    public SavedFormationState FormationState { get; set; }

    private static T LoadState<T>(BinaryReader reader, Func<BinaryReader, T> stateReader)
    {
        var posBeforeState = reader.BaseStream.Position;
        var state = stateReader(reader);
        var posAfterState = reader.BaseStream.Position;

        var sentinel = reader.ReadUInt32();
        if (sentinel != Sentinel)
        {
            throw new CorruptSaveException($"Read sentinel 0x{sentinel:X} at pos {posAfterState} for " +
                                           $"state {typeof(T)} (which started at {posBeforeState})");
        }

        return state;
    }

    private static void SaveState(BinaryWriter writer, Action<BinaryWriter> stateSaver)
    {
        stateSaver(writer);
        writer.Write(Sentinel);
    }

    private static void SkipSentinel(BinaryReader reader)
    {
        var posAfterState = reader.BaseStream.Position;

        var sentinel = reader.ReadUInt32();
        if (sentinel != Sentinel)
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
        result.TimeEventState = LoadState(reader, SavedTimeEventState.Load);
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
        SkipSentinel(reader); // Random encounter system
        result.ObjectEventState = LoadState(reader, SavedObjectEventState.Read);
        result.FormationState = LoadState(reader, SavedFormationState.Read);

        result.MapFleeState = SavedMapFleeState.Load(mapFleeData);
        result.SavedGroupsState = SavedGroupSelections.Load(partyConfigData);

        // Check that there's no trailing data remaining
        var remainingBytes = reader.BaseStream.Length - reader.BaseStream.Position;
        if (!reader.AtEnd())
        {
            throw new CorruptSaveException($"There are {remainingBytes} bytes at the end of the game " +
                                           $"state file remaining");
        }

        return result;
    }

    [SuppressMessage("ReSharper", "RedundantCast")]
    private void Save(BinaryWriter writer, BinaryWriter spellPacketData,
        BinaryWriter partyConfigData, BinaryWriter mapFleeData, bool co8Extensions)
    {
        if (SaveVersion != 0)
        {
            throw new CorruptSaveException("Save game version mismatch error. Expected 0, is " +
                                           SaveVersion);
        }
        writer.WriteInt32( SaveVersion);

        writer.WriteInt32( (IsIronmanSave ? 1 : 0));
        if (IsIronmanSave)
        {
            writer.WriteInt32( IronmanSlotNumber);
            writer.WritePrefixedString(IronmanSaveName);
        }

        // Save the individual state blocks (which was done by the game systems themselves before)

        SaveState(writer, DescriptionState.Write);
        SaveState(writer, SectorState.Write);
        SaveState(writer, SkillState.Write);
        SaveState(writer, ScriptState.Write);
        // The map system would read the spells (Also note that none of the map subsystems had their own load functions)
        MapState.Write(writer);
        SaveState(writer, SpellState.Write);
        SaveState(writer, LightSchemeState.Write);
        // The player subsystem had an empty load hook (0x101f5850), but we still need to skip the sentinel
        writer.Write(Sentinel);
        SaveState(writer, w => AreaState.Write(w, co8Extensions));
        SaveState(writer, SoundGameState.Write);
        SaveState(writer, CombatState.Write);
        SaveState(writer, TimeEventState.Write);
        // The rumor subsystem had an empty load hook (0x101f5850), but we still need to skip the sentinel
        writer.Write(Sentinel);
        SaveState(writer, QuestsState.Write);
        SaveState(writer, AnimState.Write);
        SaveState(writer, ReputationState.Write);
        SaveState(writer, MonsterGenState.Write);
        SaveState(writer, PartyState.Write);
        SaveState(writer, w => D20State.Write(w, spellPacketData));
        SaveState(writer, ObjFadeState.Write);
        SaveState(writer, D20RollsState.Write);
        SaveState(writer, SecretDoorState.Write);
        writer.Write(Sentinel); // Random encounter system
        SaveState(writer, ObjectEventState.Write);
        SaveState(writer, FormationState.Write);

        MapFleeState.Save(mapFleeData);
        SavedGroupsState.Save(partyConfigData);
    }

    public static SerializedGameState Save(SavedGameState gameState, bool co8Extensions)
    {
        MemoryStream mainContent = new MemoryStream();
        BinaryWriter mainWriter = new BinaryWriter(mainContent);

        MemoryStream spellPackets = new MemoryStream();
        BinaryWriter spellPacketWriter = new BinaryWriter(spellPackets);

        MemoryStream partyConfig = new MemoryStream();
        BinaryWriter partyConfigWriter = new BinaryWriter(partyConfig);

        MemoryStream mapFlee = new MemoryStream();
        BinaryWriter mapFleeWriter = new BinaryWriter(mapFlee);

        gameState.Save(mainWriter, spellPacketWriter, partyConfigWriter, mapFleeWriter, co8Extensions);

        mainWriter.Flush();
        spellPacketWriter.Flush();
        partyConfigWriter.Flush();
        mapFleeWriter.Flush();

        return new SerializedGameState(
            mainContent.ToArray(),
            spellPackets.ToArray(),
            partyConfig.ToArray(),
            mapFlee.ToArray()
        );
    }

}

public class SerializedGameState
{
    // Content of data.sav
    public byte[] MainContent { get; }

    // Content of action_sequencespellpackets.bin
    public byte[] ActionSequenceSpells { get; }

    // Content of partyconfig.bin, which translates to saved groups
    public byte[] PartyConfig { get; }

    // Content of map_mapflee.bin
    public byte[] FleeData { get; }

    public SerializedGameState(byte[] mainContent, byte[] actionSequenceSpells, byte[] partyConfig, byte[] fleeData)
    {
        MainContent = mainContent;
        ActionSequenceSpells = actionSequenceSpells;
        PartyConfig = partyConfig;
        FleeData = fleeData;
    }
}