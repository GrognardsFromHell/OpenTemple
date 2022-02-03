using System;
using System.Collections.Generic;
using System.IO;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.IO;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems.Teleport;

public class DayNightTransfer
{
    [TempleDllLocation(0x10ab7540)]
    private const bool IsEditor = false;

    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    [TempleDllLocation(0x10ab7548)]
    private readonly List<DayNightTransferRecord> _records = new List<DayNightTransferRecord>();

    public int GetCurrentDayNightTransferMap(GameObject objHnd)
    {
        var currentMapId = GameSystems.Map.GetCurrentMapId();

        foreach (var record in _records)
        {
            if (record.CurrentMapId == currentMapId && record.ObjectId == objHnd.id)
            {
                if (GameSystems.TimeEvent.IsDaytime)
                {
                    return record.DayMapId;
                }
                else
                {
                    return record.NightMapId;
                }
            }
        }

        return currentMapId;
    }

    public void ProcessDayNightTransfer(int toMap)
    {
        LoadDayNightTransfer();

        var workQueue = new List<DayNightTransferRecord>();

        var isDaytime = GameSystems.TimeEvent.IsDaytime;
        foreach (var record in _records)
        {
            var intendedMap = isDaytime ? record.DayMapId : record.NightMapId;
            if (intendedMap != record.CurrentMapId)
            {
                if (intendedMap == toMap || record.CurrentMapId == toMap)
                {
                    workQueue.Add(record);
                }
            }
        }

        // Sort the work queue by map id to group entries by map id and avoid unnecessary map changes
        workQueue.Sort((a, b) => a.CurrentMapId.CompareTo(b.CurrentMapId));

        foreach (var j in workQueue)
        {
            if (j.CurrentMapId == 0 || j.CurrentMapId == -1)
            {
                continue;
            }

            if (j.CurrentMapId != GameSystems.Map.GetCurrentMapId())
            {
                GameSystems.Map.OpenMap(j.CurrentMapId, false, false, true);
            }

            if (j.CurrentMapId != GameSystems.Map.GetCurrentMapId())
            {
                Logger.Info("Failed to load map {0} to process day/night transfer for {1}", j.CurrentMapId,
                    j.ObjectId);
                continue;
            }

            var obj = GameSystems.Object.GetObject(j.ObjectId);
            if (obj != null && !GameSystems.Party.IsInParty(obj) && !GameSystems.Critter.IsDeadNullDestroyed(obj))
            {
                LocAndOffsets moveToLoc;
                int moveToMap;
                if (isDaytime)
                {
                    moveToLoc = j.DayPosition;
                    moveToMap = j.DayMapId;
                }
                else
                {
                    moveToLoc = j.NightPosition;
                    moveToMap = j.NightMapId;
                }

                GameSystems.MapObject.MoveToMap(obj, moveToMap, moveToLoc);

                // Update the current map accordingly
                j.CurrentMapId = moveToMap;

                Logger.Info("Moved {0} to map {1} for day/night transfer.", obj, moveToMap);
            }
        }

        SaveDayNightTransfer();
    }

    private void SaveDayNightTransfer()
    {
        using var writer = OpenDayNightTransferFileWriter();

        foreach (var record in _records)
        {
            WriteRecord(writer, record);
        }
    }

    public void RemoveDayNightTransfer(GameObject critter)
    {
        // TODO: This function does not make a lot of sense since it doesn't save it's results and the changes will be overwritten next time the processing is performed
        for (var i = _records.Count - 1; i >= 0; i--)
        {
            var record = _records[i];
            if (record.ObjectId == critter.id)
            {
                _records.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Path to the day night transfer file in the current save game.
    /// </summary>
    private static string SaveGamePath => Path.Join(Globals.GameFolders.CurrentSaveFolder, "daynight.nxd");

    private static BinaryReader OpenDayNightTransferFileReader()
    {
        var path = SaveGamePath;
        if (File.Exists(path))
        {
            return new BinaryReader(new FileStream(path, FileMode.Open));
        }

        return Tig.FS.OpenBinaryReader("rules/daynight.nxd");
    }

    private static BinaryWriter OpenDayNightTransferFileWriter()
    {
        if (IsEditor)
        {
            // Check address 0x100852C8
            throw new NotSupportedException("Vanilla would write to the rules/daynight file here.");
        }

        return new BinaryWriter(new FileStream(SaveGamePath, FileMode.Create));
    }

    public bool HasDayNightTransfer(GameObject critter)
    {
        foreach (var record in _records)
        {
            if (record.ObjectId == critter.id)
            {
                return true;
            }
        }

        return false;
    }

    [TempleDllLocation(0x100851c0)]
    private void LoadDayNightTransfer()
    {
        _records.Clear();

        using var reader = OpenDayNightTransferFileReader();
        while (!reader.AtEnd())
        {
            _records.Add(ReadRecord(reader));
        }
    }

    [TempleDllLocation(0x10084de0)]
    private static DayNightTransferRecord ReadRecord(BinaryReader reader)
    {
        var objectId = reader.ReadObjectId();
        var currentMapId = reader.ReadInt32();
        var dayMapId = reader.ReadInt32();
        var dayPosition = reader.ReadLocationAndOffsets();
        var nightMapId = reader.ReadInt32();
        var nightPosition = reader.ReadLocationAndOffsets();

        var record = new DayNightTransferRecord(
            objectId,
            dayMapId,
            dayPosition,
            nightMapId,
            nightPosition,
            currentMapId
        );
        return record;
    }

    [TempleDllLocation(0x10084de0)]
    private static void WriteRecord(BinaryWriter writer, DayNightTransferRecord record)
    {
        writer.WriteObjectId(record.ObjectId);
        writer.Write(record.CurrentMapId);
        writer.Write(record.DayMapId);
        writer.WriteLocationAndOffsets(record.DayPosition);
        writer.Write(record.NightMapId);
        writer.WriteLocationAndOffsets(record.NightPosition);
    }

    private class DayNightTransferRecord
    {
        public ObjectId ObjectId { get; }

        public int DayMapId { get; }

        public LocAndOffsets DayPosition { get; }

        public int NightMapId { get; }

        public LocAndOffsets NightPosition { get; }

        public int CurrentMapId { get; set; }

        [TempleDllLocation(0x10084c00)]
        public DayNightTransferRecord(ObjectId objectId, int dayMapId, LocAndOffsets dayPosition, int nightMapId,
            LocAndOffsets nightPosition, int currentMapId)
        {
            ObjectId = objectId;
            DayMapId = dayMapId;
            DayPosition = dayPosition;
            NightMapId = nightMapId;
            NightPosition = nightPosition;
            CurrentMapId = currentMapId;
        }
    }
}