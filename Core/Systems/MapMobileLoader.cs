using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.SaveGames;
using OpenTemple.Core.Logging;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems;

/// <summary>
/// Loads the mobile objects for a map.
/// </summary>
public class MapMobileLoader
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    public const string MobileDifferencesFile = "mobile.md";

    public const string DynamicMobilesFile = "mobile.mdy";

    public const string DestroyedMobilesFile = "mobile.des";

    private readonly IFileSystem _fs;

    private readonly Dictionary<ObjectId, GameObject> _mobiles = new();

    public IEnumerable<GameObject> Mobiles => _mobiles.Values;

    public MapMobileLoader(IFileSystem fs)
    {
        _fs = fs;
    }

    private void AddMobile(GameObject mobile)
    {
        if (!mobile.id.IsPermanent)
        {
            throw new InvalidOperationException($"Mobile has a non-permanent ID: {mobile.id}");
        }

        if (!_mobiles.TryAdd(mobile.id, mobile))
        {
            throw new InvalidOperationException($"Mobile ID collision: {mobile.id}");
        }
    }

    public void Load(string dataDir, string saveDir)
    {
        _mobiles.Clear();

        LoadMapData(dataDir);

        if (saveDir != null && Directory.Exists(saveDir))
        {
            LoadSaveData(saveDir);
        }
    }

    private void LoadMapData(string dataDir)
    {
        // Read all mobiles that shipped with the game files
        Logger.Info("Loading map mobiles from {0}", dataDir);

        var count = 0;
        foreach (var filename in _fs.Search(dataDir + "/*.mob"))
        {
            using var reader = Tig.FS.OpenBinaryReader(filename);
            try
            {
                AddMobile(GameObject.Load(reader));
                count++;
            }
            catch (Exception e)
            {
                Logger.Warn("Unable to load mobile object {0} for level {1}: {2}", filename, dataDir, e);
            }
        }

        Logger.Info("Loaded {0} map mobiles", count);
    }

    private void LoadSaveData(string saveDir)
    {
        LoadDiffs(saveDir);

        LoadDestroyedList(saveDir);

        LoadDynamicMobiles(saveDir);
    }

    private void LoadDiffs(string saveDir)
    {
        // Read all mobile differences that have accumulated for this map in the save dir
        var diffFilename = Path.Join(saveDir, MobileDifferencesFile);

        if (!File.Exists(diffFilename))
        {
            Logger.Info("Skipping mobile diffs, because {0} is missing", diffFilename);
            return;
        }

        Logger.Info("Loading mobile diffs from {0}", diffFilename);

        using var reader = new BinaryReader(new FileStream(diffFilename, FileMode.Open));

        var count = 0;
        var removeCount = 0;
        while (!reader.AtEnd())
        {
            var objId = reader.ReadObjectId();

            // Get the active handle for the mob so we can apply diffs to it
            if (!_mobiles.TryGetValue(objId, out var obj))
            {
                throw new CorruptSaveException(
                    $"{diffFilename} contains diffs for non-existant object {objId}"
                );
            }

            obj.LoadDiffsFromFile(reader);
            count++;

            if (obj.HasFlag(ObjectFlag.EXTINCT))
            {
                Logger.Debug("{0} ({1}) is destroyed.", obj, objId);
                _mobiles.Remove(obj.id);
                removeCount++;
            }
        }

        Logger.Info("Loaded diffs for {0} mobiles ({1} destroyed)", count, removeCount);
    }

    private void LoadDestroyedList(string saveDir)
    {
        // Destroy all mobiles that had previously been destroyed
        var desFilename = Path.Join(saveDir, DestroyedMobilesFile);

        if (!File.Exists(desFilename))
        {
            Logger.Info("Skipping destroyed mobile files, because {0} is missing", desFilename);
            return;
        }

        Logger.Info("Loading destroyed mobile file from {0}", desFilename);

        using var reader = new BinaryReader(new FileStream(desFilename, FileMode.Open));
        var count = 0;

        while (!reader.AtEnd())
        {
            var objId = reader.ReadObjectId();
            if (_mobiles.TryGetValue(objId, out var obj))
            {
                Logger.Debug("{0} ({1}) is destroyed.", obj, objId);
                _mobiles.Remove(objId);
            }

            count++;
        }

        Logger.Info("Done loading {0} destroyed map mobiles", count);
    }

    [TempleDllLocation(0x10070610)]
    private void LoadDynamicMobiles(string saveDir)
    {
        var filename = Path.Join(saveDir, DynamicMobilesFile);

        if (!File.Exists(filename))
        {
            Logger.Info("Skipping dynamic mobiles because {0} doesn't exist.", filename);
            return;
        }

        Logger.Info("Loading dynamic mobiles from {0}", filename);

        using var reader = new BinaryReader(new FileStream(filename, FileMode.Open));

        int count = 0;
        while (!reader.AtEnd())
        {
            try
            {
                var obj = GameObject.Load(reader);
                Logger.Debug("Loaded object {0}", obj);
                AddMobile(obj);
                count++;
            }
            catch (Exception e)
            {
                Logger.Error("Unable to load object: {0}", e);
                break;
            }
        }

        if (!reader.AtEnd())
        {
            throw new Exception($"Error while reading dynamic mobile file {filename}");
        }

        Logger.Info("Done reading {0} dynamic mobiles.", count);
    }
}