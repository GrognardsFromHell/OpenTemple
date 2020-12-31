using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Platform;
using OpenTemple.Core.IO.TroikaArchives;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Ui
{
    public class VfsAssetLoader : IAssetLoader
    {
        private readonly IAssetLoader _delegate;

        public VfsAssetLoader(IAssetLoader @delegate)
        {
            _delegate = @delegate;
        }

        public void SetDefaultAssembly(Assembly assembly)
        {
            _delegate.SetDefaultAssembly(assembly);
        }

        public bool Exists(Uri uri, Uri baseUri = null)
        {
            return _delegate.Exists(uri, baseUri);
        }

        public Stream Open(Uri uri, Uri baseUri = null)
        {
            if (!uri.IsAbsoluteUri)
            {
                try
                {
                    var path = uri.ToString();
                    return new VfsStream(Tig.FS, path, Tig.FS.ReadFile(path));
                }
                catch (FileNotFoundException)
                {
                }
            }

            return _delegate.Open(uri, baseUri);
        }

        public (Stream stream, Assembly assembly) OpenAndGetAssembly(Uri uri, Uri baseUri = null)
        {
            return _delegate.OpenAndGetAssembly(uri, baseUri);
        }

        public Assembly GetAssembly(Uri uri, Uri baseUri = null)
        {
            return _delegate.GetAssembly(uri, baseUri);
        }

        public IEnumerable<Uri> GetAssets(Uri uri, Uri baseUri)
        {
            return _delegate.GetAssets(uri, baseUri);
        }

        public static void Install()
        {
            var locator = AvaloniaLocator.CurrentMutable;
            var existingLoader = locator.GetService<IAssetLoader>();
            if (existingLoader == null)
            {
                throw new Exception("No existing asset loader!");
            }

            if (existingLoader is VfsAssetLoader)
            {
                throw new Exception("Trying to install the VFS loader twice");
            }

            locator.Bind<IAssetLoader>().ToConstant(new VfsAssetLoader(existingLoader));
        }
    }
}