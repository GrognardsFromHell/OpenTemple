using System;
using System.Drawing;
using System.Runtime.InteropServices;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Systems.FogOfWar;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.TownMap;

[TemplePlusLocation("ui_townmap.cpp")]
public class TownMapTileRenderer : WidgetContent
{

    private TileRenderer? _smallTileRenderer;

    private TileRenderer? _bigTileRenderer;

    private readonly FogTileRenderer _fogTileRenderer = new();

    public Rectangle SourceRect { get; set; }

    public void LoadData(TownMapData mapData)
    {
        _smallTileRenderer = new MapTileRenderer(2048, mapData.SmallTextures);
        _bigTileRenderer = new MapTileRenderer(1024, mapData.BigTextures);
    }

    public void UpdateFogOfWar(TownmapFogTile[,] tiles)
    {
        _fogTileRenderer.FogTiles = tiles;
    }

    [TempleDllLocation(0x1002c750)]
    public override void Render(PointF origin)
    {
        if (_smallTileRenderer == null || _bigTileRenderer == null)
        {
            return; // No data loaded
        }

        var destRect = ContentArea;
        destRect.Offset(origin);

        // This seems to be the origin of the townmap, basically
        var srcRect = SourceRect;
        srcRect.X += 8448;
        srcRect.Y -= 6000;
        var widthScale = destRect.Width / (float) srcRect.Width;
        var heightScale = destRect.Height / (float) srcRect.Height;

        // Render using the small / big grid size depending on zoom level
        if (widthScale <= 1 / 6.0f)
        {
            _smallTileRenderer.RenderTownmapTiles(srcRect, destRect, widthScale, heightScale);
        }
        else
        {
            _bigTileRenderer.RenderTownmapTiles(srcRect, destRect, widthScale, heightScale);
        }

        if (_fogTileRenderer.FogTiles != null)
        {
            _fogTileRenderer.RenderTownmapTiles(srcRect, destRect, widthScale, heightScale);
        }
    }

    private abstract class TileRenderer
    {
        private readonly int _cols;

        private readonly int _rows;

        private readonly int _tileDimension;

        // Real size of a townmap tile is 256x256, and it is stretched up to full size (1024x1024)
        private readonly float _tileScale;

        protected TileRenderer(int cols, int rows, int tileDimension)
        {
            _cols = cols;
            _rows = rows;
            _tileDimension = tileDimension;
            _tileScale = 256.0f / tileDimension;
        }

        public void RenderTownmapTiles(RectangleF srcRect, RectangleF destRect, float widthScale, float heightScale)
        {
            var rightmostTile = (srcRect.X + srcRect.Width) / _tileDimension;
            if (rightmostTile >= _cols)
            {
                rightmostTile = _cols;
            }

            var bottommostTile = (srcRect.Y + srcRect.Height) / _tileDimension;
            if (bottommostTile >= _rows)
            {
                bottommostTile = _rows;
            }

            var startTileX = (int) (srcRect.X / _tileDimension);
            if (srcRect.X / _tileDimension < 0)
            {
                startTileX = 0;
            }

            var startTileY = (int) (srcRect.Y / _tileDimension);
            if (srcRect.Y / _tileDimension < 0)
            {
                startTileY = 0;
            }

            for (var tileY = startTileY; tileY <= bottommostTile; ++tileY)
            {
                var curSrcY = tileY * _tileDimension;

                for (var tileX = startTileX; tileX <= rightmostTile; ++tileX)
                {
                    var curSrcX = tileX * _tileDimension;
                    var clippedSrcX = tileX * (float) _tileDimension;
                    var clippedSrcWidth = clippedSrcX + _tileDimension;
                    if (clippedSrcX < srcRect.X)
                    {
                        clippedSrcX = srcRect.X;
                    }

                    var srcRectRight = (float) (srcRect.X + srcRect.Width);
                    if (clippedSrcWidth > srcRectRight)
                    {
                        clippedSrcWidth = srcRectRight;
                    }

                    clippedSrcWidth = clippedSrcWidth - clippedSrcX;

                    var clippedSrcY = tileY * (float) _tileDimension;
                    var clippedSrcHeight = clippedSrcY + _tileDimension;
                    if (clippedSrcY < srcRect.Y)
                    {
                        clippedSrcY = srcRect.Y;
                    }

                    var srcRectBottom = (float) (srcRect.Y + srcRect.Height);
                    if (clippedSrcHeight > srcRectBottom)
                        clippedSrcHeight = srcRectBottom;
                    clippedSrcHeight -= clippedSrcY;

                    RectangleF tileDestRect = default;
                    tileDestRect.X = destRect.X + (int) ((clippedSrcX - srcRect.X) * widthScale);
                    tileDestRect.Y = destRect.Y + (int) ((clippedSrcY - srcRect.Y) * heightScale);
                    tileDestRect.Width = destRect.X +
                                         (int) ((clippedSrcX + clippedSrcWidth - srcRect.X) * widthScale) -
                                         tileDestRect.X;
                    tileDestRect.Height = destRect.Y +
                                          (int) ((clippedSrcY + clippedSrcHeight - srcRect.Y) * heightScale) -
                                          tileDestRect.Y;

                    clippedSrcX = (clippedSrcX - curSrcX) * _tileScale;
                    clippedSrcY = (clippedSrcY - curSrcY) * _tileScale;
                    clippedSrcWidth *= _tileScale;
                    clippedSrcHeight *= _tileScale;

                    if (clippedSrcWidth > 0 && clippedSrcHeight > 0 && tileDestRect.Width != 0 &&
                        tileDestRect.Height != 0)
                    {
                        var texture = GetTexture(tileX, tileY);
                        if (texture == null)
                        {
                            continue;
                        }

                        Render2dArgs arg = default;
                        arg.customTexture = texture;
                        arg.flags = Render2dFlag.BUFFERTEXTURE;
                        arg.srcRect = new RectangleF(
                            clippedSrcX,
                            clippedSrcY,
                            clippedSrcWidth,
                            clippedSrcHeight
                        );
                        arg.destRect = tileDestRect;
                        Tig.ShapeRenderer2d.DrawRectangle(ref arg);
                    }
                }
            }
        }

        protected abstract ITexture? GetTexture(int tileX, int tileY);
    }

    private sealed class MapTileRenderer : TileRenderer
    {
        private readonly ResourceRef<ITexture>[,] _textures;

        public MapTileRenderer(int tileDimension, ResourceRef<ITexture>[,] textures)
            : base(textures.GetLength(1), textures.GetLength(0), tileDimension)
        {
            _textures = textures;
        }

        protected override ITexture GetTexture(int tileX, int tileY)
        {
            return _textures[tileX, tileY].Resource;
        }
    }

    private sealed class FogTileRenderer : TileRenderer, IDisposable
    {
        public TownmapFogTile[,]? FogTiles { get; set; }

        private ResourceRef<DynamicTexture> _fogTexture;

        public FogTileRenderer() : base(2, 2, 10240)
        {
            // Fill with dummy data
            _fogTexture = Tig.RenderingDevice.CreateDynamicTexture(BufferFormat.A8R8G8B8, 256, 256);
        }

        protected override ITexture? GetTexture(int tileX, int tileY)
        {
            if (FogTiles == null)
            {
                return null;
            }

            Span<uint> sPixelData = stackalloc uint[256 * 256];

            // TODO: Direct and dirty hack ported straight from TemplePlus C++. This can be made safe.
            unsafe
            {
                fixed (uint* pixelPtrStart = sPixelData)
                {
                    var pixelPtr = pixelPtrStart;
                    fixed (byte* fogPtrStart = FogTiles[tileX, tileY].Data)
                    {
                        var fogPtr = fogPtrStart;
                        for (var y = 0; y < 256; ++y)
                        {
                            for (var chunk = 0; chunk < 32; chunk++)
                            {
                                var fogByte = *fogPtr;
                                for (var bitmask = 1; bitmask < 0x100; bitmask <<= 1)
                                {
                                    *pixelPtr = (fogByte & bitmask) == 0 ? 0xFF000000 : 0;
                                    pixelPtr++;
                                }

                                ++fogPtr;
                            }
                        }
                    }
                }
            }

            var pixelDataBytes = MemoryMarshal.Cast<uint, byte>(sPixelData);
            _fogTexture.Resource.UpdateRaw(pixelDataBytes, 256 * sizeof(uint));

            return _fogTexture.Resource;
        }

        public void Dispose()
        {
            _fogTexture.Dispose();
        }
    }

    public void Dispose()
    {
        _fogTileRenderer.Dispose();
    }

}