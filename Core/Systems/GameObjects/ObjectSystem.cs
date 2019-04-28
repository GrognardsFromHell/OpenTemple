using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting;
using System.Text;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;

namespace SpicyTemple.Core.Systems.GameObjects
{
    public class ObjectSystem : IGameSystem
    {
        private ILogger Logger = new ConsoleLogger();

        private ObjRegistry mObjRegistry = new ObjRegistry();

        public SpatialIndex SpatialIndex { get; } = new SpatialIndex();

        [TempleDllLocation(0x1009dff0)]
        public ObjectSystem()
        {
            // TODO temple.GetPointer<int(GameSystemConf*)>(0x1009dff0);
        }

        [TempleDllLocation(0x1009c8c0)]
        public void Dispose()
        {
            // TODO: Shutdown
        }

        public void CompactIndex()
        {
            mObjRegistry.RemoveDynamicObjectsFromIndex();
        }

        // Get the handle for an object by its object id
        public ObjHndl GetHandleById(ObjectId id)
        {
            // Is it already a handle?
            if (id.IsHandle)
            {
                return id.Handle;
            }

            var handle = mObjRegistry.GetHandleById(id);

            if (handle)
            {
                return handle;
            }

            // Check for positional IDs in the map
            if (!id.IsPositional)
                return ObjHndl.Null;

            var pos = id.PositionalId;

            if (GameSystems.Map.GetCurrentMapId() != pos.MapId)
            {
                return ObjHndl.Null;
            }

            locXY loc = new locXY(pos.X, pos.Y);
            using var list = ObjList.ListTile(loc, ObjectListFilter.OLC_IMMOBILE);

            for (var i = 0; i < list.Count; ++i)
            {
                var candidate = list[i];
                var tempId = GetObject(candidate).TemporaryId;
                if (tempId == pos.TempId)
                {
                    mObjRegistry.AddToIndex(candidate, id);
                    return candidate;
                }
            }

            return ObjHndl.Null;
        }

        // Get the object id for an object identified by its handle
        public ObjectId GetIdByHandle(ObjHndl handle)
        {
            return mObjRegistry.GetIdByHandle(handle);
        }

        // Frees the memory associated with the game object and removes it from the object table
        public void Remove(ObjHndl handle)
        {
            var obj = mObjRegistry.Get(handle);

            // Remove associated obj find nodes
            if (!obj.IsProto())
            {
                SpatialIndex.Remove(handle, obj);
            }

            mObjRegistry.Remove(handle);
        }

        // Converts object fields and object array fields from handles to IDs
        public void FreezeIds(ObjHndl handle)
        {
            var obj = GetObject(handle);
            obj.FreezeIds();
        }

        // Converts object fields and object array fields from IDs to handles
        public void UnfreezeIds(ObjHndl handle)
        {
            var obj = GetObject(handle);
            obj.UnfreezeIds();
        }

        // Checks if the given handle points to an active object. Null handles
        // are considered valid
        public bool IsValidHandle(ObjHndl handle)
        {
            if (!handle)
            {
                return true;
            }

            return mObjRegistry.Contains(handle);
        }

        /*
            gets the proto ID number for the object
        */
        public int GetProtoId(ObjHndl obj)
        {
            return GetObject(obj).GetObjectId(obj_f.prototype_handle).PrototypeId;
        }

        public GameObjectBody GetObject(ObjHndl handle)
        {
            return mObjRegistry.Get(handle);
        }

        // Resolve an id for persisting a reference to the given object
        public ObjectId GetPersistableId(ObjHndl handle)
        {
            if (!handle)
            {
                return ObjectId.CreateNull();
            }

            if (!IsValidHandle(handle))
            {
                return ObjectId.CreateNull();
            }

            var obj = GetObject(handle);

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
                mObjRegistry.AddToIndex(handle, obj.id);
            }

            return obj.id;
        }

        public bool ValidateSector(bool requireHandles)
        {
            // Check all objects
            foreach (var entry in mObjRegistry)
            {
                var obj = entry.Value;

                // Primary keys for objects must be persistable ids
                if (!obj.id.IsPersistable())
                {
                    Logger.Error("Found non persistable object id {0}", obj.id);
                    return false;
                }

                if (obj.IsProto())
                {
                    continue;
                }

                if (GetInventoryFields(obj.type, out var idxField, out var countField))
                {
                    ValidateInventory(obj, idxField, countField, requireHandles);
                }
            }

            return true;
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

        /**
         * Returns the handle to a prototype with the given prototype id or the null handle.
         */
        public ObjHndl GetProtoHandle(ushort protoId)
        {
            ObjectId objId = ObjectId.CreatePrototype(protoId);
            return GetHandleById(objId);
        }

        /**
         * Creates a new object with the given prototype at the given location.
         */
        public ObjHndl CreateObject(ObjHndl protoHandle, locXY location)
        {
            var protoObj = GetObject(protoHandle);
            Trace.Assert(protoObj != null && protoObj.IsProto());

            var newHandle = mObjRegistry.Add(new GameObjectBody());
            var obj = mObjRegistry.Get(newHandle);

            obj.protoId = protoObj.id;
            obj.type = protoObj.type;
            obj.field40 = 0; // TODO Is this even used?
            obj.hasDifs = false;
            obj.propCollection = null;

            // We allocate one array for both bitmaps
            var bitmapLen = ObjectFields.GetBitmapBlockCount(obj.type);
            obj.propCollBitmap = new uint[bitmapLen];
            obj.difBitmap = new uint[bitmapLen];

            obj.id = ObjectId.CreatePermanent();
            AddToIndex(obj.id, newHandle);

            obj.SetLocation(location);

            if (obj.IsNPC())
            {
                obj.SetUInt64(obj_f.critter_teleport_dest, location);

                StandPoint standpoint = new StandPoint();
                standpoint.mapId = GameSystems.Map.GetCurrentMapId();
                standpoint.location.location = location;
                standpoint.location.off_x = 0;
                standpoint.location.off_y = 0;
                standpoint.jumpPointId = -1;

                GameSystems.Critter.SetStandPoint(newHandle, StandPointType.Day, standpoint);
                GameSystems.Critter.SetStandPoint(newHandle, StandPointType.Night, standpoint);

                var flags = obj.GetNPCFlags();
                flags |= NpcFlag.WAYPOINTS_DAY;
                obj.SetNPCFlags(flags);
            }

            SpatialIndex.Add(newHandle);

            InitDynamic(obj, newHandle, location);

            return newHandle;
        }

        /// <summary>
        /// Loads an object from the given file.
        /// </summary>
        public ObjHndl LoadFromFile(BinaryReader reader)
        {
            var obj = GameObjectBody.Load(reader);

            // Add it to the registry
            var id = obj.id;
            var handle = mObjRegistry.Add(obj);
            if (!id.IsNull)
            {
                mObjRegistry.AddToIndex(handle, id);
            }

            SpatialIndex.Add(handle);

            return handle;
        }

        /**
         * Calls a given callback for each non prototype object.
         */
        public void ForEachObj(Action<ObjHndl, GameObjectBody> callback)
        {
            foreach (var entry in mObjRegistry)
            {
                if (entry.Value.IsProto())
                {
                    continue; // Only instances
                }

                callback(entry.Key, entry.Value);
            }
        }

        /**
         * Create a new empty prototype object.
         */
        public ObjHndl CreateProto(ObjectType type)
        {
            var obj = new GameObjectBody();
            obj.type = type;
            obj.id = ObjectId.CreatePermanent();

            var handle = mObjRegistry.Add(obj);
            mObjRegistry.AddToIndex(handle, obj.id);

            obj.protoId = ObjectId.CreateBlocked();

            var bitmapLen = ObjectFields.GetBitmapBlockCount(type);
            obj.difBitmap = new uint[bitmapLen];

            var count = ObjectFields.GetSupportedFieldCount(type);
            obj.propCollection = new object [count];
            for (var i = 0; i < count; ++i)
            {
                obj.propCollection[i] = null;
            }

            // TODO static var obj_proto_set_defaults = temple.GetPointer <void (ObjHndl) > (0x100a1620);
            // TODO obj_proto_set_defaults(handle);

            return handle;
        }

        /**
         * Clone an existing object and give it the requested location.
         */
        public ObjHndl Clone(ObjHndl handle, locXY location)
        {
            var src = GetObject(handle);
            var dest = src.Clone();
            var result = mObjRegistry.Add(dest);
            mObjRegistry.AddToIndex(result, dest.id);

            GetInventoryFields(GetObject(result).type, out var invField, out _);

            // Clone the inventory as well
            int childIdx = 0;
            src.ForEachChild(childHandle =>
            {
                var clonedChild = GetObject(childHandle).Clone();
                var newChildHandle = mObjRegistry.Add(clonedChild);
                mObjRegistry.AddToIndex(newChildHandle, clonedChild.id);

                dest.SetObjHndl(invField, childIdx++, newChildHandle);
                clonedChild.SetObjHndl(obj_f.item_parent, result);
            });

            SpatialIndex.Add(result);

            dest.SetDispatcher(null);
            InitDynamic(dest, result, location);

            LocAndOffsets extendedLoc;
            extendedLoc.location = location;
            extendedLoc.off_x = 0;
            extendedLoc.off_y = 0;
            GameSystems.Map.MapObject.Move(result, extendedLoc);

            if (dest.IsNPC())
            {
                StandPoint standpoint = new StandPoint();
                standpoint.location.location = location;
                standpoint.location.off_x = 0;
                standpoint.location.off_y = 0;
                standpoint.mapId = GameSystems.Map.GetCurrentMapId();
                standpoint.jumpPointId = -1;

                GameSystems.Critter.SetStandPoint(result, StandPointType.Day, standpoint);
                GameSystems.Critter.SetStandPoint(result, StandPointType.Night, standpoint);
            }

            return result;
        }

        private bool ValidateInventory(GameObjectBody container, obj_f idxField, obj_f countField, bool requireHandles)
        {
            var actualCount = container.GetObjectIdArray(idxField).Count;

            if (actualCount != container.GetInt32(countField))
            {
                Logger.Error("Count stored in {0} doesn't match actual item count of {1}.",
                    countField, idxField);
                return false;
            }

            for (var i = 0; i < actualCount; ++i)
            {
                var itemId = container.GetObjectId(idxField, i);

                var positional = $"Entry {itemId} in {idxField}@{i} of {container.id}";

                if (itemId.IsNull)
                {
                    Logger.Error("{0} is null", positional);
                    return false;
                }
                else if (!itemId.IsHandle)
                {
                    if (requireHandles)
                    {
                        Logger.Error("{0} is not a handle, but handles are required.", positional);
                        return false;
                    }

                    if (!itemId.IsPersistable())
                    {
                        Logger.Error("{0} is not a valid persistable id.", positional);
                        return false;
                    }
                }

                var itemObj = GetObject(GetHandleById(itemId));

                if (itemObj == null)
                {
                    Logger.Error("{0} does not resolve to a loaded object.", positional);
                    return false;
                }

                if (itemObj == container)
                {
                    Logger.Error("{0} is contained inside of itself.", positional);
                    return false;
                }

                // Only items are allowed in containers
                if (!itemObj.IsItem())
                {
                    Logger.Error("{0} is not an item.", positional);
                    return false;
                }
            }

            return true;
        }

        internal void AddToIndex(ObjectId id, ObjHndl handle)
        {
            mObjRegistry.AddToIndex(handle, id);
        }

        private void InitDynamic(GameObjectBody obj, ObjHndl handle, locXY location)
        {
            // Mark the object and all its children as dynamic
            obj.SetFlag(ObjectFlag.DYNAMIC, true);
            obj.ForEachChild(itemHandle =>
            {
                var itemObj = GetObject(itemHandle);
                itemObj.SetFlag(ObjectFlag.DYNAMIC, true);
            });

            // Add the new object to the sector system if needed
            // TODO SectorLoc sectorLoc(location);
            // TODO if (GameSystems.MapSector.IsSectorLoaded(sectorLoc))
            // TODO {
            // TODO     LockedMapSector sector(sectorLoc);
            // TODO     sector.AddObject(handle);
            // TODO }

            GameSystems.MapSector.RemoveSectorLight(handle);

            // Init NPC state
            if (obj.IsNPC())
            {
                GameSystems.AI.AddAiTimer(handle);
            }

            if (obj.IsCritter())
            {
                // TODO GameSystems.D20.d20Status.D20StatusInit(handle);
            }

            // Apply random sizing of the 3d model if requested
            var flags = obj.GetFlags();
            if (flags.HasFlag(ObjectFlag.RANDOM_SIZE))
            {
                var scale = obj.GetInt32(obj_f.model_scale);
                scale -= new Random().Next(0, 21);
                obj.SetInt32(obj_f.model_scale, scale);
            }

            // TODO static var possibly_spawn_inven_source = temple.GetPointer <void (ObjHndl) > (0x1006dcf0);
            // TODO possibly_spawn_inven_source(handle);

            // TODO static var sub_10025050 = temple.GetPointer < int(ObjHndl, int) > (0x10025050);
            // TODO sub_10025050(handle, 2);

            LocAndOffsets fromLoc;
            fromLoc.location.locx = 0;
            fromLoc.location.locy = 0;
            fromLoc.off_x = 0;
            fromLoc.off_y = 0;

            LocAndOffsets toLoc = fromLoc;
            toLoc.location = location;

            GameSystems.ObjectEvent.NotifyMoved(handle, fromLoc, toLoc);
        }
    }
}