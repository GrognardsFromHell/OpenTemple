using System;
using System.Diagnostics;
using OpenTemple.Core.AAS;
using OpenTemple.Core.GFX;
using OpenTemple.Core.GFX.RenderMaterials;
using OpenTemple.Core.IO;
using OpenTemple.Core.Logging;

namespace OpenTemple.Core.Systems;

public class AASSystem : IGameSystem
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

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
            ResolveMaterial
        );
    }

    public void Dispose()
    {
    }

    private object ResolveMaterial(string materialPath)
    {
        using var material = _materials.LoadMaterial(materialPath);
        return material;
    }

}