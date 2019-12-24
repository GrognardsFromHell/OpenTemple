using System;
using SharpDX;
using SharpDX.DirectWrite;

namespace OpenTemple.Core.GFX.TextRendering
{
    internal class MemoryFontStream : CallbackBase, FontFileStream
    {
        private readonly DataStream _stream;

        public MemoryFontStream(byte[] data)
        {
            _stream = new DataStream(data.Length, true, true);
            _stream.WriteRange(data);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _stream.Dispose();
            }
        }

        void FontFileStream.ReadFileFragment(out IntPtr fragmentStart, long fileOffset, long fragmentSize,
            out IntPtr fragmentContext)
        {
            lock (this)
            {
                fragmentContext = IntPtr.Zero;
                _stream.Position = fileOffset;
                fragmentStart = _stream.PositionPointer;
            }
        }

        void FontFileStream.ReleaseFileFragment(IntPtr fragmentContext)
        {
        }

        long FontFileStream.GetFileSize()
        {
            return _stream.Length;
        }

        long FontFileStream.GetLastWriteTime()
        {
            return 0;
        }
    }
}