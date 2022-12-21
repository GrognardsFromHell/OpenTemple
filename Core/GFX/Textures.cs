using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.Images;
using OpenTemple.Core.Logging;
using Rectangle = System.Drawing.Rectangle;

namespace OpenTemple.Core.GFX;

// One of the predefined texture types
public enum TextureType
{
    Invalid,
    Dynamic,
    File,
    RenderTarget
}

/*
Represents a game texture in either an unloaded or
loaded state.
*/
public interface ITexture : IRefCounted
{
    int GetId();

    string GetName();

    Rectangle GetContentRect();

    Size GetSize();

    bool IsValid();

    // Unloads the device texture (does't prevent it from being loaded again later)
    void FreeDeviceTexture();

    ShaderResourceView? GetResourceView();

    TextureType Type { get; }
}

internal class InvalidTexture : ITexture
{
    public int GetId() => -1;

    public string GetName() => "<invalid>";

    public Rectangle GetContentRect() => new(0, 0, 1, 1);

    public Size GetSize() => new(1, 1);

    public void FreeDeviceTexture()
    {
    }

    public ShaderResourceView? GetResourceView() => null;

    public TextureType Type => TextureType.Invalid;

    public bool IsValid() => false;

    public void Reference()
    {
    }

    public void Dereference()
    {
    }
}

internal class TextureLoader
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    public FileTexture? LeastRecentlyUsed;
    public FileTexture? MostRecentlyUsed;

    public RenderingDevice Device { get; }

    private int _loaded;
    private uint _estimatedUsage;
    private uint _memoryBudget;
    private readonly IFileSystem _fs;
    
    public TextureLoader(IFileSystem fs, RenderingDevice device, uint memoryBudget)
    {
        Device = device;
        _memoryBudget = memoryBudget;
        _fs = fs;
    }

    public ShaderResourceView? Load(string filename, out Rectangle contentRectOut, out Size sizeOut)
    {
        Debug.Assert(filename != null);

        var textureData = _fs.ReadBinaryFile(filename);

        try
        {
            DecodedImage image;

            if (filename.ToLowerInvariant().EndsWith(".img") && textureData.Length == 4)
            {
                image = ImageIO.DecodeCombinedImage(_fs, filename, textureData);
            }
            else
            {
                image = ImageIO.DecodeImage(textureData);
            }

            var texWidth = image.info.width;
            var texHeight = image.info.height;

            contentRectOut = new Rectangle(0, 0, image.info.width, image.info.height);

            sizeOut = new Size(texWidth, texHeight);

            // Load the D3D11 one
            var textureDesc = new Texture2DDescription();
            textureDesc.Format = Format.B8G8R8A8_UNorm;
            textureDesc.Width = texWidth;
            textureDesc.Height = texHeight;
            textureDesc.ArraySize = 1;
            textureDesc.MipLevels = 1;
            textureDesc.BindFlags = BindFlags.ShaderResource;
            textureDesc.Usage = ResourceUsage.Immutable;
            textureDesc.SampleDescription.Count = 1;

            Texture2D texture;
            var initialData = new DataBox();
            unsafe
            {
                fixed (byte* imageDataPtr = image.data)
                {
                    initialData.DataPointer = (IntPtr) imageDataPtr;
                    initialData.RowPitch = image.info.width * 4;

                    texture = new Texture2D(Device.Device, textureDesc, new[] {initialData});
                }
            }

            if (Device.IsDebugDevice())
            {
                texture.DebugName = filename;
            }

            // Make a shader resource view for the texture since that's the only thing we're interested in here
            var resourceViewDesc = new ShaderResourceViewDescription();
            resourceViewDesc.Texture2D.MipLevels = 1;
            resourceViewDesc.Dimension = ShaderResourceViewDimension.Texture2D;

            _loaded++;
            _estimatedUsage += (uint) (texWidth * texHeight * 4);

            return new ShaderResourceView(Device.Device, texture, resourceViewDesc);
        }
        catch (Exception e)
        {
            Logger.Error("Unable to load texture {0}: {1}", filename, e.Message);
            contentRectOut = new Rectangle(0, 0, 0, 0);
            sizeOut = new Size(0, 0);
            return null;
        }
    }

    public void Unload(Size size)
    {
        _loaded--;

        _estimatedUsage -= (uint) (size.Width * size.Height * 4);
    }

    public int GetLoaded() => _loaded;

    public uint GetEstimatedUsage() => _estimatedUsage;

    public uint GetMemoryBudget() => _memoryBudget;

    public void FreeUnusedTextures()
    {
        // Start with the least recently used texture
        var texture = LeastRecentlyUsed;
        while (texture != null)
        {
            if (texture.UsedThisFrame)
            {
                break;
            }

            var aboutToDelete = texture;

            texture = texture._nextMoreRecentlyUsed;

            if (_estimatedUsage > _memoryBudget)
            {
                aboutToDelete.FreeDeviceTexture();
            }
        }

        // Reset the rest of the textures to not be used this frame
        while (texture != null)
        {
            texture.UsedThisFrame = false;
            texture = texture._nextMoreRecentlyUsed;
        }
    }
}

internal class FileTexture : GpuResource<FileTexture>, ITexture
{
    private readonly TextureLoader _loader;
    private readonly int _id;
    private readonly string _filename;
    
    internal bool UsedThisFrame;
    private bool _metadataValid;
    private bool _loadFailed;
    private Rectangle _contentRect;
    private Size _size;
    private ShaderResourceView? _resourceView;

    internal FileTexture? _nextMoreRecentlyUsed;
    private FileTexture? _nextLessRecentlyUsed;
    
    public FileTexture(TextureLoader loader, int id, string filename)
    {
        _loader = loader;
        _id = id;
        _filename = filename;
    }

    protected override void FreeResource() => FreeDeviceTexture();

    public int GetId() => _id;

    public string GetName() => _filename;

    public Rectangle GetContentRect()
    {
        if (!_metadataValid)
        {
            Load();
        }

        return _contentRect;
    }

    public Size GetSize()
    {
        if (!_metadataValid)
        {
            Load();
        }

        return _size;
    }

    public void FreeDeviceTexture()
    {
        if (_resourceView != null)
        {
            _resourceView.Dispose();
            _resourceView = null;
            _loader.Unload(_size);
            DisconnectMru();
        }
    }

    public ShaderResourceView? GetResourceView()
    {
        if (_resourceView == null)
        {
            Load();
        }

        MarkUsed();
        return _resourceView;
    }

    public TextureType Type => TextureType.File;

    public bool IsValid() => true;

    private void MarkUsed()
    {
        UsedThisFrame = true;

        if (_loader.MostRecentlyUsed == this)
            return; // Already MRU

        // Disconnect from current position of MRU list
        DisconnectMru();

        // Insert to front of MRU list
        MakeMru();
    }

    private void Load()
    {
        if (_loadFailed)
        {
            return;
        }

        Trace.Assert(_resourceView == null);
        _resourceView = _loader.Load(_filename, out _contentRect, out _size);

        if (_resourceView != null)
        {
            _metadataValid = true;

            // The texture should not be in the MRU cache at this point
            Trace.Assert(_nextMoreRecentlyUsed == null);
            Trace.Assert(_nextLessRecentlyUsed == null);
            MakeMru();
        }
        else
        {
            _loadFailed = true;
        }
    }

    private void MakeMru()
    {
        if (_loader.MostRecentlyUsed != null)
        {
            Trace.Assert(_loader.MostRecentlyUsed._nextMoreRecentlyUsed == null);
            _loader.MostRecentlyUsed._nextMoreRecentlyUsed = this;
        }

        _nextLessRecentlyUsed = _loader.MostRecentlyUsed;

        _loader.MostRecentlyUsed = this;

        if (_loader.LeastRecentlyUsed == null)
        {
            _loader.LeastRecentlyUsed = this;
        }
    }

    private void DisconnectMru()
    {
        if (_nextLessRecentlyUsed != null)
        {
            Trace.Assert(_nextLessRecentlyUsed._nextMoreRecentlyUsed == this);
            _nextLessRecentlyUsed._nextMoreRecentlyUsed = _nextMoreRecentlyUsed;
        }

        if (_nextMoreRecentlyUsed != null)
        {
            Trace.Assert(_nextMoreRecentlyUsed._nextLessRecentlyUsed == this);
            _nextMoreRecentlyUsed._nextLessRecentlyUsed = _nextLessRecentlyUsed;
        }

        if (_loader.LeastRecentlyUsed == this)
        {
            _loader.LeastRecentlyUsed = _nextMoreRecentlyUsed;
            Trace.Assert(_nextMoreRecentlyUsed == null
                         || _loader.LeastRecentlyUsed?._nextLessRecentlyUsed == null);
        }

        if (_loader.MostRecentlyUsed == this)
        {
            _loader.MostRecentlyUsed = _nextLessRecentlyUsed;
            Trace.Assert(_nextLessRecentlyUsed == null
                         || _loader.MostRecentlyUsed?._nextMoreRecentlyUsed == null);
        }

        _nextLessRecentlyUsed = null;
        _nextMoreRecentlyUsed = null;
    }
}

public class Textures
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();
    
    private readonly IFileSystem _fs;
    private readonly TextureLoader _loader;
    private readonly Dictionary<int, ResourceRef<ITexture>> _texturesById = new();
    private readonly Dictionary<string, ResourceRef<ITexture>> _texturesByName = new();
    private int _nextFreeId = 1;
    
    public Textures(IFileSystem fs, RenderingDevice device, uint memoryBudget)
    {
        _fs = fs;
        _loader = new TextureLoader(fs, device, memoryBudget);
    }

    /*
        Call this after every frame to free texture memory by
        freeing the least recently used textures.
    */
    public void FreeUnusedTextures()
    {
        _loader.FreeUnusedTextures();
    }

    // Frees all GPU texture memory i.e. after a device reset
    public void FreeAllTextures()
    {
        foreach (var entry in _texturesByName.Values)
        {
            entry.Resource.FreeDeviceTexture();
        }
    }

    public ResourceRef<ITexture> Resolve(string filename, bool withMipMaps)
    {
        var filenameLower = filename.ToLowerInvariant();

        if (_texturesByName.TryGetValue(filenameLower, out var textureRef))
        {
            return textureRef.Resource.Ref();
        }

        // Texture is not registered yet, so let's do that
        if (!_fs.FileExists(filename))
        {
            Logger.Error("Cannot register texture '{0}', because it does not exist.", filename);
            var result = InvalidTexture.Ref();
            _texturesByName[filenameLower] = result;
            return result.CloneRef();
        }

        var id = _nextFreeId++;

        var texture = new ResourceRef<ITexture>(new FileTexture(_loader, id, filename));

        Trace.Assert(!_texturesByName.ContainsKey(filenameLower));
        Trace.Assert(!_texturesById.ContainsKey(id));
        _texturesByName[filenameLower] = texture.CloneRef();
        _texturesById[id] = texture.CloneRef();

        return texture;
    }

    public static ITexture InvalidTexture { get; } = new InvalidTexture();

    public ITexture ResolveUncached(string filename, bool withMipMaps)
    {
        // Texture is not registered yet, so let's do that
        if (!_fs.FileExists(filename))
        {
            Logger.Error("Cannot load texture '{0}', because it does not exist.", filename);
            return InvalidTexture;
        }

        return new FileTexture(_loader, -1, filename);
    }

    public ITexture GetById(int textureId)
    {
        if (textureId == -1)
        {
            return InvalidTexture;
        }

        if (_texturesById.TryGetValue(textureId, out var textureRef))
        {
            return textureRef.Resource;
        }

        Logger.Info("Trying to retrieve unknown texture id {0}", textureId);
        return InvalidTexture;
    }

    public int GetLoaded() => _loader.GetLoaded();
    public int GetRegistered() => _texturesById.Count;
    public uint GetUsageEstimate() => _loader.GetEstimatedUsage();
    public uint GetMemoryBudget() => _loader.GetMemoryBudget();
}
