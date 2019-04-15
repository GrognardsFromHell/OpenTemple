using System;
using System.Collections.Generic;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Logging;

namespace SpicyTemple.Core.GFX
{

    public struct ContentRect {
        public int x;
        public int y;
        public int width;
        public int height;

        public ContentRect(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
    }

    // One of the predefined texture types
    public enum TextureType {
        Invalid,
        Dynamic,
        File,
        RenderTarget
    }

    /*
    Represents a game texture in either an unloaded or
    loaded state.
    */
    public abstract class Texture : GpuResource {

        protected Texture(RenderingDevice device) : base(device)
        {
        }

        public abstract int GetId();

        public abstract string GetName();

        public abstract ContentRect GetContentRect();

        public abstract Size GetSize();

        public virtual bool IsValid() => true;

        // Unloads the device texture (does't prevent it from being loaded again later)
        public abstract void FreeDeviceTexture();

        public abstract ShaderResourceView GetResourceView();

        public abstract TextureType GetType();

        private static readonly ResourceRef<Texture> InvalidTexture = new ResourceRef<Texture>(new InvalidTexture(null));

        internal static ResourceRef<Texture> GetInvalidTexture()
        {
            return InvalidTexture;
        }

    }

    internal class InvalidTexture : Texture
    {
        public InvalidTexture(RenderingDevice device) : base(device)
        {
        }

        protected override void FreeResource()
        {
        }

        public override int GetId() => -1;

        public override string GetName() => "<invalid>";

        public override ContentRect GetContentRect() => new ContentRect(0, 0, 1, 1);

        public override Size GetSize() => new Size(1, 1);

        public override void FreeDeviceTexture()
        {
        }

        public override ShaderResourceView GetResourceView() => null;

        public override TextureType GetType() => TextureType.Invalid;

        public override bool IsValid() => false;

    }

    internal class TextureLoader {

        private static readonly ILogger Logger = new ConsoleLogger();

        public TextureLoader(IFileSystem fs, RenderingDevice device, uint memoryBudget)
        {
            Device = device;
            mMemoryBudget = memoryBudget;
        }

        public ShaderResourceView Load(string filename, out ContentRect contentRectOut, out Size sizeOut)
        {

            var textureData = _fs.ReadBinaryFile(filename);

            try {
                DecodedImage image;

                if (filename.ToLowerInvariant().EndsWith(".img") && textureData.Length == 4) {
                    image = gfx::DecodeCombinedImage(filename, textureData);
                } else {
                    image = gfx::DecodeImage(textureData);
                }

                var texWidth = image.info.width;
                var texHeight = image.info.height;

                contentRectOut.x = 0;
                contentRectOut.y = 0;
                contentRectOut.width = image.info.width;
                contentRectOut.height = image.info.height;

                sizeOut.width = texWidth;
                sizeOut.height = texHeight;

                // Load the D3D11 one
                var textureDesc = new Texture2DDescription();
                textureDesc.Format = Format.B8G8R8A8_UNorm;
                textureDesc.Width = texWidth;
                textureDesc.Height = texHeight;
                textureDesc.ArraySize = 1;
                textureDesc.MipLevels = 1;
                textureDesc.BindFlags = BindFlags.ShaderResource;
                textureDesc.Usage = ResourceUsage.Immutable;

                var initialData = new DataBox();
                memset(&initialData, 0, sizeof(initialData));
                initialData.pSysMem = image.data.get();
                initialData.SysMemPitch = image.info.width * 4;

                var texture = new Texture2D(Device.mD3d11Device, textureDesc, new []{initialData});

                if (Device.IsDebugDevice())
                {
                    texture.DebugName = filename;
                }

                // Make a shader resource view for the texture since that's the only thing we're interested in here
                var resourceViewDesc = new ShaderResourceViewDescription();
                resourceViewDesc.Texture2D.MipLevels = -1;
                resourceViewDesc.Texture2D.MostDetailedMip = 0;
                resourceViewDesc.Dimension = ShaderResourceViewDimension.Texture2D;

                var result =

                mLoaded++;
                mEstimatedUsage += texWidth * texHeight * 4;

                return new ShaderResourceView(Device, texture, resourceViewDesc);

            } catch (Exception e) {
                Logger.Error("Unable to load texture {0}: {1}", filename, e.Message);
                contentRectOut = new ContentRect(0, 0, 0, 0);
                sizeOut = new Size(0, 0);
                return null;
            }

        }

        public void Unload(Size size) {
            mLoaded--;

            mEstimatedUsage -= (uint) (size.width * size.height * 4);
        }

        public int GetLoaded() => mLoaded;

        public uint GetEstimatedUsage() => mEstimatedUsage;

        public uint GetMemoryBudget() => mMemoryBudget;

        public void FreeUnusedTextures()
        {

            // Start with the least recently used texture
            var texture = mLeastRecentlyUsed;
            while (texture != null) {

                if (texture.mUsedThisFrame) {
                    break;
                }

                var aboutToDelete = texture;

                texture = texture.mNextMoreRecentlyUsed;

                if (mEstimatedUsage > mMemoryBudget) {
                    aboutToDelete.FreeDeviceTexture();
                }

            }

            // Reset the rest of the textures to not be used this frame
            while (texture != null) {
                texture.mUsedThisFrame = false;
                texture = texture.mNextMoreRecentlyUsed;
            }
        }
        public FileTexture mLeastRecentlyUsed = null;
        public FileTexture mMostRecentlyUsed = null;

        public RenderingDevice Device { get; }

        private int mLoaded = 0;
        private uint mEstimatedUsage = 0;
        private uint mMemoryBudget;
        private readonly IFileSystem _fs;
    }

    internal class FileTexture : Texture
    {
        public FileTexture(TextureLoader loader, int id, string filename) : base(loader.Device)
        {
            mLoader = loader;
        }

        protected override void FreeResource() => FreeDeviceTexture();

        public override int GetId() => mId;

        public override string GetName() => mFilename;

        public override ContentRect GetContentRect()
        {
            if (!mMetadataValid)
            {
                Load();
            }
            return mContentRect;
        }

        public override Size GetSize()
        {
            if (!mMetadataValid)
            {
                Load();
            }
            return mSize;
        }

        public override void FreeDeviceTexture()
        {
            if (mResourceView != null) {
                mResourceView.Dispose();
                mResourceView = null;
                mLoader.Unload(mSize);
                DisconnectMru();
            }
        }

        public override ShaderResourceView GetResourceView()
        {
            if (mResourceView == null) {
                Load();
            }
            MarkUsed();
            return mResourceView;
        }

        public override TextureType GetType() => TextureType.File;

        private TextureLoader mLoader;
        private int mId;
        private string mFilename;

        private void MarkUsed()
        {
            mUsedThisFrame = true;

            if (mLoader.mMostRecentlyUsed == this)
                return; // Already MRU

            // Disconnect from current position of MRU list
            DisconnectMru();

            // Insert to front of MRU list
            MakeMru();
        }

        private void Load() {
            Trace.Assert(mResourceView == null);
            mResourceView = mLoader.Load(mFilename, out mContentRect, out mSize);

            if (mResourceView != null) {
                mMetadataValid = true;

                // The texture should not be in the MRU cache at this point
                Trace.Assert(this.mNextMoreRecentlyUsed == null);
                Trace.Assert(this.mNextLessRecentlyUsed == null);
                MakeMru();
            }
        }

        private void MakeMru()
        {
            if (mLoader.mMostRecentlyUsed != null) {
                Trace.Assert(mLoader.mMostRecentlyUsed.mNextMoreRecentlyUsed== null);
                mLoader.mMostRecentlyUsed.mNextMoreRecentlyUsed = this;
            }

            mNextLessRecentlyUsed = mLoader.mMostRecentlyUsed;

            mLoader.mMostRecentlyUsed = this;

            if (mLoader.mLeastRecentlyUsed == null) {
                mLoader.mLeastRecentlyUsed = this;
            }
        }

        private void DisconnectMru()
        {
            if (mNextLessRecentlyUsed != null) {
                Trace.Assert(mNextLessRecentlyUsed.mNextMoreRecentlyUsed == this);
                mNextLessRecentlyUsed.mNextMoreRecentlyUsed = mNextMoreRecentlyUsed;
            }
            if (mNextMoreRecentlyUsed != null) {
                Trace.Assert(mNextMoreRecentlyUsed.mNextLessRecentlyUsed == this);
                mNextMoreRecentlyUsed.mNextLessRecentlyUsed = mNextLessRecentlyUsed;
            }
            if (mLoader.mLeastRecentlyUsed == this) {
                mLoader.mLeastRecentlyUsed = mNextMoreRecentlyUsed;
                Trace.Assert(mNextMoreRecentlyUsed == null
                        || mLoader.mLeastRecentlyUsed.mNextLessRecentlyUsed == null);
            }
            if (mLoader.mMostRecentlyUsed == this) {
                mLoader.mMostRecentlyUsed = mNextLessRecentlyUsed;
                Trace.Assert(mNextLessRecentlyUsed == null
                        || mLoader.mMostRecentlyUsed.mNextMoreRecentlyUsed == null);
            }
            mNextLessRecentlyUsed = null;
            mNextMoreRecentlyUsed = null;
        }

        internal bool mUsedThisFrame = false;
        private bool mMetadataValid = false;
        private ContentRect mContentRect;
        private Size mSize;
        private ShaderResourceView mResourceView;

        internal FileTexture mNextMoreRecentlyUsed = null;
        private FileTexture mNextLessRecentlyUsed = null;

    }

    public class Textures
    {

        private readonly IFileSystem _fs;

        private static readonly ILogger Logger = new ConsoleLogger();

        public Textures(IFileSystem fs, RenderingDevice device, uint memoryBudget)
        {
            _fs = fs;
            mLoader = new TextureLoader(fs, device, memoryBudget);
        }

        /*
            Call this after every frame to free texture memory by
            freeing the least recently used textures.
        */
        public void FreeUnusedTextures()
        {
            mLoader.FreeUnusedTextures();
        }

        // Frees all GPU texture memory i.e. after a device reset
        public void FreeAllTextures()
        {
            foreach (var entry in mTexturesByName.Values) {
                entry.Resource.FreeDeviceTexture();
            }
        }

        public ResourceRef<Texture> Resolve(string filename, bool withMipMaps)
        {
            var filenameLower = filename.ToLowerInvariant();

            if (mTexturesByName.TryGetValue(filenameLower, out var textureRef))
            {
                return textureRef.CloneRef();
            }

            // Texture is not registered yet, so let's do that
            if (!_fs.FileExists(filename)) {
                Logger.Error("Cannot register texture '{0}', because it does not exist.", filename);
                var result = Texture.GetInvalidTexture();
                mTexturesByName[filenameLower] = result;
                return result.CloneRef();
            }

            var id = mNextFreeId++;

            var texture = new ResourceRef<Texture>(new FileTexture(mLoader, id, filename));

            Trace.Assert(!mTexturesByName.ContainsKey(filenameLower));
            Trace.Assert(!mTexturesById.ContainsKey(id));
            mTexturesByName[filenameLower] = texture;
            mTexturesById[id] = texture;

            return texture;
        }

        public ResourceRef<Texture> Override(string filename, bool withMipMaps)
        {

            var filenameLower = filename.ToLowerInvariant();

            var id = -1;
            if (mTexturesByName.TryGetValue(filenameLower, out var textureRef))
            {
                id = textureRef.Resource.GetId();
            }

            // Texture is not registered yet, so let's do that
            if (!_fs.FileExists(filename)) {
                Logger.Error("Cannot register texture '{0}', because it does not exist.", filename);
                var result = Texture.GetInvalidTexture();
                mTexturesByName[filenameLower] = result.CloneRef();
                return result;
            }

            if (id == -1)
                id = mNextFreeId++;

            var texture = new ResourceRef<Texture>(new FileTexture(mLoader, id, filename));

            mTexturesByName[filenameLower] = texture.CloneRef();
            mTexturesById[id] = texture.CloneRef();

            return texture;
        }

        public ResourceRef<Texture> ResolveUncached(string filename, bool withMipMaps)
        {

            // Texture is not registered yet, so let's do that
            if (!_fs.FileExists(filename)) {
                Logger.Error("Cannot laod texture '{0}', because it does not exist.", filename);
                return Texture.GetInvalidTexture();
            }

            return new ResourceRef<Texture>(new FileTexture(mLoader, -1, filename));
        }

        public ResourceRef<Texture> GetById(int textureId)
        {
            if (textureId == -1) {
                return Texture.GetInvalidTexture();
            }

            if (mTexturesById.TryGetValue(textureId, out var textureRef))
            {
                return textureRef.CloneRef();
            }

            Logger.Info("Trying to retrieve unknown texture id {0}", textureId);
            return Texture.GetInvalidTexture();
        }

        public int GetLoaded() => mLoader.GetLoaded();
        public int GetRegistered() => mTexturesById.Count;
        public uint GetUsageEstimate() => mLoader.GetEstimatedUsage();
        public uint GetMemoryBudget() => mLoader.GetMemoryBudget();

        private TextureLoader mLoader;
        private int mNextFreeId = 1;
        private Dictionary<int, ResourceRef<Texture>> mTexturesById;
        private Dictionary<string, ResourceRef<Texture>> mTexturesByName;

    }

}
