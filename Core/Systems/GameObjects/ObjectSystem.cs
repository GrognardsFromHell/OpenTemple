using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.IO;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.MapSector;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.GameObjects;

public class ObjectSystem : IGameSystem
{
    private ILogger Logger = LoggingSystem.CreateLogger();

    private ObjRegistry mObjRegistry = new();

    public SpatialIndex SpatialIndex { get; } = new();

    [TempleDllLocation(0x1009dff0)]
    public ObjectSystem()
    {
        // TODO temple.GetPointer<int(GameSystemConf*)>(0x1009dff0);
    }

    [TempleDllLocation(0x1009c8c0)]
    public void Dispose()
    {
        mObjRegistry.Clear();

        // TODO: Shutdown
    }

    public void CompactIndex()
    {
        mObjRegistry.RemoveDynamicObjectsFromIndex();
    }

    public IEnumerable<GameObject> EnumerateNonProtos()
    {
        foreach (var obj in mObjRegistry)
        {
            if (obj.IsProto())
            {
                continue;
            }

            yield return obj;
        }
    }

    // Get the handle for an object by its object id
    public GameObject? GetObject(ObjectId id)
    {
        // It may already be part of the registries index
        var handle = mObjRegistry.GetById(id);
        if (handle != null)
        {
            return handle;
        }

        // Check for positional IDs in the map
        if (!id.IsPositional)
        {
            return null;
        }

        var pos = id.PositionalId;
        if (GameSystems.Map.GetCurrentMapId() != pos.MapId)
        {
            return null;
        }

        var loc = new locXY(pos.X, pos.Y);
        using var list = ObjList.ListTile(loc, ObjectListFilter.OLC_STATIC);

        for (var i = 0; i < list.Count; ++i)
        {
            var candidate = list[i];
            var tempId = candidate.TemporaryId;
            if (tempId == pos.TempId)
            {
                // NOTE: Vanilla did NOT register objects in the registry when loading statics from a sector (I believe)
                //  but we do, so this is redundant!
                if (!mObjRegistry.Contains(candidate))
                {
                    Add(candidate);
                }
                return candidate;
            }
        }

        return null;
    }

    public void Add(GameObject obj)
    {
        // Add it to the registry
        var id = obj.id;
        mObjRegistry.Add(obj);
        if (!id.IsNull)
        {
            mObjRegistry.AddToIndex(obj, id);
        }

        SpatialIndex.Add(obj);
    }

    // Frees the memory associated with the game object and removes it from the object table
    [TempleDllLocation(0x1009e0d0)]
    public void Remove(GameObject obj)
    {
        // Remove associated obj find nodes
        if (!obj.IsProto())
        {
            SpatialIndex.Remove(obj);
        }

        mObjRegistry.Remove(obj);
    }

    // Checks if the given handle points to an active object. Null handles
    // are considered valid
    public bool IsValidHandle(GameObject obj)
    {
        if (obj == null)
        {
            return true;
        }

        return mObjRegistry.Contains(obj);
    }

    // Resolve an id for persisting a reference to the given object
    public ObjectId GetPersistableId(GameObject obj)
    {
        if (obj == null)
        {
            return ObjectId.CreateNull();
        }

        // This ensures that game objects not part of the current "world" will not be assigned IDs
        if (!IsValidHandle(obj))
        {
            return ObjectId.CreateNull();
        }

        // This may happen for sector objs, but when are those not static anyway?
        if (obj.id.IsNull)
        {
            // Generate a positional ID for static objects
            if (obj.IsStatic())
            {
                var loc = obj.GetLocation();
                obj.id = ObjectId.CreatePositional(
                    GameSystems.Map.GetCurrentMapId(),
                    loc.locx,
                    loc.locy,
                    obj.TemporaryId
                );
            }
            else
            {
                obj.id = ObjectId.CreatePermanent();
            }

            obj.hasDifs = true;

            // Make the new id known to the registry
            mObjRegistry.AddToIndex(obj, obj.id);
        }

        return obj.id;
    }

    public bool GetInventoryFields(ObjectType type, out obj_f listIndexField, out obj_f numField)
    {
        if (type == ObjectType.container)
        {
            listIndexField = obj_f.container_inventory_list_idx;
            numField = obj_f.container_inventory_num;
            return true;
        }

        if (type == ObjectType.pc || type == ObjectType.npc)
        {
            listIndexField = obj_f.critter_inventory_list_idx;
            numField = obj_f.critter_inventory_num;
            return true;
        }

        listIndexField = default;
        numField = default;
        return false;
    }

    /// <summary>
    /// Creates a new object with the given prototype at the given location.
    /// </summary>
    [TempleDllLocation(0x100a0c00)]
    public GameObject CreateFromProto(GameObject protoObj, LocAndOffsets location)
    {
        Trace.Assert(protoObj != null && protoObj.IsProto());

        var obj = new GameObject();
        obj.id = ObjectId.CreatePermanent();

        obj.protoId = protoObj.id;
        obj.type = protoObj.type;
        obj.field40 = 0; // TODO Is this even used?
        obj.hasDifs = false;
        obj.propCollection = Array.Empty<object>();

        // We allocate one array for both bitmaps
        var bitmapLen = ObjectFields.GetBitmapBlockCount(obj.type);
        obj.propCollBitmap = new uint[bitmapLen];
        obj.difBitmap = new uint[bitmapLen];

        obj.SetLocation(location.location);
        obj.OffsetX = location.off_x;
        obj.OffsetY = location.off_y;

        if (obj.IsNPC())
        {
            obj.SetUInt64(obj_f.critter_teleport_dest, location.location);

            StandPoint standpoint = new StandPoint();
            standpoint.mapId = GameSystems.Map.GetCurrentMapId();
            standpoint.location = location;
            standpoint.jumpPointId = -1;

            obj.SetStandPoint(StandPointType.Day, standpoint);
            obj.SetStandPoint(StandPointType.Night, standpoint);

            var flags = obj.GetNPCFlags();
            flags |= NpcFlag.WAYPOINTS_DAY;
            obj.SetNPCFlags(flags);
        }

        Add(obj);

        return obj;
    }

    /// <summary>
    /// Loads an object from the given file.
    /// </summary>
    [TempleDllLocation(0x100DE690)]
    public GameObject LoadFromFile(BinaryReader reader)
    {
        var obj = GameObject.Load(reader);
        Add(obj);
        return obj;
    }

    /// <summary>
    /// Calls a given callback for each non prototype object.
    /// </summary>
    /// <param name="callback"></param>
    public void ForEachObj(Action<GameObject> callback)
    {
        foreach (var obj in GameObjects)
        {
            callback(obj);
        }
    }

    /// <summary>
    /// Returns all non prototype objects.
    /// </summary>
    public IEnumerable<GameObject> GameObjects => mObjRegistry.Where(x => !x.IsProto());

    /**
         * Clone an existing object and give it the requested location.
         */
    public GameObject Clone(GameObject src)
    {
        var dest = src.Clone();

        GetInventoryFields(dest.type, out var invField, out _);

        // Clone the inventory as well
        int childIdx = 0;
        src.ForEachChild(childObj =>
        {
            var clonedChild = childObj.Clone();

            dest.SetObject(invField, childIdx++, clonedChild);
            clonedChild.SetObject(obj_f.item_parent, dest);

            Add(clonedChild);
        });

        Add(dest);

        return dest;
    }

    [TempleDllLocation(0x100257a0)]
    public void Destroy(GameObject obj)
    {
        var name = GameSystems.MapObject.GetDisplayName(obj);
        Logger.Info("Destroying {0}", name);

        var flags = obj.GetFlags();

        if (flags.HasFlag(ObjectFlag.DESTROYED))
        {
            return; // Already destroyed
        }

        if (GameSystems.Script.ExecuteObjectScript(obj, obj, ObjScriptEvent.Destroy) == 0)
        {
            return; // Scripts tells us to skip it
        }

        var type = obj.type;
        if (type.IsEquipment())
        {
            var parentObj = GameSystems.Item.GetParent(obj);
            if (parentObj != null)
            {
                var loc = parentObj.GetLocation();
                GameSystems.Item.Remove(obj);
                GameSystems.MapObject.MoveItem(obj, loc);
            }
        }
        else if (type == ObjectType.container)
        {
            // For whatever reason...
            if (obj.ProtoId != 1000)
            {
                GameSystems.Item.PoopInventory(obj, true, onlyInvulnerable: true);
            }
        }
        else if (type.IsCritter())
        {
            GameSystems.Item.PoopInventory(obj, true, onlyInvulnerable: true);
        }

        // This is an OT addition, setting it to OFF should prevent it being a valid combat actor when
        // the turn order is advanced in OnObjectRemovedFromWorld (in case the object being removed is
        // the current combat actor).
        obj.SetFlag(ObjectFlag.OFF, true);
        GameSystems.MapObject.OnObjectRemovedFromWorld(obj);

        obj.SetFlag(ObjectFlag.DESTROYED, true);
    }

    public void AddToIndex(ObjectId id, GameObject obj)
    {
        mObjRegistry.AddToIndex(obj, id);
    }

    public void SetGenderRace(GameObject obj, Gender gender, RaceId race)
    {
        GameSystems.Stat.SetBasicStat(obj, Stat.race, (int) race);
        GameSystems.Stat.SetBasicStat(obj, Stat.gender, (int) gender);
        var genderIdx = (int) gender;
        var raceIdx = (int) race;
        obj.SetInt32(obj_f.sound_effect, 10 * (genderIdx + 2 * raceIdx + 1));
    }

}