using System;
using System.Collections.Generic;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;

namespace OpenTemple.Core.AAS;

public class AnimatedModelFactory : IAnimatedModelFactory
{
    private readonly IFileSystem _fileSystem;
    private readonly IDictionary<int, string> _meshTable;
    private readonly MaterialResolver _materialResolver;
    private readonly Func<string, Mesh> _meshLoader;

    public AnimatedModelFactory(
        IFileSystem fileSystem,
        IDictionary<int, string> meshTable,
        Func<string, object> resolveMaterial)
    {
        _fileSystem = fileSystem;
        _meshTable = meshTable;
        _meshLoader = LoadMeshFile;

        _materialResolver = new MaterialResolver(resolveMaterial);
    }

    private string GetMeshFilename(int meshId)
    {
        if (!_meshTable.TryGetValue(meshId, out var meshFilename))
        {
            throw new AasException($"Could not resolve the filename for mesh id {meshId}");
        }
        return $"art/meshes/{meshFilename}.skm";
    }

    private string GetSkeletonFilename(int meshId)
    {
        if (!_meshTable.TryGetValue(meshId, out var meshFilename))
        {
            throw new AasException($"Could not resolve the filename for mesh id {meshId}");
        }
        return $"art/meshes/{meshFilename}.ska";
    }

    public IAnimatedModel FromIds(int meshId, int skeletonId, EncodedAnimId idleAnimId,
        in AnimatedModelParams animParams)
    {
        var skeletonFilename = GetSkeletonFilename(skeletonId);
        var meshFilename = GetMeshFilename(meshId);
        
        return FromFilenames(meshFilename, skeletonFilename, idleAnimId, in animParams);
    }

    public IAnimatedModel FromFilenames(string meshFilename, string skeletonFilename,
        EncodedAnimId idleAnimId,
        in AnimatedModelParams animParams)
    {
        var mesh = LoadMeshFile(meshFilename);
        var skeleton = LoadSkeletonFile(skeletonFilename);

        return new AnimatedModelAdapter(
            _materialResolver,
            _meshLoader,
            mesh,
            skeleton,
            idleAnimId,
            in animParams
        );
    }
    
    private Skeleton LoadSkeletonFile(string skeletonFilename)
    {
        var buffer = _fileSystem.ReadBinaryFile(skeletonFilename);

        return new Skeleton(buffer)
        {
            Path = skeletonFilename
        };
    }

    private Mesh LoadMeshFile(string meshFilename)
    {
        var buffer = _fileSystem.ReadBinaryFile(meshFilename);

        return new Mesh(buffer)
        {
            Path = meshFilename
        };
    }
}