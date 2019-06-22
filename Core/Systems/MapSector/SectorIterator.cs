using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.GameObjects;

namespace SpicyTemple.Core.Systems.MapSector
{
    public struct SectorIterator : IDisposable
    {
        private readonly int _fromX, _fromY, _toX, _toY;
        private int _x, _y;
        private LockedMapSector _lockedMapSector;

        public SectorIterator(int fromX, int toX, int fromY, int toY)
        {
            _lockedMapSector = default;
            _fromX = fromX / Sector.SectorSideSize;
            _fromY = fromY / Sector.SectorSideSize;
            _toX = toX / Sector.SectorSideSize;
            _toY = toY / Sector.SectorSideSize;
            _x = _fromX;
            _y = _fromY;
        }

        public SectorIterator(TileRect tileRect) : this(tileRect.x1, tileRect.x2, tileRect.y1, tileRect.y2)
        {
        }

        public bool HasNext => _y <= _toY || (_y == _toY && _x <= _toX);

        public LockedMapSector Next()
        {
            _lockedMapSector.Dispose();

            _lockedMapSector = new LockedMapSector(_x, _y);
            if (++_x > _toX)
            {
                _x = _fromX;
                ++_y;
            }

            return _lockedMapSector;
        }

        public void Dispose()
        {
            _lockedMapSector.Dispose();
        }

        public IEnumerable<LockedMapSector> EnumerateSectors()
        {
            while (HasNext)
            {
                yield return Next();
            }
        }
        public IEnumerable<GameObjectBody> EnumerateObjects()
        {
            while (HasNext)
            {
                var sector = Next();
                foreach (var obj in sector.EnumerateObjects())
                {
                    yield return obj;
                }
            }
        }
    }
}