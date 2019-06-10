using System.Collections.Generic;
using System.Diagnostics;
using SharpDX;
using SharpDX.DirectWrite;

namespace SpicyTemple.Core.GFX.TextRendering
{
    internal class FontLoader : CallbackBase, FontCollectionLoader, FontFileEnumerator, FontFileLoader
    {
        private readonly Factory _factory;

        private readonly List<FontFile> _fonts;

        private readonly DataStream _keyStream = new DataStream(sizeof(int), true, true);

        public FontLoader(Factory factory, List<FontFile> fonts)
        {
            _factory = factory;
            _fonts = fonts;
        }

        public FontFileEnumerator CreateEnumeratorFromKey(Factory factory, DataPointer collectionKey)
        {
            _streamIndex = 0;
            return this;
        }

        public FontFileStream CreateStreamFromKey(DataPointer fontFileReferenceKey)
        {
            using var keyStream = fontFileReferenceKey.ToDataStream();
            Trace.Assert(keyStream.Length == sizeof(int));
            var index = keyStream.Read<int>();

            var font = _fonts[index];
            return new MemoryFontStream(font.Data);
        }

        public bool MoveNext()
        {
            var hasCurrentFile = false;

            if (_streamIndex < _fonts.Count)
            {
                _keyStream.Position = 0;
                _keyStream.Write(_streamIndex);

                CurrentFontFile?.Dispose();
                CurrentFontFile = new SharpDX.DirectWrite.FontFile(
                    _factory,
                    _keyStream.DataPointer,
                    (int) _keyStream.Length,
                    this
                );

                hasCurrentFile = true;
                ++_streamIndex;
            }

            return hasCurrentFile;
        }

        public SharpDX.DirectWrite.FontFile CurrentFontFile { get; private set; }

        private int _streamIndex;
    }
}