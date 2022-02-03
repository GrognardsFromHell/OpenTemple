using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using OpenTemple.Core;
using OpenTemple.Core.Config;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.SaveGames;
using OpenTemple.Core.IO.SaveGames.Archive;
using OpenTemple.Core.IO.TroikaArchives;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.Protos;
using OpenTemple.Core.TigSubsystems;

namespace DumpSaveObjectTable;

static class Program
{
    static int Main(string[] args)
    {
        // Create a root command with some options
        var rootCommand = new RootCommand
        {
            new Argument<DirectoryInfo>("toee-dir")
            {
                Arity = ArgumentArity.ExactlyOne,
                Description = "ToEE Installation Directory"
            },
            new Argument<FileInfo>("file")
            {
                Arity = ArgumentArity.ExactlyOne,
                Description = "ToEE GSI Save File"
            }
        };

        rootCommand.Description = "Loads a save game and dumps a table of all objects across all maps.";

        rootCommand.Handler =
            CommandHandler.Create<DirectoryInfo, FileInfo>((toeeDir, file) =>
                DumpObjectTable(toeeDir.FullName, file.FullName));

        return rootCommand.InvokeAsync(args).Result;
    }

    private static void DumpObjectTable(string toeePath, string gsiPath)
    {
        using var vfs = TroikaVfs.CreateFromInstallationDir(toeePath);
        Tig.FS = vfs;

        var sgi = SaveGameInfoReader.Read(gsiPath);
        Console.WriteLine($"Loading {sgi.Name}");

        var tempDir = Path.GetTempFileName();
        File.Delete(tempDir);
        Directory.CreateDirectory(tempDir);

        try
        {
            var tfaiFile = sgi.BasePath + ".tfai";
            var tfafFile = sgi.BasePath + ".tfaf";

            ExtractSaveArchive.Extract(tfaiFile, tfafFile, tempDir);

            DumpObjectTableForSaveDir(tempDir);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    private static void DumpObjectTableForSaveDir(string saveDir)
    {
        // Objects are only stored in maps
        var mapList = MapListParser.Parse(Tig.FS);

        var loader = new MapMobileLoader(Tig.FS);

        using var mapMobileIndex = new StreamWriter("map_mobiles.txt");

        foreach (var mapListEntry in mapList)
        {
            var mapName = mapListEntry.Value.name;

            var mapDataDir = $"maps/{mapName}";
            var mapSaveDir = Path.Join(saveDir, "maps", mapName);

            loader.Load(mapDataDir, mapSaveDir);

            mapMobileIndex.WriteLine("-----------------------------------------------------------------");
            mapMobileIndex.WriteLine(mapName);
            mapMobileIndex.WriteLine("-----------------------------------------------------------------");

            foreach (var mobile in loader.Mobiles)
            {
                mapMobileIndex.WriteLine(mobile.id + " " + mobile);
            }
        }
    }
}