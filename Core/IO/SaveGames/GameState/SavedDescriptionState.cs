using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace OpenTemple.Core.IO.SaveGames.GameState;

public class SavedDescriptionState
{
    public List<string> CustomNames { get; set; } = new List<string>();

    [TempleDllLocation(0x100868b0)]
    public static SavedDescriptionState Read(BinaryReader reader)
    {
        var customNameCount = reader.ReadInt32();

        var result = new SavedDescriptionState();
        result.CustomNames.Capacity = customNameCount;
        for (var i = 0; i < customNameCount; i++)
        {
            result.CustomNames.Add(reader.ReadPrefixedString());
        }

        return result;
    }

    [TempleDllLocation(0x10086810)]
    [SuppressMessage("ReSharper", "RedundantCast")]
    public void Write(BinaryWriter writer)
    {
        writer.WriteInt32( CustomNames.Count);
        foreach (var customName in CustomNames)
        {
            writer.WritePrefixedString(customName);
        }
    }
}