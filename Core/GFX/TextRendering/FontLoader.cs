using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SharpGen.Runtime;
using Vortice.DirectWrite;

namespace OpenTemple.Core.GFX.TextRendering
{
    internal class FontLoader : CallbackBase, IDWriteFontCollectionLoader, IDWriteFontFileEnumerator, IDWriteFontFileLoader
    {
        private readonly IDWriteFactory _factory;

        private readonly List<FontFile> _fonts;

        public FontLoader(IDWriteFactory factory, List<FontFile> fonts)
        {
            _factory = factory;
            _fonts = fonts;
        }

        public IDWriteFontFileEnumerator CreateEnumeratorFromKey(IDWriteFactory factory, IntPtr collectionKey, int size)
        {
            _streamIndex = 0;
            return this;
        }

        public void CreateStreamFromKey(IntPtr fontFileReferenceKey, int fontFileReferenceKeySize,
            out IDWriteFontFileStream fontFileStream)
        {
            Trace.Assert(fontFileReferenceKeySize == sizeof(int));
            var index = Marshal.ReadInt32(fontFileReferenceKey);

            var font = _fonts[index];
            fontFileStream = new MemoryFontStream(font.Data);
        }

        public unsafe bool MoveNext()
        {
            var hasCurrentFile = false;

            if (_streamIndex < _fonts.Count)
            {
                Span<int> key = stackalloc int[1] {_streamIndex};

                CurrentFontFile?.Dispose();
                fixed (void* keyPtr = key)
                {
                    CurrentFontFile = _factory.CreateCustomFontFileReference(
                        new IntPtr(keyPtr),
                        sizeof(int),
                        this
                    );
                }

                hasCurrentFile = true;
                ++_streamIndex;
            }

            return hasCurrentFile;
        }

        public IDWriteFontFile CurrentFontFile { get; private set; }

        private int _streamIndex;
    }
}