using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting;
using System.Text;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.IO;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.MapSector;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.GameObjects
{
    public class ObjectSystem : IGameSystem
    {
        private ILogger Logger = LoggingSystem.CreateLogger();

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
        public GameObjectBody GetObject(ObjectId id)
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

        public void Add(GameObjectBody obj)
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
        public void Remove(GameObjectBody obj)
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
        public bool IsValidHandle(GameObjectBody obj)
        {
            if (obj == null)
            {
                return true;
            }

            return mObjRegistry.Contains(obj);
        }

        // Resolve an id for persisting a reference to the given object
        public ObjectId GetPersistableId(GameObjectBody obj)
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
        public GameObjectBody CreateFromProto(GameObjectBody protoObj, LocAndOffsets location)
        {
            Trace.Assert(protoObj != null && protoObj.IsProto());

            var obj = new GameObjectBody();
            obj.id = ObjectId.CreatePermanent();

            obj.protoId = protoObj.id;
            obj.type = protoObj.type;
            obj.field40 = 0; // TODO Is this even used?
            obj.hasDifs = false;
            obj.propCollection = null;

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

                GameSystems.AI.SetStandPoint(obj, StandPointType.Day, standpoint);
                GameSystems.AI.SetStandPoint(obj, StandPointType.Night, standpoint);

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
        public GameObjectBody LoadFromFile(BinaryReader reader)
        {
            var obj = GameObjectBody.Load(reader);
            Add(obj);
            return obj;
        }

        /**
         * Calls a given callback for each non prototype object.
         */
        public void ForEachObj(Action<GameObjectBody> callback)
        {
            foreach (var obj in mObjRegistry)
            {
                if (obj.IsProto())
                {
                    continue; // Only instances
                }

                callback(obj);
            }
        }

        /**
         * Create a new empty prototype object.
         */
        [TempleDllLocation(0x100a1930)]
        public GameObjectBody CreateProto(ObjectType type, ObjectId id)
        {
            Trace.Assert(id.IsPrototype);

            var obj = new GameObjectBody();
            obj.type = type;
            obj.id = id;

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

            Add(obj);

            return obj;
        }

        /**
         * Clone an existing object and give it the requested location.
         */
        public GameObjectBody Clone(GameObjectBody src)
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
                        GameUiBridge.CancelDialog(player);
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

            GameUiBridge.OnObjectDestroyed(obj);

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

        public void AddToIndex(ObjectId id, GameObjectBody obj)
        {
            mObjRegistry.AddToIndex(obj, id);
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