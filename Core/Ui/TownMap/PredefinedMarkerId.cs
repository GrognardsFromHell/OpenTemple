using System;

namespace OpenTemple.Core.Ui.TownMap
{
    /// <summary>
    /// Identifies predefined markers by the map they're on, and their id.
    /// </summary>
    public readonly struct PredefinedMarkerId
    {

        public readonly int MapId;

        public readonly int MarkerId;

        public PredefinedMarkerId(int mapId, int markerId)
        {
            MapId = mapId;
            MarkerId = markerId;
        }

        public bool Equals(PredefinedMarkerId other)
        {
            return MapId == other.MapId && MarkerId == other.MarkerId;
        }

        public override bool Equals(object obj)
        {
            return obj is PredefinedMarkerId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MapId, MarkerId);
        }
    }
}