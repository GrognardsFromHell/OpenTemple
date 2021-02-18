using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.SaveGames;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.GameObjects;

namespace OpenTemple.Core.GameObject
{
    public class GameObjectBody : IDisposable
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private static long _nextObjectId = 1;

        private long _objectId = 0;

        /// <summary>
        /// Indicates that fields of type <see cref="ObjectFieldType.Obj"/> and <see cref="ObjectFieldType.ObjArray"/>
        /// do not store actual pointers, but rather the persistable IDs of those objects.
        /// </summary>
        private bool _frozenObjRefs = false;

        public GameObjectBody()
        {
            _objectId = _nextObjectId++;
        }

        [TempleDllLocation(0x100a1930)]
        public static GameObjectBody CreateProto(ObjectType type, int protoId)
        {
            var obj = new GameObjectBody();
            obj.type = type;
            obj.id = ObjectId.CreatePrototype((ushort) protoId);

            obj.protoId = ObjectId.CreateBlocked();

            var bitmapLen = ObjectFields.GetBitmapBlockCount(type);
            obj.difBitmap = new uint[bitmapLen];

            var count = ObjectFields.GetSupportedFieldCount(type);
            obj.propCollection = new object [count];
            for (var i = 0; i < count; ++i)
            {
                obj.propCollection[i] = null;
            }

            return obj;
        }

        public void Dispose()
        {
            ForEachField((field, storage) =>
            {
                FreeStorage(ref storage);
                return true;
            });

            // Delete transient props
            if (!IsProto())
            {
                transientProps.Dispose();
            }

            // This is the "old" way of doing it
            if (IsProto())
            {
                // TODO free(difBitmap);
            }
            else
            {
                // TODO free(propCollBitmap);
            }

            propCollection = null;
            // TODO delete[] propCollection;
        }

        public ObjectType type;
        uint field4;
        public ObjectId id;
        public ObjectId protoId;
        public uint field40;
        public bool hasDifs;
        public uint[] propCollBitmap;
        public uint[] difBitmap;
        public object[] propCollection;
        TransientProps transientProps;
        uint padding;
        private IDispatcher dispatcher;

        public bool IsProto()
        {
            return protoId.IsBlocked;
        }

        #region Type Tests

        public bool IsItem()
        {
            return type.IsEquipment();
        }

        public bool IsContainer()
        {
            return type == ObjectType.container;
        }

        public bool IsCritter()
        {
            return type == ObjectType.npc || type == ObjectType.pc;
        }

        public bool IsPC()
        {
            return type == ObjectType.pc;
        }

        public bool IsNPC()
        {
            return type == ObjectType.npc;
        }

        #endregion

        public uint GetUInt32(obj_f field) => unchecked((uint) GetInt32(field));

        public uint GetUInt32(obj_f field, int index) => unchecked((uint) GetInt32(field, index));

        [TempleDllLocation(0x1009e1d0)]
        public int GetInt32(obj_f field)
        {
            if (field == obj_f.type)
            {
                return (int) type;
            }

            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.Int32);

            var storageLoc = GetFieldValue(field);
            if (storageLoc == null)
            {
                return default;
            }
            else
            {
                return (int) storageLoc;
            }
        }

        [TempleDllLocation(0x1009e260)]
        public float GetFloat(obj_f field)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.Float32);

            var storageLoc = GetFieldValue(field);
            if (storageLoc == null)
            {
                return default;
            }
            else
            {
                return (float) storageLoc;
            }
        }

        public ulong GetUInt64(obj_f field) => unchecked((ulong) GetInt64(field));

        [TempleDllLocation(0x1009e2e0)]
        public long GetInt64(obj_f field)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.Int64);
            var storageLoc = GetFieldValue(field);
            if (storageLoc == null)
            {
                return default;
            }
            else
            {
                return (long) storageLoc;
            }
        }

        // This gets the object handle and returns true, if it is valid (by validating it against the obj registry)
        // handleOut will always be set to the null handle if the handle is invalid.
        // If the handle is invalid, this function will also clear the storage location
        public bool GetValidObject(obj_f field, out GameObjectBody objOut)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.Obj);

            // Special case for prototype handle
            if (field == obj_f.prototype_handle)
            {
                // GetProtoObj already validates and Protos can never be invalidated either
                var protoObj = GetProtoObj();
                objOut = protoObj;
                return true;
            }

            objOut = GetObject(field);
            if (!GameSystems.Object.IsValidHandle(objOut))
            {
                Logger.Warn("Object {0} has a stale reference to object {1}.", this, objOut);
                objOut = null;
                return false;
            }

            return true;
        }

        public string GetString(obj_f field)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.String);
            return (string) GetFieldValue(field);
        }

        public void SetString(obj_f field, string text)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.String);

            SetFieldValue(field, text);
        }

        [TempleDllLocation(0x1009e7e0)]
        public int GetArrayLength(obj_f field)
        {
            if (ObjectFields.GetType(field) == ObjectFieldType.ObjArray
                || ObjectFields.GetType(field) == ObjectFieldType.SpellArray)
            {
                var backingArray = (IList) GetFieldValue(field);
                return backingArray?.Count ?? 0;
            }
            else
            {
                var backingArray = (ISparseArray) GetFieldValue(field);
                return backingArray?.Count ?? 0;
            }
        }

        public ArrayAccess<int> GetInt32Array(obj_f field)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.Int32Array
                         || ObjectFields.GetType(field) == ObjectFieldType.AbilityArray);

            var backingArray = (SparseArray<int>) GetFieldValue(field);
            return new ArrayAccess<int>(this, backingArray);
        }

        public ArrayAccess<long> GetInt64Array(obj_f field)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.Int64Array);

            var backingArray = (SparseArray<long>) GetFieldValue(field);
            return new ArrayAccess<long>(this, backingArray);
        }

        private static readonly IReadOnlyList<GameObjectBody> EmptyList = ImmutableList<GameObjectBody>.Empty;

        public IReadOnlyList<GameObjectBody> GetObjectArray(obj_f field)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.ObjArray);
            if (_frozenObjRefs)
            {
                throw new InvalidOperationException("Cannot access this method on a frozen object.");
            }

            return (List<GameObjectBody>) GetFieldValue(field) ?? EmptyList;
        }

        public ArrayAccess<ObjectScript> GetScriptArray(obj_f field)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.ScriptArray);

            var backingArray = (SparseArray<ObjectScript>) GetFieldValue(field);
            return new ArrayAccess<ObjectScript>(this, backingArray);
        }

        private static readonly IReadOnlyList<SpellStoreData> EmptySpellList = ImmutableList<SpellStoreData>.Empty;

        public IReadOnlyList<SpellStoreData> GetSpellArray(obj_f field)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.SpellArray);

            return (List<SpellStoreData>) GetFieldValue(field) ?? EmptySpellList;
        }

        // Convenience array accessors
        [TempleDllLocation(0x1009e5c0)]
        public int GetInt32(obj_f field, int index)
        {
            return GetInt32Array(field)[index];
        }

        [TempleDllLocation(0x100a1310)]
        public void SetInt32(obj_f field, int index, int value)
        {
            GetMutableInt32Array(field)[index] = value;
        }

        public void AppendInt32(obj_f field, int value)
        {
            GetMutableInt32Array(field).Append(value);
        }

        public void RemoveInt32(obj_f field, int index)
        {
            GetMutableInt32Array(field).Remove(index);
        }

        public long GetInt64(obj_f field, int index)
        {
            return GetInt64Array(field)[index];
        }

        public void SetInt64(obj_f field, int index, long value)
        {
            GetMutableInt64Array(field)[index] = value;
        }

        public void RemoveInt64(obj_f field, int index)
        {
            GetMutableInt64Array(field).Remove(index);
        }

        public void RemoveObject(obj_f field, int index)
        {
            var arr = GetMutableObjectArray(field);

            if (index < arr.Count)
            {
                arr[index] = null;
            }

            // Trim null's at the end
            for (var i = arr.Count - 1; i >= 0; i--)
            {
                if (arr[i] == null)
                {
                    arr.RemoveAt(i);
                }
            }
        }

        public GameObjectBody GetObject(obj_f field, int index)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.ObjArray);
            if (_frozenObjRefs)
            {
                throw new InvalidOperationException(
                    "While object references are frozen, objects cannot be retrieved."
                );
            }

            var arr = (List<GameObjectBody>) GetFieldValue(field);
            if (arr == null || index >= arr.Count)
            {
                return null;
            }

            return arr[index];
        }

        public GameObjectBody GetObject(obj_f field)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.Obj);

            // Keep in mind that handles are stored in the form of ObjectIds
            return (GameObjectBody) GetFieldValue(field);
        }

        [TempleDllLocation(0x100a14a0)]
        public void SetObject(obj_f field, int index, GameObjectBody obj)
        {
            if (obj == null)
            {
                RemoveObject(field, index);
                return;
            }

            // Pad the array with null's if needed
            var arr = GetMutableObjectArray(field);
            int missingEls = (index + 1) - arr.Count;
            if (missingEls > 0)
            {
                // Try to resize in bulk
                arr.Capacity += arr.Count + missingEls;
                for (int i = 0; i < missingEls; i++)
                {
                    arr.Add(null);
                }
            }

            arr[index] = obj;
        }

        [TempleDllLocation(0x100a0280)]
        public void SetObject(obj_f field, GameObjectBody obj)
        {
            if (_frozenObjRefs)
            {
                throw new InvalidOperationException("Cannot mutate a frozen object.");
            }

            SetFieldValue(field, obj);
        }

        public void SetScript(obj_f field, int index, in ObjectScript script)
        {
            GetMutableScriptArray(field)[index] = script;
        }

        public ObjectScript GetScript(obj_f field, int index)
        {
            return GetScriptArray(field)[index];
        }

        public void RemoveScript(obj_f field, int index)
        {
            GetMutableScriptArray(field).Remove(index);
        }

        public void AppendSpell(obj_f field, in SpellStoreData spell)
        {
            GetMutableSpellArray(field).Add(spell);
        }

        public void SetSpell(obj_f field, int index, in SpellStoreData spell)
        {
            GetMutableSpellArray(field)[index] = spell;
        }

        public SpellStoreData GetSpell(obj_f field, int index)
        {
            return GetSpellArray(field)[index];
        }

        public void RemoveSpell(obj_f field, int index)
        {
            GetMutableSpellArray(field).RemoveAt(index);
        }

        public void ClearArray(obj_f field)
        {
            Trace.Assert(!IsProto());

            switch (ObjectFields.GetType(field))
            {
                case ObjectFieldType.Int32Array:
                case ObjectFieldType.AbilityArray:
                    GetMutableInt32Array(field).Clear();
                    break;
                case ObjectFieldType.Int64Array:
                    GetMutableInt64Array(field).Clear();
                    break;
                case ObjectFieldType.ScriptArray:
                    GetMutableScriptArray(field).Clear();
                    break;
                case ObjectFieldType.ObjArray:
                    GetMutableObjectArray(field).Clear();
                    break;
                case ObjectFieldType.SpellArray:
                    GetMutableSpellArray(field).Clear();
                    break;
                default:
                    throw new Exception($"Cannot clear a non-array field: {field}");
            }
        }

        // Setters
        public void SetInt32(obj_f field, int value)
        {
            Trace.Assert(ObjectFields.GetFieldDef(field).type == ObjectFieldType.Int32);
            SetFieldValue(field, value);
        }

        public void SetUInt32(obj_f field, uint value) => SetInt32(field, unchecked((int) value));

        public void SetFloat(obj_f field, float value)
        {
            Trace.Assert(ObjectFields.GetFieldDef(field).type == ObjectFieldType.Float32);
            SetFieldValue(field, value);
        }

        public void SetUInt64(obj_f field, ulong value) => SetInt64(field, unchecked((long) value));

        public void SetInt64(obj_f field, long value)
        {
            Trace.Assert(ObjectFields.GetFieldDef(field).type == ObjectFieldType.Int64);
            SetFieldValue(field, value);
            if (field == obj_f.location)
            {
                GameSystems.Object.SpatialIndex.UpdateLocation(this);
            }
        }

        public void ResetDiffs()
        {
            // Reset diff state to 0
            if (hasDifs)
            {
                for (int i = 0; i < ObjectFields.GetBitmapBlockCount(type); ++i)
                {
                    difBitmap[i] = 0;
                }
            }

            hasDifs = false;
        }

        /**
         * Removes a field from this object instance, which effectively
         * resets the value of the field to the value from this object's
         * prototype.
         */
        public void ResetField(obj_f field)
        {
            // This method has no effect on prototypes
            if (IsProto())
            {
                return;
            }

            ref readonly var fieldDef = ref ObjectFields.GetFieldDef(field);
            if (HasDataForField(fieldDef))
            {
                var idx = GetPropCollIdx(fieldDef);
                // Copy the property behind the one we remove
                for (var i = idx; i + 1 < propCollection.Length; ++i)
                {
                    propCollection[i] = propCollection[i + 1];
                }

                Array.Resize(ref propCollection, propCollection.Length - 1);

                propCollBitmap[fieldDef.bitmapBlockIdx] &= ~fieldDef.bitmapMask;
                difBitmap[fieldDef.bitmapBlockIdx] &= ~fieldDef.bitmapMask;
            }
        }

        /**
         * Portals, Scenery, and Traps are "static objects" unless they have
         * OF_DYNAMIC set to make them explicitly dynamic objects.
         */
        [TempleDllLocation(0x1001dca0)]
        public bool IsStatic()
        {
            if (type == ObjectType.portal || type == ObjectType.scenery || type == ObjectType.trap)
            {
                return !HasFlag(ObjectFlag.DYNAMIC);
            }

            return false;
        }

        public delegate bool ForEachFieldConstCallback(obj_f field, object currentValue);

        public bool ForEachField(ForEachFieldConstCallback callback)
        {
            if (IsProto())
            {
                return ObjectFields.IterateTypeFields(type, field =>
                {
                    ref readonly var fieldDef = ref ObjectFields.GetFieldDef(field);
                    return callback(field, propCollection[fieldDef.protoPropIdx]);
                });
            }

            return ObjectFields.IterateTypeFields(type, field =>
            {
                ref readonly var fieldDef = ref ObjectFields.GetFieldDef(field);

                // Does this object have the prop?
                if (HasDataForField(fieldDef))
                {
                    var idx = GetPropCollIdx(fieldDef);
                    return callback(field, propCollection[idx]);
                }

                return true;
            });
        }

        /**
         * Returns true if the type of this object has the given field.
         */
        public bool SupportsField(obj_f field)
        {
            return ObjectFields.DoesTypeSupportField(type, field);
        }

        /**
         * If this object is currently storing persistable IDs, they are resolved to the handles of the
         * corresponding objects.
         */
        [TempleDllLocation(0x1009f9e0)]
        public void UnfreezeIds()
        {
            if (!_frozenObjRefs)
            {
                throw new InvalidOperationException("Object IDs are already unfrozen.");
            }

            ForEachField((field, currentValue) =>
            {
                if (currentValue == null)
                {
                    return true;
                }

                var fieldType = ObjectFields.GetType(field);
                if (fieldType == ObjectFieldType.Obj)
                {
                    var handle = GameSystems.Object.GetObject((ObjectId) currentValue);
                    SetFieldValue(field, handle);
                }
                else if (fieldType == ObjectFieldType.ObjArray)
                {
                    var objectIdArray = (List<ObjectId>) currentValue;
                    var objArray = new List<GameObjectBody>(objectIdArray.Count);
                    foreach (var objId in objectIdArray)
                    {
                        if (objId.IsNull)
                        {
                            objArray.Add(null);
                        }
                        else
                        {
                            var obj = GameSystems.Object.GetObject(objId);
                            if (obj == null)
                            {
                                Logger.Warn("Object {0} referenced stale object {1} in field {2}.", this, objId, field);
                            }

                            objArray.Add(obj);
                        }
                    }

                    SetFieldValue(field, objArray);
                }

                return true;
            });

            // Mark as not frozen
            _frozenObjRefs = false;
        }

        /**
         * If this object is currently storing references to other objects as handles, those references will
         * be converted to persistable object ids.
         */
        [TempleDllLocation(0x100a1080)]
        public void FreezeIds()
        {
            if (_frozenObjRefs)
            {
                throw new InvalidOperationException("Object IDs are already frozen.");
            }

            ForEachField((field, currentValue) =>
            {
                if (currentValue == null)
                {
                    return true;
                }

                var fieldType = ObjectFields.GetType(field);
                if (fieldType == ObjectFieldType.Obj)
                {
                    var obj = (GameObjectBody) currentValue;
                    SetFieldValue(field, obj.id);
                }
                else if (fieldType == ObjectFieldType.ObjArray)
                {
                    var objArray = (List<GameObjectBody>) currentValue;
                    var objIdArray = new List<ObjectId>(objArray.Count);
                    foreach (var obj in objArray)
                    {
                        objIdArray.Add(obj?.id ?? ObjectId.CreateNull());
                    }

                    SetFieldValue(field, objIdArray);
                }

                return true;
            });

            // Mark as frozen
            _frozenObjRefs = true;
        }

        public GameObjectBody Clone()
        {
            if (!id.IsPermanent && !id.IsPositional)
            {
                throw new Exception($"Cannot copy an object with an id of type {id.Type}");
            }

            var obj = new GameObjectBody();
            obj.id = ObjectId.CreatePermanent();

            obj.protoId = protoId;
            obj.type = type;

            obj.hasDifs = false;
            obj.propCollection = new object [propCollection.Length];

            var bitmapLen = ObjectFields.GetBitmapBlockCount(obj.type);
            obj.propCollBitmap = new uint[bitmapLen];
            propCollBitmap.CopyTo(obj.propCollBitmap, 0);
            obj.difBitmap = new uint[bitmapLen];

            // Copy field values
            obj.ForEachField((field, currentValue) =>
            {
                if (currentValue == null)
                {
                    return true; // No value to copy
                }

                switch (ObjectFields.GetType(field))
                {
                    // Storage by value
                    case ObjectFieldType.Float32:
                    case ObjectFieldType.Int32:
                    case ObjectFieldType.Int64:
                    case ObjectFieldType.String:
                    case ObjectFieldType.Obj:
                        obj.SetFieldValue(field, currentValue);
                        break;
                    case ObjectFieldType.ObjArray:
                        obj.SetFieldValue(field, new List<GameObjectBody>((List<GameObjectBody>) currentValue));
                        break;
                    case ObjectFieldType.SpellArray:
                        obj.SetFieldValue(field, new List<SpellStoreData>((List<SpellStoreData>) currentValue));
                        break;
                    case ObjectFieldType.AbilityArray:
                    case ObjectFieldType.UnkArray:
                    case ObjectFieldType.Int32Array:
                    case ObjectFieldType.Int64Array:
                    case ObjectFieldType.ScriptArray:
                    case ObjectFieldType.Unk2Array:
                        var array = (ISparseArray) currentValue;
                        obj.SetFieldValue(field, array.Copy());
                        break;
                    default:
                        throw new Exception($"Unable to copy field {field}");
                }

                return true;
            });

            return obj;
        }

        #region Object Field Getters and Setters

        public ObjectFlag GetFlags()
        {
            return (ObjectFlag) unchecked((uint) GetInt32(obj_f.flags));
        }

        public void SetFlags(ObjectFlag flags)
        {
            SetInt32(obj_f.flags, unchecked((int) flags));
        }

        public bool HasFlag(ObjectFlag flag)
        {
            return GetFlags().HasFlag(flag);
        }

        public void SetFlag(ObjectFlag flag, bool enabled)
        {
            if (enabled)
            {
                SetFlags(GetFlags() | flag);
            }
            else
            {
                SetFlags(GetFlags() & ~ flag);
            }
        }

        // This is used a lot
        public bool IsOffOrDestroyed => HasFlag(ObjectFlag.OFF) || HasFlag(ObjectFlag.DESTROYED);

        public SpellFlag GetSpellFlags() => (SpellFlag) GetUInt32(obj_f.spell_flags);

        public void SetSpellFlags(SpellFlag flags) => SetUInt32(obj_f.spell_flags, (uint) flags);

        public ItemFlag GetItemFlags() => (ItemFlag) GetUInt32(obj_f.item_flags);

        public SceneryFlag GetSceneryFlags() => (SceneryFlag) GetUInt32(obj_f.scenery_flags);

        public void SetSceneryFlags(SceneryFlag flags) => SetUInt32(obj_f.scenery_flags, (uint) flags);

        public ContainerFlag GetContainerFlags() => (ContainerFlag) GetUInt32(obj_f.container_flags);

        public void SetContainerFlags(ContainerFlag flags) => SetUInt32(obj_f.container_flags, (uint) flags);

        public void SetItemFlags(ItemFlag flags)
        {
            SetUInt32(obj_f.item_flags, (uint) flags);
        }

        public void SetItemFlag(ItemFlag flag, bool enabled)
        {
            if (enabled)
            {
                SetItemFlags(GetItemFlags() | flag);
            }
            else
            {
                SetItemFlags(GetItemFlags() & ~flag);
            }
        }

        public ProjectileFlag ProjectileFlags
        {
            get => (ProjectileFlag) GetUInt32(obj_f.projectile_flags_combat);
            set => SetUInt32(obj_f.projectile_flags_combat, (uint) value);
        }

        public SecretDoorFlag GetSecretDoorFlags() => (SecretDoorFlag) GetUInt32(obj_f.secretdoor_flags);

        public void SetSecretDoorFlags(SecretDoorFlag flags) => SetUInt32(obj_f.secretdoor_flags, (uint) flags);

        public ItemWearFlag GetItemWearFlags() => (ItemWearFlag) GetUInt32(obj_f.item_wear_flags);

        public WeaponType GetWeaponType() => (WeaponType) GetUInt32(obj_f.weapon_type);

        public WeaponFlag WeaponFlags
        {
            get => (WeaponFlag) GetUInt32(obj_f.weapon_flags);
            set => SetUInt32(obj_f.weapon_flags, (uint) value);
        }

        public ArmorFlag GetArmorFlags() => (ArmorFlag) GetUInt32(obj_f.armor_flags);

        public PortalFlag GetPortalFlags() => (PortalFlag) GetUInt32(obj_f.portal_flags);

        public void SetPortalFlags(PortalFlag flags) => SetUInt32(obj_f.portal_flags, (uint) flags);

        public CritterFlag GetCritterFlags() => (CritterFlag) GetUInt32(obj_f.critter_flags);

        public CritterFlag2 GetCritterFlags2() => (CritterFlag2) GetUInt32(obj_f.critter_flags2);

        public void SetCritterFlags(CritterFlag flags) => SetUInt32(obj_f.critter_flags, (uint) flags);

        public void SetCritterFlags2(CritterFlag2 flags) => SetUInt32(obj_f.critter_flags2, (uint) flags);

        public TrapFlag GetTrapFlags() => (TrapFlag) GetUInt32(obj_f.trap_flags);

        public void SetTrapFlags(TrapFlag flags) => SetUInt32(obj_f.trap_flags, (uint) flags);

        public locXY GetLocation()
        {
            return locXY.fromField(unchecked((ulong) GetInt64(obj_f.location)));
        }

        [TempleDllLocation(0x1001da20)]
        public Vector2 GetWorldPos() => GetLocationFull().ToInches2D();

        [TempleDllLocation(0x100b4900)]
        public float DistanceToInFeetClamped(GameObjectBody otherObj) =>
            GetLocationFull().DistanceTo(otherObj.GetLocationFull()) / locXY.INCH_PER_FEET;

        [TempleDllLocation(0x100b4940)]
        public float DistanceToInFeetClamped(LocAndOffsets location) =>
            GetLocationFull().DistanceTo(location) / locXY.INCH_PER_FEET;

        public LocAndOffsets GetLocationFull()
        {
            return new LocAndOffsets(
                locXY.fromField(GetUInt64(obj_f.location)),
                GetFloat(obj_f.offset_x),
                GetFloat(obj_f.offset_y)
            );
        }

        public void SetLocation(locXY location)
        {
            SetInt64(obj_f.location, unchecked((long) location.ToField()));
        }

        public void SetLocationFull(LocAndOffsets location)
        {
            SetLocation(location.location);
            OffsetX = location.off_x;
            OffsetY = location.off_y;
        }

// TODO: Move to extension methods
        public IDispatcher GetDispatcher()
        {
            return dispatcher;
        }

        // TODO: Move to extension methods
        public void SetDispatcher(IDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        #endregion

        #region NPC Field Getters and Setters

        public NpcFlag GetNPCFlags()
        {
            return (NpcFlag) GetUInt32(obj_f.npc_flags);
        }

        public void SetNPCFlags(NpcFlag flags)
        {
            SetUInt32(obj_f.npc_flags, (uint) flags);
        }

        public AiFlag AiFlags
        {
            get => (AiFlag) GetUInt64(obj_f.npc_ai_flags64);
            set => SetUInt64(obj_f.npc_ai_flags64, (ulong) value);
        }

        // TODO: move this to GameObjectBody extensions because it's data access
        [TempleDllLocation(0x100ba890)]
        public void GetStandPoint(StandPointType type, out StandPoint standPoint)
        {
            var standpointArray = GetMutableInt64Array(obj_f.npc_standpoints);
            standPoint = DeserializeStandpoint(standpointArray, (int) type);
        }

        public static StandPoint DeserializeStandpoint(IReadOnlyList<long> serializedStandpoints, int typeIndex)
        {
            // TODO Check that we're actually getting standpoints correctly here...
            Span<long> packedStandpoint = stackalloc long[10];

            for (int i = 0; i < 10; i++)
            {
                packedStandpoint[i] = serializedStandpoints[10 * typeIndex + i];
            }

            return MemoryMarshal.Read<StandPoint>(MemoryMarshal.Cast<long, byte>(packedStandpoint));
        }

        [TempleDllLocation(0x100ba8f0)]
        public void SetStandPoint(StandPointType type, StandPoint standpoint)
        {
            // TODO Check that we're actually setting standpoints correctly here...
            Span<long> packedStandpoint = stackalloc long[10];
            MemoryMarshal.Write(MemoryMarshal.Cast<long, byte>(packedStandpoint), ref standpoint);

            for (int i = 0; i < 10; i++)
            {
                SetInt64(obj_f.npc_standpoints, 10 * (int) type + i, packedStandpoint[i]);
            }
        }

        #endregion

        [TempleDllLocation(0x100646d0)]
        public IEnumerable<GameObjectBody> EnumerateChildren()
        {
            if (!GameSystems.Object.GetInventoryFields(type, out var indexField, out var countField))
            {
                yield break;
            }

            var count = GetInt32(countField);
            for (var i = 0; i < count; ++i)
            {
                var item = GetObject(indexField, i);
                if (item != null)
                {
                    yield return item;
                }
            }
        }

        public IEnumerable<KeyValuePair<EquipSlot, GameObjectBody>> EnumerateEquipment()
        {
            if (!IsCritter())
            {
                yield break;
            }

            foreach (var item in EnumerateChildren())
            {
                if (item.TryGetEquipSlot(out var slot))
                {
                    yield return KeyValuePair.Create(slot, item);
                }
            }
        }

        // Utility function for containers and critters
        // Will iterate over the content of this object
        // If this object is not a container or critter, will do nothing.
        public void ForEachChild(Action<GameObjectBody> callback)
        {
            if (!GameSystems.Object.GetInventoryFields(type, out var indexField, out var countField))
            {
                return;
            }

            var count = GetInt32(countField);
            for (var i = 0; i < count; ++i)
            {
                var item = GetObject(indexField, i);
                callback(item);
            }
        }

        #region Transient Property Accessors

        public int TemporaryId => transientProps.tempId;

        public float OffsetX
        {
            get => GetFloat(obj_f.offset_x);
            set => SetFloat(obj_f.offset_x, value);
        }

        public float OffsetY
        {
            get => GetFloat(obj_f.offset_y);
            set => SetFloat(obj_f.offset_y, value);
        }

        public float OffsetZ
        {
            get => GetFloat(obj_f.offset_z);
            set => SetFloat(obj_f.offset_z, value);
        }

        /// <summary>
        /// The object's rotation in the world.
        /// A rotation of 0 means the object is facing straight upwards on screen.
        /// A positive rotation rotates to the right on screen.
        /// </summary>
        public float Rotation
        {
            get => GetFloat(obj_f.rotation);
            set => SetFloat(obj_f.rotation, value);
        }

        public float RotationPitch => GetFloat(obj_f.rotation_pitch);

        #endregion

        #region Persistence

        /**
         * Writes this object to a file. Only supported for non-prototype objects.
         * Prefixes the object's body with 0x77 (the object file version).
         */
        [TempleDllLocation(0x1009fb00)]
        public bool Write(BinaryWriter writer)
        {
            Trace.Assert(!IsProto());

            writer.Write((uint) 0x77);
            writer.WriteObjectId(protoId);
            writer.WriteObjectId(id);
            writer.Write((uint) type);

            // Write the number of properties we are going to write
            writer.Write((ushort) propCollection.Length);

            // Write the is-property-set bitmap blocks
            var bitmapLen = ObjectFields.GetBitmapBlockCount(type);
            Trace.Assert(bitmapLen == propCollBitmap.Length);
            var propCollData = MemoryMarshal.Cast<uint, byte>(propCollBitmap);
            writer.Write(propCollData);

            return ForEachField((field, currentValue) =>
            {
                WriteFieldToStream(ObjectFields.GetType(field), currentValue, writer);
                return true;
            });
        }

        [TempleDllLocation(0x100a11a0)]
        public static GameObjectBody Load(BinaryReader reader)
        {
            var header = reader.ReadUInt32();
            if (header != 0x77)
            {
                throw new Exception($"Expected object header 0x77, but got 0x{header:X}");
            }

            var protoId = reader.ReadObjectId();

            if (!protoId.IsPrototype)
            {
                throw new Exception($"Expected a prototype id, but got type {protoId.Type} instead.");
            }

            ObjectId objId = reader.ReadObjectId();

            // Null IDs are allowed for sector objects
            if (!objId.IsPermanent && !objId.IsNull)
            {
                throw new Exception($"Expected an object id of type Permanent, but got type {objId.Type} instead.");
            }

            var typeCode = (ObjectType) reader.ReadUInt32();

            var obj = new GameObjectBody();
            obj.protoId = protoId;
            obj.id = objId;
            obj.type = typeCode;

            // Initialize and load bitmaps
            var bitmapLen = ObjectFields.GetBitmapBlockCount(obj.type);
            obj.propCollBitmap = new uint[bitmapLen];
            obj.difBitmap = new uint[bitmapLen];

            var propCount = reader.ReadUInt16();

            var rawBitmap = MemoryMarshal.Cast<uint, byte>(obj.propCollBitmap.AsSpan());
            reader.Read(rawBitmap);

            obj.propCollection = new object [propCount];
            obj.ForEachField((field, currentValue) =>
            {
                var value = ObjectFields.ReadFieldValue(field, reader);
                obj.SetFieldValue(field, value);
                return true;
            });

            obj._frozenObjRefs = true;

            return obj;
        }

        private const int DiffHeader = 0x12344321;
        private const int DiffFooter = 0x23455432;

        [TempleDllLocation(0x1009fe20)]
        public void LoadDeltaFromFile(BinaryReader reader)
        {
            if (!_frozenObjRefs)
            {
                throw new InvalidOperationException(
                    "Cannot load difs for an object that is not storing persistable ids.");
            }

            var version = reader.ReadInt32();
            if (version != 0x77)
            {
                throw new CorruptSaveException($"Expected object version 0x77, but read {version}");
            }

            var magicNumber = reader.ReadInt32();
            if (magicNumber != DiffHeader)
            {
                throw new CorruptSaveException($"Expected diff-header {DiffHeader}, but read {magicNumber}");
            }

            var diffId = reader.ReadObjectId();

            // For static objects that are read from a .sec file with an associated .dif file, the object-id
            // within the .sec file should *usually* be a NULL-ID.
            if (!id.IsNull)
            {
                if (id != diffId)
                {
                    throw new CorruptSaveException($"ID {diffId} of diff record differs from object id {id}");
                }
            }
            else
            {
                // We do not have to remove the current ID from any index since it's null!
                id = diffId;
                if (!diffId.IsNull)
                {
                    GameSystems.Object.AddToIndex(diffId, this);
                }
            }

            var bitmapLen = ObjectFields.GetBitmapBlockCount(type);
            for (var i = 0; i < bitmapLen; i++)
            {
                difBitmap[i] = reader.ReadUInt32();
            }

            hasDifs = true;

            foreach (var field in ObjectFields.GetTypeFields(type))
            {
                // Is it marked for diffs?
                ref readonly var fieldDef = ref ObjectFields.GetFieldDef(field);
                if ((difBitmap[fieldDef.bitmapBlockIdx] & fieldDef.bitmapMask) != 0)
                {
                    // Read the object field value
                    var value = ObjectFields.ReadFieldValue(field, reader);
                    SetFieldValue(field, value);
                }
            }

            magicNumber = reader.ReadInt32();
            if (magicNumber != DiffFooter)
            {
                throw new CorruptSaveException($"Expected diff-footer {DiffFooter}, but read {magicNumber}");
            }

            GameSystems.Object.SpatialIndex.UpdateLocation(this);
        }

        private static void WriteObjectId(BinaryWriter writer, ObjectId item) => writer.WriteObjectId(item);

        private static void WriteSpellStoreData(BinaryWriter writer, SpellStoreData item)
        {
            writer.Write(item.spellEnum);
            writer.Write(item.classCode);
            writer.Write(item.spellLevel);

            var state = (uint) item.spellStoreState.spellStoreType;
            if (item.spellStoreState.usedUp)
            {
                state |= 0x100u;
            }

            writer.Write(state);
            writer.Write(item.metaMagicData.Pack());
            writer.Write(item.pad1);
            writer.Write(item.pad2);
            writer.Write(item.pad3);
        }


        private static void WriteListAsSparseArray<T>(BinaryWriter writer, int itemSize,
            SparseArrayConverter.ItemWriter<T> itemWriter, IList<T> items)
        {
            if (items == null)
            {
                writer.Write((byte) 0);
            }
            else
            {
                writer.Write((byte) 1);

                SparseArrayConverter.WriteTo(writer, itemSize, items, itemWriter);
            }
        }

        private const uint DiffMagicNumberStart = 0x12344321;
        private const uint DiffMagicNumberEnd = 0x23455432;

        [TempleDllLocation(0x1009fc10)]
        public void WriteDiffsToStream(BinaryWriter stream)
        {
            Trace.Assert(hasDifs);

            stream.Write((uint) 0x77); // Version
            stream.Write(DiffMagicNumberStart); // Magic header

            // NOTE: this seems redundant since for obj diffs the id comes first anyway
            stream.WriteObjectId(id);

            var bitmapLen = ObjectFields.GetBitmapBlockCount(type);

            // Validate dif bitmap
            ObjectFields.IterateTypeFields(type, field =>
            {
                ref readonly var fieldDef = ref ObjectFields.GetFieldDef(field);
                bool hasDifs = (difBitmap[fieldDef.bitmapBlockIdx] & fieldDef.bitmapMask) != 0;
                bool hasData = HasDataForField(fieldDef);
                if (hasDifs && !hasData)
                {
                    Logger.Error("Object has diffs for {0}, but no data!", field);
                }

                return true;
            });

            Trace.Assert(bitmapLen == difBitmap.Length);
            var rawDifBitmap = MemoryMarshal.Cast<uint, byte>(difBitmap);
            stream.Write(rawDifBitmap);

            // Write each field that is different
            ForEachField((field, currentValue) =>
            {
                ref readonly var fieldDef = ref ObjectFields.GetFieldDef(field);
                if ((difBitmap[fieldDef.bitmapBlockIdx] & fieldDef.bitmapMask) != 0)
                {
                    WriteFieldToStream(fieldDef.type, currentValue, stream);
                }

                return true;
            });

            stream.Write(DiffMagicNumberEnd); // Magic footer
        }

        public void LoadDiffsFromFile(BinaryReader file)
        {
            if (!_frozenObjRefs)
            {
                throw new InvalidOperationException(
                    "Cannot load difs for an object that is not storing persistable ids.");
            }

            var version = file.ReadUInt32();
            if (version != 0x77)
            {
                throw new Exception("Cannot read object file version. Read: " + version);
            }

            var magicNumber = file.ReadUInt32();
            if (magicNumber != DiffMagicNumberStart)
            {
                throw new Exception("Cannot read diff header.");
            }

            var id = file.ReadObjectId();
            if (!this.id.IsNull)
            {
                if (this.id != id)
                {
                    throw new Exception($"ID {id} of diff record differs from object id {this.id}");
                }
            }
            else
            {
                this.id = id;
                if (!id.IsNull)
                {
                    GameSystems.Object.AddToIndex(id, this);
                }
            }

            var bitmapLen = ObjectFields.GetBitmapBlockCount(type);
            Trace.Assert(difBitmap.Length == bitmapLen);
            var rawDifBitmap = MemoryMarshal.Cast<uint, byte>(difBitmap);
            if (file.Read(rawDifBitmap) != rawDifBitmap.Length)
            {
                throw new Exception("Unable to read diff property bitmap");
            }

            hasDifs = true;

            foreach (var field in ObjectFields.GetTypeFields(type))
            {
                // Is it marked for diffs?
                ref readonly var fieldDef = ref ObjectFields.GetFieldDef(field);
                if ((difBitmap[fieldDef.bitmapBlockIdx] & fieldDef.bitmapMask) != 0)
                {
                    // Read the object field value
                    var newValue = ObjectFields.ReadFieldValue(field, file);
                    SetFieldValue(field, newValue);
                }
            }

            magicNumber = file.ReadUInt32();
            if (magicNumber != DiffMagicNumberEnd)
            {
                throw new Exception("Cannot read diff footer.");
            }
        }

        #endregion

        private bool ValidateFieldForType(obj_f field)
        {
            if (!ObjectFields.DoesTypeSupportField(type, field))
            {
                Logger.Error("Accessing unsupported field {0} ({1}) in type {2}",
                    field, (int) field, type);
                return false;
            }

            return true;
        }

        // Checks propBitmap1 for the given field
        private bool HasDataForField(in ObjectFieldDef field)
        {
            Trace.Assert(!IsProto());
            return (propCollBitmap[field.bitmapBlockIdx] & field.bitmapMask) != 0;
        }

        /// <summary>
        /// Indicates whether the object has it's own data for the given field or whether it's inheriting
        /// the value from it's prototype.
        /// </summary>
        public bool HasOwnDataForField(obj_f field)
        {
            if (IsProto())
            {
                return true;
            }

            ref readonly var fieldDef = ref ObjectFields.GetFieldDef(field);
            return HasDataForField(in fieldDef);
        }

        // Determines the packed index in the prop coll for the given field
        private int GetPropCollIdx(ObjectFieldDef field)
        {
            Trace.Assert(!IsProto());

            int count = 0;
            for (int i = 0; i < field.bitmapBlockIdx; ++i)
            {
                count += ArrayIndexBitmaps.Instance.PopCnt(propCollBitmap[i]);
            }

            count += ArrayIndexBitmaps.Instance.PopCntConstrained(
                propCollBitmap[field.bitmapBlockIdx],
                field.bitmapBitIdx
            );

            return count;
        }

        // Gets a readable storage location, possibly in the object_s prototype
        // if this object doesnt have the requested field
        private object GetFieldValue(obj_f field)
        {
            if (!ValidateFieldForType(field))
            {
                throw new Exception($"Trying to get field {field} on object {id}, whose type {type} doesn't support it.");
            }

            ref readonly var fieldDef = ref ObjectFields.GetFieldDef(field);

            if (IsProto())
            {
                return propCollection[fieldDef.protoPropIdx];
            }
            else if (ObjectFields.IsTransient(field))
            {
                return transientProps.GetFieldValue(field);
            }
            else if (HasDataForField(fieldDef))
            {
                var propCollIdx = GetPropCollIdx(fieldDef);
                return propCollection[propCollIdx];
            }
            else
            {
                // Fall back to the storage in the parent prototype
                var protoObj = GetProtoObj();
                return protoObj?.propCollection[fieldDef.protoPropIdx];
            }
        }

        private void SetFieldValue(obj_f field, object newValue)
        {
            if (!ValidateFieldForType(field))
            {
                throw new Exception($"Trying to get field {field} on object {id}, whose type {type} doesn't support it.");
            }

            ref readonly var fieldDef = ref ObjectFields.GetFieldDef(field);

            MarkChanged(field);

            if (IsProto())
            {
                ref var currentValue = ref propCollection[fieldDef.protoPropIdx];
                if (!ReferenceEquals(currentValue, newValue))
                {
                    FreeStorage(ref currentValue);
                    currentValue = newValue;
                }
            }
            else if (ObjectFields.IsTransient(field))
            {
                transientProps.SetFieldValue(field, newValue);
            }
            else if (HasDataForField(fieldDef))
            {
                var propCollIdx = GetPropCollIdx(fieldDef);

                ref var currentValue = ref propCollection[propCollIdx];
                if (!ReferenceEquals(currentValue, newValue))
                {
                    FreeStorage(ref currentValue);
                    currentValue = newValue;
                }

                currentValue = newValue;
            }
            else
            {
                // Allocate the storage location
                propCollBitmap[fieldDef.bitmapBlockIdx] |= fieldDef.bitmapMask;

                // TODO: This should just be a vector or similar so we can use the STL's insert function
                if (propCollection == null)
                {
                    propCollection = new object[1];
                }
                else
                {
                    Array.Resize(ref propCollection, propCollection.Length + 1);
                }

                var desiredIdx = GetPropCollIdx(fieldDef);

                for (var i = propCollection.Length - 1; i > desiredIdx; --i)
                {
                    propCollection[i] = propCollection[i - 1];
                }

                propCollection[desiredIdx] = newValue; // TODO Copy???
            }
        }

        private void MarkChanged(obj_f field)
        {
            if (IsProto())
            {
                return; // Dont mark prototype objects as changed
            }

            if (ObjectFields.IsTransient(field))
            {
                return; // Dont mark transient fields as changed
            }

            hasDifs = true;
            ref readonly var fieldDef = ref ObjectFields.GetFieldDef(field);
            difBitmap[fieldDef.bitmapBlockIdx] |= fieldDef.bitmapMask;
        }

        public int ProtoId => protoId.PrototypeId;

        // Resolves the proto object for this instance
        public GameObjectBody GetProtoObj()
        {
            if (protoId.IsPrototype)
            {
                return GameSystems.Proto.GetProtoById(protoId.PrototypeId);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        // Frees storage that may have been allocated to store a property of the given type
        public void FreeStorage(ref object storage)
        {
            if (storage == null)
            {
                return;
            }

            if (storage is IDisposable disposable)
            {
                // Referenced objects are not owned by this
                if (!(disposable is GameObjectBody))
                {
                    disposable.Dispose();
                }
            }

            storage = null;
        }

        private SparseArray<T> GetOrCreateSparseArray<T>(obj_f field) where T : struct
        {
            var value = (SparseArray<T>) GetFieldValue(field);

            if (IsProto())
            {
                if (value == null)
                {
                    value = new SparseArray<T>();
                    SetFieldValue(field, value);
                }
            }
            else
            {
                // Make sure to create a new value if the parent value belonged to the proto (otherwise it will
                // be mutated)
                var protoValue = (SparseArray<T>) GetProtoObj()?.GetFieldValue(field);
                if (value == null || ReferenceEquals(value, protoValue))
                {
                    if (protoValue != null)
                    {
                        // Copy the values present in the proto for mutation
                        value = (SparseArray<T>) protoValue.Copy();
                    }
                    else
                    {
                        value = new SparseArray<T>();
                    }

                    SetFieldValue(field, value);
                }
            }

            return value;
        }

        private SparseArray<int> GetMutableInt32Array(obj_f field)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.Int32Array
                         || ObjectFields.GetType(field) == ObjectFieldType.AbilityArray);

            return GetOrCreateSparseArray<int>(field);
        }

        private SparseArray<long> GetMutableInt64Array(obj_f field)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.Int64Array);

            return GetOrCreateSparseArray<long>(field);
        }

        private List<GameObjectBody> GetMutableObjectArray(obj_f field)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.ObjArray);
            if (_frozenObjRefs)
            {
                throw new InvalidOperationException("Cannot mutate an object id array of a frozen object");
            }

            // NOTE: An object id array cannot be inherited from a prototype
            ref readonly var fieldDef = ref ObjectFields.GetFieldDef(field);

            if (IsProto())
            {
                return (List<GameObjectBody>) propCollection[fieldDef.protoPropIdx];
            }
            else if (ObjectFields.IsTransient(field))
            {
                return (List<GameObjectBody>) transientProps.GetFieldValue(field);
            }

            if (!HasDataForField(fieldDef))
            {
                SetFieldValue(field, new List<GameObjectBody>());
            }

            var propCollIdx = GetPropCollIdx(fieldDef);
            return (List<GameObjectBody>) propCollection[propCollIdx];
        }

        private SparseArray<ObjectScript> GetMutableScriptArray(obj_f field)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.ScriptArray);

            return GetOrCreateSparseArray<ObjectScript>(field);
        }

        private List<SpellStoreData> GetMutableSpellArray(obj_f field)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.SpellArray);

            // NOTE: An object id array cannot be inherited from a prototype
            ref readonly var fieldDef = ref ObjectFields.GetFieldDef(field);

            if (IsProto())
            {
                if (propCollection[fieldDef.protoPropIdx] == null)
                {
                    propCollection[fieldDef.protoPropIdx] = new List<SpellStoreData>();
                }

                return (List<SpellStoreData>) propCollection[fieldDef.protoPropIdx];
            }
            else if (ObjectFields.IsTransient(field))
            {
                return (List<SpellStoreData>) transientProps.GetFieldValue(field);
            }

            if (!HasDataForField(fieldDef))
            {
                var protoObj = GameSystems.Proto.GetProtoById(ProtoId);
                SetFieldValue(field, new List<SpellStoreData>(protoObj.GetMutableSpellArray(field)));
            }

            var propCollIdx = GetPropCollIdx(fieldDef);
            return (List<SpellStoreData>) propCollection[propCollIdx];
        }

        /// <summary>
        /// Writes a field value to file.
        /// </summary>
        private static void WriteFieldToStream(ObjectFieldType type, object value, BinaryWriter stream)
        {
            switch (type)
            {
                case ObjectFieldType.Int32:
                    stream.WriteInt32((int) value);
                    break;
                case ObjectFieldType.Float32:
                    stream.Write((float) value);
                    break;
                case ObjectFieldType.Int64:
                    if (value != null)
                    {
                        stream.Write((byte) 1);
                        stream.Write((long) value);
                    }
                    else
                    {
                        stream.Write((byte) 0);
                    }

                    break;
                case ObjectFieldType.String:
                    if (value == null)
                    {
                        stream.Write((byte) 0);
                    }
                    else
                    {
                        stream.Write((byte) 1);

                        // Encode the string using the default encoding
                        var str = (string) value;
                        int length = Encoding.Default.GetByteCount(str);
                        Span<byte> encoded = stackalloc byte[length + 1];
                        Encoding.Default.GetBytes(str, encoded);
                        encoded[^1] = 0; // Ensure null-termination

                        stream.Write(length);

                        // ToEE writes the strlen, but includes the 0 byte anyway
                        stream.Write(encoded);
                    }

                    break;
                case ObjectFieldType.Obj:
                    if (value != null)
                    {
                        var objectId = (ObjectId) value;
                        stream.Write((byte) 1);
                        Trace.Assert(objectId.IsPersistable());
                        stream.WriteObjectId(objectId);
                    }
                    else
                    {
                        stream.Write((byte) 0);
                    }

                    break;
                case ObjectFieldType.UnkArray:
                case ObjectFieldType.Unk2Array:
                    throw new Exception("Can't write an unknown array type.");
                case ObjectFieldType.AbilityArray:
                case ObjectFieldType.Int32Array:
                case ObjectFieldType.Int64Array:
                case ObjectFieldType.ScriptArray:
                    if (value == null)
                    {
                        stream.Write((byte) 0);
                    }
                    else
                    {
                        stream.Write((byte) 1);

                        var sparseArray = (ISparseArray) value;
                        sparseArray.WriteTo(stream);
                    }

                    break;
                case ObjectFieldType.ObjArray:
                    WriteListAsSparseArray(stream, 24, WriteObjectId, (List<ObjectId>) value);
                    break;
                case ObjectFieldType.SpellArray:
                    WriteListAsSparseArray(stream, 32, WriteSpellStoreData, (List<SpellStoreData>) value);
                    break;
                default:
                    throw new Exception("Cannot write unknown field type to file.");
            }
        }

        [TempleDllLocation(0x10063f10)]
        public int GetItemInventoryLocation() => GetInt32(obj_f.item_inv_location);

        public bool TryGetEquipSlot(out EquipSlot slot)
        {
            var invLocation = GetItemInventoryLocation();
            return GameSystems.Item.TryGetSlotByInvIdx(invLocation, out slot);
        }

        public override string ToString()
        {
            if (!IsProto())
            {
                if (IsPC())
                {
                    var pcName = GetString(obj_f.pc_player_name);
                    return $"{pcName} (PC, Proto {ProtoId})";
                }
                else
                {
                    if (GameSystems.Description == null)
                    {
                        return $"{type} (Proto {ProtoId})";
                    }

                    if (type == ObjectType.key)
                    {
                        var keyId = GetInt32(obj_f.key_key_id);
                        return GameSystems.Description.GetKeyName(keyId) + $" ({type}, Proto {ProtoId})";
                    }

                    var descriptionId = GetInt32(obj_f.description);
                    return GameSystems.Description.Get(descriptionId) + $" ({type}, Proto {ProtoId})";
                }
            }
            else
            {
                return id.ToString();
            }
        }

        public bool ValidateInventory()
        {
            if (!GameSystems.Object.GetInventoryFields(type, out var idxField, out var countField))
            {
                return true;
            }

            var content = GetObjectArray(idxField);

            if (content.Count != GetInt32(countField))
            {
                Logger.Error("Count stored in {0} doesn't match actual item count of {1}.",
                    countField, idxField);
                return false;
            }

            for (var i = 0; i < content.Count; ++i)
            {
                var item = GetObject(idxField, i);

                var positional = $"Entry in {idxField}@{i} of {id}";

                if (item == null)
                {
                    Logger.Error("{0} is null", positional);
                    return false;
                }

                if (!GameSystems.Object.IsValidHandle(item))
                {
                    Logger.Error("{0} does is not registered with the object system.", positional);
                    return false;
                }

                if (item == this)
                {
                    Logger.Error("{0} is contained inside of itself.", positional);
                    return false;
                }

                // Only items are allowed in containers
                if (!item.IsItem())
                {
                    Logger.Error("{0} is not an item.", positional);
                    return false;
                }
            }

            return true;
        }
    }
}