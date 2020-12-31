using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Avalonia.Media;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO.Fonts;
using OpenTemple.Core.IO.Images;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Utils;
using SkiaSharp;

namespace OpenTemple.Core.TigSubsystems
{
    // Fonts shipping with the base game
    public enum PredefinedFont
    {
        ARIAL_10,
        ARIAL_12,
        ARIAL_BOLD_10,
        ARIAL_BOLD_24,
        PRIORY_12,
        SCURLOCK_48
    }

    public sealed class TigFonts : IDisposable
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private readonly Dictionary<string, TigFont> _fonts = new Dictionary<string, TigFont>();

        private readonly Stack<TigFont> _fontStack = new Stack<TigFont>(10);

        public bool FontIsEnglish { get; set; } = true;

        /// <summary>
        /// Loads all .fnt files found in the given directory.
        /// </summary>
        [TempleDllLocation(0x101EA1F0)]
        public void LoadAllFrom(string path)
        {
            var textures = Tig.RenderingDevice.GetTextures();

            var dirEntries = Tig.FS.ListDirectory(path);

            foreach (var dirEntry in dirEntries)
            {
                var dirEntryPath = Path.Join(path, dirEntry);

                if (dirEntry.EndsWith(".fnt"))
                {
                    var fontFaceData = Tig.FS.ReadBinaryFile(dirEntryPath);
                    var fontFace = FontFaceReader.Read(fontFaceData);

                    var fontTextures = new ResourceRef<ITexture>[fontFace.FontArtCount];
                    var fontAlphaMasks = new SKImage[fontFace.FontArtCount];
                    for (var i = 0; i < fontTextures.Length; i++)
                    {
                        var fontArtPath = Path.Combine(path, $"{fontFace.Name}_{i:D4}.fntart");
                        fontTextures[i] = textures.Resolve(fontArtPath, false);
                        fontAlphaMasks[i] = ImageIO.DecodeAlphaMask(Tig.FS, fontArtPath).ToSkImage();
                    }

                    var font = new TigFont(fontFace, fontTextures, fontAlphaMasks);
                    _fonts[fontFace.Name] = font;

                    Logger.Debug($"Loaded font name='{fontFace.Name}' size={fontFace.Size}");
                }
                else if (Tig.FS.DirectoryExists(dirEntryPath))
                {
                    // Recurse into subdirectories
                    LoadAllFrom(dirEntryPath);
                }
            }
        }

        /// <summary>
        /// Pushes a font for further text rendering.
        /// </summary>
        [TempleDllLocation(0x101e89d0)]
        public void PushFont(PredefinedFont font)
        {
            switch (font)
            {
                case PredefinedFont.ARIAL_10:
                    PushFont("arial-10", 10);
                    break;
                case PredefinedFont.ARIAL_12:
                    PushFont("arial-12", 12);
                    break;
                case PredefinedFont.ARIAL_BOLD_10:
                    PushFont("arial-bold-10", 10);
                    break;
                case PredefinedFont.ARIAL_BOLD_24:
                    PushFont("arial-bold-24", 24);
                    break;
                case PredefinedFont.PRIORY_12:
                    PushFont("priory-12", 12);
                    break;
                case PredefinedFont.SCURLOCK_48:
                    PushFont("scurlock-48", 48);
                    break;
                default:
                    throw new ArgumentException("Unknown font literal was used!");
            }
        }

        public TigFont FindFont(string faceName, int pixelSize)
        {
            if (_fonts.TryGetValue(faceName, out var font))
            {
                if (pixelSize == 0 || font.FontFace.Size == pixelSize)
                {
                    return font;
                }
            }

            return null;
        }

        /// <summary>
        /// Pushes a custom font for further text rendering.
        /// </summary>
        public void PushFont(string faceName, int pixelSize)
        {
            var font = FindFont(faceName, pixelSize);
            if (font != null)
            {
                _fontStack.Push(font);
            }
            else
            {
                Logger.Warn($"Couldn't find font '{faceName}' size={pixelSize}");
            }
        }

        /// <summary>
        /// Pops the last pushed font from the font stack.
        /// </summary>
        [TempleDllLocation(0x101e8ac0)]
        public void PopFont()
        {
            _fontStack.Pop();
            if (_fontStack.Count == 0)
            {
                Logger.Warn("Popped last element from font stack.");
            }
        }

        /// <summary>
        /// Draws text positioned in screen coordinates. Width of rectangle may be 0 to cause automatic
        /// measurement of the text.
        /// </summary>
        public bool RenderText(ReadOnlySpan<char> text, Rectangle extents, TigTextStyle style, DrawingContext context = null)
        {
            style.colorSlot = 0;

            if (!_fontStack.TryPeek(out var font))
            {
                return false;
            }

            if (extents.Width < 0 || extents.Height < 0)
            {
                Logger.Warn("Negative Text extents! Aborting draw.");
                return false;
            }

            Tig.TextLayouter.LayoutAndDraw(text, font, ref extents, style, context);
            return true;
        }

        /// <summary>
        /// Measures the given text and returns the bounding rect.
        /// </summary>
        public Rectangle MeasureTextSize(string text, TigTextStyle style, int maxWidth = 0, int maxHeight = 0)
        {
            var metrics = new TigFontMetrics {width = maxWidth, height = maxHeight};
            Measure(style, text, ref metrics);
            return new Rectangle(0, 0, metrics.width, metrics.height);
        }

        [TempleDllLocation(0x101ea4e0)]
        public bool Measure(TigTextStyle style, ReadOnlySpan<char> text, ref TigFontMetrics metrics)
        {
            if (!_fontStack.TryPeek(out var font))
                return false;

            // Some places in ToEE will not correctly set the text color before calling into this ...
            var textStyleFixed = style.Copy();
            textStyleFixed.textColor = null;

            Tig.TextLayouter.Measure(font, textStyleFixed, text, ref metrics);
            return true;
        }

        [TempleDllLocation(0x101e8b20)]
        public int MeasureWordWrap(TigTextStyle style, ReadOnlySpan<char> text, Rectangle bounds)
        {
            if (!_fontStack.TryPeek(out var font))
                return text.Length;

            return Tig.TextLayouter.MeasureLineWrap(font, style, text, bounds);
        }

        public void Dispose()
        {
            _fonts.Values.DisposeAndClear();
        }

        public PredefinedFont GetPredefinedFont(string name, int size)
        {
            switch (name.ToLowerInvariant())
            {
                case "arial-10":
                    return PredefinedFont.ARIAL_10;
                case "arial-bold-10":
                    return PredefinedFont.ARIAL_BOLD_10;
                case "priory-12":
                    return PredefinedFont.PRIORY_12;
                default:
                    Logger.Warn("No such predefined font: '{0}'", name);
                    Debugger.Break();
                    return PredefinedFont.ARIAL_10;
            }
        }

    }
}