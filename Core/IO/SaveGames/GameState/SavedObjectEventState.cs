using System.Collections.Generic;
using System.IO;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.MapSector;

namespace SpicyTemple.Core.IO.SaveGames.GameState
{
    public class SavedObjectEventState
    {
        public int NextObjectEventId { get; set; }

        [TempleDllLocation(0x100451b0)]
        public static SavedObjectEventState Read(BinaryReader reader)
        {
            var result = new SavedObjectEventState();

            var count = reader.ReadInt32();

            for (var i = 0; i < count; i++)
            {
            }

            result.NextObjectEventId = reader.ReadInt32();

            return result;
        }
    }

    public class SavedObjectEvent
    {
        public int Id { get; set; }

        public SectorLoc SectorLoc { get; set; }

        public ObjectId SourceObjectId { get; set; }

        public int EnterCallbackId { get; set; }

        public int LeaveCallbackId { get; set; }

        public ObjectListFilter ObjListFlags { get; set; }

        public float RadiusInch { get; set; }

        public float ConeAngleStart { get; set; }

        public float ConeRadians { get; set; }

        public List<ObjectId> PreviouslyAffected { get; set; } = new List<ObjectId>();

        [TempleDllLocation(0x10044ef0)]
        public static SavedObjectEvent Load(BinaryReader reader)
        {
            var result = new SavedObjectEvent();
            result.Id = reader.ReadInt32();
            var sectorLoc = reader.ReadUInt64();
            result.SectorLoc = SectorLoc.Unpack(sectorLoc);
            result.SourceObjectId = reader.ReadObjectId();
            result.EnterCallbackId = reader.ReadInt32();
            result.LeaveCallbackId = reader.ReadInt32();
            result.ObjListFlags = (ObjectListFilter) reader.ReadInt32();
            result.RadiusInch = reader.ReadSingle();
            result.ConeAngleStart = reader.ReadSingle();
            result.ConeRadians = reader.ReadSingle();

            var objectCount = reader.ReadInt32();
            for (var i = 0; i < objectCount; i++)
            {
                result.PreviouslyAffected.Add(reader.ReadObjectId());
            }

            return result;
        }
    }
}