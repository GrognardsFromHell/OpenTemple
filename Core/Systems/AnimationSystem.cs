using System;
using SpicyTemple.Core.AAS;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.GFX.RenderMaterials;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Logging;

namespace SpicyTemple.Core.Systems
{
    public class AASSystem : IGameSystem, IDisposable
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private readonly AnimatedModelFactory _modelFactory;

        private readonly MdfMaterialFactory _materials;

        public IAnimatedModelFactory ModelFactory => _modelFactory;

        public AasRenderer Renderer { get; }

        public AASSystem(IFileSystem fs, MdfMaterialFactory materials, AasRenderer renderer)
        {
            var meshIndex = fs.ReadMesFile("art/meshes/meshes.mes");

            _materials = materials;

            Renderer = renderer;

            _modelFactory = new AnimatedModelFactory(
                fs,
                meshIndex,
                RunScript,
                ResolveMaterial
            );
        }

        public void Dispose()
        {
            _modelFactory.Dispose();
        }

        private object ResolveMaterial(string materialPath)
        {
            using var material = _materials.LoadMaterial(materialPath);
            if (material.Resource == null)
            {
                Logger.Error("Unable to load material {0}", materialPath);
                return -1;
            }

            return material;
        }

        private static void RunScript(string script)
        {
            Stub.TODO();
            // pythonObjIntegration.RunAnimFrameScript(command);
        }
    }
}