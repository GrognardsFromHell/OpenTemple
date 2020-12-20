using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenTemple.Core.IO;

namespace OpenTemple.Core.GameObject
{
    public class SavedGameObject
    {
        public int ProtoId { get; set; }

        public ObjectId Id { get; set; }

        public ObjectType Type { get; set; }

        public Dictionary<obj_f, object> Properties { get; set; }

        [TempleDllLocation(0x100a11a0)]
        public static SavedGameObject Load(BinaryReader reader)
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

            var result = new SavedGameObject {ProtoId = protoId.protoId, Id = objId, Type = typeCode};

            // Initialize and load bitmaps
            var bitmapLen = ObjectFields.GetBitmapBlockCount(result.Type);

            var propCount = reader.ReadUInt16();

            var propCollBitmap = new uint[bitmapLen];
            reader.Read(MemoryMarshal.Cast<uint, byte>(propCollBitmap));

            // Validate that the property bitmap has the same number of bits enabled as the number of serialized properties
            var enabledBitCount = CountPropBitmap(propCollBitmap);
            if (enabledBitCount != propCount)
            {
                throw new Exception($"Mismatch between serialized property count {propCount} " +
                                    $"and enabled bits in property bitmap {enabledBitCount}");
            }

            var properties = new Dictionary<obj_f, object>();
            foreach (var field in ObjectFields.GetTypeFields(typeCode))
            {
                ref readonly var fieldDef = ref ObjectFields.GetFieldDef(field);

                // Does this object have the prop?
                if ((propCollBitmap[fieldDef.bitmapBlockIdx] & fieldDef.bitmapMask) != 0)
                {
                    properties[field] = ObjectFields.ReadFieldValue(fieldDef.type, reader);
                }
            }

            result.Properties = properties;
            return result;
        }

        private static int CountPropBitmap(ReadOnlySpan<uint> bitmap)
        {
            var propsSet = 0;
            foreach (var u in bitmap)
            {
                propsSet += BitOperations.PopCount(u);
            }

            return propsSet;
        }
    }
}