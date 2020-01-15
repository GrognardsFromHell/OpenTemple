using System.Collections.Generic;
using System.IO;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.MapSector;

namespace OpenTemple.Core.IO.SaveGames.GameState
{
    public class SavedObjectEventState
    {
        public int NextObjectEventId { get; set; }

        public List<SavedObjectEvent> Events { get; set; }

        [TempleDllLocation(0x100451b0)]
        public static SavedObjectEventState Read(BinaryReader reader)
        {
            var result = new SavedObjectEventState();

            var count = reader.ReadInt32();
            result.Events = new List<SavedObjectEvent>(count);

            for (var i = 0; i < count; i++)
            {
                result.Events.Add(SavedObjectEvent.Load(reader));
            }

            result.NextObjectEventId = reader.ReadInt32();

            return result;
        }

        [TempleDllLocation(0x100456d0)]
        public void Write(BinaryWriter writer)
        {
            writer.WriteInt32(Events.Count);
            foreach (var evt in Events)
            {
                evt.Save(writer);
            }

            writer.WriteInt32(NextObjectEventId);
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

        [TempleDllLocation(0x10045480)]
        public void Save(BinaryWriter writer)
        {
            writer.WriteInt32(Id);
            writer.WriteUInt64(SectorLoc.Pack());
            writer.WriteObjectId(SourceObjectId);
            writer.WriteInt32(EnterCallbackId);
            writer.WriteInt32(LeaveCallbackId);
            writer.WriteInt32((int) ObjListFlags);
            writer.WriteSingle(RadiusInch);
            writer.WriteSingle(ConeAngleStart);
            writer.WriteSingle(ConeRadians);

            writer.WriteInt32(PreviouslyAffected.Count);
            foreach (var objectId in PreviouslyAffected)
            {
                writer.WriteObjectId(objectId);
            }
        }
    }
}