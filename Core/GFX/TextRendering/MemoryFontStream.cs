using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SharpGen.Runtime;
using Vortice.DirectWrite;

namespace OpenTemple.Core.GFX.TextRendering
{
    internal class MemoryFontStream : CallbackBase, IDWriteFontFileStream
    {
        private readonly byte[] _data;

        private readonly IntPtr _dataPointer;

        private GCHandle _handle;

        public MemoryFontStream(byte[] data)
        {
            _data = data;
            _handle = GCHandle.Alloc(_data, GCHandleType.Pinned);
            _dataPointer = _handle.AddrOfPinnedObject();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _handle.Free();
            }
        }

        public void ReadFileFragment(out IntPtr fragmentStart, ulong fileOffset, ulong fragmentSize,
            out IntPtr fragmentContext)
        {
            Debug.Assert(fileOffset + fragmentSize <= (ulong) _data.Length);

            fragmentContext = IntPtr.Zero;
            fragmentStart = _dataPointer + (int) fileOffset;
        }

        public void GetFileSize(out ulong fileSize)
        {
            fileSize = (ulong) _data.Length;
        }

        public void GetLastWriteTime(out ulong lastWriteTime)
        {
            lastWriteTime = 0;
        }

        public void ReleaseFileFragment(IntPtr fragmentContext)
        {
        }
    }
}