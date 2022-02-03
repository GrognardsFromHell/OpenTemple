using System;
using System.IO;

namespace OpenTemple.Core.IO.SaveGames.UiState;

public class SavedUiState
{
    private const uint Sentinel = 0xBEEFCAFEu;

    // Initialized here because it's unused (and never set)
    public SavedRandomEncounterUiState RandomEncounterState { get; set; } = new SavedRandomEncounterUiState();
    public SavedDialogUiState DialogState { get; set; }
    public SavedLogbookUiState LogbookState { get; set; }
    public SavedTownmapUiState TownmapState { get; set; }
    public SavedWorldmapUiState WorldmapState { get; set; }
    public SavedPartyPoolUiState PartyPoolState { get; set; }
    public SavedCampingUiState CampingState { get; set; }
    public SavedHelpManagerUiState HelpManagerState { get; set; }

    [TempleDllLocation(0x101154b0)]
    public static SavedUiState Load(byte[] buffer)
    {
        using var reader = new BinaryReader(new MemoryStream(buffer));

        var result = new SavedUiState();

        var version = reader.ReadInt32();
        if (version != 0)
        {
            throw new CorruptSaveException($"Expected saved UI state version 0, but got: {version}");
        }

        SkipSentinel(reader); // Intgame UI
        SkipSentinel(reader); // Anim UI
        result.RandomEncounterState = LoadState(reader, SavedRandomEncounterUiState.Read);
        result.DialogState = LoadState(reader, SavedDialogUiState.Read);
        result.LogbookState = LoadState(reader, SavedLogbookUiState.Read);
        result.TownmapState = LoadState(reader, SavedTownmapUiState.Read);
        result.WorldmapState = LoadState(reader, SavedWorldmapUiState.Read);
        result.PartyPoolState = LoadState(reader, SavedPartyPoolUiState.Read);
        SkipSentinel(reader); // Party
        SkipSentinel(reader); // Formation
        result.CampingState = LoadState(reader, SavedCampingUiState.Read);
        SkipSentinel(reader); // Help Inventory UI
        SkipSentinel(reader); // UI Manager
        result.HelpManagerState = LoadState(reader, SavedHelpManagerUiState.Read);

        return result;
    }

    [TempleDllLocation(0x101152f0)]
    public static byte[] Save(SavedUiState uiState, bool co8Extensions)
    {
        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);

        writer.WriteInt32(0); // Save version number

        writer.WriteUInt32(Sentinel); // Intgame UI
        writer.WriteUInt32(Sentinel); // Anim UI
        WriteState(writer, uiState.RandomEncounterState.Write);
        WriteState(writer, uiState.DialogState.Write);
        WriteState(writer, uiState.LogbookState.Write);
        WriteState(writer, uiState.TownmapState.Write);
        WriteState(writer, writer => uiState.WorldmapState.Write(writer, co8Extensions));
        WriteState(writer, uiState.PartyPoolState.Write);
        writer.WriteUInt32(Sentinel); // Party
        writer.WriteUInt32(Sentinel); // Formation
        WriteState(writer, uiState.CampingState.Write);
        writer.WriteUInt32(Sentinel); // Help Inventory UI
        writer.WriteUInt32(Sentinel); // UI Manager
        WriteState(writer, uiState.HelpManagerState.Write);

        writer.Flush();
        return stream.ToArray();
    }

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

    private static void WriteState(BinaryWriter writer, Action<BinaryWriter> stateWriter)
    {
        stateWriter(writer);
        writer.WriteUInt32(Sentinel);
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

}