using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.IO;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.AI;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.MapSector;
using OpenTemple.Core.Systems.TimeEvents;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Utils;
using Rectangle = System.Drawing.Rectangle;
using Vector2 = System.Numerics.Vector2;

namespace OpenTemple.Core.Systems
{
    using ReplacementSet = IImmutableDictionary<MaterialPlaceholderSlot, ResourceRef<IMdfRenderMaterial>>;

    public class MapObjectSystem : IGameSystem
    {
        private const bool IsEditor = false;

        private readonly ObjectFlag _hiddenFlags;

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        // Loaded from rules/materials.mes
        private readonly Dictionary<int, ReplacementSet> _replacementSets = new Dictionary<int, ReplacementSet>();

        private static readonly Dictionary<string, MaterialPlaceholderSlot> SlotMapping =
            new Dictionary<string, MaterialPlaceholderSlot>
            {
                {"CHEST", MaterialPlaceholderSlot.CHEST},
                {"BOOTS", MaterialPlaceholderSlot.BOOTS},
                {"GLOVES", MaterialPlaceholderSlot.GLOVES},
                {"HEAD", MaterialPlaceholderSlot.HEAD}
            };


        public MapObjectSystem()
        {
            LoadReplacementSets("rules/materials.mes");
            if (Tig.FS.FileExists("rules/materials_ext.mes"))
            {
                LoadReplacementSets("rules/materials_ext.mes");
            }

            if (IsEditor)
            {
                // Nothing is hidden in the editor
                _hiddenFlags = default;
            }
            else
            {
                _hiddenFlags = ObjectFlag.OFF | ObjectFlag.DESTROYED;
            }
        }

        public void Dispose()
        {
            // Clear all referenced maintained by the replacement sets
            foreach (var set in _replacementSets.Values)
            {
                foreach (var value in set.Values)
                {
                    value.Dispose();
                }
            }

            _replacementSets.Clear();
        }

        [TempleDllLocation(0x10021930)]
        public void FreeRenderState(GameObject obj)
        {
            obj.DestroyRendering();

            var sectorLoc = new SectorLoc(obj.GetLocation());
            if (obj.IsStatic() || GameSystems.MapSector.IsSectorLoaded(sectorLoc))
            {
                using var lockedSector = new LockedMapSector(sectorLoc);
                lockedSector.RemoveObject(obj);
            }
        }

        /// <summary>
        /// Handles cleanup tasks when an object leaves the world for any reason.
        /// This includes, but is not limited to:
        /// - It's being destroyed via <see cref="ObjectSystem.Destroy"/>
        /// - It's being fully deleted via <see cref="RemoveMapObj"/>
        /// - An item is being moved into a container
        /// </summary>
        public void OnObjectRemovedFromWorld(GameObject obj)
        {
            if (obj.HasFlag(ObjectFlag.TEXT))
            {
                GameSystems.TextBubble.Remove(obj);
            }

            if (obj.HasFlag(ObjectFlag.TEXT_FLOATER))
            {
                GameSystems.TextFloater.Remove(obj);
            }

            GameSystems.Anim.ClearForObject(obj);

            if (obj.IsCritter())
            {
                GameSystems.AI.RemoveAiTimer(obj);

                if (GameSystems.Combat.IsCombatActive())
                {
                    if (GameSystems.D20.Initiative.CurrentActor == obj)
                    {
                        GameSystems.Combat.AdvanceTurn(obj);
                    }

                    GameSystems.Combat.CritterLeaveCombat(obj);
                }

                GameSystems.D20.Initiative.RemoveFromInitiative(obj);

                GameSystems.Critter.RemoveFromGroups(obj);

                // If the object that is leaving the world is an NPC, stop any dialog that might be ongoing
                if (obj.type == ObjectType.npc)
                {
                    var player = GameSystems.Reaction.GetLastReactionPlayer(obj);
                    if (player != null)
                    {
                        GameUiBridge.CancelDialog(player);
                    }
                }

                GameUiBridge.OnObjectDestroyed(obj);
            }

            obj.DestroyRendering();

            // Remove the object from the sector it's currently in (OT addition),
            // this only applies to dynamic objects because static objects cannot
            // be truly removed from the world
            if (!obj.IsStatic())
            {
                var sectorLoc = new SectorLoc(obj.GetLocation());
                if (GameSystems.MapSector.IsSectorLoaded(sectorLoc))
                {
                    using var lockedSector = new LockedMapSector(sectorLoc);
                    lockedSector.RemoveObject(obj);
                }
            }
        }

        /// <summary>
        /// This is a lower-level method to remove an object from the map. If you want to properly remove
        /// a game object, use <see cref="ObjectSystem.Destroy"/> instead.
        /// </summary>
        [TempleDllLocation(0x100219B0)]
        public void RemoveMapObj(GameObject obj)
        {
            OnObjectRemovedFromWorld(obj);

            GameSystems.D20.RemoveDispatcher(obj);

            GameSystems.Object.Remove(obj);
        }

        [TempleDllLocation(0x1009f550)]
        public bool ValidateSector()
        {
            // Check all objects
            foreach (var obj in GameSystems.Object.EnumerateNonProtos())
            {
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

                obj.ValidateInventory();
            }

            return true;
        }

        public string GetDisplayNameForParty(GameObject obj)
        {
            var observer = GameSystems.Party.GetConsciousLeader();
            return GetDisplayName(obj, observer);
        }

        [TempleDllLocation(0x1001fa80)]
        public string GetDisplayName(GameObject obj, GameObject observer)
        {
            if (obj == null)
            {
                return "OBJ_HANDLE_NULL";
            }

            if (obj.type == ObjectType.key)
            {
                var keyId = obj.GetInt32(obj_f.key_key_id);
                if (keyId != 0)
                {
                    return GameSystems.Description.GetKeyName(keyId);
                }
            }

            if (obj.IsItem())
            {
                if (IsEditor || obj == observer || GameSystems.Item.IsIdentified(obj))
                {
                    return GameSystems.Description.Get(obj.GetInt32(obj_f.description));
                }

                var unknownDescriptionId = obj.GetInt32(obj_f.item_description_unknown);
                return GameSystems.Description.Get(unknownDescriptionId);
            }

            if (obj.IsPC())
            {
                return obj.GetString(obj_f.pc_player_name);
            }

            if (obj.IsNPC())
            {
                return GameSystems.Description.Get(GameSystems.Critter.GetDescriptionId(obj, observer));
            }

            return GameSystems.Description.Get(obj.GetInt32(obj_f.description));
        }

        [TempleDllLocation(0x1001f970)]
        public string GetDisplayName(GameObject obj) => GetDisplayName(obj, obj);

        [TempleDllLocation(0x100C2110)]
        public void AddDynamicObjectsToSector(ref SectorObjects sectorObjects, SectorLoc loc, bool unknownFlag)
        {
            foreach (var obj in GameSystems.Object.SpatialIndex.EnumerateInSector(loc))
            {
                var flags = obj.GetFlags();
                if (!flags.HasFlag(ObjectFlag.INVENTORY) && !obj.IsStatic())
                {
                    sectorObjects.Insert(obj);
                    GameSystems.MapObject.StartAnimating(obj);
                    AddAiTimerOnSectorLoad(obj, unknownFlag);
                }
            }
        }

        [TempleDllLocation(0x1001e800)]
        public void StartAnimating(GameObject obj)
        {
            var flags = obj.GetFlags();
            if (flags.HasFlag(ObjectFlag.OFF) || flags.HasFlag(ObjectFlag.DESTROYED))
            {
                return;
            }

            if (obj.type == ObjectType.scenery)
            {
                var sceneryFlags = obj.GetSceneryFlags();
                if (sceneryFlags.HasFlag(SceneryFlag.NO_AUTO_ANIMATE))
                {
                    if (!GameSystems.Anim.IsProcessing)
                    {
                        GameSystems.Anim.ClearForObject(obj);
                    }

                    return;
                }
            }

            GameSystems.Anim.PushIdleOrLoop(obj);
        }

        [TempleDllLocation(0x10020F50)]
        public void SetFlags(GameObject obj, ObjectFlag flags)
        {
            if (flags.HasFlag(ObjectFlag.FLAT))
            {
                ChangeFlat(obj, true);
                flags &= ObjectFlag.FLAT;
            }

            var currentFlags = obj.GetFlags();
            if (flags.HasFlag(ObjectFlag.OFF) && !currentFlags.HasFlag(ObjectFlag.OFF))
            {
                if (currentFlags.HasFlag(ObjectFlag.TEXT))
                {
                    GameSystems.TextBubble.Remove(obj);
                }

                if (currentFlags.HasFlag(ObjectFlag.TEXT_FLOATER))
                {
                    GameSystems.TextFloater.Remove(obj);
                }

                GameSystems.MapSector.SetLightHandleFlag(obj, 0);
            }

            obj.SetFlags(obj.GetFlags() | flags);
        }

        [TempleDllLocation(0x10021020)]
        public void ClearFlags(GameObject obj, ObjectFlag flags)
        {
            if (flags.HasFlag(ObjectFlag.FLAT))
            {
                ChangeFlat(obj, true);
                flags &= ObjectFlag.FLAT;
            }

            var startAnimating = false;
            uint clearRenderFlags = 0;
            var currentFlags = obj.GetFlags();
            if (flags.HasFlag(ObjectFlag.OFF) && !currentFlags.HasFlag(ObjectFlag.OFF))
            {
                clearRenderFlags |= 0x7000000;
                GameSystems.MapSector.SetLightHandleFlag(obj, 0);
                startAnimating = true;
            }

            if (flags.HasFlag(ObjectFlag.STONED) && currentFlags.HasFlag(ObjectFlag.STONED))
            {
                clearRenderFlags |= 0x2000000;
            }

            if (flags.HasFlag(ObjectFlag.ANIMATED_DEAD) && currentFlags.HasFlag(ObjectFlag.ANIMATED_DEAD))
            {
                clearRenderFlags |= 0x2000000;
            }

            if (flags.HasFlag(ObjectFlag.DONTLIGHT) && currentFlags.HasFlag(ObjectFlag.DONTLIGHT))
            {
                clearRenderFlags |= 0x2000000;
            }

            obj.SetFlags(obj.GetFlags() & ~flags);
            obj.SetInt32(obj_f.render_flags, (int) (obj.GetInt32(obj_f.render_flags) & ~clearRenderFlags));

            if (startAnimating)
            {
                if (!IsEditor)
                {
                    StartAnimating(obj);
                }
            }
        }

        /**
         * Changes the FLAT flag of an object and re-sorts the object-list in the sector.
         */
        private void ChangeFlat(GameObject obj, bool enabled)
        {
            obj.SetFlag(ObjectFlag.FLAT, enabled);

            var location = obj.GetLocation();
            var sectorLoc = new SectorLoc(location);
            if (GameSystems.MapSector.IsSectorLoaded(sectorLoc))
            {
                using var lockedSector = new LockedMapSector(sectorLoc);
                lockedSector.RemoveObject(obj);
                lockedSector.AddObject(obj);
            }
        }

        [TempleDllLocation(0x1001D840)]
        public void ClearRectList()
        {
            // VERY LIKELY UNUSED
            Stub.TODO();
        }

        [TempleDllLocation(0x10025950)]
        public void Move(GameObject obj, LocAndOffsets loc)
        {
            if (obj.GetFlags().HasFlag(ObjectFlag.DESTROYED))
            {
                return;
            }

            var objLoc = obj.GetLocationFull();
            if (objLoc.location == loc.location)
            {
                MoveOffsets(obj, loc.off_x, loc.off_y);
                return;
            }

            var currentSectorLoc = new SectorLoc(objLoc.location);
            var newSectorLoc = new SectorLoc(loc.location);
            if (currentSectorLoc != newSectorLoc)
            {
                if (obj.IsStatic() || GameSystems.MapSector.IsSectorLoaded(currentSectorLoc))
                {
                    using var lockedSector = new LockedMapSector(currentSectorLoc);
                    lockedSector.RemoveObject(obj);
                }

                obj.SetLocationFull(loc);

                if (obj.IsStatic() || GameSystems.MapSector.IsSectorLoaded(newSectorLoc))
                {
                    using var lockedSector = new LockedMapSector(newSectorLoc);
                    lockedSector.AddObject(obj);
                }
            }
            else
            {
                if (obj.IsStatic())
                {
                    using var lockedSector = new LockedMapSector(currentSectorLoc);
                    lockedSector.UpdateObjectPos(obj, loc);
                }
                else
                {
                    obj.SetLocationFull(loc);
                }
            }

            GameSystems.Light.MoveObjectLight(obj, loc);

            HandleDepthChange(obj, objLoc, loc);

            obj.UpdateRenderingState(currentSectorLoc != newSectorLoc);

            GameSystems.ObjectEvent.NotifyMoved(obj, objLoc, loc);
            GameSystems.PathX.ClearCache();
        }

        [TempleDllLocation(0x10025590)]
        [TempleDllLocation(0x10025cb0)]
        public void MoveOffsets(GameObject obj, float offsetX, float offsetY)
        {
            if (obj.HasFlag(ObjectFlag.DESTROYED))
            {
                return;
            }

            var currentLoc = obj.GetLocationFull();
            var newLoc = new LocAndOffsets(currentLoc.location, offsetX, offsetY);

            HandleDepthChange(obj, currentLoc, newLoc);

            var sectorLoca = new SectorLoc(currentLoc.location);
            if (obj.IsStatic() || GameSystems.MapSector.IsSectorLoaded(sectorLoca))
            {
                using var lockedSector = new LockedMapSector(sectorLoca);
                lockedSector.UpdateObjectPos(obj, newLoc);
            }
            else
            {
                obj.OffsetX = offsetX;
                obj.OffsetY = offsetY;
            }

            obj.AdvanceAnimationTime(0.0f);

            GameSystems.ObjectEvent.NotifyMoved(obj, currentLoc, newLoc);

            GameSystems.Light.MoveObjectLightOffsets(obj, offsetX, offsetY);
        }

        private void HandleDepthChange(GameObject obj, LocAndOffsets oldLoc, LocAndOffsets newLoc)
        {
            // Handle changes in depth (entering/exiting deep water)
            var oldHeight = GameSystems.Height.GetDepth(oldLoc);
            var newHeight = GameSystems.Height.GetDepth(newLoc);
            if (!obj.GetFlags().HasFlag(ObjectFlag.NOHEIGHT)
                && oldHeight != newHeight && (oldHeight == 0 || newHeight == 0))
            {
                GameSystems.ParticleSys.CreateAtObj("ef-splash", obj);
                GameSystems.SoundGame.PositionalSound(4000, 1, obj);
            }
        }

        [TempleDllLocation(0x10025f70)]
        public void MoveToMap(GameObject obj, int mapId, LocAndOffsets loc)
        {
            var curMap = GameSystems.Map.GetCurrentMapId();

            // It's within the same map, just move the obj to the target
            if (curMap == mapId)
            {
                if (!obj.GetFlags().HasFlag(ObjectFlag.INVENTORY))
                {
                    GameSystems.MapObject.Move(obj, loc);
                }
                return;
            }

            // Collect all objects that need to be moved (includes equipment / container content)
            var moveList = new List<GameObject> {obj};
            foreach (var childObj in obj.EnumerateChildren())
            {
                moveList.Add(childObj);
            }

            var mapName = GameSystems.Map.GetMapName(mapId);
            if (mapName.Length == 0)
                return;

            var mapSaveFolder = Path.Join(Globals.GameFolders.CurrentSaveFolder, "maps", mapName);
            Directory.CreateDirectory(mapSaveFolder);

            foreach (var objToMove in moveList)
            {
                Logger.Debug("Moving {0} to {1} @ {2}", objToMove, mapName, loc);

                GameSystems.Critter.StopNormalHealingTimer(objToMove);
                GameSystems.Critter.StopSubdualHealingTimer(objToMove);
                GameSystems.Teleport.RemoveObjectFromCurrentMap(objToMove);
                GameSystems.Anim.ClearForObject(objToMove);

                objToMove.FreezeIds();

                // Move the object to the target map by appending it to that maps dynamic mobile file,
                // and marking it extinct in the current
                objToMove.SetFlag(ObjectFlag.DYNAMIC, true);
                objToMove.SetLocationFull(loc);

                var mobileMdyPath = Path.Join(mapSaveFolder, MapMobileLoader.DynamicMobilesFile);

                using var writer = new BinaryWriter(new FileStream(mobileMdyPath, FileMode.Append));
                obj.Write(writer);

                obj.SetFlag(ObjectFlag.EXTINCT | ObjectFlag.DESTROYED, true);
            }
        }

        [TempleDllLocation(0x1001ffe0)]
        public void AddAiTimerOnSectorLoad(GameObject obj, bool unknownFlag)
        {
            if (!IsEditor)
            {
                if (obj.IsNPC())
                {
                    var flags = obj.GetFlags();
                    if (!flags.HasFlag(ObjectFlag.DESTROYED))
                    {
                        GameSystems.AI.AddOrReplaceAiTimer(obj, unknownFlag ? 1 : 0);
                    }
                }
            }
        }

        private void AddReplacementSetEntry(int id, string entry)
        {
            if (entry.Length == 0)
            {
                return;
            }

            var set = new Dictionary<MaterialPlaceholderSlot, ResourceRef<IMdfRenderMaterial>>();

            var elems = entry.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var elem in elems)
            {
                var subElems = elem.Split(':');

                if (subElems.Length < 2)
                {
                    Logger.Warn("Invalid material replacement set {0}: {1}", id, entry);
                    continue;
                }

                var slotName = subElems[0];
                var mdfName = subElems[1];

                // Map the slot name to the enum literal for it
                if (!SlotMapping.TryGetValue(slotName, out var slot))
                {
                    Logger.Warn("Invalid material replacement set {0}: {1}", id, entry);
                    continue;
                }

                // Resolve the referenced material
                var material = Tig.MdfFactory.LoadMaterial(mdfName);
                if (!material.IsValid)
                {
                    Logger.Warn("Material replacement set {0} references unknown MDF: {1}", id, mdfName);
                    continue;
                }

                set[slot] = material;
            }

            _replacementSets[id] = set.ToImmutableDictionary();
        }

        [TempleDllLocation(0x10022170)]
        public void ApplyReplacementMaterial(IAnimatedModel model, int id, int fallbackId = -1)
        {
            var replacementSet = GetReplacementSet(id, fallbackId);
            foreach (var entry in replacementSet)
            {
                model.AddReplacementMaterial(entry.Key, entry.Value.Resource);
            }
        }

        // Retrieves a material replacement set from rules/materials.mes
        private ReplacementSet GetReplacementSet(int id, int fallbackId = -1)
        {
            if (_replacementSets.TryGetValue(id, out var set))
            {
                return set;
            }

            if (fallbackId != -1)
            {
                if (_replacementSets.TryGetValue(id, out set))
                {
                    return set;
                }
            }

            return ImmutableDictionary<MaterialPlaceholderSlot, ResourceRef<IMdfRenderMaterial>>.Empty;
        }

        private void LoadReplacementSets(string filename)
        {
            var mapping = Tig.FS.ReadMesFile(filename);

            foreach (var entry in mapping)
            {
                AddReplacementSetEntry(entry.Key, entry.Value);
            }
        }

        [TempleDllLocation(0x100252d0)]
        public void MoveItem(GameObject item, locXY loc)
        {
            var flags = item.GetFlags();
            if (flags.HasFlag(ObjectFlag.DESTROYED))
            {
                return;
            }

            if (item.IsItem())
            {
                item.SetObject(obj_f.item_parent, null);
            }

            item.SetFlag(ObjectFlag.INVENTORY, false);
            item.SetLocation(loc);
            item.SetFloat(obj_f.offset_x, 0f);
            item.SetFloat(obj_f.offset_y, 0f);

            var secLoc = new SectorLoc(loc);
            if (GameSystems.MapSector.IsSectorLoaded(secLoc))
            {
                using var sector = new LockedMapSector(secLoc);
                sector.AddObject(item);
            }

            item.SetInt32(obj_f.render_flags, 0);
            GameSystems.MapSector.MapSectorResetLightHandle(item);

            item.UpdateRenderingState(true);

            GameSystems.ObjectEvent.NotifyMoved(item, LocAndOffsets.Zero, new LocAndOffsets(loc, 0, 0));
        }


        /// <summary>
        /// Creates a new object with the given prototype at the given location.
        /// </summary>
        [TempleDllLocation(0x10028d20)]
        [TempleDllLocation(0x10026330)]
        public GameObject CreateObject(GameObject protoObj, LocAndOffsets location)
        {
            var obj = GameSystems.Object.CreateFromProto(protoObj, location);

            InitDynamic(obj, location.location);

            return obj;
        }

        public GameObject CreateObject(GameObject protoObj, locXY location) =>
            CreateObject(protoObj, new LocAndOffsets(location));

        /// <summary>
        /// Creates a new object with the given prototype at the given location.
        /// </summary>
        public GameObject CreateObject(int protoId, locXY location) =>
            CreateObject(protoId, new LocAndOffsets(location));

        public GameObject CreateObject(int protoId, LocAndOffsets location)
        {
            var protoObj = GameSystems.Proto.GetProtoById(protoId);
            return CreateObject(protoObj, location);
        }

        [TempleDllLocation(0x10028d70)]
        [TempleDllLocation(0x100263b0)]
        public GameObject CloneObject(GameObject obj, LocAndOffsets location)
        {
            var dest = GameSystems.Object.Clone(obj);

            dest.SetDispatcher(null);
            InitDynamic(dest, location.location);

            GameSystems.MapObject.Move(dest, location);

            if (dest.IsNPC())
            {
                StandPoint standpoint = new StandPoint();
                standpoint.location = location;
                standpoint.mapId = GameSystems.Map.GetCurrentMapId();
                standpoint.jumpPointId = -1;

                dest.SetStandPoint(StandPointType.Day, standpoint);
                dest.SetStandPoint(StandPointType.Night, standpoint);
            }

            return dest;
        }

        public void InitDynamic(GameObject obj, locXY location)
        {
            // Mark the object and all its children as dynamic
            obj.SetFlag(ObjectFlag.DYNAMIC, true);
            obj.ForEachChild(itemObj => { itemObj.SetFlag(ObjectFlag.DYNAMIC, true); });

            // Add the new object to the sector system if needed
            var sectorLoc = new SectorLoc(location);
            if (GameSystems.MapSector.IsSectorLoaded(sectorLoc))
            {
                using var sector = new LockedMapSector(sectorLoc);
                sector.AddObject(obj);
            }

            GameSystems.MapSector.MapSectorResetLightHandle(obj);

            // Init NPC state
            if (obj.IsNPC())
            {
                GameSystems.AI.AddAiTimer(obj);
            }

            if (obj.IsCritter())
            {
                GameSystems.D20.Status.D20StatusInit(obj);
            }

            // Apply random sizing of the 3d model if requested
            var flags = obj.GetFlags();
            if (flags.HasFlag(ObjectFlag.RANDOM_SIZE))
            {
                var scale = obj.GetInt32(obj_f.model_scale);
                scale -= new Random().Next(0, 21);
                obj.SetInt32(obj_f.model_scale, scale);
            }

            GameSystems.Item.PossiblySpawnInvenSource(obj);

            obj.UpdateRenderingState(true);

            LocAndOffsets fromLoc;
            fromLoc.location.locx = 0;
            fromLoc.location.locy = 0;
            fromLoc.off_x = 0;
            fromLoc.off_y = 0;

            LocAndOffsets toLoc = fromLoc;
            toLoc.location = location;

            GameSystems.ObjectEvent.NotifyMoved(obj, fromLoc, toLoc);
        }

        [TempleDllLocation(0x10021d20)]
        public bool HasAnim(GameObject obj, EncodedAnimId animId)
        {
            var animModel = obj.GetOrCreateAnimHandle();
            if (animModel != null)
            {
                return animModel.HasAnim(animId);
            }

            return false;
        }

        [TempleDllLocation(0x1001f770)]
        public void MakeItemParented(GameObject item, GameObject parent)
        {
            Trace.Assert(parent != null);

            if (!item.HasFlag(ObjectFlag.DESTROYED))
            {
                OnObjectRemovedFromWorld(item);

                item.SetFlag(ObjectFlag.INVENTORY, true);
                item.SetObject(obj_f.item_parent, parent);
            }
        }

        #region Global Stashed Object

        private GameObject _globalStashedObject;
        private FrozenObjRef _globalStashedObjectRef;

        [TempleDllLocation(0x100206d0)]
        private bool ValidateFrozenRef(ref GameObject obj, in FrozenObjRef frozenRef)
        {
            if (obj == null || frozenRef.guid.IsNull)
            {
                obj = null;
                return false;
            }

            if (!frozenRef.guid.IsNull && obj != null && !GameSystems.Object.IsValidHandle(obj))
            {
                if (!Unfreeze(in frozenRef, out obj))
                {
                    Logger.Error("Failed to recover an object during validation: {0}", frozenRef);
                    obj = null;
                    return false;
                }

                if (obj != null && (obj.GetFlags() & (ObjectFlag.OFF | ObjectFlag.DESTROYED)) != 0)
                {
                    obj = null;
                }
            }

            return true;
        }

        [TempleDllLocation(0x10808CE8)]
        public GameObject GlobalStashedObject
        {
            [TempleDllLocation(0x10020ee0)]
            get
            {
                if (_globalStashedObject != null)
                {
                    if (ValidateFrozenRef(ref _globalStashedObject, in _globalStashedObjectRef))
                    {
                        return _globalStashedObject;
                    }

                    if (_globalStashedObject != null)
                    {
                        GlobalStashedObject = null;
                    }
                }

                return null;
            }
            [TempleDllLocation(0x10020e50)]
            set
            {
                if (_globalStashedObject != value)
                {
                    _globalStashedObject = value;
                    _globalStashedObjectRef = value != null ? CreateFrozenRef(value) : FrozenObjRef.Null;
                }
            }
        }

        [TempleDllLocation(0x10020eb0)]
        public void ClearGlobalStashedObject()
        {
            GlobalStashedObject = null;
        }

        [TempleDllLocation(0x10020540)]
        public FrozenObjRef CreateFrozenRef(GameObject obj)
        {
            FrozenObjRef result;
            if (obj == null)
            {
                return FrozenObjRef.Null;
            }

            if (obj.GetFlags().HasFlag(ObjectFlag.DESTROYED))
            {
                return FrozenObjRef.Null;
            }

            var objId = GameSystems.Object.GetPersistableId(obj);
            locXY location;
            int mapNumber;
            if (obj.IsStatic())
            {
                location = obj.GetLocation();
                mapNumber = GameSystems.Map.GetCurrentMapId();
            }
            else
            {
                location = locXY.fromField(0);
                mapNumber = GameSystems.Map.GetCurrentMapId();
            }

            return new FrozenObjRef(objId, location, mapNumber);
        }

        [TempleDllLocation(0x10020610)]
        public bool Unfreeze(in FrozenObjRef frozenRef, out GameObject obj)
        {
            if (frozenRef.guid.IsNull)
            {
                obj = null;
                return true;
            }

            if (frozenRef.location != locXY.Zero && frozenRef.mapNumber != GameSystems.Map.GetCurrentMapId())
            {
                obj = null;
                return false;
                // At this point, ToEE did query the map, but didn't do anything with it
            }

            obj = GameSystems.Object.GetObject(frozenRef.guid);
            return obj != null;
        }

        [TempleDllLocation(0x10020370)]
        public bool LoadFrozenRef(out GameObject objOut, out FrozenObjRef frozenRef, BinaryReader fh)
        {
            var objId = fh.ReadObjectId();
            locXY location = fh.ReadTileLocation();
            var mapId = fh.ReadInt32();

            if (objId.IsNull)
            {
                objOut = null;
                frozenRef = FrozenObjRef.Null;
            }
            else
            {
                // Again, as in unfreeze, location is completely ignored

                objOut = GameSystems.Object.GetObject(objId);
                if (objOut == null)
                {
                    Logger.Warn("Couldn't find object with id {0} @ {1} on map {2}", objId, location, mapId);
                }

                frozenRef = new FrozenObjRef(
                    objId,
                    location,
                    mapId
                );
            }

            return true;
        }

        public void SaveFrozenRef(GameObject obj, BinaryWriter writer)
        {
            ObjectId id;
            int mapNumber;
            locXY loc;
            if (obj != null)
            {
                id = obj.id; // TODO: Previously this was checked against the registry
                loc = obj.GetLocation();
                mapNumber = GameSystems.Map.GetCurrentMapId();
            }
            else
            {
                Logger.Debug("SaveTimeEventObjInfo(): Caught null handle when serializing time event!");
                id = ObjectId.CreateNull();
                loc = locXY.Zero;
                mapNumber = 0;
            }

            var frozenRef = new FrozenObjRef(id, loc, mapNumber);
            SaveFrozenRef(frozenRef, writer);
        }

        public void SaveFrozenRef(in FrozenObjRef frozenRef, BinaryWriter writer)
        {
            writer.WriteObjectId(frozenRef.guid);
            writer.WriteTileLocation(frozenRef.location);
            writer.Write(frozenRef.mapNumber);
        }

        #endregion

        [TempleDllLocation(0x1001fb60)]
        public string GetLongDescription(GameObject obj, GameObject observer)
        {
            if (obj.type == ObjectType.key)
            {
                var keyId = obj.GetInt32(obj_f.key_key_id);
                return GameSystems.Description.GetKeyName(keyId);
            }
            else if (obj.type.IsEquipment())
            {
                int descId;
                if (IsEditor || GameSystems.Item.IsIdentified(obj))
                {
                    descId = obj.GetInt32(obj_f.description);
                }
                else
                {
                    descId = obj.GetInt32(obj_f.item_description_unknown);
                }

                return GameSystems.Description.GetLong(descId);
            }
            else if (obj.type == ObjectType.pc)
            {
                return obj.GetString(obj_f.pc_player_name);
            }
            else if (obj.type == ObjectType.npc)
            {
                var descId = GameSystems.Critter.GetDescriptionId(obj, observer);
                return GameSystems.Description.GetLong(descId);
            }
            else
            {
                var descId = obj.GetInt32(obj_f.description);
                return GameSystems.Description.GetLong(descId);
            }
        }

        [TempleDllLocation(0x1001fc20)]
        public bool HasLongDescription(GameObject obj)
        {
            return !string.IsNullOrEmpty(GetLongDescription(obj, obj));
        }

        [TempleDllLocation(0x1001e730)]
        public bool IsBusted(GameObject obj)
        {
            if (obj.HasFlag(ObjectFlag.DESTROYED))
            {
                return false;
            }
            else
            {
                switch (obj.type)
                {
                    case ObjectType.portal:
                        return obj.GetPortalFlags().HasFlag(PortalFlag.BUSTED);
                    case ObjectType.container:
                        return obj.GetContainerFlags().HasFlag(ContainerFlag.BUSTED);
                    case ObjectType.scenery:
                        return obj.GetSceneryFlags().HasFlag(SceneryFlag.BUSTED);
                    case ObjectType.trap:
                        return obj.GetTrapFlags().HasFlag(TrapFlag.BUSTED);
                    default:
                        return false;
                }
            }
        }

        [TempleDllLocation(0x1001fe40)]
        public bool SetLocked(GameObject obj, bool locked)
        {
            if (obj.ProtoId == 1000)
            {
                return false;
            }

            bool currentlyLocked;
            if (obj.type == ObjectType.portal)
            {
                currentlyLocked = obj.GetPortalFlags().HasFlag(PortalFlag.LOCKED);
            }
            else if (obj.type == ObjectType.container)
            {
                currentlyLocked = obj.GetContainerFlags().HasFlag(ContainerFlag.LOCKED);
            }
            else
            {
                return false;
            }

            if (currentlyLocked && !locked)
            {
                var timeEvent = new TimeEvent(TimeEventType.Lock);
                timeEvent.arg1.handle = obj;
                GameSystems.TimeEvent.Schedule(timeEvent, TimeSpan.FromHours(1), out _);
            }

            if (obj.type == ObjectType.portal)
            {
                var flags = obj.GetPortalFlags() & ~PortalFlag.LOCKED;
                if (locked)
                {
                    flags |= PortalFlag.LOCKED;
                }

                obj.SetPortalFlags(flags);
            }
            else if (obj.type == ObjectType.container)
            {
                var flags = obj.GetContainerFlags() & ~ContainerFlag.LOCKED;
                if (locked)
                {
                    flags |= ContainerFlag.LOCKED;
                }

                obj.SetContainerFlags(flags);
            }

            return obj.NeedsToBeUnlocked();
        }

        [TempleDllLocation(0x100249d0)]
        public void SetRotation(GameObject obj, float rotation)
        {
            while (rotation >= 2 * MathF.PI)
            {
                rotation -= 2 * MathF.PI;
            }

            while (rotation < 0.0)
            {
                rotation += 2 * MathF.PI;
            }

            obj.Rotation = rotation;
            obj.AdvanceAnimationTime(0.0f);
        }

        [Flags]
        public enum ObstacleFlag
        {
            UNK_1 = 0x01, // Set by PathQueryFlag.PQF_HAS_CRITTER (and cannot open portals !?)
            UNK_2 = 0x02, // Set by PathQueryFlag.PQF_MAX_PF_LENGTH_STHG
            UNK_4 = 0x04, // Set by PathQueryFlag.PQF_10
            UNK_8 = 0x08,
            UNK_10 = 0x10,
            SOUND_BLOCKERS = 0x20, // Used for sound blockage
            UNK_40 = 0x40 // Seems unused
        }

        [TempleDllLocation(0x10025CF0)]
        public bool HasBlockingObjectInDir(GameObject actor, LocAndOffsets fromLocation,
            CompassDirection direction, ObstacleFlag blockingFlags)
        {
            return GetObstacleInternal(actor, fromLocation, direction, blockingFlags, out var obstacleObj) != 0
                   || obstacleObj != null;
        }

        [TempleDllLocation(0x10025280)]
        public int GetBlockingObjectInDir(GameObject actor, LocAndOffsets fromLocation,
            CompassDirection direction, ObstacleFlag blockingFlags, out GameObject obstacleObj)
        {
            return GetObstacleInternal(actor, fromLocation, direction, blockingFlags, out obstacleObj);
        }

        public static CompassDirection GetCurrentForwardDirection(GameObject obj)
        {
            var compassSlice = (int) (Angles.ToDegrees(obj.Rotation) / 45);
            if ((compassSlice & 1) == 0)
            {
                compassSlice += 1;
            }

            return (CompassDirection) compassSlice;
        }

        [TempleDllLocation(0x10024A60)]
        private int GetObstacleInternal(GameObject actor, LocAndOffsets loc, CompassDirection direction,
            ObstacleFlag blockingFlags,
            out GameObject obstacleObj)
        {
            obstacleObj = null;
            if (direction.IsCardinalDirection())
            {
                var dirLeft = direction.GetLeft();
                var overallSum = GetObstacleInternal(actor, loc, dirLeft, blockingFlags, out obstacleObj);
                if (obstacleObj != null)
                {
                    return overallSum;
                }

                var tileToLeft = loc.OffsetSubtile(dirLeft);
                var dirRight = direction.GetRight();
                overallSum += GetObstacleInternal(actor, tileToLeft, dirRight, blockingFlags, out obstacleObj);
                if (obstacleObj != null)
                {
                    return overallSum;
                }

                overallSum += GetObstacleInternal(actor, loc, dirRight, blockingFlags, out obstacleObj);
                if (obstacleObj != null)
                {
                    return overallSum;
                }

                var tileToRight = loc.OffsetSubtile(dirRight);
                overallSum += GetObstacleInternal(actor, tileToRight, dirLeft, blockingFlags, out obstacleObj);
                return overallSum;
            }

            var weightSum = 0;
            var radius = actor?.GetRadius() ?? locXY.INCH_PER_HALFTILE;

            using var portalList = ObjList.ListRadius(loc, radius, ObjectListFilter.OLC_PORTAL);
            foreach (var portal in portalList)
            {
                if (actor == portal)
                {
                    continue;
                }

                var objDir = GetCurrentForwardDirection(portal);
                if (objDir == direction && !portal.IsPortalOpen())
                {
                    if (blockingFlags.HasFlag(ObstacleFlag.SOUND_BLOCKERS))
                    {
                        weightSum++;
                        continue;
                    }

                    if (blockingFlags.HasFlag(ObstacleFlag.UNK_1))
                    {
                        obstacleObj = portal;
                        return weightSum;
                    }

                    if (blockingFlags.HasFlag(ObstacleFlag.UNK_8))
                    {
                        var uVar6 = portal.GetFlags();
                        if (!uVar6.HasFlag(ObjectFlag.SHOOT_THROUGH))
                        {
                            obstacleObj = portal;
                            return weightSum;
                        }

                        if (!uVar6.HasFlag(ObjectFlag.SEE_THROUGH))
                        {
                            weightSum += 50;
                        }
                        else if (uVar6.HasFlag(ObjectFlag.PROVIDES_COVER))
                        {
                            weightSum += 20;
                        }

                        continue;
                    }

                    if (actor != null && GameSystems.AI.AttemptToOpenDoor(actor, portal) != LockStatus.PLS_OPEN)
                    {
                        obstacleObj = portal;
                        return weightSum;
                    }
                }
            }

            var targetTile = loc.OffsetSubtile(direction);
            if (blockingFlags.HasFlag(ObstacleFlag.SOUND_BLOCKERS) &&
                GameSystems.Tile.MapTileIsSoundProof(targetTile.location))
            {
                weightSum += 8;
            }

            // This just seems to filter for any non-item
            using var objList = ObjList.ListRadius(loc, radius,
                ObjectListFilter.OLC_PORTAL | ObjectListFilter.OLC_CONTAINER | ObjectListFilter.OLC_SCENERY |
                ObjectListFilter.OLC_PROJECTILE | ObjectListFilter.OLC_PC | ObjectListFilter.OLC_NPC |
                ObjectListFilter.OLC_TRAP);
            foreach (var obj in objList)
            {
                if (actor == obj)
                {
                    continue;
                }

                var objType = obj.type;
                if (objType == ObjectType.portal)
                {
                    var objOppositeDir = GetCurrentForwardDirection(obj).GetOpposite();

                    if (objOppositeDir != direction || obj.IsPortalOpen())
                        continue;

                    if (blockingFlags.HasFlag(ObstacleFlag.SOUND_BLOCKERS))
                    {
                        weightSum++;
                        continue;
                    }

                    if (blockingFlags.HasFlag(ObstacleFlag.UNK_1))
                    {
                        obstacleObj = obj;
                        break;
                    }

                    if (blockingFlags.HasFlag(ObstacleFlag.UNK_8))
                    {
                        var objFlags = obj.GetFlags();
                        if (!objFlags.HasFlag(ObjectFlag.SHOOT_THROUGH))
                        {
                            obstacleObj = obj;
                            break;
                        }

                        if (!objFlags.HasFlag(ObjectFlag.SEE_THROUGH))
                        {
                            weightSum += 50;
                        }
                        else if (objFlags.HasFlag(ObjectFlag.PROVIDES_COVER))
                        {
                            weightSum += 20;
                        }

                        continue;
                    }

                    if (actor != null && GameSystems.AI.DryRunAttemptOpenDoor(actor, obj) != LockStatus.PLS_OPEN)
                    {
                        obstacleObj = obj;
                        break;
                    }

                    continue;
                }

                if (blockingFlags.HasFlag(ObstacleFlag.UNK_10) || blockingFlags.HasFlag(ObstacleFlag.SOUND_BLOCKERS))
                {
                    continue;
                }

                if (objType.IsCritter())
                {
                    if (!blockingFlags.HasFlag(ObstacleFlag.UNK_4) &&
                        (!blockingFlags.HasFlag(ObstacleFlag.UNK_40) || !GameSystems.Critter.IsFriendly(actor, obj)))
                    {
                        if (GameSystems.Critter.IsDeadOrUnconscious(obj))
                        {
                            obstacleObj = obj;
                            break;
                        }
                    }
                }
                else
                {
                    var objFlags = obj.GetFlags();
                    if (!objFlags.HasFlag(ObjectFlag.PROVIDES_COVER))
                    {
                        if (!blockingFlags.HasFlag(ObstacleFlag.UNK_8))
                        {
                            obstacleObj = obj;
                            break;
                        }

                        if (objFlags.HasFlag(ObjectFlag.SHOOT_THROUGH))
                        {
                            obstacleObj = obj;
                            break;
                        }
                    }
                }

                if (blockingFlags.HasFlag(ObstacleFlag.UNK_8) &&
                    (!objType.IsCritter() || !GameSystems.Critter.IsDeadNullDestroyed(obj)))
                {
                    var objFlags = obj.GetFlags();
                    if (!objFlags.HasFlag(ObjectFlag.SHOOT_THROUGH))
                    {
                        obstacleObj = obj;
                        break;
                    }

                    if (!objFlags.HasFlag(ObjectFlag.SEE_THROUGH))
                    {
                        weightSum += 50;
                    }
                    else if (objFlags.HasFlag(ObjectFlag.PROVIDES_COVER))
                    {
                        weightSum += 20;
                    }
                }
            }

            return weightSum;
        }

        public bool IsHiddenByFlags(GameObject obj)
        {
            return (obj.GetFlags() & _hiddenFlags) != default;
        }

        [TempleDllLocation(0x1001dab0)]
        public Vector2 GetScreenPosOfObject(IGameViewport viewport, GameObject obj)
        {
            var worldLoc = obj.GetLocationFull().ToInches3D();
            return viewport.WorldToScreen(worldLoc);
        }

        [TempleDllLocation(0x1001fcb0)]
        public bool IsUntargetable(GameObject obj)
        {
            if ((obj.GetFlags() & (ObjectFlag.OFF | ObjectFlag.CLICK_THROUGH | ObjectFlag.DONTDRAW)) != 0)
            {
                return true;
            }

            if (obj.type == ObjectType.portal && obj.IsUndetectedSecretDoor())
            {
                return true;
            }

            if (obj.IsNPC() && obj.AiFlags.HasFlag(AiFlag.RunningOff))
            {
                return true;
            }

            return false;
        }

        [Flags]
        public enum ObjectRectFlags
        {
            IgnoreHeight = 1,
            IgnoreHidden = 8
        }

        [TempleDllLocation(0x10022bc0)]
        public Rectangle GetObjectRect(IGameViewport viewport, GameObject obj, ObjectRectFlags flags = default)
        {
            // i & 8 seems to force
            if (!flags.HasFlag(ObjectRectFlags.IgnoreHidden) && IsHiddenByFlags(obj))
            {
                return Rectangle.Empty;
            }

            var screenPos = GetScreenPosOfObject(viewport, obj);

            var result = new Rectangle((int) screenPos.X, (int) screenPos.Y, 0, 0);

            var radius = obj.GetRadius();

            if (flags.HasFlag(ObjectRectFlags.IgnoreHeight))
            {
                result.X -= (int) radius;
                result.Y += (int) (-0.7f * radius); // TODO: Again. Manually appplied camera tilt -> blergh
                result.Width = 2 * (int) radius;
                result.Height = (int) (radius * 1.4f); // TODO: Again. Manually appplied camera tilt -> blergh
            }
            else
            {
                var height = obj.GetRenderHeight() * 0.71414f; // TODO: Again. Manually appplied camera tilt -> blergh
                result.X -= (int) radius;
                result.Y -= (int) (radius * 0.7f + height); // TODO: Again. Manually appplied camera tilt -> blergh
                result.Width = (int) (2 * radius);
                result.Height = (int) (radius * 1.4f + height); // TODO: Again. Manually appplied camera tilt -> blergh
            }

            return result;
        }

        [TempleDllLocation(0x10020060)]
        public void SetTransparency(GameObject obj, int newOpacity)
        {
            var currentOpacity = obj.GetInt32(obj_f.transparency);
            obj.SetInt32(obj_f.transparency, newOpacity);
            if (currentOpacity <= 64)
            {
                if (newOpacity > 64)
                {
                    GameSystems.D20.D20SendSignal(obj, D20DispatcherKey.SIG_Show);
                    if (obj.IsCritter())
                    {
                        // Signal the equipment as well
                        foreach (var slot in EquipSlots.Slots)
                        {
                            var item = GameSystems.Item.ItemWornAt(obj, slot);
                            if (item != null)
                            {
                                GameSystems.D20.D20SendSignal(item, D20DispatcherKey.SIG_Show);
                            }
                        }
                    }
                }
            }
            else if (newOpacity <= 64)
            {
                GameSystems.D20.D20SendSignal(obj, D20DispatcherKey.SIG_Hide);
                if (obj.IsCritter())
                {
                    // Signal the equipment as well
                    foreach (var slot in EquipSlots.Slots)
                    {
                        var item = GameSystems.Item.ItemWornAt(obj, slot);
                        if (item != null)
                        {
                            GameSystems.D20.D20SendSignal(item, D20DispatcherKey.SIG_Hide);
                        }
                    }
                }
            }
        }

        [TempleDllLocation(0x1001dba0)]
        public void ChangeTotalDamage(GameObject obj, int overallDamage)
        {
            // Temporary hitpoints are tracked separately
            if (overallDamage < 0)
            {
                overallDamage = 0;
            }

            obj.SetInt32(obj_f.hp_damage, overallDamage);
            if (overallDamage > 0 && obj.IsNPC())
            {
                GameSystems.Critter.RescheduleNormalHealingTimer(obj, false);
            }
        }

        [TempleDllLocation(0x1001db10)]
        public int ChangeSubdualDamage(GameObject critter, int damage)
        {
            if (!critter.IsCritter())
            {
                return 0;
            }

            if (damage < 0)
            {
                damage = 0;
            }

            critter.SetInt32(obj_f.critter_subdual_damage, damage);
            if (damage > 0 && critter.IsNPC())
            {
                GameSystems.Critter.RescheduleSubdualHealingTimer(critter, false);
            }

            return damage;
        }

        /// <summary>
        /// Checks if the game object is visible in any viewport on screen.
        /// </summary>
        public bool IsOnScreen(GameObject obj)
        {
            foreach (var gameView in GameViews.AllVisible)
            {
                var screenSize = gameView.Camera.ViewportSize;
                var rect = GameSystems.MapObject.GetObjectRect(gameView, obj, ObjectRectFlags.IgnoreHidden);
                if (rect.Left < screenSize.Width
                    && rect.Top < screenSize.Height
                    && 0 < rect.Right
                    && 0 < rect.Bottom)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
