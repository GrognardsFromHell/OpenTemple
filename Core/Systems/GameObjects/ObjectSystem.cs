using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting;
using System.Text;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.MapSector;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Utils;

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

        public IEnumerable<GameObjectBody> EnumerateNonProtos()
        {
            foreach (var entry in mObjRegistry)
            {
                var obj = entry.Value;
                if (obj.IsProto())
                {
                    continue;
                }

                yield return obj;
            }
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

        // Frees the memory associated with the game object and removes it from the object table
        [TempleDllLocation(0x1009e0d0)]
        public void Remove(GameObjectBody obj)
        {
            var handle = GetHandleById(obj.id);

            // Remove associated obj find nodes
            if (!obj.IsProto())
            {
                SpatialIndex.Remove(handle, obj);
            }

            mObjRegistry.Remove(handle);
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

        public GameObjectBody GetObject(ObjectId id)
        {
            return GetObject(GetHandleById(id));
        }

        // Resolve an id for persisting a reference to the given object
        public ObjectId GetPersistableId(GameObjectBody obj)
        {
            if (obj == null)
            {
                return ObjectId.CreateNull();
            }

            // TODO: We might be able to remove this
            if (!IsValidHandle(GetHandleById(obj.id)))
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
                mObjRegistry.AddToIndex(GetHandleById(obj.id), obj.id);
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

        /**
         * Returns the handle to a prototype with the given prototype id or the null handle.
         */
        public GameObjectBody GetProto(int protoId)
        {
            var objId = ObjectId.CreatePrototype((ushort) protoId);
            return GetObject(GetHandleById(objId));
        }

        /// <summary>
        /// Creates a new object with the given prototype at the given location.
        /// </summary>
        [TempleDllLocation(0x100a0c00)]
        public GameObjectBody CreateFromProto(GameObjectBody protoObj, locXY location)
        {
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

                GameSystems.Critter.SetStandPoint(obj, StandPointType.Day, standpoint);
                GameSystems.Critter.SetStandPoint(obj, StandPointType.Night, standpoint);

                var flags = obj.GetNPCFlags();
                flags |= NpcFlag.WAYPOINTS_DAY;
                obj.SetNPCFlags(flags);
            }

            SpatialIndex.Add(obj);

            return obj;
        }

        /// <summary>
        /// Loads an object from the given file.
        /// </summary>
        [TempleDllLocation(0x100DE690)]
        public GameObjectBody LoadFromFile(BinaryReader reader)
        {
            var obj = GameObjectBody.Load(reader);

            // Add it to the registry
            var id = obj.id;
            var handle = mObjRegistry.Add(obj);
            if (!id.IsNull)
            {
                mObjRegistry.AddToIndex(handle, id);
            }

            SpatialIndex.Add(obj);

            return obj;
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
        [TempleDllLocation(0x100a1930)]
        public GameObjectBody CreateProto(ObjectType type, ObjectId id)
        {
            var obj = new GameObjectBody();
            obj.type = type;
            obj.id = id;

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

            ObjectDefaultProperties.SetDefaultProperties(obj);

            return obj;
        }

        /**
         * Clone an existing object and give it the requested location.
         */
        public GameObjectBody Clone(GameObjectBody src)
        {
            var dest = src.Clone();
            var result = mObjRegistry.Add(dest);
            mObjRegistry.AddToIndex(result, dest.id);

            GetInventoryFields(GetObject(result).type, out var invField, out _);

            // Clone the inventory as well
            int childIdx = 0;
            src.ForEachChild(childObj =>
            {
                var clonedChild = childObj.Clone();
                var newChildHandle = mObjRegistry.Add(clonedChild);
                mObjRegistry.AddToIndex(newChildHandle, clonedChild.id);

                dest.SetObjHndl(invField, childIdx++, newChildHandle);
                clonedChild.SetObjHndl(obj_f.item_parent, result);
            });

            SpatialIndex.Add(dest);

            return dest;
        }

        [TempleDllLocation(0x100257a0)]
        public void Destroy(GameObjectBody obj)
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
                RemoveFromGroups(obj);

                GameSystems.AI.RemoveAiTimer(obj);

                if (type == ObjectType.npc)
                {
                    var player = GameSystems.Reaction.GetLastReactionPlayer(obj);
                    if (player != null)
                    {
                        // TODO: This will call into UI DialogExit @ 0x1009A5D0
                    }
                }

                GameSystems.Item.PoopInventory(obj, true, onlyInvulnerable: true);
            }

            GameSystems.Anim.ClearForObject(obj);

            if (GameSystems.Combat.IsCombatActive())
            {
                if (GameSystems.D20.Initiative.CurrentActor == obj)
                {
                    GameSystems.Combat.AdvanceTurn(obj);
                }
            }

            GameSystems.D20.Initiative.RemoveFromInitiative(obj);

            GameSystems.D20.RemoveDispatcher(obj);

            // TODO var updateTbUi = temple.GetPointer<void(ObjHndl)>(0x1014DE90);
            // TODO updateTbUi(ObjHnd);

            obj.DestroyRendering();

            obj.SetFlag(ObjectFlag.DESTROYED, true);
        }

        [TempleDllLocation(0x10080DA0)]
        private void RemoveFromGroups(GameObjectBody obj)
        {
            Trace.Assert(obj.IsCritter());

            GameSystems.Party.RemoveFromAllGroups(obj);

            if (obj.IsNPC())
            {
                GameSystems.AI.FollowerAddWithTimeEvent(obj, true);
                GameSystems.Critter.RemoveFollowerFromLeaderCritterFollowers(obj);
            }
        }

        internal void AddToIndex(ObjectId id, ObjHndl handle)
        {
            mObjRegistry.AddToIndex(handle, id);
        }

        public bool IsValidHandle(GameObjectBody handle)
        {
            // TODO: This is a bit tricky. The object can still be around, but due to resets or map changes
            // TODO it's possible that it is no longer in the registry. we need a better way of checking
            return GetHandleById(handle.id);
        }

        public void SetTransparency(GameObjectBody obj, int newOpacity)
        {
            var currentOpacity = obj.GetInt32(obj_f.transparency);
            obj.SetInt32(obj_f.transparency, newOpacity);
            if (currentOpacity <= 64)
            {
                if (newOpacity > 64)
                {
                    GameSystems.D20.D20SendSignal(obj, D20DispatcherKey.SIG_Show, null);
                    if (obj.IsCritter())
                    {
                        // Signal the equipment as well
                        foreach (var slot in EquipSlots.Slots)
                        {
                            var item = GameSystems.Item.ItemWornAt(obj, slot);
                            if (item != null)
                            {
                                GameSystems.D20.D20SendSignal(item, D20DispatcherKey.SIG_Show, null);
                            }
                        }
                    }
                }
            }
            else if (newOpacity <= 64)
            {
                GameSystems.D20.D20SendSignal(obj, D20DispatcherKey.SIG_Hide, null);
                if (obj.IsCritter())
                {
                    // Signal the equipment as well
                    foreach (var slot in EquipSlots.Slots)
                    {
                        var item = GameSystems.Item.ItemWornAt(obj, slot);
                        if (item != null)
                        {
                            GameSystems.D20.D20SendSignal(item, D20DispatcherKey.SIG_Hide, null);
                        }
                    }
                }
            }
        }

        public void SetGenderRace(GameObjectBody obj, Gender gender, RaceId race)
        {
            GameSystems.Stat.SetBasicStat(obj, Stat.race, (int) race);
            GameSystems.Stat.SetBasicStat(obj, Stat.gender, (int) gender);
            var genderIdx = (int) gender;
            var raceIdx = (int) race;
            obj.SetInt32(obj_f.sound_effect, 10 * (genderIdx + 2 * raceIdx + 1));
        }
    }
}