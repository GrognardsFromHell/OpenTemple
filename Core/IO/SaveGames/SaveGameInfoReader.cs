using System;
using System.IO;
using OpenTemple.Core.Systems;

namespace OpenTemple.Core.IO.SaveGames;

public static class SaveGameInfoReader
{
    public static bool TryParseFilename(string path, out SaveGameType type, out int slotNumber)
    {
        type = default;
        slotNumber = -1;

        var basename = Path.GetFileNameWithoutExtension(path);
        if (basename == "SlotQwik")
        {
            type = SaveGameType.QuickSave;
        }
        else if (basename == "SlotAuto")
        {
            type = SaveGameType.AutoSave;
        }
        else if (basename.StartsWith("slot"))
        {
            type = SaveGameType.Normal;
            var slotNumberStr = basename.Substring(4, 4);
            if (!int.TryParse(slotNumberStr, out slotNumber))
            {
                return false;
            }
        }
        else if (basename.StartsWith("iron"))
        {
            type = SaveGameType.IronMan;
            var slotNumberStr = basename.Substring(4, 4);
            if (!int.TryParse(slotNumberStr, out slotNumber))
            {
                return false;
            }
        }
        else
        {
            return false;
        }

        return true;
    }

    public static SaveGameInfo? Read(string path)
    {
        if (!TryParseFilename(path, out var type, out var slotNumber))
        {
            return null;
        }

        var info = new SaveGameInfo();
        info.Path = path;
        info.Type = type;
        info.Slot = slotNumber;
        info.LastModified = File.GetLastWriteTime(path);

        // Open the GSI File and read the metadata
        using var reader = new BinaryReader(new FileStream(path, FileMode.Open));

        reader.ReadInt32(); // Unknown purpose
        info.ModuleName = reader.ReadPrefixedString();
        info.LeaderName = reader.ReadPrefixedString();
        info.MapId = reader.ReadInt32();

        var days = reader.ReadInt32();
        var milliseconds = reader.ReadInt32();
        info.GameTime = new GameTime(days, milliseconds);
        info.LeaderPortrait = reader.ReadInt32();
        info.LeaderLevel = reader.ReadInt32();
        info.LeaderLoc = reader.ReadTileLocation();
        reader.ReadInt32(); // Story state is unused in ToEE (Used in Arkanum)
        info.Name = reader.ReadPrefixedString();

        var dirName = Path.GetDirectoryName(path);
        info.BasePath = type switch
        {
            SaveGameType.Normal => Path.Join(dirName, $"slot{slotNumber:D4}"),
            SaveGameType.QuickSave => Path.Join(dirName, "SlotQwik"),
            SaveGameType.AutoSave => Path.Join(dirName, "SlotAuto"),
            SaveGameType.IronMan => Path.Join(dirName, $"iron{slotNumber:D4}"),
            _ => throw new ArgumentOutOfRangeException()
        };

        var smallScreenshotPath = info.BasePath + "s.jpg";
        if (File.Exists(smallScreenshotPath))
        {
            info.SmallScreenshotPath = smallScreenshotPath;
        }

        var largeScreenshotPath = info.BasePath + "l.jpg";
        if (File.Exists(largeScreenshotPath))
        {
            info.LargeScreenshotPath = largeScreenshotPath;
        }

        return info;
    }
}