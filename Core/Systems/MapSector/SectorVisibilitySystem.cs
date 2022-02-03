using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using OpenTemple.Core.Logging;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.Systems.MapSector;

public class SectorVisibilitySystem : IGameSystem, IResetAwareSystem
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    [TempleDllLocation(0x11868FA0)]
    private string _dataDir;

    [TempleDllLocation(0x118690C0)]
    private string _saveDir;

    private readonly Dictionary<SectorLoc, SectorVisibility> _cache;

    // Previously mask 0x10 on the bitfield (which was always 0)
    [TempleDllLocation(0x10BCA378)]
    private bool _useSaveFolder;

    [TempleDllLocation(0x100aa5c0)]
    public SectorVisibilitySystem()
    {
        _cache = new Dictionary<SectorLoc, SectorVisibility>();
    }

    [TempleDllLocation(0x100aaa80)]
    public void Dispose()
    {
        Flush();
    }

    [TempleDllLocation(0x100aa430)]
    public void SetDirectories(string dataDir, string saveDir)
    {
        _dataDir = dataDir;
        _saveDir = saveDir;
    }

    [TempleDllLocation(0x100aaa80)]
    public void Flush()
    {
        foreach (var sectorVisibility in _cache.Values)
        {
            if (sectorVisibility.IsDirty && _useSaveFolder)
            {
                var path = Path.Join(_saveDir, GetSvbFilename(sectorVisibility.Location));
                using var writer = new BinaryWriter(new FileStream(path, FileMode.Create));
                sectorVisibility.Save(writer);
            }
        }

        _cache.Clear();
    }

    [TempleDllLocation(0x100aa630)]
    public void Reset()
    {
        _cache.Clear();
    }

    [TempleDllLocation(0x100aaaa0)]
    public SectorVisibility Lock(SectorLoc location)
    {
        var visibility = Get(location);
        visibility.UsageCount++;
        return visibility;
    }

    [TempleDllLocation(0x100aa470)]
    public void Unlock(SectorLoc location)
    {
        var visibility = Get(location);
        visibility.UsageCount--;
    }

    [TempleDllLocation(0x100aa650)]
    private SectorVisibility Get(SectorLoc location)
    {
        if (_cache.TryGetValue(location, out var cachedVisibility))
        {
            cachedVisibility.MarkUsed();
            return cachedVisibility;
        }

        // Always create a new one
        var visibility = new SectorVisibility(location);
        _cache[location] = visibility;

        var svbFilename = GetSvbFilename(location);
        if (_useSaveFolder)
        {
            // Flag 0x10 seems to mean "allow loading from the save directory", which is inactive for vanilla
            var svgSavePath = Path.Join(_saveDir, svbFilename);
            if (File.Exists(svgSavePath))
            {
                using var reader = new BinaryReader(new FileStream(svgSavePath, FileMode.Open));
                if (!visibility.Load(reader))
                {
                    Logger.Error("Failed to load sector visibility file {0}", svgSavePath);
                }
                return visibility;
            }
        }

        // Attempt loading it from the archives
        var svbPath = _dataDir + "/" + svbFilename;
        if (Tig.FS.FileExists(svbPath))
        {
            using var reader = Tig.FS.OpenBinaryReader(svbPath);
            if (!visibility.Load(reader))
            {
                Logger.Error("Failed to load sector visibility file {0}", svbPath);
            }
        }

        return visibility;
    }

    private static string GetSvbFilename(SectorLoc location)
    {
        return location.Pack().ToString(CultureInfo.InvariantCulture) + ".svb";
    }
}

public class SectorVisibility
{
    public const int Subtiles = 192;

    public const int BitPerSubtile = 4;

    public bool IsDirty { get; private set; } = false;

    internal int UsageCount { get; set; }

    public TimePoint LastUsed { get; private set; } = TimePoint.Now;

    public SectorLoc Location { get; }

    // We store 4 bit of data per subtile
    private readonly byte[] _data = new byte[Subtiles * Subtiles * BitPerSubtile / 8];

    public SectorVisibility(SectorLoc location)
    {
        Location = location;
    }

    public void MarkUsed()
    {
        LastUsed = TimePoint.Now;
    }

    public bool Load(BinaryReader reader)
    {
        return reader.Read(_data) == _data.Length;
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(_data);
    }

    public VisibilityFlags this[int x, int y]
    {
        get
        {
            var idx = y * 96 + x / 2;
            if (x % 2 == 0)
            {
                return (VisibilityFlags) (_data[idx] & 0xF);
            }
            else
            {
                return (VisibilityFlags) (_data[idx] >> 4);
            }
        }
    }
}

[Flags]
public enum VisibilityFlags
{
    Extend = 1,
    End = 2,
    Base = 4,
    Archway = 8
}