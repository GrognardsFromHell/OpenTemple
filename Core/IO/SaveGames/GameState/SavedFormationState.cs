using System.Diagnostics;
using System.IO;

namespace OpenTemple.Core.IO.SaveGames.GameState;

public class SavedFormationState
{
    public int FormationSelected { get; set; }

    public int[][] Positions { get; set; }

    [TempleDllLocation(0x100432e0)]
    public static SavedFormationState Read(BinaryReader reader)
    {
        var result = new SavedFormationState();
        result.FormationSelected = reader.ReadInt32();

        var formationCount = reader.ReadInt32();
        if (formationCount != 4)
        {
            throw new CorruptSaveException($"Expected 4 formations, but found: {formationCount}");
        }

        result.Positions = new int[formationCount][];
        for (var i = 0; i < formationCount; i++)
        {
            var positions = new int[8];
            for (var j = 0; j < 8; j++)
            {
                positions[j] = reader.ReadInt32();
            }

            result.Positions[i] = positions;
        }

        return result;
    }

    [TempleDllLocation(0x10043270)]
    public void Write(BinaryWriter writer)
    {
        writer.WriteInt32(FormationSelected);

        // Always saved 4 formations
        Trace.Assert(Positions.Length == 4);
        writer.WriteInt32(Positions.Length);

        foreach (var formation in Positions)
        {
            Trace.Assert(formation.Length == 8);
            for (var j = 0; j < 8; j++)
            {
                writer.WriteInt32(formation[j]);
            }
        }
    }
}