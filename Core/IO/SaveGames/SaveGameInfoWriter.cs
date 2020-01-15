using System;
using System.IO;
using OpenTemple.Core.Systems;

namespace OpenTemple.Core.IO.SaveGames
{
    public static class SaveGameInfoWriter
    {
        public static void Write(string path, SaveGameInfo info)
        {
            // Open the GSI File and read the metadata
            using var writer = new BinaryWriter(new FileStream(path, FileMode.Create));

            writer.WriteInt32(0); // Unknown purpose (version most likely)
            writer.WritePrefixedString(info.ModuleName);
            writer.WritePrefixedString(info.LeaderName);
            writer.WriteInt32(info.MapId);

            writer.WriteGameTime(info.GameTime);
            writer.WriteInt32(info.LeaderPortrait);
            writer.WriteInt32(info.LeaderLevel);
            writer.WriteTileLocation(info.LeaderLoc);
            writer.WriteInt32(0); // Story state is unused in ToEE (Used in Arkanum)
            writer.WritePrefixedString(info.Name);
        }
    }
}