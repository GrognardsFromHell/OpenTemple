using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.GameObjects;

namespace SpicyTemple.Core.GameObject
{
// Stored in obj_f.script_idx array
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ObjectScript
    {
        public int unk1;
        public uint counters;
        public int scriptId;

        public bool Equals(ObjectScript other)
        {
            return unk1 == other.unk1 && counters == other.counters && scriptId == other.scriptId;
        }

        public override bool Equals(object obj)
        {
            return obj is ObjectScript other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = unk1;
                hashCode = (hashCode * 397) ^ (int) counters;
                hashCode = (hashCode * 397) ^ scriptId;
                return hashCode;
            }
        }

        public static bool operator ==(ObjectScript left, ObjectScript right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ObjectScript left, ObjectScript right)
        {
            return !left.Equals(right);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TransientProps : IDisposable
    {
        public int renderColor;
        public int renderColors;
        public int renderPalette;
        public int renderScale;
        public SparseArray<int> renderAlpha;
        public int renderX;
        public int renderY;
        public int renderWidth;
        public int renderHeight;
        public int palette;
        public int color;
        public int colors;
        public int renderFlags;
        public int tempId;
        public int lightHandle;
        public SparseArray<int> overlayLightHandles;
        public int internalFlags;
        public int findNode;
        public int animationHandle;
        public int grappleState;

        public void Dispose()
        {
            renderAlpha?.Dispose();
            renderAlpha = null;
            overlayLightHandles?.Dispose();
            overlayLightHandles = null;
        }

        public object GetFieldValue(obj_f field)
        {
            switch (field)
            {
                case obj_f.render_color:
                    return renderColor;
                case obj_f.render_colors:
                    return renderColors;
                case obj_f.render_palette:
                    return renderPalette;
                case obj_f.render_scale:
                    return renderScale;
                case obj_f.render_alpha:
                    return renderAlpha;
                case obj_f.render_x:
                    return renderX;
                case obj_f.render_y:
                    return renderY;
                case obj_f.render_width:
                    return renderWidth;
                case obj_f.render_height:
                    return renderHeight;
                case obj_f.palette:
                    return palette;
                case obj_f.color:
                    return color;
                case obj_f.colors:
                    return colors;
                case obj_f.render_flags:
                    return renderFlags;
                case obj_f.temp_id:
                    return tempId;
                case obj_f.light_handle:
                    return lightHandle;
                case obj_f.overlay_light_handles:
                    return overlayLightHandles;
                case obj_f.internal_flags:
                    return internalFlags;
                case obj_f.find_node:
                    return findNode;
                case obj_f.animation_handle:
                    return animationHandle;
                case obj_f.grapple_state:
                    return grappleState;
                default:
                    throw new ArgumentOutOfRangeException(nameof(field), field, null);
            }
        }

        public void SetFieldValue(obj_f field, object newValue)
        {
            switch (field)
            {
                case obj_f.render_color:
                    renderColor = (int) newValue;
                    break;
                case obj_f.render_colors:
                    renderColors = (int) newValue;
                    break;
                case obj_f.render_palette:
                    renderPalette = (int) newValue;
                    break;
                case obj_f.render_scale:
                    renderScale = (int) newValue;
                    break;
                case obj_f.render_alpha:
                    if (!ReferenceEquals(renderAlpha, newValue))
                    {
                        renderAlpha?.Dispose();
                    }

                    renderAlpha = (SparseArray<int>) newValue;
                    break;
                case obj_f.render_x:
                    renderX = (int) newValue;
                    break;
                case obj_f.render_y:
                    renderY = (int) newValue;
                    break;
                case obj_f.render_width:
                    renderWidth = (int) newValue;
                    break;
                case obj_f.render_height:
                    renderHeight = (int) newValue;
                    break;
                case obj_f.palette:
                    palette = (int) newValue;
                    break;
                case obj_f.color:
                    color = (int) newValue;
                    break;
                case obj_f.colors:
                    colors = (int) newValue;
                    break;
                case obj_f.render_flags:
                    renderFlags = (int) newValue;
                    break;
                case obj_f.temp_id:
                    tempId = (int) newValue;
                    break;
                case obj_f.light_handle:
                    lightHandle = (int) newValue;
                    break;
                case obj_f.overlay_light_handles:
                    if (!ReferenceEquals(overlayLightHandles, newValue))
                    {
                        overlayLightHandles?.Dispose();
                    }

                    overlayLightHandles = (SparseArray<int>) newValue;
                    break;
                case obj_f.internal_flags:
                    internalFlags = (int) newValue;
                    break;
                case obj_f.find_node:
                    findNode = (int) newValue;
                    break;
                case obj_f.animation_handle:
                    animationHandle = (int) newValue;
                    break;
                case obj_f.grapple_state:
                    grappleState = (int) newValue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(field), field, null);
            }
        }
    };


    public class GameObjectBody : IDisposable
    {
        private static readonly ILogger Logger = new ConsoleLogger();

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
        ObjHndl protoHandle;
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

#pragma region Type Tests
        public bool IsItem()
        {
            return type >= ObjectType.weapon & type <= ObjectType.generic || type == ObjectType.bag;
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

#pragma endregion

        public uint GetUInt32(obj_f field) => unchecked((uint) GetInt32(field));

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

        // This is a convenience version of GetObjectId
        [TempleDllLocation(0x1009E6D0)]
        public ObjHndl GetObjHndl(obj_f field)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.Obj);

            // Special case for prototype handle
            if (field == obj_f.prototype_handle)
            {
                return GetProtoHandle();
            }

            // Keep in mind that handles are stored in the form of ObjectIds
            var storageLoc = GetFieldValue(field);
            if (storageLoc == null)
            {
                return ObjHndl.Null;
            }

            var objectId = (ObjectId) storageLoc;
            if (!objectId.IsHandle)
            {
                return ObjHndl.Null;
            }

            return objectId.Handle;
        }

        // This gets the object handle and returns true, if it is valid (by validating it against the obj registry)
        // handleOut will always be set to the null handle if the handle is invalid.
        // If the handle is invalid, this function will also clear the storage location
        public bool GetValidObjHndl(obj_f field, out ObjHndl handleOut)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.Obj);

            // Special case for prototype handle
            if (field == obj_f.prototype_handle)
            {
                var protoHandle = GetProtoHandle();
                if (!GameSystems.Object.IsValidHandle(protoHandle))
                {
                    handleOut = ObjHndl.Null;
                    return false;
                }
                else
                {
                    handleOut = protoHandle;
                    return true;
                }
            }

            // Keep in mind that handles are stored in the form of ObjectIds
            var storageLoc = GetFieldValue(field);

            // Null handles are considered valid by this function
            if (storageLoc == null)
            {
                handleOut = ObjHndl.Null;
                return true;
            }

            var objectId = (ObjectId) storageLoc;
            if (objectId.IsNull)
            {
                handleOut = ObjHndl.Null;
                return true;
            }

            // If it's not a handle id, that is probably an error
            if (!objectId.IsHandle)
            {
                handleOut = ObjHndl.Null;
                return false;
            }

            // Use the handle from the id and validate it before returning it
            var handle = objectId.Handle;
            if (!GameSystems.Object.IsValidHandle(handle))
            {
                // Since we know the handle is invalid, we'll fix the issue in the object
                ResetField(field);

                handleOut = ObjHndl.Null;
                return false;
            }
            else
            {
                handleOut = handle;
                return true;
            }
        }

        public ObjectId GetObjectId(obj_f field)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.Obj);

            // Keep in mind that handles are stored in the form of ObjectIds
            var storageLoc = GetFieldValue(field);

            // Null handles are considered valid by this function
            if (storageLoc == null)
            {
                return ObjectId.CreateNull();
            }

            return (ObjectId) storageLoc;
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

        public ArrayAccess<ObjectId> GetObjectIdArray(obj_f field)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.ObjArray);

            var backingArray = (SparseArray<ObjectId>) GetFieldValue(field);
            return new ArrayAccess<ObjectId>(this, backingArray);
        }

        public ArrayAccess<ObjectScript> GetScriptArray(obj_f field)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.ScriptArray);

            var backingArray = (SparseArray<ObjectScript>) GetFieldValue(field);
            return new ArrayAccess<ObjectScript>(this, backingArray);
        }

        public ArrayAccess<SpellStoreData> GetSpellArray(obj_f field)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.SpellArray);

            var backingArray = (SparseArray<SpellStoreData>) GetFieldValue(field);
            return new ArrayAccess<SpellStoreData>(this, backingArray);
        }

        // Convenience array accessors
        public int GetInt32(obj_f field, int index)
        {
            return GetInt32Array(field)[index];
        }

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

        public ObjectId GetObjectId(obj_f field, int index)
        {
            return GetObjectIdArray(field)[index];
        }

        public void SetObjectId(obj_f field, int index, in ObjectId value)
        {
            GetMutableObjectIdArray(field)[index] = value;
        }

        public void RemoveObjectId(obj_f field, int index)
        {
            GetMutableObjectIdArray(field).Remove(index);
        }

        public ObjHndl GetObjHndl(obj_f field, int index)
        {
            var objId = GetObjectId(field, index);
            if (objId.IsHandle)
            {
                return objId.Handle;
            }

            return ObjHndl.Null;
        }

        public GameObjectBody GetObject(obj_f field, int index)
        {
            var objId = GetObjectId(field, index);
            if (objId.IsHandle)
            {
                return GameSystems.Object.GetObject(objId.Handle);
            }

            return null;
        }

        public GameObjectBody GetObject(obj_f field)
        {
            var objId = GetObjectId(field);
            if (objId.IsHandle)
            {
                return GameSystems.Object.GetObject(objId.Handle);
            }

            return null;
        }

        public bool GetValidObjHndl(obj_f field, int index, out ObjHndl handleOut)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.ObjArray);

            // Get the stored object id
            var objId = GetObjectId(field, index);

            // Null handles are considered valid by this function
            if (objId.IsNull)
            {
                handleOut = ObjHndl.Null;
                return true;
            }

            // If it's not a handle id, that is probably an error
            if (!objId.IsHandle)
            {
                handleOut = ObjHndl.Null;
                return false;
            }

            // Use the handle from the id and validate it before returning it
            var handle = objId.Handle;
            if (!GameSystems.Object.IsValidHandle(handle))
            {
                // Since we know the handle is invalid, we'll fix the issue in the object
                RemoveObjectId(field, index);
                handleOut = ObjHndl.Null;
                return false;
            }
            else
            {
                handleOut = handle;
                return true;
            }
        }

        public void SetObjHndl(obj_f field, int index, ObjHndl value)
        {
            if (!value)
            {
                SetObjectId(field, index, ObjectId.CreateNull());
            }
            else
            {
                SetObjectId(field, index, ObjectId.CreateHandle(value));
            }
        }

        public void SetObject(obj_f field, int index, GameObjectBody obj)
        {
            if (obj == null)
            {
                SetObjectId(field, index, ObjectId.CreateNull());
            }
            else
            {
                SetObjectId(field, index, ObjectId.CreateHandle(GameSystems.Object.GetHandleById(obj.id)));
            }
        }

        public void SetObject(obj_f field, GameObjectBody obj)
        {
            if (obj == null)
            {
                SetObjectId(field, ObjectId.CreateNull());
            }
            else
            {
                SetObjectId(field, ObjectId.CreateHandle(GameSystems.Object.GetHandleById(obj.id)));
            }
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
            GetMutableSpellArray(field).Remove(index);
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
                    GetMutableObjectIdArray(field).Clear();
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

        public void SetObjHndl(obj_f field, ObjHndl value)
        {
            if (!value)
            {
                SetObjectId(field, ObjectId.CreateNull());
            }
            else
            {
                SetObjectId(field, ObjectId.CreateHandle(value));
            }
        }

        public void SetObjectId(obj_f field, ObjectId id)
        {
            // TODO: Additional validation regarding internal flags that checks whether
            // this id is valid for the id storage state of this object (persistent ids vs. handles)
            Trace.Assert(ObjectFields.GetFieldDef(field).type == ObjectFieldType.Obj);
            SetFieldValue(field, id);
        }

        // Internal flags
        public uint GetInternalFlags()
        {
            Trace.Assert(!IsProto());
            return unchecked((uint) transientProps.internalFlags);
        }

        public void SetInternalFlags(uint internalFlags)
        {
            Trace.Assert(!IsProto());
            transientProps.internalFlags = unchecked((int) internalFlags);
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
                for (var i = idx; i < propCollection.Length; ++i)
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
            var internalFlags = GetInternalFlags();

            if ((internalFlags & 1) == 0)
            {
                Logger.Info("Object references in object {0} are already unfrozen.", id);
                return;
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
                    var handle = GameSystems.Object.GetHandleById((ObjectId) currentValue);
                    SetObjectId(field, handle ? ObjectId.CreateHandle(handle) : ObjectId.CreateNull());
                }
                else if (fieldType == ObjectFieldType.ObjArray)
                {
                    var objectIdArray = (SparseArray<ObjectId>) currentValue;
                    objectIdArray.ForEachIndex(idx =>
                    {
                        var handle = GameSystems.Object.GetHandleById(objectIdArray[idx]);
                        if (handle)
                        {
                            objectIdArray[idx] = ObjectId.CreateHandle(handle);
                        }
                        else
                        {
                            objectIdArray[idx] = ObjectId.CreateNull();
                        }
                    });
                }

                return true;
            });

            // Mark as not frozen
            internalFlags &= unchecked((uint) ~1);
            SetInternalFlags(internalFlags);
        }

        /**
         * If this object is currently storing references to other objects as handles, those references will
         * be converted to persistable object ids.
         */
        [TempleDllLocation(0x100a1080)]
        public void FreezeIds()
        {
            var internalFlags = GetInternalFlags();

            if ((internalFlags & 1) == 1)
            {
                Logger.Info("Object references in object {0} are already frozen.", id.ToString());
                return;
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
                    var objectId = (ObjectId) currentValue;
                    if (objectId.IsHandle)
                    {
                        var obj = GameSystems.Object.GetObject(GameSystems.Object.GetHandleById(objectId));
                        SetObjectId(field, obj.id);
                    }
                }
                else if (fieldType == ObjectFieldType.ObjArray)
                {
                    var objectIdArray = (SparseArray<ObjectId>) currentValue;
                    objectIdArray.ForEachIndex(idx =>
                    {
                        var objId = objectIdArray[idx];
                        if (objId.IsHandle)
                        {
                            var obj = GameSystems.Object.GetObject(GameSystems.Object.GetHandleById(objId));
                            objectIdArray[idx] = obj.id;
                        }
                    });
                }

                return true;
            });

            // Mark as frozen
            internalFlags |= 1;
            SetInternalFlags(internalFlags);
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
            obj.protoHandle = protoHandle;
            obj.type = type;

            obj.hasDifs = false;
            obj.propCollection = new object [obj.propCollection.Length];

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
                        obj.SetFieldValue(field, currentValue);
                        break;
                    case ObjectFieldType.Obj:
                        // TODO: When ObjectId becomes a readonly struct, this copy will not be necessary
                        obj.SetFieldValue(field, (ObjectId) currentValue);
                        break;
                    case ObjectFieldType.AbilityArray:
                    case ObjectFieldType.UnkArray:
                    case ObjectFieldType.Int32Array:
                    case ObjectFieldType.Int64Array:
                    case ObjectFieldType.ScriptArray:
                    case ObjectFieldType.Unk2Array:
                    case ObjectFieldType.ObjArray:
                    case ObjectFieldType.SpellArray:
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

#pragma region Object Field Getters and Setters
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

        public ItemFlag GetItemFlags() => (ItemFlag) GetUInt32(obj_f.item_flags);

        public SceneryFlag GetSceneryFlags() => (SceneryFlag) GetUInt32(obj_f.scenery_flags);

        public void SetSceneryFlags(SceneryFlag flags) => SetUInt32(obj_f.scenery_flags, (uint) flags);

        public ContainerFlag GetContainerFlags() => (ContainerFlag) GetUInt32(obj_f.container_flags);

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

        public SecretDoorFlag GetSecretDoorFlags() => (SecretDoorFlag) GetUInt32(obj_f.secretdoor_flags);

        public ItemWearFlag GetItemWearFlags() => (ItemWearFlag) GetUInt32(obj_f.item_wear_flags);

        public WeaponType GetWeaponType() => (WeaponType) GetUInt32(obj_f.weapon_type);

        public WeaponFlag WeaponFlags
        {
            get => (WeaponFlag) GetUInt32(obj_f.weapon_flags);
            set => SetUInt32(obj_f.weapon_flags, (uint) value);
        }

        public ArmorFlag GetArmorFlags() => (ArmorFlag) GetUInt32(obj_f.armor_flags);

        public PortalFlag GetPortalFlags() => (PortalFlag) GetUInt32(obj_f.portal_flags);

        public CritterFlag GetCritterFlags() => (CritterFlag) GetUInt32(obj_f.critter_flags);

        public void SetCritterFlags(CritterFlag flags) => SetUInt32(obj_f.critter_flags, (uint) flags);

        public locXY GetLocation()
        {
            return locXY.fromField(unchecked((ulong) GetInt64(obj_f.location)));
        }

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

#pragma endregion

#pragma region NPC Field Getters and Setters
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

#pragma endregion

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

#pragma region Transient Property Accessors
        public int TemporaryId => transientProps.tempId;

        public float OffsetX => GetFloat(obj_f.offset_x);

        public float OffsetY => GetFloat(obj_f.offset_y);

        public float OffsetZ => GetFloat(obj_f.offset_z);

        public float Rotation => GetFloat(obj_f.rotation);

        public float RotationPitch => GetFloat(obj_f.rotation_pitch);

#pragma endregion

#pragma region Persistence
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
                var value = ReadFieldValue(field, reader);
                obj.SetFieldValue(field, value);
                return true;
            });

            obj.SetInternalFlags(1); // Storing persistent IDs at the moment

            return obj;
        }

        private static object ReadFieldValue(obj_f field, BinaryReader reader)
        {
            var type = ObjectFields.GetType(field);

            byte dataPresent;
            switch (type)
            {
                case ObjectFieldType.Int32:
                    return reader.ReadInt32();
                case ObjectFieldType.Float32:
                    return reader.ReadSingle();
                case ObjectFieldType.Int64:
                    dataPresent = reader.ReadByte();

                    if (dataPresent == 0)
                    {
                        return null;
                    }

                    return reader.ReadInt64();
                case ObjectFieldType.Obj:
                    dataPresent = reader.ReadByte();

                    if (dataPresent == 0)
                    {
                        return null;
                    }

                    var objIdValue = reader.ReadObjectId();
                    if (!objIdValue.IsPersistable())
                    {
                        throw new Exception($"Read an invalid object id {objIdValue} for field {field}");
                    }

                    return objIdValue;
                case ObjectFieldType.String:
                    dataPresent = reader.ReadByte();
                    if (dataPresent == 0)
                    {
                        return null;
                    }

                    // The string length excludes the null-byte, but the data does include it
                    var strLen = reader.ReadInt32();
                    Span<byte> str = stackalloc byte[strLen + 1];
                    reader.Read(str);
                    return Encoding.Default.GetString(str.Slice(0, str.Length - 1));
                case ObjectFieldType.AbilityArray:
                case ObjectFieldType.Int32Array:
                    return ReadSparseArray<int>(reader);
                case ObjectFieldType.Int64Array:
                    return ReadSparseArray<long>(reader);
                case ObjectFieldType.ScriptArray:
                    return ReadSparseArray<ObjectScript>(reader);
                case ObjectFieldType.ObjArray:
                    return ReadSparseArray<ObjectId>(reader);
                case ObjectFieldType.SpellArray:
                    return ReadSparseArray<SpellStoreData>(reader);
                default:
                    throw new Exception($"Cannot deserialize field type {type}");
            }
        }

        private static SparseArray<T> ReadSparseArray<T>(BinaryReader reader) where T : unmanaged
        {
            var dataPresent = reader.ReadByte();
            if (dataPresent == 0)
            {
                return null;
            }

            return SparseArray<T>.ReadFrom(reader);
        }

        private const uint DiffMagicNumberStart = 0x12344321;
        private const uint DiffMagicNumberEnd = 0x23455432;

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

        public void LoadDiffsFromFile(ObjHndl handle, BinaryReader file)
        {
            if ((transientProps.internalFlags & 1) == 0)
            {
                throw new Exception("Cannot load difs for an object that is not storing persistable ids.");
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
                    GameSystems.Object.AddToIndex(id, handle);
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

            // TODO: Make more efficient
            ObjectFields.IterateTypeFields(type, field =>
            {
                // Is it marked for diffs?
                ref readonly var fieldDef = ref ObjectFields.GetFieldDef(field);
                if ((difBitmap[fieldDef.bitmapBlockIdx] & fieldDef.bitmapMask) == 0)
                {
                    return true;
                }

                // Read the object field value
                var newValue = ReadFieldValue(field, file);
                SetFieldValue(field, newValue);
                return true;
            });

            magicNumber = file.ReadUInt32();
            if (magicNumber != DiffMagicNumberEnd)
            {
                throw new Exception("Cannot read diff footer.");
            }

            GameSystems.Object.SpatialIndex.UpdateLocation(this);
        }
#pragma endregion

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
                throw new Exception($"Trying to get field {field} on object {id}, whose type doesn't support it.");
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
                throw new Exception($"Trying to get field {field} on object {id}, whose type doesn't support it.");
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

        // Resolves the proto handle for this instance
        public ObjHndl GetProtoHandle()
        {
            if (!protoHandle)
            {
                protoHandle = GameSystems.Object.GetHandleById(protoId);
            }

            return protoHandle;
        }

        public int ProtoId => protoId.PrototypeId;

        // Resolves the proto object for this instance
        public GameObjectBody GetProtoObj()
        {
            if (protoId.IsPrototype)
            {
                return GameSystems.Object.GetObject(protoId);
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
                disposable.Dispose();
                storage = null;
                return;
            }
        }

        private SparseArray<T> GetOrCreateSparseArray<T>(obj_f field) where T : unmanaged
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

        private SparseArray<ObjectId> GetMutableObjectIdArray(obj_f field)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.ObjArray);

            return GetOrCreateSparseArray<ObjectId>(field);
        }

        private SparseArray<ObjectScript> GetMutableScriptArray(obj_f field)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.ScriptArray);

            return GetOrCreateSparseArray<ObjectScript>(field);
        }

        private SparseArray<SpellStoreData> GetMutableSpellArray(obj_f field)
        {
            Trace.Assert(ObjectFields.GetType(field) == ObjectFieldType.SpellArray);

            return GetOrCreateSparseArray<SpellStoreData>(field);
        }

        /// <summary>
        /// Writes a field value to file.
        /// </summary>
        private static void WriteFieldToStream(ObjectFieldType type, object value, BinaryWriter stream)
        {
            switch (type)
            {
                case ObjectFieldType.Int32:
                    stream.Write((int) value);
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
                        Span<byte> encoded = stackalloc byte[length];
                        Encoding.Default.GetBytes(str, encoded);
                        encoded[encoded.Length - 1] = 0; // Ensure null-termination

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
                case ObjectFieldType.ObjArray:
                case ObjectFieldType.SpellArray:
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
                default:
                    throw new Exception("Cannot write unknown field type to file.");
            }
        }

        [TempleDllLocation(0x10063f10)]
        public int GetItemInventoryLocation() => GetInt32(obj_f.item_inv_location);

        public override string ToString()
        {
            if (!IsProto())
            {
                return $"{id} (Proto {ProtoId})";
            }
            else
            {
                return id.ToString();
            }
        }
    }
}