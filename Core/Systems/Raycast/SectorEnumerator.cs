using System;
using System.Diagnostics;
using System.Drawing;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.MapSector;

namespace SpicyTemple.Core.Systems.Raycast
{
    public struct SectorEnumerator : IDisposable
    {
        private readonly Rectangle _tileRectangle;

        private readonly int _endX;

        private readonly int _endY;

        private int _currentX;

        private int _currentY;

        private LockedMapSector _lockedSector;

        private readonly bool _lockSectors;

        public SectorEnumerator(TileRect tileRect, bool lockSectors = true) : this()
        {
            _tileRectangle = new Rectangle(
                tileRect.x1,
                tileRect.y1,
                tileRect.x2 - tileRect.x1,
                tileRect.y2 - tileRect.y1
            );
            _currentX = _tileRectangle.Left;
            _currentY = _tileRectangle.Top;
            _endX = _tileRectangle.Right;
            _endY = _tileRectangle.Bottom;
            _lockSectors = lockSectors;
        }

        /**
         * The tile rectangle being enumerated.
         */
        public SectorEnumerator(Rectangle tileRectangle, bool lockSectors = true) : this()
        {
            _tileRectangle = tileRectangle;
            _currentX = _tileRectangle.Left;
            _currentY = _tileRectangle.Top;
            _endX = _tileRectangle.Right;
            _endY = _tileRectangle.Bottom;
            _lockSectors = lockSectors;
        }

        public bool MoveNext()
        {
            _lockedSector.Dispose();

            if (_currentX >= _endX || _currentY >= _endY)
            {
                return false;
            }

            var secX = _currentX / 64;
            var remX = _currentX % 64;
            var secY = _currentY / 64;
            var remY = _currentY % 64;
            var w = Math.Min(64 - remX, _endX - _currentX);
            var h = Math.Min(64 - remY, _endY - _currentY);

            var sectorLoc = new SectorLoc(secX, secY);
            if (_lockSectors)
            {
                _lockedSector = new LockedMapSector(sectorLoc);
            }

            Current = new PartialSector(
                sectorLoc,
                w < 64 || h < 64,
                new Rectangle(remX, remY, w, h),
                _lockedSector
            );

            _currentX += w;
            if (_currentX >= _endX)
            {
                _currentX = _tileRectangle.Left;
                _currentY += h;
            }

            return true;
        }

        public PartialSector Current { get; private set; }

        public void Dispose()
        {
            _lockedSector.Dispose();
        }
    }
}