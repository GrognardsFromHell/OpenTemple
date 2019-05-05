using System;

namespace SpicyTemple.Core.Systems.MapSector
{
    public struct SectorIterator
    {
        private readonly int _fromX, _fromY, _toX, _toY;
        private int _x, _y;

        public SectorIterator(int fromX, int toX, int fromY, int toY)
        {
            _fromX = fromX / Sector.SectorSideSize;
            _fromY = fromY / Sector.SectorSideSize;
            _toX = toX / Sector.SectorSideSize;
            _toY = toY / Sector.SectorSideSize;
            _x = _fromX;
            _y = _fromY;
        }

        public bool HasNext => _y <= _toY || (_y == _toY && _x <= _toX);

        public LockedMapSector Next()
        {
            var lockedSector = new LockedMapSector(_x, _y);
            if (++_x > _toX)
            {
                _x = _fromX;
                ++_y;
            }

            return lockedSector;
        }
    }
}