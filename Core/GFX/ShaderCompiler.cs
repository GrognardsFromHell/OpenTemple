using System;
using System.Collections.Generic;
using System.IO;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using OpenTemple.Core.IO;
using OpenTemple.Core.Logging;

namespace OpenTemple.Core.GFX;

internal class VfsIncludeHandler : Include
{
    private readonly IFileSystem _fs;

    public VfsIncludeHandler(IFileSystem fs)
    {
        _fs = fs;
    }

    public IDisposable Shadow { get; set; }

    public void Dispose()
    {
        Shadow.Dispose();
    }

    public Stream Open(IncludeType type, string fileName, Stream parentStream)
    {
        var filename = $"shaders/{fileName}";
        var content = _fs.ReadBinaryFile(filename);
        return new MemoryStream(content);
    }

    public void Close(Stream stream)
    {
        stream.Close();
    }
}

internal class ShaderCompiler
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private readonly IFileSystem _fs;

    public ShaderCompiler(IFileSystem fs)
    {
        _fs = fs;
    }

    public Dictionary<string, string> Defines { get; set; }
    public string SourceCode { get; set; }
    public string Name { get; set; }

    public bool DebugMode { get; set; }

    public ResourceRef<VertexShader> CompileVertexShader(RenderingDevice device)
    {
        var code = CompileShaderCode("vs_4_0");
        return new ResourceRef<VertexShader>(new VertexShader(device, Name, code));
    }

    public ResourceRef<PixelShader> CompilePixelShader(RenderingDevice device)
    {
        var code = CompileShaderCode("ps_4_0");
        return new ResourceRef<PixelShader>(new PixelShader(device, Name, code));
    }

    private byte[] CompileShaderCode(string profile)
    {
        using var includeHandler = new VfsIncludeHandler(_fs);

        // Convert the defines
        List<ShaderMacro> macros = new List<ShaderMacro>(Defines.Count + 1);
        foreach (var (key, value) in Defines)
        {
            macros.Add(new ShaderMacro(key, value));
        }

        // Add a D3D11 define if we compile for newer targets
        if (profile == "vs_4_0" || profile == "ps_4_0")
        {
            macros.Add(new ShaderMacro("D3D11", "1"));
        }

        // Debug flags
        var flags = ShaderFlags.SkipOptimization;
        if (DebugMode)
        {
            flags |= ShaderFlags.Debug | ShaderFlags.SkipOptimization | ShaderFlags.PreferFlowControl;
        }

        using var result = ShaderBytecode.Compile(
            SourceCode,
            "main",
            profile,
            flags,
            EffectFlags.None,
            macros.ToArray(),
            includeHandler,
            Name + ".hlsl"
        );

        if (result.HasErrors)
        {
            // Unable to compile the actual shader
            throw new GfxException($"Unable to compile shader {Name}: {result.Message}");
        }

        if (result.Message != null)
        {
            Logger.Warn("Errors/Warnings compiling shader {0}: {1}", Name, result.Message);
        }

        return result.Bytecode.Data;
    }
}