using System;
using System.Collections.Generic;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using OpenTemple.Core.IO;
using OpenTemple.Core.Logging;
using ShaderDefines = System.Collections.Generic.Dictionary<string, string>;

namespace OpenTemple.Core.GFX;

public abstract class Shader<TSelf, T> : GpuResource<TSelf> where TSelf : GpuResource<TSelf> where T : DeviceChild
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    public string Name { get; }

    public Shader(string name, byte[] compiledShader)
    {
        Name = name;
        CompiledShader = compiledShader;
    }

    void PrintConstantBuffers()
    {
        using var reflector = new ShaderReflection(CompiledShader);

        var shaderDesc = reflector.Description;

        Logger.Info("Vertex Shader '{0}' has {1} constant buffers:", Name, shaderDesc.ConstantBuffers);

        for (var i = 0; i < shaderDesc.ConstantBuffers; i++)
        {
            var cbufferDesc = reflector.GetConstantBuffer(i);
            var bufferDesc = cbufferDesc.Description;

            Logger.Info("  Constant Buffer #{0} '{1}'", i, bufferDesc.Name);

            for (var j = 0; j < bufferDesc.VariableCount; j++)
            {
                var variable = cbufferDesc.GetVariable(j);
                var variableDesc = variable.Description;

                Logger.Info("    {0} @ {1}", variableDesc.Name, variableDesc.StartOffset);
            }
        }
    }

    public abstract void CreateShader();

    public void FreeShader()
    {
        FreeResource();
    }

    protected override void FreeResource()
    {
        DeviceShader?.Dispose();
        DeviceShader = null;
    }

    public abstract void Bind();
    public abstract void Unbind();

    public byte[] CompiledCode => CompiledShader;

    protected T? DeviceShader;
    protected byte[] CompiledShader;
}

public class VertexShader : Shader<VertexShader, SharpDX.Direct3D11.VertexShader>
{
    private readonly RenderingDevice _device;

    public VertexShader(RenderingDevice device, string name, byte[] compiledShader) : base(name, compiledShader)
    {
        _device = device;
    }

    public override void CreateShader()
    {
        FreeResource();

        DeviceShader = new SharpDX.Direct3D11.VertexShader(_device.Device, CompiledShader);
        if (_device.IsDebugDevice())
        {
            DeviceShader.DebugName = Name;
        }
    }

    public override void Bind()
    {
        _device.Context.VertexShader.SetShader(DeviceShader, null, 0);
    }

    public override void Unbind()
    {
        _device.Context.VertexShader.SetShader(null, null, 0);
    }
}

public class PixelShader : Shader<PixelShader, SharpDX.Direct3D11.PixelShader>
{
    private readonly RenderingDevice _device;

    public PixelShader(RenderingDevice device, string name, byte[] compiledShader) : base(name, compiledShader)
    {
        _device = device;
    }

    public override void CreateShader()
    {
        FreeResource();

        DeviceShader = new SharpDX.Direct3D11.PixelShader(_device.Device, CompiledShader);
        if (_device.IsDebugDevice())
        {
            DeviceShader.DebugName = Name;
        }
    }

    public override void Bind()
    {
        _device.Context.PixelShader.SetShader(DeviceShader, null, 0);
    }

    public override void Unbind()
    {
        _device.Context.PixelShader.SetShader(null, null, 0);
    }
}

public class Shaders : IDisposable
{
    private static readonly ShaderDefines EmptyDefines = new();

    private readonly RenderingDevice _device;

    private readonly IFileSystem _fs;

    // For each shader file, we may have multiple compiled
    // variants depending on the defines used
    private readonly Dictionary<string, ShaderCode<VertexShader>> _vertexShaders = new();

    private readonly Dictionary<string, ShaderCode<PixelShader>> _pixelShaders = new();

    private readonly ResourceLifecycleCallbacks _registration;    

    public Shaders(IFileSystem fs, RenderingDevice device)
    {
        _fs = fs;
        _device = device;
        _registration = new ResourceLifecycleCallbacks(device, CreateResources, FreeResources);
    }

    /// <summary>
    /// Compares two dictionaries for equality (ignoring ordering).
    /// </summary>
    private static bool AreDefinesEqual(ShaderDefines a, ShaderDefines b)
    {
        if (a.Count != b.Count)
        {
            return false;
        }

        foreach (var pair in a)
        {
            if (!b.TryGetValue(pair.Key, out var valueB))
            {
                return false;
            }

            if (!pair.Value.Equals(valueB))
            {
                return false;
            }
        }

        return true;
    }

    public ResourceRef<VertexShader> LoadVertexShader(string name, ShaderDefines defines)
    {
        if (!_vertexShaders.TryGetValue(name, out var shaderCode))
        {
            var content = _fs.ReadTextFile($"shaders/{name}.hlsl");
            shaderCode = new ShaderCode<VertexShader>(content);
            _vertexShaders[name] = shaderCode;
        }
        else
        {
            // Search for a variant that matches the defines that are required
            foreach (var variant in shaderCode.CompiledVariants)
            {
                if (AreDefinesEqual(variant.Item1, defines))
                {
                    return variant.Item2.CloneRef();
                }
            }
        }

        // No variant was available for the requested defines, so
        // we compile it now
        var compiler = new ShaderCompiler(_fs);
        compiler.Defines = defines;
        compiler.Name = name;
        compiler.SourceCode = shaderCode.Source;
        compiler.DebugMode = _device.IsDebugDevice();
        var shader = compiler.CompileVertexShader(_device);
        shader.Resource.CreateShader();

        // Insert the newly created shader into the cache
        shaderCode.CompiledVariants.Add(Tuple.Create(defines, shader));

        return shader.CloneRef();
    }

    public ResourceRef<VertexShader> LoadVertexShader(string name)
    {
        return LoadVertexShader(name, EmptyDefines);
    }

    public ResourceRef<PixelShader> LoadPixelShader(string name, ShaderDefines defines)
    {
        if (!_pixelShaders.TryGetValue(name, out var shaderCode))
        {
            var content = _fs.ReadTextFile($"shaders/{name}.hlsl");
            shaderCode = new ShaderCode<PixelShader>(content);
            _pixelShaders[name] = shaderCode;
        }
        else
        {
            // Search for a variant that matches the defines that are required
            foreach (var variant in shaderCode.CompiledVariants)
            {
                if (AreDefinesEqual(variant.Item1, defines))
                {
                    return new ResourceRef<PixelShader>(variant.Item2.Resource);
                }
            }
        }

        // No variant was available for the requested defines, so
        // we compile it now
        var compiler = new ShaderCompiler(_fs);
        compiler.Defines = defines;
        compiler.Name = name;
        compiler.SourceCode = shaderCode.Source;
        compiler.DebugMode = _device.IsDebugDevice();
        var shader = compiler.CompilePixelShader(_device);
        shader.Resource.CreateShader();

        // Insert the newly created shader into the cache
        shaderCode.CompiledVariants.Add(Tuple.Create(defines, shader));

        return shader;
    }

    public ResourceRef<PixelShader> LoadPixelShader(string name)
    {
        return LoadPixelShader(name, EmptyDefines);
    }

    public void Dispose()
    {
        _registration.Dispose();
    }

    private class ShaderCode<T> where T : GpuResource<T>
    {
        // The HLSL shader source code
        public readonly string Source;

        // The compiled variants based on the defines used to compile them
        public readonly List<Tuple<ShaderDefines, ResourceRef<T>>> CompiledVariants;

        public ShaderCode(string source)
        {
            Source = source;
            CompiledVariants = new List<Tuple<ShaderDefines, ResourceRef<T>>>();
        }
    }

    private void CreateResources(RenderingDevice device)
    {
        foreach (var pair in _vertexShaders.Values)
        {
            foreach (var variant in pair.CompiledVariants)
            {
                variant.Item2.Resource.CreateShader();
            }
        }

        foreach (var pair in _pixelShaders.Values)
        {
            foreach (var variant in pair.CompiledVariants)
            {
                variant.Item2.Resource.CreateShader();
            }
        }
    }

    private void FreeResources(RenderingDevice device)
    {
        foreach (var pair in _vertexShaders.Values)
        {
            foreach (var variant in pair.CompiledVariants)
            {
                variant.Item2.Resource.FreeShader();
            }
        }

        foreach (var pair in _pixelShaders.Values)
        {
            foreach (var variant in pair.CompiledVariants)
            {
                variant.Item2.Resource.FreeShader();
            }
        }
    }
}